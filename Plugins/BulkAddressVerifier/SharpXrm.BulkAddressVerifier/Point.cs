using System;
using System.Collections.Generic;

namespace SharpXrm.BulkAddressVerifier
{
	public class Point
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
	}
}
