using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace SharpXrm.BulkAddressVerifier
{
	public class Resource
	{
		public string __type
		{
			get;
			set;
		}

		public List<double> bbox
		{
			get;
			set;
		}

		public string name
		{
			get;
			set;
		}

		public Point point
		{
			get;
			set;
		}

		[JsonProperty("Address")]
		public BingAddress address
		{
			get;
			set;
		}

		public string confidence
		{
			get;
			set;
		}

		public string entityType
		{
			get;
			set;
		}

		public List<GeocodePoint> geocodePoints
		{
			get;
			set;
		}

		public List<string> matchCodes
		{
			get;
			set;
		}
	}
}
