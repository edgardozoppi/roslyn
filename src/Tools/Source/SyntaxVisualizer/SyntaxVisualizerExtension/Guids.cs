﻿using System;

namespace Roslyn.SyntaxVisualizer.Extension
{
    internal static class GuidList
    {
        internal const string GuidSyntaxVisualizerExtensionPkgString = "1e4ce699-d626-42c3-9803-cac3cd73ed77";
        internal const string GuidSyntaxVisualizerExtensionCmdSetString = "8443a724-465d-4706-a46c-3f7bc8eb7588";
        internal const string GuidSyntaxVisualizerToolWindowPersistanceString = "da7e21aa-da94-452d-8aa1-d1b23f73f576";
        internal const string GuidIOperationVisualizerExtensionCmdSetString = "c2543a55-d70d-41df-94fc-c6d0efca1185";
        internal const string GuidIOperationVisualizerToolWindowPersistanceString = "0fb98e70-8f51-419a-88cf-f7eeffb64537";

        internal static readonly Guid GuidSyntaxVisualizerExtensionCmdSet = new Guid(GuidSyntaxVisualizerExtensionCmdSetString);
        internal static readonly Guid GuidIOperationVisualizerExtensionCmdSet = new Guid(GuidIOperationVisualizerExtensionCmdSetString);
        internal static readonly Guid GuidProgressionPkg = new Guid("AD1A73B0-C489-4C9C-B1FE-EEA54CD19A4F");
        internal static readonly Guid GuidVsDesignerViewKind = new Guid(EnvDTE.Constants.vsViewKindDesigner);
    }
}
