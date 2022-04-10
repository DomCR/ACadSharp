using System;

namespace ACadSharp
{
	public class ReferenceChangedEventArgs : EventArgs
	{
		/// <summary>
		/// Object that is being added or removed from the collection
		/// </summary>
		public CadObject Item { get; }

		public ReferenceChangedEventArgs(CadObject item)
		{
			this.Item = item;
		}
	}
}
