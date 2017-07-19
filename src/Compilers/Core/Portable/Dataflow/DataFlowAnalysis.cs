using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Microsoft.CodeAnalysis.Semantics.Dataflow
{
    internal abstract class AbstractDomain<TAbstractValue>
    {
        public abstract TAbstractValue Bottom { get; }

        public virtual TAbstractValue Copy(TAbstractValue value)
        {
            return value;
        }

        public abstract TAbstractValue Merge(TAbstractValue value1, TAbstractValue value2);

        public virtual TAbstractValue Merge(IEnumerable<TAbstractValue> values)
        {
            TAbstractValue result;

            if (values.Count() > 1)
            {
                result = values.Aggregate((accum, value) => Merge(accum, value));
            }
            else
            {
                result = Copy(values.Single());
            }

            return result;
        }

        public abstract int Compare(TAbstractValue oldValue, TAbstractValue newValue);
    }

    internal abstract class DataFlowAnalysis<TAbstractValue>
    {
        #region DataFlowAnalysisResult

        public class DataFlowAnalysisResult
        {
            public TAbstractValue Input { get; set; }
            public TAbstractValue Output { get; set; }
        }

        #endregion

        protected AbstractDomain<TAbstractValue> _domain;
        protected IDictionary<BasicBlock, DataFlowAnalysisResult> _result;

        protected void Initialize(AbstractDomain<TAbstractValue> domain)
        {
            _domain = domain;
            _result = new Dictionary<BasicBlock, DataFlowAnalysisResult>();
        }

        public DataFlowAnalysisResult this[BasicBlock block]
        {
            get => _result[block];
        }

        public virtual void Run(ControlFlowGraph cfg)
        {
            _result.Clear();

            foreach (var block in cfg.Blocks)
            {
                _result.Add(block, new DataFlowAnalysisResult());
            }

            var entry = Entry(cfg);
            var worklist = new HashSet<BasicBlock>();

            Output(_result[entry], _domain.Bottom);
            worklist.UnionWith(Successors(entry));

            while (worklist.Count > 0)
            {
                var block = worklist.First();
                worklist.Remove(block);

                var blockResult = _result[block];
                var inputs = Predecessors(block).Select(b => Output(_result[b]));
                var input = _domain.Merge(inputs);
                var output = Flow(block, Input(blockResult), input);
                var compare = _domain.Compare(Output(blockResult), output);

                // old < new ?
                if (compare < 0)
                {
                    Input(blockResult, input);
                    Output(blockResult, output);

                    worklist.UnionWith(Successors(block));
                }
            }
        }

        protected abstract BasicBlock Entry(ControlFlowGraph cfg);

        protected abstract IEnumerable<BasicBlock> Predecessors(BasicBlock block);

        protected abstract IEnumerable<BasicBlock> Successors(BasicBlock block);

        protected abstract TAbstractValue Input(DataFlowAnalysisResult result);

        protected abstract TAbstractValue Output(DataFlowAnalysisResult result);

        protected abstract void Input(DataFlowAnalysisResult result, TAbstractValue value);

        protected abstract void Output(DataFlowAnalysisResult result, TAbstractValue value);

        protected abstract TAbstractValue Flow(BasicBlock block, TAbstractValue oldInput, TAbstractValue newInput);
    }

    internal abstract class ForwardDataFlowAnalysis<TAbstractValue> : DataFlowAnalysis<TAbstractValue>
    {
        protected override BasicBlock Entry(ControlFlowGraph cfg) => cfg.Entry;

        protected override IEnumerable<BasicBlock> Predecessors(BasicBlock block) => block.Predecessors;

        protected override IEnumerable<BasicBlock> Successors(BasicBlock block) => block.Successors;

        protected override TAbstractValue Input(DataFlowAnalysisResult result) => result.Input;

        protected override TAbstractValue Output(DataFlowAnalysisResult result) => result.Output;

        protected override void Input(DataFlowAnalysisResult result, TAbstractValue value) => result.Input = value;

        protected override void Output(DataFlowAnalysisResult result, TAbstractValue value) => result.Output = value;
    }

    internal abstract class BackwardDataFlowAnalysis<TAbstractValue> : DataFlowAnalysis<TAbstractValue>
    {
        protected override BasicBlock Entry(ControlFlowGraph cfg) => cfg.Exit;

        protected override IEnumerable<BasicBlock> Predecessors(BasicBlock block) => block.Successors;

        protected override IEnumerable<BasicBlock> Successors(BasicBlock block) => block.Predecessors;

        protected override TAbstractValue Input(DataFlowAnalysisResult result) => result.Output;

        protected override TAbstractValue Output(DataFlowAnalysisResult result) => result.Input;

        protected override void Input(DataFlowAnalysisResult result, TAbstractValue value) => result.Output = value;

        protected override void Output(DataFlowAnalysisResult result, TAbstractValue value) => result.Input = value;
    }
}
