using System;

namespace ACadSharp
{
	public class CollectionChangedEventArgs : EventArgs
	{
		/// <summary>
		/// Object that is being added or removed from the collection
		/// </summary>
		public CadObject Item { get; }

		public CollectionChangedEventArgs(CadObject item)
		{
			this.Item = item;
		}
	}
}
