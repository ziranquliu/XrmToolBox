using System;
using System.Collections.Generic;

namespace SharpXrm.BulkAddressVerifier
{
	public class ResourceSet
	{
		public int estimatedTotal
		{
			get;
			set;
		}

		public List<Resource> resources
		{
			get;
			set;
		}
	}
}
