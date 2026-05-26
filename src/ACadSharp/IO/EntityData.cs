namespace ACadSharp.IO;

/// <summary>
/// Represents the common graphical properties associated with a CAD entity.
/// </summary>
public readonly struct EntityData
{
	/// <summary>
	/// Gets the reference data for the book color of the entity.
	/// </summary>
	public ReferenceData BookColor { get; }

	/// <summary>
	/// Gets the color of the entity.
	/// </summary>
	public Color Color { get; }

	/// <summary>
	/// Gets a value indicating whether the entity is invisible.
	/// </summary>
	public bool IsInvisible { get; }

	/// <summary>
	/// Gets the reference data for the layer the entity belongs to.
	/// </summary>
	public ReferenceData Layer { get; }

	/// <summary>
	/// Gets the reference data for the line type of the entity.
	/// </summary>
	public ReferenceData LineType { get; }

	/// <summary>
	/// Gets the line type scale applied to the entity.
	/// </summary>
	public double LineTypeScale { get; }

	/// <summary>
	/// Gets the line weight of the entity.
	/// </summary>
	public LineWeightType LineWeight { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="EntityData"/> struct.
	/// </summary>
	/// <param name="bookColor">The reference data for the book color.</param>
	/// <param name="color">The color of the entity.</param>
	/// <param name="isInvisible">A value indicating whether the entity is invisible.</param>
	/// <param name="layer">The reference data for the layer.</param>
	/// <param name="lineType">The reference data for the line type.</param>
	/// <param name="lineTypeScale">The line type scale.</param>
	/// <param name="lineWeight">The line weight.</param>
	public EntityData(
		ReferenceData bookColor,
		Color color,
		bool isInvisible,
		ReferenceData layer,
		ReferenceData lineType,
		double lineTypeScale,
		LineWeightType lineWeight)
	{
		this.BookColor = bookColor;
		this.Color = color;
		this.IsInvisible = isInvisible;
		this.Layer = layer;
		this.LineType = lineType;
		this.LineTypeScale = lineTypeScale;
		this.LineWeight = lineWeight;
	}
}
