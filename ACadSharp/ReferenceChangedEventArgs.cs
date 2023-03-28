using System;

namespace ACadSharp
{
	public class ReferenceChangedEventArgs : EventArgs
	{
		/// <summary>
		/// Current value
		/// </summary>
		public CadObject Current { get; }

		/// <summary>
		/// Old value for the object
		/// </summary>
		public CadObject Old { get; }

		/// <summary>
		/// Flag to remove the old object in the document
		/// </summary>
		public bool RemoveOld { get; } = false;

		public ReferenceChangedEventArgs(CadObject curr, CadObject old)
		{
			this.Current = curr;
			this.Old = old;
		}
	}
}
