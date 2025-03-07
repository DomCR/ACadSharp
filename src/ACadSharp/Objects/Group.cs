using ACadSharp.Attributes;
using ACadSharp.Entities;
using System.Collections.Generic;
using System.Linq;

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
	public class Group : NonGraphicalObject
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.GROUP;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.TableGroup;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.Group;

		/// <summary>
		/// Group description.
		/// </summary>
		[DxfCodeValue(300)]
		public string Description { get; set; }

		/// <summary>
		/// If the group has an automatic generated name.
		/// </summary>
		[DxfCodeValue(71)]
		public bool IsUnnamed { get; set; }

		/// <summary>
		/// If the group is selectable.
		/// </summary>
		[DxfCodeValue(71)]
		public bool Selectable { get; set; }

		//340	Hard-pointer handle to entity in group(one entry per object)

		/// <summary>
		/// Entities in this group.
		/// </summary>
		public IEnumerable<Entity> Entities { get { return this._entities; } }

		private readonly List<Entity> _entities = new List<Entity>();

		public void Add(Entity entity)
		{
			//if (this.Document is null && entity.Document is null
			//	|| this.Document == entity.Document)
			{
				this._entities.Add(entity);
				entity.Reactors.Add(this.Handle, this);
			}
		}

		internal override void AssignDocument(CadDocument doc)
		{
			base.AssignDocument(doc);
		}

		internal override void UnassignDocument()
		{
			base.UnassignDocument();
		}
	}
}
