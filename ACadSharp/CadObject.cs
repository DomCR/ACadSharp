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
		public event EventHandler<ReferenceChangedEventArgs> OnReferenceChange;

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

		/// <inheritdoc/>
		public override string ToString()
		{
			return $"{this.ObjectName}:{this.ObjectType}";
		}

		protected void onReferenceChange(ReferenceChangedEventArgs args)
		{
			OnReferenceChange?.Invoke(this, args);
		}

		protected virtual void createCopy(CadObject copy)
		{
			//TODO: copy ExtendedData, Reactors, XDictionary needed ??
		}
	}
}
