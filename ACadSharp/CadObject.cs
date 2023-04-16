using ACadSharp.Attributes;
using ACadSharp.Objects;
using System;
using System.Collections.Generic;

namespace ACadSharp
{
	/// <summary>
	/// Represents an element in a CadDocument
	/// </summary>
	public abstract class CadObject : IHandledCadObject
	{
		public event EventHandler<ReferenceChangedEventArgs> OnReferenceChanged;

		/// <summary>
		/// Get the object type
		/// </summary>
		public abstract ObjectType ObjectType { get; }

		/// <summary>
		/// The AutoCAD class name of an object
		/// </summary>
		public virtual string ObjectName { get; } = DxfFileToken.Undefined;

		/// <inheritdoc/>
		/// <remarks>
		/// If the value is 0 the object is not assigned to a document or a parent
		/// </remarks>
		[DxfCodeValue(5)]
		public ulong Handle { get; internal set; }

		/// <summary>
		/// Soft-pointer ID/handle to owner object
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Handle, 330)]
		public IHandledCadObject Owner { get; internal set; }

		/// <summary>
		/// Object dictionary
		/// </summary>
		public CadDictionary XDictionary
		{
			get { return this._xdictionary; }
			internal set
			{
				if (value == null)
					return;

				this._xdictionary = value;
				this._xdictionary.Owner = this;

				if (this.Document != null)
					this.Document.RegisterCollection(this._xdictionary);
			}
		}

		/// <summary>
		/// Objects that are attached to this object
		/// </summary>
		public Dictionary<ulong, CadObject> Reactors { get; } = new Dictionary<ulong, CadObject>();

		/// <summary>
		/// Extended data attached to this object
		/// </summary>
		public ExtendedDataDictionary ExtendedData { get; } = new ExtendedDataDictionary();

		/// <summary>
		/// Document where this element belongs
		/// </summary>
		public CadDocument Document { get; internal set; }

		private CadDictionary _xdictionary = null;

		/// <summary>
		/// Default constructor
		/// </summary>
		public CadObject() { }

		/// <summary>
		/// Creates a new object that is a copy of the current instance.
		/// </summary>
		/// <remarks>
		/// The copy will be unatached from the document or any reference
		/// </remarks>
		/// <returns>A new object that is a copy of this instance.</returns>
		public virtual CadObject Clone()
		{
			CadObject clone = (CadObject)this.MemberwiseClone();

			clone.Handle = 0;
			clone.Document = null;
			clone.Owner = null;

			//Collections
			clone.Reactors.Clear();
			clone.XDictionary = null;
			clone.ExtendedData.Clear();

			return clone;
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return $"{this.ObjectName}:{this.ObjectType}";
		}

		protected void onReferenceChange(ReferenceChangedEventArgs args)
		{
			OnReferenceChanged?.Invoke(this, args);
		}

		[Obsolete("Use memberwiseclone")]
		protected virtual void mapClone(CadObject clone)
		{
			//TODO: copy ExtendedData, Reactors, XDictionary needed ??
		}
	}
}
