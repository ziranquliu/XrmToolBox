﻿using System.Windows.Forms;

namespace McTools.Xrm.Connection.WinForms.CustomControls
{
    public partial class ConnectionSucceededControl : UserControl, IConnectionWizardControl
    {
        public ConnectionSucceededControl()
        {
            InitializeComponent();
        }

        public ConnectionDetail ConnectionDetail { private get; set; }

        public string ConnectionName
        {
            get => txtConnectionName.Text;
            set => txtConnectionName.Text = value;
        }

        private void btnClearEnvHighlight_Click(object sender, System.EventArgs e)
        {
            ConnectionDetail.EnvironmentHighlightingInfo = null;

            btnClearEnvHighlight.Visible = false;
        }

        private void btnSetEnvHighlight_Click(object sender, System.EventArgs e)
        {
            var dialog = new EnvHighlightDialog(ConnectionDetail);
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                ConnectionDetail.EnvironmentHighlightingInfo = new EnvironmentHighlighting
                {
                    Color = dialog.BackColorSelected,
                    TextColor = dialog.TextColorSelected,
                    Text = dialog.TextSelected
                };

                btnClearEnvHighlight.Visible = true;
            }
        }

        private void ConnectionSucceededControl_Load(object sender, System.EventArgs e)
        {
            if (!ConnectionManager.Instance.FromXrmToolBox)
            {
                lblHighlight.Visible = false;
                btnClearEnvHighlight.Visible = ConnectionDetail.IsEnvironmentHighlightSet;
                btnSetEnvHighlight.Visible = false;
            }
            else
            {
                btnClearEnvHighlight.Visible = ConnectionDetail.IsEnvironmentHighlightSet;
                lblHighlight.Visible = true;
                btnSetEnvHighlight.Visible = true;
            }
        }
    }
}