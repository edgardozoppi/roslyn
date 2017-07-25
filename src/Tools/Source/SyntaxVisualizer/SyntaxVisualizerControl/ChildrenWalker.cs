// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Semantics;

namespace Roslyn.SyntaxVisualizer.Control
{
    public static class IOperationExtensions
    {
        // TODO: When 2.6.0 can be dependended on, this should be removed, and we should just use the Children
        // IOperation property.
        public static ImmutableArray<IOperation> GetChildren(this IOperation operation)
        {
            return new ChildrenWalker().GetChildren(operation);
        }
    }

    internal class ChildrenWalker : OperationWalker
    {
        private bool _recursed = false;
        private readonly IList<IOperation> _children = new List<IOperation>();

        public ImmutableArray<IOperation> GetChildren(IOperation operation)
        {
            _recursed = false;
            Visit(operation);
            return _children.ToImmutableArray();
        }

        public override void Visit(IOperation operation)
        {
            if (!_recursed)
            {
                _recursed = true;
                base.Visit(operation);
            }
            else if (operation != null)
            {
                _children.Add(operation);
            }
        }
    }
}
