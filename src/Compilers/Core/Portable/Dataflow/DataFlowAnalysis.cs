using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
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

        [DebuggerStepThrough]
        protected abstract BasicBlock Entry(ControlFlowGraph cfg);

        [DebuggerStepThrough]
        protected abstract IEnumerable<BasicBlock> Predecessors(BasicBlock block);

        [DebuggerStepThrough]
        protected abstract IEnumerable<BasicBlock> Successors(BasicBlock block);

        [DebuggerStepThrough]
        protected abstract TAbstractValue Input(DataFlowAnalysisResult result);

        [DebuggerStepThrough]
        protected abstract TAbstractValue Output(DataFlowAnalysisResult result);

        [DebuggerStepThrough]
        protected abstract void Input(DataFlowAnalysisResult result, TAbstractValue value);

        [DebuggerStepThrough]
        protected abstract void Output(DataFlowAnalysisResult result, TAbstractValue value);

        [DebuggerStepThrough]
        protected abstract TAbstractValue Flow(BasicBlock block, TAbstractValue oldInput, TAbstractValue newInput);
    }

    internal abstract class ForwardDataFlowAnalysis<TAbstractValue> : DataFlowAnalysis<TAbstractValue>
    {
        [DebuggerStepThrough]
        protected override BasicBlock Entry(ControlFlowGraph cfg) => cfg.Entry;

        [DebuggerStepThrough]
        protected override IEnumerable<BasicBlock> Predecessors(BasicBlock block) => block.Predecessors;

        [DebuggerStepThrough]
        protected override IEnumerable<BasicBlock> Successors(BasicBlock block) => block.Successors;

        [DebuggerStepThrough]
        protected override TAbstractValue Input(DataFlowAnalysisResult result) => result.Input;

        [DebuggerStepThrough]
        protected override TAbstractValue Output(DataFlowAnalysisResult result) => result.Output;

        [DebuggerStepThrough]
        protected override void Input(DataFlowAnalysisResult result, TAbstractValue value) => result.Input = value;

        [DebuggerStepThrough]
        protected override void Output(DataFlowAnalysisResult result, TAbstractValue value) => result.Output = value;
    }

    internal abstract class BackwardDataFlowAnalysis<TAbstractValue> : DataFlowAnalysis<TAbstractValue>
    {
        [DebuggerStepThrough]
        protected override BasicBlock Entry(ControlFlowGraph cfg) => cfg.Exit;

        [DebuggerStepThrough]
        protected override IEnumerable<BasicBlock> Predecessors(BasicBlock block) => block.Successors;

        [DebuggerStepThrough]
        protected override IEnumerable<BasicBlock> Successors(BasicBlock block) => block.Predecessors;

        [DebuggerStepThrough]
        protected override TAbstractValue Input(DataFlowAnalysisResult result) => result.Output;

        [DebuggerStepThrough]
        protected override TAbstractValue Output(DataFlowAnalysisResult result) => result.Input;

        [DebuggerStepThrough]
        protected override void Input(DataFlowAnalysisResult result, TAbstractValue value) => result.Output = value;

        [DebuggerStepThrough]
        protected override void Output(DataFlowAnalysisResult result, TAbstractValue value) => result.Input = value;
    }
}
