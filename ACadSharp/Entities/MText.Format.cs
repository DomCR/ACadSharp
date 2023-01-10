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
            public readonly Font Font;

            /// <summary>
            /// Text height of the token.
            /// </summary>
            public float? Height { get; set; }

            /// <summary>
            /// True if the passed height is a relative height.
            /// </summary>
            public bool IsHeightRelative { get; set; }

            /// <summary>
            /// Text width.
            /// </summary>
            public float? Width { get; set; }

            /// <summary>
            /// Text Obliquing
            /// </summary>
            public float? Obliquing { get; set; }

            /// <summary>
            /// Text Tracking
            /// </summary>
            public float? Tracking { get; set; }

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
                Font = new Font();
            }

            /// <summary>
            /// Creates a format with the specified simple formats.  Testing use onle.
            /// </summary>
            /// <param name="formats">Format string to use.</param>
            internal Format(string formats)
            {
                // Used only for testing
                if (formats.Contains("L"))
                    IsUnderline = true;

                if (formats.Contains("O"))
                    IsOverline = true;

                if (formats.Contains("K"))
                    IsStrikeThrough = true;

                if (formats.Contains("l"))
                    IsUnderline = false;

                if (formats.Contains("o"))
                    IsOverline = false;

                if (formats.Contains("k"))
                    IsStrikeThrough = false;

                Font = new Font();
            }

            /// <summary>
            /// Overrides the current format values with the passed format
            /// </summary>
            /// <param name="source">Source format to copy from.</param>
            public void OverrideFrom(Format? source)
            {
                if (source == null)
                    return;

                IsUnderline = source.IsUnderline;
                IsOverline = source.IsOverline;
                IsStrikeThrough = source.IsStrikeThrough;
                Font.OverrideFrom(source.Font);
                Height = source.Height;
                IsHeightRelative = source.IsHeightRelative;
                Width = source.Width;
                Obliquing = source.Obliquing;
                Tracking = source.Tracking;
                Align = source.Align;
                Color = source.Color;
                Paragraph.Clear();
                Paragraph.AddRange(source.Paragraph);
            }

            /// <summary>
            /// Resets the format to the default values for reuse.
            /// </summary>
            public void Reset()
            {
                IsUnderline = false;
                IsOverline = false;
                IsStrikeThrough = false;
                Font.Reset();
                Height = null;
                IsHeightRelative = false;
                Width = null;
                Obliquing = null;
                Tracking = null;
                Align = null;
                Color = null;
                Paragraph.Clear();
            }



            /// <summary>
            /// Checks to see if the passed format's contents are equal to this format.
            /// </summary>
            /// <param name="other">Other format to compare to.</param>
            /// <returns>True of the formats are equal, false otherwise.</returns>
            internal bool AreParagraphsEqual(List<ReadOnlyMemory<char>>? other)
            {

                if (Nullable.Equals(Paragraph, other))
                {
                    return true;
                }
                else
                {
                    if (other != null && Paragraph.Count == other.Count)
                    {
                        if (Paragraph.Count == 0)
                        {
                            return true;
                        }
                        else
                        {
                            bool paragraphsEqual = false;
                            for (int i = 0; i < Paragraph.Count; i++)
                            {
                                if (Paragraph[i].Span.SequenceEqual(other[i].Span))
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

            public override bool Equals(object? obj)
            {
                return Equals(obj as Format);
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

                return
                    IsUnderline == other.IsUnderline
                    && IsOverline == other.IsOverline
                    && IsStrikeThrough == other.IsStrikeThrough
                    && Nullable.Equals(Height, other.Height)
                    && IsHeightRelative == other.IsHeightRelative
                    && Nullable.Equals(Width, other.Width)
                    && Nullable.Equals(Obliquing, other.Obliquing)
                    && Nullable.Equals(Tracking, other.Tracking)
                    && Nullable.Equals(Align, other.Align)
                    && Color.Equals(other.Color)
                    && Font.Equals(other.Font)
                    && AreParagraphsEqual(other.Paragraph);
            }

            [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
            public override int GetHashCode()
            {
#if NETFRAMEWORK
                return base.GetHashCode();
#else
                var hashCode = new HashCode();
                hashCode.Add(Font);
                hashCode.Add(IsUnderline);
                hashCode.Add(IsOverline);
                hashCode.Add(IsStrikeThrough);
                hashCode.Add(Height);
                hashCode.Add(IsHeightRelative);
                hashCode.Add(Width);
                hashCode.Add(Obliquing);
                hashCode.Add(Tracking);
                hashCode.Add(Align);
                hashCode.Add(Color);
                hashCode.Add(Paragraph);
                return hashCode.ToHashCode();
#endif
            }

            public override string ToString()
            {
                return
                    $"U:{IsUnderline}; O:{IsOverline}; S:{IsStrikeThrough}; H:{Height}{(IsHeightRelative ? "x" : "")}; W: {Width}; Align: {Align}; Color: {Color}; Font: {Font} ";
            }
        }
    }
}
