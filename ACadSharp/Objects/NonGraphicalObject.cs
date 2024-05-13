using ACadSharp.Attributes;
using System;

namespace ACadSharp.Objects
{
	/// <summary>
	/// The standard class for a basic CAD non-graphical object.
	/// </summary>
	[DxfSubClass(null, true)]
	public abstract class NonGraphicalObject : CadObject, INamedCadObject
	{
		public event EventHandler<OnNameChangedArgs> OnNameChanged;

		/// <inheritdoc/>
		public virtual string Name
		{
			get { return this._name; }
			set
			{
				OnNameChanged?.Invoke(this, new OnNameChangedArgs(this._name, value));
				this._name = value;
			}
		}

		private string _name = string.Empty;

		/// <inheritdoc/>
		public override string ToString()
		{
			if (string.IsNullOrEmpty(this.Name))
			{
				return $"{this.ObjectName}:{this.Handle}";
			}
			else
			{
				return $"{this.ObjectName}:{this.Name}:{this.Handle}";
			}
		}
	}
}
