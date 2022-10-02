using System;

namespace ACadSharp
{
	public class CollectionChangedEventArgs : EventArgs
	{
		/// <summary>
		/// Item that is beeing added or removed from the collection
		/// </summary>
		public CadObject Item { get; }

		public CollectionChangedEventArgs(CadObject curr)
		{
			this.Item = curr;
		}
	}
}
