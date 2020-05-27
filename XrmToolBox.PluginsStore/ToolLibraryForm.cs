using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using XrmToolBox.PluginsStore.CustomControls;
using XrmToolBox.PluginsStore.DTO;

namespace XrmToolBox.PluginsStore
{
    public partial class ToolLibraryForm : Form
    {
        private readonly List<string> selectedPackagesId;
        private readonly StoreFromPortal store;

        public ToolLibraryForm(bool allowConnectionControlPrerelease = false)
        {
            InitializeComponent();

            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.DoubleBuffer,
                true);

            selectedPackagesId = new List<string>();

            store = new StoreFromPortal(allowConnectionControlPrerelease);
            store.LoadNugetPackages();
            store.PluginsUpdated += (sender, e) => { PluginsUpdated?.Invoke(sender, e); };
        }

        public event EventHandler PluginsUpdated;

        public void RefreshPluginsList(bool reload = true)
        {
            selectedPackagesId.Clear();

            var bw = new BackgroundWorker();
            bw.DoWork += (sender, e) =>
            {
                if (reload)
                {
                    store.LoadNugetPackages();
                }
                var xtbPackages = store.XrmToolBoxPlugins.Plugins.OrderByDescending(p => p.Name);

                e.Result = xtbPackages;
            };
            bw.RunWorkerCompleted += (sender, e) =>
            {
                if (e.Error != null)
                {
                    MessageBox.Show(this, $@"An error occured when refreshing list: {e.Error.Message}", @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    var items = (IOrderedEnumerable<XtbPlugin>)e.Result;
                    var list = new List<ToolLibraryItem>();
                    foreach (var xtbPackage in items)//.Take(2)
                    {
                        list.Add(new ToolLibraryItem(xtbPackage) { Dock = DockStyle.Top });
                    }

                    scMain.Panel1.Controls.AddRange(list.ToArray());
                }
            };
            bw.RunWorkerAsync();
        }

        private void ToolLibraryForm_Load(object sender, EventArgs e)
        {
            RefreshPluginsList();
        }
    }
}