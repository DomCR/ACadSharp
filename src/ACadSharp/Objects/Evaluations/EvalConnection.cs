namespace ACadSharp.Objects.Evaluations;

public class EvalConnection
{
	public int Id { get; set; }

	public string Name { get; set; }

	public const string DisplacementX = "DisplacementX";

	public const string DisplacementY = "DisplacementY";

	public const string Scale = "Scale";

	public const string XScale = "XScale";

	public const string YScale = "YScale";

	/// <inheritdoc/>
	public override string ToString()
	{
		return $"{this.Id}:{this.Name}";
	}
}