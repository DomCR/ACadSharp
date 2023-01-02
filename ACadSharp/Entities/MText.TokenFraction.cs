using ACadSharp.Attributes;
using System;
using System.Collections.Generic;

namespace ACadSharp.Entities
{
    public partial class MText
    {
        public class TokenFraction : Token
        {
            public enum Divider
            {
                Stacked,
                FractionBar,
                Condensed
            }
            public IReadOnlyList<ReadOnlyMemory<char>> Numerator { get; internal set; }

            public string NumeratorCombined => string.Concat(Numerator);
            public IReadOnlyList<ReadOnlyMemory<char>> Denominator { get; internal set; }

            public string DenominatorCombined => string.Concat(Denominator);

            public Divider DividerType { get; set; }

            public TokenFraction()
            {
            }

            public TokenFraction(MText.Format format)
                : base(format)
            {
            }

            internal TokenFraction(string? numerator, string? denominator, Divider divider)
                : this(new MText.Format(), numerator, denominator, divider)
            {
            }

            internal TokenFraction(MText.Format format, string? numerator, string? denominator, Divider divider)
                : base(format)
            {
                Numerator = new[] { numerator.AsMemory() };
                Denominator = new[] { denominator.AsMemory() };
                DividerType = divider;
            }



            protected bool Equals(TokenFraction other)
            {
                bool MemoryEqual(ReadOnlyMemory<char>? val1, ReadOnlyMemory<char>? val2)
                {
                    if (Nullable.Equals(val1, val2))
                        return true;

                    if (val1 == null)
                        return false;

                    if (val2 == null)
                        return false;

                    return val1.Value.Span.SequenceEqual(val2.Value.Span);
                }

                return NumeratorCombined == other.NumeratorCombined
                       && DenominatorCombined == other.DenominatorCombined
                       && DividerType == other.DividerType;
            }

            public override bool Equals(object? obj)
            {
                if (ReferenceEquals(null, obj))
                {
                    return false;
                }

                if (ReferenceEquals(this, obj))
                {
                    return true;
                }

                if (obj.GetType() != this.GetType())
                {
                    return false;
                }

                return Equals((TokenFraction)obj);
            }

            public override string ToString()
            {
                return $"{NumeratorCombined}/{DenominatorCombined}; {DividerType}";
            }
        }

    }
}
