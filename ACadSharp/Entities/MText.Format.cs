using System;
using System.Collections.Generic;

namespace ACadSharp.Entities
{
    public partial class MText
    {
        public class Format
        {
            public enum Alignment
            {
                Bottom = 0,
                Center = 1,
                Top = 2
            }

            public bool IsUnderline { get; set; } = false;
            public bool IsOverline { get; set; } = false;
            public bool IsStrikeThrough { get; set; } = false;

            public readonly Font Font;
            public float? Height { get; set; } = null;
            public bool IsHeightRelative { get; set; }
            public float? Width { get; set; } = null;
            public float? Obliquing { get; set; } = null;
            public float? Tracking { get; set; } = null;
            public Alignment? Align { get; set; } = null;
            public Color? Color { get; set; } = null;
            public List<ReadOnlyMemory<char>> Paragraph { get; } = new List<ReadOnlyMemory<char>>();

            public Format()
            {
                Font = new Font();
            }

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

            public void OverrideFrom(Format source)
            {
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


            public override bool Equals(object obj)
            {
                return Equals(obj as Format);
            }

            public bool Equals(Format other)
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

            public bool AreParagraphsEqual(List<ReadOnlyMemory<char>>? other)
            {
                
                if (Nullable.Equals(Paragraph, other))
                {
                    return true;
                }
                else
                {
                    if (Paragraph != null
                        && other != null
                        && Paragraph.Count == other.Count)
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

            public override string ToString()
            {
                return
                    $"U:{IsUnderline}; O:{IsOverline}; S:{IsStrikeThrough}; H:{Height}{(IsHeightRelative ? "x" : "")}; W: {Width}; Align: {Align}; Color: {Color}; Font: {Font} ";
            }
            private string[]? DeepCopyArray(string[]? array)
            {
                if (array == null)
                    return null;

                var copyArray = new string[array.Length];

                for (int i = 0; i < array.Length; i++)
                {
                    copyArray[i] = new string(array[0].ToCharArray());
                }

                Array.Copy(array, copyArray, array.Length);

                return copyArray;
            }
        }
    }
}
