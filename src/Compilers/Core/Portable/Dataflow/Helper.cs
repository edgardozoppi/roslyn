using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.CodeAnalysis.Semantics.Dataflow
{
    internal static class Helper
    {
        public static bool IsStatement(this IOperation operation)
        {
            switch (operation.Kind)
            {
                case OperationKind.InvalidStatement:
                case OperationKind.BlockStatement:
                case OperationKind.VariableDeclarationStatement:
                case OperationKind.SwitchStatement:
                case OperationKind.IfStatement:
                case OperationKind.LoopStatement:
                case OperationKind.LabelStatement:
                case OperationKind.BranchStatement:
                case OperationKind.EmptyStatement:
                case OperationKind.ThrowStatement:
                case OperationKind.ReturnStatement:
                case OperationKind.YieldBreakStatement:
                case OperationKind.LockStatement:
                case OperationKind.TryStatement:
                case OperationKind.UsingStatement:
                case OperationKind.YieldReturnStatement:
                case OperationKind.ExpressionStatement:
                case OperationKind.FixedStatement:
                case OperationKind.LocalFunctionStatement:
                case OperationKind.StopStatement:
                case OperationKind.EndStatement:
                case OperationKind.WithStatement:
                case OperationKind.ConditionalGotoStatement:
                    return true;

                default:
                    return false;
            }
        }
    }
}
