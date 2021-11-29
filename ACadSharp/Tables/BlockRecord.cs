using ACadSharp.Attributes;
using ACadSharp.Types.Units;
using ACadSharp.IO.Templates;
using System;

namespace ACadSharp.Tables
{
	public class BlockRecord : TableEntry
	{
		public override ObjectType ObjectType => ObjectType.BLOCK;
		public override string ObjectName => DxfFileToken.TableBlockRecord;
		/// <summary>
		/// Hard-pointer ID/handle to associated LAYOUT object
		/// </summary>
		[DxfCodeValue(DxfCode.HardPointerId)]
		public string LayoutHanlde { get; set; }
		/// <summary>
		/// Block insertion units.
		/// </summary>
		[DxfCodeValue(DxfCode.Int16)]
		public UnitsType Units { get; set; }
		/// <summary>
		/// Specifies whether the block can be exploded.
		/// </summary>
		[DxfCodeValue(DxfCode.Int8)]
		public bool IsExplodable { get; set; }
		/// <summary>
		/// Specifies the scaling allowed for the block.
		/// </summary>
		[DxfCodeValue(DxfCode.RenderMode)]
		public bool CanScale { get; set; }
		/// <summary>
		/// DXF: Binary data for bitmap preview(optional)
		/// </summary>
		[DxfCodeValue(DxfCode.BinaryChunk)]
		public byte[] Preview { get; private set; }


		public BlockRecord(string name) : base(name)
		{
			if (string.IsNullOrEmpty(name))
				throw new ArgumentNullException(nameof(name), "Block record must have a name.");
		}

		internal BlockRecord(DxfEntryTemplate template) : base(template)
		{
			if (string.IsNullOrEmpty(template.TableName))
				throw new ArgumentNullException(nameof(template.TableName), "Block record must have a name.");
		}
	}
}
