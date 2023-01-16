#nullable enable
using System;

namespace ACadSharp.Entities
{
	public partial class MText
	{
		/// <summary>
		/// Font used by MText Tokens.
		/// </summary>
		public class Font : IEquatable<Font>
		{
			/// <summary>
			/// Tet Font Family.
			/// </summary>
			public ReadOnlyMemory<char> FontFamily { get; set; }

			/// <summary>
			/// Bold Text
			/// </summary>
			public bool IsBold { get; set; }

			/// <summary>
			/// Italic text.
			/// </summary>
			public bool IsItalic { get; set; }

			/// <summary>
			/// Code page for the text.
			/// </summary>
			public int CodePage { get; set; }

			/// <summary>
			/// Text pitch.
			/// </summary>
			public int Pitch { get; set; }

			/// <summary>
			/// Creates a blank font.
			/// </summary>
			public Font()
			{

			}

			/// <summary>
			/// Overrides this font with the passed font contents.
			/// </summary>
			/// <param name="source">Source font to copy from.</param>
			public void OverrideFrom(Font source)
			{
				this.FontFamily = source.FontFamily;
				this.IsBold = source.IsBold;
				this.IsItalic = source.IsItalic;
				this.CodePage = source.CodePage;
				this.Pitch = source.Pitch;
			}

			/// <summary>
			/// Resets this font to the default values.
			/// </summary>
			public void Reset()
			{
				this.FontFamily = default;
				this.IsBold = false;
				this.IsItalic = false;
				this.CodePage = 0;
				this.Pitch = 0;
			}

			public override bool Equals(object? obj)
			{
				return this.Equals(obj as Font);
			}

			/// <summary>
			/// Checks to see if the passed font equals this font.
			/// </summary>
			/// <param name="other">Other font to compare to.</param>
			/// <returns>True of the fonts are equal, false otherwise.</returns>
			public bool Equals(Font? other)
			{
				if (other == null)
					return false;

				return
					(this.FontFamily.Span.SequenceEqual(other.FontFamily.Span))
					&& this.IsBold == other.IsBold
					&& this.IsItalic == other.IsItalic
					&& this.CodePage == other.CodePage
					&& this.Pitch == other.Pitch;
			}

			public override int GetHashCode()
			{
#if NETFRAMEWORK
                return base.GetHashCode();
#else
				// ReSharper disable all NonReadonlyMemberInGetHashCode
				return HashCode.Combine(this.FontFamily, this.IsBold, this.IsItalic, this.CodePage, this.Pitch);
#endif
			}

			public override string ToString()
			{
				return
					$"F:{this.FontFamily}; B:{this.IsBold}; I:{this.IsItalic}; C:{this.CodePage}; P:{this.Pitch};";
			}

		}
	}
}
