namespace XrmToolBox.PluginsStore.CustomControls
{
    partial class ToolLibraryItem
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ToolLibraryItem));
            this.pnlLeft = new System.Windows.Forms.Panel();
            this.pbLogo = new System.Windows.Forms.PictureBox();
            this.pnlRight = new System.Windows.Forms.Panel();
            this.pbStar = new System.Windows.Forms.PictureBox();
            this.lblVersion = new System.Windows.Forms.Label();
            this.pnlMain = new System.Windows.Forms.Panel();
            this.pnlMiddleBottom = new System.Windows.Forms.Panel();
            this.lblDescription = new System.Windows.Forms.Label();
            this.lblAuthor = new System.Windows.Forms.Label();
            this.pnlMiddleTop = new System.Windows.Forms.Panel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.ilImages24 = new System.Windows.Forms.ImageList(this.components);
            this.pnlFooterSpace = new System.Windows.Forms.Panel();
            this.pnlLeft.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbLogo)).BeginInit();
            this.pnlRight.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbStar)).BeginInit();
            this.pnlMain.SuspendLayout();
            this.pnlMiddleBottom.SuspendLayout();
            this.pnlMiddleTop.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlLeft
            // 
            this.pnlLeft.Controls.Add(this.pbLogo);
            this.pnlLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlLeft.Location = new System.Drawing.Point(0, 0);
            this.pnlLeft.Name = "pnlLeft";
            this.pnlLeft.Padding = new System.Windows.Forms.Padding(10);
            this.pnlLeft.Size = new System.Drawing.Size(125, 140);
            this.pnlLeft.TabIndex = 0;
            // 
            // pbLogo
            // 
            this.pbLogo.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.pbLogo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pbLogo.Location = new System.Drawing.Point(10, 10);
            this.pbLogo.Name = "pbLogo";
            this.pbLogo.Size = new System.Drawing.Size(105, 120);
            this.pbLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbLogo.TabIndex = 0;
            this.pbLogo.TabStop = false;
            // 
            // pnlRight
            // 
            this.pnlRight.Controls.Add(this.pbStar);
            this.pnlRight.Controls.Add(this.lblVersion);
            this.pnlRight.Dock = System.Windows.Forms.DockStyle.Right;
            this.pnlRight.Location = new System.Drawing.Point(866, 0);
            this.pnlRight.Name = "pnlRight";
            this.pnlRight.Size = new System.Drawing.Size(140, 140);
            this.pnlRight.TabIndex = 1;
            // 
            // pbStar
            // 
            this.pbStar.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.pbStar.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pbStar.Image = ((System.Drawing.Image)(resources.GetObject("pbStar.Image")));
            this.pbStar.Location = new System.Drawing.Point(0, 110);
            this.pbStar.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            this.pbStar.MaximumSize = new System.Drawing.Size(140, 30);
            this.pbStar.MinimumSize = new System.Drawing.Size(140, 30);
            this.pbStar.Name = "pbStar";
            this.pbStar.Size = new System.Drawing.Size(140, 30);
            this.pbStar.TabIndex = 1;
            this.pbStar.TabStop = false;
            this.pbStar.Visible = false;
            // 
            // lblVersion
            // 
            this.lblVersion.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblVersion.Location = new System.Drawing.Point(0, 0);
            this.lblVersion.Name = "lblVersion";
            this.lblVersion.Size = new System.Drawing.Size(140, 49);
            this.lblVersion.TabIndex = 0;
            this.lblVersion.Text = "label1";
            this.lblVersion.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pnlMain
            // 
            this.pnlMain.Controls.Add(this.pnlMiddleBottom);
            this.pnlMain.Controls.Add(this.pnlMiddleTop);
            this.pnlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlMain.Location = new System.Drawing.Point(125, 0);
            this.pnlMain.Name = "pnlMain";
            this.pnlMain.Size = new System.Drawing.Size(741, 140);
            this.pnlMain.TabIndex = 2;
            // 
            // pnlMiddleBottom
            // 
            this.pnlMiddleBottom.Controls.Add(this.lblDescription);
            this.pnlMiddleBottom.Controls.Add(this.lblAuthor);
            this.pnlMiddleBottom.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlMiddleBottom.Location = new System.Drawing.Point(0, 52);
            this.pnlMiddleBottom.Name = "pnlMiddleBottom";
            this.pnlMiddleBottom.Size = new System.Drawing.Size(741, 88);
            this.pnlMiddleBottom.TabIndex = 1;
            // 
            // lblDescription
            // 
            this.lblDescription.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblDescription.Location = new System.Drawing.Point(0, 37);
            this.lblDescription.Name = "lblDescription";
            this.lblDescription.Padding = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this.lblDescription.Size = new System.Drawing.Size(741, 51);
            this.lblDescription.TabIndex = 2;
            this.lblDescription.Text = "label1";
            // 
            // lblAuthor
            // 
            this.lblAuthor.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblAuthor.Location = new System.Drawing.Point(0, 0);
            this.lblAuthor.Name = "lblAuthor";
            this.lblAuthor.Padding = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this.lblAuthor.Size = new System.Drawing.Size(741, 37);
            this.lblAuthor.TabIndex = 1;
            this.lblAuthor.Text = "label1";
            // 
            // pnlMiddleTop
            // 
            this.pnlMiddleTop.Controls.Add(this.lblTitle);
            this.pnlMiddleTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlMiddleTop.Location = new System.Drawing.Point(0, 0);
            this.pnlMiddleTop.Name = "pnlMiddleTop";
            this.pnlMiddleTop.Size = new System.Drawing.Size(741, 52);
            this.pnlMiddleTop.TabIndex = 0;
            // 
            // lblTitle
            // 
            this.lblTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitle.Location = new System.Drawing.Point(0, 0);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(741, 52);
            this.lblTitle.TabIndex = 1;
            this.lblTitle.Text = "label1";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // ilImages24
            // 
            this.ilImages24.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("ilImages24.ImageStream")));
            this.ilImages24.TransparentColor = System.Drawing.Color.Transparent;
            this.ilImages24.Images.SetKeyName(0, "star24_5.png");
            // 
            // pnlFooterSpace
            // 
            this.pnlFooterSpace.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlFooterSpace.Location = new System.Drawing.Point(0, 140);
            this.pnlFooterSpace.Name = "pnlFooterSpace";
            this.pnlFooterSpace.Size = new System.Drawing.Size(1006, 10);
            this.pnlFooterSpace.TabIndex = 3;
            // 
            // ToolLibraryItem
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pnlMain);
            this.Controls.Add(this.pnlRight);
            this.Controls.Add(this.pnlLeft);
            this.Controls.Add(this.pnlFooterSpace);
            this.Name = "ToolLibraryItem";
            this.Size = new System.Drawing.Size(1006, 150);
            this.pnlLeft.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pbLogo)).EndInit();
            this.pnlRight.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pbStar)).EndInit();
            this.pnlMain.ResumeLayout(false);
            this.pnlMiddleBottom.ResumeLayout(false);
            this.pnlMiddleTop.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlLeft;
        private System.Windows.Forms.Panel pnlRight;
        private System.Windows.Forms.Label lblVersion;
        private System.Windows.Forms.Panel pnlMain;
        private System.Windows.Forms.Panel pnlMiddleBottom;
        private System.Windows.Forms.Panel pnlMiddleTop;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.PictureBox pbLogo;
        private System.Windows.Forms.ImageList ilImages24;
        private System.Windows.Forms.PictureBox pbStar;
        private System.Windows.Forms.Label lblDescription;
        private System.Windows.Forms.Label lblAuthor;
        private System.Windows.Forms.Panel pnlFooterSpace;
    }
}
