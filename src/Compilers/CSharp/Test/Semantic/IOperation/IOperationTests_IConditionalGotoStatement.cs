﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Semantics;
using Microsoft.CodeAnalysis.Test.Utilities;
using Roslyn.Test.Utilities;
using Xunit;

namespace Microsoft.CodeAnalysis.CSharp.UnitTests
{
    public partial class IOperationTests : SemanticModelTestBase
    {
        [Fact]
        public void IConditionalGotoStatement_FromIf()
        {
            string source = @"
class C
{
    /*<bind>*/
    static void Method(int p)
    {
        if (p < 0) p = 0;
    }
    /*</bind>*/
}
";
string expectedOperationTree = @"
IBlockStatement (1 statements) (OperationKind.BlockStatement) (Syntax: '{ ... }')
  IBlockStatement (3 statements) (OperationKind.BlockStatement) (Syntax: 'if (p < 0) p = 0;')
    IConditionalGotoStatement (Target Symbol: <afterif-2>, JumpIfTrue: False) (OperationKind.ConditionalGotoStatement) (Syntax: 'p < 0')
      Condition: IBinaryOperatorExpression (BinaryOperationKind.IntegerLessThan) (OperationKind.BinaryOperatorExpression, Type: System.Boolean) (Syntax: 'p < 0')
          Left: IParameterReferenceExpression: p (OperationKind.ParameterReferenceExpression, Type: System.Int32) (Syntax: 'p')
          Right: ILiteralExpression (Text: 0) (OperationKind.LiteralExpression, Type: System.Int32, Constant: 0) (Syntax: '0')
    IExpressionStatement (OperationKind.ExpressionStatement) (Syntax: 'p = 0;')
      Expression: ISimpleAssignmentExpression (OperationKind.SimpleAssignmentExpression, Type: System.Int32) (Syntax: 'p = 0')
          Left: IParameterReferenceExpression: p (OperationKind.ParameterReferenceExpression, Type: System.Int32) (Syntax: 'p')
          Right: ILiteralExpression (Text: 0) (OperationKind.LiteralExpression, Type: System.Int32, Constant: 0) (Syntax: '0')
    ILabelStatement (OperationKind.LabelStatement) (Syntax: 'if (p < 0) p = 0;')
      LabeledStatement: null
";
            var expectedDiagnostics = DiagnosticDescription.None;
            VerifyOperationTreeAndDiagnosticsForTest<BaseMethodDeclarationSyntax>(source, expectedOperationTree, expectedDiagnostics, highLevelOperation: false);
        }
    }
}
