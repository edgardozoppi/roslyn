using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.CodeAnalysis.Semantics.Dataflow
{
    using ZeroAnalysisAbstractValue = IDictionary<ILocalSymbol, ZeroAnalysis.ZeroAbstractValue>;

    internal class ZeroAnalysis : ForwardDataFlowAnalysis<ZeroAnalysisAbstractValue>
    {
        #region ZeroAbstractValue

        public enum ZeroAbstractValue
        {
            Undefined, Zero, NotZero, MaybeZero
        }

        #endregion

        #region ZeroAbstractDomain

        private class ZeroAbstractDomain : AbstractDomain<ZeroAbstractValue>
        {
            public static ZeroAbstractDomain Default = new ZeroAbstractDomain();

            private ZeroAbstractDomain() { }

            public override ZeroAbstractValue Bottom => ZeroAbstractValue.Undefined;

            public override int Compare(ZeroAbstractValue oldValue, ZeroAbstractValue newValue)
            {
                return Comparer<ZeroAbstractValue>.Default.Compare(oldValue, newValue);
            }

            public override ZeroAbstractValue Merge(ZeroAbstractValue value1, ZeroAbstractValue value2)
            {
                ZeroAbstractValue result;

                if (value1 == ZeroAbstractValue.MaybeZero ||
                    value2 == ZeroAbstractValue.MaybeZero)
                {
                    result = ZeroAbstractValue.MaybeZero;
                }
                else if (value1 == ZeroAbstractValue.Undefined)
                {
                    result = value2;
                }
                else if (value2 == ZeroAbstractValue.Undefined)
                {
                    result = value1;
                }
                else if (value1 != value2)
                {
                    result = ZeroAbstractValue.MaybeZero;
                }
                else
                {
                    result = value1;
                }

                return result;
            }
        }

        #endregion

        public ZeroAnalysis()
        {
            var domain = new MappingAbstractDomain<ILocalSymbol, ZeroAbstractValue>(ZeroAbstractDomain.Default);
            base.Initialize(domain);
        }

        protected override ZeroAnalysisAbstractValue Flow(BasicBlock block, ZeroAnalysisAbstractValue oldInput, ZeroAnalysisAbstractValue newInput)
        {
            var output = new Dictionary<ILocalSymbol, ZeroAbstractValue>(newInput);

            foreach (var statement in block.Statements)
            {
                Flow(statement, output);
            }

            return output;
        }

        private void Flow(IOperation statement, ZeroAnalysisAbstractValue output)
        {
            switch (statement.Kind)
            {
                case OperationKind.ExpressionStatement:
                    Flow(statement as IExpressionStatement, output);
                    break;
            }
        }

        private void Flow(IExpressionStatement statement, ZeroAnalysisAbstractValue output)
        {
            if (statement.Expression is ISimpleAssignmentExpression assignment &&
                assignment.Target is ILocalReferenceExpression localReference)
            {
                output[localReference.Local] = GetAbstractValue(assignment.Value, output);
            }
        }

        private ZeroAbstractValue GetAbstractValue(IOperation value, ZeroAnalysisAbstractValue output)
        {
            var result = ZeroAbstractValue.MaybeZero;

            if (value is ILiteralExpression literal &&
                literal.ConstantValue.HasValue)
            {
                if (literal.ConstantValue.Value is 0)
                {
                    result = ZeroAbstractValue.Zero;
                }
                else
                {
                    result = ZeroAbstractValue.NotZero;
                }
            }
            else if (value is ILocalReferenceExpression localReference)
            {
                output.TryGetValue(localReference.Local, out result);
            }

            return result;
        }
    }
}
