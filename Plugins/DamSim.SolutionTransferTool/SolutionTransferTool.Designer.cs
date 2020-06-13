﻿using System.Windows.Forms;

namespace DamSim.SolutionTransferTool
{
    partial class SolutionTransferTool
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SolutionTransferTool));
            this.tsMain = new System.Windows.Forms.ToolStrip();
            this.tsbLoadSolutions = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.tsbTransfertSolution = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.tsbSwitchOrgs = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.tsbFindMissingDependencies = new System.Windows.Forms.ToolStripButton();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.dpMain = new WeifenLuo.WinFormsUI.Docking.DockPanel();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.tsbExportSolutions = new System.Windows.Forms.ToolStripButton();
            this.tsMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // tsMain
            // 
            this.tsMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsbLoadSolutions,
            this.toolStripSeparator2,
            this.tsbTransfertSolution,
            this.toolStripSeparator4,
            this.tsbSwitchOrgs,
            this.toolStripSeparator3,
            this.tsbFindMissingDependencies,
            this.toolStripSeparator1,
            this.tsbExportSolutions});
            this.tsMain.Location = new System.Drawing.Point(0, 0);
            this.tsMain.Name = "tsMain";
            this.tsMain.Padding = new System.Windows.Forms.Padding(0, 0, 2, 0);
            this.tsMain.Size = new System.Drawing.Size(1600, 39);
            this.tsMain.TabIndex = 0;
            this.tsMain.Text = "tsMain";
            // 
            // tsbLoadSolutions
            // 
            this.tsbLoadSolutions.Image = ((System.Drawing.Image)(resources.GetObject("tsbLoadSolutions.Image")));
            this.tsbLoadSolutions.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbLoadSolutions.Name = "tsbLoadSolutions";
            this.tsbLoadSolutions.Size = new System.Drawing.Size(208, 36);
            this.tsbLoadSolutions.Text = "Load Solutions";
            this.tsbLoadSolutions.Click += new System.EventHandler(this.TsbLoadSolutionsClick);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 39);
            // 
            // tsbTransfertSolution
            // 
            this.tsbTransfertSolution.Image = ((System.Drawing.Image)(resources.GetObject("tsbTransfertSolution.Image")));
            this.tsbTransfertSolution.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbTransfertSolution.Name = "tsbTransfertSolution";
            this.tsbTransfertSolution.Size = new System.Drawing.Size(228, 36);
            this.tsbTransfertSolution.Text = "Transfer solution";
            this.tsbTransfertSolution.Click += new System.EventHandler(this.TsbTransfertSolutionClick);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(6, 39);
            // 
            // tsbSwitchOrgs
            // 
            this.tsbSwitchOrgs.Image = global::DamSim.SolutionTransferTool.Properties.Resources.arrow_switch;
            this.tsbSwitchOrgs.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbSwitchOrgs.Name = "tsbSwitchOrgs";
            this.tsbSwitchOrgs.Size = new System.Drawing.Size(270, 36);
            this.tsbSwitchOrgs.Text = "Switch organizations";
            this.tsbSwitchOrgs.ToolTipText = "Switch source and target organizations";
            this.tsbSwitchOrgs.Click += new System.EventHandler(this.tsbSwitchOrgs_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 39);
            // 
            // tsbFindMissingDependencies
            // 
            this.tsbFindMissingDependencies.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsbFindMissingDependencies.Enabled = false;
            this.tsbFindMissingDependencies.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbFindMissingDependencies.Name = "tsbFindMissingDependencies";
            this.tsbFindMissingDependencies.Size = new System.Drawing.Size(313, 36);
            this.tsbFindMissingDependencies.Text = "Find Missing Dependencies";
            this.tsbFindMissingDependencies.ToolTipText = "Use this button to detect what component were missing for the previous failed sol" +
    "ution import";
            this.tsbFindMissingDependencies.Click += new System.EventHandler(this.tsbFindMissingDependencies_Click);
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "DamSimIcon.png");
            // 
            // dpMain
            // 
            this.dpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dpMain.DocumentStyle = WeifenLuo.WinFormsUI.Docking.DocumentStyle.DockingWindow;
            this.dpMain.Location = new System.Drawing.Point(0, 39);
            this.dpMain.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            this.dpMain.Name = "dpMain";
            this.dpMain.Size = new System.Drawing.Size(1600, 1267);
            this.dpMain.TabIndex = 1;
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 39);
            // 
            // tsbExportSolutions
            // 
            this.tsbExportSolutions.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsbExportSolutions.Enabled = false;
            this.tsbExportSolutions.Image = ((System.Drawing.Image)(resources.GetObject("tsbExportSolutions.Image")));
            this.tsbExportSolutions.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbExportSolutions.Name = "tsbExportSolutions";
            this.tsbExportSolutions.Size = new System.Drawing.Size(332, 36);
            this.tsbExportSolutions.Text = "Download exported solutions";
            this.tsbExportSolutions.Click += new System.EventHandler(this.tsbExportSolutions_Click);
            // 
            // SolutionTransferTool
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.dpMain);
            this.Controls.Add(this.tsMain);
            this.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            this.Name = "SolutionTransferTool";
            this.Size = new System.Drawing.Size(1600, 1306);
            this.tsMain.ResumeLayout(false);
            this.tsMain.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

 
        #endregion

        private System.Windows.Forms.ToolStrip tsMain;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.ToolStripButton tsbLoadSolutions;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton tsbTransfertSolution;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private ToolStripButton tsbFindMissingDependencies;
        private ToolStripSeparator toolStripSeparator4;
        private ToolStripButton tsbSwitchOrgs;
        private WeifenLuo.WinFormsUI.Docking.DockPanel dpMain;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripButton tsbExportSolutions;
    }
}
