namespace ACadSharp.Objects
{
	/// <summary>
	/// Type of a subentity, with the values AutoCAD uses (<c>AcDb::SubentType</c>), as found in the DXF code 73 of an
	/// osnap point reference.
	/// </summary>
	public enum SubentType : short
	{
		Unknown = 0,

		Face = 1,

		Edge = 2,

		Vertex = 3
	}
}
