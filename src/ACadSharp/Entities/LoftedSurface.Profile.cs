namespace ACadSharp.Entities;

public partial class LoftedSurface
{
	/// <summary>
	/// Entity data used as a cross section, guide or path of the loft.
	/// </summary>
	public class Profile
	{
		/// <summary>
		/// Binary data of the entity.
		/// </summary>
		public byte[] BinaryData { get; set; }

		/// <summary>
		/// Class ID of the entity.
		/// </summary>
		public int ClassId { get; set; }

		/// <summary>
		/// Creates a copy of the profile.
		/// </summary>
		/// <returns>A new profile containing the same BinaryData and ClassId values.</returns>
		public Profile Clone()
		{
			return new Profile
			{
				BinaryData = this.BinaryData,
				ClassId = this.ClassId
			};
		}
	}
}
