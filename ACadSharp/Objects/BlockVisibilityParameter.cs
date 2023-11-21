using System;
using System.Collections.Generic;

using ACadSharp.Attributes;
using ACadSharp.Entities;

using CSMath;

namespace ACadSharp.Objects {

	public class BlockVisibilityParameter : CadObject {

		public override ObjectType ObjectType => ObjectType.UNLISTED;

		public override string ObjectName => DxfFileToken.ObjectBlockVisibilityParameter;

		public override string SubclassMarker => DxfSubclassMarker.BlockVisibilityParameter;


		public IList<Entity> Entities { get; } = new List<Entity>();


		public IList<SubBlock> SubBlocks { get; } = new List<SubBlock>();

		[DxfCodeValue(1010, 1020, 1030)]
		public XYZ BasePosition { get; internal set; }
		
		[DxfCodeValue(300)]
		public string ParameterType { get; internal set; }

		[DxfCodeValue(301)]
		public string Name { get; internal set; }

		[DxfCodeValue(302)]
		public string Description { get; internal set; }

		[DxfCodeValue(302)]
		public int L91 { get; internal set; }


		public class SubBlock : ICloneable {

			public string Name { get; set; }

			public IList<Entity> Entities { get; } = new List<Entity>();


			public object Clone()
			{
				SubBlock clone = (SubBlock)MemberwiseClone();

				clone.Entities.Clear();
				foreach (var item in this.Entities)
				{
					clone.Entities.Add((Entity)item.Clone());
				}

				return clone;
			}
		}


		/// <inheritdoc/>
		public override CadObject Clone()
		{
			BlockVisibilityParameter clone = (BlockVisibilityParameter)base.Clone();

			clone.Entities.Clear();
			foreach (var item in this.Entities)
			{
				clone.Entities.Add((Entity)item.Clone());
			}

			clone.SubBlocks.Clear();
			foreach (var item in this.SubBlocks)
			{
				clone.Entities.Add((Entity)item.Clone());
			}

			return clone;
		}
	}
}