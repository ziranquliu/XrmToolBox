﻿using McTools.Xrm.Connection;
using McTools.Xrm.Connection.WinForms;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using XrmToolBox.Announcement;
using XrmToolBox.AppCode;
using XrmToolBox.Controls;
using XrmToolBox.Extensibility;
using XrmToolBox.Extensibility.Args;
using XrmToolBox.Extensibility.Interfaces;
using XrmToolBox.Forms;
using XrmToolBox.New.EventArgs;
using XrmToolBox.PluginsStore;
using XrmToolBox.PluginsStore.DTO;

namespace XrmToolBox.New
{
    public partial class NewForm : Form
    {
        private const string AiEndpoint = "https://dc.services.visualstudio.com/v2/track";
        private const string AiKey = "77a2080e-f82c-4b2f-bb77-eb407236b729";
        private readonly Dictionary<PluginForm, ConnectionDetail> pluginConnections = new Dictionary<PluginForm, ConnectionDetail>();
        private readonly List<PluginControlStatus> pluginControlStatuses = new List<PluginControlStatus>();
        private readonly PluginsForm pluginsForm;
        private AppInsights ai = new AppInsights(new AiConfig(AiEndpoint, AiKey));
        private CrmConnectionStatusBar ccsb;
        private ConnectionManager cManager;
        private ConnectionDetail connectionDetail;
        private IDockContent currentContent;
        private FormHelper fHelper;
        private string initialConnectionName;
        private string initialPluginName;
        private int numberOfConnectionReceived;
        private IOrganizationService service;
        private StartPage startPage;
        private StoreFromPortal store;

        public NewForm(string[] args)
        {
            InitializeComponent();

            ai.WriteEvent("XrmToolBox-Start");

            Text = $@"{Text} (v{Assembly.GetExecutingAssembly().GetName().Version})";

            // Set drawing optimizations
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);

            WelcomeDialog.ShowSplashScreen();

            SetTheme();
            dpMain.Theme.Extender.FloatWindowFactory = new CustomFloatWindowFactory();

            // Connection Management
            WelcomeDialog.SetStatus("Loading connection controls...");
            ManageConnectionControl();
            ccsb.MergeConnectionsFiles = Options.Instance.MergeConnectionFiles;

            WelcomeDialog.SetStatus("Loading tools...");
            try
            {
                pluginsForm = new PluginsForm();
            }
            catch
            {
                // do nothing
            }

            if (pluginsForm != null)
            {
                pluginsForm.OpenPluginRequested += PluginsForm_OpenPluginRequested;
                pluginsForm.OpenPluginProjectUrlRequested += PluginsForm_OpenPluginProjectUrlRequested;
                pluginsForm.UninstallPluginRequested += PluginsForm_UninstallPluginRequested;
                pluginsForm.ActionRequested += PluginsForm_ActionRequested;
                if (!Options.Instance.DoNotShowStartPage)
                {
                    WelcomeDialog.SetStatus("Preparing Start page...");

                    ShowStartPage();
                }

                pluginsForm.Show(dpMain, Options.Instance.PluginsListDocking);
                pluginsForm.IsHidden = Options.Instance.PluginsListIsHidden;

                ProcessMenuItemsForPlugin2();

                // Restore session management
                if (Options.Instance.RememberSession)
                {
                    if (!string.IsNullOrEmpty(Options.Instance.LastConnection))
                    {
                        initialConnectionName = Options.Instance.LastConnection;
                    }

                    if (!string.IsNullOrEmpty(Options.Instance.LastPlugin))
                    {
                        initialPluginName = Options.Instance.LastPlugin;
                    }
                }

                // Read arguments to detect if a plugin should be displayed automatically
                if (args.Length > 0)
                {
                    initialConnectionName = ExtractSwitchValue("/connection:", ref args);
                    initialPluginName = ExtractSwitchValue("/plugin:", ref args);

                    if (!string.IsNullOrEmpty(initialConnectionName))
                    {
                        pnlConnectLoading.BringToFront();

                        pnlConnectLoading.Visible = true;
                        lblConnecting.Text = string.Format(lblConnecting.Tag.ToString(), initialConnectionName);
                    }
                }
            }
        }

        public sealed override string Text
        {
            get => base.Text;
            set => base.Text = value;
        }

        private void CheckForEarlyBoundEntities()
        {
            var assemblies = (from a in AppDomain.CurrentDomain.GetAssemblies()
                              where a.CustomAttributes.Any(ca => ca.AttributeType == typeof(ProxyTypesAssemblyAttribute))
                                    && !a.GetName().Name.StartsWith("Microsoft.")
                              select a).ToList();

            if (assemblies.Any())
            {
                string message = $@"The following files are not respecting XrmToolBox development best practices:

{string.Join(Environment.NewLine, assemblies.Select(a => "- " + a.GetName().Name))}

They can impact the behavior of XrmToolBox or its tools.

We recommend that you remove the corresponding files from XrmToolBox Plugins folder";

                MessageBox.Show(this, message, @"Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        #region Form management

        private void BringToTop()
        {
            //Checks if the method is called from UI thread or not
            if (InvokeRequired)
            {
                Invoke(new Action(BringToTop));
            }
            else
            {
                if (WindowState == FormWindowState.Minimized)
                {
                    WindowState = FormWindowState.Normal;
                }
                //Keeps the current topmost status of form
                bool top = TopMost;
                //Brings the form to top
                TopMost = true;
                //Set form's topmost status back to whatever it was
                TopMost = top;
            }
        }

        private void CheckForConnectionControlsUpdate(bool force = false)
        {
            if (store == null)
                store = new StoreFromPortal(Options.Instance.ConnectionControlsAllowPreReleaseUpdates);

            if (store.IsConnectionControlsUpdateAvailable(out string version, out string releasenotes,
                Options.Instance.ConnectionControlsVersion))
            {
                Options.Instance.ConnectionControlsVersion = version;
                Options.Instance.Save();

                if (force || !Options.Instance.PluginsUpdateSkip.Any(
                    x => x.Name == "McTools.Xrm.ConnectionControls"
                         && x.Version == version
                         && x.Date > DateTime.Now))
                {
                    var dialog = new NewConnectionVersion(version, releasenotes);
                    if (dialog.ShowDialog(this) == DialogResult.Yes)
                    {
                        var restartNow = store.PrepareConnectionControlsUpdate(this, dialog.OnNextRestart,
                            out string versionToStore);

                        Options.Instance.ConnectionControlsVersion = versionToStore;
                        Options.Instance.Save();
                        if (restartNow)
                        {
                            Application.Restart();
                        }
                    }
                    else
                    {
                        UpdatePluginUpdateSkip("McTools.Xrm.ConnectionControls", version, dialog.IsVersionSkipped,
                            dialog.NumberOfDaysToSkip);
                    }
                }
            }
        }

        private string ExtractSwitchValue(string key, ref string[] args)
        {
            var name = string.Empty;

            foreach (var arg in args)
            {
                if (arg.StartsWith(key))
                {
                    name = arg.Substring(key.Length);
                }
            }

            return name;
        }

        private void NewForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            var pci = new PluginCloseInfo(ToolBoxCloseReason.CloseAll);
            RequestCloseTabs(dpMain.Contents.OfType<PluginForm>(), pci);
            e.Cancel = pci.Cancel;

            AnnouncementSettings.Instance.LastShownDate = DateTime.Now;
            AnnouncementSettings.Instance.Save();

            Options.Instance.Size.Height = Height;
            Options.Instance.Size.CurrentSize = Size;
            Options.Instance.Size.IsMaximized = WindowState == FormWindowState.Maximized;
            Options.Instance.LastConnection = connectionDetail?.ConnectionName;
            Options.Instance.PluginsListDocking = pluginsForm?.DockState ?? DockState.Document;
            Options.Instance.PluginsListIsHidden = pluginsForm?.IsHidden ?? false;

            if (dpMain.ActiveContent is PluginForm pf)
            {
                Options.Instance.LastPlugin = pf.PluginTitle;
            }
            else
            {
                Options.Instance.LastPlugin = "";
            }

            Options.Instance.Save();
        }

        private async void NewForm_Load(object sender, System.EventArgs e)
        {
            if (!Options.Instance.DoNotShowStartPage && startPage != null)
            {
                startPage.EnsureVisible(dpMain, DockState.Document);
            }

            WebProxyHelper.ApplyProxy();

            var tasks = new List<Task>
            {
                LaunchVersionCheck()
            };

            if (!string.IsNullOrEmpty(initialConnectionName))
            {
                var initialConnectionDetail = ConnectionManager.Instance.ConnectionsList.Connections.FirstOrDefault(x => x.ConnectionName == initialConnectionName);

                if (initialConnectionDetail != null)
                {
                    // If initiall connection is present, connect to given sever is initiated.
                    // After connection try to open intial plugin will be attempted.
                    tasks.Add(LaunchInitialConnection(initialConnectionDetail));
                }
                else
                {
                    // Connection detail was not found, so name provided was incorrect.
                    // But if name of the plugin is set, it should be started
                    if (!string.IsNullOrEmpty(initialPluginName))
                    {
                        StartPluginWithoutConnection();
                    }
                }
            }
            else if (!string.IsNullOrEmpty(initialPluginName))
            {
                // If there is no initial connection, but initial plugin is set, openning plugin
                StartPluginWithoutConnection();
            }

            tasks.ForEach(x => x.Start());
            await Task.WhenAll(tasks.ToArray());

            // Adapt size of current form
            if (Options.Instance.Size.IsMaximized)
            {
                Invoke(new Action(() =>
                {
                    WindowState = FormWindowState.Maximized;
                }));
            }
            else
            {
                Options.Instance.Size.ApplyFormSize(this);
            }

            // Hide & remove Welcome screen
            WelcomeDialog.CloseForm();
            Invoke(new Action(() =>
            {
                Opacity = 100;
                BringToTop();

                CheckForEarlyBoundEntities();
            }));

            if (store == null)
            {
                try
                {
                    CheckForConnectionControlsUpdate();

                    if (store.PluginsCount == 0)
                    {
                        store.LoadNugetPackages(false);
                    }

                    if (Options.Instance.ShowPluginUpdatesPanelAtStartup)
                    {
                        var count = store.XrmToolBoxPlugins.Plugins
                            .Where(p => p.Action == PackageInstallAction.Update)
                            .ToList().Count;

                        pnlPluginsUpdate.Visible = count > 0;
                        if (count > 0)
                        {
                            lblPluginsUpdateAvailable.Text = string.Format(lblPluginsUpdateAvailable.Tag.ToString(),
                                count,
                                count > 1 ? "s" : "",
                                count > 1 ? "are" : "is");
                        }
                    }

                    if (Options.Instance.DisplayPluginsStoreOnStartup)
                    {
                        if (Options.Instance.DisplayPluginsStoreOnlyIfUpdates)
                        {
                            if (store.PluginsCount == 0)
                            {
                                store.LoadNugetPackages(false);
                            }

                            if (store.HasUpdates)
                            {
                                tsddbTools_DropDownItemClicked(sender,
                                    new ToolStripItemClickedEventArgs(pluginsStoreToolStripMenuItem));
                            }
                        }
                        else
                        {
                            tsddbTools_DropDownItemClicked(sender,
                                new ToolStripItemClickedEventArgs(pluginsStoreToolStripMenuItem));
                        }
                    }

                    // Prepare Categories
                    PrepareCategories();
                }
                catch
                {
                    // We avoid to make XrmToolBox crash if querying web services fail
                    pluginsForm.DisplayCategories(null);
                }
            }
        }

        private void PrepareCategories()
        {
            if (store?.XrmToolBoxPlugins == null) return;

            var dico = new Dictionary<string, List<string>>();
            foreach (var plugin in store.XrmToolBoxPlugins.Plugins)
            {
                if (plugin.CategoriesList == null) continue;

                foreach (var category in plugin.CategoriesList.Split(','))
                {
                    if (!dico.ContainsKey(category))
                    {
                        dico.Add(category, new List<string>());
                    }

                    foreach (var file in plugin.Files)
                    {
                        foreach (var tool in PluginManagerExtended.Instance.Plugins)
                        {
                            var assemblyFile = Path.GetFileName(tool.Value.GetType().Assembly.Location);
                            if (assemblyFile.ToLower() == Path.GetFileName(file).ToLower())
                            {
                                dico[category].Add(tool.Metadata.Name);
                            }
                        }
                    }
                }
            }

            dico = dico.OrderByDescending(obj => obj.Key).ToDictionary(obj => obj.Key, obj => obj.Value);

            pluginsForm.DisplayCategories(dico);
        }

        private void SetTheme()
        {
            if (Options.Instance.Theme != null)
            {
                switch (Options.Instance.ThemeValue)
                {
                    case ThemeName.Blue:
                        {
                            var theme = new VS2015BlueTheme();
                            dpMain.Theme = theme;
                        }
                        break;

                    case ThemeName.Light:
                        {
                            var theme = new VS2015LightTheme();
                            dpMain.Theme = theme;
                        }
                        break;

                    case ThemeName.Dark:
                        {
                            var theme = new VS2015DarkTheme();
                            dpMain.Theme = theme;
                        }
                        break;
                }
            }
        }

        #endregion Form management

        #region Start page

        private void ShowStartPage()
        {
            if (startPage == null || startPage.IsDisposed)
            {
                startPage = new StartPage(pluginsForm.PluginManager);
                startPage.OpenMruPluginRequested += StartPage_OpenMruPluginRequested;
                startPage.OpenPluginRequested += StartPage_OpenPluginRequested;
                startPage.OpenConnectionsManagementRequested += (s, evt) => { fHelper.DisplayConnectionsList(this); };
                startPage.OpenPluginsStoreRequested += (s, evt) =>
                {
                    tsddbTools_DropDownItemClicked(s, new ToolStripItemClickedEventArgs(pluginsStoreToolStripMenuItem));
                };
            }
            startPage.Show(dpMain, DockState.Document);
        }

        private void StartPage_OpenMruPluginRequested(object sender, OpenMruPluginEventArgs e)
        {
            var cid = e.Item.ConnectionId;

            if (cid == Guid.Empty)
            {
                initialPluginName = e.Item.PluginName;
                StartPluginWithoutConnection(e);
                return;
            }

            foreach (var file in ConnectionsList.Instance.Files)
            {
                var cList = CrmConnections.LoadFromFile(file.Path);
                var connection = cList.Connections.FirstOrDefault(c => c.ConnectionId == cid);
                if (connection != null)
                {
                    initialPluginName = e.Item.PluginName;
                    initialConnectionName = e.Item.ConnectionName;

                    if (connection.ConnectionName != connectionDetail?.ConnectionName)
                    {
                        var info = new ConnectionParameterInfo();

                        UserControl connectingControl;
                        if (connection.UseOnline)
                        {
                            connectingControl = new ConnectingCdsControl { Anchor = AnchorStyles.None };
                        }
                        else
                        {
                            connectingControl = new ConnectingControl { Anchor = AnchorStyles.None };
                        }
                        connectingControl.Left = Width / 2 - connectingControl.Width / 2;
                        connectingControl.Top = Height / 2 - connectingControl.Height / 2;
                        Controls.Add(connectingControl);
                        connectingControl.BringToFront();

                        info.ConnControl = connectingControl;
                        numberOfConnectionReceived = 0;
                        fHelper.AskForConnection(connection, info, null);
                    }
                    else
                    {
                        StartPluginWithConnection();
                    }
                }
            }
        }

        private void StartPage_OpenPluginRequested(object sender, OpenFavoritePluginEventArgs e)
        {
            initialPluginName = e.Item.PluginName;
            if (e.NewConnectionNeeded)
            {
                pluginsForm.OpenPlugin(initialPluginName, null, true);
                return;
            }

            StartPluginWithoutConnection();
        }

        #endregion Start page

        #region Plugins Form events

        private UserControl DisplayPluginControl(Lazy<IXrmToolBoxPlugin, IPluginMetadata> plugin, bool displayConnected = true)
        {
            Guid pluginControlInstanceId = Guid.NewGuid();
            UserControl pluginControl = null;
            try
            {
                var mruItem = new MostRecentlyUsedItem
                {
                    PluginName = plugin.Metadata.Name,
                    Date = DateTime.Now
                };

                pluginControl = (UserControl)plugin.Value.GetControl();
                pluginControl.Tag = pluginControlInstanceId;

                // ReSharper disable once SuspiciousTypeConversion.Global
                if (pluginControl is IMessageBusHost host)
                {
                    host.OnOutgoingMessage += MainForm_MessageBroker;
                }

                string name;

                if (service != null && displayConnected)
                {
                    mruItem.ConnectionId = connectionDetail.ConnectionId ?? Guid.Empty;
                    mruItem.ConnectionName = connectionDetail.ConnectionName;

                    if (connectionDetail.IsFromSdkLoginCtrl)
                    {
                        ((IXrmToolBoxPluginControl)pluginControl).UpdateConnection(service, connectionDetail);
                    }
                    else
                    {
                        var crmSvcClient = connectionDetail.GetCrmServiceClient();

                        ((IXrmToolBoxPluginControl)pluginControl).UpdateConnection(crmSvcClient, connectionDetail);
                    }

                    name = $"{plugin.Metadata.Name} ({connectionDetail?.ConnectionName ?? "Not connected"})";
                }
                else
                {
                    name = $"{plugin.Metadata.Name} (Not connected)";
                }

                ((IXrmToolBoxPluginControl)pluginControl).OnRequestConnection += NewForm_OnRequestConnection;
                ((IXrmToolBoxPluginControl)pluginControl).OnWorkAsync += (sender, e) =>
               {
                   //var bw = new BackgroundWorker();
                   //bw.DoWork += (s, evt) => { evt.Result = AnnouncementManager.GetItemToDisplay(); };
                   //bw.RunWorkerCompleted += (s, evt) => { AnnouncementManager.Display(evt.Result as AnnouncementItem); };
                   //bw.RunWorkerAsync();
               };

                if (pnlConnectLoading.Visible)
                {
                    pnlConnectLoading.SendToBack();
                    pnlConnectLoading.Visible = false;
                }

                if (pluginControl is IDuplicatableTool idt)
                {
                    idt.DuplicateRequested += (sender, e) =>
                    {
                        var pluginName = ((PluginForm)((PluginControlBase)sender).ParentForm).PluginName;
                        if (e.NewConnection)
                        {
                            var pluginToDuplicate = pluginsForm.PluginManager.ValidatedPlugins.FirstOrDefault(p => p.Metadata.Name == pluginName);

                            if (((PluginForm)dpMain.ActiveContent).Control is IDuplicatableTool dt)
                            {
                                ConnectUponApproval(new DuplicateToolWithConnectionArgs { SourceTool = dt, Tool = pluginToDuplicate, State = e.State });
                            }
                        }
                        else
                        {
                            if (sender is IDuplicatableTool dt)
                            {
                                pluginsForm.DuplicateTool(((PluginForm)dpMain.ActiveContent).PluginName, dt, e.State);
                            }
                        }
                    };
                }

                var pluginForm = new PluginForm(pluginControl, name, plugin.Metadata.Name);
                pluginForm.PluginName = plugin.Metadata.Name;
                pluginForm.Show(dpMain, DockState.Document);
                pluginForm.SendMessageToStatusBar += StatusBarMessenger_SendMessageToStatusBar;
                pluginForm.TabPageContextMenuStrip = cmsMain;
                pluginForm.FormClosed += (sender, e) =>
                {
                    var pf = (PluginForm)sender;
                    pluginConnections.Remove(pf);

                    if (!pf.IsDisposed && !pf.Disposing)
                        pf.Dispose();

                    FillPluginsListMenuDisplay();
                };

                dpMain.Refresh();

                pluginConnections.Add(pluginForm, connectionDetail);
                if (connectionDetail == null)
                {
                    tssOpenOrg.Visible = false;
                    tsbOpenOrg.Visible = false;
                    tsbImpersonate.Visible = false;
                }

                var pluginInOption =
                    Options.Instance.MostUsedList.FirstOrDefault(i => i.Name == plugin.Value.GetType().FullName);
                if (pluginInOption == null)
                {
                    pluginInOption = new PluginUseCount { Name = plugin.Value.GetType().FullName, Count = 0 };
                    Options.Instance.MostUsedList.Add(pluginInOption);
                }

                pluginInOption.Count++;

                if (Options.Instance.LastAdvertisementDisplay == new DateTime() ||
                    Options.Instance.LastAdvertisementDisplay > DateTime.Now ||
                    Options.Instance.LastAdvertisementDisplay.AddDays(7) < DateTime.Now)
                {
                    bool displayAdvertisement = true;
                    try
                    {
                        var assembly = Assembly.LoadFile(
                            new FileInfo(Assembly.GetExecutingAssembly().Location).Directory +
                            "\\McTools.StopAdvertisement.dll");
                        {
                            Type type = assembly.GetType("McTools.StopAdvertisement.LicenseManager");
                            if (type != null)
                            {
                                MethodInfo methodInfo = type.GetMethod("IsValid");
                                if (methodInfo != null)
                                {
                                    object classInstance = Activator.CreateInstance(type, null);

                                    if ((bool)methodInfo.Invoke(classInstance, null))
                                    {
                                        displayAdvertisement = false;
                                    }
                                }
                            }
                        }
                    }
                    catch (FileNotFoundException)
                    {
                    }

                    if (displayAdvertisement)
                    {
                        pnlSupport.Visible = true;
                        Options.Instance.LastAdvertisementDisplay = DateTime.Now;
                    }
                }

                var existingItem = MostRecentlyUsedItems.Instance.Items.FirstOrDefault(i
                    => i.PluginName == mruItem.PluginName && i.ConnectionId == mruItem.ConnectionId);
                if (existingItem == null)
                {
                    MostRecentlyUsedItems.Instance.Items.Add(mruItem);
                    MostRecentlyUsedItems.Instance.Save();
                }
                else
                {
                    existingItem.Date = DateTime.Now;
                    MostRecentlyUsedItems.Instance.Save();
                }

                if (!(pluginControl is IPrivatePlugin))
                {
                    ai.WritePageView(plugin.Metadata.Name, plugin.Value.GetVersion());
                }

                Options.Instance.Save();

                return pluginControl;
            }
            catch (Exception error)
            {
                Invoke(new Action(() =>
                {
                    MessageBox.Show(this, $@"An error occured when trying to display this tool: {error.Message}",
                        @"Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);

                    if (pnlConnectLoading.Visible)
                    {
                        pnlConnectLoading.SendToBack();
                        pnlConnectLoading.Visible = false;
                    }
                }));

                return pluginControl;
            }
        }

        private void PluginsForm_ActionRequested(object sender, PluginsListEventArgs e)
        {
            switch (e.Action)
            {
                case PluginsListAction.OpenPluginsStore:
                    tsddbTools_DropDownItemClicked(sender, new ToolStripItemClickedEventArgs(pluginsStoreToolStripMenuItem));
                    break;
            }
        }

        private void PluginsForm_OpenPluginProjectUrlRequested(object sender, PluginEventArgs e)
        {
            var type = e.PluginControl?.GetType();
            if (type == null)
            {
                type = e.Plugin?.Value?.GetType();
                if (type == null)
                {
                    MessageBox.Show(this, @"Unable to determine tool location on disk");
                    return;
                }
            }

            var filePath = Assembly.GetAssembly(type).Location;
            if (File.Exists(filePath))
            {
                var fileName = Path.GetFileName(filePath);

                if (store == null)
                {
                    MessageBox.Show(this,
                        @"The Tool Library is not initialized so we cannot find the project url",
                        @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string projectUrl = store.GetPluginProjectUrlByFileName(fileName.ToLower());
                if (projectUrl != null)
                {
                    Process.Start(projectUrl);
                }
                else
                {
                    MessageBox.Show(this,
                        @"This tool is not on the Tool Library or its Project Url is not defined. Therefore, we cannot lead you to the project page",
                        @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void PluginsForm_OpenPluginRequested(object sender, PluginEventArgs e)
        {
            if (store != null && store.PluginsCount > 0)
            {
                var location = e.Plugin.Value.GetType().Assembly.Location;

                var updatedPlugin = store.GetPluginUpdateByFile(location);
                if (updatedPlugin != null)
                {
                    var exitPluginUpdate = false;

                    if (!Options.Instance.PluginsUpdateSkip.Any(
                        x => x.Name == updatedPlugin.Name
                             && x.Version == updatedPlugin.Version
                             && x.Date > DateTime.Now))
                    {
                        var dialog = new NewPluginVersion(updatedPlugin);
                        if (dialog.ShowDialog(this) == DialogResult.Yes)
                        {
                            var pu = store.PrepareInstallationPackages(new List<XtbPlugin> { updatedPlugin });
                            if (pu.Plugins.Any(p => p.RequireRestart))
                            {
                                if (dialog.OnNextRestart)
                                {
                                    store.PerformInstallation(pu, this);
                                    UpdatePluginUpdateSkip(updatedPlugin.Name, updatedPlugin.Version, true, 1);
                                }
                                else
                                {
                                    if (DialogResult.Yes == MessageBox.Show(this,
                                            @"This application needs to restart to install updated tools (or new tools that share some files with already installed tools). Click Yes to restart this application now",
                                            @"Information", MessageBoxButtons.YesNo, MessageBoxIcon.Information))
                                    {
                                        RequestCloseTabs(dpMain.Contents.OfType<PluginForm>(),
                                            new PluginCloseInfo(ToolBoxCloseReason.CloseAll));
                                        store.PerformInstallation(pu, this);
                                        Application.Restart();
                                    }
                                }
                            }

                            exitPluginUpdate = true;
                        }

                        if (!exitPluginUpdate)
                        {
                            UpdatePluginUpdateSkip(updatedPlugin.Name, updatedPlugin.Version, dialog.IsVersionSkipped, dialog.NumberOfDaysToSkip);
                        }
                    }
                }
            }

            Cursor = Cursors.WaitCursor;

            if (e.NeedNewConnection)
            {
                ConnectUponApproval(e.Plugin);
                Cursor = Cursors.Default;
                return;
            }

            if (e.Plugin.Value is INoConnectionRequired)
            {
                var ctrl = DisplayPluginControl(e.Plugin);
                if (ctrl is IDuplicatableTool dt && e.SourceTool != null)
                {
                    dt.ApplyState(e.State ?? e.SourceTool.GetState());
                }
                Cursor = Cursors.Default;
                return;
            }

            if (service == null && e.MruInfo == null)
            {
                var result = MessageBox.Show(new Form { TopMost = true }, @"Do you want to connect to an organization first?", $@"Opening {e.Plugin.Metadata.Name}",
                    MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    ConnectUponApproval(e.Plugin);
                }
                else if (result == DialogResult.No)
                {
                    if (e.Plugin != null)
                    {
                        var ctrl = DisplayPluginControl(e.Plugin);
                        if (ctrl is IDuplicatableTool dt && e.SourceTool != null)
                        {
                            dt.ApplyState(e.State ?? e.SourceTool.GetState());
                        }
                    }
                }
            }
            else
            {
                if (e.Plugin != null)
                {
                    var ctrl = DisplayPluginControl(e.Plugin, e.MruInfo == null || e.MruInfo.Item.ConnectionId != Guid.Empty);
                    if (ctrl is IDuplicatableTool dt && e.SourceTool != null)
                    {
                        dt.ApplyState(e.State ?? e.SourceTool.GetState());
                    }
                }
            }

            FillPluginsListMenuDisplay();

            Cursor = Cursors.Default;
        }

        private void PluginsForm_UninstallPluginRequested(object sender, PluginEventArgs e)
        {
            if (DialogResult.No == MessageBox.Show(this,
                    @"Are you sure you want to uninstall this tool?",
                    @"Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
            {
                return;
            }

            var filePath = Assembly.GetAssembly(e.PluginControl?.GetType() ?? e.Plugin.Value.GetType()).Location;
            if (File.Exists(filePath))
            {
                var fileName = Path.GetFileName(filePath);

                if (store == null)
                {
                    MessageBox.Show(this,
                        @"The Tool Library is not initialized so we cannot find files to uninstall",
                        @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                store.UninstallByFileName(fileName);
            }
        }

        private void UpdatePluginUpdateSkip(string name, string version, bool isVersionSkipped, int nbDays)
        {
            var existing = Options.Instance.PluginsUpdateSkip.FirstOrDefault(
                x => x.Name == name);

            if (existing != null)
            {
                Options.Instance.PluginsUpdateSkip.Remove(existing);
            }

            var nextDate = DateTime.Now.AddDays(nbDays);
            if (isVersionSkipped)
            {
                nextDate = DateTime.Now.AddYears(10);
            }

            Options.Instance.PluginsUpdateSkip.Add(new PluginUpdateSkip
            {
                Name = name,
                Version = version,
                Date = nextDate
            });
            Options.Instance.Save();
        }

        #endregion Plugins Form events

        #region Connection methods

        private void ApplyConnectionToTabs(ConnectionDetail detail)
        {
            var pluginForms = dpMain.Contents.OfType<PluginForm>().ToList();

            var tcu = new TabConnectionUpdater(pluginForms, detail) { StartPosition = FormStartPosition.CenterParent };

            if (tcu.ShowDialog(this) == DialogResult.OK)
            {
                foreach (PluginForm form in tcu.SelectedPluginForms)
                {
                    UpdateTabConnection(form, detail);
                }

                if (tcu.SelectedPluginForms.Any())
                {
                    FillPluginsListMenuDisplay();
                }
            }
        }

        private void ConnectUponApproval(object connectionParameter)
        {
            numberOfConnectionReceived = 0;

            var info = new ConnectionParameterInfo
            {
                ConnectionParmater = connectionParameter
            };

            fHelper.AskForConnection(info, details =>
            {
                if (details.Any(d => d.UseOnline == false))
                {
                    info.ConnControl = new ConnectingControl { Anchor = AnchorStyles.None };
                }
                else
                {
                    info.ConnControl = new ConnectingCdsControl { Anchor = AnchorStyles.None };
                }

                info.ConnControl.Left = Width / 2 - info.ConnControl.Width / 2;
                info.ConnControl.Top = Height / 2 - info.ConnControl.Height / 2;
                Controls.Add(info.ConnControl);
                info.ConnControl.BringToFront();
            });
        }

        private Task LaunchInitialConnection(ConnectionDetail detail)
        {
            return new Task(() => ConnectionManager.Instance.ConnectToServer(new List<ConnectionDetail> { detail }));
        }

        private void ManageConnectionControl()
        {
            if (!Directory.Exists(Paths.ConnectionsPath))
            {
                Directory.CreateDirectory(Paths.ConnectionsPath);
            }

            ConnectionsList.ConnectionsListFilePath = Path.Combine(Paths.ConnectionsPath, "MscrmTools.ConnectionsList.xml");
            cManager = ConnectionManager.Instance;
            cManager.FromXrmToolBox = true;
            cManager.RequestPassword += (sender, e) => fHelper.RequestPassword(e.ConnectionDetail);
            cManager.StepChanged += (sender, e) => ccsb.SetMessage(e.CurrentStep);
            cManager.ConnectionSucceed += (sender, e) =>
            {
                numberOfConnectionReceived++;
                var parameter = e.Parameter as ConnectionParameterInfo;
                if (parameter != null && e.NumberOfConnectionsRequested == numberOfConnectionReceived)
                {
                    Controls.Remove(parameter.ConnControl);
                    parameter.ConnControl.Dispose();
                }

                _ = Invoke(new Action(() =>
                  {
                      if (e.ConnectionDetail.UseOnline)
                      {
                          tsbOpenOrg.Text = @"Open environment";
                          tsbOpenOrg.ToolTipText = @"Open the connected environment in your web browser";
                          tsbOpenOrg.Image = new Bitmap(Properties.Resources.Dataverse_16x16);
                      }
                      else
                      {
                          tsbOpenOrg.Text = @"Open organization";
                          tsbOpenOrg.ToolTipText = @"Open the connected organization in your web browser";
                          tsbOpenOrg.Image = new Bitmap(Properties.Resources.LogoDyn365);
                      }

                      tsbImpersonate.Enabled = e.ConnectionDetail.CanImpersonate;
                      tsbImpersonate.Visible = true;

                      if (parameter != null)
                      {
                          var us = parameter.ConnectionParmater as UserControl;
                          var p = parameter.ConnectionParmater as Lazy<IXrmToolBoxPlugin, IPluginMetadata>;
                          var rcea = parameter.ConnectionParmater as RequestConnectionEventArgs;
                          var dtwc = parameter.ConnectionParmater as DuplicateToolWithConnectionArgs;

                          if (us != null)
                          {
                              ccsb.SetConnectionStatus(true, e.ConnectionDetail);
                              ccsb.SetMessage(string.Empty);
                              connectionDetail = e.ConnectionDetail;
                              service = e.OrganizationService;

                              if (!(us.Tag is Lazy<IXrmToolBoxPlugin, IPluginMetadata> pluginModel))
                              {
                                  // Actual Plugin was passed, Just update the plugin's Tab.
                                  UpdateTabConnection((PluginForm)us.ParentForm, e.ConnectionDetail);
                              }
                              else
                              {
                                  DisplayPluginControl(pluginModel);
                              }
                          }
                          else if (p != null)
                          {
                              ccsb.SetConnectionStatus(true, e.ConnectionDetail);
                              ccsb.SetMessage(string.Empty);
                              connectionDetail = e.ConnectionDetail;
                              service = e.OrganizationService;

                              DisplayPluginControl(p);
                          }
                          else if (parameter.ConnectionParmater?.ToString() == "ApplyConnectionToTabs" && dpMain.Contents.OfType<PluginForm>().Any())
                          {
                              ccsb.SetConnectionStatus(true, e.ConnectionDetail);
                              ccsb.SetMessage(string.Empty);
                              connectionDetail = e.ConnectionDetail;
                              service = e.OrganizationService;

                              ApplyConnectionToTabs(e.ConnectionDetail);
                          }
                          else if (rcea != null)
                          {
                              if (rcea.Parameter is Lazy<IXrmToolBoxPlugin, IPluginMetadata> plugin)
                              {
                                  ccsb.SetConnectionStatus(true, e.ConnectionDetail);
                                  ccsb.SetMessage(string.Empty);
                                  connectionDetail = e.ConnectionDetail;
                                  service = e.OrganizationService;

                                  DisplayPluginControl(plugin);
                              }
                              else
                              {
                                  var userControl = (UserControl)rcea.Control;

                                  if (rcea.ActionName != "AdditionalOrganization")
                                  {
                                      ccsb.SetConnectionStatus(true, e.ConnectionDetail);
                                      ccsb.SetMessage(string.Empty);
                                      connectionDetail = e.ConnectionDetail;
                                      service = e.OrganizationService;

                                      if (userControl.ParentForm != null)
                                      {
                                          var indexOfParenthesis = userControl.ParentForm?.Text?.IndexOf("(") ?? -1;
                                          var pluginName =
                                              userControl.ParentForm?.Text?.Substring(0, indexOfParenthesis - 1) ??
                                              "N/A";

                                          userControl.ParentForm.Text =
                                              $@"{pluginName} ({e.ConnectionDetail.ConnectionName})";
                                      }
                                  }

                                  ((PluginForm)userControl.ParentForm).UpdateConnection(e.OrganizationService,
                                      e.ConnectionDetail, rcea.ActionName, rcea.Parameter);
                              }
                          }
                          else if (dtwc != null)
                          {
                              ccsb.SetConnectionStatus(true, e.ConnectionDetail);
                              ccsb.SetMessage(string.Empty);
                              connectionDetail = e.ConnectionDetail;
                              service = e.OrganizationService;

                              var state = dtwc.State ?? dtwc.SourceTool.GetState();
                              var duplicatedTool = DisplayPluginControl(dtwc.Tool);
                              ((IDuplicatableTool)duplicatedTool).ApplyState(state);
                          }
                          else
                          {
                              ccsb.SetConnectionStatus(true, e.ConnectionDetail);
                              ccsb.SetMessage(string.Empty);
                              connectionDetail = e.ConnectionDetail;
                              service = e.OrganizationService;
                          }
                      }
                      else if (dpMain.Contents.OfType<PluginForm>().Count() > 1)
                      {
                          ccsb.SetConnectionStatus(true, e.ConnectionDetail);
                          ccsb.SetMessage(string.Empty);
                          connectionDetail = e.ConnectionDetail;
                          service = e.OrganizationService;

                          ApplyConnectionToTabs(e.ConnectionDetail);
                      }
                      else if (dpMain.Contents.OfType<PluginForm>().Count() == 1)
                      {
                          ccsb.SetConnectionStatus(true, e.ConnectionDetail);
                          ccsb.SetMessage(string.Empty);
                          connectionDetail = e.ConnectionDetail;
                          service = e.OrganizationService;

                          UpdateTabConnection(dpMain.Contents.OfType<PluginForm>().First(), e.ConnectionDetail);
                      }
                      else
                      {
                          ccsb.SetConnectionStatus(true, e.ConnectionDetail);
                          ccsb.SetMessage(string.Empty);
                          connectionDetail = e.ConnectionDetail;
                          service = e.OrganizationService;
                      }

                      StartPluginWithConnection();

                      tssOpenOrg.Visible = true;
                      tsbOpenOrg.Visible = true;
                      tsbImpersonate.Visible = true;
                  }));
            };
            cManager.ConnectionFailed += (sender, e) =>
            {
                numberOfConnectionReceived++;
                Invoke(new Action(() =>
                {
                    if (e.Parameter is ConnectionParameterInfo param && param.ConnControl != null && e.NumberOfConnectionsRequested == numberOfConnectionReceived)
                    {
                        Controls.Remove(param.ConnControl);
                        param.ConnControl.Dispose();
                    }

                    MessageBox.Show(this, e.FailureReason, @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    connectionDetail = null;
                    service = null;
                    ccsb.SetConnectionStatus(false, null);
                    ccsb.SetMessage(e.FailureReason);

                    StartPluginWithConnection();
                }));
            };

            fHelper = new FormHelper(this);
            ccsb = new CrmConnectionStatusBar(fHelper) { Dock = DockStyle.Bottom };
            Controls.Add(ccsb);
            Controls.SetChildIndex(ccsb, Controls.Count - 1);
        }

        private void NewForm_OnRequestConnection(object sender, System.EventArgs e)
        {
            ConnectUponApproval(e is RequestConnectionEventArgs ? e : sender);
        }

        private void StartPluginWithConnection()
        {
            if (!string.IsNullOrEmpty(initialConnectionName) && !string.IsNullOrEmpty(initialPluginName))
            {
                StartPluginWithoutConnection();

                // Resetting initial connection name
                initialConnectionName = string.Empty;
            }
        }

        private void StartPluginWithoutConnection(OpenMruPluginEventArgs e = null)
        {
            if (!string.IsNullOrEmpty(initialPluginName))
            {
                pluginsForm.OpenPlugin(initialPluginName, e);

                // Resetting initial plugin name
                initialPluginName = string.Empty;
            }
        }

        private void tsbConnect_Click(object sender, System.EventArgs e)
        {
            ConnectUponApproval("ApplyConnectionToTabs");
        }

        private void UpdateTabConnection(PluginForm pluginForm, ConnectionDetail detail, string actionName = "", object parameter = null)
        {
            var indexOfParenthesis = pluginForm.Text?.IndexOf("(") ?? -1;
            var pluginName = pluginForm.Text?.Substring(0, indexOfParenthesis - 1) ?? "N/A";

            if (pluginConnections.ContainsKey(pluginForm))
                pluginConnections[pluginForm] = connectionDetail;

            pluginForm.UpdateConnection(service, detail, actionName, parameter);
            pluginForm.Text = $@"{pluginName} ({detail?.ConnectionName ?? "Not connected"})";
        }

        #endregion Connection methods

        #region Message broker

        private PluginForm GetPluginByName(string name, Guid connectionDetailId)
        {
            if (Guid.TryParse(name, out Guid pluginId) && !pluginId.Equals(Guid.Empty))
            {
                var expectedPlugin = PluginManagerExtended.Instance.Plugins.FirstOrDefault(p =>
                    p.Value is PluginBase pb && pb.GetId().Equals(pluginId));

                if (expectedPlugin != null)
                {
                    name = expectedPlugin.Metadata.Name;
                }
            }

            return dpMain.Contents
                .OfType<PluginForm>()
                .LastOrDefault(c => c.PluginName == name
                                     && (((PluginControlBase)c.Control).ConnectionDetail?.ConnectionId ?? Guid.Empty).Equals(connectionDetailId));
        }

        private bool IsMessageValid(object sender, MessageBusEventArgs message)
        {
            if (message == null ||
                !(sender is UserControl sourceControl) ||
                !(sourceControl is IXrmToolBoxPluginControl) ||
                !(sourceControl.ParentForm is PluginForm pluginForm))
            {
                // Error. Possible reasons are:
                // * empty sender
                // * empty message
                // * sender is not UserControl
                // * sender is not XrmToolBox Plugin
                return false;
            }

            if (string.IsNullOrEmpty(message.SourcePlugin))
            {
                message.SourcePlugin = pluginForm.PluginName;
            }
            // Commenting below - perhaps the caller wanted to override the default name for some reason // J Rapp 2018-05-30
            //else if (message.SourcePlugin != sourceControl.GetType().GetTitle())
            //{
            //    // For some reason incorrect name was set in Source Plugin field
            //    return false;
            //}

            // Everything went ok
            return true;
        }

        private void MainForm_MessageBroker(object sender, MessageBusEventArgs message)
        {
            if (!IsMessageValid(sender, message))
            {
                return;
            }

            var sourceDetail = ((PluginControlBase)sender).ConnectionDetail;

            var content = message.NewInstance ? null : GetPluginByName(message.TargetPlugin, sourceDetail?.ConnectionId ?? Guid.Empty);
            if (content == null)
            {
                pluginsForm.OpenPlugin(message.TargetPlugin);
            }

            content = GetPluginByName(message.TargetPlugin, sourceDetail?.ConnectionId ?? Guid.Empty);
            if (content == null)
            {
                MessageBox.Show($@"Cannot switch to tool {message.TargetPlugin}.", message.SourcePlugin, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            content.Show(dpMain, content.DockState);
            content.SendIncomingBrokerMessage(message);
        }

        #endregion Message broker

        #region StatusBar Messenger

        private void StatusBarMessenger_SendMessageToStatusBar(object sender, StatusBarMessageEventArgs e)
        {
            var currentPlugin = pluginControlStatuses.FirstOrDefault(pcs => pcs.Control == sender);
            if (currentPlugin == null)
            {
                pluginControlStatuses.Add(new PluginControlStatus(
                    (IXrmToolBoxPluginControl)sender,
                    e.Progress,
                    e.Message
                ));
            }
            else
            {
                currentPlugin.Percentage = e.Progress;
                currentPlugin.Message = e.Message;
            }

            void Mi()
            {
                if (dpMain.ActiveContent is PluginForm pf)
                {
                    if (pf.Control == sender)
                    {
                        ccsb.SetMessage(e.Message);
                        ccsb.SetProgress(e.Progress);
                    }
                }
                else
                {
                    ccsb.SetMessage(null);
                    ccsb.SetProgress(null);
                }
            }

            if (InvokeRequired)
            {
                Invoke((MethodInvoker)Mi);
            }
            else
            {
                Mi();
            }
        }

        #endregion StatusBar Messenger

        #region Check for update

        private Task LaunchVersionCheck()
        {
            return new Task(() =>
            {
                if (Options.Instance.DoNotCheckForUpdates)
                {
                    return;
                }

                WelcomeDialog.SetStatus("Checking for XrmToolBox update...");
                // blackScreen.SetWorkingMessage("Checking for XrmToolBox update...");

                var currentVersion = Assembly.GetExecutingAssembly().GetName().Version;
                try
                {
                    var request = WebRequest.CreateHttp("https://www.xrmtoolbox.com/_odata/releases");
                    var response = request.GetResponse();
                    Releases releases = null;
                    using (Stream dataStream = response.GetResponseStream())
                    {
                        if (dataStream != null)
                        {
                            var serializer = new DataContractJsonSerializer(typeof(Releases),
                                new DataContractJsonSerializerSettings
                                {
                                    UseSimpleDictionaryFormat = true,
                                    DateTimeFormat = new DateTimeFormat("yyyy-MM-dd'T'HH:mm:ss",
                                        new DateTimeFormatInfo { FullDateTimePattern = "yyyy-MM-dd'T'HH:mm:ss" })
                                });

                            releases = (Releases)serializer.ReadObject(dataStream);
                        }
                    }

                    if (releases != null)
                    {
                        var lastReleaseVersion = releases.Items.Max(i => new Version(i.Version));
                        if (lastReleaseVersion > currentVersion &&
                            Options.Instance.LastUpdateCheck.Date != DateTime.Now.Date)
                        {
                            var release =
                                releases.Items.FirstOrDefault(r => r.Version == lastReleaseVersion.ToString());

                            Invoke(new Action(() =>
                            {
                                var nvForm = new NewVersionForm(release?.Version, new Uri(release?.DownloadUrl ?? "https://www.xrmtoolbox.com"));
                                nvForm.Show(dpMain, DockState.Document);
                            }));
                        }
                    }

                    Options.Instance.LastUpdateCheck = DateTime.Now;
                    Options.Instance.Save();
                }
                // ReSharper disable once EmptyGeneralCatchClause
                catch
                {
                }
            });
        }

        #endregion Check for update

        #region Change of active content

        private void ApplyActiveContentDisplay()
        {
            if (dpMain.ActiveContent != null && currentContent != dpMain.ActiveContent)
            {
                currentContent = dpMain.ActiveContent;

                ProcessMenuItemsForPlugin2();

                if (dpMain.ActiveContent is StartPage sp)
                {
                    sp.LoadMru();
                }

                if (dpMain.ActiveContent is PluginForm pf)
                {
                    //  tsmiAbout.Click -= tsmiAbout_Click;

                    if (pf.Control is PluginControlBase pcb)
                    {
                        ccsb.SetConnectionStatus(pcb.ConnectionDetail != null, pcb.ConnectionDetail);
                        connectionDetail = pcb.ConnectionDetail;
                    }
                }
                else
                {
                    //tsmiAbout.Click -= tsmiAbout_Click;
                    //tsmiAbout.Click += tsmiAbout_Click;
                }
            }
        }

        private void dpMain_ActiveContentChanged(object sender, System.EventArgs e)
        {
            ApplyActiveContentDisplay();
        }

        private void dpMain_ActiveDocumentChanged(object sender, System.EventArgs e)
        {
            ApplyActiveContentDisplay();
        }

        private void dpMain_ActivePaneChanged(object sender, System.EventArgs e)
        {
            ApplyActiveContentDisplay();
        }

        private void dpMain_DocumentDragged(object sender, System.EventArgs e)
        {
            ApplyActiveContentDisplay();
        }

        #endregion Change of active content

        #region Menu Toolbar

        private void checkForUpdateToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            var worker = new BackgroundWorker();
            worker.DoWork += (s, evt) =>
            {
                var request = WebRequest.CreateHttp("https://www.xrmtoolbox.com/_odata/releases");
                var response = request.GetResponse();
                using (Stream dataStream = response.GetResponseStream())
                {
                    if (dataStream != null)
                    {
                        var serializer = new DataContractJsonSerializer(typeof(Releases),
                            new DataContractJsonSerializerSettings
                            {
                                UseSimpleDictionaryFormat = true,
                                DateTimeFormat = new DateTimeFormat("yyyy-MM-dd'T'HH:mm:ss", new DateTimeFormatInfo { FullDateTimePattern = "yyyy-MM-dd'T'HH:mm:ss" })
                            });

                        evt.Result = (Releases)serializer.ReadObject(dataStream);
                    }
                }
            };
            worker.RunWorkerCompleted += (s, evt) =>
            {
                if (evt.Result is Releases releases)
                {
                    var lastReleaseVersion = releases.Items.Max(i => new Version(i.Version));
                    var currentVersion = Assembly.GetExecutingAssembly().GetName().Version;
                    if (lastReleaseVersion > currentVersion)
                    {
                        var release = releases.Items.FirstOrDefault(r => r.Version == lastReleaseVersion.ToString());

                        Invoke(new Action(() =>
                        {
                            var nvForm = new NewVersionForm(release?.Version, new Uri(release?.DownloadUrl ?? "about:_blank"));
                            nvForm.Show(dpMain, DockState.Document);
                        }));
                    }
                    else
                    {
                        MessageBox.Show(this, @"No update available!", @"Information", MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }
                }
                else
                {
                    MessageBox.Show(this, @"No update information found!", @"Information", MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }

                Options.Instance.LastUpdateCheck = DateTime.Now;
                Options.Instance.Save();
            };
            worker.RunWorkerAsync();
        }

        private void tsbOpenOrg_Click(object sender, System.EventArgs e)
        {
            var activeContent = dpMain.ActiveContent as PluginForm;
            var url = string.Empty;
            if (activeContent == null)
            {
                url = connectionDetail.WebApplicationUrl;
            }
            else if (pluginConnections.ContainsKey(activeContent) && pluginConnections[activeContent] != null)
            {
                url = pluginConnections[activeContent].WebApplicationUrl;
            }

            if (url.Length > 0)
            {
                Process.Start(url);
            }
        }

        private void tsddbTools_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem == pluginsStoreToolStripMenuItem)
            {
                if (pluginsForm == null)
                {
                    return;
                }

                // If the options were not initialized, it means we are using the
                // new plugins store for the first time. Copy values from main
                // options file
                if (!PluginsStore.Options.Instance.IsInitialized)
                {
                    PluginsStore.Options.Instance.DisplayPluginsStoreOnStartup = Options.Instance.DisplayPluginsStoreOnStartup;
                    PluginsStore.Options.Instance.PluginsStoreShowInstalled = Options.Instance.PluginsStoreShowInstalled;
                    PluginsStore.Options.Instance.PluginsStoreShowIncompatible = Options.Instance.PluginsStoreShowIncompatible;
                    PluginsStore.Options.Instance.PluginsStoreShowNew = Options.Instance.PluginsStoreShowNew;
                    PluginsStore.Options.Instance.PluginsStoreShowUpdates = Options.Instance.PluginsStoreShowUpdates;
                    PluginsStore.Options.Instance.UseLegacy = false;
                    PluginsStore.Options.Instance.IsInitialized = true;
                }

                IStoreForm form = new StoreFormFromPortal(Options.Instance.ConnectionControlsAllowPreReleaseUpdates);
                ((StoreFormFromPortal)form).PluginsClosingRequested += (s, evt) =>
                {
                    RequestCloseTabs(dpMain.Contents.OfType<PluginForm>(), new PluginCloseInfo(ToolBoxCloseReason.CloseAll));
                };

                // Avoid scanning for new files during Plugins Store usage.
                pluginsForm.PluginManager.IsWatchingForNewPlugins = false;
                ((Form)form).ShowDialog(this);
                pluginsForm.PluginManager.IsWatchingForNewPlugins = true;
                pluginsForm.ReloadPluginsList();
                PrepareCategories();

                // Apply option to show Plugins Store on startup on main options
                if (Options.Instance.DisplayPluginsStoreOnStartup != PluginsStore.Options.Instance.DisplayPluginsStoreOnStartup)
                {
                    Options.Instance.DisplayPluginsStoreOnStartup = PluginsStore.Options.Instance.DisplayPluginsStoreOnStartup ?? false;
                }
            }
            else if (e.ClickedItem == manageConnectionsToolStripMenuItem)
            {
                fHelper.DisplayConnectionsList(this);
            }
            else if (e.ClickedItem == tsmiXtbSettings)
            {
                var oDialog = new OptionsDialog(Options.Instance);
                if (oDialog.ShowDialog(this) == DialogResult.OK)
                {
                    bool reinitDisplay = Options.Instance.MostUsedList.Count != oDialog.Option.MostUsedList.Count
                                         || Options.Instance.IconDisplayMode != oDialog.Option.IconDisplayMode
                                         || !oDialog.Option.HiddenPlugins.SequenceEqual(Options.Instance.HiddenPlugins)
                                         || Options.Instance.PluginsDisplayOrder != oDialog.Option.PluginsDisplayOrder
                                         || Options.Instance.NumberOfDaysToShowNewRibbon != oDialog.Option.NumberOfDaysToShowNewRibbon;

                    if (Options.Instance.ConnectionControlsAllowPreReleaseUpdates !=
                        oDialog.Option.ConnectionControlsAllowPreReleaseUpdates)
                    {
                        store.AllowConnectionControlPreRelease = oDialog.Option.ConnectionControlsAllowPreReleaseUpdates;
                        Options.Instance.ConnectionControlsAllowPreReleaseUpdates = oDialog.Option.ConnectionControlsAllowPreReleaseUpdates;
                        Options.Instance.Save();

                        if (store.AllowConnectionControlPreRelease == false)
                        {
                            var message =
                                @"You asked to not use Connection Controls pre release anymore.

Would you like to reinstall last stable release of connection controls?";

                            if (MessageBox.Show(this, message, @"Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                            {
                                var restartNow = store.PrepareConnectionControlsUpdate(this, false, out string versionToStore);

                                Options.Instance.ConnectionControlsVersion = versionToStore;
                                Options.Instance.Save();

                                if (restartNow)
                                {
                                    Application.Restart();
                                }
                            }
                        }
                        else
                        {
                            CheckForConnectionControlsUpdate(true);
                        }
                    }

                    if (Options.Instance.DoNotRememberPluginsWithoutConnection != oDialog.Option.DoNotRememberPluginsWithoutConnection
                        && oDialog.Option.DoNotRememberPluginsWithoutConnection)
                    {
                        MostRecentlyUsedItems.Instance.RemovePluginsWithNoConnection();
                        MostRecentlyUsedItems.Instance.Save();
                    }

                    if (Options.Instance.MruItemsToDisplay != oDialog.Option.MruItemsToDisplay)
                    {
                        MostRecentlyUsedItems.Instance.Save();
                    }

                    if (Options.Instance.Theme != oDialog.Option.Theme)
                    {
                        SetTheme();
                    }

                    Options.Instance.Replace(oDialog.Option);

                    if (reinitDisplay)
                    {
                        pluginsForm.ReloadPluginsList();
                    }

                    cManager.ReuseConnections = Options.Instance.ReuseConnections;
                }
            }
            else if (e.ClickedItem == tsmiToolSettings)
            {
                var plugin = (ISettingsPlugin)((PluginForm)dpMain.ActiveContent).Control;
                plugin.ShowSettings();
            }
        }

        #region Document management

        private void cmsMain_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem == cmsMainCloseAll)
            {
                RequestCloseTabs(dpMain.Contents.OfType<PluginForm>(), new PluginCloseInfo(ToolBoxCloseReason.CloseAll));
            }
            else if (e.ClickedItem == cmsMainCloseExceptThis)
            {
                RequestCloseTabs(dpMain.Contents.OfType<PluginForm>().Where(p => dpMain.ActiveContent != p), new PluginCloseInfo(ToolBoxCloseReason.CloseAllExceptActive));
            }
            else if (e.ClickedItem == cmsMainCloseThis)
            {
                RequestCloseTab((PluginForm)dpMain.ActiveContent, new PluginCloseInfo(ToolBoxCloseReason.CloseCurrent));
            }
            else if (e.ClickedItem == cmsMainDuplicateTool)
            {
                if (((PluginForm)dpMain.ActiveContent).Control is IDuplicatableTool dt)
                {
                    pluginsForm.DuplicateTool(((PluginForm)dpMain.ActiveContent).PluginName, dt, null);
                }
                else
                {
                    pluginsForm.OpenPlugin(((PluginForm)dpMain.ActiveContent).PluginName);
                }
            }
            else if (e.ClickedItem == cmsMainDuplicateToolWithConnection)
            {
                var pluginName = ((PluginForm)dpMain.ActiveContent).PluginName;
                var plugin = pluginsForm.PluginManager.ValidatedPlugins.FirstOrDefault(p => p.Metadata.Name == pluginName);

                if (((PluginForm)dpMain.ActiveContent).Control is IDuplicatableTool dt)
                {
                    ConnectUponApproval(new DuplicateToolWithConnectionArgs { SourceTool = dt, Tool = plugin });
                }
                else
                {
                    ConnectUponApproval(new RequestConnectionEventArgs
                    { ActionName = "", Control = ((PluginForm)dpMain.ActiveDocument).Control, Parameter = plugin });
                }
            }
            else if (e.ClickedItem == tsmiChangeTabConnection)
            {
                ConnectUponApproval(new RequestConnectionEventArgs { ActionName = "", Control = ((PluginForm)dpMain.ActiveDocument).Control });
            }
        }

        private void FillPluginsListMenuDisplay()
        {
            var staticItems = tsbManageWindows.DropDownItems.Cast<ToolStripItem>().Take(5).ToList();

            tsbManageWindows.DropDownItems.Clear();

            var pluginsPages = dpMain.Contents.OfType<PluginForm>().ToList().OrderBy(pf => pf.Text).ToList();

            tsbManageWindows.DropDownItems.AddRange(staticItems.ToArray());

            if (pluginsPages.Any())
            {
                tsbManageWindows.DropDownItems.Add(new ToolStripSeparator());
            }

            foreach (var pluginPage in pluginsPages)
            {
                tsbManageWindows.DropDownItems.Add(new ToolStripMenuItem
                {
                    Text = pluginPage.Text,
                    Tag = pluginPage
                });
            }
        }

        private void RequestCloseTab(PluginForm content, PluginCloseInfo info, bool forceSilent = false)
        {
            if (content.CloseWithReason(info.ToolBoxReason, forceSilent))
            {
                pluginConnections.Remove(content);
            }
        }

        private void RequestCloseTabs(IEnumerable<PluginForm> pages, PluginCloseInfo info)
        {
            var pagesList = pages.ToList();
            if ((info.FormReason != CloseReason.None ||
                 info.ToolBoxReason == ToolBoxCloseReason.CloseAll ||
                 info.ToolBoxReason == ToolBoxCloseReason.CloseAllExceptActive)
                && pagesList.Count > 0
                && !Options.Instance.CloseEachPluginSilently)
            {
                info.Cancel = MessageBox.Show(@"Are you sure you want to close " + pagesList.Count + @" tab(s)?", @"Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes;
                if (info.Cancel)
                {
                    return;
                }
            }

            foreach (var page in pagesList)
            {
                RequestCloseTab(page, info, true);
                if (info.Cancel) return;
            }
        }

        private void tsbManageWindows_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem == closeAllWindowsToolStripMenuItem)
            {
                RequestCloseTabs(dpMain.Contents.OfType<PluginForm>(), new PluginCloseInfo(ToolBoxCloseReason.CloseAll));
            }
            else if (e.ClickedItem == closeAllWindowsExceptActiveToolStripMenuItem)
            {
                RequestCloseTabs(dpMain.Contents.OfType<PluginForm>().Where(p => dpMain.ActiveContent != p), new PluginCloseInfo(ToolBoxCloseReason.CloseAllExceptActive));
            }
            else if (e.ClickedItem == closeCurrentWindowToolStripMenuItem)
            {
                if (dpMain.ActiveContent is PluginForm p)
                {
                    RequestCloseTab(p, new PluginCloseInfo(ToolBoxCloseReason.CloseCurrent));
                }
            }
            else if (e.ClickedItem == tsmiShowStartPage)
            {
                if (dpMain.Contents.OfType<StartPage>().Any())
                {
                    startPage.EnsureVisible(dpMain, DockState.Document);
                }
                else
                {
                    ShowStartPage();
                }
            }
            else if (e.ClickedItem.Tag is PluginForm pf)
            {
                pf.EnsureVisible(dpMain, DockState.Document);
            }
        }

        #endregion Document management

        #endregion Menu Toolbar

        #region Key Shortcuts

        private void NewForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.Shift && e.KeyCode == Keys.C)
            {
                tsbConnect_Click(null, null);
            }
            else
            {
                ((dpMain.ActiveContent as PluginForm)?.Control as IShortcutReceiver)?.ReceiveKeyDownShortcut(e);
            }
        }

        private void NewForm_KeyPress(object sender, KeyPressEventArgs e)
        {
            ((dpMain.ActiveContent as PluginForm)?.Control as IShortcutReceiver)?.ReceiveKeyPressShortcut(e);
        }

        private void NewForm_KeyUp(object sender, KeyEventArgs e)
        {
            if (!(e.Control && e.Shift && e.KeyCode == Keys.C))
            {
                ((dpMain.ActiveContent as PluginForm)?.Control as IShortcutReceiver)?.ReceiveKeyUpShortcut(e);
            }
        }

        private void NewForm_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (!(e.Control && e.Shift && e.KeyCode == Keys.C))
            {
                ((dpMain.ActiveContent as PluginForm)?.Control as IShortcutReceiver)?.ReceivePreviewKeyDownShortcut(e);
            }
        }

        #endregion Key Shortcuts

        #region Support panel

        private void llDismiss_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            pnlSupport.Visible = false;
        }

        private void llDonate_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string pluginName = null;
            if (currentContent is PluginForm pf)
            {
                pluginName = pf.PluginName;
            }

            var form = new DonationIntroForm(pluginName);
            form.ShowDialog(this);
        }

        #endregion Support panel

        private void llClosePluginsUpdatePanel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            pnlPluginsUpdate.Visible = false;
        }

        private void openPluginsStoreButton_Click(object sender, System.EventArgs e)
        {
            pnlPluginsUpdate.Visible = false;
            tsddbTools_DropDownItemClicked(null, new ToolStripItemClickedEventArgs(pluginsStoreToolStripMenuItem));
        }

        private void tsbImpersonate_Click(object sender, System.EventArgs e)
        {
            var dialog = new UserSelectionDialog(connectionDetail.ServiceClient);
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                connectionDetail.Impersonate(dialog.SelectedUser.Id, dialog.SelectedUser.GetAttributeValue<string>("fullname"));
            }
        }
    }
}