namespace ACadSharp.Entities
{
	public partial class TableEntity
	{
		[System.Flags]
		public enum TableCellStylePropertyFlags
		{
			None = 0x0,
			//Content format properties:
			DataType = 0x1,
			DataFormat = 0x2,
			Rotation = 0x4,
			BlockScale = 0x8,
			Alignment = 0x10,
			ContentColor = 0x20,
			TextStyle = 0x40,
			TextHeight = 0x80,
			AutoScale = 0x100,
			// Cell style properties:
			BackgroundColor = 0x200,
			MarginLeft = 0x400,
			MarginTop = 0x800,
			MarginRight = 0x1000,
			MarginBottom = 0x2000,
			ContentLayout = 0x4000,
			MarginHorizontalSpacing = 0x20000,
			MarginVerticalSpacing = 0x40000,
			//Row/column properties:
			MergeAll = 0x8000,
			//Table properties:
			FlowDirectionBottomToTop = 0x10000
		}
	}
}
