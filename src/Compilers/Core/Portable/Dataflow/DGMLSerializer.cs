using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Text;
using System.Xml;

namespace Microsoft.CodeAnalysis.Semantics.Dataflow
{
    internal static class DGMLSerializer
    {
        public static string Serialize(ControlFlowGraph cfg)
        {
            using (var stringWriter = new StringWriter())
            using (var xmlWriter = GetXmlWriter(stringWriter))
            {
                xmlWriter.WriteHeader();
                xmlWriter.WriteStartElement("Nodes");

                foreach (var block in cfg.Blocks)
                {
                    xmlWriter.Serialize(block);
                }

                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement("Links");

                foreach (var block in cfg.Blocks)
                {
                    foreach (var successor in block.Successors)
                    {
                        xmlWriter.Serialize(block, successor);
                    }
                }

                xmlWriter.WriteEndElement();
                xmlWriter.WriteStyles();
                xmlWriter.Flush();
                return stringWriter.ToString();
            }
        }

        public static string Serialize<TAbstractValue>(ControlFlowGraph cfg, DataFlowAnalysisResult<TAbstractValue> result, IFormatProvider abstractValueFormatProvider)
        {
            using (var stringWriter = new StringWriter())
            using (var xmlWriter = GetXmlWriter(stringWriter))
            {
                xmlWriter.WriteHeader();
                xmlWriter.WriteStartElement("Nodes");

                foreach (var block in cfg.Blocks)
                {
                    var info = result[block];
                    xmlWriter.Serialize(block, info, abstractValueFormatProvider);
                }

                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement("Links");

                foreach (var block in cfg.Blocks)
                {
                    xmlWriter.SerializeWithInfo(block);

                    foreach (var successor in block.Successors)
                    {
                        xmlWriter.SerializeWithInfo(block, successor);
                    }
                }

                xmlWriter.WriteEndElement();
                xmlWriter.WriteStyles();
                xmlWriter.Flush();
                return stringWriter.ToString();
            }
        }

        private static void Serialize(this XmlWriter xmlWriter, BasicBlock block)
        {
            var nodeId = Convert.ToString(block.GetHashCode());
            var label = GetLabel(block);

            xmlWriter.WriteStartElement("Node");
            xmlWriter.WriteAttributeString("Id", nodeId);
            xmlWriter.WriteAttributeString("Label", label);

            if (block.Kind == BasicBlockKind.Entry ||
                block.Kind == BasicBlockKind.Exit)
            {
                xmlWriter.WriteAttributeString("Background", "Yellow");
            }

            xmlWriter.WriteEndElement();
        }

        private static void Serialize<TAbstractValue>(this XmlWriter xmlWriter, BasicBlock block, DataFlowAnalysisInfo<TAbstractValue> info, IFormatProvider abstractValueFormatProvider)
        {
            string label;
            var nodeId = Convert.ToString(block.GetHashCode());

            if (block.Kind != BasicBlockKind.Entry &&
                block.Predecessors.Count > 1)
            {
                label = string.Format(abstractValueFormatProvider, "{0}", info.Input);

                xmlWriter.WriteStartElement("Node");
                xmlWriter.WriteAttributeString("Id", $"{nodeId}_in");
                xmlWriter.WriteAttributeString("Label", label);
                xmlWriter.WriteAttributeString("Shape", "None");
                xmlWriter.WriteEndElement();
            }

            xmlWriter.Serialize(block);

            if (block.Kind != BasicBlockKind.Exit)
            {
                label = string.Format(abstractValueFormatProvider, "{0}", info.Output);

                xmlWriter.WriteStartElement("Node");
                xmlWriter.WriteAttributeString("Id", $"{nodeId}_out");
                xmlWriter.WriteAttributeString("Label", label);
                xmlWriter.WriteAttributeString("Shape", "None");
                xmlWriter.WriteEndElement();
            }
        }

        private static void Serialize(this XmlWriter xmlWriter, BasicBlock source, BasicBlock target)
        {
            var sourceId = Convert.ToString(source.GetHashCode());
            var targetId = Convert.ToString(target.GetHashCode());

            xmlWriter.WriteStartElement("Link");
            xmlWriter.WriteAttributeString("Source", sourceId);
            xmlWriter.WriteAttributeString("Target", targetId);
            xmlWriter.WriteEndElement();
        }

        private static void SerializeWithInfo(this XmlWriter xmlWriter, BasicBlock block)
        {
            var blockId = Convert.ToString(block.GetHashCode());

            if (block.Predecessors.Count > 1)
            {
                xmlWriter.WriteStartElement("Link");
                xmlWriter.WriteAttributeString("Source", $"{blockId}_in");
                xmlWriter.WriteAttributeString("Target", blockId);
                xmlWriter.WriteEndElement();
            }

            if (block.Kind != BasicBlockKind.Exit)
            {
                xmlWriter.WriteStartElement("Link");
                xmlWriter.WriteAttributeString("Source", blockId);
                xmlWriter.WriteAttributeString("Target", $"{blockId}_out");
                xmlWriter.WriteEndElement();
            }
        }

        private static void SerializeWithInfo(this XmlWriter xmlWriter, BasicBlock source, BasicBlock target)
        {
            var sourceId = Convert.ToString(source.GetHashCode());
            var targetId = Convert.ToString(target.GetHashCode());

            if (target.Predecessors.Count > 1)
            {
                targetId = $"{targetId}_in";
            }

            xmlWriter.WriteStartElement("Link");
            xmlWriter.WriteAttributeString("Source", $"{sourceId}_out");
            xmlWriter.WriteAttributeString("Target", targetId);
            xmlWriter.WriteEndElement();
        }

        private static string GetLabel(BasicBlock block)
        {
            string result;

            switch (block.Kind)
            {
                case BasicBlockKind.Entry: result = "entry"; break;
                case BasicBlockKind.Exit: result = "exit"; break;
                default:
                    //result = string.Join(Environment.NewLine, block.Statements);
                    result = SerializeStatements(block.Statements);
                    break;
            }

            return result;
        }

        private static string SerializeStatements(ImmutableArray<IOperation> statements)
        {
            var builder = new StringBuilder();

            foreach (var statement in statements)
            {
                var text = OperationTreeSerializer.Serialize(statement);
                builder.AppendLine(text);
            }

            return builder.ToString().Trim();
        }

        #region Helper methods

        private static XmlWriter GetXmlWriter(TextWriter textWriter)
        {
            var settings = new XmlWriterSettings()
            {
                Indent = true,
                Encoding = Encoding.UTF8
            };

            var xmlWriter = XmlWriter.Create(textWriter, settings);
            return xmlWriter;
        }

        private static void WriteHeader(this XmlWriter xmlWriter)
        {
            xmlWriter.WriteStartElement("DirectedGraph", "http://schemas.microsoft.com/vs/2009/dgml");
        }

        private static void WriteStyles(this XmlWriter xmlWriter)
        {
            xmlWriter.WriteStartElement("Styles");
            xmlWriter.WriteStartElement("Style");
            xmlWriter.WriteAttributeString("TargetType", "Node");

            xmlWriter.WriteStartElement("Setter");
            xmlWriter.WriteAttributeString("Property", "FontFamily");
            xmlWriter.WriteAttributeString("Value", "Consolas");
            xmlWriter.WriteEndElement();

            xmlWriter.WriteStartElement("Setter");
            xmlWriter.WriteAttributeString("Property", "NodeRadius");
            xmlWriter.WriteAttributeString("Value", "5");
            xmlWriter.WriteEndElement();

            xmlWriter.WriteStartElement("Setter");
            xmlWriter.WriteAttributeString("Property", "MinWidth");
            xmlWriter.WriteAttributeString("Value", "0");
            xmlWriter.WriteEndElement();

            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndElement();
        }

        #endregion
    }
}
