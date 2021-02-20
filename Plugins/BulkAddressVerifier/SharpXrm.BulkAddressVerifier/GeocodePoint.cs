using System;
using System.Collections.Generic;

namespace SharpXrm.BulkAddressVerifier
{
	public class GeocodePoint
	{
		public string type
		{
			get;
			set;
		}

		public List<double> coordinates
		{
			get;
			set;
		}

		public string calculationMethod
		{
			get;
			set;
		}

		public List<string> usageTypes
		{
			get;
			set;
		}
	}
}
