using System;
using System.Collections.Generic;

namespace SharpXrm.BulkAddressVerifier
{
	public class BingMapGeocodeResponse
	{
		public string authenticationResultCode
		{
			get;
			set;
		}

		public string brandLogoUri
		{
			get;
			set;
		}

		public string copyright
		{
			get;
			set;
		}

		public List<ResourceSet> resourceSets
		{
			get;
			set;
		}

		public int statusCode
		{
			get;
			set;
		}

		public string statusDescription
		{
			get;
			set;
		}

		public string traceId
		{
			get;
			set;
		}
	}
}
