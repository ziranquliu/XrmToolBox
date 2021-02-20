using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using XrmToolBox.Extensibility;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;
using McTools.Xrm.Connection;
using XrmToolBox.Extensibility.Interfaces;
using XrmToolBox.Extensibility.Args;
using DMM365.DataContainers;
using DMM365.Helper;
using System.IO;

namespace AttachmentsReversibleCopy
{
    public partial class CrmFieldsAttacmentsReversibleCopy : PluginControlBase, IXrmToolBoxPluginControl, IStatusBarMessenger, INoConnectionRequired, IPayPalPlugin, IHelpPlugin
    {

        #region declarations

        //private Settings mySettings;
        public event EventHandler OnRequestConnection;

        private ConnectionDetail sourceDetail;
        private ConnectionDetail targetDetail;
        IOrganizationService sourceService;
        IOrganizationService targetService;

        bool isLog = false;

        #endregion declarations


        #region IPayPalPlugin

        string IPayPalPlugin.DonationDescription => "Thanks in advance";

        string IPayPalPlugin.EmailAccount => "michael.kalinov@gmail.com";

        #endregion IPayPalPlugin


        #region IHelpPlugin

        string IHelpPlugin.HelpUrl => "https://github.com/mkalinov/DynamicsCrm_AttachmentsReversibleCopy";

        #endregion IHelpPlugin


        #region IStatusBarMessenger

        public event EventHandler<StatusBarMessageEventArgs> SendMessageToStatusBar;

        #endregion IStatusBarMessenger


        #region PluginControlBase

        public CrmFieldsAttacmentsReversibleCopy()
        {
            InitializeComponent();
        }

        private void CrmFieldsAttacmentsReversibleCopy_Load(object sender, EventArgs e)
        {
            //  ShowInfoNotification("This is a notification that can lead to XrmToolBox repository", new Uri("https://github.com/MscrmTools/XrmToolBox"));

        }

        private void tsbClose_Click(object sender, EventArgs e)
        {
            CloseTool();
        }


        /// <summary>
        /// This event occurs when the plugin is closed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CrmFieldsAttacmentsReversibleCopy_OnCloseTool(object sender, EventArgs e)
        {

        }


        /// <summary>
        /// This event occurs when the connection has been updated in XrmToolBox
        /// </summary>
        public override void UpdateConnection(IOrganizationService newService, ConnectionDetail detail, string actionName = "", object parameter = null)
        {

            if (actionName == "Target")
            {
                targetService = newService;
                targetDetail = detail;
                displayConnectionDetails(detail, "Target");
            }
            else
            {
                sourceService = newService;
                sourceDetail = detail;
                displayConnectionDetails(detail, "Source");
            }
            
        }


        #endregion PluginControlBase


        #region Connection Change

        private void btnChangeEnvironment_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            if (ReferenceEquals(btn, null))
            {
                MessageBox.Show("Error on environment change");
                return;
            }
            if (OnRequestConnection != null)
            {
                var arg = new RequestConnectionEventArgs
                {
                    ActionName = btn.Name.IndexOf("Target") > -1 ? "Target" : "Source",
                    Control = this
                };
                OnRequestConnection(this, arg);
            }
        }


        #endregion Connection Change


        #region  Attachments Copier

        private void cbxAttachments_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            if (ReferenceEquals(cb, null)) return;

            lblTutorial1.Visible = lblTutorial2.Visible = lblTutorial3.Visible = lblTutorial4.Visible = linkDataUtility.Visible = cb.Checked;

            if (!cb.Checked)
            {
                reSetUI();
            }
            else
            {
                cbxReverseCopy.Checked =  !cb.Checked;
                groupSelectFile.Visible = groupBackupIDs.Visible = groupAttachmentsCopySettings.Visible = groupSource.Visible = cb.Checked;
            }

        }

        private void cbxReverseCopy_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            if (ReferenceEquals(cb, null)) return;

            btnRollBack.Visible = cbxReverseCopy.Checked;
            lblReverse1.Visible = lblReverse2.Visible = cb.Checked;

            if (!cb.Checked)
            {
                reSetUI();
            }
            else
            {
                cbxAttachments.Checked = groupSelectFile.Visible = groupAttachmentsCopySettings.Visible = groupSource.Visible = !cb.Checked;
                groupBackupIDs.Visible = cb.Checked;
            }

        }


        private void btnSelectPackage_Click(object sender, EventArgs e)
        {
            fileDialogResult(getCMfile, txtPackageUrl);

            //create directory and unzip in there
            if (!GlobalHelper.isValidString(txtPackageUrl.Text)) return;
            else
            {
                try
                {
                    string unpackDirName = Path.GetFileNameWithoutExtension(txtPackageUrl.Text) + "_" + DateTime.Now.ToString("MM-dd-yyyy HH:mm").Replace(':', '-');
                    string unpackDirPath = Path.Combine(Path.GetDirectoryName(txtPackageUrl.Text), unpackDirName);
                    //uppack dir
                    //if directory exists: check with user if ok to delete the content
                    if (Directory.Exists(unpackDirPath))
                    {
                        if (DialogResult.Yes == MessageBox.Show($"The directory {unpackDirPath} already exists. Is it ok to delete its content and uppack zip there?", "Confirm existing directory clean", MessageBoxButtons.YesNo))
                        {
                            Directory.Delete(Path.GetDirectoryName(unpackDirPath));
                        }
                        else
                        {
                            throw new Exception("Please change zip file name to create anoter directory.");
                        }
                    }
                    //create dir
                    DirectoryInfo unPackDir = IOHelper.createDirectory(unpackDirPath);
                    //unzip package to new dir
                    ArchiveHelper.ExtractZipToDirectory(txtPackageUrl.Text, unPackDir.FullName);
                    //check if data.xml exists
                    string dataXmlPath = Path.Combine(unPackDir.FullName, "data.xml");
                    if (!File.Exists(dataXmlPath)) throw new Exception($"No 'data.xml' found at {unPackDir.FullName}");
                    else txtPackageUrl.Text = dataXmlPath;

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    txtPackageUrl.Text = string.Empty;
                    return;
                }

            }
        }

        private void cbxAttachmentsKeepIDs_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            if (ReferenceEquals(cb, null)) return;

            if (!cb.Checked)
            {
                txtAttachmentsIDsBackUpFile.Text = string.Empty;
                return;
            }

            groupBackupIDs.Visible = cb.Checked;

            MessageBox.Show("Select/create an xml file to keep list of created IDs");
            fileDialogResult(setBackupFile, txtAttachmentsIDsBackUpFile);
        }

        private void btnCopyAttachments_Click(object sender, EventArgs e)
        {
            CopyAttachments();
        }

        private void btnRollBack_Click(object sender, EventArgs e)
        {
            RollBackAttachments();
        }

        private void btnSelectIDsBackup_Click(object sender, EventArgs e)
        {
            fileDialogResult(setBackupFile, txtAttachmentsIDsBackUpFile);
        }

        private void groupBackupIDs_VisibleChanged(object sender, EventArgs e)
        {
            GroupBox gb = sender as GroupBox;
            if (ReferenceEquals(gb, null)) return;

            btnRollBack.Visible = cbxReverseCopy.Checked;

        }

        private void linkDataUtility_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://docs.microsoft.com/en-us/dynamics365/customer-engagement/admin/manage-configuration-data");
        }



        #endregion Attachments Copier


        #region Helpers

        private void CopyAttachments()
        {
            bool keepIds = cbxAttachmentsKeepIDs.Checked;
            bool includeNotes = cbxIncludeTextNotes.Checked;
            string message = string.Empty;
            List<Guid> ids = new List<Guid>();

            //validate data file
            if (!GlobalHelper.isValidString(txtPackageUrl.Text) || !IOHelper.isFileExist(txtPackageUrl.Text, ".xml"))
            {
                MessageBox.Show("Data file was not found");
                return;
            }

            if (keepIds)
            {
                if (!GlobalHelper.isValidString(txtAttachmentsIDsBackUpFile.Text) || !IOHelper.isFileExist(txtAttachmentsIDsBackUpFile.Text, ".xml"))
                {
                    MessageBox.Show("Attachment IDs backup file was not found", "File Not Found");
                    return;
                }
            }

            
            if (ReferenceEquals(sourceDetail, null) || ReferenceEquals(targetDetail, null))
            {
                MessageBox.Show("Please establish connections first.", "NO CONNECTION");
                return;

            }

            //vlidate not the same portal for same connections
            if (sourceDetail.ConnectionName == targetDetail.ConnectionName)
            {
                MessageBox.Show("Copy from source to  itself is not supported. Please select different target.", "WRONG SET");
                return;
            }


            if (DialogResult.Yes == MessageBox.Show("You're going to copy attachments \r\n from '" + sourceDetail.OrganizationFriendlyName + "' crm \r\n to '" + targetDetail.OrganizationFriendlyName + "' crm based on Configuration migration data file '" + txtPackageUrl.Text + "'. \r\n Recording of newly created attachemnts for potential rollback is " + (keepIds ? "'ON'" : "'OFF'") + " \r\n\n Execute?", "CONFIRM COPY", MessageBoxButtons.YesNo))
            {
                string datafilePath = txtPackageUrl.Text;
                string bkLocation = txtAttachmentsIDsBackUpFile.Text;
                int counter = 0;
                int filesCount = 0;
                int filesFailed = 0;
                int tottalcount = 0;
                int tottalMatchesFoundInTarget = 0;
                isLog = false;

                WorkAsync(new WorkAsyncInfo
                {
                    Message = $"Copy has begun ...",
                    Work = (bw, e) =>
                    {

                        List<Guid> result = new List<Guid>();
                        DataEntities entities = IOHelper.DeserializeXmlFromFile<DataEntities>(datafilePath); //attachments

                        //get total count
                        foreach (DataEntity de in entities.entities)
                        {
                            if(!ReferenceEquals(de.RecordsCollection, null))
                                tottalcount += de.RecordsCollection.Count;
                        }

                        //check if empty
                        if (tottalcount == 0)
                        {
                            message = "Selected package has no records";
                            throw new Exception ("Selected package has no records");
                        }


                        foreach (DataEntity de in entities.entities)
                        {
                            counter += de.RecordsCollection.Count;
                            //get attachment per entity record, files only, get lates
                            foreach (Record rec in de.RecordsCollection)
                            {
                                Entity latestAttacnment = CrmHelper.getLattestAttachmentByEntity(sourceService, new Guid(rec.id), de.name, includeNotes);
                                if (ReferenceEquals(latestAttacnment, null)) continue;

                                //check is target has same entity
                                Entity targetMaster = targetService.Retrieve(de.name, new Guid(rec.id), new ColumnSet());
                                if (ReferenceEquals(targetMaster, null)) continue;
                                else tottalMatchesFoundInTarget++;

                                //copy to target
                                try
                                {
                                    Guid? newNote = CrmHelper.cloneAnnotation(targetService, latestAttacnment);
                                    if (newNote.HasValue) result.Add(newNote.Value);
                                    filesCount++;

                                }
                                catch (Exception ex)
                                {
                                    LogError(ex.Message);
                                    isLog = true;
                                    filesFailed++;
                                    continue;
                                }
                            }
                            decimal p = (counter / tottalcount) * 100;
                            int? progress = (int?)Math.Round(p, 0);
                            SendMessageToStatusBar?.Invoke(this, new StatusBarMessageEventArgs(progress, "Copy in progress ..."));

                        }

                        if (ids.Count > 0 && keepIds) IOHelper.SerializeObjectToXmlFile<List<Guid>>(ids, bkLocation);

                        message = $" Records tottal = {tottalcount}, \r\n Attachments total = {counter},\r\n total matches in target crm = {tottalMatchesFoundInTarget},\r\n copied = {filesCount}, \r\n failed = {filesFailed} \r\n Attachments backed up - {ids.Count}";

                    },
                    PostWorkCallBack = e =>
                    {
                        if (e.Error != null)
                        {
                            MessageBox.Show(this, e.Error.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                        else if (isLog)
                        {
                            MessageBox.Show(this, "There were some errors on the execution. Please see log. ", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            isLog = false;
                            return;
                        }
                        MessageBox.Show(message, "RESULT");

                    },
                    ProgressChanged = e => { SetWorkingMessage(e.UserState.ToString()); }
                });
            }

        }
        
        private void RollBackAttachments()
        {
            List<Guid> ids = new List<Guid>();
            //identify type of action
            bool keepIds = cbxAttachmentsKeepIDs.Checked;
            bool includeNotes = cbxIncludeTextNotes.Checked;
            string message = string.Empty;
            //validate backup file   
            if (!GlobalHelper.isValidString(txtAttachmentsIDsBackUpFile.Text) || !IOHelper.isFileExist(txtAttachmentsIDsBackUpFile.Text, ".xml"))
            {
                MessageBox.Show("Attachment IDs backup file was not found");
                return;
            }

            if (DialogResult.Yes == MessageBox.Show("You are going to remove attachments from '" + targetDetail.OrganizationFriendlyName + "' crm \r\n " + " based on lest of ids  lokated in '" + txtAttachmentsIDsBackUpFile.Text + "' file. \r\n\n Execute?", "CONFIRM DELETE?", MessageBoxButtons.YesNo))
            {
                int counter = 0;
                int filesFailed = 0;
                isLog = false;


                WorkAsync(new WorkAsyncInfo
                {
                    Message = $"Rolling back Web Files attachments",
                    Work = (bw, e) =>
                    {

                        ids = IOHelper.DeserializeXmlFromFile<List<Guid>>(txtAttachmentsIDsBackUpFile.Text);
                    //execute
                    foreach (Guid item in ids)
                        {
                            try
                            {

                                targetService.Delete("annotation", item);
                                counter++;

                                decimal p = (counter / ids.Count) * 100;
                                int? progress = (int?)Math.Round(p, 0);
                                SendMessageToStatusBar?.Invoke(this, new StatusBarMessageEventArgs(progress, "Copy in progress ..."));
                            }
                            catch (Exception ex)
                            {
                                LogError(ex.Message);
                                isLog = true;
                                filesFailed++;
                                continue;
                            }
                        }
                        message = $"Total - {ids.Count.ToString()}, \r\n attachments removed from  {targetDetail.OrganizationFriendlyName} - {counter}, \r\n failed - {filesFailed}";
                    },
                    PostWorkCallBack = e =>
                    {
                        if (e.Error != null)
                        {
                            MessageBox.Show(this, e.Error.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                        else if (isLog)
                        {
                            MessageBox.Show(this, "There were some errors on the execution. Please see log. ", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            isLog = false;
                            return;
                        }

                        MessageBox.Show(message, "RESULT");

                    },
                    ProgressChanged = e => { SetWorkingMessage(e.UserState.ToString()); }
                });

            }
        }

        private void fileDialogResult(OpenFileDialog ofd, TextBox target)
        {
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                target.Text = ofd.FileName;
            }
        }
        
        private void displayConnectionDetails(ConnectionDetail detail, string serviceType)
        {
            switch (serviceType)
            {
                case "Source":
                    linkSource.Text = "Source CRM:   {0} \r\n{1}";
                    linkSource.Text = string.Format(linkSource.Text, detail.ConnectionName, detail.WebApplicationUrl);
                    linkSource.Visible = true;
                    break;

                case "Target":
                    linkTarget.Text = "Target CRM:   {0} \r\n{1}";
                    linkTarget.Text = string.Format(linkTarget.Text, detail.ConnectionName, detail.WebApplicationUrl);
                    linkTarget.Visible = true;
                    break;
            }
        }

        private void populateDropDown(ComboBox cb, List<CrmEntityContainer> ds)
        {
            cb.DataSource = ds;
            cb.DisplayMember = "name";
            cb.ValueMember = "id";
            cb.SelectedItem = null;

        }

        private void cleanAttachmentsSetting(bool _checked)
        {
            foreach (Control item in groupAttachmentsCopySettings.Controls)
            {
                if (item is CheckBox) ((CheckBox)item).Checked = _checked;
            }
        }

        private void reSetUI()
        {

            if (!cbxAttachments.Checked && !cbxReverseCopy.Checked)
            {
                cleanAttachmentsSetting(cbxReverseCopy.Checked);
                groupAttachmentsCopySettings.Visible = groupBackupIDs.Visible = cbxReverseCopy.Checked;
                groupSource.Visible = !cbxReverseCopy.Checked;

                lblReverse1.Visible = lblReverse2.Visible = cbxReverseCopy.Checked;

                lblTutorial1.Visible = lblTutorial2.Visible = lblTutorial3.Visible = lblTutorial4.Visible = linkDataUtility.Visible = cbxAttachments.Checked;
            }
        }

        #endregion Helpers

    }
}