using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.CodeAnalysis.Semantics.Dataflow
{
    using IZeroAnalysisAbstractValue = IDictionary<ILocalSymbol, ZeroAnalysis.ZeroAbstractValue>;
    using ZeroAnalysisAbstractValue = Dictionary<ILocalSymbol, ZeroAnalysis.ZeroAbstractValue>;

    internal class ZeroAnalysisAbstractValueFormat : IFormatProvider, ICustomFormatter
    {
        public static ZeroAnalysisAbstractValueFormat Default = new ZeroAnalysisAbstractValueFormat();

        private ZeroAnalysisAbstractValueFormat() { }

        public object GetFormat(Type formatType)
        {
            if (formatType == typeof(ICustomFormatter)) return this;
            else return null;
        }

        public string Format(string fmt, object arg, IFormatProvider formatProvider)
        {
            string result;

            if (arg is IZeroAnalysisAbstractValue value)
            {
                var builder = new StringBuilder();

                if (value.Count > 0)
                {
                    foreach (var entry in value)
                    {
                        builder.AppendLine($"{entry.Key.Name} = {entry.Value}");
                    }
                }
                else
                {
                    builder.Append("Empty");
                }

                result = builder.ToString().Trim();
            }
            else
            {
                result = Convert.ToString(arg);
            }

            return result;
        }
    }

    internal class ZeroAnalysis : ForwardDataFlowAnalysis<IZeroAnalysisAbstractValue>
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

        protected override IZeroAnalysisAbstractValue Flow(BasicBlock block, IZeroAnalysisAbstractValue oldInput, IZeroAnalysisAbstractValue newInput)
        {
            var output = new ZeroAnalysisAbstractValue(newInput);

            foreach (var statement in block.Statements)
            {
                Flow(statement, output);
            }

            return output;
        }

        private void Flow(IOperation statement, IZeroAnalysisAbstractValue output)
        {
            switch (statement.Kind)
            {
                case OperationKind.ExpressionStatement:
                    Flow(statement as IExpressionStatement, output);
                    break;
            }
        }

        private void Flow(IExpressionStatement statement, IZeroAnalysisAbstractValue output)
        {
            if (statement.Expression is ISimpleAssignmentExpression assignment &&
                assignment.Target is ILocalReferenceExpression localReference)
            {
                output[localReference.Local] = GetAbstractValue(assignment.Value, output);
            }
        }

        private ZeroAbstractValue GetAbstractValue(IOperation value, IZeroAnalysisAbstractValue output)
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
