using ACadSharp.Attributes;

namespace ACadSharp.Objects;

/// <summary>
/// Represents a <see cref="CadDictionaryWithDefault"/> object.
/// </summary>
/// <remarks>
/// Object name <see cref="DxfFileToken.ObjectDictionaryWithDefault"/> <br/>
/// Dxf class name <see cref="DxfSubclassMarker.DictionaryWithDefault"/>
/// </remarks>
[DxfName(DxfFileToken.ObjectDictionaryWithDefault)]
[DxfSubClass(DxfSubclassMarker.DictionaryWithDefault)]
public class CadDictionaryWithDefault : CadDictionary
{
	/// <summary>
	/// Default entry.
	/// </summary>
	[DxfCodeValue(DxfReferenceType.Handle, 340)]
	public CadObject DefaultEntry
	{
		get
		{
			return _defaultEntry;
		}

		set
		{
			if (value == null)
			{
				this.Document?.AddCadObject(value);
			}

			if (this._defaultEntry != null)
			{
				this.Document?.RemoveCadObject(this._defaultEntry);
			}

			this._defaultEntry = value;
		}
	}

	/// <inheritdoc/>
	public override string ObjectName => DxfFileToken.ObjectDictionaryWithDefault;

	/// <inheritdoc/>
	public override ObjectType ObjectType { get { return ObjectType.UNLISTED; } }

	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.DictionaryWithDefault;

	private CadObject _defaultEntry;

	public CadDictionaryWithDefault() : base()
	{
	}

	public CadDictionaryWithDefault(string name, CadObject defaultEntry) : base(name)
	{
		this.DefaultEntry = defaultEntry;
	}
}