using ACadSharp.Attributes;

namespace ACadSharp.Entities
{
	//Is it present in the DXF ??
	public enum AttributeType   //TODO: Check how AttributeType is assigned in Autocad
	{
		SingleLine = 1,
		MultiLine = 2,
		ConstantMultiLine = 4,
	}

	/// <summary>
	/// Common base class for <see cref="AttributeEntity" /> and <see cref="AttributeDefinition" />.
	/// </summary>
	[DxfSubClass(null, true)]
	public abstract class AttributeBase : TextEntity
	{
		[DxfCodeValue(74)]
		public override TextVerticalAlignmentType VerticalAlignment { get; set; } = TextVerticalAlignmentType.Baseline;

		[DxfCodeValue(280)]
		public byte Version { get; set; }

		/// <summary>
		/// Specifies the tag string of the object
		/// </summary>
		/// <value>
		/// Cannot contain spaces (not applied)
		/// </value> 
		[DxfCodeValue(2)]
		public string Tag
		{
			get { return this._tag; }
			set
			{
				this._tag = value;
				return;

				//TODO: explore AttributeBase tag constrain
				if (value == null)
					throw new System.ArgumentNullException(nameof(value));

				if (value.Contains(" "))
					throw new System.ArgumentException($"Attribute Tag {value} cannot contain spaces", nameof(value));

				this._tag = value;
			}
		}

		/// <summary>
		/// Attribute flags
		/// </summary>
		[DxfCodeValue(70)]
		public AttributeFlags Flags { get; set; }

		/// <summary>
		/// MText flag
		/// </summary>
		//[DxfCodeValue(280)]
		//TODO: Check the dxf code of Attribute type.
		//Missmatch between Autodesk documentation and OpenDesign
		public AttributeType AttributeType { get; set; } = AttributeType.SingleLine;

		//Missmatch between Autodesk documentation and OpenDesign
		public bool IsReallyLocked { get; set; }

		private string _tag = string.Empty;

		public AttributeBase() : base() { }

		protected void matchAttributeProperties(AttributeBase src)
		{
			src.MatchProperties(this);

			this.Thickness = src.Thickness;
			this.InsertPoint = src.InsertPoint;
			this.Height = src.Height;
			this.Value = src.Value;
			this.Rotation = src.Rotation;
			this.WidthFactor = src.WidthFactor;
			this.ObliqueAngle = src.ObliqueAngle;
			this.Style = (Tables.TextStyle)src.Style.Clone();
			this.Mirror = src.Mirror;
			this.HorizontalAlignment = src.HorizontalAlignment;
			this.AlignmentPoint = src.AlignmentPoint;
			this.Normal = src.Normal;
			this.VerticalAlignment = src.VerticalAlignment;

			this.Version = src.Version;
			this.Tag = src.Tag;
			this.Flags = src.Flags;
			this.AttributeType = src.AttributeType;
			this.IsReallyLocked = src.IsReallyLocked;
		}
	}
}
