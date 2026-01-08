using ACadSharp.Attributes;
using ACadSharp.Entities;

namespace ACadSharp.Objects
{
	/// <summary>
	/// Represents a <see cref="TableStyle"/> object.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.TableStyle"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.TableStyle"/>
	/// </remarks>
	[DxfName(DxfFileToken.TableStyle)]
	[DxfSubClass(DxfSubclassMarker.TableStyle)]
	public class TableStyle : NonGraphicalObject
	{
		/// <summary>
		/// Table style description.
		/// </summary>
		/// <remarks>
		/// 255 characters maximum.
		/// </remarks>
		[DxfCodeValue(3)]
		public string Description { get; set; }

		[DxfCodeValue(71)]
		public short Flags { get; set; }

		[DxfCodeValue(70)]
		public TableFlowDirectionType FlowDirection { get; set; }

		[DxfCodeValue(40)]
		public double HorizontalCellMargin { get; set; }

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.TableStyle;

		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.UNLISTED;

		/// <inheritdoc/>
		public override string SubclassMarker { get; } = DxfSubclassMarker.TableStyle;

		[DxfCodeValue(281)]
		public bool SuppressHeaderRow { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the title should be suppressed.
		/// </summary>
		[DxfCodeValue(280)]
		public bool SuppressTitle { get; set; }

		[DxfCodeValue(41)]
		public double VerticalCellMargin { get; set; }

		public TableEntity.CellStyle DataCellStyle { get; } = new TableEntity.CellStyle();
		public TableEntity.CellStyle TitleCellStyle { get; } = new TableEntity.CellStyle();
		public TableEntity.CellStyle HeaderCellStyle { get; } = new TableEntity.CellStyle();
		public TableEntity.CellStyle TableCellStyle { get; } = new TableEntity.CellStyle();

		public const string DefaultName = "Standard";
	}
}