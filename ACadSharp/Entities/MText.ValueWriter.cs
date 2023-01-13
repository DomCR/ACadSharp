#nullable enable
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;

namespace ACadSharp.Entities
{
	public partial class MText
	{
		/// <summary>
		/// Writer which takes Tokens and serializes it.
		/// </summary>
		public class ValueWriter
		{
			//private List<ReadOnlyMemory<char>> _outputList = null!;
			private readonly Format _defaultFormat = new Format();

			private Format? _currentFormat;

			// Tokens used to prevent memory allocation during serialization.
			//                                                      ⌄0         ⌄10       ⌄20       ⌄30       ⌄40       ⌄50       ⌄60       ⌄70       ⌄80       ⌄90   
			private static readonly ReadOnlyMemory<char> _tokens = @"\\P\~\{\}\A0;\A1;\A2;\L\l\O\o\K\k\p\;,\S\#\^\/x;%%D%%P%%C{\f|b0|b1|i0|i1|c|p\T\Q\H{\c{\C0123456789.-\W"
					.AsMemory();

			private static readonly ReadOnlyMemory<char> _tokenEscapeCharacter = _tokens.Slice(0, 1);
			private static readonly ReadOnlyMemory<char> _tokenEscapedNewLine = _tokens.Slice(1, 2);
			private static readonly ReadOnlyMemory<char> _tokenEscapedNbs = _tokens.Slice(3, 2);
			private static readonly ReadOnlyMemory<char> _tokenEscapedOpenBrace = _tokens.Slice(5, 2);
			private static readonly ReadOnlyMemory<char> _tokenEscapedClosedBrace = _tokens.Slice(7, 2);
			private static readonly ReadOnlyMemory<char> _tokenClosedBrace = _tokens.Slice(8, 1);

			private static readonly ReadOnlyMemory<char> _tokenAlignmentBottom = _tokens.Slice(9, 4);
			private static readonly ReadOnlyMemory<char> _tokenAlignmentCenter = _tokens.Slice(13, 4);
			private static readonly ReadOnlyMemory<char> _tokenAlignmentTop = _tokens.Slice(17, 4);

			private static readonly ReadOnlyMemory<char> _tokenControlCodeL = _tokens.Slice(21, 2);
			private static readonly ReadOnlyMemory<char> _tokenControlCodeLOut = _tokens.Slice(23, 2);
			private static readonly ReadOnlyMemory<char> _tokenControlCodeO = _tokens.Slice(25, 2);
			private static readonly ReadOnlyMemory<char> _tokenControlCodeOOut = _tokens.Slice(27, 2);
			private static readonly ReadOnlyMemory<char> _tokenControlCodeK = _tokens.Slice(29, 2);
			private static readonly ReadOnlyMemory<char> _tokenControlCodeKOut = _tokens.Slice(31, 2);
			private static readonly ReadOnlyMemory<char> _tokenControlParagraph = _tokens.Slice(33, 2);
			private static readonly ReadOnlyMemory<char> _tokenEscapedSemiColon = _tokens.Slice(35, 2);
			private static readonly ReadOnlyMemory<char> _tokenSemiColon = _tokens.Slice(36, 1);
			private static readonly ReadOnlyMemory<char> _tokenComma = _tokens.Slice(37, 1);

			private static readonly ReadOnlyMemory<char> _tokenFraction = _tokens.Slice(38, 2);
			private static readonly ReadOnlyMemory<char> _tokenFractionCondensed = _tokens.Slice(41, 1);
			private static readonly ReadOnlyMemory<char> _tokenFractionStacked = _tokens.Slice(43, 1);
			private static readonly ReadOnlyMemory<char> _tokenFractionFractionBar = _tokens.Slice(45, 1);
			private static readonly ReadOnlyMemory<char> _tokenFractionEscapedCondensed = _tokens.Slice(40, 2);
			private static readonly ReadOnlyMemory<char> _tokenFractionEscapedStacked = _tokens.Slice(42, 2);
			private static readonly ReadOnlyMemory<char> _tokenFractionEscapedFractionBar = _tokens.Slice(44, 2);

			private static readonly ReadOnlyMemory<char> _tokenRelativeHeightTrailer = _tokens.Slice(46, 2);

			private static readonly ReadOnlyMemory<char> _tokenDegrees = _tokens.Slice(48, 3);
			private static readonly ReadOnlyMemory<char> _tokenPlusMinus = _tokens.Slice(51, 3);
			private static readonly ReadOnlyMemory<char> _tokenDiameter = _tokens.Slice(54, 3);

			private static readonly ReadOnlyMemory<char> _tokenFontStartBracket = _tokens.Slice(57, 3);
			private static readonly ReadOnlyMemory<char> _tokenFontStart = _tokens.Slice(58, 2);

			private static readonly ReadOnlyMemory<char> _tokenFontBold0 = _tokens.Slice(60, 3);
			private static readonly ReadOnlyMemory<char> _tokenFontBold1 = _tokens.Slice(63, 3);

			private static readonly ReadOnlyMemory<char> _tokenFontItalic0 = _tokens.Slice(66, 3);
			private static readonly ReadOnlyMemory<char> _tokenFontItalic1 = _tokens.Slice(69, 3);

			private static readonly ReadOnlyMemory<char> _tokenFontCodePageStart = _tokens.Slice(72, 2);
			private static readonly ReadOnlyMemory<char> _tokenFontPitchStart = _tokens.Slice(74, 2);

			private static readonly ReadOnlyMemory<char> _tokenTextTracking = _tokens.Slice(76, 2);
			private static readonly ReadOnlyMemory<char> _tokenTextObliquing = _tokens.Slice(78, 2);
			private static readonly ReadOnlyMemory<char> _tokenTextHeight = _tokens.Slice(80, 2);
			private static readonly ReadOnlyMemory<char> _tokenTextColorTrueBracket = _tokens.Slice(82, 3);
			private static readonly ReadOnlyMemory<char> _tokenTextColorTrue = _tokens.Slice(83, 2);
			private static readonly ReadOnlyMemory<char> _tokenTextColorIndexBracket = _tokens.Slice(85, 3);
			private static readonly ReadOnlyMemory<char> _tokenTextColorIndex = _tokens.Slice(86, 2);

			private static readonly ReadOnlyMemory<char> _token0 = _tokens.Slice(88, 1);
			private static readonly ReadOnlyMemory<char> _token1 = _tokens.Slice(89, 1);
			private static readonly ReadOnlyMemory<char> _token2 = _tokens.Slice(90, 1);
			private static readonly ReadOnlyMemory<char> _token3 = _tokens.Slice(91, 1);
			private static readonly ReadOnlyMemory<char> _token4 = _tokens.Slice(92, 1);
			private static readonly ReadOnlyMemory<char> _token5 = _tokens.Slice(93, 1);
			private static readonly ReadOnlyMemory<char> _token6 = _tokens.Slice(94, 1);
			private static readonly ReadOnlyMemory<char> _token7 = _tokens.Slice(95, 1);
			private static readonly ReadOnlyMemory<char> _token8 = _tokens.Slice(96, 1);
			private static readonly ReadOnlyMemory<char> _token9 = _tokens.Slice(97, 1);

			private static readonly ReadOnlyMemory<char> _tokenPeriod = _tokens.Slice(98, 1);
			private static readonly ReadOnlyMemory<char> _tokenNegative = _tokens.Slice(99, 1);

			private static readonly ReadOnlyMemory<char> _tokenWidth = _tokens.Slice(100, 2);

			private int _lastConsumedPosition;
			private int _position;
			private ReadOnlyMemory<char> _values;
			private int _closeFormatCount;
			private Walker? _visitor;

			public delegate void Walker(in ReadOnlyMemory<char> walk);

			/// <summary>
			/// Serializes the tokens into a list of memory.
			/// </summary>
			/// <param name="tokens">Tokens to Serialize</param>
			/// <returns>List of memory</returns>
			public StringBuilder Serialize(Token[] tokens)
			{
				var stringBuilder = new StringBuilder();

				void Visitor(in ReadOnlyMemory<char> value)
				{
					stringBuilder.Append(value);
				}

				this.SerializeWalker(Visitor, tokens);

				return stringBuilder;
			}

			/// <summary>
			/// Walks through the tokens while serializing.
			/// </summary>
			/// <param name="visitor">Visitor to walk through the output.</param>
			/// <param name="tokens">Tokens to serialize.</param>
			public void SerializeWalker(in Walker visitor, Token[] tokens)
			{
				this._visitor = visitor;
				this._lastConsumedPosition = 0;
				this._position = 0;
				this._currentFormat = this._defaultFormat;
				this._closeFormatCount = 0;

				foreach (var token in tokens)
				{
					this.outputAnyFormatChanges(token.Format);

					if (token is TokenValue tokenValue)
					{
						this.writeTokenValue(tokenValue);
					}
					else if (token is TokenFraction tokenFraction)
					{
						this.writeTokenFraction(tokenFraction);
					}

					if (this._closeFormatCount > 0)
					{
						for (int i = 0; i < this._closeFormatCount; i++) this._visitor!.Invoke(_tokenClosedBrace);

						this._currentFormat = this._defaultFormat;
						this._closeFormatCount = 0;
					}
				}

				this._visitor = null;
				this._values = null;
			}

			/// <summary>
			/// Writes out the fraction token.
			/// </summary>
			/// <param name="tokenFraction">Token to write.</param>
			/// <exception cref="ArgumentOutOfRangeException"></exception>
			private void writeTokenFraction(TokenFraction tokenFraction)
			{
				this._visitor!.Invoke(_tokenFraction);

				if (tokenFraction.Numerator != null)
				{
					for (var i = 0; i < tokenFraction.Numerator.Count; i++)
					{
						this._lastConsumedPosition = 0;
						this.writeContents(tokenFraction.Numerator[i], true);
					}
				}

				this._visitor!.Invoke(tokenFraction.DividerType switch
				{
					TokenFraction.Divider.Stacked => _tokenFractionStacked,
					TokenFraction.Divider.FractionBar => _tokenFractionFractionBar,
					TokenFraction.Divider.Condensed => _tokenFractionCondensed,
					_ => throw new ArgumentOutOfRangeException()
				});

				if (tokenFraction.Denominator != null)
				{
					for (var i = 0; i < tokenFraction.Denominator.Count; i++)
					{
						this._lastConsumedPosition = 0;
						this.writeContents(tokenFraction.Denominator[i], true);
					}
				}

				this._visitor!.Invoke(_tokenSemiColon);

			}

			private void writeTokenValue(TokenValue tokenValue)
			{
				if (tokenValue.Values == null)
					return;

				for (int i = 0; i < tokenValue.Values.Count; i++)
				{
					this._lastConsumedPosition = 0;
					this.writeContents(tokenValue.Values[i]);
				}
			}

			private void writeContents(ReadOnlyMemory<char> values, bool fractionEscapes = false)
			{
				var spanValues = values.Span;
				this._values = values;

				for (this._position = 0; this._position < spanValues.Length; this._position++)
				{
					char token = spanValues[this._position];
					if (token == '\\')
					{
						this.appendCharacter(_tokenEscapeCharacter);
					}
					else if (token == '\n')
					{
						this.replaceCharacter(_tokenEscapedNewLine);
					}
					else if (token == '\u00A0')
					{
						// Non Breaking Space
						this.replaceCharacter(_tokenEscapedNbs);
					}
					else if (token == '°')
					{
						this.replaceCharacter(_tokenDegrees);
					}
					else if (token == '±')
					{
						this.replaceCharacter(_tokenPlusMinus);
					}
					else if (token == 'Ø')
					{
						this.replaceCharacter(_tokenDiameter);
					}
					else if (token == '{')
					{
						this.replaceCharacter(_tokenEscapedOpenBrace);
					}
					else if (token == '}')
					{
						this.replaceCharacter(_tokenEscapedClosedBrace);
					}
					else
					{
						if (fractionEscapes)
						{
							if (token == '#')
							{
								this.replaceCharacter(_tokenFractionEscapedCondensed);
							}
							else if (token == '/')
							{
								this.replaceCharacter(_tokenFractionEscapedFractionBar);
							}
							else if (token == '^')
							{
								this.replaceCharacter(_tokenFractionEscapedStacked);
							}
							else if (token == ';')
							{
								this.replaceCharacter(_tokenEscapedSemiColon);
							}
						}
					}
				}

				if (this._position != this._lastConsumedPosition)
				{
					// If the lastWritePosition is zero, this is a special case for when nothing has been transformed in the value.
					// and we can just output the entire Memory<char>
					this._visitor!.Invoke(this._lastConsumedPosition == 0
						? values
						: this._values.Slice(this._lastConsumedPosition, this._position - this._lastConsumedPosition));
				}
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private void replaceCharacter(ReadOnlyMemory<char> replaceWith)
			{
				var writeLength = this._position - this._lastConsumedPosition;
				if (writeLength > 0) this._visitor!.Invoke(this._values.Slice(this._lastConsumedPosition, writeLength));

				this._visitor!.Invoke(replaceWith);
				this._lastConsumedPosition = this._position + 1;
			}

			private void appendCharacter(ReadOnlyMemory<char> appendCharacter)
			{
				var writeLength = this._position - this._lastConsumedPosition;
				if (writeLength > 0) this._visitor!.Invoke(this._values.Slice(this._lastConsumedPosition, writeLength));

				this._visitor!.Invoke(appendCharacter);
				this._lastConsumedPosition = this._position;
			}

			[SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
			private void outputAnyFormatChanges(Format? newFormat)
			{
				if (newFormat == null || this._currentFormat == null)
					throw new InvalidOperationException("Format can't be null");

				if (this._currentFormat.Align != newFormat.Align)
				{
					this._visitor!.Invoke(newFormat.Align switch
					{
						Format.Alignment.Bottom => _tokenAlignmentBottom,
						Format.Alignment.Center => _tokenAlignmentCenter,
						Format.Alignment.Top => _tokenAlignmentTop,
						null => _tokenAlignmentCenter,
						_ => throw new ArgumentOutOfRangeException()
					});
				}

				if (this._currentFormat.IsUnderline != newFormat.IsUnderline)
				{
					this._visitor!.Invoke(newFormat.IsUnderline
						? _tokenControlCodeL
						: _tokenControlCodeLOut);
				}

				if (this._currentFormat.IsOverline != newFormat.IsOverline)
				{
					this._visitor!.Invoke(newFormat.IsOverline
						? _tokenControlCodeO
						: _tokenControlCodeOOut);
				}

				if (this._currentFormat.IsStrikeThrough != newFormat.IsStrikeThrough)
				{
					this._visitor!.Invoke(newFormat.IsStrikeThrough
						? _tokenControlCodeK
						: _tokenControlCodeKOut);
				}

				if (this._currentFormat.Tracking != newFormat.Tracking && newFormat.Tracking != null)
				{
					this._visitor!.Invoke(_tokenTextTracking);
					this.outputFloat(newFormat.Tracking.Value);
					this._visitor!.Invoke(_tokenSemiColon);
				}

				if (this._currentFormat.Obliquing != newFormat.Obliquing && newFormat.Obliquing != null)
				{
					this._visitor!.Invoke(_tokenTextObliquing);
					this.outputFloat(newFormat.Obliquing.Value);
					this._visitor!.Invoke(_tokenSemiColon);
				}

				if (this._currentFormat.Width != newFormat.Width && newFormat.Width != null)
				{
					this._visitor!.Invoke(_tokenWidth);
					this.outputFloat(newFormat.Width.Value);
					this._visitor!.Invoke(_tokenSemiColon);
				}

				if (this._currentFormat.Height != newFormat.Height && newFormat.Height != null)
				{
					this._visitor!.Invoke(_tokenTextHeight);
					this.outputFloat(newFormat.Height.Value);
					this._visitor!.Invoke(newFormat.IsHeightRelative ? _tokenRelativeHeightTrailer : _tokenSemiColon);
				}

				if (newFormat.Color != null)
				{
					var value = newFormat.Color.Value;
					if (value.IsTrueColor)
					{
						if (this._closeFormatCount == 0)
						{
							this._visitor!.Invoke(_tokenTextColorTrueBracket);
							this._closeFormatCount++;
						}
						else
						{
							this._visitor!.Invoke(_tokenTextColorTrue);
						}

						this.outputUint((uint)value.TrueColor);
					}
					else
					{
						if (this._closeFormatCount == 0)
						{
							this._visitor!.Invoke(_tokenTextColorIndexBracket);
							this._closeFormatCount++;
						}
						else
						{
							this._visitor!.Invoke(_tokenTextColorIndex);
						}

						this.outputUint((uint)value.Index);
					}

					this._visitor!.Invoke(_tokenSemiColon);
				}

				if (!this._currentFormat.Font.Equals(newFormat.Font))
				{
					if (this._closeFormatCount == 0)
					{
						this._visitor!.Invoke(_tokenFontStartBracket);
						this._closeFormatCount++;
					}
					else
					{
						this._visitor!.Invoke(_tokenFontStart);
					}

					this._visitor!.Invoke(newFormat.Font.FontFamily);
					this._visitor!.Invoke(newFormat.Font.IsBold ? _tokenFontBold1 : _tokenFontBold0);
					this._visitor!.Invoke(newFormat.Font.IsItalic ? _tokenFontItalic1 : _tokenFontItalic0);
					this._visitor!.Invoke(_tokenFontCodePageStart);
					//_visitor!.Invoke(newFormat.Font.CodePage.ToString().AsMemory());
					this.outputUint((uint)newFormat.Font.CodePage);
					this._visitor!.Invoke(_tokenFontPitchStart);
					//_visitor!.Invoke(newFormat.Font.Pitch.ToString().AsMemory());
					this.outputUint((uint)newFormat.Font.Pitch);
					this._visitor!.Invoke(_tokenSemiColon);
				}

				if (newFormat.AreParagraphsEqual(this._currentFormat.Paragraph) == false)
				{
					this._visitor!.Invoke(_tokenControlParagraph);

					var count = newFormat.Paragraph.Count;
					for (int i = 0; i < count; i++)
					{
						this._visitor!.Invoke(newFormat.Paragraph[i]);
						if (i + 1 != count)
						{
							this._visitor!.Invoke(_tokenComma);
						}
					}

					this._visitor!.Invoke(_tokenSemiColon);
				}

				this._currentFormat = newFormat;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private void outputUint(uint value)
			{
				int bufferLength = countDigits(value);
				if (bufferLength == 1)
				{
					this.outputNumber(value);
					return;
				}

				Span<uint> outputArray = stackalloc uint[bufferLength];
				var position = bufferLength - 1;
				do
				{
					(value, outputArray[position--]) = divRem(value, 10);
				} while (value != 0);

				for (int i = 0; i < bufferLength; i++) this.outputNumber(outputArray[i]);
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private void outputNumber(uint value)
			{
				switch (value)
				{
					case 0:
						this._visitor?.Invoke(_token0);
						break;
					case 1:
						this._visitor?.Invoke(_token1);
						break;
					case 2:
						this._visitor?.Invoke(_token2);
						break;
					case 3:
						this._visitor?.Invoke(_token3);
						break;
					case 4:
						this._visitor?.Invoke(_token4);
						break;
					case 5:
						this._visitor?.Invoke(_token5);
						break;
					case 6:
						this._visitor?.Invoke(_token6);
						break;
					case 7:
						this._visitor?.Invoke(_token7);
						break;
					case 8:
						this._visitor?.Invoke(_token8);
						break;
					case 9:
						this._visitor?.Invoke(_token9);
						break;
					case 10:
						// Special case for the period.
						this._visitor?.Invoke(_tokenPeriod);
						break;
				}
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private static (uint Quotient, uint Remainder) divRem(uint left, uint right)
			{
				uint quotient = left / right;
				return (quotient, left - (quotient * right));
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private static int countDigits(uint value)
			{
				int digits = 1;
				if (value >= 100000)
				{
					value /= 100000;
					digits += 5;
				}

				if (value < 10)
				{
					// no-op
				}
				else if (value < 100)
				{
					digits++;
				}
				else if (value < 1000)
				{
					digits += 2;
				}
				else if (value < 10000)
				{
					digits += 3;
				}
				else
				{
					digits += 4;
				}

				return digits;
			}

			/// <summary>
			/// Outputs a float to the visitor.
			/// </summary>
			/// <param name="value">Float value to output</param>
			/// <exception cref="ArgumentOutOfRangeException"></exception>
			/// <seealso>https://stackoverflow.com/a/41254697</seealso>
			private void outputFloat(float value)
			{
				// Handle the 0 case
				if (value == 0)
				{
					this._visitor!.Invoke(_token0);
					return;
				}

				bool isNegative = value < 0;
				// Handle the negative case
				if (isNegative)
				{
					value = -value;
				}

				int nbDecimals = 0;
				while (value < 10000000)
				{
					value *= 10;
					nbDecimals++;
				}

				long valueLong = (long)Math.Round(value);
				// Parse the number in reverse order
				bool isLeadingZero = true;
				Span<uint> outputArray = stackalloc uint[20];
				int outPosition = 0;
				while (valueLong != 0 || nbDecimals >= 0)
				{
					// We stop removing leading 0 when non-0 or decimal digit
					if (valueLong % 10 != 0 || nbDecimals <= 0)
						isLeadingZero = false;

					// Write the last digit (unless a leading zero)
					if (!isLeadingZero)
						outputArray[outPosition++] = (uint)(valueLong % 10);

					// Add the decimal point
					if (--nbDecimals == 0 && !isLeadingZero)
						outputArray[outPosition++] = 10;

					valueLong /= 10;
				}

				if (isNegative) this._visitor!.Invoke(_tokenNegative);

				for (int i = outPosition - 1; i >= 0; i--) this.outputNumber(outputArray[i]);
			}
		}
	}
}
