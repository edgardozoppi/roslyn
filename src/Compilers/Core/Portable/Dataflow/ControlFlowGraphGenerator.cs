using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.CodeAnalysis.Semantics.Dataflow
{
    internal class ControlFlowGraphGenerator
    {
        #region StatementCollector

        private sealed class StatementCollector : OperationWalker
        {
            private readonly List<IOperation> _list;

            public StatementCollector(List<IOperation> list)
            {
                _list = list;
            }

            public override void Visit(IOperation operation)
            {
                if (operation != null)
                {
                    var isStatement = operation.IsStatement();
                    var isBlockStatement = operation.Kind == OperationKind.BlockStatement;

                    if (isStatement && !isBlockStatement)
                    {
                        _list.Add(operation);
                    }
                }

                base.Visit(operation);
            }
        }

        #endregion

        private IMethodSymbol _method;
        private IList<BasicBlock> _blocks;
        private IDictionary<ILabelSymbol, BasicBlock> _labeledBlocks;
        private BasicBlock _currentBlock;
        private ControlFlowGraph _graph;

        private ControlFlowGraphGenerator(IMethodSymbol method)
        {
            _method = method;
            _blocks = new List<BasicBlock>();
            _labeledBlocks = new Dictionary<ILabelSymbol, BasicBlock>();
            _graph = new ControlFlowGraph();
        }

        public ControlFlowGraph Result => _graph;

        public static ControlFlowGraph Generate(IMethodSymbol method, IOperation body)
        {
            var generator = new ControlFlowGraphGenerator(method);
            generator.CreateBlocks(body);
            generator.ConnectBlocks();
            return generator.Result;
        }

        private void CreateBlocks(IOperation body)
        {
            var statements = new List<IOperation>();
            var collector = new StatementCollector(statements);

            collector.Visit(body);

            foreach (var statement in statements)
            {
                Visit(statement);
            }
        }

        private void Visit(IOperation statement)
        {
            var isLastStatement = false;

            switch (statement.Kind)
            {
                case OperationKind.LabelStatement:
                    var label = (ILabelStatement)statement;
                    _currentBlock = NewBlock();
                    _labeledBlocks.Add(label.Label, _currentBlock);
                    break;

                case OperationKind.ThrowStatement:
                case OperationKind.ReturnStatement:
                case OperationKind.BranchStatement:
                case OperationKind.ConditionalGotoStatement:
                    isLastStatement = true;
                    break;
            }

            if (_currentBlock == null)
            {
                _currentBlock = NewBlock();
            }

            _currentBlock.AddStatement(statement);

            if (isLastStatement)
            {
                _currentBlock = null;
            }
        }

        private BasicBlock NewBlock()
        {
            var block = new BasicBlock(BasicBlockKind.Block);

            _blocks.Add(block);
            _graph.AddBlock(block);
            return block;
        }

        private void ConnectBlocks()
        {
            if (_blocks.Any())
            {
                var connectWithPrev = true;
                var prevBlock = _graph.Entry;

                foreach (var block in _blocks)
                {
                    if (connectWithPrev)
                    {
                        _graph.ConnectBlocks(prevBlock, block);
                    }
                    else
                    {
                        connectWithPrev = true;
                    }

                    BasicBlock target = null;
                    var lastStatement = block.Statements.LastOrDefault();

                    switch (lastStatement)
                    {
                        case IConditionalGotoStatement branch:
                            target = _labeledBlocks[branch.Target];
                            _graph.ConnectBlocks(block, target);
                            break;

                        case IBranchStatement branch:
                            target = _labeledBlocks[branch.Target];
                            _graph.ConnectBlocks(block, target);
                            connectWithPrev = false;
                            break;

                        case IReturnStatement ret:
                        case IThrowStatement thrw:
                            _graph.ConnectBlocks(block, _graph.Exit);
                            connectWithPrev = false;
                            break;
                    }

                    prevBlock = block;
                }
            }
            else
            {
                _graph.ConnectBlocks(_graph.Entry, _graph.Exit);
            }
        }
    }
}
