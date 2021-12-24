using ACadSharp.Attributes;
using ACadSharp.Types.Units;
using ACadSharp.IO.Templates;
using System;
using ACadSharp.Objects;
using ACadSharp.Blocks;

namespace ACadSharp.Tables
{
	public class BlockRecord : TableEntry
	{
		public override ObjectType ObjectType => ObjectType.BLOCK;

		public override string ObjectName => DxfFileToken.TableBlockRecord;

		public override string Name
		{
			get => ((Block)Owner).Name;
			set => ((Block)Owner).Name = value;
		}

		/// <summary>
		/// Block insertion units
		/// </summary>
		[DxfCodeValue(70)]
		public UnitsType Units { get; set; }

		/// <summary>
		/// Specifies whether the block can be exploded
		/// </summary>
		[DxfCodeValue(280)]
		public bool IsExplodable { get; set; }

		/// <summary>
		/// Specifies the scaling allowed for the block
		/// </summary>
		[DxfCodeValue(281)]
		public bool CanScale { get; set; }

		/// <summary>
		/// DXF: Binary data for bitmap preview(optional)
		/// </summary>
		[DxfCodeValue(310)]
		public byte[] Preview { get; set; }

		/// <summary>
		/// Associated Layout
		/// </summary>
		[DxfCodeValue(340)]
		public Layout Layout { get; set; }

		public BlockRecord(Block owner) : base()
		{
			this.Owner = owner;
		}
	}
}
