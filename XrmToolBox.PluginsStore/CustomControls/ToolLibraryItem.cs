using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using XrmToolBox.PluginsStore.DTO;

namespace XrmToolBox.PluginsStore.CustomControls
{
    public partial class ToolLibraryItem : UserControl
    {
        public ToolLibraryItem(XtbPlugin tool)
        {
            InitializeComponent();

            lblTitle.Text = tool.Name;
            lblAuthor.Text = tool.Authors;
            lblDescription.Text = tool.Description;
            lblVersion.Text = $@"v{tool.Version}";

            SetLogo(tool);
            // DisplayRatings(tool.AverageFeedbackRating);
            //RegisterEvents(this);
        }

        private void ClickItem(object sender, EventArgs e)
        {
            MessageBox.Show("Clicked!");
        }

        private void DisplayRatings(decimal rating)
        {
            pbStar.Visible = false;
            if (rating > 0)
            {
                var sourceBmp = new Bitmap(ilImages24.Images[0]);
                Rectangle srcRect = new Rectangle(0, 0, Convert.ToInt32(Math.Truncate(rating / 5 * 120)), 24);
                Bitmap cropped = sourceBmp.Clone(srcRect, sourceBmp.PixelFormat);

                pbStar.Image = cropped;
                pbStar.Visible = true;
            }
        }

        private void RegisterEvents(Control parentControl)
        {
            foreach (var ctrl in parentControl.Controls.OfType<Label>())
            {
                ctrl.Click += ClickItem;
                ctrl.Cursor = Cursors.Hand;
            }
            foreach (var ctrl in parentControl.Controls.OfType<PictureBox>())
            {
                ctrl.Click += ClickItem;
                ctrl.Cursor = Cursors.Hand;
            }
            foreach (var ctrl in parentControl.Controls.OfType<Panel>())
            {
                ctrl.Click += ClickItem;
                ctrl.Cursor = Cursors.Hand;

                RegisterEvents(ctrl);
            }
        }

        private void SetLogo(XtbPlugin tool)
        {
            try
            {
                pbLogo.Load(tool.LogoUrl ?? "https://raw.githubusercontent.com/wiki/MscrmTools/XrmToolBox/Images/unknown.png");
            }
            catch
            {
                byte[] bytes = Convert.FromBase64String(
                    "iVBORw0KGgoAAAANSUhEUgAAAEAAAABACAMAAACdt4HsAAAAA3NCSVQICAjb4U/gAAAACXBIWXMAAAFzAAABcwHEdCJ9AAAAGXRFWHRTb2Z0d2FyZQB3d3cuaW5rc2NhcGUub3Jnm+48GgAAAVlQTFRF////AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAXF73CwAAAHJ0Uk5TAAEDBAUGBwsPEBEUFRYXGxwkJyorLjAxNzk+P0BBRkdKS01TVVlaW1xfYWJjZWhpbHBxcnZ3eHp8f4KGh4iJioyRkpOUnaOkpaaorrCxsrO2uLq7vsLDztLV2Nnc3d7f4+bn6Ort8fL09vf4+fr7/P3+Afu7TgAAAwJJREFUWMOtl+lbElEUxg+YImpkmZHaBm65oNJi4YZUSpFpOSgJpqYpiIL4+/8/9EHZvHNnxmc8n+ZyloeZ+77vOUdEY75wNJ5MZ/P5bDoZj4Z9civrmlgv0mTF9Ykup9mt09uVWmK5XHusbE23Okj3jOwBUDJikVAw4PEEgqFIzCgBsDfisct/kQE4SQ37m3/3D6dOADIvLNO7DYDckOlfbR3KARjd+vy+Q+BoqkXnb5k6Ag77dP6xMyjELC/MFyvA2Zipz7sM7PbYfaSeXWDZa5K/Bhgd9tfUYQBraoVlIOF1ghNvAlhW3h+YcQq1GeDGd+g7g4RzqCfgrOkuug/B8Dov4DXgsBEPBux23IZtHbtgNOAXCmb31z+/eXy8Od9vdpsFqKHak4GYGtO+UuXhSrvqjUGmyqwROFLxFzwAOD0FOAiqmDyCkWuS7MGUEtC2A+dzvS0tvXPnsNOmBEzB3hXppiGn8mcR9nuvHnv3YVFlVg6mRURkG4ZUYl9wOVA9DFxyoZJ4CLZFRLoqnKj8H4TV+mkVBlV9OKHSJSKTkFI/8gKM10/jsKDGpGBSRDZgWHX+hMf10xP4pcYMw4aIr0jJrzrfXG41nF7CVzXGX6Lok3ATJuv2oFF/P8I7kxgDwhI1ReEN0PxpwG0zGqMSh4hdgc/w24yrEYhLEkLW6Z0p4LmZJwRJSUPQSkM/fD8Gvpg6g5CWLAT0+U8BuHhr7g1AVvKULfrde6D045muj5bJ2xSYhW+d+kZcJm/zCrMwq/cGIGvzEa0LBCFtc43WBUKQtAGSdYEIxG2gPAqjem8MoloyXSvj0lKb3mtAWEdnJ3ZFZ42gOLErQdFImhO7ljRzUa2BTQ/Tqqiay3pVTXPZ1zpfVdY1jUVERO6fQ1HDhXpjMW9tVajBK3NfvbVpmquISOs/+HvPXCfrzVXX3kVEHn1KPNSisNbetQOGldQVmoTa7YjjfshyPea5HzTdj7ruh23X4777heMOVh73S5f7te8OFk/3q+8dLN+3W///A5bFM9Y/bySSAAAAAElFTkSuQmCC");

                Image errorImage;
                using (MemoryStream ms = new MemoryStream(bytes))
                {
                    errorImage = Image.FromStream(ms);
                }

                pbLogo.ErrorImage = errorImage;
            }
        }
    }
}