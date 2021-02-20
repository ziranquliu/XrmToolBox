using Newtonsoft.Json;
using System;

namespace SharpXrm.BulkAddressVerifier
{
	[JsonObject("Address")]
	public class BingAddress
	{
		public string addressLine
		{
			get;
			set;
		}

		public string adminDistrict
		{
			get;
			set;
		}

		public string adminDistrict2
		{
			get;
			set;
		}

		public string countryRegion
		{
			get;
			set;
		}

		public string formattedAddress
		{
			get;
			set;
		}

		public string locality
		{
			get;
			set;
		}

		public string postalCode
		{
			get;
			set;
		}
	}
}
