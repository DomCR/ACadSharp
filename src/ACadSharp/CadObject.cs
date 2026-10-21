using ACadSharp.Attributes;
using ACadSharp.Extensions;
using ACadSharp.IO;
using ACadSharp.Objects;
using ACadSharp.Objects.Collections;
using ACadSharp.Tables;
using ACadSharp.Tables.Collections;
using ACadSharp.XData;
using CSMath.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ACadSharp;

/// <summary>
/// Represents an element in a CadDocument.
/// </summary>
public abstract class CadObject : IHandledCadObject
{
	/// <summary>
	/// Document where this element belongs.
	/// </summary>
	public CadDocument Document { get; private set; }

	/// <summary>
	/// Extended data attached to this object.
	/// </summary>
	public ExtendedDataDictionary ExtendedData { get; private set; }

	/// <inheritdoc/>
	/// <remarks>
	/// If the value is 0 the object is not assigned to a document or a parent.
	/// </remarks>
	[DxfCodeValue(5)]
	public ulong Handle { get; internal set; }

	/// <summary>
	/// Flag that indicates if this object has a dynamic dxf sublcass.
	/// </summary>
	public virtual bool HasDynamicSubclass { get { return false; } }

	/// <summary>
	/// The CAD class name of an object.
	/// </summary>
	public virtual string ObjectName { get; }

	/// <summary>
	/// Get the object type.
	/// </summary>
	public abstract ObjectType ObjectType { get; }

	/// <summary>
	/// Soft-pointer ID/handle to owner object.
	/// </summary>
	[DxfCodeValue(DxfReferenceType.Handle, 330)]
	public IHandledCadObject Owner { get; internal set; }

	/// <summary>
	/// Objects that are attached to this object.
	/// </summary>
	public IEnumerable<CadObject> Reactors
	{
		get
		{
			return this._reactors;
		}
	}

	/// <summary>
	/// Object Subclass marker.
	/// </summary>
	public abstract string SubclassMarker { get; }

	/// <summary>
	/// Extended Dictionary object.
	/// </summary>
	/// <remarks>
	/// An extended dictionary can be created using <see cref="CreateExtendedDictionary"/>.
	/// </remarks>
	public CadDictionary XDictionary
	{
		get { return this._xdictionary; }
		// [PATCH] internal -> public (follows the property): the downstream consumer's cross-document cloning
		// (DWG direct-cut clone trimming) needs to strip the extended dictionary after cloning
		// (the cloned dictionary has handle 0 and streaming entities are not registered into the document ->
		// when writeCommonNonEntityData enqueues the write, multiple handle 0s collide in the handle table Map.Add and crash).
		// Upstream setter is internal.
		set
		{
			// [PATCH] Upstream silently ignores null (impossible to clear). The downstream consumer OFF-route
			// parity needs it: netDxf has no XDictionary concept, and an empty
			// `102 {ACAD_XDICTIONARY` marker read from external DXF must be dropped.
			this._xdictionary = value;

			if (value != null)
			{
				value.Owner = this;

				if (this.Document != null)
					this.Document.RegisterCollection(value);
			}
		}
	}

	private List<CadObject> _reactors = new List<CadObject>();

	private CadDictionary _xdictionary = null;

	/// <summary>
	/// Default constructor.
	/// </summary>
	public CadObject()
	{
		this.ExtendedData = new ExtendedDataDictionary(this);
	}

	/// <summary>
	/// Add a reactor object linked to this one.
	/// </summary>
	/// <remarks>
	/// The <see cref="CadObject"/> and its reactors must be in the same <see cref="CadDocument"/> to be valid.
	/// </remarks>
	/// <param name="reactor"></param>
	public void AddReactor(CadObject reactor)
	{
		this._reactors.Add(reactor);
	}

	/// <summary>
	/// Removes any reactor object that doesn't belong to the same <see cref="CadDocument"/> as this <see cref="CadObject"/>.
	/// </summary>
	public void CleanReactors()
	{
		var reactors = this._reactors.ToList();
		foreach (var reactor in reactors)
		{
			if (reactor.Document != this.Document)
			{
				this._reactors.Remove(reactor);
			}
		}
	}

	/// <summary>
	/// Creates a new object that is a copy of the current instance.
	/// </summary>
	/// <remarks>
	/// The copy will be unattached from the document or any reference.
	/// </remarks>
	/// <returns>A new object that is a copy of this instance.</returns>
	public virtual CadObject Clone()
	{
		CadObject clone = (CadObject)this.MemberwiseClone();

		clone.Handle = 0;

		clone.Document = null;
		clone.Owner = null;

		//Collections
		clone._reactors = new List<CadObject>();
		// [PATCH] Upstream replaces ExtendedData with an EMPTY dictionary, silently dropping all
		// XData on clone (MemberwiseClone alone would share the original's internal dictionary,
		// hence the replacement). The downstream consumer's cross-document clones (external DXF blocks, e.g. the
		// frame logo with gradient-hatch XData) rely on the clone keeping its XData; the target
		// document re-keys the AppId entries on registration. Copy the entries (value objects are
		// read-only during writing, so sharing them is safe).
		clone.ExtendedData = new ExtendedDataDictionary(clone);
		foreach (var kv in this.ExtendedData)
			clone.ExtendedData.Add(kv.Key, kv.Value);
		clone.XDictionary = this._xdictionary?.CloneTyped();

		return clone;
	}

	/// <summary>
	/// Creates the extended dictionary if null.
	/// </summary>
	/// <returns>The <see cref="CadDictionary"/> attached to this <see cref="CadObject"/></returns>
	public CadDictionary CreateExtendedDictionary()
	{
		if (this._xdictionary == null)
		{
			this.XDictionary = new CadDictionary();
		}

		return this._xdictionary;
	}

	/// <summary>
	/// Determines whether the object is valid and can be written in a CAD file.
	/// </summary>
	/// <param name="format">The CAD file format.</param>
	/// <param name="version">The CAD version.</param>
	/// <returns>True if the object is valid; otherwise, false.</returns>
	public bool IsValid(CadFileFormat format, ACadVersion version)
	{
		return this.IsValid(format, version, out _);
	}

	/// <summary>
	/// Determines whether the object is valid and can be written in a CAD file.
	/// </summary>
	/// <param name="format">The CAD file format.</param>
	/// <param name="version">The CAD version.</param>
	/// <param name="errors">A list of errors found during the validation.</param>
	/// <returns>True if the object is valid; otherwise, false.</returns>
	public virtual bool IsValid(CadFileFormat format, ACadVersion version, out IList<string> errors)
	{
		bool result = true;
		errors = new List<string>();

		if (this is IOrientable orientable)
		{
			if (orientable.Normal.IsZero())
			{
				errors.Add($"{nameof(orientable.Normal)} vector cannot be zero.");
				result = false;
			}
		}

		return result;
	}

	/// <summary>
	/// Remove a reactor linked to this object.
	/// </summary>
	/// <param name="reactor"></param>
	/// <returns></returns>
	public bool RemoveReactor(CadObject reactor)
	{
		return this._reactors.Remove(reactor);
	}

	/// <inheritdoc/>
	public override string ToString()
	{
		return $"{this.ObjectName}:{this.Handle}";
	}

	internal virtual void AssignDocument(CadDocument doc)
	{
		this.Document = doc;

		if (this.XDictionary != null)
		{
			doc.RegisterCollection(this.XDictionary);
		}

		if (this.ExtendedData.Any())
		{
			//Reset existing collection
			var entries = this.ExtendedData.ToArray();
			this.ExtendedData.Clear();

			foreach (var item in entries)
			{
				this.ExtendedData.Add(item.Key, item.Value);
			}
		}
	}

	internal virtual void UnassignDocument()
	{
		if (this.XDictionary != null)
		{
			this.Document.UnregisterCollection(this.XDictionary);
		}

		this.Handle = 0;
		this.Document = null;

		if (this.ExtendedData.Any())
		{
			//Reset existing collection
			var entries = this.ExtendedData.ToArray();
			this.ExtendedData.Clear();

			foreach (var item in entries)
			{
				this.ExtendedData.Add(item.Key.Clone() as AppId, item.Value);
			}
		}

		this._reactors.Clear();
	}

	protected T updateCollectionEntry<T>(T entry, Action<T> assignValue, ObjectDictionaryCollection<T> collection)
		where T : NonGraphicalObject
	{
		if (collection == null || entry == null)
		{
			return entry;
		}

		return collection.UpdateReference(this, entry, assignValue);
	}

	protected T updateTableEntry<T>(T entry, Action<T> assignValue, Table<T> table)
		where T : TableEntry
	{
		if (table == null || entry == null)
		{
			return entry;
		}

		return table.UpdateReference(this, entry, assignValue);
	}
}