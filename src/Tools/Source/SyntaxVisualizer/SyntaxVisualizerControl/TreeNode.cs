// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;

namespace Roslyn.SyntaxVisualizer.Control
{
    public struct TreeNode
    {
        public TreeNode(SyntaxNode syntaxNode) :
            this(NodeCategory.SyntaxNode, syntaxNode, default, default, null)
        { }

        public TreeNode(SyntaxToken syntaxToken) :
            this(NodeCategory.SyntaxToken, null, syntaxToken, default, null)
        { }

        public TreeNode(SyntaxTrivia syntaxTrivia) :
            this(NodeCategory.SyntaxTrivia, null, default, syntaxTrivia, null)
        { }

        public TreeNode(IOperation operation) :
            this(NodeCategory.IOperationNode, null, default, default, operation)
        { }

        private TreeNode(NodeCategory type, SyntaxNode syntaxNode, SyntaxToken syntaxToken, SyntaxTrivia syntaxTrivia, IOperation operation)
        {
            Category = type;
            SyntaxNode = syntaxNode;
            SyntaxToken = syntaxToken;
            SyntaxTrivia = syntaxTrivia;
            Operation = operation;
        }

        public NodeCategory Category { get; }
        public SyntaxNode SyntaxNode { get; }
        public SyntaxToken SyntaxToken { get; }
        public SyntaxTrivia SyntaxTrivia { get; }
        public IOperation Operation { get; }
    }
}
