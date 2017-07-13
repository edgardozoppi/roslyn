using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace Microsoft.CodeAnalysis.Semantics.Dataflow
{
    internal static class DGMLSerializer
    {
        public static string Serialize(ControlFlowGraph cfg)
        {
            var settings = new XmlWriterSettings()
            {
                Indent = true,
                Encoding = Encoding.UTF8
            };

            using (var stringWriter = new StringWriter())
            using (var xmlWriter = XmlWriter.Create(stringWriter, settings))
            {
                xmlWriter.WriteStartElement("DirectedGraph", "http://schemas.microsoft.com/vs/2009/dgml");
                xmlWriter.WriteStartElement("Nodes");

                foreach (var block in cfg.Blocks)
                {
                    var nodeId = Convert.ToString(block.GetHashCode());
                    var label = DGMLSerializer.Serialize(block);

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

                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement("Links");

                foreach (var block in cfg.Blocks)
                {
                    var sourceId = Convert.ToString(block.GetHashCode());

                    foreach (var successor in block.Successors)
                    {
                        var targetId = Convert.ToString(successor.GetHashCode());

                        xmlWriter.WriteStartElement("Link");
                        xmlWriter.WriteAttributeString("Source", sourceId);
                        xmlWriter.WriteAttributeString("Target", targetId);
                        xmlWriter.WriteEndElement();
                    }
                }

                xmlWriter.WriteEndElement();
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
                xmlWriter.Flush();
                return stringWriter.ToString();
            }
        }

        private static string Serialize(BasicBlock block)
        {
            string result;

            switch (block.Kind)
            {
                case BasicBlockKind.Entry: result = "entry"; break;
                case BasicBlockKind.Exit: result = "exit"; break;
                default:
                    result = string.Join(Environment.NewLine, block.Statements);
                    result = string.Format("{0}", result);
                    break;
            }

            return result;
        }
    }
}
