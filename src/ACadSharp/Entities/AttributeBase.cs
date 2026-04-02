using ACadSharp.Attributes;
using ACadSharp.Extensions;

namespace ACadSharp.Entities;

/// <summary>
/// Common base class for <see cref="AttributeEntity" /> and <see cref="AttributeDefinition" />.
/// </summary>
[DxfSubClass(null, true)]
public abstract class AttributeBase : TextEntity
{
	/// <summary>
	/// Attribute type.
	/// </summary>
	[DxfCodeValue(71)]
	public AttributeType AttributeType { get; set; } = AttributeType.SingleLine;

	/// <summary>
	/// Attribute flags.
	/// </summary>
	[DxfCodeValue(70)]
	public AttributeFlags Flags { get; set; }

	public bool IsReallyLocked { get; set; }

	public MText MText { get; set; }

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

	[DxfCodeValue(280)]
	public byte Version { get; set; }

	[DxfCodeValue(74)]
	public override TextVerticalAlignmentType VerticalAlignment { get; set; } = TextVerticalAlignmentType.Baseline;

	private string _tag = string.Empty;

	public AttributeBase() : base()
	{
	}

	protected void matchAttributeProperties(AttributeBase src)
	{
		this.MatchProperties(src);

		this.Thickness = src.Thickness;
		this.InsertPoint = src.InsertPoint;
		this.Height = src.Height;
		this.Value = src.Value;
		this.Rotation = src.Rotation;
		this.WidthFactor = src.WidthFactor;
		this.ObliqueAngle = src.ObliqueAngle;

		if (this.Style.Document != src.Style.Document)
		{
			this.Style = src.Style.CloneTyped();
		}
		else
		{
			this.Style = src.Style;
		}

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

		this.InsertPoint = src.InsertPoint;
	}
}