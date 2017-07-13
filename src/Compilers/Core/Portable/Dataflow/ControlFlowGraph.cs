using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;

namespace Microsoft.CodeAnalysis.Semantics.Dataflow
{
    internal enum BasicBlockKind
    {
        Entry,
        Exit,
        Block
    }

    [DebuggerDisplay("{Kind} ({Statements.Length} statements)")]
    internal class BasicBlock
    {
        private BasicBlockKind _kind;
        private ImmutableArray<IOperation> _statements;
        private ImmutableHashSet<BasicBlock> _successors;
        private ImmutableHashSet<BasicBlock> _predecessors;

        public BasicBlock(BasicBlockKind kind)
        {
            _kind = kind;
            _statements = ImmutableArray<IOperation>.Empty;
            _successors = ImmutableHashSet<BasicBlock>.Empty;
            _predecessors = ImmutableHashSet<BasicBlock>.Empty;
        }

        public BasicBlockKind Kind => _kind;
        public ImmutableArray<IOperation> Statements => _statements;
        public ImmutableHashSet<BasicBlock> Successors => _successors;
        public ImmutableHashSet<BasicBlock> Predecessors => _predecessors;

        internal void AddStatement(IOperation statement)
        {
            _statements = _statements.Add(statement);
        }

        internal void AddSuccessor(BasicBlock block)
        {
            _successors = _successors.Add(block);
        }

        internal void AddPredecessor(BasicBlock block)
        {
            _predecessors = _predecessors.Add(block);
        }
    }

    [DebuggerDisplay("CFG ({_blocks.Count} blocks)")]
    internal class ControlFlowGraph
    {
        private BasicBlock _entry;
        private BasicBlock _exit;
        private ImmutableHashSet<BasicBlock> _blocks;

        public ControlFlowGraph()
        {
            _blocks = ImmutableHashSet<BasicBlock>.Empty;
            _entry = new BasicBlock(BasicBlockKind.Entry);
            _exit = new BasicBlock(BasicBlockKind.Exit);

            AddBlock(_entry);
            AddBlock(_exit);
        }

        public BasicBlock Entry => _entry;
        public BasicBlock Exit => _exit;
        public ImmutableHashSet<BasicBlock> Blocks => _blocks;

        internal void AddBlock(BasicBlock block)
        {
            _blocks = _blocks.Add(block);
        }

        internal void ConnectBlocks(BasicBlock from, BasicBlock to)
        {
            from.AddSuccessor(to);
            to.AddPredecessor(from);
            _blocks = _blocks.Add(from).Add(to);
        }
    }
}
