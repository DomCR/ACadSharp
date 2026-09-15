namespace ACadSharp.Entities.ProxyGraphics;

[System.Flags]
public enum PrimitiveFlags 
{
	None = 0x0000,
	HasColors = 0x0001,
	HasLayers = 0x0002,
	HasLineTypes = 0x0004,
	HasMarkers = 0x0020,
	HasVisibilities = 0x0040,
	HasNormals = 0x0080,
	HasOrientation = 0x0400
}