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
		/// <remarks>
		/// The name of a <see cref="NonGraphicalObject"/> will be used as the name of the entry when the owner is a <see cref="CadDictionary"/>
		/// otherwise the name may not be saved if there is no dxf code assigned to the <see cref="CadObject"/>.
		/// </remarks>
		public virtual string Name
		{
			get { return this._name; }
			set
			{
				OnNameChanged?.Invoke(this, new OnNameChangedArgs(this._name, value));
				this._name = value;
			}
		}

		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.UNLISTED;

		private string _name = string.Empty;

		/// <summary>
		/// Default constructor.
		/// </summary>
		public NonGraphicalObject()
		{ }

		/// <summary>
		/// Initialize a <see cref="NonGraphicalObject"/> with an specific name.
		/// </summary>
		/// <param name="name"></param>
		public NonGraphicalObject(string name)
		{
			this.Name = name;
		}

		/// <inheritdoc/>
		public override CadObject Clone()
		{
			NonGraphicalObject clone = (NonGraphicalObject)base.Clone();
			clone.OnNameChanged = null;
			return clone;
		}

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