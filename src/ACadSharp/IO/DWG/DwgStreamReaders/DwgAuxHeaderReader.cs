namespace ACadSharp.IO.DWG.DwgStreamReaders;

internal class DwgAuxHeaderReader : DwgSectionIO
{
	public override string SectionName => DwgSectionDefinition.AuxHeader;

	private readonly IDwgStreamReader _sreader;

	public DwgAuxHeaderReader(ACadVersion version, IDwgStreamReader sreader)
	: base(version)
	{
		this._sreader = sreader;
	}

	public void Read()
	{
		var v1 = this._sreader.ReadByte();
		var v2 = this._sreader.ReadByte();
		var v3 = this._sreader.ReadByte();

		//RS: DWG version:
		ACadVersion version = (ACadVersion)this._sreader.ReadShort();
		long maintenanceVersion = this.readMaintenanceVersion();

		//RL: Number of saves (starts at 1)
		var nsaves = this._sreader.ReadRawLong();
		//RL: -1
		var check = this._sreader.ReadRawLong();

		//RS: Number of saves part 1( = Number of saves – number of saves part 2)
		var nsaves1 = this._sreader.ReadShort();
		//RS: Number of saves part 2( = Number of saves – 0x7fff if Number of saves > 0x7fff, otherwise 0)
		var nsaves2 = this._sreader.ReadShort();

		//RL: 0
		var value0 = this._sreader.ReadRawLong();

		//RS: DWG version string
		ACadVersion version1 = (ACadVersion)this._sreader.ReadShort();
		//RS : Maintenance version
		long maintenanceVersion1 = this.readMaintenanceVersion();
		//RS: DWG version string
		ACadVersion version2 = (ACadVersion)this._sreader.ReadShort();
		//RS : Maintenance version
		long maintenanceVersion2 = this.readMaintenanceVersion();

		//RS: 0x0005
		var value5 = this._sreader.ReadShort();
		//RS: 0x0893
		var value893 = this._sreader.ReadShort();
		//RS: 0x0005
		value5 = this._sreader.ReadShort();
		//RS: 0x0893
		value893 = this._sreader.ReadShort();
		//RS: 0x0000
		value0 = this._sreader.ReadShort();
		//RS: 0x0001
		var value1 = this._sreader.ReadShort();
		//RL: 0x0000
		value0 = this._sreader.ReadRawLong();
		//RL: 0x0000
		value0 = this._sreader.ReadRawLong();
		//RL: 0x0000
		value0 = this._sreader.ReadRawLong();
		//RL: 0x0000
		value0 = this._sreader.ReadRawLong();
		//RL: 0x0000
		value0 = this._sreader.ReadRawLong();

		//TD: TDCREATE(creation datetime)
		this._sreader.Read8BitJulianDate();

		//TD: TDUPDATE(update datetime)
		this._sreader.Read8BitJulianDate();

		//RL: HANDSEED(Handle seed) if < 0x7fffffff, otherwise - 1.
		this._sreader.ReadRawLong();
		//RL : Educational plot stamp(default value is 0)
		var edflag = this._sreader.ReadRawLong();
		//RS: 0
		this._sreader.ReadShort();
		//RS: Number of saves part 1 – number of saves part 2
		this._sreader.ReadShort();
		//RL: 0
		this._sreader.ReadRawLong();
		//RL: 0
		this._sreader.ReadRawLong();
		//RL: 0
		this._sreader.ReadRawLong();
		//RL: Number of saves
		this._sreader.ReadRawLong();
		//RL : 0
		this._sreader.ReadRawLong();
		//RL: 0
		this._sreader.ReadRawLong();
		//RL: 0
		this._sreader.ReadRawLong();
		//RL: 0
		this._sreader.ReadRawLong();

		//R2018 +
		if (this.R2018Plus)
		{
			//RS : 0
			this._sreader.ReadShort();
			//RS : 0
			this._sreader.ReadShort();
			//RS : 0
			this._sreader.ReadShort();
		}
	}

	private long readMaintenanceVersion()
	{
		if (this._version > ACadVersion.AC1027)
		{
			//RS: Maintenance version
			return this._sreader.ReadRawLong();
		}
		else
		{
			//RS: Maintenance version
			return this._sreader.ReadShort();
		}
	}
}