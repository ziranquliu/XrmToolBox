using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using XrmToolBox.Extensibility;
using XrmToolBox.Extensibility.Interfaces;

namespace AuditHistoryRollBack
{
    // Do not forget to update version number and author (company attribute) in AssemblyInfo.cs class
    // To generate Base64 string for Images below, you can use https://www.base64-image.de/
    [Export(typeof(IXrmToolBoxPlugin)),
        ExportMetadata("Name", "Audit History Rollback"),
        ExportMetadata("Description", "Perform rollback transactions of audit history"),
        // Please specify the base64 content of a 32x32 pixels image
        ExportMetadata("SmallImageBase64", "iVBORw0KGgoAAAANSUhEUgAAAB4AAAAeCAMAAAAM7l6QAAAABGdBTUEAALGPC/xhBQAAACBjSFJNAAB6JgAAgIQAAPoAAACA6AAAdTAAAOpgAAA6mAAAF3CculE8AAABxVBMVEUAAADyw0ryxkryx0ryxUryyEonM0zytkryvkryu0ryxErzqUvyuUryz0rHlUvyv0r//0ryt0rzrkvzp0vzuUvzpkvzqEtbTkz/pUvznksRFiDyvErywEryxEryxUonM0wnM0wnM0wnM0wnM0wnM0wnM0zyr0vytUryv0ryv0onM0wnM0wnM0wnM0wnM0wnM0z4rEvzq0vyt0onM0wnM0wkMUy7g0v0o0vzq0vzr0snM0wnM0wnM0wnM0wnM0wnM0wnM0wnM0wmMkzekUv0nUvzoEvzo0vzqUsnM0wnM0wnM0wnM0wnM0wnM0wnM0wnM0wjMUz/pkv0m0snM0wnM0wnM0wnM0wnM0wnM0wkMUwnM0wnM0wnM0wnM0wnM0wnM0wnM0wnM0wnM0wnM0wnM0wnM0wnM0wnM0wnM0wnM0wnM0wnM0wnM0wnM0wnM0wnM0wnM0wnM0wnM0wnM0wnM0wnM0wnM0wnM0wnM0wnM0wnM0wnM0wnM0wnM0wnM0wnM0wnM0wnM0wnM0wnM0wnM0wnM0wnM0wnM0wnM0wnM0wnM0wnM0wnM0wnM0zyuUryvUonM0zzrkvyskrytUrzpEvzqEv///80tzSoAAAAjnRSTlMAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAhfYk1TYeFfF0tBg+21ycBpP7pqD8u71qj7Xol0/A8ovnPx8nX8/2NUbzIaATkCw4YQJ31+2wJDeMhBGbx4CmChB/ayA8CpecnivA0lC8Mwdsb9KwFJcr6WOIeB0W1735qbIDZMtI+7pwmht7UuzkNGhUIXg6TTQAAAAFiS0dElpFpKzkAAAAHdElNRQfkBBURFR2aEbHTAAAAAW9yTlQBz6J3mgAAATRJREFUKM9jYKAzYGRiYmZhRRJggwMgh52Dk5ODi4ULiywIcPNIy8jK8fAiScsrKIKAkrKKKhufmnpfv4YmP0JaS3sCBOjo6ukLGBhOnDTZSBBJ2ngCHJiYmplPmWphKYQkbTVhgrWNrZ29A1De0cnZxdXNXRhV2kPF08vbx9dvwgT/gMAgEVExVOngEJCzQ8PCJ0yIiBSXkGTAkAb7MApofjQ4ADCkQX6MiZ0wIS4eh7QUW0LihAlJyTikgdpTJkxITcMtnQ70ewZOuzOzJkzIzsEuDXS5fO6ECXn52KRBoKBwwoSiYkyPlZQCJcvKbYHervDClK6sqq6prQCG2YS6ehRZiDQcNDSyoUtrwyWbmlvQZIHubW3LBoH2DsXOUHRZoHRXNxj09MLSI6o0MiAx2ZMPAM4UdQeQVC/fAAAAJXRFWHRkYXRlOmNyZWF0ZQAyMDIwLTA0LTIxVDE3OjIxOjI5KzAwOjAwH88dOQAAACV0RVh0ZGF0ZTptb2RpZnkAMjAyMC0wNC0yMVQxNzoyMToyOSswMDowMG6SpYUAAAAASUVORK5CYII="),
        // Please specify the base64 content of a 80x80 pixels image
        ExportMetadata("BigImageBase64", "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"),
        ExportMetadata("BackgroundColor", "White"),
        ExportMetadata("PrimaryFontColor", "Black"),
        ExportMetadata("SecondaryFontColor", "Black")]
    public class MyPlugin : PluginBase
    {
        public override IXrmToolBoxPluginControl GetControl()
        {
            return new MyPluginControl();
        }

        /// <summary>
        /// Constructor 
        /// </summary>
        public MyPlugin()
        {
            // If you have external assemblies that you need to load, uncomment the following to 
            // hook into the event that will fire when an Assembly fails to resolve
            // AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(AssemblyResolveEventHandler);
        }

        /// <summary>
        /// Event fired by CLR when an assembly reference fails to load
        /// Assumes that related assemblies will be loaded from a subfolder named the same as the Plugin
        /// For example, a folder named Sample.XrmToolBox.MyPlugin 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        private Assembly AssemblyResolveEventHandler(object sender, ResolveEventArgs args)
        {
            Assembly loadAssembly = null;
            Assembly currAssembly = Assembly.GetExecutingAssembly();

            // base name of the assembly that failed to resolve
            var argName = args.Name.Substring(0, args.Name.IndexOf(","));

            // check to see if the failing assembly is one that we reference.
            List<AssemblyName> refAssemblies = currAssembly.GetReferencedAssemblies().ToList();
            var refAssembly = refAssemblies.Where(a => a.Name == argName).FirstOrDefault();

            // if the current unresolved assembly is referenced by our plugin, attempt to load
            if (refAssembly != null)
            {
                // load from the path to this plugin assembly, not host executable
                string dir = Path.GetDirectoryName(currAssembly.Location).ToLower();
                string folder = Path.GetFileNameWithoutExtension(currAssembly.Location);
                dir = Path.Combine(dir, folder);

                var assmbPath = Path.Combine(dir, $"{argName}.dll");

                if (File.Exists(assmbPath))
                {
                    loadAssembly = Assembly.LoadFrom(assmbPath);
                }
                else
                {
                    throw new FileNotFoundException($"Unable to locate dependency: {assmbPath}");
                }
            }

            return loadAssembly;
        }
    }
}