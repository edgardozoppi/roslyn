﻿using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using Roslyn.SyntaxVisualizer.Control;

namespace Roslyn.SyntaxVisualizer.Extension
{
    /// <summary>
    /// This class implements the tool window exposed by this package and hosts a user control.
    ///
    /// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane, 
    /// usually implemented by the package implementer.
    ///
    /// This class derives from the ToolWindowPane class provided from the MPF in order to use its 
    /// implementation of the IVsUIElementPane interface.
    /// </summary>
    [Guid("da7e21aa-da94-452d-8aa1-d1b23f73f576")]
    public class SyntaxVisualizerToolWindow : VisualizerToolWindow
    {
        /// <summary>
        /// Standard constructor for the tool window.
        /// </summary>
        public SyntaxVisualizerToolWindow()
        {
            // Set the window title reading it from the resources.
            this.Caption = Resources.SyntaxVisualizerToolWindowTitle;

            // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
            // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on 
            // the object returned by the Content property.
            this.Content = new VisualizerContainer(this, new SyntaxVisualizerControl());
        }
    }
}
