using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Roslyn.SyntaxVisualizer.Control
{
    public delegate void SyntaxNodeDelegate(SyntaxNode node);
    public delegate void SyntaxTokenDelegate(SyntaxToken token);
    public delegate void SyntaxTriviaDelegate(SyntaxTrivia trivia);

    public interface IVisualizerControl
    {
        event SyntaxNodeDelegate SyntaxNodeDirectedGraphRequested;
        event SyntaxNodeDelegate SyntaxNodeNavigationToSourceRequested;

        event SyntaxTokenDelegate SyntaxTokenDirectedGraphRequested;
        event SyntaxTokenDelegate SyntaxTokenNavigationToSourceRequested;

        event SyntaxTriviaDelegate SyntaxTriviaDirectedGraphRequested;
        event SyntaxTriviaDelegate SyntaxTriviaNavigationToSourceRequested;

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
