using System;

namespace SharpXrm.BulkAddressVerifier
{
	public class Address
	{
		public string UnitNumber
		{
			get;
			set;
		}

		public string HouseNumber
		{
			get;
			set;
		}

		public string StreetAddress
		{
			get;
			set;
		}

		public string City
		{
			get;
			set;
		}

		public string State
		{
			get;
			set;
		}

		public string Zip
		{
			get;
			set;
		}

		public string Country
		{
			get;
			set;
		}

		public string County
		{
			get;
			set;
		}

		public string FormattedAddress
		{
			get;
			set;
		}

		public double? Latitude
		{
			get;
			set;
		}

		public double? Longitude
		{
			get;
			set;
		}

		public string TimeZone
		{
			get;
			set;
		}

		public string VerificationDetails
		{
			get;
			set;
		}

		public string TransmissionResult
		{
			get;
			set;
		}

		public string LanguageFormat
		{
			get;
			set;
		}

		public string FullAddress
		{
			get
			{
				string text = "";
				if (this.StreetAddress != string.Empty)
				{
					text = text + this.StreetAddress + ",";
				}
				if (this.UnitNumber != string.Empty)
				{
					text = text + this.UnitNumber + ",";
				}
				if (this.City != string.Empty)
				{
					text = text + this.City + ",";
				}
				if (this.State != string.Empty)
				{
					text = text + this.State + ",";
				}
				if (this.Zip != string.Empty)
				{
					text = text + this.Zip + ",";
				}
				if (this.Country != string.Empty)
				{
					text += this.Country;
				}
				return text;
			}
		}

		public bool IsValidForGeocoding()
		{
			return !string.IsNullOrEmpty(this.StreetAddress) && ((!string.IsNullOrEmpty(this.State) && !string.IsNullOrEmpty(this.City)) || !string.IsNullOrEmpty(this.Zip));
		}

		public override bool Equals(object obj)
		{
			Address address = obj as Address;
			return address != null && (this.UnitNumber == address.UnitNumber && this.HouseNumber == address.HouseNumber && this.StreetAddress == address.StreetAddress && this.City == address.City && this.State == address.State && this.Zip == address.Zip && this.Country == address.Country && this.County == address.County && this.FormattedAddress == address.FormattedAddress && this.Latitude == address.Latitude && this.Longitude == address.Longitude && this.TimeZone == address.TimeZone && this.VerificationDetails == address.VerificationDetails && this.TransmissionResult == address.TransmissionResult) && this.LanguageFormat == address.LanguageFormat;
		}
	}
}
