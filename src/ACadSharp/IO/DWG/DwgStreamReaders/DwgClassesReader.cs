using ACadSharp.Classes;
using CSUtilities.IO;

namespace ACadSharp.IO.DWG
{
	internal class DwgClassesReader : DwgSectionIO
	{
		public override string SectionName { get { return DwgSectionDefinition.Classes; } }

		private DwgFileHeader _fileHeader;

		private IDwgStreamReader _sreader;

		public DwgClassesReader(ACadVersion version, IDwgStreamReader sreader, DwgFileHeader fileHeader) : base(version)
		{
			this._sreader = sreader;
			this._fileHeader = fileHeader;
		}

		public DxfClassCollection Read()
		{
			DxfClassCollection classes = new DxfClassCollection();

			//SN : 0x8D 0xA1 0xC4 0xB8 0xC4 0xA9 0xF8 0xC5 0xC0 0xDC 0xF4 0x5F 0xE7 0xCF 0xB6 0x8A
			this.checkSentinel(this._sreader, DwgSectionDefinition.StartSentinels[this.SectionName]);

			//RL : size of class data area
			long size = this._sreader.ReadRawLong();
			long endSection = this._sreader.Position + size;

			//R2010+ (only present if the maintenance version is greater than 3!)
			if (this._fileHeader.AcadVersion >= ACadVersion.AC1024
				&& this._fileHeader.AcadMaintenanceVersion > 3
				|| this._fileHeader.AcadVersion > ACadVersion.AC1027)
			{
				//RL : unknown, possibly the high 32 bits of a 64-bit size?
				long unknown = this._sreader.ReadRawLong();
			}

			long flagPos = 0;
			//+R2007 Only:
			if (this.R2007Plus)
			{
				//Setup readers
				flagPos = this._sreader.PositionInBits() + this._sreader.ReadRawLong() - 1L;
				long savedOffset = this._sreader.PositionInBits();
				endSection = this._sreader.SetPositionByFlag(flagPos);

				this._sreader.SetPositionInBits(savedOffset);

				//Setup the text reader for versions 2007 and above
				IDwgStreamReader textReader = DwgStreamReaderBase.GetStreamHandler(this._version,
					//Create a copy of the stream
					new StreamIO(this._sreader.Stream, true).Stream);
				//Set the position and use the flag
				textReader.SetPositionInBits(endSection);

				this._sreader = new DwgMergedReader(this._sreader, textReader, null);

				//BL: 0x00
				this._sreader.ReadBitLong();
				//B : flag - to find the data string at the end of the section
				this._sreader.ReadBit();
			}

			if (this._fileHeader.AcadVersion == ACadVersion.AC1018)
			{
				//BS : Maximum class number
				this._sreader.ReadBitShort();
				//RC: 0x00
				this._sreader.ReadRawChar();
				//RC: 0x00
				this._sreader.ReadRawChar();
				//B : true
				this._sreader.ReadBit();
			}

			//We read sets of these until we exhaust the data.
			while (this.getCurrPos(this._sreader) < endSection)
			{
				DxfClass dxfClass = new DxfClass();
				//BS : classnum
				dxfClass.ClassNumber = this._sreader.ReadBitShort();
				//BS : version – in R14, becomes a flag indicating whether objects can be moved, edited, etc.
				dxfClass.ProxyFlags = (ProxyFlags)this._sreader.ReadBitShort();

				//TV : appname
				dxfClass.ApplicationName = this._sreader.ReadVariableText();
				//TV: cplusplusclassname
				dxfClass.CppClassName = this._sreader.ReadVariableText();
				//TV : classdxfname
				dxfClass.DxfName = this._sreader.ReadVariableText();

				//B : wasazombie
				dxfClass.WasZombie = this._sreader.ReadBit();
				//BS : itemclassid -- 0x1F2 for classes which produce entities, 0x1F3 for classes which produce objects.
				dxfClass.ItemClassId = this._sreader.ReadBitShort();
				if (dxfClass.ItemClassId == 0x1F2)
				{
					dxfClass.IsAnEntity = true;
				}
				else if (dxfClass.ItemClassId == 0x1F3)
				{
					dxfClass.IsAnEntity = false;
				}
				else
				{
					this.notify($"Invalid DxfClass id value: {dxfClass.ItemClassId} for {dxfClass.CppClassName}", NotificationType.Error);
				}

				if (this.R2004Plus)
				{
					//BL : Number of objects created of this type in the current DB(DXF 91).
					dxfClass.InstanceCount = this._sreader.ReadBitLong();

					//BS : Dwg Version
					dxfClass.DwgVersion = (ACadVersion)this._sreader.ReadBitLong();
					//BS : Maintenance release version.
					dxfClass.MaintenanceVersion = (short)this._sreader.ReadBitLong();

					//BL : Unknown(normally 0L)
					this._sreader.ReadBitLong();
					//BL : Unknown(normally 0L)
					this._sreader.ReadBitLong();
				}

				classes.AddOrUpdate(dxfClass);
			}

			if (this.R2007Plus)
			{
				this._sreader.SetPositionInBits(flagPos + 1);
			}

			//RS: CRC
			this._sreader.ResetShift();

			//0x72,0x5E,0x3B,0x47,0x3B,0x56,0x07,0x3A,0x3F,0x23,0x0B,0xA0,0x18,0x30,0x49,0x75
			this.checkSentinel(this._sreader, DwgSectionDefinition.EndSentinels[this.SectionName]);

			return classes;
		}

		private long getCurrPos(IDwgStreamReader sreader)
		{
			if (this.R2007Plus)
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
