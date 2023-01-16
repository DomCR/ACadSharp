#nullable enable
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ACadSharp.Entities
{
	public partial class MText
	{
		/// <summary>
		/// Zero copy reader to deserialize AutoCAD M-Text value entries and return tokens.
		/// </summary>
		/// <remarks>
		/// Main goal of this reader is to be a zero copy reader.
		/// This class is NOT thread safe, but is designed to be re-used.
		/// </remarks>
		/// <seealso>
		/// https://www.cadforum.cz/en/text-formatting-codes-in-mtext-objects-tip8640
		/// https://knowledge.autodesk.com/support/autocad-lt/learn-explore/caas/CloudHelp/cloudhelp/2019/ENU/AutoCAD-LT/files/GUID-7D8BB40F-5C4E-4AE5-BD75-9ED7112E5967-htm.html
		/// https://knowledge.autodesk.com/support/autocad-lt/learn-explore/caas/CloudHelp/cloudhelp/2019/ENU/AutoCAD-LT/files/GUID-6F59DA4A-A790-4316-A79C-2CCE723A30CA-htm.html
		/// </seealso>
		public class ValueReader : IDisposable
		{
			private ReadOnlyMemory<char> _content;
			private int _length;

			private int _position;
			private int _textValueStart = -1;
			private int _textValueEnd;
			private readonly TokenValue _flushTokenValue = new TokenValue();
			private readonly TokenFraction _flushFractionValue = new TokenFraction();

			private readonly Stack<Format> _fontStateStack = new Stack<Format>(4);
			private readonly Stack<Format> _freeFormatStates = new Stack<Format>(4);
			private Format? _currentFormat;

			private readonly List<ReadOnlyMemory<char>> _mainBuffer = new List<ReadOnlyMemory<char>>(8);
			private readonly List<ReadOnlyMemory<char>> _secondBuffer = new List<ReadOnlyMemory<char>>(8);
			private Action<Token>? _visitor;
			private bool _controlCode;

			private readonly Memory<char> _charBuffer = new Memory<char>(new char[1]);

			/// <summary>
			/// Deserializes the passed content value.
			/// </summary>
			/// <param name="content">Content to deserialize.</param>
			/// <returns>Deserialized token list.  This is not a zero copy parsing process.</returns>
			/// <remarks>Not thread safe.</remarks>
			public Token[] Deserialize(string content)
			{
				return this.Deserialize(content, null);
			}


			/// <summary>
			/// Deserializes the passed content value.
			/// </summary>
			/// <param name="content">Content to deserialize.</param>
			/// <param name="baseFormat">This is the base format that will be used.</param>
			/// <returns>Deserialized token list.  This is not a zero copy parsing process.</returns>
			/// <remarks>Not thread safe.</remarks>
			public Token[] Deserialize(string content, Format? baseFormat)
			{
				return this.Deserialize(content.AsMemory(), baseFormat);
			}

			/// <summary>
			/// Deserializes the passed content value.
			/// </summary>
			/// <param name="content">Content to deserialize.</param>
			/// <param name="baseFormat">This is the base format that will be used.</param>
			/// <returns>Deserialized token list.  This is not a zero copy parsing process.</returns>
			/// <remarks>Not thread safe.</remarks>
			public Token[] Deserialize(ReadOnlyMemory<char> content, Format? baseFormat)
			{
				var list = new List<Token>();
				this.DeserializeWalker(token =>
				{
					var copiedFormat = new Format();
					copiedFormat.OverrideFrom(token.Format);

					// Copy the values if we are not using the values when walking since the memory will change
					// throughout the iteration.
					if (token is TokenValue value)
					{
						list.Add(new TokenValue()
						{
							Format = copiedFormat,
							Values = new[] { value.CombinedValues.AsMemory() }
						});
					}
					else if (token is TokenFraction fraction)
					{
						list.Add(new TokenFraction()
						{
							Format = copiedFormat,
							Denominator = new[] { fraction.DenominatorCombined.AsMemory() },
							Numerator = new[] { fraction.NumeratorCombined.AsMemory() },
							DividerType = fraction.DividerType
						});
					}

				}, content, baseFormat);

				return list.ToArray();
			}

			/// <summary>
			/// Walks the content as it is being deserialized.  The visitor is passed tokens to use.
			/// </summary>
			/// <param name="visitor">Visitor to walk through the data as it is read.</param>
			/// <param name="content">Content to walk through.</param>
			/// <returns>True on successful read.  False otherwise.</returns>
			/// <remarks>
			/// Walking is a zero copy parsing process.  This means that the content of the tokens
			/// are not guaranteed to be valid beyond the return of the visitor.
			/// Not Thread Safe.
			/// </remarks>
			public bool DeserializeWalker(Action<Token> visitor, ReadOnlyMemory<char> content)
			{
				this._currentFormat?.Reset();
				return this.DeserializeWalker(visitor, content, this._currentFormat);
			}

			/// <summary>
			/// Walks the content as it is being deserialized.  The visitor is passed tokens to use.
			/// </summary>
			/// <param name="visitor">Visitor to walk through the data as it is read.</param>
			/// <param name="content">Content to walk through.</param>
			/// <param name="baseFormat">This is the base format that will be used.</param>
			/// <returns>True on successful read.  False otherwise.</returns>
			/// <remarks>
			/// Walking is a zero copy parsing process.  This means that the content of the tokens
			/// are not guaranteed to be valid beyond the return of the visitor.
			/// Not Thread Safe.
			/// </remarks>
			public bool DeserializeWalker(Action<Token> visitor, ReadOnlyMemory<char> content, Format? baseFormat)
			{
				this._content = content;
				this._visitor = visitor;
				this._textValueStart = -1;
				this._textValueEnd = -1;
				this._length = this._content.Length;
				this._controlCode = false;
				this._position = 0;
				this._currentFormat = baseFormat;
				this.setNewCurrentFormat();

				if (this._currentFormat == null)
					throw new InvalidOperationException("Format is corrupted.");

				var spanText = this._content.Span;
				var charBufferSpan = this._charBuffer.Span;
				while (true)
				{
					var token = spanText[this._position];
					if (token == '%'
					    && this.canAdvance(2)
					    && spanText[this._position + 1] == '%')
					{
						this._controlCode = false;
						token = spanText[this._position + 2];
						if (token == 'D' || token == 'd')
						{
							charBufferSpan[0] = '°';
							this.flushText(this._charBuffer);
						}
						else if (token == 'P' || token == 'p')
						{
							// ± (PLUS-MINUS SIGN)
							charBufferSpan[0] = '\u00B1';
							this.flushText(this._charBuffer);
						}
						else if (token == 'C' || token == 'c')
						{
							// Ø (LATIN CAPITAL LETTER O WITH STROKE)
							charBufferSpan[0] = '\u00D8';
							this.flushText(this._charBuffer);
						}
						else if (token == '%')
						{
							charBufferSpan[0] = '%';
							this.flushText(this._charBuffer);
						}
						else
						{
							this.pushTextEnd();
							continue;
						}

						if (this.canAdvance(3))
						{
							this._position += 3;
							continue;
						}
						else
						{
							// If we can't advance 3, we are at the end.
							return true;
						}
					}
					else if (token == '\\')
					{
						if (this._controlCode) this._mainBuffer.Add(this._content.Slice(this._position, 1));

						this._controlCode = !this._controlCode;
					}
					else if (this._controlCode && token == 'A')
					{
						this._controlCode = false;
						this.flushText();
						if (!this.tryParseEnum<Format.Alignment>(spanText, out var value))
							return false;

						this._currentFormat.Align = value;
					}
					else if (this._controlCode && token == 'C')
					{
						this._controlCode = false;
						this.flushText();
						if (!this.tryParseControlCodeInt(spanText, out var value))
							return false;

						this._currentFormat.Color = new Color((short)value);
					}
					else if (this._controlCode && token == 'c')
					{
						this._controlCode = false;
						this.flushText();
						if (!this.tryParseControlCodeInt(spanText, out var value))
							return false;

						this._currentFormat.Color = Color.FromTrueColor(value);
					}
					else if (this._controlCode && token == 'T')
					{
						this._controlCode = false;
						this.flushText();
						if (!this.tryParseControlCodeFloat(spanText, out var value))
							return false;

						this._currentFormat.Tracking = value;
					}
					else if (this._controlCode && token == 'W')
					{
						this._controlCode = false;
						this.flushText();
						if (!this.tryParseControlCodeFloat(spanText, out var value))
							return false;

						this._currentFormat.Width = value;
					}
					else if (this._controlCode && token == 'H')
					{
						this._controlCode = false;
						this.flushText();
						if (!this.tryParseControlCodeFloatWithX(spanText, out var value, out bool relative))
							return false;
						this._currentFormat.IsHeightRelative = relative;

						if (relative)
						{
							this._currentFormat.Height *= value;
						}
						else
						{
							this._currentFormat.Height = value;
						}
					}
					else if (this._controlCode && token == 'Q')
					{
						this._controlCode = false;
						this.flushText();
						if (!this.tryParseControlCodeFloat(spanText, out var value))
							return false;

						this._currentFormat.Obliquing = value;
					}
					else if (this._controlCode && token == 'p')
					{
						this._controlCode = false;
						this.flushText();
						if (!this.trySetParagraphCodes(spanText))
							return false;
					}
					else if (this._controlCode && token == 'f')
					{
						this._controlCode = false;
						this.flushText();
						if (!this.trySetFontCodes(spanText))
							return false;
					}
					else if (this._controlCode && token == 'S')
					{
						this._controlCode = false;
						this.flushText();
						if (!this.tryParseFraction(spanText))
							return false;
					}
					else if (this._controlCode && token == 'L')
					{
						this._controlCode = false;
						this.flushText();
						this._currentFormat.IsUnderline = true;
					}
					else if (this._controlCode && token == 'l')
					{
						this._controlCode = false;
						this.flushText();
						this._currentFormat.IsUnderline = false;
					}
					else if (this._controlCode && token == 'O')
					{
						this._controlCode = false;
						this.flushText();
						this._currentFormat.IsOverline = true;
					}
					else if (this._controlCode && token == 'o')
					{
						this._controlCode = false;
						this.flushText();
						this._currentFormat.IsOverline = false;
					}
					else if (this._controlCode && token == 'K')
					{
						this._controlCode = false;
						this.flushText();
						this._currentFormat.IsStrikeThrough = true;
					}
					else if (this._controlCode && token == 'k')
					{
						this._controlCode = false;
						this.flushText();
						this._currentFormat.IsStrikeThrough = false;
					}
					else if (this._controlCode && token == 'P')
					{
						this._controlCode = false;
						charBufferSpan[0] = '\n';
						this.flushText(this._charBuffer);
					}
					else if (this._controlCode && token == '~')
					{
						// Non Breaking Space
						this._controlCode = false;
						charBufferSpan[0] = '\u00A0';
						this.flushText(this._charBuffer);
					}
					else if (!this._controlCode && token == '{')
					{
						this.flushText();
						this._fontStateStack.Push(this._currentFormat);
						this.setNewCurrentFormat();

					}
					else if (!this._controlCode && token == '}')
					{
						this.flushText();
						this._currentFormat.Reset();
						this._freeFormatStates.Push(this._currentFormat);
						this._currentFormat = this._fontStateStack.Pop();
					}
					else
					{
						this._controlCode = false;
						this.pushTextEnd();
					}

					// See if we are at the end of the string.
					if (!this.tryAdvance())
					{
						if (this._controlCode)
						{
							charBufferSpan[0] = '\\';
							this._mainBuffer.Add(this._charBuffer);
						}

						this.flushText();

						// Cleanup
						this._currentFormat.Reset();
						this._freeFormatStates.Push(this._currentFormat);

						// Should not do anything
						this._fontStateStack.Clear();
						this._visitor = null!;
						this._controlCode = false;
						this._content = null!;
						this._mainBuffer.Clear();
						this._secondBuffer.Clear();
						return true;
					}

				}
			}

			private void setNewCurrentFormat()
			{
				// ReSharper disable once InlineOutVariableDeclaration
				Format? newFormat;
#if NETFRAMEWORK
				if (this._freeFormatStates.Count > 0)
				{
					newFormat = this._freeFormatStates.Pop();
				}
				else
#else
				if (!this._freeFormatStates.TryPop(out newFormat))
#endif
				{
					newFormat = new Format();
				}

				// Copy the formatting from the current format to the new format.
				if (this._currentFormat != null)
				{
					newFormat.OverrideFrom(this._currentFormat);
				}

				this._currentFormat = newFormat;
			}

			/// <summary>
			/// Tries to parse an enum from the control code value.
			/// </summary>
			/// <param name="spanText">Text to read from.</param>
			/// <param name="value">Enum value on success.  Invalid enum on failure.</param>
			/// <returns>True on success, false otherwise.</returns>
			private bool tryParseEnum<TEnum>(ReadOnlySpan<char> spanText, out TEnum value)
				where TEnum : struct
			{
				if (!this.tryGetControlCodeValue(spanText, out var content))
				{
					value = default;
					return false;
				}

#if NET6_0_OR_GREATER
				if (Enum.TryParse(content, out value))
					return true;
#elif NETSTANDARD2_1_OR_GREATER

				// Fallback when the enum can't parse a span directly.
				if (int.TryParse(content, out var intValue))
				{
					value = Unsafe.As<int, TEnum>(ref intValue);
					return true;
				}
#else
				// Fallback when the enum can't parse a span directly.
				if (Enum.TryParse(content.ToString(), out value))
					return true;
#endif
				value = default;
				return false;
			}

			/// <summary>
			/// Tries to parse an int from the control code value.
			/// </summary>
			/// <param name="spanText">Text to read from.</param>
			/// <param name="value">Deserialized value.  Invalid data on failure.</param>
			/// <returns>True on success, false otherwise.</returns>
			private bool tryParseControlCodeInt(ReadOnlySpan<char> spanText, out int value)
			{
				if (!this.tryGetControlCodeValue(spanText, out var content))
				{
					value = -1;
					return false;
				}

#if NETFRAMEWORK
				// Fallback when the enum can't parse a span directly.
				if (int.TryParse(content.ToString(), out value))
					return true;
#else
				if (int.TryParse(content, out value))
					return true;
#endif
				return false;
			}

			/// <summary>
			/// Tries to parse a float from the control code value.
			/// </summary>
			/// <param name="spanText">Text to read from.</param>
			/// <param name="value">Deserialized value.  Invalid data on failure.</param>
			/// <param name="trailingX">True if there is a training X</param>
			/// <returns>True on success, false otherwise.</returns>
			private bool tryParseControlCodeFloatWithX(ReadOnlySpan<char> spanText, out float value, out bool trailingX)
			{
				trailingX = false;
				if (!this.tryGetControlCodeValue(spanText, out var content))
				{
					value = -1;
					return false;
				}

				// Sometimes there is an "x" at the end of the float value.  Remove it.
				// ReSharper disable once UseIndexFromEndExpression
				if (content[content.Length - 1] == 'x')
				{
					trailingX = true;
					content = content.Slice(0, content.Length - 1);
				}

#if NETFRAMEWORK
				// Fallback when the enum can't parse a span directly.
				if (float.TryParse(content.ToString(), out value))
					return true;
#else
				if (float.TryParse(content, out value))
					return true;
#endif
				return false;
			}

			/// <summary>
			/// Tries to parse a float from the control code value.
			/// </summary>
			/// <param name="spanText">Text to read from.</param>
			/// <param name="value">Deserialized value.  Invalid data on failure.</param>
			/// <returns>True on success, false otherwise.</returns>
			private bool tryParseControlCodeFloat(ReadOnlySpan<char> spanText, out float value)
			{
				if (!this.tryGetControlCodeValue(spanText, out var content))
				{
					value = -1;
					return false;
				}

#if NETFRAMEWORK
				// Fallback when the enum can't parse a span directly.
				if (float.TryParse(content.ToString(), out value))
					return true;
#else
				if (float.TryParse(content, out value))
					return true;
#endif
				return false;
			}

			/// <summary>
			/// Deserializes the paragraph codes and sets the current format with the values.
			/// </summary>
			/// <param name="spanText"></param>
			/// <returns></returns>
			private bool trySetParagraphCodes(ReadOnlySpan<char> spanText)
			{
				var startPosition = this._position + 1;

				if (!this.tryGetControlCodeValue(spanText, out var content) || this._currentFormat == null)
					return false;

				this._currentFormat.Paragraph.Clear();
				var startIndex = 0;

				for (int i = 0; i < content.Length; i++)
				{
					if (content[i] == ',')
					{
						// Ensure we are not at the end of the span.
						if (i + 1 > content.Length)
							return false;

						this._currentFormat.Paragraph.Add(this._content.Slice(startPosition + startIndex,
							i - startIndex));

						startIndex = i + 1;
					}
				}

				// Add the last part.
				if (startIndex != content.Length)
					this._currentFormat.Paragraph.Add(this._content.Slice(startIndex + startPosition,
						content.Length - startIndex));

				return true;
			}

			private bool trySetFontCodes(ReadOnlySpan<char> spanText)
			{
				var startPosition = this._position + 1;

				if (!this.tryGetControlCodeValue(spanText, out var content) || this._currentFormat == null)
					return false;

				var startIndex = 0;
				bool fontSet = false;
				char formatCode = '0';

				for (int i = 0; i <= content.Length; i++)
				{
					if (i == content.Length || content[i] == '|')
					{
						if (!fontSet)
						{
							this._currentFormat.Font.FontFamily =
								this._content.Slice(startPosition + startIndex, i - startIndex);
							fontSet = true;
						}
						else
						{
							ReadOnlySpan<char> slice;
							switch (formatCode)
							{
								case 'b':
									this._currentFormat.Font.IsBold = content[i - 1] == '1';
									break;

								case 'i':
									var val = content[i - 1];
									this._currentFormat.Font.IsItalic = val == '1';
									break;

								case 'c':
									slice = content.Slice(startIndex, i - startIndex);
#if NETFRAMEWORK
									if (!int.TryParse(slice.ToString(), out var codePage))
										return false;
#else
									if (!int.TryParse(slice, out var codePage))
										return false;
#endif

									this._currentFormat.Font.CodePage = codePage;
									break;

								case 'p':
									slice = content.Slice(startIndex, i - startIndex);
#if NETFRAMEWORK
									if (!int.TryParse(slice.ToString(), out var pitch))
										return false;
#else
									if (!int.TryParse(slice, out var pitch))
										return false;
#endif
									this._currentFormat.Font.Pitch = pitch;
									break;
							}
						}

						if (i == content.Length)
							return true;


						// Ensure we are not at the end of the span.
						if (i + 2 > content.Length)
							return false;

						formatCode = content[i + 1];
						startIndex = i + 2;
						i += 2;
					}
				}

				return true;
			}

			/// <summary>
			/// Tries to parse a control code value from the current position in the reader.
			/// </summary>
			/// <param name="spanText">Text to read from.</param>
			/// <param name="value">Span containing the data from the control code.</param>
			/// <returns>True on success, false otherwise.</returns>
			private bool tryGetControlCodeValue(ReadOnlySpan<char> spanText, out ReadOnlySpan<char> value)
			{
				// Consume the control letter.
				if (!this.tryAdvance())
				{
					value = default;
					return false;
				}

				var startPosition = this._position;

				do
				{
					if (spanText[this._position] == ';')
					{
						// Check to see if this was a control character and then immediately a semicolon.
						// If so, this is a malformed value.
						var length = this._position - startPosition;
						if (length == 0)
						{
							value = default;
							return false;
						}

						value = spanText.Slice(startPosition, length);
						return true;
					}
				} while (this.tryAdvance());

				value = default;
				return false;
			}

			/// <summary>
			/// Tries to parse a MText fraction.  Handles escapes decently.
			/// </summary>
			/// <param name="spanText">Text to read from.</param>
			/// <returns>True on success, false otherwise.</returns>
			private bool tryParseFraction(ReadOnlySpan<char> spanText)
			{
				this._mainBuffer.Clear();
				List<ReadOnlyMemory<char>> buffer = this._mainBuffer;

				bool numeratorSet = false;
				bool fractionEscaped = false;
				bool dividerSet = false;

				// Advance once.
				if (!this.tryAdvance())
					return false;

				var partStart = this._position;

				while (true)
				{
					var token = spanText[this._position];

					if (token == '\\')
					{
						if (fractionEscaped)
							buffer.Add(this._content.Slice(this._position, 1));

						fractionEscaped = !fractionEscaped;
					}
					else if (token == '^'
					         || token == '/'
					         || token == '#'
					         || token == ';')
					{
						// Check for an escape
						if (fractionEscaped)
						{
							// Skip the escape character.
							if (this._position - partStart - 1 > 0)
							{
								buffer.Add(this._content.Slice(partStart, this._position - partStart - 1));
								buffer.Add(this._content.Slice(this._position, 1));
								partStart = this._position + 1;

								if (!this.tryAdvance())
									return false;

								continue;
							}

							fractionEscaped = false;
						}
						else if (partStart != this._position)
						{
							buffer.Add(this._content.Slice(partStart, this._position - partStart));
						}

						if (!numeratorSet)
						{
							if (!dividerSet)
							{
								dividerSet = true;
								this._flushFractionValue.DividerType = token switch
								{
									'^' => TokenFraction.Divider.Stacked,
									'#' => TokenFraction.Divider.Condensed,
									'/' => TokenFraction.Divider.FractionBar,
									_ => TokenFraction.Divider.FractionBar
								};
							}

							this._flushFractionValue.Numerator = buffer;

							// If we are at the end and can't advance, then the fraction is broken.
							if (!this.canAdvance())
								return false;

							partStart = this._position + 1;

							// Switch over to the second buffer;
							buffer = this._secondBuffer;

						}
						else
						{
							this._flushFractionValue.Format = this._currentFormat;
							this._flushFractionValue.Denominator = buffer;
							this._visitor?.Invoke(this._flushFractionValue);
							this._mainBuffer.Clear();
							this._secondBuffer.Clear();
							return true;
						}

						numeratorSet = true;
					}

					if (!this.tryAdvance())
						return false;
				}
			}

			/// <summary>
			/// Pushes text to range to flush.
			/// </summary>
			private void pushTextEnd()
			{
				if (this._textValueStart == -1)
				{
					this._textValueStart = this._position;
					this._textValueEnd = this._position + 1;
					return;
				}

				// If there is a gap here, we need to add the range to the list.
				if (this._textValueEnd != this._position)
				{
					this._mainBuffer.Add(this._content.Slice(this._textValueStart, this._position - this._textValueStart - 1));
					this._textValueStart = this._position;
					this._textValueEnd = this._position + 1;
					return;
				}

				this._textValueEnd++;
			}


			/// <summary>
			/// Flushes the text to the visitor with the current formatting.
			/// </summary>
			private void flushText(ReadOnlyMemory<char>? append = null)
			{
				if (this._textValueEnd > this._textValueStart)
				{
					this._mainBuffer.Add(this._content.Slice(this._textValueStart, this._textValueEnd - this._textValueStart));
					this._textValueStart = -1;
					this._textValueEnd = -1;
				}

				if (append != null) this._mainBuffer.Add(append.Value);

				if (this._mainBuffer.Count > 0)
				{
					this._flushTokenValue.Format = this._currentFormat;
					this._flushTokenValue.Values = this._mainBuffer;
					this._visitor?.Invoke(this._flushTokenValue);
					this._mainBuffer.Clear();
				}
			}



			/// <summary>
			/// Tries to advance the position in the reader.
			/// </summary>
			/// <returns>True if the end has not been reached.  False otherwise.</returns>
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private bool tryAdvance()
			{
				return ++this._position < this._length;
			}

			/// <summary>
			/// Checks to see if it is possible to advance the position in the reader.
			/// </summary>
			/// <returns>True if the end has not been reached.  False otherwise.</returns>
			private bool canAdvance(int advanceAmount = 1)
			{
				return this._position + advanceAmount < this._length;
			}

			public void Dispose()
			{
				this._fontStateStack.Clear();
				this._mainBuffer.Clear();
				this._mainBuffer.TrimExcess();
				this._secondBuffer.Clear();
				this._secondBuffer.Capacity = 0;
			}
		}
	}
}
