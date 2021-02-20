 using System;
using Microsoft.Xrm.Sdk;

namespace DMM365.DataContainers
{

    public class CrmEntityContainer 
    {
        /// <summary>
        /// Constractor will try to define custome entity name
        /// </summary>
        /// <param name="entity"></param>
        public CrmEntityContainer(Entity entity)
        {
            _crmEntity = entity;

            int index = entity.LogicalName.IndexOf('_');
            bool isCustom = (index != -1);
            if (isCustom)
            {
                string prefix = entity.LogicalName.Substring(0, index);
                customName = prefix + "_name";
            }
            else customName = "name";
        }

        string customName;
        Entity _crmEntity;
        public Entity crmEntity { get { return _crmEntity; } }
        public Guid id { get { return crmEntity.Id; } }

        public string idFlat { get { return crmEntity.Id.ToString(); } }
        public string name { get { return crmEntity.GetAttributeValue<string>(customName); } }
        public string logicalName { get { return crmEntity.LogicalName; } }
        public EntityReference crmEntityRef { get { return crmEntity.ToEntityReference(); } }
    }
}
