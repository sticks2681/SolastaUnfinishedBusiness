﻿using System;
using System.Collections.Generic;
using ModKit;
using ModKit.Utility;
using UnityEngine;

namespace SolastaCommunityExpansion.DataViewer;

public class ReflectionTreeView
{
    private readonly ReflectionSearchResult _searchResults = new();
    private GUIStyle _buttonStyle;
    private float _height;
    private bool _mouseOver;
    private int _nodesCount;
    private int _skipLevels;
    private int _startIndex;
    private int _totalNodeCount;
    private ReflectionTree _tree;
    private GUIStyle _valueStyle;

    private Rect _viewerRect;
    private int searchBreadth;
    private int searchDepth;
    private string searchText = "";
    private int visitCount;

    public ReflectionTreeView() { }

    public ReflectionTreeView(object root)
    {
        SetRoot(root);
    }

    public float DepthDelta { get; set; } = 30f;

    public static int MaxRows => Main.Settings.MaxRows;

    public object Root => _tree.Root;

    public float TitleMinWidth { get; set; } = 300f;

    private void updateCounts(int visitCount, int depth, int breadth)
    {
        this.visitCount = visitCount;
        searchDepth = depth;
        searchBreadth = breadth;
    }

    public void Clear()
    {
        _tree = null;
        _searchResults.Clear();
    }

    public void SetRoot(object root)
    {
        if (_tree != null)
        {
            _tree.SetRoot(root);
        }
        else
        {
            _tree = new ReflectionTree(root);
        }

        _searchResults.Node = null;
#pragma warning disable CS0618 // Type or member is obsolete
        _tree.RootNode.Expanded = ToggleState.On;
#pragma warning restore CS0618 // Type or member is obsolete
        ReflectionSearch.Shared.StartSearch(_tree.RootNode, searchText, updateCounts, _searchResults);
    }

    public void OnGUI(bool drawRoot = true, bool collapse = false)
    {
        if (_tree == null)
        {
            return;
        }

        if (_buttonStyle == null)
        {
            _buttonStyle = new GUIStyle(GUI.skin.button) {alignment = TextAnchor.MiddleLeft, stretchHeight = true};
        }

        if (_valueStyle == null)
        {
            _valueStyle = new GUIStyle(GUI.skin.box) {alignment = TextAnchor.MiddleLeft, stretchHeight = true};
        }

        var startIndexUBound = Math.Max(0, _nodesCount - MaxRows);

        // mouse wheel & fix scroll position
        if (Event.current.type == EventType.Layout)
        {
            _totalNodeCount = _tree.RootNode.ChildrenCount;
            if (startIndexUBound > 0)
            {
                if (_mouseOver)
                {
                    var delta = Input.mouseScrollDelta;
                    if (delta.y > 0 && _startIndex > 0)
                    {
                        _startIndex--;
                    }
                    else if (delta.y < 0 && _startIndex < startIndexUBound)
                    {
                        _startIndex++;
                    }
                }

                if (_startIndex > startIndexUBound)
                {
                    _startIndex = startIndexUBound;
                }
            }
            else
            {
                _startIndex = 0;
            }
        }

        using (new GUILayout.VerticalScope())
        {
            // tool-bar
            using (new GUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Collapse", GUILayout.ExpandWidth(false)))
                {
                    collapse = true;
                    _skipLevels = 0;
                }

                if (GUILayout.Button("Refresh", GUILayout.ExpandWidth(false)))
                {
                    _tree.RootNode.SetDirty();
                }

                GUILayout.Space(10f);
                Main.Settings.MaxRows = GUIHelper.AdjusterButton(Main.Settings.MaxRows, "Max Rows:", 10);
                GUILayout.Space(10f);
                GUILayout.Label($"Scroll: {_startIndex} / {_totalNodeCount}", GUILayout.ExpandWidth(false));
                GUILayout.Space(10f);
                UI.ActionTextField(ref searchText, "searhText", _ => { }, () =>
                {
                    searchText = searchText.Trim();
                    ReflectionSearch.Shared.StartSearch(_tree.RootNode, searchText, updateCounts, _searchResults);
                }, UI.Width(250));
                GUILayout.Space(10f);
                var isSearching = ReflectionSearch.Shared.IsSearching;
                UI.ActionButton(isSearching ? "Stop" : "Search", () =>
                {
                    if (isSearching)
                    {
                        ReflectionSearch.Shared.Stop();
                    }
                    else
                    {
                        searchText = searchText.Trim();
                        ReflectionSearch.Shared.StartSearch(_tree.RootNode, searchText, updateCounts,
                            _searchResults);
                    }
                }, UI.AutoWidth());
                GUILayout.Space(10f);
                if (GUIHelper.AdjusterButton(ref Main.Settings.MaxSearchDepth, "Max Depth:", 0))
                {
                    ReflectionSearch.Shared.StartSearch(_tree.RootNode, searchText, updateCounts, _searchResults);
                }

                GUILayout.Space(10f);
                if (visitCount > 0)
                {
                    GUILayout.Label($"found {_searchResults.Count}".Cyan() +
                                    $" visited: {visitCount} (d: {searchDepth} b: {searchBreadth})".Orange());
                }

                GUILayout.FlexibleSpace();
            }

            // view
            using (new GUILayout.VerticalScope())
            {
                using (new GUILayout.ScrollViewScope(new Vector2(), GUIStyle.none, GUIStyle.none,
                           GUILayout.Height(_height)))
                {
                    using (new GUILayout.HorizontalScope(GUI.skin.box))
                    {
                        // nodes
                        using (new GUILayout.VerticalScope())
                        {
                            _nodesCount = 0;
                            if (searchText.Length > 0)
                            {
                                _searchResults.Traverse((node, depth) =>
                                {
                                    var toggleState = node.ToggleState;
                                    if (!node.Node.hasChildren)
                                    {
                                        toggleState = ToggleState.None;
                                    }
                                    else if (node.ToggleState == ToggleState.None)
                                    {
                                        toggleState = ToggleState.Off;
                                    }

                                    if (node.Node.NodeType == NodeType.Root)
                                    {
                                        if (node.matches.Count == 0)
                                        {
                                            return false;
                                        }

                                        GUILayout.Label("Search Results".Cyan().Bold());
                                    }
                                    else
                                    {
                                        DrawNodePrivate(node.Node, depth, ref toggleState);
                                    }

                                    if (node.ToggleState != toggleState) { Main.Log(node.ToString()); }

                                    node.ToggleState = toggleState;
                                    if (toggleState.IsOn())
                                    {
                                        DrawChildren(node.Node, depth + 1, collapse);
                                    }

                                    return true;
                                });
                            }

                            if (drawRoot)
                            {
                                DrawNode(_tree.RootNode, 0, collapse);
                            }
                            else
                            {
                                DrawChildren(_tree.RootNode, 0, collapse);
                            }
                        }

                        // scrollbar
                        _startIndex = (int)GUILayout.VerticalScrollbar(_startIndex, MaxRows, 0f,
                            Math.Max(MaxRows, _totalNodeCount), GUILayout.ExpandHeight(true));
                    }

                    // cache height
                    if (Event.current.type == EventType.Repaint)
                    {
                        _mouseOver = _viewerRect.Contains(Event.current.mousePosition);
                        _viewerRect = GUILayoutUtility.GetLastRect();
                        _height = _viewerRect.height + 5f;
                    }
                }
            }
        }
    }

    private void DrawNodePrivate(Node node, int depth, ref ToggleState expanded)
    {
        _nodesCount++;

        if (_nodesCount > _startIndex && _nodesCount <= _startIndex + MaxRows)
        {
            using (new GUILayout.HorizontalScope())
            {
                // title
                GUILayout.Space(DepthDelta * (depth - _skipLevels));
                var name = node.Name;
                name = name.MarkedSubstring(searchText);
                UI.ToggleButton(ref expanded,
                    $"[{node.NodeTypePrefix}] ".color(RGBA.grey) +
                    name + " : " + node.Type.Name.color(
                        node.IsBaseType ? RGBA.grey :
                        node.IsGameObject ? RGBA.magenta :
                        node.IsEnumerable ? RGBA.cyan : RGBA.orange),
                    _buttonStyle, GUILayout.ExpandWidth(false), GUILayout.MinWidth(TitleMinWidth));

                // value
                var originalColor = GUI.contentColor;
                GUI.contentColor = node.IsException ? Color.red : node.IsNull ? Color.grey : originalColor;
                GUILayout.TextArea(node.ValueText.MarkedSubstring(searchText));
                GUI.contentColor = originalColor;

                // instance type
                if (node.InstType != null && node.InstType != node.Type)
                {
                    GUILayout.Label(node.InstType.Name.color(RGBA.yellow), _buttonStyle,
                        GUILayout.ExpandWidth(false));
                }
            }
        }
    }

    private void DrawNode(Node node, int depth, bool collapse)
    {
#pragma warning disable CS0618 // Type or member is obsolete
        var expanded = node.Expanded;
        if (depth >= _skipLevels && !(collapse && depth > 0))
        {
            if (!node.hasChildren)
            {
                expanded = ToggleState.None;
            }
            else if (node.Expanded == ToggleState.None)
            {
                expanded = ToggleState.Off;
            }

            DrawNodePrivate(node, depth, ref expanded);
            node.Expanded = expanded;
        }

        if (collapse)
        {
            node.Expanded = ToggleState.Off;
        }

        // children
        if (expanded.IsOn())
        {
            DrawChildren(node, depth + 1, collapse);
        }
#pragma warning restore CS0618 // Type or member is obsolete
    }

    private void DrawChildren(Node node, int depth, bool collapse, Func<Node, bool> hoist = null)
    {
        if (node.IsBaseType)
        {
            return;
        }

        if (hoist == null)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            hoist = n => n.Matches;
#pragma warning restore CS0618 // Type or member is obsolete
        }

        var toHoist = new List<Node>();
        var others = new List<Node>();
        var nodesCount = _nodesCount;
        var maxNodeCount = _startIndex + (MaxRows * 2);
        foreach (var child in node.GetItemNodes())
        {
            if (nodesCount > maxNodeCount)
            {
                break;
            }

            nodesCount++;
            if (hoist(child))
            {
                toHoist.Add(child);
            }
            else
            {
                others.Add(child);
            }
        }

        foreach (var child in node.GetComponentNodes())
        {
            if (nodesCount > maxNodeCount)
            {
                break;
            }

            nodesCount++;
            if (hoist(child))
            {
                toHoist.Add(child);
            }
            else
            {
                others.Add(child);
            }
        }

        foreach (var child in node.GetPropertyNodes())
        {
            if (nodesCount > maxNodeCount)
            {
                break;
            }

            nodesCount++;
            if (hoist(child))
            {
                toHoist.Add(child);
            }
            else
            {
                others.Add(child);
            }
        }

        foreach (var child in node.GetFieldNodes())
        {
            if (nodesCount > maxNodeCount)
            {
                break;
            }

            nodesCount++;
            if (hoist(child))
            {
                toHoist.Add(child);
            }
            else
            {
                others.Add(child);
            }
        }

        foreach (var child in toHoist) { DrawNode(child, depth, collapse); }

        foreach (var child in others) { DrawNode(child, depth, collapse); }

        _totalNodeCount = Math.Max(_nodesCount, _totalNodeCount);
    }
}
