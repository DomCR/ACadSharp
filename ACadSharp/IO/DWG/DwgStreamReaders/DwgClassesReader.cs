using ACadSharp.Classes;
using CSUtilities.IO;

namespace ACadSharp.IO.DWG
{
	internal class DwgClassesReader : DwgSectionIO
	{
		public override string SectionName { get { return DwgSectionDefinition.Classes; } }

		private DwgFileHeader _fileHeader;

		public DwgClassesReader(ACadVersion version, DwgFileHeader fileHeader) : base(version)
		{
			this._fileHeader = fileHeader;
		}

		public DxfClassCollection Read(IDwgStreamReader sreader)
		{
			DxfClassCollection classes = new DxfClassCollection();

			//SN : 0x8D 0xA1 0xC4 0xB8 0xC4 0xA9 0xF8 0xC5 0xC0 0xDC 0xF4 0x5F 0xE7 0xCF 0xB6 0x8A
			this.checkSentinel(sreader, DwgSectionDefinition.StartSentinels[SectionName]);

			//RL : size of class data area
			long size = sreader.ReadRawLong();
			long endSection = sreader.Position + size;

			//R2010+ (only present if the maintenance version is greater than 3!)
			if (this._fileHeader.AcadVersion >= ACadVersion.AC1024
				&& this._fileHeader.AcadMaintenanceVersion > 3
				|| this._fileHeader.AcadVersion > ACadVersion.AC1027)
			{
				//RL : unknown, possibly the high 32 bits of a 64-bit size?
				long unknown = sreader.ReadRawLong();
			}

			long flagPos = 0;
			//+R2007 Only:
			if (R2007Plus)
			{
				//Setup readers
				flagPos = sreader.PositionInBits() + sreader.ReadRawLong() - 1L;
				long savedOffset = sreader.PositionInBits();
				endSection = sreader.SetPositionByFlag(flagPos);

				sreader.SetPositionInBits(savedOffset);

				//Setup the text reader for versions 2007 and above
				IDwgStreamReader textReader = DwgStreamReaderBase.GetStreamHandler(_version,
					//Create a copy of the stream
					new StreamIO(sreader.Stream, true).Stream);
				//Set the position and use the flag
				textReader.SetPositionInBits(endSection);

				sreader = new DwgMergedReader(sreader, textReader, null);

				//BL: 0x00
				sreader.ReadBitLong();
				//B : flag - to find the data string at the end of the section
				sreader.ReadBit();
			}

			if (this._fileHeader.AcadVersion == ACadVersion.AC1018)
			{
				//BS : Maxiumum class number
				sreader.ReadBitShort();
				//RC: 0x00
				sreader.ReadRawChar();
				//RC: 0x00
				sreader.ReadRawChar();
				//B : true
				sreader.ReadBit();
			}

			//We read sets of these until we exhaust the data.
			while (getCurrPos(sreader) < endSection)
			{
				DxfClass dxfClass = new DxfClass();
				//BS : classnum
				dxfClass.ClassNumber = sreader.ReadBitShort();
				//BS : version – in R14, becomes a flag indicating whether objects can be moved, edited, etc.
				dxfClass.ProxyFlags = (ProxyFlags)sreader.ReadBitShort();

				//TV : appname
				dxfClass.ApplicationName = sreader.ReadVariableText();
				//TV: cplusplusclassname
				dxfClass.CppClassName = sreader.ReadVariableText();
				//TV : classdxfname
				dxfClass.DxfName = sreader.ReadVariableText();

				//B : wasazombie
				dxfClass.WasZombie = sreader.ReadBit();
				//BS : itemclassid -- 0x1F2 for classes which produce entities, 0x1F3 for classes which produce objects.
				dxfClass.ItemClassId = sreader.ReadBitShort();

				if (this._fileHeader.AcadVersion == ACadVersion.AC1018)
				{
					//BL : Number of objects created of this type in the current DB(DXF 91).
					sreader.ReadBitLong();
					//BS : Dwg Version
					sreader.ReadBitShort();
					//BS : Maintenance release version.
					sreader.ReadBitShort();
					//BL : Unknown(normally 0L)
					sreader.ReadBitLong();
					//BL : Unknown(normally 0L)
					sreader.ReadBitLong();
				}
				else if (this._fileHeader.AcadVersion > ACadVersion.AC1018)
				{

					//BL : Number of objects created of this type in the current DB(DXF 91).
					dxfClass.InstanceCount = sreader.ReadBitLong();

					//BS : Dwg Version
					sreader.ReadBitLong();
					//BS : Maintenance release version.
					sreader.ReadBitLong();
					//BL : Unknown(normally 0L)
					sreader.ReadBitLong();
					//BL : Unknown(normally 0L)
					sreader.ReadBitLong();
				}

				classes.Add(dxfClass);
			}

			if (R2007Plus)
			{
				sreader.SetPositionInBits(flagPos + 1);
			}

			//RS: CRC
			sreader.ResetShift();

			//0x72,0x5E,0x3B,0x47,0x3B,0x56,0x07,0x3A,0x3F,0x23,0x0B,0xA0,0x18,0x30,0x49,0x75
			this.checkSentinel(sreader, DwgSectionDefinition.EndSentinels[SectionName]);

			return classes;
		}

		private long getCurrPos(IDwgStreamReader sreader)
		{
			if (R2007Plus)
			{
				return sreader.PositionInBits();
			}
			else
			{
				return sreader.Position;
			}
		}
	}
}
