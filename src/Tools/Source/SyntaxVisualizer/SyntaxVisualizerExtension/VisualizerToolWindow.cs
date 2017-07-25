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
    public abstract class VisualizerToolWindow : ToolWindowPane
    {
        /// <summary>
        /// Standard constructor for the tool window.
        /// </summary>
        protected VisualizerToolWindow() :
            base(null)
        {
            // Set the image that will appear on the tab of the window frame
            // when docked with an other window
            // The resource ID correspond to the one defined in the resx file
            // while the Index is the offset in the bitmap strip. Each image in
            // the strip being 16x16.
            this.BitmapResourceID = 500;
            this.BitmapIndex = 0;
        }

        internal TServiceInterface GetVsService<TServiceInterface, TService>() 
            where TServiceInterface : class
            where TService : class
        {
            return (TServiceInterface)GetService(typeof(TService));
        }
    }
}
