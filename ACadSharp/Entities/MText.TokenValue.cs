#nullable enable
using System;
using System.Collections.Generic;

namespace ACadSharp.Entities
{
	public partial class MText
	{
		/// <summary>
		/// Contains a formatted value.
		/// </summary>
		public class TokenValue : Token, IEquatable<TokenValue>
		{
			/// <summary>
			/// Contains all the token values.  Will normally be multiple splices of memory.
			/// </summary>
			/// <remarks>Does not allocate.</remarks>
			public IReadOnlyList<ReadOnlyMemory<char>>? Values { get; set; }

			/// <summary>
			/// Helper method which will combine all the <see cref="Values"/> into a single new string.
			/// </summary>
			/// <remarks>Allocates.</remarks>
			public string CombinedValues => this.Values == null ? string.Empty : string.Concat(this.Values);

			/// <summary>
			/// Creates a blank token value for holding a list of memory.
			/// </summary>
			public TokenValue()
			{
			}

			/// <summary>
			/// Creates a token value with the passed parameters for it's starting state.
			/// </summary>
			/// <param name="format">Current Format of the value.</param>
			/// <param name="value">Single value this token contains.</param>
			public TokenValue(Format format, ReadOnlyMemory<char> value)
				: base(format)
			{
				this.Values = new[] { value };
			}

			/// <summary>
			/// Creates a token value with the passed parameters for it's starting state.
			/// </summary>
			/// <param name="format">Current Format of the value.</param>
			/// <param name="values">Multiple values this token contains.</param>
			public TokenValue(Format format, ReadOnlyMemory<char>[] values)
				: base(format)
			{
				this.Values = values;
			}

			public override string ToString()
			{
				return this.CombinedValues;
			}

			public override bool Equals(object? obj)
			{
				return this.Equals(obj as TokenValue);
			}


			public bool Equals(TokenValue? other)
			{
				if (other == null)
					return false;

				return Token.AreSequencesEqual(this.Values, other.Values)
				       && this.Format?.Equals(other.Format) == true;
			}

			public override int GetHashCode()
			{
#if NETFRAMEWORK
                return base.GetHashCode();
#else
				// ReSharper disable all NonReadonlyMemberInGetHashCode
				return HashCode.Combine(this.Format, this.Values);
#endif
			}
		}
	}
}
