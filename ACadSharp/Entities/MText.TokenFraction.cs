#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace ACadSharp.Entities
{
    public partial class MText
    {
        /// <summary>
        /// Contains a formatted fraction value.
        /// </summary>
        public class TokenFraction : Token
        {
            /// <summary>
            /// Type of divisor used to split the Numerator and Denominator.
            /// </summary>
            public enum Divider
            {
                /// <summary>
                /// Values are stacked on top of each other less a fraction divider bar symbol.
                /// </summary>
                Stacked,

                /// <summary>
                /// Values are stacked on top of each other with a fraction divider bar symbol.
                /// </summary>
                FractionBar,

                /// <summary>
                /// Values are side by side with a slight sup/sub divider. Example: ½
                /// </summary>
                Condensed
            }

            /// <summary>
            /// Contains all the numerator token values.  Will normally be multiple splices of memory.
            /// </summary>
            /// <remarks>Does not allocate.</remarks>
            public IReadOnlyList<ReadOnlyMemory<char>>? Numerator { get; internal set; }

            /// <summary>
            /// Helper method which will combine all the <see cref="Numerator"/> into a single new string.
            /// </summary>
            /// <remarks>Allocates.</remarks>
            public string NumeratorCombined => Numerator == null ? string.Empty : string.Concat(Numerator);

            /// <summary>
            /// Contains all the denominator token values.  Will normally be multiple splices of memory.
            /// </summary>
            /// <remarks>Does not allocate.</remarks>
            public IReadOnlyList<ReadOnlyMemory<char>>? Denominator { get; internal set; }

            /// <summary>
            /// Helper method which will combine all the <see cref="Denominator"/> into a single new string.
            /// </summary>
            /// <remarks>Allocates.</remarks>
            public string DenominatorCombined => Denominator == null ? string.Empty : string.Concat(Denominator);

            /// <summary>
            /// Divisor type of the fraction.
            /// </summary>
            public Divider DividerType { get; set; }

            /// <summary>
            /// Creates a blank fraction token for holding lists of memory.
            /// </summary>
            public TokenFraction()
            {
            }

            /// <summary>
            /// Creates a fraction token with the passed parameters for it's starting state.
            /// </summary>
            /// <param name="format">Current Format of the value.</param>
            public TokenFraction(Format format)
                : base(format)
            {
            }


            /// <summary>
            /// Creates a fraction token with the passed parameters for it's starting state. Used for testing.
            /// </summary>
            /// <param name="numerator">Numerator to set.</param>
            /// <param name="denominator">Denominator to set.</param>
            /// <param name="divider">Divisor to set.</param>
            internal TokenFraction(string? numerator, string? denominator, Divider divider)
                : this(new Format(), numerator, denominator, divider)
            {
            }

            /// <summary>
            /// Creates a fraction token with the passed parameters for it's starting state. Used for testing.
            /// </summary>
            /// <param name="format">Format to set.</param>
            /// <param name="numerator">Numerator to set.</param>
            /// <param name="denominator">Denominator to set.</param>
            /// <param name="divider">Divisor to set.</param>
            internal TokenFraction(Format format, string? numerator, string? denominator, Divider divider)
                : base(format)
            {
                Numerator = new[] { numerator.AsMemory() };
                Denominator = new[] { denominator.AsMemory() };
                DividerType = divider;
            }

            protected bool equals(TokenFraction other)
            {
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

                if (obj.GetType() != GetType())
                {
                    return false;
                }

                return equals((TokenFraction)obj);
            }

            [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
            public override int GetHashCode()
            {
#if NETFRAMEWORK
                return base.GetHashCode();
#else
                return HashCode.Combine(Numerator, Denominator, (int)DividerType, Format);
#endif
            }

            public override string ToString()
            {
                return $"{NumeratorCombined}/{DenominatorCombined}; {DividerType}";
            }
        }

    }
}
