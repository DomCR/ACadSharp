#nullable enable
namespace ACadSharp.Entities
{
	public partial class MText
	{
		/// <summary>
		/// Base token containing a format.
		/// </summary>
		public abstract class Token
		{
			/// <summary>
			/// Format this token uses to render.
			/// </summary>
			public Format? Format { get; internal set; }

			/// <summary>
			/// Creates a token with the specified format.
			/// </summary>
			/// <param name="format">Format this token uses.</param>
			protected Token(Format format)
			{
				this.Format = format;
			}

			/// <summary>
			/// Creates a blank token format.
			/// </summary>
			protected Token()
			{
			}
		}
	}
}
