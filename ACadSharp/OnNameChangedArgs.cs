using System;

namespace ACadSharp
{
	public class OnNameChangedArgs : EventArgs
	{
		/// <summary>
		/// Old object name
		/// </summary>
		public string OldName { get; }

		/// <summary>
		/// New name to be assign at the object
		/// </summary>
		public string NewName { get; }

		public OnNameChangedArgs(string oldName, string newName)
		{
			this.OldName = oldName;
			this.NewName = newName;
		}
	}
}
