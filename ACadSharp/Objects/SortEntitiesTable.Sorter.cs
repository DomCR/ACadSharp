using ACadSharp.Attributes;
using ACadSharp.Entities;

namespace ACadSharp.Objects
{
	public partial class SortEntitiesTable
	{
		/// <summary>
		/// Entity sorter based in their position in the collection.
		/// </summary>
		public class Sorter
		{
			/// <inheritdoc/>
			[DxfCodeValue(5)]
			public ulong Handle
			{
				get
				{
					if (this._handle.HasValue)
					{
						return this._handle.Value;
					}
					else
					{
						return this.Entity.Handle;
					}
				}
				internal set { this._handle = value; }
			}

			/// <summary>
			/// Soft-pointer ID/handle to an entity
			/// </summary>
			[DxfCodeValue(331)]
			public Entity Entity { get; set; }

			private ulong? _handle;

			/// <summary>
			/// Sorter constructor with the entity and handle.
			/// </summary>
			/// <param name="entity">Enity in the block to be sorted.</param>
			/// <param name="handle">Sorter handle, will use the entity handle if null.</param>
			public Sorter(Entity entity, ulong? handle = null)
			{
				this.Entity = entity;
				this._handle = handle;
			}
		}
	}
}
