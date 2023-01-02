using System;

namespace ACadSharp.Entities
{
    public partial class MText
    {
        public class Font
        {
            public ReadOnlyMemory<char> FontFamily { get; set; } = default;
            public bool IsBold { get; set; } = false;
            public bool IsItalic { get; set; } = false;
            public int CodePage { get; set; } = 0;
            public int Pitch { get; set; } = 0;

            public Font()
            {
                
            }

            public Font(Font original)
            {
                FontFamily = original.FontFamily;
                IsBold = original.IsBold;
                IsItalic = original.IsItalic;
                CodePage = original.CodePage;
                Pitch = original.Pitch;
            }

            internal Font(string formats)
            {
                // Used only for testing
                if (formats.Contains("B"))
                    IsBold = true;

                if (formats.Contains("I"))
                    IsItalic = true;

                if (formats.Contains("b"))
                    IsBold = false;

                if (formats.Contains("i"))
                    IsItalic = false;
            }

            public void OverrideFrom(Font source)
            {
                FontFamily = source.FontFamily;
                IsBold = source.IsBold;
                IsItalic = source.IsItalic;
                CodePage = source.CodePage;
                Pitch = source.Pitch;
            }

            public void Reset()
            {
                FontFamily = default;
                IsBold = false;
                IsItalic = false;
                CodePage = 0;
                Pitch = 0;
            }


            public override bool Equals(object obj)
            {
                return Equals(obj as Font);
            }

            public bool Equals(Font other)
            {
                if(other == null)
                    return false;

                return
                    (FontFamily.Span.SequenceEqual(other.FontFamily.Span))
                    && IsBold == other.IsBold
                    && IsItalic == other.IsItalic
                    && CodePage == other.CodePage
                    && Pitch == other.Pitch;
            }

            public override string ToString()
            {
                return
                    $"F:{FontFamily}; B:{IsBold}; I:{IsItalic}; C:{CodePage}; P:{Pitch};";
            }
        }
    }
}
