using ACadSharp.Attributes;
using ACadSharp.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ACadSharp
{
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

		//TODO: CadDictionary for the CadObjects
		public CadDictionary XDictionary
		{
			get { return this._xdictionary; }
			set
			{
				if (value == null)
					return;

				this._xdictionary = value;
				this._xdictionary.Owner = this;
			}
		}

		/// <summary>
		/// Objects that are attached to this entity
		/// </summary>
		public Dictionary<ulong, CadObject> Reactors { get; } = new Dictionary<ulong, CadObject>();

		//TODO: Extended data
		public ExtendedDataDictionary ExtendedData { get; } = new ExtendedDataDictionary();

		/// <summary>
		/// Document where this element belongs
		/// </summary>
		public virtual CadDocument Document
		{
			get { return this._document; }
			internal set
			{
				this._document = value;
			}
		}

		private CadDocument _document = null;

		private CadDictionary _xdictionary = null;

		/// <summary>
		/// Default constructor
		/// </summary>
		public CadObject() { }

		protected void onReferenceChange(ReferenceChangedEventArgs args)
		{
			OnReferenceChange?.Invoke(this, args);
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return $"{this.ObjectName}:{this.ObjectType}";
		}
	}
}
