using ACadSharp.Attributes;
using ACadSharp.Entities;
using ACadSharp.Objects.Collections;
using CSUtilities.Extensions;
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
	public class Group : NonGraphicalObject
	{
		/// <summary>
		/// Group description.
		/// </summary>
		[DxfCodeValue(300)]
		public string Description { get; set; } = string.Empty;

		/// <summary>
		/// Entities in this group.
		/// </summary>
		/// <remarks>
		/// Hard-pointer handle to entity in group.
		/// </remarks>
		[DxfCollectionCodeValue(DxfReferenceType.Handle, 340)]
		public IEnumerable<Entity> Entities { get { return this._entities; } }

		/// <summary>
		/// If the group has an automatic generated name.
		/// </summary>
		/// <remarks>
		/// The name for an unnamed group will be managed by the <see cref="GroupCollection"/>.
		/// </remarks>
		[DxfCodeValue(70)]
		public bool IsUnnamed { get { return this.Name.IsNullOrWhiteSpace() || this.Name.StartsWith("*"); } }

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.TableGroup;

		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.GROUP;

		/// <summary>
		/// If the group is selectable.
		/// </summary>
		[DxfCodeValue(71)]
		public bool Selectable { get; set; } = true;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.Group;

		private List<Entity> _entities = new();

		/// <inheritdoc/>
		public Group() : base()
		{
		}

		/// <inheritdoc/>
		public Group(string name) : base(name)
		{
		}

		/// <summary>
		/// Add an entity to the group.
		/// </summary>
		/// <param name="entity"></param>
		public void Add(Entity entity)
		{
			if (this.Document != entity.Document)
			{
				throw new System.InvalidOperationException("The Group and the entity must belong to the same document.");
			}

			this._entities.Add(entity);
			entity.AddReactor(this);
		}

		/// <summary>
		/// Add multiple entities into the group.
		/// </summary>
		/// <param name="entities"></param>
		public void AddRange(IEnumerable<Entity> entities)
		{
			foreach (var e in entities)
			{
				this.Add(e);
			}
		}

		/// <summary>
		/// Removes all entities in the group.
		/// </summary>
		public void Clear()
		{
			foreach (var e in this._entities)
			{
				e.RemoveReactor(this);
			}

			this._entities.Clear();
		}

		/// <summary>
		/// Removes the entity from the group.
		/// </summary>
		/// <param name="entity"></param>
		public bool Remove(Entity entity)
		{
			entity.RemoveReactor(this);
			return this._entities.Remove(entity);
		}

		/// <inheritdoc/>
		public override CadObject Clone()
		{
			Group clone = (Group)base.Clone();

			clone._entities = new();

			return clone;
		}

		internal override void AssignDocument(CadDocument doc)
		{
			base.AssignDocument(doc);
		}

		internal override void UnassignDocument()
		{
			base.UnassignDocument();

			foreach (var e in this._entities)
			{
				e.RemoveReactor(this);
			}

			this._entities.Clear();
		}
	}
}