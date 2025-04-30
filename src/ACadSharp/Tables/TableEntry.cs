using ACadSharp.Attributes;
using System;

namespace ACadSharp.Tables
{
	[DxfSubClass(DxfSubclassMarker.TableRecord, true)]
	public abstract class TableEntry : CadObject, INamedCadObject
	{
		/// <summary>
		/// Event occurs when the <see cref="TableEntry.Name"/> changes.
		/// </summary>
		public event EventHandler<OnNameChangedArgs> OnNameChanged;

		/// <summary>
		/// Standard flags
		/// </summary>
		[DxfCodeValue(70)]
		public StandardFlags Flags { get; set; }

		/// <summary>
		/// Specifies the name of the object
		/// </summary>
		[DxfCodeValue(2)]
		public virtual string Name
		{
			get { return this.name; }
			set
			{
				if (string.IsNullOrEmpty(value))
				{
					throw new ArgumentNullException(nameof(value), $"Table entry [{this.GetType().FullName}] must have a name");
				}

				OnNameChanged?.Invoke(this, new OnNameChangedArgs(this.name, value));
				this.name = value;
			}
		}

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.TableRecord;

		protected string name = string.Empty;

		/// <summary>
		/// Default constructor.
		/// </summary>
		/// <param name="name">Name for the entry, must be unique is added to a <see cref="CadDocument"/>.</param>
		/// <exception cref="ArgumentNullException"></exception>
		public TableEntry(string name)
		{
			if (string.IsNullOrEmpty(name))
				throw new ArgumentNullException(nameof(name), $"{this.GetType().Name} must have a name.");

			this.Name = name;
		}

		internal TableEntry()
		{ }

		/// <inheritdoc/>
		public override CadObject Clone()
		{
			TableEntry clone = (TableEntry)base.Clone();
			clone.OnNameChanged = null;
			return clone;
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return $"{this.ObjectName}:{this.Name}";
		}
	}
}