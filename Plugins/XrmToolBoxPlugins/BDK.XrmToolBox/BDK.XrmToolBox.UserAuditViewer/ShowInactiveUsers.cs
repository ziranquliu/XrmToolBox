﻿/*
MIT License

Copyright (c) 2019 Tech Quantum

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
 */


namespace BDK.XrmToolBox.UserAuditViewer
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;
    using BDK.XrmToolBox.UserAuditViewer.Model;
    using DocumentFormat.OpenXml.Packaging;
    using global::XrmToolBox.Extensibility;

    /// <summary>
    /// Shaoe inactive users list
    /// </summary>
    /// <seealso cref="XrmToolBox.Extensibility.PluginControlBase" />
    public partial class ShowInactiveUsers : PluginControlBase
    {
        /// <summary>
        /// Gets or sets the org service.
        /// </summary>
        /// <value>
        /// The org service.
        /// </value>
        public IOrganizationService OrgService { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShowInactiveUsers"/> class.
        /// </summary>
        public ShowInactiveUsers()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ddlFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            var ddlFilterSelectedIndex = ddlFilter.SelectedIndex;
            WorkAsync(new WorkAsyncInfo
            {
                Message = "Retrieving inactive users based on login history...",
                Work = (w, ev) =>
                {
                    FetchExpression query = new FetchExpression(Settings.FetchXml_EnabledUsers);

                    EntityCollection entitites = OrgService.RetrieveMultiple(query);
                    List<CRMUser> data = new List<Model.CRMUser>();
                    foreach (var item in entitites.Entities)
                    {
                        data.Add(new Model.CRMUser()
                        {
                            Username = item.Attributes.ContainsKey("domainname") ? item.Attributes["domainname"].ToString() : string.Empty,
                            Name = item.Attributes.ContainsKey("fullname") ? item.Attributes["fullname"].ToString() : string.Empty,
                            Id = item.Id.ToString(),
                            BusinessUnit = item.Attributes.ContainsKey("businessunitid") ? ((EntityReference)item.Attributes["businessunitid"]).Name : string.Empty,
                            Title = item.Attributes.ContainsKey("title") ? item.Attributes["title"].ToString() : string.Empty,
                            PrimaryEmail = item.Attributes.ContainsKey("internalemailaddress") ? item.Attributes["internalemailaddress"].ToString() : string.Empty,
                        });
                    }

                    List<CRMUser> finalData = new List<Model.CRMUser>();
                    DateTime filterDate = DateTime.Now;
                    if (ddlFilterSelectedIndex == 0)
                    {
                        filterDate = filterDate.AddMonths(-1);
                    }
                    else if (ddlFilterSelectedIndex == 1)
                    {
                        filterDate = filterDate.AddMonths(-3);
                    }
                    else if (ddlFilterSelectedIndex == 2)
                    {
                        filterDate = filterDate.AddMonths(-6);
                    }
                    else if (ddlFilterSelectedIndex == 3)
                    {
                        filterDate = filterDate.AddMonths(-12);
                    }

                    Parallel.ForEach(data, (item) =>
                    {
                        FetchExpression query1 = new FetchExpression(string.Format(Settings.FetchXml_LastLoginCheck, item.Id));
                        EntityCollection entitites1 = OrgService.RetrieveMultiple(query1);
                        if (entitites1.Entities.Count > 0)
                        {
                            DateTime createdOn = (DateTime)entitites1.Entities[0]["createdon"];
                            if (createdOn < filterDate)
                            {
                                item.LastLoggedIn = createdOn;
                                finalData.Add(item);
                            }
                        }
                    });

                    ev.Result = finalData.OrderBy(x=>(x.Name)).ToList();
                },
                ProgressChanged = ev =>
                {
                    // If progress has to be notified to user, use the following method:
                    SetWorkingMessage("Message to display");
                },
                PostWorkCallBack = ev =>
                {
                    List<CRMUser> result = (List<CRMUser>)ev.Result;
                    GridUsers.DataSource = result;
                },
                AsyncArgument = null,
                IsCancelable = true,
                MessageWidth = 340,
                MessageHeight = 150
            });
        }

        /// <summary>
        /// Handles the Click event of the btnExport control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void btnExport_Click(object sender, EventArgs e)
        {
            DataSet ds = GetDataset(GridUsers);
            DialogResult dialogResult = saveFile.ShowDialog();
            if (dialogResult.ToString() == "OK")
            {
                ExportDataSet(ds, saveFile.FileName);
            }
        }

        /// <summary>
        /// Exports the data set.
        /// </summary>
        /// <param name="ds">The ds.</param>
        /// <param name="destination">The destination.</param>
        private void ExportDataSet(DataSet ds, string destination)
        {
            using (var workbook = SpreadsheetDocument.Create(destination, DocumentFormat.OpenXml.SpreadsheetDocumentType.Workbook))
            {
                var workbookPart = workbook.AddWorkbookPart();

                workbook.WorkbookPart.Workbook = new DocumentFormat.OpenXml.Spreadsheet.Workbook();

                workbook.WorkbookPart.Workbook.Sheets = new DocumentFormat.OpenXml.Spreadsheet.Sheets();

                foreach (System.Data.DataTable table in ds.Tables)
                {

                    var sheetPart = workbook.WorkbookPart.AddNewPart<WorksheetPart>();
                    var sheetData = new DocumentFormat.OpenXml.Spreadsheet.SheetData();
                    sheetPart.Worksheet = new DocumentFormat.OpenXml.Spreadsheet.Worksheet(sheetData);

                    DocumentFormat.OpenXml.Spreadsheet.Sheets sheets = workbook.WorkbookPart.Workbook.GetFirstChild<DocumentFormat.OpenXml.Spreadsheet.Sheets>();
                    string relationshipId = workbook.WorkbookPart.GetIdOfPart(sheetPart);

                    uint sheetId = 1;
                    if (sheets.Elements<DocumentFormat.OpenXml.Spreadsheet.Sheet>().Count() > 0)
                    {
                        sheetId =
                            sheets.Elements<DocumentFormat.OpenXml.Spreadsheet.Sheet>().Select(s => s.SheetId.Value).Max() + 1;
                    }

                    DocumentFormat.OpenXml.Spreadsheet.Sheet sheet = new DocumentFormat.OpenXml.Spreadsheet.Sheet() { Id = relationshipId, SheetId = sheetId, Name = table.TableName };
                    sheets.Append(sheet);

                    DocumentFormat.OpenXml.Spreadsheet.Row headerRow = new DocumentFormat.OpenXml.Spreadsheet.Row();

                    List<String> columns = new List<string>();
                    foreach (System.Data.DataColumn column in table.Columns)
                    {
                        columns.Add(column.ColumnName);

                        DocumentFormat.OpenXml.Spreadsheet.Cell cell = new DocumentFormat.OpenXml.Spreadsheet.Cell();
                        cell.DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.String;
                        cell.CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue(column.ColumnName);
                        headerRow.AppendChild(cell);
                    }


                    sheetData.AppendChild(headerRow);

                    foreach (System.Data.DataRow dsrow in table.Rows)
                    {
                        DocumentFormat.OpenXml.Spreadsheet.Row newRow = new DocumentFormat.OpenXml.Spreadsheet.Row();
                        foreach (String col in columns)
                        {
                            DocumentFormat.OpenXml.Spreadsheet.Cell cell = new DocumentFormat.OpenXml.Spreadsheet.Cell();
                            cell.DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.String;
                            cell.CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue(dsrow[col].ToString()); //
                            newRow.AppendChild(cell);
                        }

                        sheetData.AppendChild(newRow);
                    }

                }
            }
        }

        /// <summary>
        /// Gets the dataset.
        /// </summary>
        /// <param name="gridView">The grid view.</param>
        /// <returns></returns>
        private DataSet GetDataset(DataGridView gridView)
        {
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();

            // add the columns to the datatable            
            if (gridView.Columns != null)
            {

                for (int i = 0; i < gridView.Columns.Count; i++)
                {
                    dt.Columns.Add(gridView.Columns[i].DataPropertyName);
                }
            }

            //  add each of the data rows to the table
            foreach (DataGridViewRow row in gridView.Rows)
            {
                DataRow dr;
                dr = dt.NewRow();

                for (int i = 0; i < row.Cells.Count; i++)
                {
                    dr[i] = row.Cells[i].Value;
                }

                dt.Rows.Add(dr);
            }

            ds.Tables.Add(dt);
            return ds;
        }
    }
}
