using System;

using ACadSharp.Attributes;

namespace ACadSharp.Objects;

[DxfSubClass(null, true)]
public abstract class AnnotScaleObjectContextData : ObjectContextData
{
	//TODO: solve conflict with MultiLeaderObjectContextData
	//[DxfCodeValue(DxfReferenceType.Handle, 340)]
	public Scale Scale
	{
		get { return _scale; }
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException(nameof(value));
			}

			this._scale = this.updateCollectionEntry(value, s => this._scale = s, this.Document?.Scales);
		}
	}

	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.AnnotScaleObjectContextData;

	private Scale _scale = Scale.Default;

	/// <inheritdoc/>
	public override CadObject Clone()
	{
		AnnotScaleObjectContextData clone = (AnnotScaleObjectContextData)base.Clone();

		clone._scale = (Scale)this._scale?.Clone();

		return clone;
	}

	internal override void AssignDocument(CadDocument doc)
	{
		base.AssignDocument(doc);

		this._scale = this.updateCollectionEntry(this._scale, s => this._scale = s, this.Document.Scales);
	}

	internal override void UnassignDocument()
	{
		this.Document.Scales.RemoveReference(this.Scale?.Name, this);

		base.UnassignDocument();

		this._scale = (Scale)this._scale.Clone();
	}
}
