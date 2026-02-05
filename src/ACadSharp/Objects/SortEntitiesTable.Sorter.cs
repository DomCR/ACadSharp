using ACadSharp.Attributes;
using ACadSharp.Entities;

namespace ACadSharp.Objects
{
	public partial class SortEntitiesTable
	{
		/// <summary>
		/// Entity sorter based in their position in the collection.
		/// </summary>
		public class Sorter : System.IComparable<Sorter>
		{
			/// <summary>
			/// Sorter handle, if this doesn't point to an entity, the value will match with <see cref="Entity"/>'s handle.
			/// </summary>
			[DxfCodeValue(5)]
			public ulong SortHandle { get; set; }

			/// <summary>
			/// Entity to set the order to.
			/// </summary>
			[DxfCodeValue(331)]
			public Entity Entity { get; set; }

			/// <summary>
			/// Sorter constructor with the entity and handle.
			/// </summary>
			/// <param name="entity">Entity in the block to be sorted.</param>
			/// <param name="handle">Sorter handle.</param>
			public Sorter(Entity entity, ulong handle)
			{
				this.Entity = entity;
				this.SortHandle = handle;
			}

			/// <inheritdoc/>
			public override string ToString()
			{
				return $"{this.SortHandle} | {this.Entity?.ToString()}";
			}

			/// <inheritdoc/>
			public int CompareTo(Sorter other)
			{
				if (this.SortHandle < other.SortHandle)
				{
					return -1;
				}
				else if (this.SortHandle > other.SortHandle)
				{
					return 1;
				}
				else
				{
					return 0;
				}
			}
		}
	}
}
