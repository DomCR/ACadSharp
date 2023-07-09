using ACadSharp.Attributes;
using System.Collections.Generic;

namespace ACadSharp.Objects
{
	/// <summary>
	/// Represents a <see cref="Group"/> object.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.TableGroup"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.Group"/>
	/// </remarks>
	[DxfName(DxfFileToken.TableGroup)]
	[DxfSubClass(DxfSubclassMarker.Group)]
	public class Group : CadObject
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.GROUP;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.TableGroup;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.Group;

		/// <summary>
		/// Group description
		/// </summary>
		[DxfCodeValue(300)]
		public string Description { get; set; }

		//70	“Unnamed” flag: 1 = Unnamed; 0 = Named

		[DxfCodeValue(71)]
		public bool Selectable { get; set; }

		//340	Hard-pointer handle to entity in group(one entry per object)
		public Dictionary<ulong, CadObject> Members { get; set; } = new Dictionary<ulong, CadObject>();
	}
}
