namespace ACadSharp.Entities.ProxyGraphics;

public enum ProxyColorMethod : byte
	{
		ByLayer = 0xC0,

		ByBlock = 0xC1,

		ByColor = 0xC2,

		ByACI = 0xC3,

		Foreground = 0xC5,

		None = 0xC8
	}