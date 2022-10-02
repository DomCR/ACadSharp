using ACadSharp.Entities;

namespace ACadSharp.IO.DWG
{
	internal static class DwgLineWeightConverter
	{
		public static readonly LineweightType[] IndexedValue = new LineweightType[]
		{
				 LineweightType.W0,
				 LineweightType.W5,
				 LineweightType.W9,
				 LineweightType.W13,
				 LineweightType.W15,
				 LineweightType.W18,
				 LineweightType.W20,
				 LineweightType.W25,
				 LineweightType.W30,
				 LineweightType.W35,
				 LineweightType.W40,
				 LineweightType.W50,
				 LineweightType.W53,
				 LineweightType.W60,
				 LineweightType.W70,
				 LineweightType.W80,
				 LineweightType.W90,
				 LineweightType.W100,
				 LineweightType.W106,
				 LineweightType.W120,
				 LineweightType.W140,
				 LineweightType.W158,
				 LineweightType.W200,
				 LineweightType.W211
		};

		public static LineweightType ToValue(byte b)
		{
			switch (b)
			{
				case 28:
				case 29:
					return LineweightType.ByLayer;
				case 30:
					return LineweightType.ByBlock;
				case 31:
					return LineweightType.Default;
				default:
					if (b < 0 || b >= IndexedValue.Length)
					{
						return LineweightType.Default;
					}
					return IndexedValue[b];
			}
		}
	}
}
