using ACadSharp.Attributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

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
        /// </seealso>
        public class ValueWriter
        {
            private List<ReadOnlyMemory<char>> _outputList = null!;
            private readonly Format _defaultFormat = new Format();
            private Format _currentFormat;
            private Stack<Format> _formatStack = new Stack<Format>();
            //                                                      ⌄0         ⌄10       ⌄20       ⌄30       ⌄40  
            private static readonly ReadOnlyMemory<char> _tokens = @"\\P\~\{\}\A0;\A1;\A2;\L\l\O\o\K\k\p\;,\S\#\^\/".AsMemory();
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
            private static readonly ReadOnlyMemory<char> _tokenEscapedSemiColon = _tokens.Slice(35, 1);
            private static readonly ReadOnlyMemory<char> _tokenSemiColon = _tokens.Slice(36, 1);
            private static readonly ReadOnlyMemory<char> _tokenComma = _tokens.Slice(37, 1);

            private static readonly ReadOnlyMemory<char> _tokenFraction = _tokens.Slice(38, 2);
            private static readonly ReadOnlyMemory<char> _tokenFractionCondensed = _tokens.Slice(41, 1);
            private static readonly ReadOnlyMemory<char> _tokenFractionStacked = _tokens.Slice(43, 1);
            private static readonly ReadOnlyMemory<char> _tokenFractionFractionBar = _tokens.Slice(45, 1);
            private static readonly ReadOnlyMemory<char> _tokenFractionEscapedCondensed = _tokens.Slice(40, 2);
            private static readonly ReadOnlyMemory<char> _tokenFractionEscapedStacked = _tokens.Slice(42, 2);
            private static readonly ReadOnlyMemory<char> _tokenFractionEscapedFractionBar = _tokens.Slice(44, 2);
            private int _lastConsumedPosition;
            private int _position;
            private ReadOnlyMemory<char> _values;
            private bool _closeFormat = false;


            public IReadOnlyList<ReadOnlyMemory<char>> Seralize(MText.Token[] tokens)
            {
                _currentFormat = _defaultFormat;
                _outputList = new List<ReadOnlyMemory<char>>((int)(tokens.Length * 1.5));

                foreach (var token in tokens)
                {
                    OutputAnyFormatChanges(token.Format);

                    if (token is MText.TokenValue tokenValue)
                    {
                        WriteTokenValue(tokenValue);
                    }
                    else if (token is MText.TokenFraction tokenFraction)
                    {
                        WriteTokenFraction(tokenFraction);
                    }

                    if (_closeFormat)
                    {
                        _outputList.Add(_tokenClosedBrace);
                        _currentFormat = _defaultFormat;
                        _closeFormat = false;
                    }
                }

                return _outputList;
            }

            private void WriteTokenFraction(TokenFraction tokenFraction)
            {
                _outputList.Add(_tokenFraction);

                for (int i = 0; i < tokenFraction.Numerator.Count; i++)
                {
                    _lastConsumedPosition = 0;
                    WriteContents(tokenFraction.Numerator[i], true);
                }

                _outputList.Add(tokenFraction.DividerType switch {
                    TokenFraction.Divider.Stacked => _tokenFractionStacked,
                    TokenFraction.Divider.FractionBar => _tokenFractionFractionBar,
                    TokenFraction.Divider.Condensed => _tokenFractionCondensed,
                    _ => throw new ArgumentOutOfRangeException()
                });

                for (int i = 0; i < tokenFraction.Denominator.Count; i++)
                {
                    _lastConsumedPosition = 0;
                    WriteContents(tokenFraction.Denominator[i], true);
                }
                _outputList.Add(_tokenSemiColon);

            }

            private void WriteTokenValue(TokenValue tokenValue)
            {
                for (int i = 0; i < tokenValue.Values.Count; i++)
                {
                    _lastConsumedPosition = 0;

                    WriteContents(tokenValue.Values[i]);
                }
            }

            private void WriteContents(ReadOnlyMemory<char> values, bool fractionEscapes = false)
            {
                var spanValues = values.Span;
                for (_position = 0; _position < spanValues.Length; _position++)
                {
                    if (spanValues[_position] == '\\')
                    {
                        AppendCharacter(_tokenEscapeCharacter);
                    }
                    else if (spanValues[_position] == '\n')
                    {
                        ReplaceCharacter(_tokenEscapedNewLine);
                    }
                    else if (spanValues[_position] == '\u00A0')
                    {
                        ReplaceCharacter(_tokenEscapedNbs);
                    }
                    else if (spanValues[_position] == '{')
                    {
                        ReplaceCharacter(_tokenEscapedOpenBrace);
                    }
                    else if (spanValues[_position] == '}')
                    {
                        ReplaceCharacter(_tokenEscapedClosedBrace);
                    }
                    else
                    {
                        if (fractionEscapes)
                        {
                            if (spanValues[_position] == '#')
                            {
                                ReplaceCharacter(_tokenFractionEscapedCondensed);
                            }
                            else if (spanValues[_position] == '/')
                            {
                                ReplaceCharacter(_tokenFractionEscapedFractionBar);
                            }
                            else if (spanValues[_position] == '^')
                            {
                                ReplaceCharacter(_tokenFractionEscapedStacked);
                            }
                            else if (spanValues[_position] == ';')
                            {
                                ReplaceCharacter(_tokenEscapedSemiColon);
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
            private void ReplaceCharacter(ReadOnlyMemory<char> replaceWith)
            {
                var writeLength = _position - _lastConsumedPosition;
                if (writeLength > 0 )
                    _outputList.Add(_values.Slice(_lastConsumedPosition + 1, writeLength));

                _outputList.Add(replaceWith);
                _lastConsumedPosition = _position + 1;
            }

            private void AppendCharacter(ReadOnlyMemory<char> appendCharacter)
            {
                var writeLength = _position - _lastConsumedPosition;
                if (writeLength > 0)
                    _outputList.Add(_values.Slice(_lastConsumedPosition, writeLength));

                _outputList.Add(appendCharacter);
                _lastConsumedPosition = _position;
            }

            private void OutputAnyFormatChanges(Format newFormat)
            {
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
                    _outputList.Add(newFormat?.IsUnderline == true 
                        ? _tokenControlCodeL 
                        : _tokenControlCodeLOut);
                }

                if (_currentFormat.IsOverline != newFormat.IsOverline)
                {
                    _outputList.Add(newFormat?.IsOverline == true
                        ? _tokenControlCodeO
                        : _tokenControlCodeOOut);
                }

                if (_currentFormat.IsStrikeThrough != newFormat.IsStrikeThrough)
                {
                    _outputList.Add(newFormat?.IsStrikeThrough == true
                        ? _tokenControlCodeK
                        : _tokenControlCodeKOut);
                }

                if (_currentFormat.Tracking != newFormat.Tracking && newFormat.Tracking != null)
                {
                    // determine if we can round to a whole number
                    if (Math.Abs(newFormat.Tracking.Value % 1) <= (float.Epsilon * 100))
                    {
                        _outputList.Add($"\\T{(int)newFormat.Tracking};".AsMemory());
                    }
                    else
                    {
                        _outputList.Add($"\\T{newFormat.Tracking:###0.0#};".AsMemory());
                    }
                }

                if (_currentFormat.Obliquing != newFormat.Obliquing && newFormat.Obliquing != null)
                {
                    // determine if we can round to a whole number
                    if (Math.Abs(newFormat.Obliquing.Value % 1) <= (float.Epsilon * 100))
                    {
                        _outputList.Add($"\\Q{(int)newFormat.Obliquing};".AsMemory());
                    }
                    else
                    {
                        _outputList.Add($"\\Q{newFormat.Obliquing:###0.0#};".AsMemory());
                    }
                }

                if (_currentFormat.Height != newFormat.Height && newFormat.Height != null)
                {
                    // determine if we can round to a whole number
                    if (Math.Abs(newFormat.Height.Value % 1) <= (float.Epsilon * 100))
                    {
                        _outputList.Add($"\\H{(int)newFormat.Height}x;".AsMemory());
                    }
                    else
                    {
                        _outputList.Add($"\\H{newFormat.Height:###0.0#}x;".AsMemory());
                    }
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
                    _outputList.Add($"{{\\f".AsMemory());
                    _outputList.Add(newFormat.Font.FontFamily);
                    var bold = newFormat.Font.IsBold ? '1' : '0';
                    var italic = newFormat.Font.IsItalic ? '1' : '0';
                    _outputList.Add($"|b{bold}|i{italic}|c{newFormat.Font.CodePage}|p{newFormat.Font.Pitch};".AsMemory());

                    _closeFormat = true;
                }

                if (newFormat.Paragraph != null && newFormat.AreParagraphsEqual(_currentFormat.Paragraph) == false)
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
