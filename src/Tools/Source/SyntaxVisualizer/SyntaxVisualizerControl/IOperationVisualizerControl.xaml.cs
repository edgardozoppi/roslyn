// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Roslyn.SyntaxVisualizer.Control
{
    /// <summary>
    /// Interaction logic for IOperationVisualizerControl.xaml
    /// </summary>
    public partial class IOperationVisualizerControl : UserControl, IVisualizerControl
    {
        // Instances of this class are stored in the Tag field of each item in the treeview.
        private class IOperationTag
        {
            internal TextSpan Span { get; set; }
            internal TextSpan FullSpan { get; set; }
            internal TreeViewItem ParentItem { get; set; }
            internal string Kind { get; set; }
            internal IOperation Operation { get; set; }
            internal SyntaxNode SyntaxNode { get; set; }
            internal SyntaxToken SyntaxToken { get; set; }
            internal SyntaxTrivia SyntaxTrivia { get; set; }
            internal NodeCategory Category { get; set; }
        }

        #region Private State
        private TreeViewItem _currentSelection;
        private bool _isNavigatingFromSourceToTree;
        private bool _isNavigatingFromTreeToSource;
        private readonly System.Windows.Forms.PropertyGrid _propertyGrid;
        private static readonly Thickness s_defaultBorderThickness = new Thickness(1);
        #endregion

        #region Public Properties, Events
        public SyntaxTree SyntaxTree { get; private set; }
        public SemanticModel SemanticModel { get; private set; }
        public bool IsLazy { get; private set; }

        public event DgmlCreationDelegate DirectedGraphRequested;
        public event TreeNodeDelegate NavigationToSourceRequested;
        #endregion

        #region Public Methods
        public IOperationVisualizerControl()
        {
            InitializeComponent();

            _propertyGrid = new System.Windows.Forms.PropertyGrid();
            _propertyGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            _propertyGrid.PropertySort = System.Windows.Forms.PropertySort.Alphabetical;
            _propertyGrid.HelpVisible = false;
            _propertyGrid.ToolbarVisible = false;
            _propertyGrid.CommandsVisibleIfAvailable = false;
            windowsFormsHost.Child = _propertyGrid;
        }

        public void Clear()
        {
            treeView.Items.Clear();
            _propertyGrid.SelectedObject = null;
            typeTextLabel.Visibility = Visibility.Collapsed;
            kindTextLabel.Visibility = Visibility.Collapsed;
            typeValueLabel.Visibility = Visibility.Collapsed;
            kindValueLabel.Visibility = Visibility.Collapsed;
            typeValueLabel.Content = string.Empty;
            kindValueLabel.Content = string.Empty;
            legendButton.Visibility = Visibility.Hidden;
            disabledFeatureLabel.Visibility = Visibility.Collapsed;
        }

        // If lazy is true then treeview items are populated on-demand. In other words, when lazy is true
        // the children for any given item are only populated when the item is selected. If lazy is
        // false then the entire tree is populated at once (and this can result in bad performance when
        // displaying large trees).
        public void DisplayTree(SyntaxTree tree, SemanticModel model, bool lazy = true)
        {
            if (tree != null)
            {
                var enabled = CheckIOperationFeatureFlag(tree);

                if (enabled)
                {
                    IsLazy = lazy;
                    SyntaxTree = tree;
                    SemanticModel = model;
                    AddSyntax(null, SyntaxTree.GetRoot());
                    legendButton.Visibility = Visibility.Visible;
                }
            }
        }

        private bool CheckIOperationFeatureFlag(SyntaxTree tree)
        {
            var enabled = tree.Options.Features.ContainsKey("IOperation");

            if (!enabled)
            {
                disabledFeatureLabel.Visibility = Visibility.Visible;
            }

            return enabled;
        }

        // If lazy is true then treeview items are populated on-demand. In other words, when lazy is true
        // the children for any given item are only populated when the item is selected. If lazy is
        // false then the entire tree is populated at once (and this can result in bad performance when
        // displaying large trees).
        public void DisplaySyntaxNode(SyntaxNode node, SemanticModel model = null, bool lazy = true)
        {
            if (node != null)
            {
                IsLazy = lazy;
                SyntaxTree = node.SyntaxTree;
                SemanticModel = model;
                AddSyntax(null, node);
                legendButton.Visibility = Visibility.Visible;
            }
        }

        // Select the SyntaxNode / SyntaxToken / SyntaxTrivia whose position best matches the supplied position.
        public bool NavigateToBestMatch(int position, string kind = null,
            NodeCategory category = NodeCategory.None,
            bool highlightMatch = false, string highlightLegendDescription = null)
        {
            TreeViewItem match = null;

            if (treeView.HasItems && !_isNavigatingFromTreeToSource)
            {
                _isNavigatingFromSourceToTree = true;
                match = NavigateToBestMatch((TreeViewItem)treeView.Items[0], position, kind, category);
                _isNavigatingFromSourceToTree = false;
            }

            var matchFound = match != null;

            if (highlightMatch && matchFound)
            {
                match.Background = Brushes.Yellow;
                match.BorderBrush = Brushes.Black;
                match.BorderThickness = s_defaultBorderThickness;
                highlightLegendTextLabel.Visibility = Visibility.Visible;
                highlightLegendDescriptionLabel.Visibility = Visibility.Visible;
                if (!string.IsNullOrWhiteSpace(highlightLegendDescription))
                {
                    highlightLegendDescriptionLabel.Content = highlightLegendDescription;
                }
            }

            return matchFound;
        }

        // Select the SyntaxNode / SyntaxToken / SyntaxTrivia whose span best matches the supplied span.
        public bool NavigateToBestMatch(int start, int length, string kind = null,
            NodeCategory category = NodeCategory.None,
            bool highlightMatch = false, string highlightLegendDescription = null)
        {
            return NavigateToBestMatch(new TextSpan(start, length), kind, category, highlightMatch, highlightLegendDescription);
        }

        // Select the SyntaxNode / SyntaxToken / SyntaxTrivia whose span best matches the supplied span.
        public bool NavigateToBestMatch(TextSpan span, string kind = null,
            NodeCategory category = NodeCategory.None,
            bool highlightMatch = false, string highlightLegendDescription = null)
        {
            TreeViewItem match = null;

            if (treeView.HasItems && !_isNavigatingFromTreeToSource)
            {
                _isNavigatingFromSourceToTree = true;
                match = NavigateToBestMatch((TreeViewItem)treeView.Items[0], span, kind, category);
                _isNavigatingFromSourceToTree = false;
            }

            var matchFound = match != null;

            if (highlightMatch && matchFound)
            {
                match.Background = Brushes.Yellow;
                match.BorderBrush = Brushes.Black;
                match.BorderThickness = s_defaultBorderThickness;
                highlightLegendTextLabel.Visibility = Visibility.Visible;
                highlightLegendDescriptionLabel.Visibility = Visibility.Visible;
                if (!string.IsNullOrWhiteSpace(highlightLegendDescription))
                {
                    highlightLegendDescriptionLabel.Content = highlightLegendDescription;
                }
            }

            return matchFound;
        }
        #endregion

        #region Private Helpers - TreeView Navigation
        // Collapse all items in the treeview except for the supplied item. The supplied item
        // is also expanded, selected and scrolled into view.
        private void CollapseEverythingBut(TreeViewItem item)
        {
            if (item != null)
            {
                DeepCollapse((TreeViewItem)treeView.Items[0]);
                ExpandPathTo(item);
                item.IsSelected = true;
                item.BringIntoView();
            }
        }

        // Collapse the supplied treeview item including all its descendants.
        private void DeepCollapse(TreeViewItem item)
        {
            if (item != null)
            {
                item.IsExpanded = false;
                foreach (TreeViewItem child in item.Items)
                {
                    DeepCollapse(child);
                }
            }
        }

        // Ensure that the supplied treeview item and all its ancestors are expanded.
        private void ExpandPathTo(TreeViewItem item)
        {
            if (item != null)
            {
                item.IsExpanded = true;
                ExpandPathTo(((IOperationTag)item.Tag).ParentItem);
            }
        }

        // Select the SyntaxNode / SyntaxToken / SyntaxTrivia whose position best matches the supplied position.
        private TreeViewItem NavigateToBestMatch(TreeViewItem current, int position, string kind = null,
            NodeCategory category = NodeCategory.None)
        {
            TreeViewItem match = null;

            if (current != null)
            {
                IOperationTag currentTag = (IOperationTag)current.Tag;
                if (currentTag.FullSpan.Contains(position))
                {
                    CollapseEverythingBut(current);

                    foreach (TreeViewItem item in current.Items)
                    {
                        match = NavigateToBestMatch(item, position, kind, category);
                        if (match != null)
                        {
                            break;
                        }
                    }

                    if (match == null && (kind == null || currentTag.Kind == kind) &&
                       (category == NodeCategory.None || category == currentTag.Category))
                    {
                        match = current;
                    }
                }
            }

            return match;
        }

        // Select the SyntaxNode / SyntaxToken / SyntaxTrivia whose span best matches the supplied span.
        private TreeViewItem NavigateToBestMatch(TreeViewItem current, TextSpan span, string kind = null,
            NodeCategory category = NodeCategory.None)
        {
            TreeViewItem match = null;

            if (current != null)
            {
                IOperationTag currentTag = (IOperationTag)current.Tag;
                if (currentTag.FullSpan.Contains(span))
                {
                    if ((currentTag.Span == span || currentTag.FullSpan == span) && (kind == null || currentTag.Kind == kind))
                    {
                        CollapseEverythingBut(current);
                        match = current;
                    }
                    else
                    {
                        CollapseEverythingBut(current);

                        foreach (TreeViewItem item in current.Items)
                        {
                            match = NavigateToBestMatch(item, span, kind, category);
                            if (match != null)
                            {
                                break;
                            }
                        }

                        if (match == null && (kind == null || currentTag.Kind == kind) &&
                           (category == NodeCategory.None || category == currentTag.Category))
                        {
                            match = current;
                        }
                    }
                }
            }

            return match;
        }
        #endregion

        #region Private Helpers - TreeView Population
        // Helpers for populating the treeview.

        private void AddSyntaxOrIOperation(TreeViewItem parentItem, SyntaxNode node)
        {
            var operation = SemanticModel.GetOperation(node);
            if (operation == null)
            {
                AddSyntax(parentItem, node);
            }
            else
            {
                AddIOperation(parentItem, operation);
            }
        }

        private void AddIOperation(TreeViewItem parentItem, IOperation operation)
        {
            var syntax = operation.Syntax;
            var kind = operation.Kind.ToString();
            var tag = new IOperationTag()
            {
                Operation = operation,
                Category = NodeCategory.IOperationNode,
                Span = syntax.Span,
                FullSpan = syntax.FullSpan,
                ParentItem = parentItem
            };

            var item = new TreeViewItem()
            {
                Tag = tag,
                IsExpanded = false,
                Foreground = Brushes.Sienna,
                Background = syntax.ContainsDiagnostics ? Brushes.Pink : Brushes.White,
                Header = $"{operation.Kind} {syntax.Span.ToString()}"
            };

            if (SyntaxTree != null && syntax.ContainsDiagnostics)
            {
                item.ToolTip = string.Empty;
                foreach (var diagnostic in SyntaxTree.GetDiagnostics(syntax))
                {
                    item.ToolTip += diagnostic.ToString() + "\n";
                }

                item.ToolTip = item.ToolTip.ToString().Trim();
            }

            var children = operation.GetChildren();

            item.Selected += new RoutedEventHandler((sender, e) =>
            {
                _isNavigatingFromTreeToSource = true;

                typeTextLabel.Visibility = Visibility.Visible;
                kindTextLabel.Visibility = Visibility.Visible;
                typeValueLabel.Visibility = Visibility.Visible;
                kindValueLabel.Visibility = Visibility.Visible;
                typeValueLabel.Content = operation.GetType().Name;
                kindValueLabel.Content = kind;
                _propertyGrid.SelectedObject = operation;

                item.IsExpanded = true;

                if (!_isNavigatingFromSourceToTree && NavigationToSourceRequested != null)
                {
                    NavigationToSourceRequested(new TreeNode(syntax));
                }

                _isNavigatingFromTreeToSource = false;
                e.Handled = true;
            });

            item.Expanded += new RoutedEventHandler((sender, e) =>
            {
                if (item.Items.Count == 1 && item.Items[0] == null)
                {
                    // Remove placeholder child and populate real children.
                    item.Items.RemoveAt(0);
                    foreach (var child in children)
                    {
                        AddIOperation(item, child);
                    }
                }
            });

            if (parentItem == null)
            {
                treeView.Items.Clear();
                treeView.Items.Add(item);
            }
            else
            {
                parentItem.Items.Add(item);
            }

            if (children.Length > 0)
            {
                if (IsLazy)
                {
                    // Add placeholder child to indicate that real children need to be populated on expansion.
                    item.Items.Add(null);
                }
                else
                {
                    // Recursively populate all descendants.
                    foreach (var child in children)
                    {
                        AddIOperation(item, child);
                    }
                }
            }
        }

        private void AddSyntax(TreeViewItem parentItem, SyntaxNode node)
        {
            var kind = node.GetKind();
            var tag = new IOperationTag()
            {
                SyntaxNode = node,
                Category = NodeCategory.SyntaxNode,
                Span = node.Span,
                FullSpan = node.FullSpan,
                Kind = kind,
                ParentItem = parentItem
            };

            var item = new TreeViewItem()
            {
                Tag = tag,
                IsExpanded = false,
                Foreground = Brushes.Blue,
                Background = node.ContainsDiagnostics ? Brushes.Pink : Brushes.White,
                Header = tag.Kind + " " + node.Span.ToString()
            };

            if (SyntaxTree != null && node.ContainsDiagnostics)
            {
                item.ToolTip = string.Empty;
                foreach (var diagnostic in SyntaxTree.GetDiagnostics(node))
                {
                    item.ToolTip += diagnostic.ToString() + "\n";
                }

                item.ToolTip = item.ToolTip.ToString().Trim();
            }

            item.Selected += new RoutedEventHandler((sender, e) =>
            {
                _isNavigatingFromTreeToSource = true;

                typeTextLabel.Visibility = Visibility.Visible;
                kindTextLabel.Visibility = Visibility.Visible;
                typeValueLabel.Visibility = Visibility.Visible;
                kindValueLabel.Visibility = Visibility.Visible;
                typeValueLabel.Content = node.GetType().Name;
                kindValueLabel.Content = kind;
                _propertyGrid.SelectedObject = node;

                item.IsExpanded = true;

                if (!_isNavigatingFromSourceToTree && NavigationToSourceRequested != null)
                {
                    NavigationToSourceRequested(new TreeNode(node));
                }

                _isNavigatingFromTreeToSource = false;
                e.Handled = true;
            });

            item.Expanded += new RoutedEventHandler((sender, e) =>
            {
                if (item.Items.Count == 1 && item.Items[0] == null)
                {
                    // Remove placeholder child and populate real children.
                    item.Items.RemoveAt(0);
                    foreach (var child in node.ChildNodes())
                    {
                        AddSyntaxOrIOperation(item, child);
                    }
                }
            });

            if (parentItem == null)
            {
                treeView.Items.Clear();
                treeView.Items.Add(item);
            }
            else
            {
                parentItem.Items.Add(item);
            }

            if (node.ChildNodesAndTokens().Count > 0)
            {
                if (IsLazy)
                {
                    // Add placeholder child to indicate that real children need to be populated on expansion.
                    item.Items.Add(null);
                }
                else
                {
                    // Recursively populate all descendants.
                    foreach (var child in node.ChildNodes())
                    {
                        AddSyntaxOrIOperation(item, child);
                    }
                }
            }
        }
        #endregion

        #region Private Helpers - Other
        private void DisplaySymbolInPropertyGrid(ISymbol symbol)
        {
            if (symbol == null)
            {
                typeTextLabel.Visibility = Visibility.Collapsed;
                kindTextLabel.Visibility = Visibility.Collapsed;
                typeValueLabel.Visibility = Visibility.Collapsed;
                kindValueLabel.Visibility = Visibility.Collapsed;
                typeValueLabel.Content = string.Empty;
                kindValueLabel.Content = string.Empty;
            }
            else
            {
                typeTextLabel.Visibility = Visibility.Visible;
                kindTextLabel.Visibility = Visibility.Visible;
                typeValueLabel.Visibility = Visibility.Visible;
                kindValueLabel.Visibility = Visibility.Visible;
                typeValueLabel.Content = symbol.GetType().Name;
                kindValueLabel.Content = symbol.Kind.ToString();
            }

            _propertyGrid.SelectedObject = symbol;
        }

        private static TreeViewItem FindTreeViewItem(DependencyObject source)
        {
            while (source != null && !(source is TreeViewItem))
            {
                source = VisualTreeHelper.GetParent(source);
            }

            return (TreeViewItem)source;
        }
        #endregion

        #region Event Handlers
        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (treeView.SelectedItem != null)
            {
                _currentSelection = (TreeViewItem)treeView.SelectedItem;
            }
        }

        private void TreeView_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var item = FindTreeViewItem((DependencyObject)e.OriginalSource);

            if (item != null)
            {
                item.Focus();
            }
        }

        private void TreeView_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (DirectedGraphRequested != null)
            {
                directedSyntaxGraphMenuItem.Visibility = Visibility.Visible;
            }
            else
            {
                e.Handled = true;
            }
        }

        private void LegendButton_Click(object sender, RoutedEventArgs e)
        {
            legendPopup.IsOpen = true;
        }

        private void DirectedSyntaxGraphMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (_currentSelection != null)
            {
                var currentTag = (IOperationTag)_currentSelection.Tag;
                if (currentTag.Category == NodeCategory.SyntaxNode)
                {
                    DirectedGraphRequested(new TreeNode(currentTag.SyntaxNode), true);
                }
                else if (currentTag.Category == NodeCategory.IOperationNode)
                {
                    DirectedGraphRequested(new TreeNode(currentTag.Operation), true);
                }
            }
        }
        #endregion
    }
}
