#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace ACadSharp.Entities
{
	public partial class MText
	{
		/// <summary>
		/// Format used by MText Tokens.
		/// </summary>
		public class Format
		{
			private float? _height;
			private float? _width;
			private float? _obliquing;
			private float? _tracking;

			/// <summary>
			/// Text alignments
			/// </summary>
			public enum Alignment
			{
				/// <summary>
				/// Align to bottom
				/// </summary>
				Bottom = 0,

				/// <summary>
				/// Align to center.
				/// </summary>
				Center = 1,

				/// <summary>
				/// Align to top.
				/// </summary>
				Top = 2
			}

			/// <summary>
			/// Underline the token.
			/// </summary>
			public bool IsUnderline { get; set; }

			/// <summary>
			/// Overline the token.
			/// </summary>
			public bool IsOverline { get; set; }

			/// <summary>
			/// Strike-through the token.
			/// </summary>
			public bool IsStrikeThrough { get; set; }

			/// <summary>
			/// Font to use for the token.
			/// </summary>
			public Font Font { get; } = new Font();

			/// <summary>
			/// Text height of the token.
			/// </summary>
			public float? Height
			{
				get => this._height;
				set => this._height = value;
			}

			/// <summary>
			/// True if the passed height is a relative height.
			/// </summary>
			public bool IsHeightRelative { get; set; }

			/// <summary>
			/// Text width.
			/// </summary>
			public float? Width
			{
				get => this._width;
				set => this._width = value;
			}

			/// <summary>
			/// Text Obliquing
			/// </summary>
			public float? Obliquing
			{
				get => this._obliquing;
				set => this._obliquing = value;
			}

			/// <summary>
			/// Text Tracking
			/// </summary>
			public float? Tracking
			{
				get => this._tracking;
				set => this._tracking = value;
			}

			/// <summary>
			/// Vertical text alignment.
			/// </summary>
			public Alignment? Align { get; set; }

			/// <summary>
			/// Color of the token.
			/// </summary>
			public Color? Color { get; set; }

			/// <summary>
			/// Paragraph formatting values.
			/// </summary>
			public List<ReadOnlyMemory<char>> Paragraph { get; } = new List<ReadOnlyMemory<char>>();

			/// <summary>
			/// Creates a blank format.
			/// </summary>
			public Format()
			{
			}

			/// <summary>
			/// Creates a format with the specified simple formats.  Testing use onle.
			/// </summary>
			/// <param name="formats">Format string to use.</param>
			internal Format(string formats)
			{
				// Used only for testing
				if (formats.Contains("L")) this.IsUnderline = true;

				if (formats.Contains("O")) this.IsOverline = true;

				if (formats.Contains("K")) this.IsStrikeThrough = true;

				if (formats.Contains("l")) this.IsUnderline = false;

				if (formats.Contains("o")) this.IsOverline = false;

				if (formats.Contains("k")) this.IsStrikeThrough = false;
			}

			/// <summary>
			/// Overrides the current format values with the passed format
			/// </summary>
			/// <param name="source">Source format to copy from.</param>
			public void OverrideFrom(Format? source)
			{
				if (source == null)
					return;

				this.IsUnderline = source.IsUnderline;
				this.IsOverline = source.IsOverline;
				this.IsStrikeThrough = source.IsStrikeThrough;
				this.Font.OverrideFrom(source.Font);
				this.Height = source.Height;
				this.IsHeightRelative = source.IsHeightRelative;
				this.Width = source.Width;
				this.Obliquing = source.Obliquing;
				this.Tracking = source.Tracking;
				this.Align = source.Align;
				this.Color = source.Color;
				this.Paragraph.Clear();
				this.Paragraph.AddRange(source.Paragraph);
			}

			/// <summary>
			/// Resets the format to the default values for reuse.
			/// </summary>
			public void Reset()
			{
				this.IsUnderline = false;
				this.IsOverline = false;
				this.IsStrikeThrough = false;
				this.Font.Reset();
				this.Height = null;
				this.IsHeightRelative = false;
				this.Width = null;
				this.Obliquing = null;
				this.Tracking = null;
				this.Align = null;
				this.Color = null;
				this.Paragraph.Clear();
			}

			public override bool Equals(object? obj)
			{
				return this.Equals(obj as Format);
			}

			/// <summary>
			/// Checks to see if the passed format equals this format.
			/// </summary>
			/// <param name="other">Other format to compare to.</param>
			/// <returns>True of the formats are equal, false otherwise.</returns>
			public bool Equals(Format? other)
			{
				if (other == null)
					return false;

				return this.IsUnderline == other.IsUnderline
				       && this.IsOverline == other.IsOverline
				       && this.IsStrikeThrough == other.IsStrikeThrough
				       && isApproximateEqual(this.Height, other.Height)
				       && this.IsHeightRelative == other.IsHeightRelative
				       && isApproximateEqual(this.Width, other.Width)
				       && isApproximateEqual(this.Obliquing, other.Obliquing)
				       && isApproximateEqual(this.Tracking, other.Tracking)
				       && Nullable.Equals(this.Align, other.Align)
				       && this.Color.Equals(other.Color)
				       && this.Font.Equals(other.Font)
				       && this.AreParagraphsEqual(other.Paragraph);
			}

			[SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
			public override int GetHashCode()
			{
#if NETFRAMEWORK
                return base.GetHashCode();
#else
				var hashCode = new HashCode();
				hashCode.Add(this.Font);
				hashCode.Add(this.IsUnderline);
				hashCode.Add(this.IsOverline);
				hashCode.Add(this.IsStrikeThrough);
				hashCode.Add(this.Height);
				hashCode.Add(this.IsHeightRelative);
				hashCode.Add(this.Width);
				hashCode.Add(this.Obliquing);
				hashCode.Add(this.Tracking);
				hashCode.Add(this.Align);
				hashCode.Add(this.Color);
				hashCode.Add(this.Paragraph);
				return hashCode.ToHashCode();
#endif
			}

			public override string ToString()
			{
				return
					$"U:{this.IsUnderline}; O:{this.IsOverline}; S:{this.IsStrikeThrough}; H:{this.Height}{(this.IsHeightRelative ? "x" : "")}; W: {this.Width}; Align: {this.Align}; Color: {this.Color}; Font: {this.Font} ";
			}

			/// <summary>
			/// Checks to see if the passed format's contents are equal to this format.
			/// </summary>
			/// <param name="other">Other format to compare to.</param>
			/// <returns>True of the formats are equal, false otherwise.</returns>
			internal bool AreParagraphsEqual(List<ReadOnlyMemory<char>>? other)
			{

				if (Nullable.Equals(this.Paragraph, other))
				{
					return true;
				}
				else
				{
					if (other != null && this.Paragraph.Count == other.Count)
					{
						if (this.Paragraph.Count == 0)
						{
							return true;
						}
						else
						{
							bool paragraphsEqual = false;
							for (int i = 0; i < this.Paragraph.Count; i++)
							{
								if (this.Paragraph[i].Span.SequenceEqual(other[i].Span))
								{
									paragraphsEqual = true;
								}
								else
								{
									paragraphsEqual = false;
									break;
								}
							}

							return paragraphsEqual;
						}
					}

					return false;
				}
			}

			private static bool isApproximateEqual(float? source, float? other, float precision = 0.1f)
			{
				// Both values are null
				if (source == null && other == null)
					return true;

				if (source == null || other == null)
					return false;

				return Math.Abs(source.Value - other.Value) < precision;
			}
		}
	}
}
