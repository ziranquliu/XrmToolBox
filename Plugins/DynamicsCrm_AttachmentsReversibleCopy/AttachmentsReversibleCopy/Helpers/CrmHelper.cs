using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using DMM365.DataContainers;
using Microsoft.Crm.Sdk.Messages;

namespace DMM365.Helper
{
    internal static class CrmHelper
    {
        #region Common


        internal static List<CrmEntityContainer> convertEntityToEntityContainer(List<Entity> entities)
        {
            List<CrmEntityContainer> result = new List<CrmEntityContainer>();
            foreach (Entity item in entities)
                result.Add(new CrmEntityContainer(item));

            return result;
        }


        internal static string queryToFetch(CrmServiceClient service, QueryBase query)
        {

            QueryExpressionToFetchXmlRequest request = new QueryExpressionToFetchXmlRequest
            { Query = query };
            QueryExpressionToFetchXmlResponse responce = (QueryExpressionToFetchXmlResponse)service.Execute(request);
            return responce.FetchXml;
        }

        internal static void deleteEntityByID(IOrganizationService service, string entityName, List<Guid> ids)
        {
            foreach (Guid item in ids)
            {
                service.Delete(entityName, item);
            }
        }

        #endregion Common


        #region Annotation

        internal static Entity getLattestAttachmentByEntity(IOrganizationService service, Guid masterId, string masterLogicalName, bool includeNotes)
        {
            QueryExpression query = new QueryExpression("annotation");
            FilterExpression filter = new FilterExpression(LogicalOperator.And);
            ConditionExpression cond1 = new ConditionExpression("objectid", ConditionOperator.Equal, masterId);
            ConditionExpression cond2 = new ConditionExpression("objecttypecode", ConditionOperator.Equal, masterLogicalName);
            ConditionExpression cond3 = new ConditionExpression("isdocument", ConditionOperator.Equal, "1");
            filter.AddCondition(cond1);
            filter.AddCondition(cond2);
            if (!includeNotes) filter.AddCondition(cond3);

            query.Criteria = filter;
            query.NoLock = true;
            query.ColumnSet = new ColumnSet(true);
            OrderExpression order = new OrderExpression("createdon", OrderType.Descending);
            query.Orders.Add(order);

            EntityCollection result = service.RetrieveMultiple(query);
            if (!ReferenceEquals(result, null) && result.Entities.Count > 0)
                return result.Entities.First();

            return null;
        }

        internal static Guid? cloneAnnotation(IOrganizationService service, Entity noteSource)
        {
            Entity noteClone = new Entity("annotation");

            if (noteSource.Contains("documentbody"))
                noteClone["documentbody"] = noteSource["documentbody"];
            if (noteSource.Contains("filename"))
                noteClone["filename"] = noteSource["filename"];
            if (noteSource.Contains("isdocument"))
                noteClone["isdocument"] = noteSource["isdocument"];
            if (noteSource.Contains("langid"))
                noteClone["langid"] = noteSource["langid"];
            if (noteSource.Contains("mimetype"))
                noteClone["mimetype"] = noteSource["mimetype"];
            if (noteSource.Contains("notetext"))
                noteClone["notetext"] = noteSource["notetext"];
            if (noteSource.Contains("objectid"))
                noteClone["objectid"] = noteSource["objectid"];
            if (noteSource.Contains("objecttypecode"))
                noteClone["objecttypecode"] = noteSource["objecttypecode"];
            if (noteSource.Contains("stepid"))
                noteClone["stepid"] = noteSource["stepid"];
            if (noteSource.Contains("subject"))
                noteClone["subject"] = noteSource["subject"];

            //for data migration tools only, integer
            //noteClone["importsequencenumber"] = noteSource["importsequencenumber"];

            //for migration only, use for update
            //noteClone["overriddencreatedon"] = noteSource["overriddencreatedon"];

            //leave auto
            //noteClone["ownerid"] = noteSource["ownerid"];
            //noteClone["owneridtype"] = noteSource["owneridtype"];

            return service.Create(noteClone);
        }

        internal static Guid? cloneAnnotationForSpecificID(IOrganizationService service, Entity noteSource, EntityReference ObjectID)
        {
            Entity noteClone = new Entity("annotation");
            noteClone["objectid"] = ObjectID;


            if (noteSource.Contains("documentbody"))
                noteClone["documentbody"] = noteSource["documentbody"];
            if (noteSource.Contains("filename"))
                noteClone["filename"] = noteSource["filename"];
            //if (noteSource.Contains("filesize"))
            //    noteClone["filesize"] = noteSource["filesize"];
            if (noteSource.Contains("isdocument"))
                noteClone["isdocument"] = noteSource["isdocument"];
            if (noteSource.Contains("langid"))
                noteClone["langid"] = noteSource["langid"];
            if (noteSource.Contains("mimetype"))
                noteClone["mimetype"] = noteSource["mimetype"];
            if (noteSource.Contains("notetext"))
                noteClone["notetext"] = noteSource["notetext"];
            if (noteSource.Contains("objecttypecode"))
                noteClone["objecttypecode"] = noteSource["objecttypecode"];
            if (noteSource.Contains("stepid"))
                noteClone["stepid"] = noteSource["stepid"];
            if (noteSource.Contains("subject"))
                noteClone["subject"] = noteSource["subject"];

            return service.Create(noteClone);
        }

        internal static List<CrmEntityContainer> getListOfPortals(IOrganizationService service)
        {
            QueryExpression query = new QueryExpression("adx_website");
            query.ColumnSet = new ColumnSet(true);

            EntityCollection en = service.RetrieveMultiple(query);

            if (ReferenceEquals(en, null) || ReferenceEquals(en.Entities, null) || en.Entities.Count == 0) return null;

            return convertEntityToEntityContainer(en.Entities.ToList());
        }

        internal static List<CrmEntityContainer> getWebFilesByPortalId(IOrganizationService service, string portalName, string portalId, bool activeOnly)
        {
            //TO DO: Investigate : The sdk query return is wrong. Fetch is ok
            //QueryExpression query = new QueryExpression("adx_webfile");
            //FilterExpression filter = new FilterExpression(LogicalOperator.And);
            //ConditionExpression cond1 = new ConditionExpression("adx_websiteid", ConditionOperator.Equal, portalId);
            //query.ColumnSet = new ColumnSet(new string[] { "adx_name", "adx_partialurl", "adx_websiteid" });
            //query.Criteria = filter;
            //query.NoLock = true;


            string fetch = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>"
                            + "<entity name = 'adx_webfile' >"
                           + "<attribute name = 'adx_webfileid' />" 
                           + "<attribute name = 'adx_name' />"
                           + "<attribute name = 'createdon' />"
                           + "<order attribute = 'adx_name' descending = 'false' />"
                           + "<filter type = 'and' >"
                           + "{0}"
                           + "<condition attribute = 'adx_websiteid' operator= 'eq' uiname = '" + portalName + "' uitype = 'adx_website' value = '{" + portalId + "}' />"
                           + "</filter ></entity ></fetch >";
            if (activeOnly)
                fetch = fetch.Replace("{0}", "<condition value='0' attribute='statecode' operator='eq' />");
            else fetch = fetch.Replace("{0}", "");

            FetchExpression query = new FetchExpression(fetch);


            EntityCollection result = service.RetrieveMultiple(query);
            if (!ReferenceEquals(result, null) && result.Entities.Count > 0)
                return convertEntityToEntityContainer(result.Entities.ToList());

            return null;
        }

 



        #endregion Annotation


        #region  Site Settings

        internal static List<CrmEntityContainer> getListOfSettingPerPortal(IOrganizationService service, string portalName, string portalId, bool activeOnly)
        {
            string fetch = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>"
                            + "<entity name = 'adx_sitesetting' >"
                           + "<attribute name='adx_sitesettingid' />"
                           + "<attribute name='adx_name' />"
                           + "<attribute name='createdon' />"
                           + "<attribute name='adx_value' />"
                           + "<attribute name = 'adx_description' />"
                           + "<order attribute = 'adx_name' descending = 'false' />"
                           + "<filter type = 'and' >"
                           + "{0}"
                           + "<condition attribute = 'adx_websiteid' operator= 'eq' uiname = '" + portalName + "' uitype = 'adx_website' value = '{" + portalId + "}' />"
                           + "</filter ></entity ></fetch >";
            if (activeOnly)
                fetch = fetch.Replace("{0}","<condition value='0' attribute='statecode' operator='eq' />");
            else fetch = fetch.Replace("{0}", "");

            FetchExpression query = new FetchExpression(fetch);


            EntityCollection result = service.RetrieveMultiple(query);
            if (!ReferenceEquals(result, null) && result.Entities.Count > 0)
                return convertEntityToEntityContainer(result.Entities.ToList());

            return null;
        }


        #endregion  Site Settings

    }

}
