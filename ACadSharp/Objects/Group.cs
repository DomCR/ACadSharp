using ACadSharp.Attributes;
using ACadSharp.Entities;
using System.Collections.Generic;

namespace ACadSharp.Objects
{
	public class Group : CadObject
	{
		public override ObjectType ObjectType => ObjectType.GROUP;

		public override string ObjectName => DxfFileToken.TableGroup;

		/// <summary>
		/// Group description.
		/// </summary>
		[DxfCodeValue(300)]
		public string Description { get; set; }

		//70	“Unnamed” flag: 1 = Unnamed; 0 = Named

		[DxfCodeValue(71)]
		public bool Selectable { get; set; }

		//340	Hard-pointer handle to entity in group(one entry per object)
		public Dictionary<ulong, Entity> Entities { get; set; } = new Dictionary<ulong, Entity>();
	}
}
