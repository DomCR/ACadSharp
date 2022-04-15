using CSUtilities.Text;
using System;
using System.Collections.Generic;
using System.Text;

namespace ACadSharp.IO.DWG
{
	internal abstract class DwgFileHeader
	{
		public ACadVersion AcadVersion { get; }
		public long PreviewAddress { get; set; } = -1;
		public int AcadMaintenanceVersion { get; set; }

		public DwgFileHeader() { }
		public DwgFileHeader(ACadVersion version)
		{
			AcadVersion = version;
		}

		public static DwgFileHeader GetFileHeader(ACadVersion version)
		{
			switch (version)
			{
				case ACadVersion.Unknown:
					throw new Exception();
				case ACadVersion.MC0_0:
				case ACadVersion.AC1_2:
				case ACadVersion.AC1_4:
				case ACadVersion.AC1_50:
				case ACadVersion.AC2_10:
				case ACadVersion.AC1002:
				case ACadVersion.AC1003:
				case ACadVersion.AC1004:
				case ACadVersion.AC1006:
				case ACadVersion.AC1009:
					throw new NotSupportedException();
				case ACadVersion.AC1012:
				case ACadVersion.AC1014:
				case ACadVersion.AC1015:
					return new DwgFileHeader15(version);
				case ACadVersion.AC1018:
					return new DwgFileHeader18(version);
				case ACadVersion.AC1021:
					return new DwgFileHeader21(version);
				case ACadVersion.AC1024:
				case ACadVersion.AC1027:
				case ACadVersion.AC1032:
					//Check if it works...
					return new DwgFileHeader18(version);
				default:
					break;
			}

			return null;
		}
	}
}
