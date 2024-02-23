using ACadSharp.Tables;

namespace ACadSharp.Entities
{
	public interface IText
	{
		/// <summary>
		/// Changes the height of the object.
		/// </summary>
		/// <value>
		/// This must be a positive, non-negative number.
		/// </value>
		double Height { get; set; }

		/// <summary>
		/// Specifies the text string for the entity.
		/// </summary>
		string Value { get; set; }

		/// <summary>
		/// Style of this text entity.
		/// </summary>
		TextStyle Style {  get; set; }
	}
}