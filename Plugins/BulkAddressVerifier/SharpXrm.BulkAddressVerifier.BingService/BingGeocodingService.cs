using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;

namespace SharpXrm.BulkAddressVerifier.BingService
{
	public class BingGeocodingService
	{
		private string _addressLanguageFormat;

		private static string BingGeoCodeJsonServiceUrl
		{
			get
			{
				return "http://dev.virtualearth.net/REST/v1/Locations/{0}?output=json&maxResults=5&key={1}";
			}
		}

		public Address VerifyAndGeocodeAddresses(Address customerAddress, string key = null)
		{
			string text = string.Empty;
			if (!string.IsNullOrEmpty(customerAddress.HouseNumber))
			{
				text += customerAddress.HouseNumber;
			}
			if (!string.IsNullOrEmpty(customerAddress.StreetAddress))
			{
				text = text + ", " + customerAddress.StreetAddress;
			}
			if (!string.IsNullOrEmpty(customerAddress.City))
			{
				text = text + ", " + customerAddress.City;
			}
			if (!string.IsNullOrEmpty(customerAddress.State))
			{
				text = text + ", " + customerAddress.State;
			}
			if (!string.IsNullOrEmpty(customerAddress.Zip))
			{
				text = text + ", " + customerAddress.Zip;
			}
			text = Uri.EscapeDataString(text.Trim().Trim(new char[]
			{
				','
			}).Trim());
			if (customerAddress.LanguageFormat != "")
			{
				this._addressLanguageFormat = "&c=" + customerAddress.LanguageFormat;
			}
			else
			{
				this._addressLanguageFormat = "";
			}
			Address[] array = this.BingGeocode(text, key);
			if (((array != null) ? array[0] : null) != null)
			{
				Address[] array2 = array;
				for (int i = 0; i < array2.Length; i++)
				{
					Address address = array2[i];
					if (!string.IsNullOrEmpty(customerAddress.UnitNumber))
					{
						address.UnitNumber = customerAddress.UnitNumber.Trim();
					}
				}
			}
			return array[0];
		}

		private Address[] HydrateJson(string jsonResponse)
		{
			BingMapGeocodeResponse bingMapGeocodeResponse = JsonConvert.DeserializeObject<BingMapGeocodeResponse>(jsonResponse);
			List<Address> list = new List<Address>();
			if (bingMapGeocodeResponse != null && bingMapGeocodeResponse.resourceSets != null && bingMapGeocodeResponse.resourceSets.Count > 0)
			{
				foreach (ResourceSet current in bingMapGeocodeResponse.resourceSets)
				{
					if (current != null && current.resources != null && current.resources.Count > 0)
					{
						foreach (Resource current2 in current.resources)
						{
							if (current2.address.addressLine != null)
							{
								Address address = new Address
								{
									TransmissionResult = bingMapGeocodeResponse.authenticationResultCode,
									StreetAddress = current2.address.addressLine,
									City = current2.address.locality,
									State = current2.address.adminDistrict,
									Zip = current2.address.postalCode,
									FormattedAddress = current2.address.formattedAddress,
									Country = current2.address.countryRegion,
									County = current2.address.adminDistrict2 ?? "",
									VerificationDetails = bingMapGeocodeResponse.statusDescription.ToUpper()
								};
								if (current2.point != null && current2.point.coordinates != null && current2.point.coordinates.Count > 0)
								{
									address.Latitude = new double?(current2.point.coordinates[0]);
									address.Longitude = new double?(current2.point.coordinates[1]);
								}
								list.Add(address);
							}
						}
					}
				}
			}
			return list.ToArray();
		}

		private Address[] BingGeocode(string address, string key)
		{
			using (WebClient webClient = new WebClient())
			{
				string text = string.Format(BingGeocodingService.BingGeoCodeJsonServiceUrl, address, key);
				text += this._addressLanguageFormat;
				try
				{
					string jsonResponse = webClient.DownloadString(new Uri(text));
					Address[] array = this.HydrateJson(jsonResponse);
					if (array != null && array.Length != 0)
					{
						Address[] result = array;
						return result;
					}
				}
				catch (Exception arg_43_0)
				{
					if (arg_43_0.Message.Contains("401"))
					{
						List<Address> arg_6B_0 = new List<Address>();
						Address item = new Address
						{
							TransmissionResult = "InvalidCredentials"
						};
						arg_6B_0.Add(item);
						Address[] result = arg_6B_0.ToArray();
						return result;
					}
				}
			}
			return null;
		}
	}
}
