using McTools.Xrm.Connection;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using SharpXrm.BulkAddressVerifier.BingService;
using SharpXrm.BulkAddressVerifier.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using XrmToolBox.Extensibility;
using Label = System.Windows.Forms.Label;

namespace SharpXrm.BulkAddressVerifier
{
	public class BulkVerifier : PluginControlBase
	{

	private int entityTypeCode;

	private IContainer components;

	private ToolStrip toolStripMenu;

	private Label label1;

	private Label label2;

	private TextBox tb_bingKey;

	private ComboBox entity;

	private Label label3;

	private Label label4;

	private ComboBox inputStreet;

	private ComboBox inputZip;

	private ComboBox inputCity;

	private ComboBox inputCounty;

	private ComboBox inputState;

	private ComboBox inputCountry;

	private Label label5;

	private Label label6;

	private Label label7;

	private Label label8;

	private Label label9;

	private Label label10;

	private ComboBox outputStreet;

	private ComboBox outputZip;

	private ComboBox outputCity;

	private ComboBox outputCounty;

	private ComboBox outputState;

	private ComboBox outputCountry;

	private ToolStripMenuItem RetrieveEntity;

	private ToolStripMenuItem copyInput;

	private Label label11;

	private ComboBox outputlongitude;

	private Label label12;

	private ComboBox outputLatitude;

	private Label label13;

	private ComboBox outputDetails;

	private ComboBox inputUnit;

	private Label label14;

	private ComboBox outputUnit;

	private Label label15;

	private CheckedListBox clb_viewList;

	private CheckedListBox clb_personalViewList;

	private ToolStripMenuItem bt_startValidation;

	private ToolStripMenuItem tsbClose;

	private RichTextBox rtb_validationStatus;

	private Label label16;

	private Label label17;

	private Label label18;

	private ToolStripSeparator toolStripSeparator1;

	private ToolStripSeparator toolStripSeparator2;

	private ToolStripSeparator toolStripSeparator3;

	public override void UpdateConnection(IOrganizationService newService, ConnectionDetail detail, string actionName, object parameter)
	{
		base.UpdateConnection(newService, detail, actionName, parameter);
		this.ClearAllField();
		this.entity.Text = "";
		this.entity.Items.Clear();
	}

	private void btnCancel_Click(object sender, EventArgs e)
	{
		base.CancelWorker();
		MessageBox.Show("Cancelled");
	}

	private void GetViews()
	{
		this.clb_viewList.Items.Clear();
		QueryExpression queryExpression = new QueryExpression();
		queryExpression.ColumnSet=(new ColumnSet(new string[]
		{
				"savedqueryid",
				"fetchxml",
				"name",
				"querytype",
				"isdefault",
				"returnedtypecode",
				"isquickfindquery"
		}));
		queryExpression.EntityName = ("savedquery");
		QueryExpression arg_102_0 = queryExpression;
		FilterExpression expr_74 = new FilterExpression();
		Collection<ConditionExpression> arg_A2_0 = expr_74.Conditions;
		ConditionExpression expr_7F = new ConditionExpression();
		expr_7F.AttributeName = ("querytype");
		expr_7F.Operator = (0);
		expr_7F.Values.Add(0);
		arg_A2_0.Add(expr_7F);
		Collection<ConditionExpression> arg_DA_0 = expr_74.Conditions;
		ConditionExpression expr_B2 = new ConditionExpression();
		expr_B2.AttributeName = ("returnedtypecode");
		expr_B2.Operator = (0);
		expr_B2.Values.Add(this.entityTypeCode);
		arg_DA_0.Add(expr_B2);
		Collection<ConditionExpression> arg_FD_0 = expr_74.Conditions;
		ConditionExpression expr_EA = new ConditionExpression();
		expr_EA.AttributeName=("fetchxml");
		expr_EA.Operator= ConditionOperator.NotNull;
		arg_FD_0.Add(expr_EA);
		arg_102_0.Criteria=(expr_74);
		QueryExpression query = queryExpression;
		RetrieveMultipleRequest expr_10F = new RetrieveMultipleRequest();
		expr_10F.Query=(query);
		RetrieveMultipleRequest retrieveMultipleRequest = expr_10F;
		foreach (Entity current in ((RetrieveMultipleResponse)base.Service.Execute(retrieveMultipleRequest)).EntityCollection.Entities)
		{
			ViewModel item = new ViewModel
			{
				Name = (string)current["name"],
				FetchXml = (string)current["fetchxml"]
			};
			this.clb_viewList.Items.Add(item);
		}
		queryExpression = new QueryExpression();
		queryExpression.ColumnSet=(new ColumnSet(new string[]
		{
				"userqueryid",
				"fetchxml",
				"name",
				"querytype",
				"returnedtypecode"
		}));
		queryExpression.EntityName=("userquery");
		QueryExpression arg_285_0 = queryExpression;
		FilterExpression expr_1F7 = new FilterExpression();
		Collection<ConditionExpression> arg_225_0 = expr_1F7.Conditions;
		ConditionExpression expr_202 = new ConditionExpression();
		expr_202.AttributeName=("querytype");
		expr_202.Operator=(0);
		expr_202.Values.Add(0);
		arg_225_0.Add(expr_202);
		Collection<ConditionExpression> arg_25D_0 = expr_1F7.Conditions;
		ConditionExpression expr_235 = new ConditionExpression();
		expr_235.AttributeName=("returnedtypecode");
		expr_235.Operator=(0);
		expr_235.Values.Add(this.entityTypeCode);
		arg_25D_0.Add(expr_235);
		Collection<ConditionExpression> arg_280_0 = expr_1F7.Conditions;
		ConditionExpression expr_26D = new ConditionExpression();
		expr_26D.AttributeName=("fetchxml");
		expr_26D.Operator= ConditionOperator.NotNull;
		arg_280_0.Add(expr_26D);
		arg_285_0.Criteria=(expr_1F7);
		QueryExpression query2 = queryExpression;
		RetrieveMultipleRequest expr_292 = new RetrieveMultipleRequest();
		expr_292.Query=(query2);
		RetrieveMultipleRequest retrieveMultipleRequest2 = expr_292;
		foreach (Entity current2 in ((RetrieveMultipleResponse)base.Service.Execute(retrieveMultipleRequest2)).EntityCollection.Entities)
		{
			ViewModel item2 = new ViewModel
			{
				Name = (string)current2["name"],
				FetchXml = (string)current2["fetchxml"]
			};
			this.clb_personalViewList.Items.Add(item2);
		}
	}

	private void PopulateInputOutputFields()
	{
		WorkAsyncInfo expr_06 = new WorkAsyncInfo();
		expr_06.Message=("Retrieving Entity Metadata");
		expr_06.Work=(delegate (BackgroundWorker w, DoWorkEventArgs e)
		{
			try
			{
				EntityModel entityModel = (EntityModel)this.entity.SelectedItem;
				RetrieveEntityRequest expr_16 = new RetrieveEntityRequest();
				expr_16.LogicalName=(entityModel.Name);
				expr_16.EntityFilters= EntityFilters.All;
				RetrieveEntityRequest retrieveEntityRequest = expr_16;
				RetrieveEntityResponse retrieveEntityResponse = (RetrieveEntityResponse)base.Service.Execute(retrieveEntityRequest);
				IEnumerable<AttributeMetadata> arg_67_0 = retrieveEntityResponse.EntityMetadata.Attributes;

				IEnumerable<AttributeMetadata> arg_8B_0 = arg_67_0.OrderBy(a => a.LogicalName);
				string[] array = arg_8B_0.Select(a => a.LogicalName).ToArray<string>();
				this.entityTypeCode = retrieveEntityResponse.EntityMetadata.ObjectTypeCode.Value;
				this.inputCity.Items.Clear();
				this.inputUnit.Items.Clear();
				this.inputCountry.Items.Clear();
				this.inputCounty.Items.Clear();
				this.inputState.Items.Clear();
				this.inputZip.Items.Clear();
				this.inputStreet.Items.Clear();
				this.outputCity.Items.Clear();
				this.outputUnit.Items.Clear();
				this.outputCountry.Items.Clear();
				this.outputCounty.Items.Clear();
				this.outputState.Items.Clear();
				this.outputZip.Items.Clear();
				this.outputStreet.Items.Clear();
				this.outputlongitude.Items.Clear();
				this.outputLatitude.Items.Clear();
				this.outputDetails.Items.Clear();
				ComboBox.ObjectCollection arg_1D0_0 = this.inputCity.Items;
				object[] items = array;
				arg_1D0_0.AddRange(items);
				ComboBox.ObjectCollection arg_1E5_0 = this.inputUnit.Items;
				items = array;
				arg_1E5_0.AddRange(items);
				ComboBox.ObjectCollection arg_1FA_0 = this.inputCountry.Items;
				items = array;
				arg_1FA_0.AddRange(items);
				ComboBox.ObjectCollection arg_20F_0 = this.inputCounty.Items;
				items = array;
				arg_20F_0.AddRange(items);
				ComboBox.ObjectCollection arg_224_0 = this.inputState.Items;
				items = array;
				arg_224_0.AddRange(items);
				ComboBox.ObjectCollection arg_239_0 = this.inputZip.Items;
				items = array;
				arg_239_0.AddRange(items);
				ComboBox.ObjectCollection arg_24E_0 = this.inputStreet.Items;
				items = array;
				arg_24E_0.AddRange(items);
				ComboBox.ObjectCollection arg_263_0 = this.outputCity.Items;
				items = array;
				arg_263_0.AddRange(items);
				ComboBox.ObjectCollection arg_278_0 = this.outputUnit.Items;
				items = array;
				arg_278_0.AddRange(items);
				ComboBox.ObjectCollection arg_28D_0 = this.outputCountry.Items;
				items = array;
				arg_28D_0.AddRange(items);
				ComboBox.ObjectCollection arg_2A2_0 = this.outputCounty.Items;
				items = array;
				arg_2A2_0.AddRange(items);
				ComboBox.ObjectCollection arg_2B7_0 = this.outputState.Items;
				items = array;
				arg_2B7_0.AddRange(items);
				ComboBox.ObjectCollection arg_2CC_0 = this.outputZip.Items;
				items = array;
				arg_2CC_0.AddRange(items);
				ComboBox.ObjectCollection arg_2E1_0 = this.outputStreet.Items;
				items = array;
				arg_2E1_0.AddRange(items);
				ComboBox.ObjectCollection arg_2F6_0 = this.outputlongitude.Items;
				items = array;
				arg_2F6_0.AddRange(items);
				ComboBox.ObjectCollection arg_30B_0 = this.outputLatitude.Items;
				items = array;
				arg_30B_0.AddRange(items);
				ComboBox.ObjectCollection arg_320_0 = this.outputDetails.Items;
				items = array;
				arg_320_0.AddRange(items);
				this.GetViews();
			}
			catch (Exception arg_32D_0)
			{
				MessageBox.Show(arg_32D_0.Message);
			}
		});
		expr_06.ProgressChanged=(delegate (ProgressChangedEventArgs e)
		{
			base.SetWorkingMessage("Message to display", 340, 150);
		});

		expr_06.PostWorkCallBack = new Action<RunWorkerCompletedEventArgs>(t => { });
		expr_06.AsyncArgument=null;
		expr_06.IsCancelable=true;
		expr_06.MessageWidth=(340);
		expr_06.MessageHeight=(150);
		base.WorkAsync(expr_06);
	}

	private void PopulateEntityField()
	{
		WorkAsyncInfo expr_06 = new WorkAsyncInfo();
		expr_06.Message=("Please wait while retrieving entities...");
		expr_06.Work=(delegate (BackgroundWorker w, DoWorkEventArgs e)
		{
			this.entity.Items.Clear();
			try
			{
				RetrieveAllEntitiesRequest expr_15 = new RetrieveAllEntitiesRequest();
				expr_15.RetrieveAsIfPublished=(true);
				expr_15.EntityFilters= EntityFilters.Entity;
				RetrieveAllEntitiesRequest retrieveAllEntitiesRequest = expr_15;
				IEnumerable<EntityMetadata> arg_59_0 = ((RetrieveAllEntitiesResponse)base.Service.Execute(retrieveAllEntitiesRequest)).EntityMetadata;
			
				IEnumerable<EntityMetadata> arg_7D_0 = arg_59_0.OrderBy(c=> c.LogicalName);
			
				EntityModel[] array = arg_7D_0.Select(c=> {
					return new EntityModel
					{
						Name = c.LogicalName,
						PrimaryFieldName = c.PrimaryNameAttribute
					};
				}).ToArray<EntityModel>();
				ComboBox.ObjectCollection arg_96_0 = this.entity.Items;
				object[] items = array;
				arg_96_0.AddRange(items);
			}
			catch (Exception arg_9D_0)
			{
				MessageBox.Show(arg_9D_0.Message);
			}
		});
		expr_06.ProgressChanged=(delegate (ProgressChangedEventArgs e)
		{
			base.SetWorkingMessage("Message to display", 340, 150);
		});
	
		expr_06.PostWorkCallBack=new Action<RunWorkerCompletedEventArgs>(t=>{});
		expr_06.AsyncArgument=(null);
		expr_06.IsCancelable=(true);
		expr_06.MessageWidth=(340);
		expr_06.MessageHeight=(150);
		base.WorkAsync(expr_06);
	}

	private void RetrieveEntity_Click(object sender, EventArgs e)
	{
		base.ExecuteMethod(new Action(this.PopulateEntityField));
	}

	public BulkVerifier()
	{
		this.InitializeComponent();
		this.bt_startValidation.Enabled = false;
		base.ExecuteMethod(new Action(this.PopulateEntityField));
	}

	private void clbPersonalViewList_Click(object sender, EventArgs e)
	{
		for (int i = 0; i < this.clb_personalViewList.Items.Count; i++)
		{
			if (this.clb_personalViewList.GetItemRectangle(i).Contains(this.clb_personalViewList.PointToClient(Control.MousePosition)))
			{
				switch (this.clb_personalViewList.GetItemCheckState(i))
				{
					case CheckState.Unchecked:
					case CheckState.Indeterminate:
						this.clb_personalViewList.SetItemCheckState(i, CheckState.Checked);
						break;
					case CheckState.Checked:
						this.clb_personalViewList.SetItemCheckState(i, CheckState.Unchecked);
						break;
				}
			}
		}
	}

	private void clb_viewList_Click(object sender, EventArgs e)
	{
		for (int i = 0; i < this.clb_viewList.Items.Count; i++)
		{
			if (this.clb_viewList.GetItemRectangle(i).Contains(this.clb_viewList.PointToClient(Control.MousePosition)))
			{
				switch (this.clb_viewList.GetItemCheckState(i))
				{
					case CheckState.Unchecked:
					case CheckState.Indeterminate:
						this.clb_viewList.SetItemCheckState(i, CheckState.Checked);
						break;
					case CheckState.Checked:
						this.clb_viewList.SetItemCheckState(i, CheckState.Unchecked);
						break;
				}
			}
		}
	}

	private void EnableAfterCheckView(object sender, ItemCheckEventArgs e)
	{
		base.BeginInvoke(new MethodInvoker(delegate
		{
			this.EnableValidationButton(sender, e);
		}));
	}

	private void BtnCloseClick(object sender, EventArgs e)
	{
		base.CloseTool();
	}

	private void ClearAllField()
	{
		this.clb_viewList.Items.Clear();
		this.clb_viewList.Text = "";
		this.clb_personalViewList.Items.Clear();
		this.clb_personalViewList.Text = "";
		this.inputCity.Items.Clear();
		this.inputCountry.Items.Clear();
		this.inputCounty.Items.Clear();
		this.inputState.Items.Clear();
		this.inputStreet.Items.Clear();
		this.inputUnit.Items.Clear();
		this.inputZip.Items.Clear();
		this.outputlongitude.Items.Clear();
		this.outputCity.Items.Clear();
		this.outputCountry.Items.Clear();
		this.outputCounty.Items.Clear();
		this.outputDetails.Items.Clear();
		this.outputLatitude.Items.Clear();
		this.outputState.Items.Clear();
		this.outputStreet.Items.Clear();
		this.outputUnit.Items.Clear();
		this.outputZip.Items.Clear();
		this.inputCity.Text = "";
		this.inputCountry.Text = "";
		this.inputCounty.Text = "";
		this.inputState.Text = "";
		this.inputStreet.Text = "";
		this.inputUnit.Text = "";
		this.inputZip.Text = "";
		this.outputlongitude.Text = "";
		this.outputCity.Text = "";
		this.outputCountry.Text = "";
		this.outputCounty.Text = "";
		this.outputDetails.Text = "";
		this.outputLatitude.Text = "";
		this.outputState.Text = "";
		this.outputStreet.Text = "";
		this.outputUnit.Text = "";
		this.outputZip.Text = "";
	}

	private void OnEntityChange(object sender, EventArgs e)
	{
		this.ClearAllField();
		this.EnableValidationButton(sender, e);
		base.ExecuteMethod(new Action(this.PopulateInputOutputFields));
	}

	private void btStartValidation__Click(object sender, EventArgs e)
	{
		WorkAsyncInfo expr_06 = new WorkAsyncInfo();
		expr_06.Message=("Please wait while validating the addresses...");
		expr_06.Work=(delegate (BackgroundWorker w, DoWorkEventArgs ex)
		{
			EntityModel entityModel = (EntityModel)this.entity.SelectedItem;
			ColumnSet columnSet = new ColumnSet();
			if (!string.IsNullOrEmpty(this.outputStreet.Text))
			{
				columnSet.AddColumn(this.outputStreet.Text);
			}
			if (!string.IsNullOrEmpty(this.outputCity.Text))
			{
				columnSet.AddColumn(this.outputCity.Text);
			}
			if (!string.IsNullOrEmpty(this.outputUnit.Text))
			{
				columnSet.AddColumn(this.outputUnit.Text);
			}
			if (!string.IsNullOrEmpty(this.outputZip.Text))
			{
				columnSet.AddColumn(this.outputZip.Text);
			}
			if (!string.IsNullOrEmpty(this.outputCountry.Text))
			{
				columnSet.AddColumn(this.outputCountry.Text);
			}
			if (!string.IsNullOrEmpty(this.outputCounty.Text))
			{
				columnSet.AddColumn(this.outputCounty.Text);
			}
			if (!string.IsNullOrEmpty(this.outputState.Text))
			{
				columnSet.AddColumn(this.outputState.Text);
			}
			if (!string.IsNullOrEmpty(this.outputlongitude.Text))
			{
				columnSet.AddColumn(this.outputlongitude.Text);
			}
			if (!string.IsNullOrEmpty(this.outputLatitude.Text))
			{
				columnSet.AddColumn(this.outputLatitude.Text);
			}
			if (!string.IsNullOrEmpty(this.outputDetails.Text))
			{
				columnSet.AddColumn(this.outputDetails.Text);
			}
			if (!string.IsNullOrEmpty(this.inputStreet.Text))
			{
				columnSet.AddColumn(this.inputStreet.Text);
			}
			if (!string.IsNullOrEmpty(this.inputCity.Text))
			{
				columnSet.AddColumn(this.inputCity.Text);
			}
			if (!string.IsNullOrEmpty(this.inputUnit.Text))
			{
				columnSet.AddColumn(this.inputUnit.Text);
			}
			if (!string.IsNullOrEmpty(this.inputZip.Text))
			{
				columnSet.AddColumn(this.inputZip.Text);
			}
			if (!string.IsNullOrEmpty(this.inputCountry.Text))
			{
				columnSet.AddColumn(this.inputCountry.Text);
			}
			if (!string.IsNullOrEmpty(this.inputCounty.Text))
			{
				columnSet.AddColumn(this.inputCounty.Text);
			}
			if (!string.IsNullOrEmpty(this.inputState.Text))
			{
				columnSet.AddColumn(this.inputState.Text);
			}
			columnSet.AddColumn(entityModel.PrimaryFieldName);
			this.ValidateAddress(this.clb_viewList, columnSet, entityModel);
			this.ValidateAddress(this.clb_personalViewList, columnSet, entityModel);
		});
		expr_06.ProgressChanged=(delegate (ProgressChangedEventArgs ex)
		{
			base.SetWorkingMessage("Message to display", 340, 150);
		});
		expr_06.PostWorkCallBack = new Action<RunWorkerCompletedEventArgs>(t=> {
			MessageBox.Show("The addresses have been validated.");
		});
		expr_06.AsyncArgument = (null);
		expr_06.IsCancelable = (true);
		expr_06.MessageWidth = (340);
		expr_06.MessageHeight = (150);
		base.WorkAsync(expr_06);
	}

	private void ValidateAddress(CheckedListBox list, ColumnSet columnSet, EntityModel selectedEntityModel)
	{
		BingGeocodingService bingGeocodingService = new BingGeocodingService();
		for (int i = 0; i < list.Items.Count; i++)
		{
			if (list.GetItemChecked(i))
			{
				ViewModel viewModel = (ViewModel)list.Items[i];
				foreach (Entity current in base.Service.RetrieveMultiple(new FetchExpression(viewModel.FetchXml)).Entities)
				{
					Entity entity = base.Service.Retrieve(current.LogicalName, (Guid)current.Attributes[current.LogicalName + "id"], columnSet);
					Address address = new Address
					{
						StreetAddress = entity.Attributes.Contains(this.inputStreet.Text) ? ((string)entity[this.inputStreet.Text]) : "",
						UnitNumber = entity.Attributes.Contains(this.inputUnit.Text) ? ((string)entity[this.inputUnit.Text]) : "",
						City = entity.Attributes.Contains(this.inputCity.Text) ? ((string)entity[this.inputCity.Text]) : "",
						County = entity.Attributes.Contains(this.inputCounty.Text) ? ((string)entity[this.inputCounty.Text]) : "",
						Country = entity.Attributes.Contains(this.inputCountry.Text) ? ((string)entity[this.inputCountry.Text]) : "",
						Zip = entity.Attributes.Contains(this.inputZip.Text) ? ((string)entity[this.inputZip.Text]) : "",
						State = entity.Attributes.Contains(this.inputState.Text) ? ((string)entity[this.inputState.Text]) : ""
					};
					try
					{
						if (!address.IsValidForGeocoding())
						{
							if (entity.Attributes.Contains(this.outputDetails.Text))
							{
								Entity arg_286_0 = new Entity(entity.LogicalName);
								string text = current.LogicalName + "id";
								arg_286_0[text]= (Guid)current.Attributes[current.LogicalName + "id"];
								string text2 = this.outputDetails.Text;
								arg_286_0[text2]= "You must provide a street address and [city + state] or [zip/postal code].";
								Entity entity2 = arg_286_0;
								base.Service.Update(entity2);
							}
							this.rtb_validationStatus.AppendText(string.Format("{0} {1}({2}) : Failed", Environment.NewLine, entity[selectedEntityModel.PrimaryFieldName], current.Attributes[current.LogicalName + "id"]));
						}
						else
						{
							Address address2 = bingGeocodingService.VerifyAndGeocodeAddresses(address, this.tb_bingKey.Text);
							string text;
							if (address2.TransmissionResult == "InvalidCredentials")
							{
								Entity arg_371_0 = new Entity(entity.LogicalName);
								string text2 = current.LogicalName + "id";
								arg_371_0[text2]=(Guid)current.Attributes[current.LogicalName + "id"];
								text = this.outputDetails.Text;
								arg_371_0[text]= "The credentials provided are not valid";
								Entity entity3 = arg_371_0;
								base.Service.Update(entity3);
								this.rtb_validationStatus.AppendText(string.Format("{0} {1}({2}) : Invalid Credentials", Environment.NewLine, entity[selectedEntityModel.PrimaryFieldName], current.Attributes[current.LogicalName + "id"]));
								break;
							}
							Entity arg_431_0 = new Entity(entity.LogicalName);
							text = current.LogicalName + "id";
							arg_431_0[text]= (Guid)current.Attributes[current.LogicalName + "id"];
							Entity entity4 = arg_431_0;
							if (columnSet.Columns.Contains(this.outputCity.Text))
							{
								entity4[this.outputCity.Text]= address2.City;
							}
							if (columnSet.Columns.Contains(this.outputUnit.Text))
							{
								entity4[this.outputUnit.Text]=address2.UnitNumber;
							}
							if (columnSet.Columns.Contains(this.outputZip.Text))
							{
								entity4[this.outputZip.Text]=address2.Zip;
							}
							if (columnSet.Columns.Contains(this.outputCountry.Text))
							{
								entity4[this.outputCountry.Text]= address2.Country;
							}
							if (columnSet.Columns.Contains(this.outputCounty.Text))
							{
								entity4[this.outputCounty.Text]= address2.County;
							}
							if (columnSet.Columns.Contains(this.outputState.Text))
							{
								entity4[this.outputState.Text]= address2.State;
							}
							if (columnSet.Columns.Contains(this.outputlongitude.Text))
							{
								entity4[this.outputlongitude.Text]=address2.Longitude.Value;
							}
							if (columnSet.Columns.Contains(this.outputLatitude.Text))
							{
								entity4[this.outputLatitude.Text]= address2.Latitude.Value;
							}
							if (columnSet.Columns.Contains(this.outputDetails.Text))
							{
								entity4[this.outputDetails.Text]= address2.VerificationDetails;
							}
							this.rtb_validationStatus.AppendText(string.Format("{0} {1}({2}) : {3} ", new object[]
							{
									Environment.NewLine,
									entity[selectedEntityModel.PrimaryFieldName],
									current.Attributes[current.LogicalName + "id"],
									address2.VerificationDetails
							}));
							base.Service.Update(entity4);
						}
					}
					catch (Exception ex)
					{
						Entity arg_6C7_0 = new Entity(entity.LogicalName);
						string text = current.LogicalName + "id";
						arg_6C7_0[text]= (Guid)current.Attributes[current.LogicalName + "id"];
						Entity entity5 = arg_6C7_0;
						base.Service.Update(entity5);
						this.rtb_validationStatus.AppendText(string.Format("{0} {1}({2}) : ", Environment.NewLine, entity[selectedEntityModel.PrimaryFieldName], current.Attributes[current.LogicalName + "id"]) + ex.Message);
					}
				}
			}
		}
	}

	public void EnableValidationButton(object sender, EventArgs e)
	{
		bool flag = this.clb_viewList.CheckedItems.Count > 0 || this.clb_personalViewList.CheckedItems.Count > 0;
		if ((!string.IsNullOrEmpty(this.tb_bingKey.Text) && !string.IsNullOrEmpty(this.entity.Text) && !string.IsNullOrEmpty(this.inputStreet.Text) && !string.IsNullOrEmpty(this.inputCountry.Text) && !string.IsNullOrEmpty(this.inputState.Text) && !string.IsNullOrEmpty(this.inputCity.Text) && !string.IsNullOrEmpty(this.outputStreet.Text) && !string.IsNullOrEmpty(this.outputCountry.Text) && !string.IsNullOrEmpty(this.outputState.Text) && !string.IsNullOrEmpty(this.outputCity.Text)) & flag)
		{
			this.bt_startValidation.Enabled = true;
			return;
		}
		this.bt_startValidation.Enabled = false;
	}

	public void CopyInputToOutput(object sender, EventArgs e)
	{
		this.outputStreet.Text = this.inputStreet.Text;
		this.outputState.Text = this.inputState.Text;
		this.outputCity.Text = this.inputCity.Text;
		this.outputCountry.Text = this.inputCountry.Text;
		this.outputZip.Text = this.inputZip.Text;
		this.outputUnit.Text = this.inputUnit.Text;
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && this.components != null)
		{
			this.components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof(BulkAddressVerifier));
		this.toolStripMenu = new ToolStrip();
		this.tsbClose = new ToolStripMenuItem();
		this.toolStripSeparator2 = new ToolStripSeparator();
		this.copyInput = new ToolStripMenuItem();
		this.RetrieveEntity = new ToolStripMenuItem();
		this.toolStripSeparator1 = new ToolStripSeparator();
		this.bt_startValidation = new ToolStripMenuItem();
		this.toolStripSeparator3 = new ToolStripSeparator();
		this.label1 = new Label();
		this.label2 = new Label();
		this.tb_bingKey = new TextBox();
		this.entity = new ComboBox();
		this.label3 = new Label();
		this.label4 = new Label();
		this.inputStreet = new ComboBox();
		this.inputZip = new ComboBox();
		this.inputCity = new ComboBox();
		this.inputCounty = new ComboBox();
		this.inputState = new ComboBox();
		this.inputCountry = new ComboBox();
		this.label5 = new Label();
		this.label6 = new Label();
		this.label7 = new Label();
		this.label8 = new Label();
		this.label9 = new Label();
		this.label10 = new Label();
		this.outputStreet = new ComboBox();
		this.outputZip = new ComboBox();
		this.outputCity = new ComboBox();
		this.outputCounty = new ComboBox();
		this.outputState = new ComboBox();
		this.outputCountry = new ComboBox();
		this.label11 = new Label();
		this.outputlongitude = new ComboBox();
		this.label12 = new Label();
		this.outputLatitude = new ComboBox();
		this.label13 = new Label();
		this.outputDetails = new ComboBox();
		this.inputUnit = new ComboBox();
		this.label14 = new Label();
		this.outputUnit = new ComboBox();
		this.label15 = new Label();
		this.clb_viewList = new CheckedListBox();
		this.clb_personalViewList = new CheckedListBox();
		this.rtb_validationStatus = new RichTextBox();
		this.label16 = new Label();
		this.label17 = new Label();
		this.label18 = new Label();
		this.toolStripMenu.SuspendLayout();
		base.SuspendLayout();
		this.toolStripMenu.ImageScalingSize = new Size(20, 20);
		this.toolStripMenu.Items.AddRange(new ToolStripItem[]
		{
				this.tsbClose,
				this.toolStripSeparator2,
				this.RetrieveEntity,
				this.toolStripSeparator1,
				this.copyInput,
				this.toolStripSeparator3,
				this.bt_startValidation
		});
		this.toolStripMenu.Location = new System.Drawing.Point(0, 0);
		this.toolStripMenu.Name = "toolStripMenu";
		this.toolStripMenu.Size = new Size(1467, 32);
		this.toolStripMenu.TabIndex = 4;
		this.toolStripMenu.Text = "toolStrip1";
		this.tsbClose.Margin = new Padding(4);
		this.tsbClose.Name = "tsbClose";
		this.tsbClose.Size = new Size(115, 24);
		this.tsbClose.Text = "Close";
		this.tsbClose.Image = (Image)componentResourceManager.GetObject("close");
		this.toolStripSeparator2.Name = "toolStripSeparator2";
		this.toolStripSeparator2.Size = new Size(6, 32);
		this.RetrieveEntity.Margin = new Padding(4);
		this.RetrieveEntity.Name = "RetrieveEntity";
		this.RetrieveEntity.Size = new Size(127, 24);
		this.RetrieveEntity.Text = "Retrieve Entities";
		this.RetrieveEntity.Image = (Image)componentResourceManager.GetObject("retrieve");
		this.copyInput.Margin = new Padding(4);
		this.copyInput.Name = "CopyInput";
		this.copyInput.Size = new Size(127, 24);
		this.copyInput.Text = "Copy input to output";
		this.copyInput.Image = (Image)componentResourceManager.GetObject("Copy");
		this.toolStripSeparator1.Name = "toolStripSeparator1";
		this.toolStripSeparator1.Size = new Size(6, 32);
		this.bt_startValidation.Margin = new Padding(4);
		this.bt_startValidation.Name = "bt_startValidation";
		this.bt_startValidation.Size = new Size(122, 24);
		this.bt_startValidation.Text = "Start validation";
		this.bt_startValidation.Image = (Image)componentResourceManager.GetObject("start");
		this.toolStripSeparator3.Name = "toolStripSeparator3";
		this.toolStripSeparator3.Size = new Size(6, 32);
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(13, 62);
		this.label1.Name = "label1";
		this.label1.Size = new Size(127, 17);
		this.label1.Text = "Bing Maps API Key";
		this.label2.AutoSize = true;
		this.label2.Location = new System.Drawing.Point(39, 98);
		this.label2.Name = "label2";
		this.label2.Size = new Size(105, 17);
		this.label2.Text = "Entity Selection";
		this.tb_bingKey.Location = new System.Drawing.Point(158, 60);
		this.tb_bingKey.Name = "tb_bingKey";
		this.tb_bingKey.Size = new Size(396, 22);
		this.tb_bingKey.TabIndex = 2;
		this.entity.FormattingEnabled = true;
		this.entity.Location = new System.Drawing.Point(158, 96);
		this.entity.Name = "entity";
		this.entity.Size = new Size(252, 24);
		this.entity.TabIndex = 3;
		this.label3.AutoSize = true;
		this.label3.Location = new System.Drawing.Point(445, 144);
		this.label3.Name = "label3";
		this.label3.Size = new Size(94, 17);
		this.label3.Text = "Source Fields";
		this.label3.TextAlign = ContentAlignment.MiddleLeft;
		this.label4.AutoSize = true;
		this.label4.Location = new System.Drawing.Point(668, 144);
		this.label4.Name = "label4";
		this.label4.Size = new Size(120, 17);
		this.label4.Text = "Destination Fields";
		this.inputStreet.FormattingEnabled = true;
		this.inputStreet.Location = new System.Drawing.Point(449, 170);
		this.inputStreet.Name = "inputStreet";
		this.inputStreet.Size = new Size(205, 24);
		this.inputStreet.TabIndex = 4;
		this.inputZip.FormattingEnabled = true;
		this.inputZip.Location = new System.Drawing.Point(449, 320);
		this.inputZip.Name = "inputZip";
		this.inputZip.Size = new Size(205, 24);
		this.inputZip.TabIndex = 14;
		this.inputCity.FormattingEnabled = true;
		this.inputCity.Location = new System.Drawing.Point(449, 231);
		this.inputCity.Name = "inputCity";
		this.inputCity.Size = new Size(205, 24);
		this.inputCity.TabIndex = 8;
		this.inputCounty.FormattingEnabled = true;
		this.inputCounty.Location = new System.Drawing.Point(449, 261);
		this.inputCounty.Name = "inputCounty";
		this.inputCounty.Size = new Size(205, 24);
		this.inputCounty.TabIndex = 10;
		this.inputState.FormattingEnabled = true;
		this.inputState.Location = new System.Drawing.Point(449, 291);
		this.inputState.Name = "inputState";
		this.inputState.Size = new Size(205, 24);
		this.inputState.TabIndex = 12;
		this.inputCountry.FormattingEnabled = true;
		this.inputCountry.Location = new System.Drawing.Point(449, 348);
		this.inputCountry.Name = "inputCountry";
		this.inputCountry.Size = new Size(205, 24);
		this.inputCountry.TabIndex = 16;
		this.label5.AutoSize = true;
		this.label5.Location = new System.Drawing.Point(321, 169);
		this.label5.Name = "label5";
		this.label5.Size = new Size(111, 17);
		this.label5.Text = "Street Address *";
		this.label6.AutoSize = true;
		this.label6.Location = new System.Drawing.Point(321, 319);
		this.label6.Name = "label6";
		this.label6.Size = new Size(108, 17);
		this.label6.Text = "Postal/Zip Code";
		this.label7.AutoSize = true;
		this.label7.Location = new System.Drawing.Point(321, 230);
		this.label7.Name = "label7";
		this.label7.Size = new Size(40, 17);
		this.label7.Text = "City *";
		this.label8.AutoSize = true;
		this.label8.Location = new System.Drawing.Point(321, 260);
		this.label8.Name = "label8";
		this.label8.Size = new Size(61, 17);
		this.label8.Text = "County";
		this.label9.AutoSize = true;
		this.label9.Location = new System.Drawing.Point(321, 290);
		this.label9.Name = "label9";
		this.label9.Size = new Size(50, 17);
		this.label9.Text = "State *";
		this.label10.AutoSize = true;
		this.label10.Location = new System.Drawing.Point(321, 347);
		this.label10.Name = "label10";
		this.label10.Size = new Size(57, 17);
		this.label10.Text = "Country *";
		this.outputStreet.FormattingEnabled = true;
		this.outputStreet.Location = new System.Drawing.Point(671, 169);
		this.outputStreet.Name = "outputStreet";
		this.outputStreet.Size = new Size(205, 24);
		this.outputStreet.TabIndex = 5;
		this.outputZip.FormattingEnabled = true;
		this.outputZip.Location = new System.Drawing.Point(671, 320);
		this.outputZip.Name = "outputZip";
		this.outputZip.Size = new Size(205, 24);
		this.outputZip.TabIndex = 15;
		this.outputCity.FormattingEnabled = true;
		this.outputCity.Location = new System.Drawing.Point(671, 231);
		this.outputCity.Name = "outputCity";
		this.outputCity.Size = new Size(205, 24);
		this.outputCity.TabIndex = 9;
		this.outputCounty.FormattingEnabled = true;
		this.outputCounty.Location = new System.Drawing.Point(671, 261);
		this.outputCounty.Name = "outputCounty";
		this.outputCounty.Size = new Size(205, 24);
		this.outputCounty.TabIndex = 11;
		this.outputState.FormattingEnabled = true;
		this.outputState.Location = new System.Drawing.Point(671, 291);
		this.outputState.Name = "outputState";
		this.outputState.Size = new Size(205, 24);
		this.outputState.TabIndex = 13;
		this.outputCountry.FormattingEnabled = true;
		this.outputCountry.Location = new System.Drawing.Point(671, 348);
		this.outputCountry.Name = "outputCountry";
		this.outputCountry.Size = new Size(205, 24);
		this.outputCountry.TabIndex = 17;
		this.label11.AutoSize = true;
		this.label11.Location = new System.Drawing.Point(321, 377);
		this.label11.Name = "label11";
		this.label11.Size = new Size(71, 17);
		this.label11.Text = "Longitude";
		this.outputlongitude.FormattingEnabled = true;
		this.outputlongitude.Location = new System.Drawing.Point(671, 378);
		this.outputlongitude.Name = "outputlongitude";
		this.outputlongitude.Size = new Size(205, 24);
		this.outputlongitude.TabIndex = 18;
		this.label12.AutoSize = true;
		this.label12.Location = new System.Drawing.Point(321, 406);
		this.label12.Name = "label12";
		this.label12.Size = new Size(59, 17);
		this.label12.Text = "Latitude";
		this.outputLatitude.FormattingEnabled = true;
		this.outputLatitude.Location = new System.Drawing.Point(671, 407);
		this.outputLatitude.Name = "outputLatitude";
		this.outputLatitude.Size = new Size(205, 24);
		this.outputLatitude.TabIndex = 19;
		this.label13.AutoSize = true;
		this.label13.Location = new System.Drawing.Point(321, 436);
		this.label13.Name = "label13";
		this.label13.Size = new Size(51, 17);
		this.label13.Text = "Details";
		this.outputDetails.FormattingEnabled = true;
		this.outputDetails.Location = new System.Drawing.Point(671, 438);
		this.outputDetails.Name = "outputDetails";
		this.outputDetails.Size = new Size(205, 24);
		this.outputDetails.TabIndex = 20;
		this.inputUnit.FormattingEnabled = true;
		this.inputUnit.Location = new System.Drawing.Point(449, 199);
		this.inputUnit.Name = "inputUnit";
		this.inputUnit.Size = new Size(205, 24);
		this.inputUnit.TabIndex = 6;
		this.label14.AutoSize = true;
		this.label14.Location = new System.Drawing.Point(321, 198);
		this.label14.Name = "label14";
		this.label14.Size = new Size(85, 17);
		this.label14.Text = "Unit number";
		this.outputUnit.FormattingEnabled = true;
		this.outputUnit.Location = new System.Drawing.Point(671, 199);
		this.outputUnit.Name = "outputUnit";
		this.outputUnit.Size = new Size(205, 24);
		this.outputUnit.TabIndex = 7;
		this.label15.AutoSize = true;
		this.label15.Location = new System.Drawing.Point(9, 140);
		this.label15.Name = "label15";
		this.label15.Size = new Size(282, 17);
		this.label15.Text = "Query Selection (select records to process)";
		this.clb_viewList.FormattingEnabled = true;
		this.clb_viewList.Location = new System.Drawing.Point(14, 178);
		this.clb_viewList.Margin = new Padding(3, 4, 3, 4);
		this.clb_viewList.Name = "clb_viewList";
		this.clb_viewList.Size = new Size(282, 225);
		this.clb_viewList.TabIndex = 47;
		this.clb_viewList.SelectionMode = SelectionMode.None;
		this.clb_personalViewList.FormattingEnabled = true;
		this.clb_personalViewList.Location = new System.Drawing.Point(14, 425);
		this.clb_personalViewList.Margin = new Padding(3, 4, 3, 4);
		this.clb_personalViewList.Name = "clb_personnalViewList";
		this.clb_personalViewList.Size = new Size(282, 225);
		this.clb_personalViewList.SelectionMode = SelectionMode.None;
		this.clb_personalViewList.TabIndex = 57;
		this.rtb_validationStatus.Location = new System.Drawing.Point(893, 169);
		this.rtb_validationStatus.Name = "rtb_validationStatus";
		this.rtb_validationStatus.Size = new Size(562, 451);
		this.rtb_validationStatus.TabIndex = 49;
		this.rtb_validationStatus.Text = "";
		this.label16.AutoSize = true;
		this.label16.Location = new System.Drawing.Point(889, 144);
		this.label16.Name = "label16";
		this.label16.Size = new Size(112, 17);
		this.label16.Text = "Validation status";
		this.label17.AutoSize = true;
		this.label17.Location = new System.Drawing.Point(10, 159);
		this.label17.Name = "label17";
		this.label17.Size = new Size(51, 20);
		this.label17.Text = "Views";
		this.label18.AutoSize = true;
		this.label18.Location = new System.Drawing.Point(10, 405);
		this.label18.Name = "label18";
		this.label18.Size = new Size(113, 20);
		this.label18.Text = "Personal views";
		this.tb_bingKey.TextChanged += new EventHandler(this.EnableValidationButton);
		this.entity.SelectedIndexChanged += new EventHandler(this.OnEntityChange);
		this.inputZip.SelectedIndexChanged += new EventHandler(this.EnableValidationButton);
		this.inputCity.SelectedIndexChanged += new EventHandler(this.EnableValidationButton);
		this.inputState.SelectedIndexChanged += new EventHandler(this.EnableValidationButton);
		this.inputCountry.SelectedIndexChanged += new EventHandler(this.EnableValidationButton);
		this.outputStreet.SelectedIndexChanged += new EventHandler(this.EnableValidationButton);
		this.outputZip.SelectedIndexChanged += new EventHandler(this.EnableValidationButton);
		this.outputCity.SelectedIndexChanged += new EventHandler(this.EnableValidationButton);
		this.outputState.SelectedIndexChanged += new EventHandler(this.EnableValidationButton);
		this.outputCountry.SelectedIndexChanged += new EventHandler(this.EnableValidationButton);
		this.clb_viewList.Click += new EventHandler(this.clb_viewList_Click);
		this.clb_personalViewList.Click += new EventHandler(this.clbPersonalViewList_Click);
		this.clb_viewList.ItemCheck += new ItemCheckEventHandler(this.EnableAfterCheckView);
		this.clb_personalViewList.ItemCheck += new ItemCheckEventHandler(this.EnableAfterCheckView);
		this.RetrieveEntity.Click += new EventHandler(this.RetrieveEntity_Click);
		this.bt_startValidation.Click += new EventHandler(this.btStartValidation__Click);
		this.tsbClose.Click += new EventHandler(this.BtnCloseClick);
		this.copyInput.Click += new EventHandler(this.CopyInputToOutput);
		base.AutoScaleDimensions = new SizeF(8f, 16f);
		base.AutoScaleMode = AutoScaleMode.Font;
		base.ClientSize = new Size(1467, 653);
		base.Controls.Add(this.toolStripMenu);
		base.Controls.Add(this.label16);
		base.Controls.Add(this.rtb_validationStatus);
		base.Controls.Add(this.clb_viewList);
		base.Controls.Add(this.label15);
		base.Controls.Add(this.inputUnit);
		base.Controls.Add(this.label14);
		base.Controls.Add(this.outputUnit);
		base.Controls.Add(this.label13);
		base.Controls.Add(this.outputDetails);
		base.Controls.Add(this.label12);
		base.Controls.Add(this.outputLatitude);
		base.Controls.Add(this.label11);
		base.Controls.Add(this.outputlongitude);
		base.Controls.Add(this.inputStreet);
		base.Controls.Add(this.inputZip);
		base.Controls.Add(this.inputCity);
		base.Controls.Add(this.inputCounty);
		base.Controls.Add(this.inputState);
		base.Controls.Add(this.inputCountry);
		base.Controls.Add(this.label10);
		base.Controls.Add(this.label9);
		base.Controls.Add(this.label8);
		base.Controls.Add(this.label7);
		base.Controls.Add(this.label6);
		base.Controls.Add(this.label5);
		base.Controls.Add(this.outputStreet);
		base.Controls.Add(this.outputZip);
		base.Controls.Add(this.outputCity);
		base.Controls.Add(this.outputCounty);
		base.Controls.Add(this.outputState);
		base.Controls.Add(this.outputCountry);
		base.Controls.Add(this.label4);
		base.Controls.Add(this.label3);
		base.Controls.Add(this.entity);
		base.Controls.Add(this.tb_bingKey);
		base.Controls.Add(this.label2);
		base.Controls.Add(this.label1);
		base.Controls.Add(this.clb_personalViewList);
		base.Controls.Add(this.label17);
		base.Controls.Add(this.label18);
		base.Controls.Add(this.outputStreet);
		base.Name = "Form1";
		this.Text = " ";
		this.toolStripMenu.ResumeLayout(false);
		this.toolStripMenu.PerformLayout();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
}
