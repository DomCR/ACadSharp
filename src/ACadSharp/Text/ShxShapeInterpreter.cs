#if NET5_0_OR_GREATER
using System;

namespace ACadSharp.Text;

/// <summary>
/// AutoCAD SHX shape-program interpreter. Walks the byte program of a glyph record and emits path commands to <see cref="IGlyphSink"/> —
/// in the same family as <see cref="Type1CharstringInterpreter"/> and <see cref="Type2CharstringInterpreter"/>,
/// so it sits alongside them rather than inside some container class.
///
/// <para>Unlike the other two, the result is an <b>open pen path</b>, not a closed outline: <see cref="IGlyphSink.Close"/> is never called.
/// See <see cref="ShxFont.IsStroked"/>.</para>
///
/// <para>Per-call instance state (pen, scaling, position stack) lives only on the call stack and never escapes, so concurrent calls are safe.</para>
/// </summary>
internal sealed class ShxShapeInterpreter
{
    /// <summary>
    /// The documented position-stack depth is 4. Allowing 16 costs nothing and the corpus is not adversarial: it contains 12,660 pushes against 12,770
    /// pops, so some fonts pop an empty stack. Overflow and underflow are both silently ignored rather than thrown.
    /// </summary>
    private const int PositionStackDepth = 16;

    /// <summary>Guards against self-referencing subshapes (directly or through a cycle).</summary>
    private const int MaxSubshapeDepth = 8;

    private const double Octant = Math.PI / 4;

    // The 16 packed-vector directions. They are not unit vectors: 1.0 on the major axis, 0.5 on the minor axis — exactly this definition is what
    // puts SHX diagonals on the pixel grid and keeps them sharp at small sizes. "Correctly" normalizing them would visibly skew every diagonal.
    private static readonly float[] DirX =
        [1f, 1f, 1f, 0.5f, 0f, -0.5f, -1f, -1f, -1f, -1f, -1f, -0.5f, 0f, 0.5f, 1f, 1f];
    private static readonly float[] DirY =
        [0f, 0.5f, 1f, 1f, 1f, 1f, 1f, 0.5f, 0f, -0.5f, -1f, -1f, -1f, -1f, -1f, -0.5f];

    private readonly ShxFont _font;
    private readonly IGlyphSink? _sink;
    private readonly ShxTextOrientation _orientation;

    private float _x;
    private float _y;

    // Independent X, Y scaling. 0x03/0x04 always move both together, but the width×height
    // box a bigfont composed subshape is placed into is often not square.
    private float _scaleX = 1f;
    private float _scaleY = 1f;

    // The pen starts down in SHX. Two independent corpus signals agree: 4,869 glyph records execute a
    // drawing operation before any pen-up command (with a default pen-up they would silently lose their first stroke), and across 44,332 records
    // pen-up occurs 50,202 times more often than pen-down —
    // roughly one unmatched pen-up per glyph, exactly the result of "default down + a closing pen-up".
    private bool _penDown = true;

    private bool _contourOpen;

    private readonly (float X, float Y)[] _stack = new (float X, float Y)[PositionStackDepth];
    private int _sp;
    private int _depth;

    private ShxShapeInterpreter(ShxFont font, IGlyphSink? sink, ShxTextOrientation orientation)
    {
        _font = font;
        _sink = sink;
        _orientation = orientation;
    }

    /// <summary>
    /// Runs one glyph record. Returns the step width: the pen X coordinate at figure end, the only advance this format declares.
    /// </summary>
    /// <param name="sink">May be null, to compute the step without emitting geometry.</param>
    public static float Execute(ShxFont font, ReadOnlySpan<byte> record, IGlyphSink? sink,
        ShxTextOrientation orientation)
    {
        var it = new ShxShapeInterpreter(font, sink, orientation);
        it.RunRecord(record);
        return it._x;
    }

    /// <summary>
    /// Skips the record's glyph name, then runs its instructions.
    ///
    /// <para>Every record — glyph and font-definition record alike — starts with a NUL-terminated name, usually empty (a bare <c>0x00</c>).
    /// Walking the instructions from byte 0 would read it as figure end, so every character would return an empty glyph, and it looks nothing like failure:
    /// the font loads cleanly, it just draws nothing.</para>
    /// </summary>
    private void RunRecord(ReadOnlySpan<byte> record)
    {
        var nul = record.IndexOf((byte)0);
        if (nul < 0) return;
        Run(record[(nul + 1)..]);
    }

    private void Run(ReadOnlySpan<byte> data)
    {
        var i = 0;
        while (i < data.Length)
        {
            var op = data[i++];

            // A non-zero high nibble means a packed vector, not a command.
            if (op >= 0x10)
            {
                Step(DirX[op & 0x0F] * (op >> 4), DirY[op & 0x0F] * (op >> 4));
                continue;
            }

            switch (op)
            {
                case 0x00:                                  // end of shape
                    return;

                case 0x01:                                  // pen down
                    _penDown = true;
                    break;

                case 0x02:                                  // pen up
                    _penDown = false;
                    _contourOpen = false;                    // end the subpath; never close it
                    break;

                case 0x03:                                  // divide vector lengths
                    if (i >= data.Length) return;
                    var divisor = data[i++];
                    if (divisor != 0) { _scaleX /= divisor; _scaleY /= divisor; }
                    break;

                case 0x04:                                  // multiply vector lengths
                    if (i >= data.Length) return;
                    var multiplier = data[i++];
                    if (multiplier != 0) { _scaleX *= multiplier; _scaleY *= multiplier; }
                    break;

                case 0x05:                                  // push position
                    if (_sp < PositionStackDepth) _stack[_sp++] = (_x, _y);
                    break;

                case 0x06:                                  // pop position
                    if (_sp > 0)
                    {
                        (_x, _y) = _stack[--_sp];
                        _contourOpen = false;                // a jump breaks the subpath
                    }
                    break;

                case 0x07:                                  // subshape reference
                    Subshape(data, ref i);
                    break;

                case 0x08:                                  // signed XY displacement
                    if (i + 1 >= data.Length) return;
                    Step((sbyte)data[i], (sbyte)data[i + 1]);
                    i += 2;
                    break;

                case 0x09:                                  // displacement sequence, terminated by (0,0)
                    while (true)
                    {
                        if (i + 1 >= data.Length) return;
                        var dx = (sbyte)data[i];
                        var dy = (sbyte)data[i + 1];
                        i += 2;
                        if (dx == 0 && dy == 0) break;
                        Step(dx, dy);
                    }
                    break;

                case 0x0A:                                  // octant arc
                    if (i + 1 >= data.Length) return;
                    OctantArc(data[i], data[i + 1]);
                    i += 2;
                    break;

                case 0x0B:                                  // fractional arc
                    if (i + 4 >= data.Length) return;
                    FractionalArc(data[i], data[i + 1], data[i + 2], data[i + 3], data[i + 4]);
                    i += 5;
                    break;

                case 0x0C:                                  // bulge arc
                    if (i + 2 >= data.Length) return;
                    BulgeArc((sbyte)data[i], (sbyte)data[i + 1], (sbyte)data[i + 2]);
                    i += 3;
                    break;

                case 0x0D:                                  // bulge-arc sequence
                    while (true)
                    {
                        if (i + 1 >= data.Length) return;
                        var dx = (sbyte)data[i];
                        var dy = (sbyte)data[i + 1];
                        i += 2;
                        // Terminated by a (0,0) displacement that carries no bulge byte.
                        if (dx == 0 && dy == 0) break;
                        if (i >= data.Length) return;
                        BulgeArc(dx, dy, (sbyte)data[i]);
                        i++;
                    }
                    break;

                case 0x0E:
                    // A SKIP, not a no-op: in horizontal text 0x0E suppresses the <b>following</b> command.
                    // In vertical text that command runs as usual, so 0x0E itself is the no-op. This is exactly how SHX carries vertical glyphs —
                    // inside the same glyph rather than in a separate one, and it is common: 77,397 occurrences in the corpus.
                    if (_orientation == ShxTextOrientation.Horizontal) SkipCommand(data, ref i);
                    break;

                default:
                    // 0x0F: a high nibble of 0 means length 0, so it draws nothing either way.
                    break;
            }
        }
    }

    /// <summary>
    /// Advances <paramref name="i"/> past exactly one command without executing it, used by <c>0x0E</c> in horizontal text.
    /// </summary>
    private void SkipCommand(ReadOnlySpan<byte> data, ref int i)
    {
        if (i >= data.Length) return;
        var op = data[i];
        if (op == 0x00) return;                              // leave end-of-shape to Run()
        i++;
        if (op >= 0x10) return;                              // packed vector: no operands

        switch (op)
        {
            case 0x03:
            case 0x04:
                i += 1;
                break;
            case 0x07:
                SkipSubshapeOperands(data, ref i);
                break;
            case 0x08:
            case 0x0A:
                i += 2;
                break;
            case 0x0C:
                i += 3;
                break;
            case 0x0B:
                i += 5;
                break;
            case 0x09:
            case 0x0D:
                while (i + 1 < data.Length)
                {
                    var dx = data[i];
                    var dy = data[i + 1];
                    i += 2;
                    if (dx == 0 && dy == 0) break;
                    if (op == 0x0D) i++;
                }
                break;
            default:
                break;                                       // 0x01/0x02/0x05/0x06/0x0E have no operands
        }
        if (i > data.Length) i = data.Length;
    }

    private void SkipSubshapeOperands(ReadOnlySpan<byte> data, ref int i)
    {
        if (IsTwoByteSubshape) { i += 2; return; }
        i += i < data.Length && data[i] == 0 ? 7 : 1;
    }

    /// <summary>
    /// Both unifont and shapes take a 2-byte subshape reference (high byte first); only bigfont uses a 1-byte code
    /// together with its <c>0x00</c> composition escape.
    /// </summary>
    private bool IsTwoByteSubshape
        => _font.Format == ShxFormat.Unifont || _font.Format == ShxFormat.Shapes;

    /// <summary>Moves the pen by a displacement, drawing if the pen is down.</summary>
    private void Step(float dx, float dy)
    {
        var nx = _x + dx * _scaleX;
        var ny = _y + dy * _scaleY;
        if (_penDown)
        {
            EnsureContour();
            _sink?.LineTo(nx, ny);
        }
        _x = nx;
        _y = ny;
    }

    /// <summary>
    /// Opens a subpath at the current point on the first drawing operation after a pen-down. Lazy so that "pen-down immediately
    /// followed by a jump" does not emit a stray <c>MoveTo</c>.
    /// </summary>
    private void EnsureContour()
    {
        if (_contourOpen) return;
        _sink?.MoveTo(_x, _y);
        _contourOpen = true;
    }

    /// <summary>
    /// <c>0x0A radius ±0SC</c>: the high nibble of the second operand is the start octant, the low nibble the octant count, bit 7 sets clockwise.
    /// Octants are 45 degrees each, numbered 0 counter-clockwise from the 3 o'clock direction.
    ///
    /// <para>Skipping arcs is the mistake most likely to produce misleading symptoms: it drops exactly the round glyphs and keeps everything else perfectly intact,
    /// leaving a perfectly bimodal per-character result — <c>T Y + 7 :</c> 100% correct, <c>D O R P a c e g</c> 0%.
    /// <c>txt.shx</c> will not catch this bug, because <c>txt</c> contains no arcs at all: its <c>D</c> is six straight segments.
    /// Test with <c>romans</c> or <c>isocp</c>.</para>
    /// </summary>
    private void OctantArc(byte radius, byte sc)
    {
        var count = sc & 0x07;
        if (count == 0) count = 8;                           // 0 means a full circle
        var start = (sc >> 4) & 0x07;
        var sweep = count * Octant * ((sc & 0x80) != 0 ? -1 : 1);
        ArcFromPen(radius, start * Octant, sweep);
    }

    /// <summary>
    /// <c>0x0B start_offset end_offset high_radius radius ±0SC</c>: arcs whose ends do not need to land on octant boundaries.
    /// Offsets are in 1/256 of the octant, measured from the boundary; the radius is 16-bit, high byte first.
    ///
    /// <para>Here the octant count is literal, 0 meaning <b>zero</b> octants — an arc entirely contained within one octant,
    /// whereas in <c>0x0A</c> it means eight. With both offsets zero it degenerates exactly to <c>0x0A</c>, the condition that pins down its reading.</para>
    /// </summary>
    private void FractionalArc(byte startOffset, byte endOffset, byte highRadius,
        byte lowRadius, byte sc)
    {
        var radius = (highRadius << 8) | lowRadius;
        var start = (sc >> 4) & 0x07;
        var count = sc & 0x07;
        var dir = (sc & 0x80) != 0 ? -1 : 1;

        var a0 = start * Octant + dir * (startOffset / 256.0) * Octant;
        var a1 = (start + dir * count) * Octant + dir * (endOffset / 256.0) * Octant;
        ArcFromPen(radius, a0, a1 - a0);
    }

    /// <summary>
    /// An arc whose start is at the pen. The current point is <b>on the circle</b> at <paramref name="startAngle"/>, so the center lies
    /// opposite along that radius — the format never declares a center.
    /// </summary>
    private void ArcFromPen(float radius, double startAngle, double sweep)
        => Arc(
            (float)(-radius * Math.Cos(startAngle)),
            (float)(-radius * Math.Sin(startAngle)),
            radius, startAngle, sweep);

    /// <summary>
    /// <c>0x0C dx dy bulge</c>: the arc over chord (dx, dy). The bulge is <c>127 * 2H/D</c> with D the chord length and H the arc
    /// height, so |bulge| = 127 is a semicircle and nothing larger can be expressed; positive is counter-clockwise, zero is a straight line (fonts do use it).
    /// </summary>
    private void BulgeArc(sbyte dx, sbyte dy, sbyte bulge)
    {
        if (bulge == 0) { Step(dx, dy); return; }

        float vx = dx, vy = dy;
        var chord = MathF.Sqrt(vx * vx + vy * vy);
        if (chord <= 0f) return;

        var half = chord * 0.5f;
        var height = Math.Abs(bulge) / 127f * half;
        if (height <= 0f) { Step(dx, dy); return; }
        var radius = (half * half + height * height) / (2f * height);

        // From the chord midpoint, walk (radius - height) along the left normal of the chord. Counter-clockwise (positive bulge) puts the center on the left and
        // bows the arc to the right of the chord; negative mirrors both.
        var sign = bulge > 0 ? 1f : -1f;
        var nx = -vy / chord;
        var ny = vx / chord;
        var cx = vx * 0.5f + sign * (radius - height) * nx;
        var cy = vy * 0.5f + sign * (radius - height) * ny;

        // The pen is the local origin, so its angle about the center is atan2(-cy, -cx).
        var a0 = Math.Atan2(-cy, -cx);
        var sweep = sign * 2.0 * Math.Asin(Math.Min(1.0, half / radius));
        Arc(cx, cy, radius, a0, sweep);
    }

    /// <summary>
    /// Emits the arc as cubic Béziers, one segment per 45 degrees (or less).
    ///
    /// <para>The arc becomes <see cref="IGlyphSink.CubicTo"/> rather than the polyline a quick implementation would pick. The geometry is computed in
    /// unscaled units with the pen as local origin, and every emitted point is mapped out through the X, Y scaling; Béziers are affine-invariant, so the
    /// scaled control points are exact even when the two scales differ (as inside a bigfont composition box).</para>
    /// </summary>
    /// <param name="cx">Center X, unscaled, relative to the pen.</param>
    /// <param name="cy">Center Y, unscaled, relative to the pen.</param>
    private void Arc(float cx, float cy, float radius, double startAngle, double sweep)
    {
        if (radius <= 0f || Math.Abs(sweep) < 1e-9) return;

        var segments = (int)Math.Ceiling(Math.Abs(sweep) / Octant);
        if (segments < 1) segments = 1;
        var step = sweep / segments;
        var k = 4.0 / 3.0 * Math.Tan(step / 4.0);

        var ox = _x;
        var oy = _y;
        if (_penDown) EnsureContour();

        for (var s = 0; s < segments; s++)
        {
            var t0 = startAngle + step * s;
            var t1 = t0 + step;
            var cos0 = Math.Cos(t0);
            var sin0 = Math.Sin(t0);
            var cos1 = Math.Cos(t1);
            var sin1 = Math.Sin(t1);

            var p0x = cx + radius * cos0;
            var p0y = cy + radius * sin0;
            var p3x = cx + radius * cos1;
            var p3y = cy + radius * sin1;

            if (_penDown)
            {
                _sink?.CubicTo(
                    MapX(ox, p0x - k * radius * sin0), MapY(oy, p0y + k * radius * cos0),
                    MapX(ox, p3x + k * radius * sin1), MapY(oy, p3y - k * radius * cos1),
                    MapX(ox, p3x), MapY(oy, p3y));
            }
        }

        _x = MapX(ox, cx + radius * Math.Cos(startAngle + sweep));
        _y = MapY(oy, cy + radius * Math.Sin(startAngle + sweep));
    }

    private float MapX(float origin, double local) => origin + (float)local * _scaleX;
    private float MapY(float origin, double local) => origin + (float)local * _scaleY;

    /// <summary>
    /// <c>0x07</c>: inlines the instructions of another record, inheriting pen, scaling and position.
    ///
    /// <para><b>unifont</b> takes a 2-byte code, <b>high byte first</b> — opposite to every length and count field in the container (all little-endian).
    /// Established by measurement: 3,185 references across 170 stock unifont fonts, 3,181 resolve to codes the font actually defines
    /// when high byte first, versus only 9 in the reverse order.</para>
    ///
    /// <para><b>bigfont</b> takes one byte, unless that byte is <c>0x00</c>, which is the extended composition form used to build CJK glyphs
    /// out of radicals: <c>0x00, code_hi, code_lo, base_x, base_y, width, height</c>. Reading it throughout as an ordinary 1-byte operand,
    /// 94.8% of records land on a terminator and 56% of references resolve; honoring the escape raises these to 99.98% and 98.5%.</para>
    /// </summary>
    private void Subshape(ReadOnlySpan<byte> data, ref int i)
    {
        if (IsTwoByteSubshape)
        {
            if (i + 1 >= data.Length) { i = data.Length; return; }
            var code = (data[i] << 8) | data[i + 1];
            i += 2;
            RunSubshape(code);
            return;
        }

        if (i >= data.Length) return;
        if (data[i] != 0)
        {
            RunSubshape(data[i]);
            i++;
            return;
        }

        if (i + 6 >= data.Length) { i = data.Length; return; }
        var composed = (data[i + 1] << 8) | data[i + 2];
        var baseX = (sbyte)data[i + 3];
        var baseY = (sbyte)data[i + 4];
        var width = data[i + 5];
        var height = data[i + 6];
        i += 7;
        RunComposedSubshape(composed, baseX, baseY, width, height);
    }

    private void RunSubshape(int code)
    {
        if (_depth >= MaxSubshapeDepth) return;
        if (!_font.TryGetRecord(code, out var record)) return;
        _depth++;
        RunRecord(record);
        _depth--;
    }

    /// <summary>
    /// The bigfont extended composition form: places <paramref name="code"/> offset by (<paramref name="baseX"/>, <paramref name="baseY"/>)
    /// and scaled into a <paramref name="width"/> × <paramref name="height"/> box.
    ///
    /// <para>The box is measured in font units of the same magnitude as <see cref="ShxFont.Above"/>, not fixed-point fractions: the most common observed triple is
    /// (above 60, width 59, height 60), with (above 15, height 15) dominant and width varying, i.e. same-height radicals of different widths.
    /// Hence the scaling is width/above and height/above. The parent pen is then restored, since base_x and base_y are 0 in most cases,
    /// which reads as every radical being placed from a common origin.</para>
    ///
    /// <para>These placement semantics were inferred from the corpus statistics above, not given by the spec; the geometry is exact in the dominant height == above case.</para>
    /// </summary>
    private void RunComposedSubshape(int code, int baseX, int baseY, int width, int height)
    {
        if (_depth >= MaxSubshapeDepth) return;
        if (!_font.TryGetRecord(code, out var record)) return;

        var savedX = _x;
        var savedY = _y;
        var savedScaleX = _scaleX;
        var savedScaleY = _scaleY;
        var savedPen = _penDown;

        var reference = _font.Above > 0 ? _font.Above : 1;
        _x += baseX * _scaleX;
        _y += baseY * _scaleY;
        if (width > 0) _scaleX = savedScaleX * (width / (float)reference);
        if (height > 0) _scaleY = savedScaleY * (height / (float)reference);
        _contourOpen = false;

        _depth++;
        RunRecord(record);
        _depth--;

        _x = savedX;
        _y = savedY;
        _scaleX = savedScaleX;
        _scaleY = savedScaleY;
        _penDown = savedPen;
        _contourOpen = false;
    }
}
#endif
