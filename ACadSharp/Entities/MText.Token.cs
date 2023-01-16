#nullable enable
using System.Collections.Generic;
using System;

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
			public Format? Format { get; set; }

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

			/// <summary>
			/// Checks to see if the passed lists of Memory&lt;char&gt; are equal.
			/// </summary>
			/// <param name="first">First memory to compare against.</param>
			/// <param name="second">Second memory to compare against.</param>
			/// <returns>True if the two me memory sequences are equal. False otherwise.</returns>
			internal static bool AreSequencesEqual(IReadOnlyList<ReadOnlyMemory<char>>? first, IReadOnlyList<ReadOnlyMemory<char>>? second)
			{
				if (Nullable.Equals(first, second))
				{
					return true;
				}
				else
				{
					if (first != null && second != null && first.Count == second.Count)
					{
						if (first.Count == 0)
						{
							return true;
						}
						else
						{
							bool paragraphsEqual = false;
							for (int i = 0; i < first.Count; i++)
							{
								if (first[i].Span.SequenceEqual(second[i].Span))
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
		}
	}
}
