using System;

namespace SharpXrm.BulkAddressVerifier.Models
{
	public class ViewModel
	{
		public string Name
		{
			get;
			set;
		}

		public string FetchXml
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
