using ACadSharp.Attributes;
using ACadSharp.Classes;
using ACadSharp.Extensions;

namespace ACadSharp.Objects;

/// <summary>
/// Represents a <see cref="TableContent"/> object.
/// </summary>
/// <remarks>
/// Object name <see cref="DxfFileToken.ObjectTableContent"/> <br/>
/// Dxf class name <see cref="DxfSubclassMarker.TableContent"/>
/// </remarks>
[DxfName(DxfFileToken.ObjectTableContent)]
[DxfSubClass(DxfSubclassMarker.TableContent)]
public class TableContent : FormattedTableData, IDxfClassDefined
{
	/// <inheritdoc/>
	public override string ObjectName => DxfFileToken.ObjectTableContent;

	/// <inheritdoc/>
	public override ObjectType ObjectType { get { return ObjectType.UNLISTED; } }

	/// <summary>
	/// Gets or sets the table style applied to the table.
	/// </summary>
	[DxfCodeValue(DxfReferenceType.Handle, 340)]
	public TableStyle Style
	{
		get
		{
			return _style;
		}

		set
		{
			if (value == null)
			{
				throw new System.ArgumentNullException(nameof(value), "Table style cannot be null");
			}

			if (this.Document != null)
			{
				this._style = this.updateCollectionEntry(value, s => this._style = s, this.Document.TableStyles);
			}
			else
			{
				this._style = value;
			}
		}
	}

	/// <summary>
	/// Gets or sets the table style that overrides the default style.
	/// </summary>
	public TableStyle StyleOverride { get; set; }

	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.TableContent;

	private TableStyle _style = TableStyle.Default;

	/// <inheritdoc/>
	public override CadObject Clone()
	{
		TableContent clone = (TableContent)base.Clone();

		clone.Style = this.Style?.CloneTyped();
		clone.StyleOverride = this.StyleOverride?.CloneTyped();

		return clone;
	}

	/// <inheritdoc/>
	public DxfClass GetDxfClass()
	{
		return new DxfClass
		{
			CppClassName = DxfSubclassMarker.TableContent,
			DwgVersion = ACadVersion.AC1018,
			DxfName = DxfFileToken.ObjectTableContent,
			ItemClassId = 499,
			MaintenanceVersion = 21,
			ProxyFlags = ProxyFlags.CloningAllowed | ProxyFlags.DisablesProxyWarningDialog,
			WasZombie = false,
		};
	}

	internal override void AssignDocument(CadDocument doc)
	{
		base.AssignDocument(doc);

		this._style = this.updateCollectionEntry(this.Style, s => this._style = s, doc.TableStyles);
	}

	internal override void UnassignDocument()
	{
		base.UnassignDocument();

		this._style = this.Style.CloneTyped();
	}
}