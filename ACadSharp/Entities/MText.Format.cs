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

            public bool? IsUnderline { get; set; } = null;
            public bool? IsOverline { get; set; } = null;
            public bool? IsStrikeThrough { get; set; } = null;

            public readonly Font Font;
            public float? Height { get; set; } = null;
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

            /*public Format(Format original)
            {
                IsUnderline = original.IsUnderline;
                IsOverline = original.IsOverline;
                IsStrikeThrough = original.IsStrikeThrough;
                Font = new Font(original.Font);
                Height = original.Height;
                Width = original.Width;
                Obliquing = original.Obliquing;
                Tracking = original.Tracking;
                Align = original.Align;
                Color = original.Color;
                Paragraph = original.Paragraph;
            }*/

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
                IsUnderline = null;
                IsOverline = null;
                IsStrikeThrough = null;
                Font.Reset();
                Height = null;
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
                    Nullable.Equals(IsUnderline, other.IsUnderline)
                    && Nullable.Equals(IsOverline, other.IsOverline)
                    && Nullable.Equals(IsStrikeThrough, other.IsStrikeThrough)
                    && Nullable.Equals(Height, other.Height)
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
                    $"U:{IsUnderline}; O:{IsOverline}; S:{IsStrikeThrough}; H:{Height}; W: {Width}; Align: {Align}; Color: {Color}; Font: {Font} ";
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
