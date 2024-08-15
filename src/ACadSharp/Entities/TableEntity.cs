using ACadSharp.Attributes;
using ACadSharp.Objects;
using CSMath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Represents a <see cref="TableEntity"/> entity.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.EntityTable"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.TableEntity"/>
	/// </remarks>
	[DxfName(DxfFileToken.EntityTable)]
	[DxfSubClass(DxfSubclassMarker.TableEntity)]
	public class TableEntity : Insert
	{
		public class TableAttribute
		{
			public string Value { get; internal set; }
		}

		public class CellValue
		{
			internal int Flags { get; set; }

			public bool IsEmpty
			{
				get
				{
					return (this.Flags & 1) != 0;
				}
				set
				{
					if (value)
					{
						this.Flags |= 1;
					}
					else
					{
						this.Flags &= -2;
					}
				}
			}

			public object Value { get; internal set; }
		}

		public enum CellValueType
		{
			/// <summary>
			/// Unknown
			/// </summary>
			Unknown = 0,
			/// <summary>
			/// 32 bit Long value
			/// </summary>
			Long = 1,
			/// <summary>
			/// Double value
			/// </summary>
			Double = 2,
			/// <summary>
			/// String value
			/// </summary>
			String = 4,
			/// <summary>
			/// Date value
			/// </summary>
			Date = 8,
			/// <summary>
			/// 2D point value
			/// </summary>
			Point2D = 0x10,
			/// <summary>
			/// 3D point value
			/// </summary>
			Point3D = 0x20,
			/// <summary>
			/// Object handle value
			/// </summary>
			Handle = 0x40,
			/// <summary>
			/// Buffer value
			/// </summary>
			Buffer = 0x80,
			/// <summary>
			/// Result buffer value
			/// </summary>
			ResultBuffer = 0x100,
			/// <summary>
			/// General
			/// </summary>
			General = 0x200
		}

		public enum CellStypeType
		{
			Cell = 1,
			Row = 2,
			Column = 3,
			FormattedTableData = 4,
			Table = 5
		}

		[System.Flags]
		public enum TableCellStateFlags
		{
			/// <summary>
			/// None
			/// </summary>
			None = 0x0,
			/// <summary>
			/// Content  locked
			/// </summary>
			ContentLocked = 0x1,
			/// <summary>
			/// Content read only
			/// </summary>
			ContentReadOnly = 0x2,
			/// <summary>
			/// Linked.
			/// </summary>
			Linked = 0x4,
			/// <summary>
			/// Content modifed after update
			/// </summary>
			ContentModifiedAfterUpdate = 0x8,
			/// <summary>
			/// Format locked
			/// </summary>
			FormatLocked = 0x10,
			/// <summary>
			/// Format readonly
			/// </summary>
			FormatReadOnly = 0x20,
			/// <summary>
			/// Format was modified after update
			/// </summary>
			FormatModifiedAfterUpdate = 0x40,
		}

		public class CellStyle
		{
			public CellStypeType Type { get; set; }
		}

		public class CustomDataEntry
		{
			public string Name { get; internal set; }
			public CellValue Value { get; internal set; } = new CellValue();
		}

		public class Cell
		{
			public string ToolTip { get; internal set; }
			public TableCellStateFlags StateFlags { get; internal set; }
			public bool HasLinkedData { get; internal set; }
			public int CustomData { get; internal set; }
			public List<CustomDataEntry> CustomDataCollection { get; internal set; } = new();

			public List<Content> Contents { get; internal set; } = new();

			public class Content
			{

			}
		}

		public enum TableCellContentType
		{
			Value,
			Field,
			Block
		}

		public class Row
		{
			public double Height { get; internal set; }

			public List<Cell> Cells { get; internal set; } = new();
		}

		public class Column
		{
			public double Width { get; internal set; }
			public string Name { get; internal set; }
			public int CustomData { get; internal set; }
			public List<CustomDataEntry> CustomDataCollection { get; internal set; }
		}

		public class Content
		{
			public string Name { get; set; }

			public string Description { get; set; }
		}

		internal class BreakData
		{
			internal struct BreakHeight
			{
				public XYZ Position { get; internal set; }
				public double Height { get; internal set; }
			}

			public BreakOptionFlags Flags { get; internal set; }
			public BreakFlowDirection FlowDirection { get; internal set; }
			public double BreakSpacing { get; internal set; }
			public List<BreakHeight> Heights { get; internal set; } = new List<BreakHeight>();
		}


		internal class BreakRowRange
		{
			public XYZ Position { get; internal set; }
			public int StartRowIndex { get; internal set; }
			public int EndRowIndex { get; internal set; }
		}

		[System.Flags]
		public enum BreakOptionFlags
		{
			/// <summary>
			/// None
			/// </summary>
			None = 0,
			/// <summary>
			/// Enable breaks
			/// </summary>
			EnableBreaks = 1,
			/// <summary>
			/// Repeat top labels
			/// </summary>
			RepeatTopLabels = 2,
			/// <summary>
			/// Repeat bottom labels
			/// </summary>
			RepeatBottomLabels = 4,
			/// <summary>
			/// Allow manual positions
			/// </summary>
			AllowManualPositions = 8,
			/// <summary>
			/// Allow manual heights
			/// </summary>
			AllowManualHeights = 16
		}

		public enum BreakFlowDirection
		{
			/// <summary>
			/// Right
			/// </summary>
			Right = 1,
			/// <summary>
			/// Vertical
			/// </summary>
			Vertical = 2,
			/// <summary>
			/// Left
			/// </summary>
			Left = 4
		}

		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.UNLISTED;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.EntityTable;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.TableEntity;

		/// <summary>
		/// Table data version
		/// </summary>
		[DxfCodeValue(280)]
		public short Version { get; internal set; }

		/// <summary>
		/// Table Style
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Handle, 342)]
		public TableStyle Style { get; set; }

		//343	Hard pointer ID of the owning BLOCK record

		/// <summary>
		/// Horizontal direction vector
		/// </summary>
		[DxfCodeValue(11, 21, 31)]
		public XYZ HorizontalDirection { get; set; }

		/// <summary>
		/// Flag for table value
		/// </summary>
		[DxfCodeValue(90)]
		public short ValueFlag { get; internal set; }

		/// <summary>
		/// Number of rows
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Count, 91)]
		public List<Row> Rows { get; set; } = new List<Row>();

		/// <summary>
		/// Number of columns
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Count, 92)]
		public List<Column> Columns { get; set; } = new List<Column>();

		/// <summary>
		/// Flag for an override
		/// </summary>
		[DxfCodeValue(93)]
		public bool OverrideFlag { get; set; }

		public Content TableContent { get; set; } = new();

		internal BreakData TableBreakData { get; set; } = new();

		internal List<BreakRowRange> BreakRowRanges { get; set; } = new();

		//94

		//Flag for an override of border color

		//95

		//Flag for an override of border lineweight

		//96

		//Flag for an override of border visibility

		//141

		//Row height; this value is repeated, 1 value per row

		//142

		//Column height; this value is repeated, 1 value per column

		//171

		//Cell type; this value is repeated, 1 value per cell:

		//1 = text type

		//2 = block type

		//172

		//Cell flag value; this value is repeated, 1 value per cell

		//173

		//Cell merged value; this value is repeated, 1 value per cell

		//174

		//Boolean flag indicating if the autofit option is set for the cell; this value is repeated, 1 value per cell

		//175

		//Cell border width(applicable only for merged cells); this value is repeated, 1 value per cell

		//176

		//Cell border height(applicable for merged cells); this value is repeated, 1 value per cell

		//91

		//Cell override flag; this value is repeated, 1 value per cell(from AutoCAD 2007)

		//178

		//Flag value for a virtual edge

		//145

		//Rotation value(real; applicable for a block-type cell and a text-type cell)

		//344

		//Hard pointer ID of the FIELD object. This applies only to a text-type cell.If the text in the cell contains one or more fields, only the ID of the FIELD object is saved.The text string (group codes 1 and 3) is ignored

		//1
		//Text string in a cell.If the string is shorter than 250 characters, all characters appear in code 1. If the string is longer than 250 characters, it is divided into chunks of 250 characters.The chunks are contained in one or more code 2 codes.If code 2 codes are used, the last group is a code 1 and is shorter than 250 characters.This value applies only to text-type cells and is repeated, 1 value per cell

		//2	
		//Text string in a cell, in 250-character chunks; optional.This value applies only to text-type cells and is repeated, 1 value per cell

		//340

		//Hard-pointer ID of the block table record. This value applies only to block-type cells and is repeated, 1 value per cell

		//144

		//Block scale(real). This value applies only to block-type cells and is repeated, 1 value per cell

		//179

		//Number of attribute definitions in the block table record(applicable only to a block-type cell)

		//331

		//Soft pointer ID of the attribute definition in the block table record, referenced by group code 179 (applicable only for a block-type cell). This value is repeated once per attribute definition

		//300

		//Text string value for an attribute definition, repeated once per attribute definition and applicable only for a block-type cell

		//7

		//Text style name(string); override applied at the cell level

		//140

		//Text height value; override applied at the cell level

		//170

		//Cell alignment value; override applied at the cell level

		//64

		//Value for the color of cell content; override applied at the cell level

		//63

		//Value for the background(fill) color of cell content; override applied at the cell level

		//69

		//True color value for the top border of the cell; override applied at the cell level

		//65

		//True color value for the right border of the cell; override applied at the cell level

		//66

		//True color value for the bottom border of the cell; override applied at the cell level

		//68

		//True color value for the left border of the cell; override applied at the cell level

		//279

		//Lineweight for the top border of the cell; override applied at the cell level

		//275

		//Lineweight for the right border of the cell; override applied at the cell level

		//276

		//Lineweight for the bottom border of the cell; override applied at the cell level

		//278

		//Lineweight for the left border of the cell; override applied at the cell level

		//283

		//Boolean flag for whether the fill color is on; override applied at the cell level

		//289

		//Boolean flag for the visibility of the top border of the cell; override applied at the cell level

		//285

		//Boolean flag for the visibility of the right border of the cell; override applied at the cell level

		//286

		//Boolean flag for the visibility of the bottom border of the cell; override applied at the cell level

		//288

		//Boolean flag for the visibility of the left border of the cell; override applied at the cell level

		//70

		//Flow direction; override applied at the table entity level

		//40

		//Horizontal cell margin; override applied at the table entity level

		//41

		//Vertical cell margin; override applied at the table entity level

		//280

		//Flag for whether the title is suppressed; override applied at the table entity level

		//281

		//Flag for whether the header row is suppressed; override applied at the table entity level

		//7

		//Text style name(string); override applied at the table entity level.There may be one entry for each cell type

		//140

		//Text height (real); override applied at the table entity level.There may be one entry for each cell type

		//170

		//Cell alignment (integer); override applied at the table entity level.There may be one entry for each cell type

		//63

		//Color value for cell background or for the vertical, left border of the table; override applied at the table entity level.There may be one entry for each cell type

		//64

		//Color value for cell content or for the horizontal, top border of the table; override applied at the table entity level.There may be one entry for each cell type

		//65

		//Color value for the horizontal, inside border lines; override applied at the table entity level

		//66

		//Color value for the horizontal, bottom border lines; override applied at the table entity level

		//68

		//Color value for the vertical, inside border lines; override applied at the table entity level

		//69

		//Color value for the vertical, right border lines; override applied at the table entity level

		//283

		//Flag for whether background color is enabled(default = 0); override applied at the table entity level.There may be one entry for each cell type:

		//0 = Disabled

		//1 = Enabled

		//274-279

		//Lineweight for each border type of the cell (default = kLnWtByBlock); override applied at the table entity level.There may be one group for each cell type

		//284-289

		//Flag for visibility of each border type of the cell (default = 1); override applied at the table entity level.There may be one group for each cell type:

		//0 = Invisible

		//1 = Visible

		//97

		//Standard/title/header row data type
		//98

		//Standard/title/header row unit type
		//4

		//Standard/title/header row format string

		//177

		//Cell override flag value(before AutoCAD 2007)

		//92

		//Extended cell flags(from AutoCAD 2007)
		//301	Cell value block begin(from AutoCAD 2007)
		//302	
		//Text string in a cell.If the string is shorter than 250 characters, all characters appear in code 302. If the string is longer than 250 characters, it is divided into chunks of 250 characters.The chunks are contained in one or more code 303 codes.If code 393 codes are used, the last group is a code 1 and is shorter than 250 characters.This value applies only to text-type cells and is repeated, 1 value per cell (from AutoCAD 2007)

		//303	
		//Text string in a cell, in 250-character chunks; optional.This value applies only to text-type cells and is repeated, 302 value per cell(from AutoCAD 2007)
	}
}
