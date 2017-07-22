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

    internal class DataFlowAnalysisInfo<TAbstractValue>
    {
        public TAbstractValue Input { get; set; }
        public TAbstractValue Output { get; set; }
    }

    internal class DataFlowAnalysisResult<TAbstractValue>
    {
        private IDictionary<BasicBlock, DataFlowAnalysisInfo<TAbstractValue>> _info;

        public DataFlowAnalysisResult()
        {
            _info = new Dictionary<BasicBlock, DataFlowAnalysisInfo<TAbstractValue>>();
        }

        public DataFlowAnalysisInfo<TAbstractValue> this[BasicBlock block]
        {
            get => _info[block];
        }

        internal void Add(BasicBlock block)
        {
            _info.Add(block, new DataFlowAnalysisInfo<TAbstractValue>());
        }
    }

    internal abstract class DataFlowAnalysis<TAbstractValue>
    {
        protected AbstractDomain<TAbstractValue> _domain;

        protected void Initialize(AbstractDomain<TAbstractValue> domain)
        {
            _domain = domain;
        }

        public virtual DataFlowAnalysisResult<TAbstractValue> Run(ControlFlowGraph cfg)
        {
            var result = new DataFlowAnalysisResult<TAbstractValue>();

            foreach (var block in cfg.Blocks)
            {
                result.Add(block);
            }

            var entry = Entry(cfg);
            var worklist = new HashSet<BasicBlock>();

            Output(result[entry], _domain.Bottom);
            worklist.UnionWith(Successors(entry));

            while (worklist.Count > 0)
            {
                var block = worklist.First();
                worklist.Remove(block);

                var blockResult = result[block];
                var inputs = Predecessors(block).Select(b => Output(result[b]));
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

            return result;
        }

        [DebuggerStepThrough]
        protected abstract BasicBlock Entry(ControlFlowGraph cfg);

        [DebuggerStepThrough]
        protected abstract IEnumerable<BasicBlock> Predecessors(BasicBlock block);

        [DebuggerStepThrough]
        protected abstract IEnumerable<BasicBlock> Successors(BasicBlock block);

        [DebuggerStepThrough]
        protected abstract TAbstractValue Input(DataFlowAnalysisInfo<TAbstractValue> result);

        [DebuggerStepThrough]
        protected abstract TAbstractValue Output(DataFlowAnalysisInfo<TAbstractValue> result);

        [DebuggerStepThrough]
        protected abstract void Input(DataFlowAnalysisInfo<TAbstractValue> result, TAbstractValue value);

        [DebuggerStepThrough]
        protected abstract void Output(DataFlowAnalysisInfo<TAbstractValue> result, TAbstractValue value);

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
        protected override TAbstractValue Input(DataFlowAnalysisInfo<TAbstractValue> result) => result.Input;

        [DebuggerStepThrough]
        protected override TAbstractValue Output(DataFlowAnalysisInfo<TAbstractValue> result) => result.Output;

        [DebuggerStepThrough]
        protected override void Input(DataFlowAnalysisInfo<TAbstractValue> result, TAbstractValue value) => result.Input = value;

        [DebuggerStepThrough]
        protected override void Output(DataFlowAnalysisInfo<TAbstractValue> result, TAbstractValue value) => result.Output = value;
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
        protected override TAbstractValue Input(DataFlowAnalysisInfo<TAbstractValue> result) => result.Output;

        [DebuggerStepThrough]
        protected override TAbstractValue Output(DataFlowAnalysisInfo<TAbstractValue> result) => result.Input;

        [DebuggerStepThrough]
        protected override void Input(DataFlowAnalysisInfo<TAbstractValue> result, TAbstractValue value) => result.Output = value;

        [DebuggerStepThrough]
        protected override void Output(DataFlowAnalysisInfo<TAbstractValue> result, TAbstractValue value) => result.Input = value;
    }
}
