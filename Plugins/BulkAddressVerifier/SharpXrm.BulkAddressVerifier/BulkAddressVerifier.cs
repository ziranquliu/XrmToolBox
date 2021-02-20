using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace SharpXrm.BulkAddressVerifier
{
	[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0"), DebuggerNonUserCode, CompilerGenerated]
	internal class BulkAddressVerifier
	{
		private static ResourceManager resourceMan;

		private static CultureInfo resourceCulture;

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static ResourceManager ResourceManager
		{
			get
			{
				if (BulkAddressVerifier.resourceMan == null)
				{
					BulkAddressVerifier.resourceMan = new ResourceManager("SharpXrm.BulkAddressVerifier.BulkAddressVerifier", typeof(BulkAddressVerifier).Assembly);
				}
				return BulkAddressVerifier.resourceMan;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static CultureInfo Culture
		{
			get
			{
				return BulkAddressVerifier.resourceCulture;
			}
			set
			{
				BulkAddressVerifier.resourceCulture = value;
			}
		}

		internal static Bitmap close
		{
			get
			{
				return (Bitmap)BulkAddressVerifier.ResourceManager.GetObject("close", BulkAddressVerifier.resourceCulture);
			}
		}

		internal static Bitmap Copy
		{
			get
			{
				return (Bitmap)BulkAddressVerifier.ResourceManager.GetObject("Copy", BulkAddressVerifier.resourceCulture);
			}
		}

		internal static Bitmap retrieve
		{
			get
			{
				return (Bitmap)BulkAddressVerifier.ResourceManager.GetObject("retrieve", BulkAddressVerifier.resourceCulture);
			}
		}

		internal static Bitmap start
		{
			get
			{
				return (Bitmap)BulkAddressVerifier.ResourceManager.GetObject("start", BulkAddressVerifier.resourceCulture);
			}
		}

		internal BulkAddressVerifier()
		{
		}
	}
}
