namespace AuditHistoryRollBack
{
    partial class MyPluginControl
    {
        /// <summary> 
        /// Variable nécessaire au concepteur.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Nettoyage des ressources utilisées.
        /// </summary>
        /// <param name="disposing">true si les ressources managées doivent être supprimées ; sinon, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Code généré par le Concepteur de composants

        /// <summary> 
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas 
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MyPluginControl));
            this.toolStripMenu = new System.Windows.Forms.ToolStrip();
            this.LoadDataButton = new System.Windows.Forms.ToolStripButton();
            this.fetchXML = new System.Windows.Forms.ToolStripButton();
            this.tsbSample = new System.Windows.Forms.ToolStripButton();
            this.loadAuditButton = new System.Windows.Forms.Button();
            this.rollbackbutton = new System.Windows.Forms.Button();
            this.entitiesList = new System.Windows.Forms.ComboBox();
            this.recordGuid = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.guidLabel = new System.Windows.Forms.Label();
            this.showNewestValues = new System.Windows.Forms.CheckBox();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.toolStripMenu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // toolStripMenu
            // 
            this.toolStripMenu.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.toolStripMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.LoadDataButton,
            this.fetchXML,
            this.tsbSample});
            this.toolStripMenu.Location = new System.Drawing.Point(0, 0);
            this.toolStripMenu.Name = "toolStripMenu";
            this.toolStripMenu.Padding = new System.Windows.Forms.Padding(0, 0, 2, 0);
            this.toolStripMenu.Size = new System.Drawing.Size(1686, 31);
            this.toolStripMenu.TabIndex = 4;
            this.toolStripMenu.Text = "toolStrip1";
            // 
            // LoadDataButton
            // 
            this.LoadDataButton.Image = ((System.Drawing.Image)(resources.GetObject("LoadDataButton.Image")));
            this.LoadDataButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.LoadDataButton.Name = "LoadDataButton";
            this.LoadDataButton.Size = new System.Drawing.Size(122, 28);
            this.LoadDataButton.Text = "Load Entities";
            this.LoadDataButton.Click += new System.EventHandler(this.LoadDataButton_Click);
            // 
            // fetchXML
            // 
            this.fetchXML.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.fetchXML.Image = ((System.Drawing.Image)(resources.GetObject("fetchXML.Image")));
            this.fetchXML.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.fetchXML.Name = "fetchXML";
            this.fetchXML.Size = new System.Drawing.Size(29, 28);
            this.fetchXML.Text = "FetchXML";
            this.fetchXML.Visible = false;
            // 
            // tsbSample
            // 
            this.tsbSample.Name = "tsbSample";
            this.tsbSample.Size = new System.Drawing.Size(29, 28);
            // 
            // loadAuditButton
            // 
            this.loadAuditButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.loadAuditButton.Location = new System.Drawing.Point(18, 237);
            this.loadAuditButton.Name = "loadAuditButton";
            this.loadAuditButton.Size = new System.Drawing.Size(264, 60);
            this.loadAuditButton.TabIndex = 5;
            this.loadAuditButton.Text = "Load Audit History";
            this.loadAuditButton.UseVisualStyleBackColor = true;
            this.loadAuditButton.Click += new System.EventHandler(this.LoadAuditHistoryButton);
            // 
            // rollbackbutton
            // 
            this.rollbackbutton.BackColor = System.Drawing.Color.MediumSeaGreen;
            this.rollbackbutton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rollbackbutton.Location = new System.Drawing.Point(18, 323);
            this.rollbackbutton.Name = "rollbackbutton";
            this.rollbackbutton.Size = new System.Drawing.Size(264, 65);
            this.rollbackbutton.TabIndex = 8;
            this.rollbackbutton.Text = "Rollback";
            this.rollbackbutton.UseVisualStyleBackColor = false;
            this.rollbackbutton.Click += new System.EventHandler(this.rollbackbutton_Click);
            // 
            // entitiesList
            // 
            this.entitiesList.FormattingEnabled = true;
            this.entitiesList.Location = new System.Drawing.Point(18, 84);
            this.entitiesList.Name = "entitiesList";
            this.entitiesList.Size = new System.Drawing.Size(264, 24);
            this.entitiesList.TabIndex = 9;
            // 
            // recordGuid
            // 
            this.recordGuid.Location = new System.Drawing.Point(18, 135);
            this.recordGuid.MaxLength = 38;
            this.recordGuid.Name = "recordGuid";
            this.recordGuid.Size = new System.Drawing.Size(264, 22);
            this.recordGuid.TabIndex = 10;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(15, 64);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(43, 17);
            this.label1.TabIndex = 11;
            this.label1.Text = "Entity";
            // 
            // guidLabel
            // 
            this.guidLabel.AutoSize = true;
            this.guidLabel.Location = new System.Drawing.Point(15, 115);
            this.guidLabel.Name = "guidLabel";
            this.guidLabel.Size = new System.Drawing.Size(88, 17);
            this.guidLabel.TabIndex = 12;
            this.guidLabel.Text = "Record Guid";
            // 
            // showNewestValues
            // 
            this.showNewestValues.AutoSize = true;
            this.showNewestValues.Location = new System.Drawing.Point(18, 182);
            this.showNewestValues.Name = "showNewestValues";
            this.showNewestValues.Size = new System.Drawing.Size(229, 21);
            this.showNewestValues.TabIndex = 13;
            this.showNewestValues.Text = "Show most recent audit records";
            this.showNewestValues.UseVisualStyleBackColor = true;
            this.showNewestValues.Visible = false;
            this.showNewestValues.CheckedChanged += new System.EventHandler(this.showNewestValues_CheckedChanged);
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Location = new System.Drawing.Point(304, 45);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.ReadOnly = true;
            this.dataGridView1.RowHeadersWidth = 51;
            this.dataGridView1.RowTemplate.Height = 24;
            this.dataGridView1.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridView1.Size = new System.Drawing.Size(1632, 721);
            this.dataGridView1.TabIndex = 7;
            // 
            // MyPluginControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.showNewestValues);
            this.Controls.Add(this.guidLabel);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.recordGuid);
            this.Controls.Add(this.entitiesList);
            this.Controls.Add(this.rollbackbutton);
            this.Controls.Add(this.loadAuditButton);
            this.Controls.Add(this.toolStripMenu);
            this.Controls.Add(this.dataGridView1);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "MyPluginControl";
            this.Size = new System.Drawing.Size(1686, 636);
            this.Load += new System.EventHandler(this.MyPluginControl_Load);
            this.toolStripMenu.ResumeLayout(false);
            this.toolStripMenu.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ToolStrip toolStripMenu;
        private System.Windows.Forms.ToolStripButton tsbSample;
        private System.Windows.Forms.Button loadAuditButton;
        private System.Windows.Forms.Button rollbackbutton;
        private System.Windows.Forms.ComboBox entitiesList;
        private System.Windows.Forms.ToolStripButton LoadDataButton;
        private System.Windows.Forms.TextBox recordGuid;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label guidLabel;
        private System.Windows.Forms.CheckBox showNewestValues;
        private System.Windows.Forms.ToolStripButton fetchXML;
        private System.Windows.Forms.DataGridView dataGridView1;
    }
}
