using System;

using ACadSharp.Attributes;

namespace ACadSharp.Objects
{
	[DxfSubClass(DxfSubclassMarker.AnnotScaleObjectContextData)]
	public abstract class AnnotScaleObjectContextData : ObjectContextData
	{
		private Scale _scale = Scale.Default;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.AnnotScaleObjectContextData;

		//H	340	Handle to scale(AcDbScale) object (hard pointer).
		[DxfCodeValue(DxfReferenceType.Handle, 340)]
		public Scale Scale
		{
			get { return _scale; } 
			set
			{
				if (value == null) {
					throw new ArgumentNullException(nameof(value));
				}

				if (this.Document != null)
				{
					this._scale = CadObject.updateCollection(value, this.Document.Scales);
				}
				else
				{
					this._scale = value;
				}
			}
		}

		public override CadObject Clone()
		{
			AnnotScaleObjectContextData clone =  (AnnotScaleObjectContextData)base.Clone();

			clone._scale = (Scale)this._scale?.Clone();

			return clone;
		}

		internal override void AssignDocument(CadDocument doc)
		{
			base.AssignDocument(doc);

			this._scale = CadObject.updateCollection(this._scale, this.Document?.Scales);

			this.Document.Scales.OnRemove += tableOnRemove;
		}

		internal override void UnassignDocument()
		{
			this.Document.Scales.OnRemove -= tableOnRemove;

			base.UnassignDocument();

			this._scale = (Scale)this._scale.Clone();
		}

		private void tableOnRemove(object sender, CollectionChangedEventArgs e)
		{
			if (e.Item.Equals(this._scale))
			{
				this._scale = this.Document.Scales[Scale.Default.Name];
			}
		}
	}
}
