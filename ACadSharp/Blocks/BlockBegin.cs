using ACadSharp.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ACadSharp.Blocks
{
	/// <summary>
	/// Class to read the dwg BLOCK type.
	/// </summary>
	[Obsolete]
	public class BlockBegin : Entity
	{
		public override string ObjectName => DxfFileToken.Block;

		public override ObjectType ObjectType => ObjectType.BLOCK;

		public string Name { get; set; }
	}
}
