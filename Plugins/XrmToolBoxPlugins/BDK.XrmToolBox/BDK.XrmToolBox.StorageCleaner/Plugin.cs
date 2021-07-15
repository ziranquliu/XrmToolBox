﻿/*
MIT License

Copyright (c) 2019 Tech Quantum

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
 */

namespace BDK.XrmToolBox.StorageCleaner
{
    using global::XrmToolBox.Extensibility;
    using global::XrmToolBox.Extensibility.Interfaces;
    using System.ComponentModel.Composition;

    /// <summary>
    /// Plugin main class to load the control
    /// </summary>
    /// <seealso cref="XrmToolBox.Extensibility.PluginBase" />
    [Export(typeof(IXrmToolBoxPlugin)),
     ExportMetadata("BackgroundColor", "MediumBlue"),
     ExportMetadata("PrimaryFontColor", "White"),
     ExportMetadata("SecondaryFontColor", "LightGray"),
     ExportMetadata("SmallImageBase64", "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"),
     ExportMetadata("BigImageBase64", "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"),
    ExportMetadata("Name", "Reduce Storage Space Usage"),
    ExportMetadata("Description", "Reduce the amount of storage space used by removing or deleting different types of information from Microsoft Dynamics CRM.")]
    public class Plugin : PluginBase
    {
        /// <summary>
        /// Gets the control.
        /// </summary>
        /// <returns></returns>
        public override IXrmToolBoxPluginControl GetControl()
        {
            return new PluginControl();
        }
    }
}