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
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using System.Windows.Data;
using System.ServiceModel;
using System.Threading;

namespace AuditHistoryRollBack
{
    public partial class MyPluginControl : PluginControlBase
    {
        private Settings mySettings;
        private string[] ValidActions =
            { "Update",
              "Activate",
              "Deactivate",
              "Assign"
            }; //https://docs.microsoft.com/en-us/dynamics365/customer-engagement/web-api/audit?view=dynamics-ce-odata-9
        private List<AuditRecord> AuditRecords = new List<AuditRecord>();
        private List<string> newestAuditRecords = new List<string>();
        private List<int> oldAuditRecords = new List<int>();
        private BindingSource bindingSource;
        private Entity TargetEntity;

        public MyPluginControl()
        {
            InitializeComponent();
        }

        private void MyPluginControl_Load(object sender, EventArgs e)
        {
            // Loads or creates the settings for the plugin
            if (!SettingsManager.Instance.TryLoad(GetType(), out mySettings))
            {
                mySettings = new Settings();

                LogWarning("Settings not found => a new settings file has been created!");
            }

            else
            {
                LogInfo("Settings found and loaded");
            }
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
        private void MyPluginControl_OnCloseTool(object sender, EventArgs e)
        {
            // Before leaving, save the settings
            SettingsManager.Instance.Save(GetType(), mySettings);
        }

        /// <summary>
        /// This event occurs when the connection has been updated in XrmToolBox
        /// </summary>
        public override void UpdateConnection(IOrganizationService newService, ConnectionDetail detail, string actionName, object parameter)
        {
            base.UpdateConnection(newService, detail, actionName, parameter);

            if (mySettings != null && detail != null)
            {
                mySettings.LastUsedOrganizationWebappUrl = detail.WebApplicationUrl;
                LogInfo("Connection has changed to: {0}", detail.WebApplicationUrl);
            }
        }

        private void WhoAmI()
        {
            Service.Execute(new WhoAmIRequest());
        }

        public class AuditRecord
        {
            public Object AuditId { get; set; }
            public Object CreatedOn { get; set; }
            public string UserId { get; set; }
            public string Action { get; set; }
            public string Field { get; set; }
            public string OldValue { get; set; }
            public string NewValue { get; set; }
            public object Type { get; set; }
            public string FieldMetaData { get; set; }
            public string LookupId { get; set; }

        }

        public void LoadEntities()
        {
            if (entitiesList != null)
            {
                entitiesList.Items.Clear();
            }
            disableButtons();
            WorkAsync(new WorkAsyncInfo
            {
                Message = "Loading Entities...",
                Work = (bw, e) =>
                {
                    RetrieveAllEntitiesRequest request = new RetrieveAllEntitiesRequest
                    {
                        EntityFilters = EntityFilters.Entity,
                        RetrieveAsIfPublished = true

                    };
                    RetrieveAllEntitiesResponse response = (RetrieveAllEntitiesResponse)Service.Execute(request);

                    foreach (EntityMetadata entity in response.EntityMetadata)
                    {
                        if (entity.IsAuditEnabled.Value == true)
                        {
                            entitiesList.Items.Add(entity.LogicalName);
                        }
                    }
                },
                PostWorkCallBack = (e) =>
                {
                    entitiesList.Sorted = true;
                    entitiesList.SelectedIndex = 0;
                    enableButtons();
                }
            });
        }

        private void LoadAuditHistoryButton(object sender, EventArgs e)
        {
            LoadAuditHistory();
        }

        private void LoadAuditHistory()
        {
            if (entitiesList.SelectedItem != null)
            {
                ClearAuditHistory();
                disableButtons();

                WorkAsync(new WorkAsyncInfo
                {
                    Message = "Loading Audit History...",
                    Work = (bw, d) => {
                        LoadAuditHistoryLogic();
                    },
                    PostWorkCallBack = (d) =>
                    {
                        ShowAuditHistory();
                        enableButtons();
                    }
                });
            }
            else
            {
                MessageBox.Show("You have not selected an entity");
            }
        }

        private void LoadAuditHistoryLogic()
        {
            if (Service != null)
            {
                RetrieveRecordChangeHistoryRequest changeRequest = new RetrieveRecordChangeHistoryRequest();
                try
                {
                    Guid guid = TrimGuid(recordGuid.Text);

                    changeRequest.Target = new EntityReference(entitiesList.SelectedItem.ToString(), guid);
                    RetrieveRecordChangeHistoryResponse changeResponse =
                    (RetrieveRecordChangeHistoryResponse)Service.Execute(changeRequest);

                    AuditDetailCollection auditDetailCollection = changeResponse.AuditDetailCollection;

                    Entity entity = Service.Retrieve(entitiesList.SelectedItem.ToString(), guid, new ColumnSet(true));

                    foreach (var attrAuditDetail in auditDetailCollection.AuditDetails)
                    {
                        var detailType = attrAuditDetail.GetType();
                        if (detailType == typeof(AttributeAuditDetail))
                        {
                            var auditRecord = attrAuditDetail.AuditRecord;
                            var attributeDetail = (AttributeAuditDetail)attrAuditDetail;

                            var newValueEntity = attributeDetail.NewValue;
                            var oldValueEntity = attributeDetail.OldValue;
                            TargetEntity = new Entity(oldValueEntity.LogicalName, oldValueEntity.Id);

                            var action = auditRecord.FormattedValues["action"];
                            if (Array.Exists(ValidActions, x => x == action))
                            {

                                foreach (KeyValuePair<String, object> attribute in attributeDetail.NewValue.Attributes)
                                {
                                    string fieldmetadata = "";
                                    string lookupId = "";
                                    string oldValue = "";
                                    string newValue = "";

                                    if (attributeDetail.OldValue.Contains(attribute.Key))
                                    {
                                        lookupId = GetLookupId(attribute.Key, attribute.Value, oldValueEntity, attribute.Key);
                                        fieldmetadata = GetFieldMetaData(attribute.Key, attribute.Value, oldValueEntity, attribute.Key);
                                        oldValue = GetFieldValue(attribute.Key, attribute.Value, oldValueEntity, oldValueEntity[attribute.Key].ToString());
                                    }
                                    newValue = GetFieldValue(attribute.Key, attribute.Value, newValueEntity, newValueEntity[attribute.Key].ToString());

                                    CreateAuditRecord(auditRecord.Attributes["auditid"], auditRecord.Attributes["createdon"], ((EntityReference)auditRecord.Attributes["userid"]).Name,
                                        auditRecord.FormattedValues["action"], attribute.Key, oldValue, newValue, attribute.Value, fieldmetadata, lookupId);
                                }

                                foreach (KeyValuePair<String, object> attribute in attributeDetail.OldValue.Attributes)
                                {
                                    if (!attributeDetail.NewValue.Contains(attribute.Key))
                                    {
                                        string fieldmetadata = "";
                                        string loookupId = "";
                                        string newValue = "";

                                        loookupId = GetLookupId(attribute.Key, attribute.Value, oldValueEntity, attribute.Key);
                                        fieldmetadata = GetFieldMetaData(attribute.Key, attribute.Value, oldValueEntity, attribute.Key);
                                        string oldValue = GetFieldValue(attribute.Key, attribute.Value, oldValueEntity, oldValueEntity[attribute.Key].ToString());

                                        CreateAuditRecord(auditRecord.Attributes["auditid"], auditRecord.Attributes["createdon"], ((EntityReference)auditRecord.Attributes["userid"]).Name,
                                          auditRecord.FormattedValues["action"], attribute.Key, oldValue, newValue, attribute.Value, fieldmetadata, loookupId);

                                    }
                                }
                            }
                        }
                    }
                }
                catch (InvalidPluginExecutionException ex)
                {
                    MessageBox.Show($"An error occurred: {ex.Message}");
                }
                catch (FaultException<OrganizationServiceFault> ex)
                {
                    MessageBox.Show($"An error occurred: {ex.Message}");
                }
                catch (TimeoutException ex)
                {
                    MessageBox.Show($"An error occurred: {ex.Message}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred: {ex.Message}");
                }
            }
        }

        private void UpdateMultipleRecords()
        {
            try
            {
                string fetchxml = string.Format(@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                              <entity name='{0}'>
                                                <attribute name='{1}' />
                                                <order attribute='modifiedon' descending='true' />
                                              </entity>
                                            </fetch>", entitiesList.SelectedItem.ToString(), entitiesList.SelectedItem.ToString() + "id");

                EntityCollection result = Service.RetrieveMultiple(new FetchExpression(fetchxml));

            }
            catch (InvalidPluginExecutionException ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
            catch (TimeoutException ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }

        private Guid TrimGuid(string guid)
        {
            string stringguid = guid.Replace("{", string.Empty).Replace("}", string.Empty);
            Guid updatedguid = new Guid(stringguid);
            return updatedguid;
        }

        private void ShowAuditHistory()
        {
            var bindingList = new BindingList<AuditRecord>(AuditRecords);
            bindingSource = new BindingSource(bindingList, null);
            dataGridView1.DataSource = bindingSource;

            dataGridView1.AutoResizeColumns();
            dataGridView1.ClearSelection();
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            dataGridView1.Columns[7].Visible = false;
            dataGridView1.Columns[8].Visible = false;
            dataGridView1.Columns[9].Visible = false;

            if (dataGridView1.RowCount > 0)
            {
                showNewestValues.Visible = true;
                showNewestValues.Checked = false;
            }
            else
            {
                showNewestValues.Visible = false;
                showNewestValues.Checked = false;
            }
        }

        private void ClearAuditHistory()
        {
            dataGridView1.Rows.Clear();
            AuditRecords.Clear();
        }

        private string GetFieldValue(string attribute, object type, Entity entity, string value)
        { 

            if (type is OptionSetValue)
            {
                return entity.FormattedValues[attribute];
            }
            else if (type is EntityReference)
            {
                return entity.GetAttributeValue<EntityReference>(attribute.ToString()).Name;
            }
            else if (type is Money)
            {
                return entity.GetAttributeValue<Money>(attribute.ToString()).Value.ToString();
            }
            else
            {
                return value;
            }
        }

        private string GetFieldMetaData(string attribute, object type, Entity entity, string attributeValue)
        {
            if (type is EntityReference)
            {
                EntityReference entityRef = (EntityReference)entity.Attributes[attribute];

                string EntityType = entityRef.LogicalName;
                return EntityType;
            }
            if (type is OptionSetValue)
            {
                if (attributeValue != null)
                {
                    string EntityType = entity.GetAttributeValue<OptionSetValue>(attribute).Value.ToString();
                    return EntityType;
                }
                return "";
            }
            else
            {
                return "";
            }
        }

        private string GetLookupId(string attribute, object type, Entity entity, string attributeValue)
        {
            if (type is EntityReference)
            {
                EntityReference entityRef = (EntityReference)entity.Attributes[attribute];

                string LookupId = entityRef.Id.ToString();
                return LookupId;
            }
            else
            {
                return "";
            }
        }

        private void CreateAuditRecord(object auditId, object createdon, string userid, string action, string field, string oldvalue, string newvalue, object type, string entitylookupname, string lookupid)
        {
            AuditRecords.Add(new AuditRecord
            {
                AuditId = auditId,
                CreatedOn = createdon,
                UserId = userid,
                Action = action,
                Field = field,
                OldValue = oldvalue,
                NewValue = newvalue,
                Type = type,
                FieldMetaData = entitylookupname,
                LookupId = lookupid,
            });
        }

        private void RollBackAudit(DataGridView dataGrid, Entity changeRequest)
        {
            WorkAsync(new WorkAsyncInfo
            {
                Message = "Rollingback...",
                Work = (bw, e) => {
                    try
                    {
                        foreach (DataGridViewRow auditinfo in dataGridView1.SelectedRows)
                        {
                            var type = auditinfo.Cells[7].Value;
                            var field = auditinfo.Cells[4].Value.ToString();
                            var newValue = auditinfo.Cells[6].Value.ToString();
                            var oldValue = auditinfo.Cells[5].Value.ToString();
                            var fieldmetadata = auditinfo.Cells[8].Value.ToString();
                            var lookupId = auditinfo.Cells[9].Value.ToString();
                            UpdateAudit(Service, changeRequest, type, field, newValue, oldValue, fieldmetadata, lookupId);
                        }
                        Service.Update(changeRequest);
                    }
                    catch (InvalidPluginExecutionException ex)
                    {
                        MessageBox.Show($"An error occurred: {ex.Message}");
                    }
                    catch (FaultException<OrganizationServiceFault> ex)
                    {
                        MessageBox.Show($"An error occurred: {ex.Message}");
                    }
                    catch (TimeoutException ex)
                    {
                        MessageBox.Show($"An error occurred: {ex.Message}");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"An error occurred: {ex.Message}");
                    }
                },
                PostWorkCallBack = (e) =>
                {
                    LoadAuditHistory();
                }
            }) ;
        }

        private void UpdateAudit(IOrganizationService service, Entity entity, object type, string field, string newvalue, string oldvalue, string fieldmetadata, string lookupId)
        {
            if (!string.IsNullOrEmpty(oldvalue as string))
            {
                switch (type)
                {
                    case OptionSetValue _:
                        entity.Attributes[field] = new OptionSetValue(Convert.ToInt32(fieldmetadata));
                        break;
                    case EntityReference _:
                        entity.Attributes[field] = new EntityReference(fieldmetadata, new Guid(lookupId));
                        break;
                    case Money _:
                        entity.Attributes[field] = new Money(Convert.ToDecimal(oldvalue));
                        break;
                    case bool _:
                        entity.Attributes[field] = Convert.ToBoolean(oldvalue);
                        break;
                    case int _:
                        entity.Attributes[field] = Convert.ToInt32(oldvalue);
                        break;
                    case DateTime _:
                        entity.Attributes[field] = Convert.ToDateTime(oldvalue);
                        break;
                    case decimal _:
                        entity.Attributes[field] = Convert.ToDecimal(oldvalue);
                        break;
                    case float _:
                        entity.Attributes[field] = float.Parse(oldvalue);
                        break;
                    case double _:
                        entity.Attributes[field] = Convert.ToDouble(oldvalue);
                        break;
                    default:
                        entity.Attributes[field] = oldvalue;
                        break;
                }
            }
            else
            {
                entity.Attributes[field] = null;
            }
        }     

        private void HideOldAuditRecords()
        {
            disableButtons();
            dataGridView1.ClearSelection();
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;

            if (showNewestValues.Checked == true)
            {
                WorkAsync(new WorkAsyncInfo
                {                   
                    Message = "Showing latest Audit Records...",
                    Work = (bw, k) =>
                    {
                        Thread.Sleep(500);
                        int index = 0;
                        foreach (DataGridViewRow auditinfo in dataGridView1.Rows)
                        {
                            var field = auditinfo.Cells[4].Value.ToString();
                            if (newestAuditRecords.Contains(field))
                            {
                                oldAuditRecords.Add(index);
                            }
                            else
                            {
                                newestAuditRecords.Add(field);
                            }
                            index++;
                        }
                        newestAuditRecords.Clear();
                    },
                    PostWorkCallBack = (k) =>
                    {
                        CurrencyManager currencyManager1 = (CurrencyManager)BindingContext[dataGridView1.DataSource];
                        currencyManager1.SuspendBinding();
                        foreach (var oldRecords in oldAuditRecords)
                        {
                            dataGridView1.Rows[oldRecords].Visible = false;
                        }
                        currencyManager1.ResumeBinding();
                        oldAuditRecords.Clear();
                    }
                });
            }
            else if (showNewestValues.Checked == false)
            {
                WorkAsync(new WorkAsyncInfo
                {
                    Message = "Showing all Audit Records...",
                    Work = (bw, k) =>
                    {
                        Thread.Sleep(500);
                    },
                    PostWorkCallBack = (k) =>
                    {
                        int index = 0;
                        foreach (DataGridViewRow auditinfo in dataGridView1.Rows)
                        {
                            dataGridView1.Rows[index].Visible = true;
                            index++;
                        }
                    }
                });
            }
            enableButtons();
        }     

        private void rollbackbutton_Click(object sender, EventArgs e)
        {
            if (entitiesList.SelectedItem != null)
            {
                if (dataGridView1.SelectedRows.Count > 0)
                {
                    RollBackAudit(dataGridView1, TargetEntity);
                }
                else
                {
                    MessageBox.Show("You have not selected any rows to rollback");
                }
            }
            else
            {
                MessageBox.Show("You have not selected an entity");
            }
        }

        private void LoadDataButton_Click(object sender, EventArgs e)
        {
            LoadEntities();
        }

        private void showNewestValues_CheckedChanged(object sender, EventArgs e)
        {
            HideOldAuditRecords();
        }

        private void disableButtons()
        {
            LoadDataButton.Enabled = false;
            showNewestValues.Enabled = false;
            rollbackbutton.Enabled = false;
            loadAuditButton.Enabled = false;
        }
        private void enableButtons()
        {
            LoadDataButton.Enabled = true;
            showNewestValues.Enabled = true;
            rollbackbutton.Enabled = true;
            loadAuditButton.Enabled = true;
        }
    }
}