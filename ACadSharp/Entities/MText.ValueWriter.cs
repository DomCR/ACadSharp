#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace ACadSharp.Entities
{
    public partial class MText
    {
        /// <summary>
        /// Writer which takes Tokens and serializes it.
        /// </summary>
        public class ValueWriter
        {
            private List<ReadOnlyMemory<char>> _outputList = null!;
            private readonly Format _defaultFormat = new Format();
            private Format? _currentFormat;

            // Tokens used to prevent memory allocation during serialization.
            //                                                      ⌄0         ⌄10       ⌄20       ⌄30       ⌄40       ⌄50               
            private static readonly ReadOnlyMemory<char> _tokens = @"\\P\~\{\}\A0;\A1;\A2;\L\l\O\o\K\k\p\;,\S\#\^\/x;%%D%%P%%C".AsMemory();
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

            private int _lastConsumedPosition;
            private int _position;
            private ReadOnlyMemory<char> _values;
            private bool _closeFormat;

            /// <summary>
            /// Serializes the tokens into a list of memory.
            /// </summary>
            /// <param name="tokens"></param>
            /// <returns></returns>
            public IReadOnlyList<ReadOnlyMemory<char>> Seralize(Token[] tokens)
            {
                _lastConsumedPosition = 0;
                _position = 0;
                _currentFormat = _defaultFormat;
                _outputList = new List<ReadOnlyMemory<char>>((int)(tokens.Length * 1.5));

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
                        _outputList.Add(_tokenClosedBrace);
                        _currentFormat = _defaultFormat;
                        _closeFormat = false;
                    }
                }

                _values = null;
                return _outputList;
            }

            /// <summary>
            /// Writes out the fraction token.
            /// </summary>
            /// <param name="tokenFraction">Token to write.</param>
            /// <exception cref="ArgumentOutOfRangeException"></exception>
            private void writeTokenFraction(TokenFraction tokenFraction)
            {
                _outputList.Add(_tokenFraction);

                if (tokenFraction.Numerator != null)
                {
                    for (var i = 0; i < tokenFraction.Numerator.Count; i++)
                    {
                        _lastConsumedPosition = 0;
                        writeContents(tokenFraction.Numerator[i], true);
                    }
                }

                _outputList.Add(tokenFraction.DividerType switch {
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

                _outputList.Add(_tokenSemiColon);

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
                    _outputList.Add(_lastConsumedPosition == 0
                        ? values
                        : _values.Slice(_lastConsumedPosition, _position - _lastConsumedPosition));
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void replaceCharacter(ReadOnlyMemory<char> replaceWith)
            {
                var writeLength = _position - _lastConsumedPosition;
                if (writeLength > 0 )
                    _outputList.Add(_values.Slice(_lastConsumedPosition, writeLength));

                _outputList.Add(replaceWith);
                _lastConsumedPosition = _position + 1;
            }

            private void appendCharacter(ReadOnlyMemory<char> appendCharacter)
            {
                var writeLength = _position - _lastConsumedPosition;
                if (writeLength > 0)
                    _outputList.Add(_values.Slice(_lastConsumedPosition, writeLength));

                _outputList.Add(appendCharacter);
                _lastConsumedPosition = _position;
            }

            [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
            private void outputAnyFormatChanges(Format? newFormat)
            {
                if (newFormat == null || _currentFormat == null)
                    throw new InvalidOperationException("Format can't be null");

                if (_currentFormat.Align != newFormat.Align)
                {
                    _outputList.Add(newFormat.Align switch
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
                    _outputList.Add(newFormat.IsUnderline 
                        ? _tokenControlCodeL 
                        : _tokenControlCodeLOut);
                }

                if (_currentFormat.IsOverline != newFormat.IsOverline)
                {
                    _outputList.Add(newFormat.IsOverline
                        ? _tokenControlCodeO
                        : _tokenControlCodeOOut);
                }

                if (_currentFormat.IsStrikeThrough != newFormat.IsStrikeThrough)
                {
                    _outputList.Add(newFormat.IsStrikeThrough
                        ? _tokenControlCodeK
                        : _tokenControlCodeKOut);
                }

                if (_currentFormat.Tracking != newFormat.Tracking && newFormat.Tracking != null)
                {
                    // determine if we can round to a whole number
                    _outputList.Add(Math.Abs(newFormat.Tracking.Value % 1) <= (float.Epsilon * 100)
                        ? $"\\T{(int)newFormat.Tracking};".AsMemory()
                        : $"\\T{newFormat.Tracking:###0.0#};".AsMemory());
                }

                if (_currentFormat.Obliquing != newFormat.Obliquing && newFormat.Obliquing != null)
                {
                    // determine if we can round to a whole number
                    _outputList.Add(Math.Abs(newFormat.Obliquing.Value % 1) <= (float.Epsilon * 100)
                        ? $"\\Q{(int)newFormat.Obliquing};".AsMemory()
                        : $"\\Q{newFormat.Obliquing:###0.0#};".AsMemory());
                }

                if (_currentFormat.Height != newFormat.Height && newFormat.Height != null)
                {
                    // determine if we can round to a whole number
                    _outputList.Add(Math.Abs(newFormat.Height.Value % 1) <= (float.Epsilon * 100)
                        ? $"\\H{(int)newFormat.Height}".AsMemory()
                        : $"\\H{newFormat.Height:###0.0#}".AsMemory());
                    _outputList.Add(newFormat.IsHeightRelative ? _tokenRelativeHeightTrailer : _tokenSemiColon);
                }

                if (newFormat.Color != null)
                {
                    var value = newFormat.Color.Value;
                    _outputList.Add(value.IsTrueColor
                        ? $"{{\\c{value.TrueColor};".AsMemory()
                        : $"{{\\C{value.Index};".AsMemory());

                    _closeFormat = true;
                }

                if (!_currentFormat.Font.Equals(newFormat.Font))
                {
                    _outputList.Add("{\\f".AsMemory());
                    _outputList.Add(newFormat.Font.FontFamily);
                    var bold = newFormat.Font.IsBold ? '1' : '0';
                    var italic = newFormat.Font.IsItalic ? '1' : '0';
                    _outputList.Add($"|b{bold}|i{italic}|c{newFormat.Font.CodePage}|p{newFormat.Font.Pitch};".AsMemory());

                    _closeFormat = true;
                }

                if (newFormat.AreParagraphsEqual(_currentFormat.Paragraph) == false)
                {
                    _outputList.Add(_tokenControlParagraph);

                    var count = newFormat.Paragraph.Count;
                    for (int i = 0; i < count; i++)
                    {
                        _outputList.Add(newFormat.Paragraph[i]);
                        if (i + 1 != count)
                        {
                            _outputList.Add(_tokenComma);
                        }
                    }

                    _outputList.Add(_tokenSemiColon);
                }

                _currentFormat = newFormat;
            }
        }
    }
}
