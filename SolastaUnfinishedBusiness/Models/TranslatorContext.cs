﻿using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using I2.Loc;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SolastaUnfinishedBusiness.Api.Infrastructure;
using UnityEngine;
using UnityModManagerNet;
using Resources = SolastaUnfinishedBusiness.Properties.Resources;

namespace SolastaUnfinishedBusiness.Models;

internal struct LanguageEntry
{
    public string Code, Text, Directory;
}

internal static class TranslatorContext
{
    private const string UnofficialLanguagesFolderPrefix = "Translations-";

    internal const string English = "en";

    private static readonly Dictionary<string, string> TranslationsCache = new();

    private static readonly Dictionary<string, string> Glossary = GetWordsDictionary();

    internal static readonly string[] AvailableLanguages = { "de", "en", "es", "fr", "pt", "ru", "zh-CN" };

    internal static readonly List<LanguageEntry> Languages = new();

    internal static void LoadCustomLanguages()
    {
        var cultureInfos = CultureInfo.GetCultures(CultureTypes.AllCultures);
        var directoryInfo = new DirectoryInfo($@"{UnityModManager.modsPath}/{typeof(Main).Namespace}");
        var directories = directoryInfo.GetDirectories($"{UnofficialLanguagesFolderPrefix}??");

        foreach (var directory in directories)
        {
            var code = directory.Name.Substring(UnofficialLanguagesFolderPrefix.Length,
                directory.Name.Length - UnofficialLanguagesFolderPrefix.Length);
            var cultureInfo = cultureInfos.First(o => o.Name == code);

            if (LocalizationManager.HasLanguage(cultureInfo.DisplayName))
            {
                Main.Error($"Language {code} from {directory.Name} already in game");
            }
            else
            {
                Languages.Add(new LanguageEntry
                {
                    Code = code,
                    Text = cultureInfo.TextInfo.ToTitleCase(cultureInfo.NativeName),
                    Directory = directory.Name
                });

                Main.Info($"Language {code} detected.");
            }
        }
    }

    internal static void LoadCustomTerms()
    {
        var languageSourceData = LocalizationManager.Sources[0];

        // load new language terms
        foreach (var language in Languages)
        {
            // add language
            languageSourceData.AddLanguage(language.Text, language.Code);

            var languageIndex = languageSourceData.GetLanguageIndex(language.Text);

            // add terms
            var directoryInfo =
                new DirectoryInfo($@"{UnityModManager.modsPath}/{typeof(Main).Namespace}/{language.Directory}");
            var files = directoryInfo.GetFiles("*.txt");

            foreach (var file in files)
            {
                var filename = $@"{Main.ModFolder}/{language.Directory}/{file.Name}";
                using var sr = new StreamReader(filename);

                while (sr.ReadLine() is { } line)
                {
                    try
                    {
                        var split = line.Split(new[] { '=' }, 2);
                        var term = split[0];
                        var text = split[1];

                        languageSourceData.AddTerm(term).Languages[languageIndex] = text;
                    }
                    catch
                    {
                        Main.Error($"Skipping line [{line}] in file [{filename}]");
                    }
                }
            }
        }
    }

    [NotNull]
    private static string GetPayload([NotNull] string url)
    {
        using var wc = new WebClient();

        wc.Headers.Add("user-agent",
            "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2228.0 Safari/537.36");
        wc.Encoding = Encoding.UTF8;

        return wc.DownloadString(url);
    }

    [NotNull]
    private static string GetMd5Hash([NotNull] string input)
    {
        var builder = new StringBuilder();
        var md5Hash = MD5.Create();
        var payload = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

        foreach (var item in payload)
        {
            builder.Append(item.ToString("x2"));
        }

        return builder.ToString();
    }

    private static string TranslateGoogle(string sourceText, string targetCode)
    {
        const string BASE = "https://translate.googleapis.com/translate_a/single";

        try
        {
            var encoded = HttpUtility.UrlEncode(sourceText);
            var url = $"{BASE}?client=gtx&sl=auto&tl={targetCode}&dt=t&q={encoded}";
            var payload = GetPayload(url);
            var json = JsonConvert.DeserializeObject(payload);
            var result = string.Empty;

            if (json is not JArray outerArray)
            {
                return sourceText;
            }

            return outerArray.First() is not JArray terms
                ? sourceText
                : terms.Aggregate(result, (current, term) => current + term.First());
        }
        catch
        {
            Main.Error("Failed translating: " + sourceText);

            return sourceText;
        }
    }

    private static string Translate([NotNull] string sourceText, string targetCode)
    {
        if (sourceText == string.Empty)
        {
            return string.Empty;
        }

        var md5 = GetMd5Hash(sourceText);

        if (TranslationsCache.TryGetValue(md5, out var cachedTranslation))
        {
            return cachedTranslation;
        }

        var translation = TranslateGoogle(sourceText.Replace("_", " "), targetCode);

        TranslationsCache.Add(md5, translation);

        return translation;
    }

    [NotNull]
    private static Dictionary<string, string> GetWordsDictionary()
    {
        var words = new Dictionary<string, string>();
        var path = Path.Combine(Main.ModFolder, "dictionary.txt");

        if (!File.Exists(path))
        {
            return words;
        }

        foreach (var line in File.ReadLines(path))
        {
            try
            {
                var columns = line.Split(new[] { '=' }, 2);

                words.Add(columns[0], columns[1]);
            }
            catch
            {
                Main.Error($"invalid dictionary line \"{line}\".");
            }
        }

        return words;
    }

    [ItemCanBeNull]
#if DEBUG
    internal
#else
    private
#endif
        static IEnumerable<string> GetTranslations(string languageCode)
    {
        using var zipStream = new MemoryStream(Resources.Translations);
        using var zip = new ZipArchive(zipStream, ZipArchiveMode.Read);

        foreach (var entry in zip.Entries
                     .Where(x => x.FullName.EndsWith($"{languageCode}.txt")))
        {
            using var dataStream = entry.Open();
            using var data = new StreamReader(dataStream);

            while (!data.EndOfStream)
            {
                yield return data.ReadLine();
            }
        }
    }

    internal static void Load()
    {
        var languageCode = !BootContext.SupportedLanguages.Contains(LocalizationManager.CurrentLanguageCode)
            ? English
            : LocalizationManager.CurrentLanguageCode;
        var languageSourceData = LocalizationManager.Sources[0];
        var languageIndex = languageSourceData.GetLanguageIndex(LocalizationManager.CurrentLanguage);
        var lineCount = 0;

        foreach (var line in GetTranslations(languageCode))
        {
            if (line == null)
            {
                continue;
            }

            var split = line.Split(new[] { '=' }, 2);

            if (split.Length != 2)
            {
                continue;
            }

            var term = split[0];
            var text = split[1];

            if (term == string.Empty)
            {
                continue;
            }

            text = Glossary.Aggregate(text, (current, kvp) => current.Replace(kvp.Key, kvp.Value));

            var termData = languageSourceData.GetTermData(term);

            if (termData?.Languages[languageIndex] != null)
            {
#if DEBUG
                if (languageIndex == 0)
                {
                    Main.Log($"term {term} overwritten with text {text}");
                }
#endif
                termData.Languages[languageIndex] = text;
            }
            else
            {
                languageSourceData.AddTerm(term).Languages[languageIndex] = text;
            }

            lineCount++;
        }

        Main.Info($"{lineCount} {languageCode} translation terms loaded.");
    }

    internal static bool HasTranslation(string term)
    {
        return LocalizationManager.Sources[0].ContainsTerm(term);
    }

    internal sealed class TranslatorBehaviour : MonoBehaviour
    {
        internal const string UbTranslationTag = "UB auto translation\n";

        private static TranslatorBehaviour _exporter;

        internal static readonly Dictionary<string, ExportStatus> CurrentExports = new();

        [NotNull]
        private static TranslatorBehaviour Exporter
        {
            get
            {
                if (_exporter != null)
                {
                    return _exporter;
                }

                _exporter = new GameObject().AddComponent<TranslatorBehaviour>();
                DontDestroyOnLoad(_exporter.gameObject);

                return _exporter;
            }
        }

        internal static void Cancel([NotNull] string exportName)
        {
            if (!CurrentExports.ContainsKey(exportName) || CurrentExports[exportName].Coroutine == null)
            {
                return;
            }

            Exporter.StopCoroutine(CurrentExports[exportName].Coroutine);
            CurrentExports.Remove(exportName);
        }

        internal static void TranslateUserCampaign(string languageCode, [NotNull] string exportName,
            [NotNull] UserCampaign userCampaign)
        {
            var newUserCampaign = userCampaign.DeepCopy();

            var oldUserCampaignTitle = Regex.Replace(userCampaign.Title, @"\[.+\] - (.*)", "$1");

            newUserCampaign.Title = $"[{languageCode}] - {oldUserCampaignTitle}";
            newUserCampaign.IsWorkshopItem = false;

            var coroutine = TranslateUserCampaignRoutine(languageCode, exportName, newUserCampaign);

            CurrentExports.Add(exportName,
                new ExportStatus
                {
                    Coroutine = Exporter.StartCoroutine(coroutine),
                    PercentageComplete = 0f,
                    LanguageCode = languageCode
                });
        }

        private static IEnumerator TranslateUserCampaignRoutine(string languageCode, [NotNull] string exportName,
            UserCampaign userCampaign)
        {
            yield return null;

            var current = 0;
            var total =
                userCampaign.UserDialogs
                    .SelectMany(x => x.AllDialogStates)
                    .SelectMany(x => x.DialogLines)
                    .Count() +
                userCampaign.UserItems.Count +
                userCampaign.UserMonsters.Count +
                userCampaign.UserNpcs.Count +
                userCampaign.UserMerchantInventories.Count +
                userCampaign.UserLootPacks.Count +
                userCampaign.UserLocations
                    .SelectMany(x => x.GadgetsByName)
                    .Count() +
                userCampaign.UserQuests
                    .SelectMany(x => x.AllQuestStepDescriptions)
                    .SelectMany(x => x.OutcomesTable)
                    .Count();

            IEnumerator Update()
            {
                current++;
                CurrentExports[exportName].PercentageComplete = (float)current / total;

                yield return null;
            }

            userCampaign.Description = Translate(userCampaign.Description, languageCode);
            userCampaign.TechnicalInfo = UbTranslationTag + Translate(userCampaign.TechnicalInfo, languageCode);

            // USER DIALOGS
            foreach (var dialog in userCampaign.UserDialogs)
            {
                dialog.Title = Translate(dialog.Title, languageCode);
                dialog.Description = Translate(dialog.Description, languageCode);

                foreach (var userDialogState in dialog.AllDialogStates
                             .Where(x => x.Type is "AnswerChoice" or "CharacterSpeech" or "NpcSpeech"))
                {
                    foreach (var dialogLine in userDialogState.DialogLines)
                    {
                        yield return Update();

                        dialogLine.TextLine = Translate(dialogLine.TextLine, languageCode);
                    }
                }
            }

            // USER ITEMS
            foreach (var item in userCampaign.UserItems)
            {
                yield return Update();

                item.Title = Translate(item.Title, languageCode);
                item.Description = Translate(item.Title, languageCode);

                if (item.DocumentFragments.Count == 0)
                {
                    continue;
                }

                item.DocumentFragments = item.DocumentFragments
                    .Select(documentFragment => Translate(documentFragment, languageCode)).ToList();
            }

            // USER LOCATIONS
            foreach (var userLocation in userCampaign.UserLocations)
            {
                userLocation.Title = Translate(userLocation.Title, languageCode);
                userLocation.Description = Translate(userLocation.Description, languageCode);

                foreach (var gadget in userLocation.GadgetsByName.Values)
                {
                    yield return Update();

                    foreach (var parameterValue in gadget.ParameterValues)
                    {
                        switch (parameterValue.GadgetParameterDescription.Name)
                        {
                            case "Speech":
                            case "Banter":
                            case "BanterLines":
                            case "DestinationLocation":
                            case "ExitLore":
                            case "WaypointTitle":
                                parameterValue.StringValue =
                                    Translate(parameterValue.StringValue, languageCode);
                                parameterValue.StringsList = parameterValue.StringsList
                                    .Select(stringValue => Translate(stringValue, languageCode))
                                    .ToList();

                                break;

                            case "LocationsList":
                                foreach (var destination in parameterValue.DestinationsList)
                                {
                                    destination.DisplayedTitle = Translate(destination.DisplayedTitle, languageCode);
                                }

                                break;
                        }
                    }
                }
            }

            // USER QUESTS
            foreach (var quest in userCampaign.UserQuests)
            {
                quest.Title = Translate(quest.Title, languageCode);
                quest.Description = Translate(quest.Description, languageCode);

                foreach (var userQuestStep in quest.AllQuestStepDescriptions)
                {
                    userQuestStep.Title = Translate(userQuestStep.Title, languageCode);
                    userQuestStep.Description = Translate(userQuestStep.Description, languageCode);

                    foreach (var outcome in userQuestStep.OutcomesTable)
                    {
                        yield return Update();

                        outcome.DescriptionText = Translate(outcome.DescriptionText, languageCode);
                    }
                }
            }

            // USER MONSTERS
            foreach (var monster in userCampaign.UserMonsters)
            {
                yield return Update();

                monster.Title = Translate(monster.Title, languageCode);
                monster.Description = Translate(monster.Description, languageCode);
            }

            // USER NPCs
            foreach (var npc in userCampaign.UserNpcs)
            {
                yield return Update();

                npc.Title = Translate(npc.Title, languageCode);
                npc.Description = Translate(npc.Description, languageCode);
            }

            // USER MERCHANT INVENTORIES
            foreach (var merchantInventory in userCampaign.UserMerchantInventories)
            {
                yield return Update();

                merchantInventory.Title = Translate(merchantInventory.Title, languageCode);
                merchantInventory.Description = Translate(merchantInventory.Description, languageCode);
            }

            // USER LOOT PACKS
            foreach (var lootPack in userCampaign.UserLootPacks)
            {
                yield return Update();

                lootPack.Title = Translate(lootPack.Title, languageCode);
                lootPack.Description = Translate(lootPack.Description, languageCode);
            }

            CurrentExports.Remove(exportName);

            var userCampaignPoolService = ServiceRepository.GetService<IUserCampaignPoolService>();

            userCampaignPoolService.SaveUserCampaign(userCampaign);
        }

        internal sealed class ExportStatus
        {
            internal Coroutine Coroutine;
            internal string LanguageCode;
            internal float PercentageComplete;
        }
    }
}
