﻿using AlbanianXrm.EarlyBound.Properties;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Syncfusion.Windows.Forms.Tools;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using XrmToolBox.Extensibility;

namespace AlbanianXrm.EarlyBound.Logic
{
    internal class AttributeMetadataHandler
    {
        private readonly MyPluginControl myPlugin;

        public AttributeMetadataHandler(MyPluginControl myPlugin)
        {
            this.myPlugin = myPlugin;
        }

        public void GetAttributes(string entityName, TreeNodeAdv attributesNode, bool checkedState = false, HashSet<string> checkedAttributes = default(HashSet<string>))
        {
            myPlugin.StartWorkAsync(new WorkAsyncInfo
            {
                Message = string.Format(CultureInfo.CurrentCulture, Resources.GETTING_ATTRIBUTES, entityName),
                Work = (worker, args) =>
                {
                    args.Result = myPlugin.Service.Execute(new RetrieveEntityRequest()
                    {
                        EntityFilters = EntityFilters.Attributes,
                        LogicalName = entityName
                    });
                },
                PostWorkCallBack = (args) =>
                {
                    try
                    {
                        if (args.Error != null)
                        {
                            MessageBox.Show(args.Error.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        if (args.Result is RetrieveEntityResponse result)
                        {
                            if (checkedAttributes == null) checkedAttributes = new HashSet<string>();

                            var entityMetadata = myPlugin.entityMetadatas.FirstOrDefault(x => x.LogicalName == entityName);
                            typeof(EntityMetadata).GetProperty(nameof(entityMetadata.Attributes)).SetValue(entityMetadata, result.EntityMetadata.Attributes);
                            CreateAttributeNodes(attributesNode, result.EntityMetadata, checkedState, checkedAttributes);
                        }
                    }
#pragma warning disable CA1031 // We don't want our plugin to crash because of unhandled exceptions
                    catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
                    {
                        MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    finally
                    {
                        myPlugin.WorkAsyncEnded();
                    }
                }
            });
        }

        public static void CreateAttributeNodes(TreeNodeAdv attributesNode, EntityMetadata entityMetadata, bool checkedState = false, HashSet<string> checkedAttributes = default(HashSet<string>))
        {
            attributesNode.ExpandedOnce = true;
            foreach (var item in entityMetadata.Attributes.OrderBy(x => x.LogicalName))
            {
                if (!item.DisplayName.LocalizedLabels.Any()) continue;
                var name = item.DisplayName.LocalizedLabels.First().Label;

                TreeNodeAdv node = new TreeNodeAdv($"{item.LogicalName}: {name}")
                {
                    ExpandedOnce = true,
                    ShowCheckBox = true,
                    Tag = item,
                    Checked = checkedState || checkedAttributes.Contains(item.LogicalName)
                };

                attributesNode.Nodes.Add(node);
            }
            if (entityMetadata.Attributes.Length == 0) attributesNode.Checked = checkedState;
        }
    }
}
