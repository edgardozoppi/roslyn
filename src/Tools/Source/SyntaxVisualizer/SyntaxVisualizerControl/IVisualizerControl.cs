// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Roslyn.SyntaxVisualizer.Control
{
    public delegate void SyntaxNodeDelegate(SyntaxNode node);
    public delegate void SyntaxTokenDelegate(SyntaxToken token);
    public delegate void SyntaxTriviaDelegate(SyntaxTrivia trivia);
    public delegate void TreeNodeDelegate(TreeNode node);
    public delegate void DgmlCreationDelegate(TreeNode node, bool ioperationGraph);

    public interface IVisualizerControl
    {
        event DgmlCreationDelegate DirectedGraphRequested;
        event TreeNodeDelegate NavigationToSourceRequested;

        SyntaxTree SyntaxTree { get; }
        SemanticModel SemanticModel { get; }
        bool IsLazy { get; }

        void Clear();
        void DisplayTree(SyntaxTree tree, SemanticModel model = null, bool lazy = true);
        void DisplaySyntaxNode(SyntaxNode node, SemanticModel model = null, bool lazy = true);
        bool NavigateToBestMatch(int position, string kind = null,
            NodeCategory category = NodeCategory.None,
            bool highlightMatch = false, string highlightLegendDescription = null);
        bool NavigateToBestMatch(int start, int length, string kind = null,
            NodeCategory category = NodeCategory.None,
            bool highlightMatch = false, string highlightLegendDescription = null);
        bool NavigateToBestMatch(TextSpan span, string kind = null,
            NodeCategory category = NodeCategory.None,
            bool highlightMatch = false, string highlightLegendDescription = null);
    }
}
