#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
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
            private static readonly ReadOnlyMemory<char> _tokens = @"\\P\~\{\}\A0;\A1;\A2;\L\l\O\o\K\k\p\;,\S\#\^\/x;%%D%%P%%C{\f|b0|b1|i0|i1|c|p\T\Q\H{\c{\C0123456789.".AsMemory();
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

            private static readonly ReadOnlyMemory<char> _tokenFontStart = _tokens.Slice(57, 3);

            private static readonly ReadOnlyMemory<char> _tokenFontBold0 = _tokens.Slice(60, 3);
            private static readonly ReadOnlyMemory<char> _tokenFontBold1 = _tokens.Slice(63, 3);

            private static readonly ReadOnlyMemory<char> _tokenFontItalic0 = _tokens.Slice(66, 3);
            private static readonly ReadOnlyMemory<char> _tokenFontItalic1 = _tokens.Slice(69, 3);

            private static readonly ReadOnlyMemory<char> _tokenFontCodePageStart = _tokens.Slice(72, 2);
            private static readonly ReadOnlyMemory<char> _tokenFontPitchStart = _tokens.Slice(74, 2);

            private static readonly ReadOnlyMemory<char> _tokenTextTracking = _tokens.Slice(76, 2);
            private static readonly ReadOnlyMemory<char> _tokenTextObliquing = _tokens.Slice(78, 2);
            private static readonly ReadOnlyMemory<char> _tokenTextHeight = _tokens.Slice(80, 2);
            private static readonly ReadOnlyMemory<char> _tokenTextColorTrue = _tokens.Slice(82, 3);
            private static readonly ReadOnlyMemory<char> _tokenTextColorIndex = _tokens.Slice(85, 3);

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

            private int _lastConsumedPosition;
            private int _position;
            private ReadOnlyMemory<char> _values;
            private bool _closeFormat;
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

                SerializeWalker(Visitor, tokens);

                return stringBuilder;
            }

            /// <summary>
            /// Walks through the tokens while serializing.
            /// </summary>
            /// <param name="visitor">Visitor to walk through the output.</param>
            /// <param name="tokens">Tokens to serialize.</param>
            public void SerializeWalker(in Walker visitor, Token[] tokens)
            {
                _visitor = visitor;
                _lastConsumedPosition = 0;
                _position = 0;
                _currentFormat = _defaultFormat;

                foreach (var token in tokens)
                {
                    outputAnyFormatChanges(token.Format);

                    if (token is TokenValue tokenValue)
                    {
                        writeTokenValue(tokenValue);
                    }
                    else if (token is TokenFraction tokenFraction)
                    {
                        writeTokenFraction(tokenFraction);
                    }

                    if (_closeFormat)
                    {
                        _visitor!.Invoke(_tokenClosedBrace);
                        _currentFormat = _defaultFormat;
                        _closeFormat = false;
                    }
                }

                _visitor = null;
                _values = null;
            }

            /// <summary>
            /// Writes out the fraction token.
            /// </summary>
            /// <param name="tokenFraction">Token to write.</param>
            /// <exception cref="ArgumentOutOfRangeException"></exception>
            private void writeTokenFraction(TokenFraction tokenFraction)
            {
                _visitor!.Invoke(_tokenFraction);

                if (tokenFraction.Numerator != null)
                {
                    for (var i = 0; i < tokenFraction.Numerator.Count; i++)
                    {
                        _lastConsumedPosition = 0;
                        writeContents(tokenFraction.Numerator[i], true);
                    }
                }

                _visitor!.Invoke(tokenFraction.DividerType switch {
                    TokenFraction.Divider.Stacked => _tokenFractionStacked,
                    TokenFraction.Divider.FractionBar => _tokenFractionFractionBar,
                    TokenFraction.Divider.Condensed => _tokenFractionCondensed,
                    _ => throw new ArgumentOutOfRangeException()
                });

                if (tokenFraction.Denominator != null)
                {
                    for (var i = 0; i < tokenFraction.Denominator.Count; i++)
                    {
                        _lastConsumedPosition = 0;
                        writeContents(tokenFraction.Denominator[i], true);
                    }
                }

                _visitor!.Invoke(_tokenSemiColon);

            }

            private void writeTokenValue(TokenValue tokenValue)
            {
                if (tokenValue.Values == null)
                    return;

                for (int i = 0; i < tokenValue.Values.Count; i++)
                {
                    _lastConsumedPosition = 0;
                    writeContents(tokenValue.Values[i]);
                }
            }

            private void writeContents(ReadOnlyMemory<char> values, bool fractionEscapes = false)
            {
                var spanValues = values.Span;
                _values = values;

                for (_position = 0; _position < spanValues.Length; _position++)
                {
                    char token = spanValues[_position];
                    if (token == '\\')
                    {
                        appendCharacter(_tokenEscapeCharacter);
                    }
                    else if (token == '\n')
                    {
                        replaceCharacter(_tokenEscapedNewLine);
                    }
                    else if (token == '\u00A0') 
                    {
                        // Non Breaking Space
                        replaceCharacter(_tokenEscapedNbs);
                    }
                    else if (token == '°') 
                    {
                        replaceCharacter(_tokenDegrees);
                    }
                    else if (token == '±')
                    {
                        replaceCharacter(_tokenPlusMinus);
                    }  
                    else if (token == 'Ø')
                    {
                        replaceCharacter(_tokenDiameter);
                    }
                    else if (token == '{')
                    {
                        replaceCharacter(_tokenEscapedOpenBrace);
                    }
                    else if (token == '}')
                    {
                        replaceCharacter(_tokenEscapedClosedBrace);
                    }
                    else
                    {
                        if (fractionEscapes)
                        {
                            if (token == '#')
                            {
                                replaceCharacter(_tokenFractionEscapedCondensed);
                            }
                            else if (token == '/')
                            {
                                replaceCharacter(_tokenFractionEscapedFractionBar);
                            }
                            else if (token == '^')
                            {
                                replaceCharacter(_tokenFractionEscapedStacked);
                            }
                            else if (token == ';')
                            {
                                replaceCharacter(_tokenEscapedSemiColon);
                            }
                        }
                    }
                }

                if (_position != _lastConsumedPosition)
                {
                    // If the lastWritePosition is zero, this is a special case for when nothing has been transformed in the value.
                    // and we can just output the entire Memory<char>
                    _visitor!.Invoke(_lastConsumedPosition == 0
                        ? values
                        : _values.Slice(_lastConsumedPosition, _position - _lastConsumedPosition));
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void replaceCharacter(ReadOnlyMemory<char> replaceWith)
            {
                var writeLength = _position - _lastConsumedPosition;
                if (writeLength > 0 )
                    _visitor!.Invoke(_values.Slice(_lastConsumedPosition, writeLength));

                _visitor!.Invoke(replaceWith);
                _lastConsumedPosition = _position + 1;
            }

            private void appendCharacter(ReadOnlyMemory<char> appendCharacter)
            {
                var writeLength = _position - _lastConsumedPosition;
                if (writeLength > 0)
                    _visitor!.Invoke(_values.Slice(_lastConsumedPosition, writeLength));

                _visitor!.Invoke(appendCharacter);
                _lastConsumedPosition = _position;
            }

            [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
            private void outputAnyFormatChanges(Format? newFormat)
            {
                if (newFormat == null || _currentFormat == null)
                    throw new InvalidOperationException("Format can't be null");

                if (_currentFormat.Align != newFormat.Align)
                {
                    _visitor!.Invoke(newFormat.Align switch
                    {
                        Format.Alignment.Bottom => _tokenAlignmentBottom,
                        Format.Alignment.Center => _tokenAlignmentCenter,
                        Format.Alignment.Top => _tokenAlignmentTop,
                        null => _tokenAlignmentCenter,
                        _ => throw new ArgumentOutOfRangeException()
                    });
                }

                if (_currentFormat.IsUnderline != newFormat.IsUnderline)
                {
                    _visitor!.Invoke(newFormat.IsUnderline 
                        ? _tokenControlCodeL 
                        : _tokenControlCodeLOut);
                }

                if (_currentFormat.IsOverline != newFormat.IsOverline)
                {
                    _visitor!.Invoke(newFormat.IsOverline
                        ? _tokenControlCodeO
                        : _tokenControlCodeOOut);
                }

                if (_currentFormat.IsStrikeThrough != newFormat.IsStrikeThrough)
                {
                    _visitor!.Invoke(newFormat.IsStrikeThrough
                        ? _tokenControlCodeK
                        : _tokenControlCodeKOut);
                }

                if (_currentFormat.Tracking != newFormat.Tracking && newFormat.Tracking != null)
                {
                    _visitor!.Invoke(_tokenTextTracking);
                    outputFloatRoundedIfCloseToInteger(newFormat.Tracking.Value);
                    _visitor!.Invoke(_tokenSemiColon);
                }

                if (_currentFormat.Obliquing != newFormat.Obliquing && newFormat.Obliquing != null)
                {
                    _visitor!.Invoke(_tokenTextObliquing);
                    outputFloatRoundedIfCloseToInteger(newFormat.Obliquing.Value);
                    _visitor!.Invoke(_tokenSemiColon);
                }

                if (_currentFormat.Height != newFormat.Height && newFormat.Height != null)
                {
                    _visitor!.Invoke(_tokenTextHeight);
                    outputFloatRoundedIfCloseToInteger(newFormat.Height.Value);
                    _visitor!.Invoke(newFormat.IsHeightRelative ? _tokenRelativeHeightTrailer : _tokenSemiColon);
                }

                if (newFormat.Color != null)
                {
                    var value = newFormat.Color.Value;
                    if (value.IsTrueColor)
                    {
                        _visitor!.Invoke(_tokenTextColorTrue);
                        //_visitor!.Invoke(value.TrueColor.ToString().AsMemory());
                        outputUint((uint)value.TrueColor);
                    }
                    else
                    {
                        _visitor!.Invoke(_tokenTextColorIndex);
                        outputUint((uint)value.Index);
                        //_visitor!.Invoke(value.Index.ToString().AsMemory());
                    }

                    _visitor!.Invoke(_tokenSemiColon);
                    _closeFormat = true;
                }

                if (!_currentFormat.Font.Equals(newFormat.Font))
                {
                    _visitor!.Invoke(_tokenFontStart);
                    _visitor!.Invoke(newFormat.Font.FontFamily);
                    _visitor!.Invoke(newFormat.Font.IsBold ? _tokenFontBold1 : _tokenFontBold0);
                    _visitor!.Invoke(newFormat.Font.IsItalic ? _tokenFontItalic1 : _tokenFontItalic0);
                    _visitor!.Invoke(_tokenFontCodePageStart);
                    //_visitor!.Invoke(newFormat.Font.CodePage.ToString().AsMemory());
                    outputUint((uint)newFormat.Font.CodePage);
                    _visitor!.Invoke(_tokenFontPitchStart);
                    //_visitor!.Invoke(newFormat.Font.Pitch.ToString().AsMemory());
                    outputUint((uint)newFormat.Font.Pitch);
                    _visitor!.Invoke(_tokenSemiColon);

                    _closeFormat = true;
                }

                if (newFormat.AreParagraphsEqual(_currentFormat.Paragraph) == false)
                {
                    _visitor!.Invoke(_tokenControlParagraph);

                    var count = newFormat.Paragraph.Count;
                    for (int i = 0; i < count; i++)
                    {
                        _visitor!.Invoke(newFormat.Paragraph[i]);
                        if (i + 1 != count)
                        {
                            _visitor!.Invoke(_tokenComma);
                        }
                    }

                    _visitor!.Invoke(_tokenSemiColon);
                }

                _currentFormat = newFormat;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void outputFloatRoundedIfCloseToInteger(float value)
            {
                if (Math.Abs(value % 1) <= (float.Epsilon * 100))
                    outputUint((uint)value);
                else
                    outputFloat(value);
                //_visitor!.Invoke(value.ToString("###0.0#").AsMemory());
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void outputUint(uint value)
            {
                int bufferLength = countDigits(value);
                if (bufferLength == 1)
                {
                    outputNumber(value);
                    return;
                }

                Span<uint> outputArray = stackalloc uint[bufferLength];
                var position = bufferLength - 1;
                do
                {
                    (value, outputArray[position--]) = divRem(value, 10);
                } while (value != 0);

                for (int i = 0; i < bufferLength; i++)
                    outputNumber(outputArray[i]);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void outputNumber(uint value)
            {
                switch (value)
                {
                    case 0:
                        _visitor?.Invoke(_token0);
                        break;
                    case 1:
                        _visitor?.Invoke(_token1);
                        break;
                    case 2:
                        _visitor?.Invoke(_token2);
                        break;
                    case 3:
                        _visitor?.Invoke(_token3);
                        break;
                    case 4:
                        _visitor?.Invoke(_token4);
                        break;
                    case 5:
                        _visitor?.Invoke(_token5);
                        break;
                    case 6:
                        _visitor?.Invoke(_token6);
                        break;
                    case 7:
                        _visitor?.Invoke(_token7);
                        break;
                    case 8:
                        _visitor?.Invoke(_token8);
                        break;
                    case 9:
                        _visitor?.Invoke(_token9);
                        break;
                    case 10:
                        // Special case for the period.
                        _visitor?.Invoke(_tokenPeriod);
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
                    Debug.Assert(value < 100000);
                    digits += 4;
                }

                return digits;
            }

            public void outputFloat(float value)
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value), "Value must be positive.");

                // Handle the 0 case
                if (value == 0)
                {
                    _visitor!.Invoke(_token0);
                    return;
                }


                // Get the 7 meaningful digits as a long
                int nbDecimals = 0;
                while (value < 1000000)
                {
                    value *= 10;
                    nbDecimals++;
                }
#if NETFRAMEWORK
                long valueLong = (long)System.Math.Round(value);
#else
                long valueLong = (long)System.MathF.Round(value);
#endif
                // Parse the number in reverse order
                bool isLeadingZero = true;
                Span<uint> outputArray = stackalloc uint[nbDecimals];
                int outPosition = 0;
                while (valueLong != 0 || nbDecimals >= 0)
                {
                    // We stop removing leading 0 when non-0 or decimal digit
                    if (valueLong % 10 != 0 || nbDecimals <= 0)
                        isLeadingZero = false;

                    // Write the last digit (unless a leading zero)
                    if (!isLeadingZero)
                        outputArray[outPosition++] = (uint)(valueLong % 10);
                        //m_buffer[m_bufferPos + (nbChars++)] = (char)('0' + );

                    // Add the decimal point
                    if (--nbDecimals == 0 && !isLeadingZero)
                        outputArray[outPosition++] = 10;
                        //m_buffer[m_bufferPos + (nbChars++)] = '.';

                    valueLong /= 10;
                }

                for (int i = outPosition - 1; i >= 0; i--)
                    outputNumber(outputArray[i]);
            }
        }
    }
}
