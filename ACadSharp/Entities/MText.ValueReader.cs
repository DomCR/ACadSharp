#nullable enable
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ACadSharp.Entities
{
    public partial class MText
    {
        /// <summary>
        /// Zero copy reader to parse AutoCAD M-Text value entries and return tokens.
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
            /// Deserializes the passed contest value.
            /// </summary>
            /// <param name="content">Content to parse.</param>
            /// <returns>Parsed token list.  This is not a zero copy parsing process.</returns>
            /// <remarks>Not thread safe.</remarks>
            public Token[] Deserialize(string content)
            {
                return Deserialize(content, null);
            }

            public Token[] Deserialize(string content, Format? baseFormat)
            {
                return Deserialize(content.AsMemory(), baseFormat);
            }

            /// <summary>
            /// Deserializes the passed contest value.
            /// </summary>
            /// <param name="content">Content to parse.</param>
            /// <param name="baseFormat">This is the base format that will be used.</param>
            /// <returns>Parsed token list.  This is not a zero copy parsing process.</returns>
            /// <remarks>Not thread safe.</remarks>
            public Token[] Deserialize(ReadOnlyMemory<char> content, Format? baseFormat)
            {
                var list = new List<Token>();
                DeserializeWalker(token =>
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
                _currentFormat?.Reset();
                return DeserializeWalker(visitor, content, _currentFormat);
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
                _content = content;
                _visitor = visitor;
                _textValueStart = -1;
                _textValueEnd = -1;
                _length = _content.Length;
                _controlCode = false;
                _position = 0;
                _currentFormat = baseFormat;
                setNewCurrentFormat();

                if (_currentFormat == null)
                    throw new InvalidOperationException("Format is corrupted.");

                var spanText = _content.Span;
                var charBufferSpan = _charBuffer.Span;
                while (true)
                {
                    var token = spanText[_position];
                    if (token == '%' 
                        && canAdvance(2) 
                        && spanText[_position + 1] == '%')
                    {
                        _controlCode = false;
                        token = spanText[_position + 2];
                        if (token == 'D' || token == 'd')
                        {
                            charBufferSpan[0] = '°';
                            flushText(_charBuffer);
                        }   
                        else if (token == 'P' || token == 'p')
                        {
                            // ± (PLUS-MINUS SIGN)
                            charBufferSpan[0] = '\u00B1';
                            flushText(_charBuffer);
                        } 
                        else if (token == 'C' || token == 'c')
                        {
                            // Ø (LATIN CAPITAL LETTER O WITH STROKE)
                            charBufferSpan[0] = '\u00D8';
                            flushText(_charBuffer);
                        }
                        else if (token == '%')
                        {
                            charBufferSpan[0] = '%';
                            flushText(_charBuffer);
                        }
                        else
                        {
                            pushTextEnd();
                            continue;
                        }

                        if (canAdvance(3))
                        {
                            _position += 3;
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
                        if (_controlCode)
                            _mainBuffer.Add(_content.Slice(_position, 1));

                        _controlCode = !_controlCode;
                    }
                    else if (_controlCode && token == 'A')
                    {
                        _controlCode = false;
                        flushText();
                        if (!tryParseEnum<Format.Alignment>(spanText, out var value))
                            return false;

                        _currentFormat.Align = value;
                    }
                    else if (_controlCode && token == 'C')
                    {
                        _controlCode = false;
                        flushText();
                        if (!tryParseControlCodeInt(spanText, out var value))
                            return false;

                        _currentFormat.Color = new Color((short)value);
                    }
                    else if (_controlCode && token == 'c')
                    {
                        _controlCode = false;
                        flushText();
                        if (!tryParseControlCodeInt(spanText, out var value))
                            return false;

                        _currentFormat.Color = Color.FromTrueColor(value);
                    }
                    else if (_controlCode && token == 'T')
                    {
                        _controlCode = false;
                        flushText();
                        if (!tryParseControlCodeFloat(spanText, out var value))
                            return false;

                        _currentFormat.Tracking = value;
                    }
                    else if (_controlCode && token == 'W')
                    {
                        _controlCode = false;
                        flushText();
                        if (!tryParseControlCodeFloat(spanText, out var value))
                            return false;

                        _currentFormat.Width = value;
                    }
                    else if (_controlCode && token == 'H')
                    {
                        _controlCode = false;
                        flushText();
                        if (!tryParseControlCodeFloatWithX(spanText, out var value, out bool relative))
                            return false;
                        _currentFormat.IsHeightRelative = relative;

                        if (relative)
                        {
                            _currentFormat.Height *= value;
                        }
                        else
                        {
                            _currentFormat.Height = value;
                        }
                    }
                    else if (_controlCode && token == 'Q')
                    {
                        _controlCode = false;
                        flushText();
                        if (!tryParseControlCodeFloat(spanText, out var value))
                            return false;

                        _currentFormat.Obliquing = value;
                    }
                    else if (_controlCode && token == 'p')
                    {
                        _controlCode = false;
                        flushText();
                        if (!trySetParagraphCodes(spanText))
                            return false;
                    }
                    else if (_controlCode && token == 'f')
                    {
                        _controlCode = false;
                        flushText();
                        if (!trySetFontCodes(spanText))
                            return false;
                    }
                    else if (_controlCode && token == 'S')
                    {
                        _controlCode = false;
                        flushText();
                        if (!tryParseFraction(spanText))
                            return false;
                    }
                    else if (_controlCode && token == 'L')
                    {
                        _controlCode = false;
                        flushText();
                        _currentFormat.IsUnderline = true;
                    }
                    else if (_controlCode && token == 'l')
                    {
                        _controlCode = false;
                        flushText();
                        _currentFormat.IsUnderline = false;
                    }
                    else if (_controlCode && token == 'O')
                    {
                        _controlCode = false;
                        flushText();
                        _currentFormat.IsOverline = true;
                    }
                    else if (_controlCode && token == 'o')
                    {
                        _controlCode = false;
                        flushText();
                        _currentFormat.IsOverline = false;
                    }
                    else if (_controlCode && token == 'K')
                    {
                        _controlCode = false;
                        flushText();
                        _currentFormat.IsStrikeThrough = true;
                    }
                    else if (_controlCode && token == 'k')
                    {
                        _controlCode = false;
                        flushText();
                        _currentFormat.IsStrikeThrough = false;
                    }
                    else if (_controlCode && token == 'P')
                    {
                        _controlCode = false;
                        charBufferSpan[0] = '\n';
                        flushText(_charBuffer);
                    }  
                    else if (_controlCode && token == '~')
                    {
                        // Non Breaking Space
                        _controlCode = false;
                        charBufferSpan[0] = '\u00A0';
                        flushText(_charBuffer);
                    }
                    else if (!_controlCode && token == '{')
                    {
                        flushText();
                        _fontStateStack.Push(_currentFormat);
                        setNewCurrentFormat();

                    }
                    else if (!_controlCode && token == '}')
                    {
                        flushText();
                        _currentFormat.Reset();
                        _freeFormatStates.Push(_currentFormat);
                        _currentFormat = _fontStateStack.Pop();
                    }
                    else
                    {
                        _controlCode = false;
                        pushTextEnd();
                    }

                    // See if we are at the end of the string.
                    if (!tryAdvance())
                    {
                        if (_controlCode)
                        {
                            charBufferSpan[0] = '\\';
                            _mainBuffer.Add(_charBuffer);
                        }

                        flushText();

                        // Cleanup
                        _currentFormat.Reset();
                        _freeFormatStates.Push(_currentFormat);

                        // Should not do anything
                        _fontStateStack.Clear();
                        _visitor = null!;
                        _controlCode = false;
                        _content = null!;
                        _mainBuffer.Clear();
                        _secondBuffer.Clear();
                        return true;
                    }

                }
            }

            private void setNewCurrentFormat()
            {
                // ReSharper disable once InlineOutVariableDeclaration
                Format? newFormat;
#if NETFRAMEWORK
                if (_freeFormatStates.Count > 0)
                {
                    newFormat = _freeFormatStates.Pop();
                }
                else
#else
                if (!_freeFormatStates.TryPop(out newFormat))
#endif
                {
                    newFormat = new Format();
                }
                // Copy the formatting from the current format to the new format.
                if (_currentFormat != null)
                {
                    newFormat.OverrideFrom(_currentFormat);
                }

                _currentFormat = newFormat;
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
                if (!tryGetControlCodeValue(spanText, out var content))
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
                if (Enum.TryParse<TEnum>(content.ToString(), out value))
                    return true;
#endif
                value = default;
                return false;
            }

            /// <summary>
            /// Tries to parse an int from the control code value.
            /// </summary>
            /// <param name="spanText">Text to read from.</param>
            /// <param name="value">Parsed value.  Invalid data on failure.</param>
            /// <returns>True on success, false otherwise.</returns>
            private bool tryParseControlCodeInt(ReadOnlySpan<char> spanText, out int value)
            {
                if (!tryGetControlCodeValue(spanText, out var content))
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
            /// <param name="value">Parsed value.  Invalid data on failure.</param>
            /// <param name="trailingX">True if there is a training X</param>
            /// <returns>True on success, false otherwise.</returns>
            private bool tryParseControlCodeFloatWithX(ReadOnlySpan<char> spanText, out float value, out bool trailingX)
            {
                trailingX = false;
                if (!tryGetControlCodeValue(spanText, out var content))
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
            /// <param name="value">Parsed value.  Invalid data on failure.</param>
            /// <returns>True on success, false otherwise.</returns>
            private bool tryParseControlCodeFloat(ReadOnlySpan<char> spanText, out float value)
            {
                if (!tryGetControlCodeValue(spanText, out var content))
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
                var startPosition = _position + 1;

                if (!tryGetControlCodeValue(spanText, out var content) || _currentFormat == null)
                    return false;

                _currentFormat.Paragraph.Clear();
                var startIndex = 0;

                for (int i = 0; i < content.Length; i++)
                {
                    if (content[i] == ',')
                    {
                        // Ensure we are not at the end of the span.
                        if (i + 1 > content.Length)
                            return false;

                        _currentFormat.Paragraph.Add(_content.Slice(startPosition + startIndex, i - startIndex));

                        startIndex = i + 1;
                    }
                }

                // Add the last part.
                if (startIndex != content.Length)
                    _currentFormat.Paragraph.Add(_content.Slice(startIndex + startPosition, content.Length - startIndex));

                return true;
            }

            private bool trySetFontCodes(ReadOnlySpan<char> spanText)
            {
                var startPosition = _position + 1;

                if (!tryGetControlCodeValue(spanText, out var content) || _currentFormat == null)
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
                            _currentFormat.Font.FontFamily = _content.Slice(startPosition + startIndex, i - startIndex);
                            fontSet = true;
                        }
                        else
                        {
                            ReadOnlySpan<char> slice;
                            switch (formatCode)
                            {
                                case 'b':
                                    _currentFormat.Font.IsBold = content[i - 1] == '1';
                                    break;

                                case 'i':
                                    var val = content[i - 1];
                                    _currentFormat.Font.IsItalic = val == '1';
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

                                    _currentFormat.Font.CodePage = codePage;
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
                                    _currentFormat.Font.Pitch = pitch;
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
                if (!tryAdvance())
                {
                    value = default;
                    return false;
                }
                var startPosition = _position;

                do
                {
                    if (spanText[_position] == ';')
                    {
                        // Check to see if this was a control character and then immediately a semicolon.
                        // If so, this is a malformed value.
                        var length = _position - startPosition;
                        if (length == 0)
                        {
                            value = default;
                            return false;
                        }

                        value = spanText.Slice(startPosition, length);
                        return true;
                    }
                } while (tryAdvance());

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
                _mainBuffer.Clear();
                List<ReadOnlyMemory<char>> buffer = _mainBuffer;
                
                bool numeratorSet = false;
                bool fractionEscaped = false;
                bool dividerSet = false;

                // Advance once.
                if (!tryAdvance())
                    return false;

                var partStart = _position;

                while (true)
                {
                    var token = spanText[_position];

                    if (token == '\\')
                    {
                        if (fractionEscaped)
                            buffer.Add(_content.Slice(_position, 1));

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
                            if (_position - partStart - 1 > 0)
                            {
                                buffer.Add(_content.Slice(partStart, _position - partStart - 1));
                                buffer.Add(_content.Slice(_position, 1));
                                partStart = _position + 1;

                                if (!tryAdvance())
                                    return false;

                                continue;
                            }

                            fractionEscaped = false;
                        }
                        else if (partStart != _position)
                        {
                            buffer.Add(_content.Slice(partStart, _position - partStart));
                        }

                        if (!numeratorSet)
                        {
                            if (!dividerSet)
                            {
                                dividerSet = true;
                                _flushFractionValue.DividerType = token switch
                                {
                                    '^' => TokenFraction.Divider.Stacked,
                                    '#' => TokenFraction.Divider.Condensed,
                                    '/' => TokenFraction.Divider.FractionBar,
                                    _ => TokenFraction.Divider.FractionBar
                                };
                            }

                            _flushFractionValue.Numerator = buffer;

                            // If we are at the end and can't advance, then the fraction is broken.
                            if (!canAdvance())
                                return false;

                            partStart = _position + 1;

                            // Switch over to the second buffer;
                            buffer = _secondBuffer;

                        }
                        else
                        {
                            _flushFractionValue.Format = _currentFormat;
                            _flushFractionValue.Denominator = buffer;
                            _visitor?.Invoke(_flushFractionValue);
                            _mainBuffer.Clear();
                            _secondBuffer.Clear();
                            return true;
                        }

                        numeratorSet = true;
                    }

                    if (!tryAdvance())
                        return false;
                }
            }

            /// <summary>
            /// Pushes text to range to flush.
            /// </summary>
            private void pushTextEnd()
            {
                if (_textValueStart == -1)
                {
                    _textValueStart = _position;
                    _textValueEnd = _position + 1;
                    return;
                }

                // If there is a gap here, we need to add the range to the list.
                if (_textValueEnd != _position)
                {
                    _mainBuffer.Add(_content.Slice(_textValueStart, _position - _textValueStart - 1));
                    _textValueStart = _position;
                    _textValueEnd = _position + 1;
                    return;
                }

                _textValueEnd++;
            }


            /// <summary>
            /// Flushes the text to the visitor with the current formatting.
            /// </summary>
            private void flushText(ReadOnlyMemory<char>? append = null)
            {
                if (_textValueEnd > _textValueStart)
                {
                    _mainBuffer.Add(_content.Slice(_textValueStart, _textValueEnd - _textValueStart));
                    _textValueStart = -1;
                    _textValueEnd = -1;
                }

                if(append != null)
                    _mainBuffer.Add(append.Value);

                if (_mainBuffer.Count > 0)
                {
                    _flushTokenValue.Format = _currentFormat;
                    _flushTokenValue.Values = _mainBuffer;
                    _visitor?.Invoke(_flushTokenValue);
                    _mainBuffer.Clear();
                }
            }

            

            /// <summary>
            /// Tries to advance the position in the reader.
            /// </summary>
            /// <returns>True if the end has not been reached.  False otherwise.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private bool tryAdvance()
            {
                return ++_position < _length;
            }

            /// <summary>
            /// Checks to see if it is possible to advance the position in the reader.
            /// </summary>
            /// <returns>True if the end has not been reached.  False otherwise.</returns>
            private bool canAdvance(int advanceAmount = 1)
            {
                return _position + advanceAmount < _length;
            }

            public void Dispose()
            {
                _fontStateStack.Clear();
                _mainBuffer.Clear();
                _mainBuffer.TrimExcess();
                _secondBuffer.Clear();
                _secondBuffer.Capacity = 0;
            }
        }
    }
}
