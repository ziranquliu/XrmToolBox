﻿using DamSim.SolutionTransferTool.AppCode;
using DamSim.SolutionTransferTool.Forms;
using McTools.Xrm.Connection;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.WebServiceClient;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using XrmToolBox.Extensibility;
using XrmToolBox.Extensibility.Interfaces;

namespace DamSim.SolutionTransferTool
{
    public partial class SolutionTransferTool : MultipleConnectionsPluginControlBase, IGitHubPlugin, IHelpPlugin
    {
        #region Variables

        private readonly MainForm mForm;
        private readonly ProgressForm pForm;
        private OrganizationRequest currentRequest;
        private string lastConnectionName;
        private Guid lastImportId;
        private IOrganizationService lastTargetService;
        private Dictionary<OrganizationRequest, ProgressItem> progressItems;
        private Settings settings;
        private SettingsForm sForm;
        private ConnectionDetail sourceDetail;
        private IOrganizationService sourceService;
        private System.Timers.Timer timer = new System.Timers.Timer();
        private List<BaseToProcess> toProcessList = new List<BaseToProcess>();

        #endregion Variables

        #region Constructor

        public SolutionTransferTool()
        {
            InitializeComponent();

            dpMain.Theme = new VS2015LightTheme();

            mForm = new MainForm();
            mForm.TargetOrganizationRemoved += MForm_TargetOrganizationRemoved;
            mForm.TargetOrganizationRequested += MForm_TargetOrganizationRequested;
            mForm.Show(dpMain, DockState.Document);

            pForm = new ProgressForm();
            pForm.OnRetry += PForm_OnRetry;
            pForm.Show(dpMain, DockState.DockRight);

            sForm = new SettingsForm();
            sForm.Show(dpMain, DockState.DockRight);
        }

        private void PForm_OnRetry(object sender, EventArgs e)
        {
            Retry();
        }

        #endregion Constructor

        #region Forms events callback

        private void MForm_TargetOrganizationRemoved(object sender, TargetOrganizationsEventArgs e)
        {
            var toRemove = AdditionalConnectionDetails.FirstOrDefault(c => !e.TargetOrganizations.Contains(c));
            RemoveAdditionalOrganization(toRemove);
        }

        private void MForm_TargetOrganizationRequested(object sender, EventArgs e)
        {
            AddAdditionalOrganization();
        }

        #endregion Forms events callback

        #region XrmToolbox

        public string HelpUrl => "https://github.com/MscrmTools/DamSim.SolutionTransferTool/wiki";
        public string RepositoryName => "DamSim.SolutionTransferTool";

        public string UserName => "MscrmTools";

        public override void ClosingPlugin(PluginCloseInfo info)
        {
            settings.Save(ConnectionDetail?.ConnectionName);

            base.ClosingPlugin(info);
        }

        public override void UpdateConnection(IOrganizationService newService, ConnectionDetail detail, string actionName, object parameter)
        {
            if (actionName == "AdditionalOrganization")
            {
                AdditionalConnectionDetails.Add(detail);

                if (newService is OrganizationServiceProxy proxy)
                {
                    proxy.Timeout = detail.Timeout;
                }
                else if (newService is OrganizationWebProxyClient client)
                {
                    client.InnerChannel.OperationTimeout = detail.Timeout;
                }

                mForm.DisplayTargetOrganizations(AdditionalConnectionDetails.ToList());
            }
            else
            {
                settings?.Save(ConnectionDetail?.ConnectionName);

                ConnectionDetail = detail;
                sourceDetail = detail;
                sourceService = newService;
                RetrieveSolutions();

                if (!SettingsManager.Instance.TryLoad(GetType(), out settings, ConnectionDetail.ConnectionName))
                {
                    settings = new Settings();
                }

                sForm.Settings = settings;
                mForm.SetSourceOrganization(detail);

                base.UpdateConnection(newService, detail, actionName, parameter);
            }
        }

        protected override void ConnectionDetailsUpdated(NotifyCollectionChangedEventArgs e)
        {
            mForm.DisplayTargetOrganizations(AdditionalConnectionDetails.ToList());
        }

        #endregion XrmToolbox

        #region UI Events

        private void Pi_LogFileRequested(object sender, DownloadLogEventArgs e)
        {
            DownloadLogFile(e.ImportJobId, e.Service);
        }

        private void tsbFindMissingDependencies_Click(object sender, EventArgs e)
        {
            var child = new MissingComponentsForm();
            child.ShowMissingComponents(ParentForm, lastTargetService, lastConnectionName, sourceService, lastImportId);
        }

        private void TsbLoadSolutionsClick(object sender, EventArgs e)
        {
            ExecuteMethod(RetrieveSolutions);
        }

        private void tsbSwitchOrgs_Click(object sender, EventArgs e)
        {
            if (AdditionalConnectionDetails.Count > 1)
            {
                MessageBox.Show(this,
                    @"Switch can only be performed when no more than one target organization is defined",
                    @"Warning",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            var tempDetail = sourceDetail;
            sourceDetail = AdditionalConnectionDetails.FirstOrDefault();
            ConnectionDetail = AdditionalConnectionDetails.FirstOrDefault();
            AdditionalConnectionDetails.Clear();

            if (tempDetail != null)
            {
                AdditionalConnectionDetails.Add(tempDetail);
            }

            mForm.SwitchSourceAndTarget(tempDetail, sourceDetail);

            if (sourceDetail != null)
            {
                sourceService = sourceDetail.GetCrmServiceClient();
                base.UpdateConnection(sourceService, sourceDetail, "", null);
                RetrieveSolutions();
            }
        }

        private void TsbTransfertSolutionClick(object sender, EventArgs e)
        {
            if (mForm.SelectedSolutions.Count == 0 || !AdditionalConnectionDetails.Any())
            {
                MessageBox.Show(@"You have to select a source solution and a target organization to continue.", @"Warning",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var solutionsToTransfer = PreparareSolutionsToTransfer();
            if (solutionsToTransfer.Count == 0)
            {
                return;
            }

            progressItems = new Dictionary<OrganizationRequest, ProgressItem>();
            toProcessList = new List<BaseToProcess>();

            foreach (var solution in solutionsToTransfer)
            {
                string newVersion = GetUpdatedSolutionVersion(solution);

                if (settings.UpdateSourceSolutionVersionNew == UpdateVersionEnum.Yes
                    || settings.UpdateSourceSolutionVersionNew == UpdateVersionEnum.Prompt &&
                    DialogResult.Yes == MessageBox.Show(this, $@"Do you want to update version for solution {solution.GetAttributeValue<string>("friendlyname")} ?

Current version: {solution.GetAttributeValue<string>("version")}
New version: {newVersion}",
                        @"Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
                {
                    solution["version"] = newVersion;

                    Service.Update(solution);

                    mForm.UpdateSolutionVersion(solution);
                }

                var exportItem = new ExportToProcess
                {
                    Solution = solution,
                    Previous = toProcessList.OfType<ExportToProcess>().LastOrDefault(),
                    Request = PrepareExportRequest(solution),
                    Detail = sourceDetail
                };
                toProcessList.Add(exportItem);

                foreach (var detail in AdditionalConnectionDetails)
                {
                    toProcessList.Add(new ImportToProcess
                    {
                        Solution = solution,
                        Previous = toProcessList.OfType<ImportToProcess>().LastOrDefault(x => x.Detail == detail),
                        Export = exportItem,
                        Request = PrepareImportRequest(detail, solution),
                        Detail = detail
                    });
                }
            }

            if (!settings.Managed && settings.Publish)
            {
                foreach (var detail in AdditionalConnectionDetails)
                {
                    toProcessList.Add(new PublishToProcess
                    {
                        Request = PreparePublishRequest(detail),
                        Detail = detail
                    });
                }
            }

            // Add items to progress form
            pForm.Items = progressItems.Values.ToList();
            pForm.Start();

            pForm.Show(dpMain, DockState.DockRight);

            StartExport(toProcessList.OfType<ExportToProcess>().First());

            timer.Elapsed += Timer_Elapsed;
            timer.Interval = settings.RefreshIntervalProp.TotalMilliseconds;
            timer.Start();
        }

        #endregion UI Events

        #region Methods

        private void DownloadLogFile(Guid importJobId, IOrganizationService service)
        {
            var dialog = new FolderBrowserDialog();

            if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.LastFolderUsed))
                dialog.SelectedPath = Properties.Settings.Default.LastFolderUsed;

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                Properties.Settings.Default.LastFolderUsed = dialog.SelectedPath;
                Properties.Settings.Default.Save();

                ToggleWaitMode(true);

                WorkAsync(new WorkAsyncInfo
                {
                    Message = "Downloading log file",
                    Work = (sender, e) =>
                    {
                        var importLogRequest = new RetrieveFormattedImportJobResultsRequest
                        {
                            ImportJobId = importJobId
                        };
                        var importLogResponse = (RetrieveFormattedImportJobResultsResponse)service.Execute(importLogRequest);

                        var filePath = $@"{dialog.SelectedPath}\{DateTime.Now:yyyy_MM_dd__HH_mm}.xml";
                        File.WriteAllText(filePath, importLogResponse.FormattedResults);

                        e.Result = filePath;
                    },
                    PostWorkCallBack = e =>
                    {
                        if (e.Error != null)
                        {
                            var message = string.Format("An error was encountered while downloading the log file.{0}Error:{0}{1}",
                                Environment.NewLine, e.Error.Message);
                            MessageBox.Show(message, @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        else
                        {
                            if (
                                MessageBox.Show(
                                    $@"Download completed!

Would you like to open the file now ({e.Result})?

(Microsoft Excel is required)",
                                    @"File Download", MessageBoxButtons.YesNo, MessageBoxIcon.Information) ==
                                DialogResult.Yes)
                            {
                                Process.Start("Excel.exe", e.Result.ToString());
                            }
                        }

                        ToggleWaitMode(false);
                    }
                });
            }
        }

        private string GetUpdatedSolutionVersion(Entity etpSolution)
        {
            string version = etpSolution.GetAttributeValue<string>("version");
            var versionParts = version.Split('.');
            switch (settings.VersionSchema)
            {
                case VersionType.Major:
                    versionParts[0] = (int.Parse(versionParts[0]) + 1).ToString();
                    break;

                case VersionType.Minor:
                    if (versionParts.Length < 2) break;
                    versionParts[1] = (int.Parse(versionParts[1]) + 1).ToString();
                    break;

                case VersionType.Build:
                    if (versionParts.Length < 3) break;
                    versionParts[2] = (int.Parse(versionParts[2]) + 1).ToString();
                    break;

                case VersionType.Revision:
                    if (versionParts.Length < 4) break;
                    versionParts[3] = (int.Parse(versionParts[3]) + 1).ToString();
                    break;
            }

            return string.Join(".", versionParts);
        }

        private List<Entity> PreparareSolutionsToTransfer()
        {
            var solutionsToTransfer = new List<Entity>();
            if (mForm.SelectedSolutions.Count > 1)
            {
                // Open dialog to order solutions import
                foreach (var solution in mForm.SelectedSolutions)
                {
                    solutionsToTransfer.Add(solution);
                }

                var dialog = new SolutionOrderDialog(solutionsToTransfer);
                if (dialog.ShowDialog(ParentForm) == DialogResult.OK)
                {
                    solutionsToTransfer = dialog.Solutions;
                }
                else
                {
                    return new List<Entity>();
                }
            }
            else
            {
                solutionsToTransfer.Add(mForm.SelectedSolutions.First());
            }

            return solutionsToTransfer;
        }

        private ExportSolutionRequest PrepareExportRequest(Entity solution, ExportSolutionRequest request = null)
        {
            var isNull = request == null;
            if (isNull)
            {
                request = new ExportSolutionRequest();
            }

            request.Managed = settings.Managed;
            request.SolutionName = solution.GetAttributeValue<string>("uniquename");
            request.ExportAutoNumberingSettings = settings.ExportAutoNumberingSettings;
            request.ExportCalendarSettings = settings.ExportCalendarSettings;
            request.ExportCustomizationSettings = settings.ExportCustomizationSettings;
            request.ExportEmailTrackingSettings = settings.ExportEmailTrackingSettings;
            request.ExportGeneralSettings = settings.ExportGeneralSettings;
            request.ExportIsvConfig = settings.ExportIsvConfig;
            request.ExportMarketingSettings = settings.ExportMarketingSettings;
            request.ExportOutlookSynchronizationSettings = settings.ExportOutlookSynchronizationSettings;
            request.ExportRelationshipRoles = settings.ExportRelationshipRoles;
            request.ExportSales = settings.ExportSales;

            if (ConnectionDetail.OrganizationMajorVersion >= 8)
            {
                request.ExportExternalApplications = settings.ExportExternalApplications;
            }

            if (isNull)
            {
                progressItems.Add(request, new ProgressItem
                {
                    Type = Enumerations.RequestType.Export,
                    Detail = sourceDetail,
                    Solution = solution.GetAttributeValue<string>("friendlyname"),
                    SolutionVersion = solution.GetAttributeValue<string>("version"),
                    Request = request
                });
            }

            return request;
        }

        private ImportSolutionRequest PrepareImportRequest(ConnectionDetail detail, Entity solution, ImportSolutionRequest request = null)
        {
            var isNull = request == null;
            if (isNull)
            {
                request = new ImportSolutionRequest();
            }

            request.ConvertToManaged = settings.ConvertToManaged;
            request.OverwriteUnmanagedCustomizations = settings.OverwriteUnmanagedCustomizations;
            request.PublishWorkflows = settings.PublishWorkflows;
            request.ImportJobId = Guid.NewGuid();

            if (ConnectionDetail.OrganizationMajorVersion >= 8)
            {
                request.HoldingSolution = settings.HoldingSolution;
                request.SkipProductUpdateDependencies = settings.SkipProductUpdateDependencies;
            }

            //if (ConnectionDetail.OrganizationMajorVersion >= 9 && ConnectionDetail.OrganizationMinorVersion >= 1)
            //{
            //    request.AsyncRibbonProcessing
            //}

            if (isNull)
            {
                var pi = new ProgressItem
                {
                    Type = Enumerations.RequestType.Import,
                    Detail = detail,
                    Solution = solution.GetAttributeValue<string>("friendlyname"),
                    SolutionVersion = solution.GetAttributeValue<string>("version"),
                    Request = request
                };
                pi.LogFileRequested += Pi_LogFileRequested;
                progressItems.Add(request, pi);
            }

            return request;
        }

        private PublishAllXmlRequest PreparePublishRequest(ConnectionDetail detail)
        {
            var request = new PublishAllXmlRequest();
            progressItems.Add(request, new ProgressItem
            {
                Type = Enumerations.RequestType.Publish,
                Detail = detail,
                Request = request
            });

            return request;
        }

        /// <summary>
        /// Retrieves unmanaged solutions from the source organization
        /// </summary>
        private void RetrieveSolutions()
        {
            var sourceSolutionsQuery = new QueryExpression
            {
                EntityName = "solution",
                ColumnSet = new ColumnSet("publisherid", "installedon", "version", "uniquename", "friendlyname", "description"),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression("ismanaged", ConditionOperator.Equal, false),
                        new ConditionExpression("isvisible", ConditionOperator.Equal, true),
                        new ConditionExpression("uniquename", ConditionOperator.NotEqual, "Default")
                    }
                }
            };

            var solutions = sourceService.RetrieveMultiple(sourceSolutionsQuery);

            mForm.DisplaySolutions(solutions.Entities.ToList());
        }

        private void StartExport(ExportToProcess etp)
        {
            if (settings.AutoExportSolutionsToDisk)
            {
                if (!Directory.Exists(settings.AutoExportSolutionsFolderPath))
                {
                    MessageBox.Show(this,
                        $@"Folder {settings.AutoExportSolutionsFolderPath} does not exist! Please update settings",
                        @"Warning",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    sForm.Show(dpMain, sForm.DockState);
                    return;
                }
            }

            ToggleWaitMode(true);

            progressItems[etp.Request].Solution = etp.Solution.GetAttributeValue<string>("friendlyname");
            progressItems[etp.Request].SolutionVersion = etp.Solution.GetAttributeValue<string>("version");
            progressItems[etp.Request].Start();

            WorkAsync(new WorkAsyncInfo
            {
                Message = "",
                Work = (bw, evt) =>
                {
                    etp.IsProcessing = true;
                    etp.SolutionContent = ((ExportSolutionResponse)etp.Detail.GetCrmServiceClient().Execute(etp.Request))
                        .ExportSolutionFile;
                },
                PostWorkCallBack = evt =>
                {
                    etp.IsProcessed = true;
                    etp.IsProcessing = false;
                    etp.Succeeded = true;

                    if (evt.Error != null)
                    {
                        etp.Succeeded = false;

                        progressItems[etp.Request].Error(DateTime.Now);
                        pForm.ShowRetryButton(progressItems[etp.Request]);

                        ToggleWaitMode(false);
                    }
                    else
                    {
                        progressItems[etp.Request].Success(DateTime.Now);
                        progressItems[etp.Request].SolutionFile = etp.SolutionContent;
                    }

                    if (settings.AutoExportSolutionsToDisk)
                    {
                        var fileName =
                            $"{progressItems[etp.Request].Solution}_{progressItems[etp.Request].SolutionVersion.Replace(".", "_")}.zip";
                        var filePath = Path.Combine(settings.AutoExportSolutionsFolderPath, fileName);
                        try
                        {
                            File.WriteAllBytes(filePath, etp.SolutionContent);
                        }
                        catch (Exception error)
                        {
                            MessageBox.Show(this, $@"Error when saving solution {fileName} to disk.

{error.Message}", @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            });
        }

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            foreach (var etp in toProcessList.OfType<ExportToProcess>())
            {
                if (etp.IsProcessed == false && etp.IsProcessing == false)
                {
                    StartExport(etp);
                }

                foreach (var itp in toProcessList.OfType<ImportToProcess>()
                    .Where(i => i.Export == etp && i.Export.IsProcessed))
                {
                    if (itp.Previous != null && itp.Previous.IsProcessed == false || itp.IsProcessed)
                    {
                        continue;
                    }

                    if (itp.IsProcessing == false && itp.IsProcessed == false)
                    {
                        progressItems[itp.Request].Solution = itp.Solution.GetAttributeValue<string>("friendlyname");
                        progressItems[itp.Request].Start();

                        ((ImportSolutionRequest)itp.Request).CustomizationFile = itp.Export.SolutionContent;

                        itp.AsyncOperationId = ((ExecuteAsyncResponse)itp.Detail.GetCrmServiceClient().Execute(new ExecuteAsyncRequest
                        {
                            Request = itp.Request
                        })).AsyncJobId;
                        itp.IsProcessing = true;
                    }
                    else if (itp.IsProcessing)
                    {
                        var task = itp.Detail.GetCrmServiceClient().RetrieveMultiple(new QueryExpression("asyncoperation")
                        {
                            NoLock = true,
                            ColumnSet = new ColumnSet(true),
                            Criteria =
                                {
                                    Conditions=
                                    {
                                        new ConditionExpression("asyncoperationid", ConditionOperator.Equal, itp.AsyncOperationId)
                                    }
                                }
                        }).Entities.FirstOrDefault();

                        if (task != null)
                        {
                            if (task.GetAttributeValue<OptionSetValue>("statecode")?.Value == 3)
                            {
                                itp.IsProcessed = true;
                                itp.IsProcessing = false;
                                if (task.GetAttributeValue<OptionSetValue>("statuscode")?.Value == 30)
                                {
                                    progressItems[itp.Request].Success(task.GetAttributeValue<DateTime>("completedon").ToLocalTime());
                                    itp.Succeeded = true;
                                }
                                else
                                {
                                    progressItems[itp.Request].Error(task.GetAttributeValue<DateTime>("completedon").ToLocalTime());
                                    ToggleWaitMode(false);
                                    timer.Stop();
                                    pForm.ShowRetryButton(progressItems[itp.Request]);
                                }

                                if (toProcessList.All(tp => tp.IsProcessed))
                                {
                                    timer.Stop();
                                    ToggleWaitMode(false);
                                }
                            }
                            else
                            {
                                var job = itp.Detail.GetCrmServiceClient().RetrieveMultiple(new QueryExpression("importjob")
                                {
                                    NoLock = true,
                                    ColumnSet = new ColumnSet(true),
                                    Criteria =
                                    {
                                        Conditions=
                                        {
                                            new ConditionExpression("importjobid", ConditionOperator.Equal, ((ImportSolutionRequest)itp.Request).ImportJobId)
                                        }
                                    }
                                }).Entities.FirstOrDefault();

                                if (job != null)
                                {
                                    progressItems[itp.Request]
                                        .ReportProgress(job.GetAttributeValue<double>("progress"));
                                }
                            }
                        }
                    }
                }

                foreach (var ptp in toProcessList.OfType<PublishToProcess>())
                {
                    if (toProcessList.OfType<ImportToProcess>()
                            .Where(i => i.Detail == ptp.Detail)
                            .All(i => i.IsProcessed)
                        && ptp.IsProcessed == false)
                    {
                        progressItems[ptp.Request].Start();

                        WorkAsync(new WorkAsyncInfo
                        {
                            Message = "",
                            Work = (bw, evt) =>
                            {
                                ptp.IsProcessing = true;
                                ptp.Detail.GetCrmServiceClient().Execute(ptp.Request);
                                ptp.IsProcessed = true;
                                ptp.IsProcessing = false;
                            },
                            PostWorkCallBack = evt =>
                            {
                                if (evt.Error != null)
                                    progressItems[ptp.Request].Error(DateTime.Now);
                                else
                                    progressItems[ptp.Request].Success(DateTime.Now);

                                if (toProcessList.All(tp => tp.IsProcessed))
                                {
                                    timer.Stop();
                                    ToggleWaitMode(false);
                                }
                            }
                        });
                    }
                }
            }
        }

        #endregion Methods

        private void Retry()
        {
            if (DialogResult.Yes != MessageBox.Show(this, @"Are you sure you want to retry last failed action?",
                    @"Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
                return;

            var firstNotSucceededProcess = toProcessList.FirstOrDefault(x => x.Succeeded == false);
            if (firstNotSucceededProcess == null)
            {
                return;
            }

            if (firstNotSucceededProcess is ExportToProcess etp)
            {
                etp.IsProcessed = false;
                StartExport(etp);
            }
            else if (firstNotSucceededProcess is ImportToProcess itp)
            {
                itp.IsProcessed = false;
                progressItems[itp.Request].Start();
            }

            foreach (var ep in toProcessList.OfType<ExportToProcess>().Where(x => x.IsProcessed == false))
            {
                PrepareExportRequest(ep.Solution, (ExportSolutionRequest)ep.Request);
            }

            foreach (var ip in toProcessList.OfType<ImportToProcess>().Where(x => x.IsProcessed == false))
            {
                PrepareImportRequest(ip.Detail, ip.Solution, (ImportSolutionRequest)ip.Request);
            }

            ToggleWaitMode(true);

            timer.Elapsed += Timer_Elapsed;
            timer.Interval = settings.RefreshIntervalProp.TotalMilliseconds;
            timer.Start();
        }

        private void ToggleWaitMode(bool on)
        {
            Invoke(new Action(() =>
            {
                if (on)
                {
                    tsbTransfertSolution.Enabled = false;
                    tsbLoadSolutions.Enabled = false;
                    tsbFindMissingDependencies.Enabled = false;
                    tsbSwitchOrgs.Enabled = false;
                    tsbExportSolutions.Enabled = false;
                }
                else
                {
                    tsbTransfertSolution.Enabled = true;
                    tsbLoadSolutions.Enabled = true;
                    tsbFindMissingDependencies.Enabled = true;
                    tsbSwitchOrgs.Enabled = true;
                    tsbExportSolutions.Enabled = toProcessList.OfType<ExportToProcess>().Any(etp =>
                        etp.SolutionContent != null);
                }
            }));
        }

        private void tsbExportSolutions_Click(object sender, EventArgs e)
        {
            var cfd = new CustomFolderBrowserDialog();
            if (cfd.ShowDialog(Parent) == DialogResult.OK)
            {
                foreach (var etp in toProcessList.OfType<ExportToProcess>())
                {
                    if (etp.SolutionContent != null)
                    {
                        string filename = Path.Combine(cfd.FolderPath,
                            $"{etp.Solution.GetAttributeValue<string>("uniquename")}_{etp.Solution.GetAttributeValue<string>("version").Replace(".", "_")}{(((ExportSolutionRequest)etp.Request).Managed ? "_managed" : "")}.zip");
                        File.WriteAllBytes(filename, etp.SolutionContent);
                    }
                }

                MessageBox.Show(this, $@"Solution(s) saved to {cfd.FolderPath}", @"Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}