using System;

namespace SharpXrm.BulkAddressVerifier.Models
{
	internal class EntityModel
	{
		public string Name
		{
			get;
			set;
		}

		public string PrimaryFieldName
		{
			get;
			set;
		}

		public override string ToString()
		{
			return this.Name;
		}
	}
}
