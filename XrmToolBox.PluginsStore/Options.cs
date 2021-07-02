﻿using System;
using System.IO;
using System.Windows;
using System.Xml;
using XrmToolBox.Extensibility;

namespace XrmToolBox.PluginsStore
{
    public class Options : ICloneable
    {
        private const string OptionFileName = "XrmToolBox.PluginsStore.xml";

        private static Options instance;

        private Options()
        {
            DisplayPluginsStoreOnStartup = true;
            UseLegacy = false;
        }

        public static Options Instance
        {
            get
            {
                if (instance == null)
                {
                    string errorMessage;
                    if (!Load(out instance, out errorMessage))
                    {
                        MessageBox.Show(
                            "An error occured when loading Tool Library settings. A new settings file has been created\n\n" +
                            errorMessage);

                        return new Options();
                    }

                    return instance;
                }

                return instance;
            }
        }

        public bool? DisplayPluginsStoreOnStartup { get; set; }
        public bool IsInitialized { get; set; }
        public bool? PluginsStoreShowIncompatible { get; set; } = false;
        public bool? PluginsStoreShowInstalled { get; set; } = true;
        public bool? PluginsStoreShowNew { get; set; } = true;
        public bool? PluginsStoreShowUpdates { get; set; } = true;
        public bool? UseLegacy { get; set; }

        public object Clone()
        {
            return new Options
            {
                DisplayPluginsStoreOnStartup = DisplayPluginsStoreOnStartup,
                PluginsStoreShowIncompatible = PluginsStoreShowIncompatible,
                PluginsStoreShowInstalled = PluginsStoreShowInstalled,
                PluginsStoreShowNew = PluginsStoreShowNew,
                PluginsStoreShowUpdates = PluginsStoreShowUpdates,
                UseLegacy = UseLegacy
            };
        }

        public void Save()
        {
            if (!Directory.Exists(Paths.SettingsPath))
            {
                Directory.CreateDirectory(Paths.SettingsPath);
            }

            var settingsFile = Path.Combine(Paths.SettingsPath, OptionFileName);

            XmlSerializerHelper.SerializeToFile(this, settingsFile);
        }

        private static bool Load(out Options options, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (!Directory.Exists(Paths.SettingsPath))
            {
                Directory.CreateDirectory(Paths.SettingsPath);
            }

            var settingsFile = Path.Combine(Paths.SettingsPath, OptionFileName);

            if (File.Exists(settingsFile))
            {
                try
                {
                    var document = new XmlDocument();
                    document.Load(settingsFile);

                    options = (Options)XmlSerializerHelper.Deserialize(document.OuterXml, typeof(Options));
                    options.IsInitialized = true;
                    return true;
                }
                catch (Exception error)
                {
                    errorMessage = error.Message;
                    options = new Options();
                    return false;
                }
            }

            options = new Options();
            return true;
        }
    }
}