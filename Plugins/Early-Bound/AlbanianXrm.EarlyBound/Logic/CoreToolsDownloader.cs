﻿using AlbanianXrm.EarlyBound.Helpers;
using AlbanianXrm.EarlyBound.Properties;
using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using XrmToolBox.Extensibility;

namespace AlbanianXrm.EarlyBound.Logic
{
    internal class CoreToolsDownloader
    {
        private readonly MyPluginControl myPlugin;

        public CoreToolsDownloader(MyPluginControl myPlugin)
        {
            this.myPlugin = myPlugin;
        }

        public void DownloadCoreTools()
        {
            myPlugin.StartWorkAsync(new WorkAsyncInfo
            {
                Message = Resources.DOWNLOADING_CORE_TOOLS,
                Work = (worker, args) =>
                {
                    //ID of the package to be looked 
                    string coreToolsId = "Microsoft.CrmSdk.CoreTools";

                    //Connect to the official package repository IPackageRepository
                    var repo = NuGet.PackageRepositoryFactory.Default.CreateRepository(myPlugin.options.NuGetFeed);

                    string dir = Path.GetDirectoryName(typeof(MyPluginControl).Assembly.Location).ToUpperInvariant();
                    string folder = Path.GetFileNameWithoutExtension(typeof(MyPluginControl).Assembly.Location);
                    dir = Path.Combine(dir, folder);
                    Directory.CreateDirectory(dir);

                    var coreToolsPackage = repo.GetPackages().Where(x => x.Id == coreToolsId && x.IsLatestVersion)
                                          .OrderByDescending(x => x.Version).FirstOrDefault();
                    if (coreToolsPackage == null)
                    {
                        args.Result = string.Format(Resources.Culture, Resources.CORE_TOOLS_NOT_FOUND, coreToolsId, myPlugin.options.NuGetFeed);
                        return;
                    }
                    foreach (var file in coreToolsPackage.GetFiles())
                    {
                        using (var stream = File.Create(Path.Combine(dir, Path.GetFileName(file.Path))))
                            file.GetStream().CopyTo(stream);
                    }
                },
                PostWorkCallBack = (args) =>
                {
                    try
                    {
                        if (args.Error != null)
                        {
                            MessageBox.Show(args.Error.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        if (args.Result != null)
                        {
                            MessageBox.Show(args.Result.ToString(), "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        myPlugin.options.CrmSvcUtils = new CrmSvcUtilsEditor().GetVersion(myPlugin.options.CrmSvcUtils);
                        myPlugin.options.RecycableMemoryStream = new MemoryStreamEditor().GetVersion(myPlugin.options.RecycableMemoryStream);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    finally
                    {
                        myPlugin.WorkAsyncEnded();
                    }
                }
            });
        }
    }
}
