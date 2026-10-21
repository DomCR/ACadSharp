#if NET5_0_OR_GREATER
#nullable disable
// Purely managed TrueType / OpenType font parser (zero external dependencies; replaces SkiaSharp's TTF metrics and glyph-outline extraction).
//
// Supports the two kinds of font outlines that DWG text styles actually use:
//   - TrueType outlines (glyf table; .ttf such as simhei/simsun/msyh)
//   - OpenType/CFF outlines (CFF table; .otf)
// Glyph outlines are emitted to IGlyphSink as path commands (TTF emits QuadTo quadratic curves; CFF emits CubicTo cubic curves),
// sharing the same sink set as ShxFont so the upper-layer rasterizer can consume them uniformly.
//
// Ported and condensed from SixLabors.Fonts (MIT / Six Labors Split License) table-parsing logic,
// retaining only the subset needed for export/rasterization (head/hhea/maxp/loca/hmtx/OS2/post/name/cmap/glyf/CFF).

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace ACadSharp.Text;

/// <summary>
/// A purely managed TrueType/OpenType font (TTF glyf outlines or OTF CFF outlines).
/// Provides units, ascent/descent, advance widths, glyph outlines, and so on, for reuse by image/PDF export and text-bounds computation.
/// </summary>
public sealed class TrueTypeFont
{
    /// <summary>The kind of outline.</summary>
    public enum OutlineKind
    {
        /// <summary>TrueType glyf table (quadratic Béziers).</summary>
        TrueType,
        /// <summary>OpenType CFF table (cubic Type2 charprograms).</summary>
        Cff,
    }

    // ---- public metrics ----
    public OutlineKind Kind { get; }
    public float UnitsPerEm { get; }
    public float Ascent { get; }
    public float Descent { get; }
    public float LineGap { get; }
    /// <summary>Caps height (OS/2 sCapHeight, font units); 0 when OS/2 version&lt;2 or the outlines are CFF.</summary>
    public float CapHeight { get; }
    public float BBoxXMin { get; }
    public float BBoxYMin { get; }
    public float BBoxXMax { get; }
    public float BBoxYMax { get; }
    public string FamilyName { get; }
    public string StyleName { get; }
    public string FullName { get; }
    public float ItalicAngle { get; }

    // ---- internal table data ----
    private readonly byte[] _data;
    private readonly ushort _numGlyphs;
    private readonly int _glyphBase;          // offset of the glyf table within _data
    private readonly uint[] _loca;            // glyph offsets (relative to the start of the glyf table)
    private readonly ushort[] _advWidths;     // hmtx advance widths
    private readonly int _numHMetrics;
    private readonly CMap _cmap;
    private readonly bool _isCff;
    private readonly CffData? _cff;

    private TrueTypeFont(
        byte[] data, OutlineKind kind, float unitsPerEm,
        float ascent, float descent, float lineGap, float capHeight,
        float bbX0, float bbY0, float bbX1, float bbY1,
        string family, string style, string full, float italic,
        ushort numGlyphs, int glyphBase, uint[] loca,
        ushort[] advWidths, int numHMetrics, CMap cmap, CffData? cff)
    {
        _data = data;
        Kind = kind;
        UnitsPerEm = unitsPerEm;
        Ascent = ascent;
        Descent = descent;
        LineGap = lineGap;
        CapHeight = capHeight;
        BBoxXMin = bbX0; BBoxYMin = bbY0; BBoxXMax = bbX1; BBoxYMax = bbY1;
        FamilyName = family;
        StyleName = style;
        FullName = full;
        ItalicAngle = italic;
        _numGlyphs = numGlyphs;
        _glyphBase = glyphBase;
        _loca = loca;
        _advWidths = advWidths;
        _numHMetrics = numHMetrics;
        _cmap = cmap;
        _isCff = kind == OutlineKind.Cff;
        _cff = cff;
    }

    /// <summary>Loads a font from a file (.ttf / .otf / .ttc; for TTC the first font is taken).</summary>
    public static TrueTypeFont? LoadFromFile(string path)
    {
        try
        {
            byte[] data = File.ReadAllBytes(path);
            int off = DetectFontOffset(data);
            if (off < 0) return null;
            return Parse(data, off);
        }
        catch { return null; }
    }

    /// <summary>Loads a font from in-memory bytes (.ttf / .otf / .ttc).</summary>
    public static TrueTypeFont? Load(ReadOnlySpan<byte> span)
    {
        try
        {
            byte[] data = span.ToArray();
            int off = DetectFontOffset(data);
            if (off < 0) return null;
            return Parse(data, off);
        }
        catch { return null; }
    }

    // A TTC (TrueType Collection) file starts with "ttcf", followed by numFonts and the per-font offset-table start.
    // Returns the offset of the first font within data; returns 0 when not a TTC; returns -1 when unrecognizable.
    private static int DetectFontOffset(byte[] data)
    {
        if (data.Length < 12) return 0;
        if (data[0] == (byte)'t' && data[1] == (byte)'t' && data[2] == (byte)'c' && data[3] == (byte)'f')
        {
            try
            {
                var r = new BeReader(data);
                r.Skip(4); // ttcf
                r.ReadUInt32(); // version (for v1.0/v2.0 the offset tables all come after numFonts, and dsig after the offset tables, so nothing needs skipping)
                int numFonts = (int)r.ReadUInt32();
                if (numFonts <= 0) return -1;
                return (int)r.ReadUInt32(); // position of the first font's offset table
            }
            catch { return -1; }
        }
        return 0; // ordinary .ttf/.otf
    }

    #region ===== table directory and parsing =====

    private static TrueTypeFont Parse(byte[] data, int fontOffset = 0)
    {
        var r = new BeReader(data, fontOffset);
        uint sfntVersion = r.ReadUInt32();
        bool isCff = sfntVersion == 0x4F54544F; // "OTTO"
        int numTables = r.ReadUInt16();
        r.Skip(6); // searchRange, entrySelector, rangeShift

        var tableOffsets = new Dictionary<string, (uint off, uint len)>(StringComparer.Ordinal);
        for (int i = 0; i < numTables; i++)
        {
            string tag = r.ReadTag();
            r.Skip(4); // checksum
            uint off = r.ReadUInt32();
            uint len = r.ReadUInt32();
            tableOffsets[tag] = (off, len);
        }

        // ---- head ----
        if (!tableOffsets.TryGetValue("head", out var headPos))
            throw new InvalidDataException("缺少 head 表");
        var head = new BeReader(data, (int)headPos.off);
        head.Skip(18); // version(4)+fontRevision(4)+checkSumAdj(4)+magic(4)+flags(2)
        ushort unitsPerEm = head.ReadUInt16();
        head.Skip(16); // created(8)+modified(8)
        float bbX0 = head.ReadFWord(), bbY0 = head.ReadFWord(), bbX1 = head.ReadFWord(), bbY1 = head.ReadFWord();
        head.Skip(6); // macStyle(2)+lowestRecPPEM(2)+fontDirHint(2)
        short indexToLocFormat = head.ReadInt16();

        // ---- hhea ----
        if (!tableOffsets.TryGetValue("hhea", out var hheaPos))
            throw new InvalidDataException("缺少 hhea 表");
        var hhea = new BeReader(data, (int)hheaPos.off);
        hhea.Skip(4); // version
        short hheaAsc = hhea.ReadFWord();
        short hheaDesc = hhea.ReadFWord();
        short hheaLineGap = hhea.ReadFWord();
        hhea.Skip(22); // advanceWidthMax + minLSB + minRSB + xMaxExtent + caret(SlopeRise/Run/Offset) + reserved[4]
        hhea.Skip(2); // metricDataFormat
        ushort numHMetrics = hhea.ReadUInt16();

        // ---- maxp ----
        if (!tableOffsets.TryGetValue("maxp", out var maxpPos))
            throw new InvalidDataException("缺少 maxp 表");
        var maxp = new BeReader(data, (int)maxpPos.off);
        maxp.Skip(4); // version
        ushort numGlyphs = maxp.ReadUInt16();

        // ---- OS/2 ----
        // Read using absolute offsets (verified: arial reads 1491/-431/1854/434/1467 correctly at 68/70/72/74/76/88).
        // The old version used sequential Skips but skipped only 8 bytes for subscript/superscript (it jumped 4*2=8, which is really 8 fields = 16 bytes),
        // shifting fsSelection/sTypo*/sCapHeight all out of place; it now positions directly by the spec offsets and additionally reads sCapHeight.
        bool useTypo = false;
        short typoAsc = 0, typoDesc = 0, typoLineGap = 0;
        ushort winAsc = 0, winDesc = 0;
        short capHeight = 0;
        if (tableOffsets.TryGetValue("OS/2", out var os2Pos))
        {
            int o = (int)os2Pos.off;
            ushort version2 = (ushort)((data[o] << 8) | data[o + 1]);
            ushort fsSelection = (ushort)((data[o + 62] << 8) | data[o + 63]);
            useTypo = (fsSelection & 0x80) != 0; // USE_TYPO_METRICS
            typoAsc = (short)((data[o + 68] << 8) | data[o + 69]);
            typoDesc = (short)((data[o + 70] << 8) | data[o + 71]);
            typoLineGap = (short)((data[o + 72] << 8) | data[o + 73]);
            winAsc = (ushort)((data[o + 74] << 8) | data[o + 75]);
            winDesc = (ushort)((data[o + 76] << 8) | data[o + 77]);
            if (version2 >= 2)
                capHeight = (short)((data[o + 88] << 8) | data[o + 89]); // sCapHeight
        }

        // FreeType-style global metrics
        float ascent, descent, lineGap;
        if (useTypo)
        {
            ascent = typoAsc; descent = typoDesc; lineGap = typoLineGap;
        }
        else
        {
            ascent = hheaAsc; descent = hheaDesc; lineGap = hheaLineGap;
            if (ascent == 0 || descent == 0)
            {
                if (typoAsc != 0 || typoDesc != 0)
                { ascent = typoAsc; descent = typoDesc; lineGap = typoLineGap; }
                else
                { ascent = winAsc; descent = -winDesc; lineGap = 0; }
            }
        }

        // ---- post ----
        float italicAngle = 0;
        if (tableOffsets.TryGetValue("post", out var postPos))
        {
            var post = new BeReader(data, (int)postPos.off);
            post.Skip(4); // version
            italicAngle = post.ReadFixed();
        }

        // ---- name ----
        string family = "", style = "", full = "";
        if (tableOffsets.TryGetValue("name", out var namePos))
            (family, style, full) = ParseName(data, (int)namePos.off);

        // ---- hmtx ----
        ushort[] advWidths; short[] lsb;
        if (tableOffsets.TryGetValue("hmtx", out var hmtxPos))
        {
            var hmtx = new BeReader(data, (int)hmtxPos.off);
            int metricCount = Math.Min(numHMetrics, numGlyphs);
            int bearingCount = Math.Max(0, numGlyphs - numHMetrics);
            advWidths = new ushort[numGlyphs];
            lsb = new short[numGlyphs];
            for (int i = 0; i < metricCount; i++)
            {
                advWidths[i] = hmtx.ReadUInt16();
                lsb[i] = hmtx.ReadInt16();
            }
            ushort lastAdv = metricCount > 0 ? advWidths[metricCount - 1] : (ushort)0;
            for (int i = metricCount; i < numGlyphs; i++) advWidths[i] = lastAdv;
            for (int i = 0; i < bearingCount; i++) lsb[metricCount + i] = hmtx.ReadInt16();
        }
        else { advWidths = new ushort[numGlyphs]; lsb = new short[numGlyphs]; }

        // ---- cmap ----
        CMap cmap = CMap.Empty;
        if (tableOffsets.TryGetValue("cmap", out var cmapPos))
            cmap = CMap.Parse(data, (int)cmapPos.off);

        if (isCff)
        {
            if (!tableOffsets.TryGetValue("CFF ", out var cffPos))
                throw new InvalidDataException("OTTO 字体缺少 CFF 表");
            CffData cff = CffData.Parse(data, (int)cffPos.off);
            return new TrueTypeFont(data, OutlineKind.Cff, unitsPerEm,
                ascent, descent, lineGap, 0, bbX0, bbY0, bbX1, bbY1,
                family, style, full, italicAngle,
                numGlyphs, 0, Array.Empty<uint>(),
                advWidths, numHMetrics, cmap, cff);
        }
        else
        {
            if (!tableOffsets.TryGetValue("glyf", out var glyfPos))
                throw new InvalidDataException("TrueType 字体缺少 glyf 表");
            if (!tableOffsets.TryGetValue("loca", out var locaPos))
                throw new InvalidDataException("TrueType 字体缺少 loca 表");
            uint[] loca = ReadLoca(data, (int)locaPos.off, numGlyphs, indexToLocFormat == 1);
            return new TrueTypeFont(data, OutlineKind.TrueType, unitsPerEm,
                ascent, descent, lineGap, capHeight, bbX0, bbY0, bbX1, bbY1,
                family, style, full, italicAngle,
                numGlyphs, (int)glyfPos.off, loca,
                advWidths, numHMetrics, cmap, null);
        }
    }

    private static uint[] ReadLoca(byte[] data, int off, int numGlyphs, bool longFormat)
    {
        var r = new BeReader(data, off);
        int n = numGlyphs + 1;
        uint[] loca = new uint[n];
        if (longFormat)
            for (int i = 0; i < n; i++) loca[i] = r.ReadUInt32();
        else
            for (int i = 0; i < n; i++) loca[i] = (uint)(r.ReadUInt16() * 2);
        return loca;
    }

    private static (string, string, string) ParseName(byte[] data, int off)
    {
        var r = new BeReader(data, off);
        r.Skip(2); // format
        ushort count = r.ReadUInt16();
        ushort strBase = r.ReadUInt16();
        string family = "", style = "", full = "";
        for (int i = 0; i < count; i++)
        {
            ushort platform = r.ReadUInt16();
            ushort encoding = r.ReadUInt16();
            ushort lang = r.ReadUInt16();
            ushort nameId = r.ReadUInt16();
            ushort len = r.ReadUInt16();
            ushort sOff = r.ReadUInt16();
            if (nameId is 1 or 2 or 4 or 6)
            {
                var s = ReadNameString(data, off + strBase + sOff, len, platform, encoding);
                if (nameId == 1 && family.Length == 0) family = s;
                else if (nameId == 2 && style.Length == 0) style = s;
                else if (nameId == 4 && full.Length == 0) full = s;
                else if (nameId == 6 && full.Length == 0 && family.Length == 0) full = s;
            }
        }
        if (full.Length == 0) full = (family + " " + style).Trim();
        return (family, style, full);
    }

    private static string ReadNameString(byte[] data, int off, int len, ushort platform, ushort encoding)
    {
        if (platform == 0 || platform == 3) // Unicode / Windows
        {
            var sb = new StringBuilder(len / 2);
            for (int i = 0; i + 1 < len; i += 2)
                sb.Append((char)((data[off + i] << 8) | data[off + i + 1]));
            return sb.ToString();
        }
        else if (platform == 1 && encoding == 0)
        {
            try { return Encoding.GetEncoding("macintosh").GetString(data, off, len); }
            catch { return Encoding.Latin1.GetString(data, off, len); } // fall back to Latin1 when .NET has no CodePages by default; the name is only metadata
        }
        else
            return Encoding.ASCII.GetString(data, off, len);
    }

    #endregion

    #region ===== glyph lookup and outlines =====

    /// <summary>Unicode code point → glyph index.</summary>
    public ushort GetGlyphIndex(int codepoint) => _cmap.TryGetGlyphId(codepoint, out ushort g) ? g : (ushort)0;

    /// <summary>Glyph index → advance width (font units).</summary>
    public float GetAdvanceWidth(ushort glyphIndex)
        => glyphIndex < _advWidths.Length ? _advWidths[glyphIndex] : 0;

    /// <summary>Unicode code point → advance width (font units).</summary>
    public float GetAdvanceWidth(int codepoint) => GetAdvanceWidth(GetGlyphIndex(codepoint));

    /// <summary>Glyph index → ink right edge xMax (font units). Falls back to the advance width for CFF or when there is no glyf table.</summary>
    public float GetGlyphInkRight(ushort glyphIndex)
    {
        if (_isCff || glyphIndex >= _numGlyphs) return GetAdvanceWidth(glyphIndex);
        long goff = (long)_glyphBase + _loca[glyphIndex];
        int p = (int)goff + 6; // glyf header: numberOfContours(2)+xMin(2)+yMin(2)+xMax(2)
        if (p < 0 || p + 8 > _data.Length) return GetAdvanceWidth(glyphIndex);
        return (short)((ushort)((_data[p] << 8) | _data[p + 1]));
    }

    /// <summary>Glyph index → ink left edge xMin (font units, glyf header offset +2). Falls back to 0 for CFF/no glyf.</summary>
    public float GetGlyphInkLeft(ushort glyphIndex)
    {
        if (_isCff || glyphIndex >= _numGlyphs) return 0;
        long goff = (long)_glyphBase + _loca[glyphIndex];
        int p = (int)goff + 2; // glyf header: numberOfContours(2)+xMin(2)
        if (p < 0 || p + 8 > _data.Length) return 0;
        return (short)((ushort)((_data[p] << 8) | _data[p + 1]));
    }

    /// <summary>Glyph index → left side bearing of the first glyph (font units): the space between the left edge of the glyph's advance box and the ink left edge (xMin).
    /// Line layout should align to the "ink left edge" (measured in AutoCAD), so this bearing must be yielded (clamped to 0 when negative).</summary>
    public float GetGlyphLeadingGap(ushort glyphIndex)
    {
        if (_isCff || glyphIndex >= _numGlyphs) return 0;
        float xMin = GetGlyphInkLeft(glyphIndex);
        return xMin > 0 ? xMin : 0;
    }

    /// <summary>Unicode code point → left side bearing of the first glyph (font units).</summary>
    public float GetGlyphLeadingGap(int codepoint) => GetGlyphLeadingGap(GetGlyphIndex(codepoint));

    /// <summary>Glyph index → trailing whitespace (font units): advance width − ink right edge = the right-side space after the last glyph and before the next.
    /// Wrap decisions should strip this whitespace (AutoCAD goes by the last glyph's ink right edge crossing the limit, not by the advance placeholder). 0 for CFF/no glyf.</summary>
    public float GetGlyphTrailingGap(ushort glyphIndex)
    {
        if (_isCff || glyphIndex >= _numGlyphs) return 0;
        float adv = GetAdvanceWidth(glyphIndex);
        float xMax = GetGlyphInkRight(glyphIndex);
        float g = adv - xMax;
        return g > 0 ? g : 0;
    }

    /// <summary>Unicode code point → trailing whitespace (font units).</summary>
    public float GetGlyphTrailingGap(int codepoint) => GetGlyphTrailingGap(GetGlyphIndex(codepoint));

    /// <summary>Extracts the glyph outline for a code point to the sink. Returns false when there is no such glyph.</summary>
    public bool TryGetGlyph(int codepoint, IGlyphSink sink)
        => TryGetGlyph(GetGlyphIndex(codepoint), sink);

    /// <summary>Extracts the glyph outline for a glyph index to the sink. Returns false when there is no such glyph.</summary>
    public bool TryGetGlyph(ushort glyphIndex, IGlyphSink sink)
    {
        if (glyphIndex >= _numGlyphs) return false;
        if (_isCff)
        {
            if (_cff is null) return false;
            _cff.EmitGlyph(_data, glyphIndex, sink);
            return true;
        }
        else
        {
            var contours = GetTtContours(glyphIndex, Matrix2D.Identity, 0);
            foreach (var c in contours) EmitContour(c, sink);
            return true;
        }
    }

    #endregion

    #region ===== TrueType glyf decoding =====

    private sealed class Pt
    {
        public float X, Y;
        public bool On;
        public Pt(float x, float y, bool on) { X = x; Y = y; On = on; }
    }

    private List<List<Pt>> GetTtContours(ushort glyphIndex, Matrix2D m, int depth)
    {
        if (depth > 16 || glyphIndex >= _numGlyphs) return new List<List<Pt>>();
        uint start = _loca[glyphIndex];
        uint end = _loca[glyphIndex + 1];
        if (end <= start) return new List<List<Pt>>();

        var r = new BeReader(_data, _glyphBase + (int)start);
        short contourCount = r.ReadInt16();
        if (contourCount < 0)
            return GetCompositeContours(r, m, depth);
        if (contourCount == 0) return new List<List<Pt>>();
        r.Skip(8); // bbox

        ushort[] endPts = new ushort[contourCount];
        for (int i = 0; i < contourCount; i++) endPts[i] = r.ReadUInt16();
        ushort instrLen = r.ReadUInt16();
        r.Skip(instrLen);

        int pointCount = endPts[contourCount - 1] + 1;
        byte[] flags = new byte[pointCount];
        int p = 0;
        while (p < pointCount)
        {
            byte f = r.ReadByte();
            flags[p++] = f;
            if ((f & 0x08) != 0) // REPEAT
            {
                byte rep = r.ReadByte();
                for (int k = 0; k < rep && p < pointCount; k++) flags[p++] = f;
            }
        }
        short[] xs = ReadCoords(r, pointCount, flags, 0x02, 0x10); // XBYTE, XSIGN_OR_SAME
        short[] ys = ReadCoords(r, pointCount, flags, 0x04, 0x20); // YBYTE, YSIGN_OR_SAME

        var contours = new List<List<Pt>>(contourCount);
        int startIdx = 0;
        for (int c = 0; c < contourCount; c++)
        {
            int last = endPts[c];
            var pts = new List<Pt>(last - startIdx + 1);
            for (int i = startIdx; i <= last; i++)
            {
                var v = m.Transform(xs[i], ys[i]);
                pts.Add(new Pt(v.X, v.Y, (flags[i] & 0x01) != 0));
            }
            contours.Add(pts);
            startIdx = last + 1;
        }
        return contours;
    }

    private List<List<Pt>> GetCompositeContours(BeReader r, Matrix2D m, int depth)
    {
        var result = new List<List<Pt>>();
        const ushort ARG_1_2_WORDS = 0x0001;
        const ushort ARGS_ARE_XY = 0x0002;
        const ushort WE_HAVE_SCALE = 0x0008;
        const ushort MORE_COMPONENTS = 0x0020;
        const ushort WE_HAVE_XY_SCALE = 0x0040;
        const ushort WE_HAVE_2X2 = 0x0080;

        bool more;
        do
        {
            ushort flags = r.ReadUInt16();
            ushort gi = r.ReadUInt16();
            int dx, dy;
            if ((flags & ARG_1_2_WORDS) != 0)
            {
                if ((flags & ARGS_ARE_XY) != 0) { dx = r.ReadInt16(); dy = r.ReadInt16(); }
                else { dx = r.ReadUInt16(); dy = r.ReadUInt16(); }
            }
            else
            {
                if ((flags & ARGS_ARE_XY) != 0) { dx = r.ReadSByte(); dy = r.ReadSByte(); }
                else { dx = r.ReadByte(); dy = r.ReadByte(); }
            }

            Matrix2D t = Matrix2D.Identity;
            t.E = dx; t.F = dy;
            if ((flags & WE_HAVE_SCALE) != 0)
            {
                float s = r.ReadF2Dot14();
                t.A = s; t.D = s;
            }
            else if ((flags & WE_HAVE_XY_SCALE) != 0)
            {
                t.A = r.ReadF2Dot14();
                t.D = r.ReadF2Dot14();
            }
            else if ((flags & WE_HAVE_2X2) != 0)
            {
                t.A = r.ReadF2Dot14();
                t.B = r.ReadF2Dot14();
                t.C = r.ReadF2Dot14();
                t.D = r.ReadF2Dot14();
            }

            Matrix2D combined = m * t;
            result.AddRange(GetTtContours(gi, combined, depth + 1));

            more = (flags & MORE_COMPONENTS) != 0;
        } while (more);
        return result;
    }

    private static short[] ReadCoords(BeReader r, int n, byte[] flags, byte isByteFlag, byte sameFlag)
    {
        short[] outv = new short[n];
        short v = 0;
        for (int i = 0; i < n; i++)
        {
            byte f = flags[i];
            if ((f & isByteFlag) != 0)
            {
                byte b = r.ReadByte();
                v += (short)((f & sameFlag) != 0 ? b : -b);
            }
            else if ((f & sameFlag) != 0)
            {
                // Same as the previous coordinate; v is unchanged.
            }
            else
            {
                v += r.ReadInt16();
            }
            outv[i] = v;
        }
        return outv;
    }

    private static void EmitContour(List<Pt> pts, IGlyphSink sink)
    {
        int n = pts.Count;
        if (n < 2) return;

        int start = 0;
        while (start < n && !pts[start].On) start++;

        if (start == n) // all off-curve points: insert implicit on-curve points (midpoints) between adjacent off-curve points
        {
            var m0 = Mid(pts[n - 1], pts[0]);
            sink.MoveTo(m0.X, m0.Y);
            for (int i = 0; i < n; i++)
            {
                int j = (i + 1) % n;
                var impl = Mid(pts[i], pts[j]);
                sink.QuadTo(pts[i].X, pts[i].Y, impl.X, impl.Y);
            }
            sink.Close();
            return;
        }

        sink.MoveTo(pts[start].X, pts[start].Y);
        int i2 = start;
        int guard = 0;
        do
        {
            int j = (i2 + 1) % n;
            if (pts[j].On)
            {
                sink.LineTo(pts[j].X, pts[j].Y);
                i2 = j;
            }
            else
            {
                int k = (j + 1) % n;
                if (pts[k].On)
                {
                    sink.QuadTo(pts[j].X, pts[j].Y, pts[k].X, pts[k].Y);
                    i2 = k;
                }
                else
                {
                    var impl = Mid(pts[j], pts[k]);
                    sink.QuadTo(pts[j].X, pts[j].Y, impl.X, impl.Y);
                    i2 = k;
                }
            }
            if (guard++ > n + 2) break;
        } while (i2 != start);
        sink.Close();
    }

    private static Pt Mid(Pt a, Pt b) => new Pt((a.X + b.X) * 0.5f, (a.Y + b.Y) * 0.5f, true);

    #endregion

    #region ===== 2x3 transformation matrix (avoids introducing a System.Numerics dependency)=====

    private sealed class Matrix2D
    {
        public float A = 1, B = 0, C = 0, D = 1, E = 0, F = 0; // x' = A*x + C*y + E ; y' = B*x + D*y + F
        public static readonly Matrix2D Identity = new();
        public Pt Transform(float x, float y) => new Pt(A * x + C * y + E, B * x + D * y + F, true);
        public static Matrix2D operator *(Matrix2D l, Matrix2D r)
        {
            return new Matrix2D
            {
                A = l.A * r.A + l.C * r.B,
                B = l.B * r.A + l.D * r.B,
                C = l.A * r.C + l.C * r.D,
                D = l.B * r.C + l.D * r.D,
                E = l.A * r.E + l.C * r.F + l.E,
                F = l.B * r.E + l.D * r.F + l.F,
            };
        }
    }

    #endregion

    #region ===== cmap =====

    private sealed class CMap
    {
        public static readonly CMap Empty = new();
        private readonly int _fmt;
        private readonly ushort[]? _f0;
        private readonly F4Seg[]? _f4;
        private readonly F12Grp[]? _f12;
        private readonly (ushort start, ushort end, ushort gid)[]? _f6;

        private CMap() { _fmt = -1; }
        private CMap(int fmt, ushort[]? f0, F4Seg[]? f4, F12Grp[]? f12, (ushort, ushort, ushort)[]? f6)
        { _fmt = fmt; _f0 = f0; _f4 = f4; _f12 = f12; _f6 = f6; }

        public bool TryGetGlyphId(int cp, out ushort gid)
        {
            gid = 0;
            if (_fmt < 0) return false;
            int c = cp;
            switch (_fmt)
            {
                case 0:
                    if (c >= 0 && c < 256) { gid = _f0![c]; return gid != 0; }
                    return false;
                case 4:
                    foreach (var s in _f4!)
                    {
                        if (c >= s.Start && c <= s.End)
                        {
                            if (s.Offset == 0) { gid = (ushort)((c + s.Delta) & 0xFFFF); return gid != 0; }
                            long idx = (s.Offset / 2) + (c - s.Start) - _f4.Length + s.Index;
                            if (idx < 0 || idx >= s.Glyphs.Length) return false;
                            ushort g = s.Glyphs[idx];
                            if (g == 0) return false;
                            gid = (ushort)((g + s.Delta) & 0xFFFF);
                            return gid != 0;
                        }
                    }
                    return false;
                case 6:
                    foreach (var (start, end, gidBase) in _f6!)
                        if (c >= start && c <= end) { gid = (ushort)(gidBase + (c - start)); return gid != 0; }
                    return false;
                case 12:
                    foreach (var s in _f12!)
                        if (c >= s.Start && c <= s.End) { gid = (ushort)(c - s.Start + s.Gid); return gid != 0; }
                    return false;
            }
            return false;
        }

        public static CMap Parse(byte[] data, int off)
        {
            var r = new BeReader(data, off);
            r.Skip(2); // version
            ushort numTables = r.ReadUInt16();
            var recs = new (ushort plat, ushort enc, uint offset)[numTables];
            for (int i = 0; i < numTables; i++)
                recs[i] = (r.ReadUInt16(), r.ReadUInt16(), r.ReadUInt32());

            CMap? chosen = null;
            foreach (var pref in new (ushort plat, ushort enc)[]
            {
                (3, 10), (3, 1), (0, 6), (0, 4), (0, 3), (0, 2), (0, 1), (0, 0), (1, 0),
            })
            {
                if (chosen is not null) break;
                foreach (var rec in recs)
                {
                    if (rec.plat != pref.plat || rec.enc != pref.enc) continue;
                    var sub = ParseSubtable(data, off + (int)rec.offset);
                    if (sub is not null) { chosen = sub; break; }
                }
            }
            return chosen ?? Empty;
        }

        private static CMap? ParseSubtable(byte[] data, int subOff)
        {
            var r = new BeReader(data, subOff);
            ushort fmt = r.ReadUInt16();
            switch (fmt)
            {
                case 0:
                {
                    r.Skip(2); // length
                    r.Skip(2); // language
                    ushort[] f0 = new ushort[256];
                    for (int i = 0; i < 256; i++) f0[i] = r.ReadByte();
                    return new CMap(0, f0, null, null, null);
                }
                case 4:
                {
                    ushort len = r.ReadUInt16();
                    r.Skip(2); // language
                    ushort segX2 = r.ReadUInt16();
                    r.Skip(6); // searchRange, entrySelector, rangeShift
                    int seg = segX2 / 2;
                    ushort[] end = new ushort[seg];
                    for (int i = 0; i < seg; i++) end[i] = r.ReadUInt16();
                    r.Skip(2); // reserved
                    ushort[] start = new ushort[seg];
                    for (int i = 0; i < seg; i++) start[i] = r.ReadUInt16();
                    short[] delta = new short[seg];
                    for (int i = 0; i < seg; i++) delta[i] = r.ReadInt16();
                    ushort[] off = new ushort[seg];
                    for (int i = 0; i < seg; i++) off[i] = r.ReadUInt16();
                    int headerLen = 16 + seg * 8;
                    int gidCount = Math.Max(0, (len - headerLen) / 2);
                    ushort[] glyphs = new ushort[gidCount];
                    for (int i = 0; i < gidCount; i++) glyphs[i] = r.ReadUInt16();
                    var segs = new F4Seg[seg];
                    for (int i = 0; i < seg; i++)
                        segs[i] = new F4Seg { Index = (ushort)i, Start = start[i], End = end[i], Delta = delta[i], Offset = off[i], Glyphs = glyphs };
                    return new CMap(4, null, segs, null, null);
                }
                case 6:
                {
                    r.Skip(2); // length
                    r.Skip(2); // language
                    ushort first = r.ReadUInt16();
                    ushort count = r.ReadUInt16();
                    var arr = new (ushort, ushort, ushort)[count];
                    for (int i = 0; i < count; i++)
                        arr[i] = (first, (ushort)(first + count - 1), r.ReadUInt16());
                    return new CMap(6, null, null, null, arr);
                }
                case 12:
                {
                    r.Skip(2); // reserved
                    r.Skip(4); // length
                    r.Skip(4); // language
                    uint groups = r.ReadUInt32();
                    var gs = new F12Grp[groups];
                    for (int i = 0; i < groups; i++)
                    {
                        uint s = r.ReadUInt32();
                        uint e = r.ReadUInt32();
                        uint g = r.ReadUInt32();
                        gs[i] = new F12Grp { Start = s, End = e, Gid = g };
                    }
                    return new CMap(12, null, null, gs, null);
                }
            }
            return null;
        }

        private sealed class F4Seg
        {
            public ushort Index, Start, End;
            public short Delta;
            public ushort Offset;
            public ushort[] Glyphs = Array.Empty<ushort>();
        }
        private sealed class F12Grp { public uint Start, End, Gid; }
    }

    #endregion

    #region ===== CFF / Type2 charprograms =====

    private sealed class CffData
    {
        public float[] BBox = { 0, 0, 0, 0 };
        private readonly byte[] _data;
        private int _charStringsAbs;     // absolute offset of the CharStrings INDEX header within _data
        private byte[]? _globalSubrs;
        private byte[]? _localSubrs;
        private int _gsubrBias, _subrBias;

        private CffData(byte[] data) { _data = data; }

        public static CffData Parse(byte[] data, int off)
        {
            var cff = new CffData(data);
            var r = new BeReader(data, off);
            r.Skip(2); // major, minor
            byte hdrSize = r.ReadByte();
            r.ReadByte(); // offSize
            r = new BeReader(data, off + hdrSize);

            r = SkipIndex(r);                                   // skip the Name INDEX
            (byte[] topBytes, _) = ReadIndex(r);                // read the Top DICT INDEX
            r = SkipIndex(r);                                   // skip the String INDEX
            (cff._globalSubrs, _) = ReadIndex(r);               // read the Global Subrs INDEX
            cff._gsubrBias = CalcBias(cff._globalSubrs);

            ParseTopDict(data, off, topBytes, cff);
            cff._subrBias = CalcBias(cff._localSubrs);
            return cff;
        }

        private static void ParseTopDict(byte[] data, int cffBase, byte[] topBytes, CffData cff)
        {
            var r = new BeReader(topBytes, 0);
            int charStringsOff = -1, privateOff = -1, privateLen = 0;
            while (r.Position < topBytes.Length)
            {
                var (op, operands) = ReadDictOp(r);
                if (op < 0) break;
                switch (op)
                {
                    case 5: // FontBBox (4 numbers)
                        if (operands.Count >= 4)
                            cff.BBox = new float[] { (float)operands[0], (float)operands[1], (float)operands[2], (float)operands[3] };
                        break;
                    case 17: // CharStrings offset (relative to the CFF table start)
                        if (operands.Count >= 1) charStringsOff = (int)operands[0];
                        break;
                    case 18: // Private: [size offset]
                        if (operands.Count >= 2) { privateLen = (int)operands[0]; privateOff = (int)operands[1]; }
                        break;
                }
            }
            if (charStringsOff < 0) return;
            cff._charStringsAbs = cffBase + charStringsOff;

            if (privateOff >= 0 && privateLen > 0)
            {
                int abs = cffBase + privateOff;
                var pr = new BeReader(data, abs);
                int subrsOff = -1;
                while (pr.Position < abs + privateLen)
                {
                    var (pop, ops) = ReadDictOp(pr);
                    if (pop == 19 && ops.Count >= 1) subrsOff = (int)ops[0]; // Subrs (relative to the Private start)
                }
                if (subrsOff >= 0)
                {
                    var sr = new BeReader(data, abs + subrsOff);
                    (cff._localSubrs, _) = ReadIndex(sr);
                }
            }
        }

        public void EmitGlyph(byte[] data, ushort glyphIndex, IGlyphSink sink)
        {
            byte[]? cs = ReadIndexEntry(data, _charStringsAbs, glyphIndex);
            if (cs is null) return;
            var interp = new CffInterp(_data, cs, this);
            interp.Run(sink);
        }

        // ---- CFF INDEX utilities ----
        private static (byte[] bytes, int dataBase) ReadIndex(BeReader r)
        {
            int start = r.Position;
            ushort count = r.ReadUInt16();
            if (count == 0) return (Array.Empty<byte>(), start);
            byte offSize = r.ReadByte();
            var offsets = new int[count + 1];
            for (int i = 0; i <= count; i++) offsets[i] = (int)ReadOffset(r, offSize);
            int dataStart = r.Position;
            int dataLen = offsets[count] - 1;
            if (dataLen < 0 || dataLen > r.Raw.Length) dataLen = 0;
            byte[] bytes = new byte[dataLen];
            Array.Copy(r.Raw, dataStart + offsets[0] - 1, bytes, 0, dataLen);
            r.Skip(dataLen);
            return (bytes, dataStart);
        }
        private static BeReader SkipIndex(BeReader r)
        {
            ushort count = r.ReadUInt16();
            if (count == 0) return r;
            byte offSize = r.ReadByte();
            // The offset array has count+1 entries; skip the first count entries, then read the last one (= data length + 1), and skip that entry itself and the data.
            r.Skip(offSize * count);
            int lastOff = (int)ReadOffset(r, offSize);
            int dataLen = lastOff - 1;
            if (dataLen < 0 || dataLen > r.Raw.Length) dataLen = 0;
            r.Skip(dataLen);
            return r;
        }
        private static uint ReadOffset(BeReader r, byte offSize)
        {
            switch (offSize)
            {
                case 1: return r.ReadByte();
                case 2: return r.ReadUInt16();
                case 3: return (uint)((r.ReadByte() << 16) | (r.ReadByte() << 8) | r.ReadByte());
                case 4: return r.ReadUInt32();
            }
            return r.ReadByte();
        }
        private static byte[]? ReadIndexEntry(byte[] data, int baseOff, int index)
        {
            var r = new BeReader(data, baseOff);
            ushort count = r.ReadUInt16();
            if (index >= count) return null;
            byte offSize = r.ReadByte();
            var offsets = new int[count + 1];
            for (int i = 0; i <= count; i++) offsets[i] = (int)ReadOffset(r, offSize);
            int dataStart = r.Position;
            int len = offsets[index + 1] - offsets[index];
            if (len <= 0) return null;
            byte[] bytes = new byte[len];
            Array.Copy(data, dataStart + offsets[index] - 1, bytes, 0, len);
            return bytes;
        }
        private static int CalcBias(byte[]? subrs)
        {
            if (subrs is null || subrs.Length < 2) return 107;
            int n = new BeReader(subrs, 0).ReadUInt16();
            if (n < 1240) return 107;
            if (n < 33900) return 1131;
            return 32768;
        }

        // ---- DICT operator/operand parsing ----
        private static (int op, List<double> ops) ReadDictOp(BeReader r)
        {
            var ops = new List<double>();
            while (r.Position < r.Length)
            {
                byte b0 = r.PeekByte();
                if (b0 == 12) { r.ReadByte(); int b1 = r.ReadByte(); return (1200 + b1, ops); }
                if (b0 <= 21) // single-byte operator (0..21; 28/29/30/31 are already excluded since they are >21)
                {
                    int op = r.ReadByte();
                    return (op, ops);
                }
                if (b0 == 28) { r.ReadByte(); ops.Add(r.ReadInt16()); continue; }
                if (b0 == 29) { r.ReadByte(); ops.Add(r.ReadInt32()); continue; }
                if (b0 == 30) { r.ReadByte(); ops.Add(ReadReal(r)); continue; }
                if (b0 == 31) { r.ReadByte(); continue; }
                if (b0 >= 32 && b0 <= 246) { ops.Add(b0 - 139); r.ReadByte(); continue; }
                if (b0 >= 247 && b0 <= 250) { int v = (b0 - 247) * 256 + r.PeekAt(1) + 108; ops.Add(v); r.Skip(2); continue; }
                if (b0 >= 251 && b0 <= 254) { int v = -(b0 - 251) * 256 - r.PeekAt(1) - 108; ops.Add(v); r.Skip(2); continue; }
                r.ReadByte(); // unknown byte, to prevent an infinite loop
            }
            return (-1, ops);
        }
        private static double ReadReal(BeReader r)
        {
            var sb = new StringBuilder();
            bool done = false;
            while (!done && r.Position < r.Length)
            {
                byte b = r.ReadByte();
                for (int nib = 0; nib < 2 && !done; nib++)
                {
                    int d = (nib == 0) ? (b >> 4) : (b & 0xF);
                    switch (d)
                    {
                        case 0x0: case 0x1: case 0x2: case 0x3: case 0x4:
                        case 0x5: case 0x6: case 0x7: case 0x8: case 0x9: sb.Append(d); break;
                        case 0xA: sb.Append('.'); break;
                        case 0xB: sb.Append('E'); break;
                        case 0xC: sb.Append("E-"); break;
                        case 0xD: done = true; break;
                        case 0xE: sb.Append('-'); break;
                        case 0xF: break;
                    }
                }
            }
            double.TryParse(sb.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out double v);
            return v;
        }

        // ---- Type2 charprogram interpreter ----
        private sealed class CffInterp
        {
            private readonly byte[] _font;
            private byte[] _cs;
            private readonly CffData _cff;
            private readonly List<float> _stack = new();
            private float _x, _y;
            private bool _started;
            private int _pos;
            private int _stems;

            public CffInterp(byte[] font, byte[] cs, CffData cff) { _font = font; _cs = cs; _cff = cff; }

            private byte Peek() => _cs[_pos];
            private float Pop() { if (_stack.Count == 0) return 0; float v = _stack[_stack.Count - 1]; _stack.RemoveAt(_stack.Count - 1); return v; }
            private int Arity(int op) => op switch
            {
                4 => 1, 21 => 1, 22 => 2,
                5 => 2, 6 => 1, 7 => 1, 8 => 6,
                24 => 8, 25 => 8, 26 => 4, 27 => 4, 28 => 4, 29 => 4,
                _ => 0
            };

            public void Run(IGlyphSink sink) { _pos = 0; bool firstMove = true; int widthDropped = 0; _ = widthDropped;
                while (_pos < _cs.Length)
                {
                    byte b0 = _cs[_pos];
                    if (b0 == 28) { _pos++; _stack.Add(ReadInt16()); continue; }
                    if (b0 == 29) { _pos++; _stack.Add(ReadInt32()); continue; }
                    if (b0 >= 32 && b0 <= 246) { _stack.Add(b0 - 139); _pos++; continue; }
                    if (b0 >= 247 && b0 <= 250) { int v = (b0 - 247) * 256 + _cs[_pos + 1] + 108; _stack.Add(v); _pos += 2; continue; }
                    if (b0 >= 251 && b0 <= 254) { int v = -(b0 - 251) * 256 - _cs[_pos + 1] - 108; _stack.Add(v); _pos += 2; continue; }

                    int op;
                    if (b0 == 12) { op = 1200 + _cs[_pos + 1]; _pos += 2; }
                    else { op = b0; _pos++; }

                    bool isMove = op == 21 || op == 22 || op == 4;
                    if (firstMove && isMove && _stack.Count > Arity(op)) _stack.RemoveAt(0); // drop the glyph width

                    switch (op)
                    {
                        case 1: case 18: case 3: case 23: _stems += _stack.Count / 2; _stack.Clear(); break;
                        case 19: case 20: { _stack.Clear(); int mb = (_stems + 7) / 8; _pos += mb; } break;
                        case 4: { float dy = Pop(); _y += dy; Moveto(sink); firstMove = false; } break;
                        case 21: { float dx = Pop(); _x += dx; Moveto(sink); firstMove = false; } break;
                        case 22: { float dy = Pop(); float dx = Pop(); _x += dx; _y += dy; Moveto(sink); firstMove = false; } break;
                        case 5: while (_stack.Count >= 2) { float dy = Pop(); float dx = Pop(); _x += dx; _y += dy; sink.LineTo(_x, _y); } break;
                        case 6: HLineTo(sink); break;
                        case 7: VLineTo(sink); break;
                        case 8: while (_stack.Count >= 6) RrcurvetoOne(sink); break;
                        case 24: while (_stack.Count >= 8) RrcurvetoOne(sink); if (_stack.Count >= 2) { float dy = Pop(), dx = Pop(); _x += dx; _y += dy; sink.LineTo(_x, _y); } break;
                        case 25: while (_stack.Count >= 8) { float dy = Pop(), dx = Pop(); _x += dx; _y += dy; sink.LineTo(_x, _y); } if (_stack.Count >= 6) RrcurvetoOne(sink); break;
                        case 26: HhCurveto(sink); break;
                        case 27: VvCurveto(sink); break;
                        case 28: HvCurveto(sink); break;
                        case 29: VhCurveto(sink); break;
                        case 10: CallSubr(sink, false); break;
                        case 1107: CallSubr(sink, true); break;
                        case 11: if (_started) sink.Close(); _started = false; _stack.Clear(); return;
                        case 14: if (_started) sink.Close(); _started = false; _stack.Clear(); return;
                        default: _stack.Clear(); break;
                    }
                }
                if (_started) sink.Close();
            }

            private void Moveto(IGlyphSink sink)
            {
                if (_started) sink.Close();
                sink.MoveTo(_x, _y);
                _started = true;
            }
            private void RrcurvetoOne(IGlyphSink sink)
            {
                float dy1 = Pop(), dx1 = Pop(), dy2 = Pop(), dx2 = Pop(), dy3 = Pop(), dx3 = Pop();
                float c1x = _x + dx1, c1y = _y + dy1;
                float c2x = c1x + dx2, c2y = c1y + dy2;
                _x = c2x + dx3; _y = c2y + dy3;
                sink.CubicTo(c1x, c1y, c2x, c2y, _x, _y);
            }
            private void HLineTo(IGlyphSink sink)
            {
                bool horizFirst = (_stack.Count % 2) == 0;
                while (_stack.Count >= 1)
                {
                    if (horizFirst) { float dx = Pop(); _x += dx; }
                    else { float dy = Pop(); _y += dy; }
                    sink.LineTo(_x, _y);
                    horizFirst = !horizFirst;
                }
            }
            private void VLineTo(IGlyphSink sink)
            {
                bool vertFirst = (_stack.Count % 2) == 0;
                while (_stack.Count >= 1)
                {
                    if (vertFirst) { float dy = Pop(); _y += dy; }
                    else { float dx = Pop(); _x += dx; }
                    sink.LineTo(_x, _y);
                    vertFirst = !vertFirst;
                }
            }
            private void HhCurveto(IGlyphSink sink)
            {
                bool hasDy = (_stack.Count % 2) == 1;
                while (_stack.Count >= 4)
                {
                    float dy0 = hasDy ? Pop() : 0;
                    float dxa = Pop(), dxb = Pop(), dyb = Pop(), dxc = Pop();
                    float c1x = _x + dxa, c1y = _y + (hasDy ? dy0 : 0);
                    float c2x = c1x + dxb, c2y = c1y + dyb;
                    _x = c2x + dxc; _y = c2y;
                    sink.CubicTo(c1x, c1y, c2x, c2y, _x, _y);
                }
            }
            private void VvCurveto(IGlyphSink sink)
            {
                bool hasDx = (_stack.Count % 2) == 1;
                while (_stack.Count >= 4)
                {
                    float dx0 = hasDx ? Pop() : 0;
                    float dya = Pop(), dxb = Pop(), dyb = Pop(), dyc = Pop();
                    float c1x = _x + (hasDx ? dx0 : 0), c1y = _y + dya;
                    float c2x = c1x + dxb, c2y = c1y + dyb;
                    _x = c2x; _y = c2y + dyc;
                    sink.CubicTo(c1x, c1y, c2x, c2y, _x, _y);
                }
            }
            private void HvCurveto(IGlyphSink sink)
            {
                while (_stack.Count >= 4)
                {
                    float dxa = Pop(), dya = Pop(), dxb = Pop(), dyb = Pop();
                    if ((_stack.Count % 8) < 4)
                    {
                        float c1x = _x + dxa, c1y = _y;
                        float c2x = c1x + dxb, c2y = c1y + dyb;
                        _x = c2x; _y = c2y + dya;
                        sink.CubicTo(c1x, c1y, c2x, c2y, _x, _y);
                    }
                    else
                    {
                        float c1x = _x, c1y = _y + dya;
                        float c2x = c1x + dxb, c2y = c1y + dyb;
                        _x = c2x + dxa; _y = c2y;
                        sink.CubicTo(c1x, c1y, c2x, c2y, _x, _y);
                    }
                }
            }
            private void VhCurveto(IGlyphSink sink)
            {
                while (_stack.Count >= 4)
                {
                    float dxa = Pop(), dya = Pop(), dxb = Pop(), dyb = Pop();
                    if ((_stack.Count % 8) < 4)
                    {
                        float c1x = _x, c1y = _y + dya;
                        float c2x = c1x + dxb, c2y = c1y + dyb;
                        _x = c2x + dxa; _y = c2y;
                        sink.CubicTo(c1x, c1y, c2x, c2y, _x, _y);
                    }
                    else
                    {
                        float c1x = _x + dxa, c1y = _y;
                        float c2x = c1x + dxb, c2y = c1y + dyb;
                        _x = c2x; _y = c2y + dya;
                        sink.CubicTo(c1x, c1y, c2x, c2y, _x, _y);
                    }
                }
            }
            private void CallSubr(IGlyphSink sink, bool global)
            {
                if (_stack.Count == 0) return;
                int idx = (int)Pop() + (global ? _cff._gsubrBias : _cff._subrBias);
                byte[]? subrs = global ? _cff._globalSubrs : _cff._localSubrs;
                if (subrs is null) return;
                byte[]? sub = ReadIndexEntry(subrs, 0, idx);
                if (sub is null) return;
                byte[] savedCs = _cs; int savedPos = _pos; var savedStack = new List<float>(_stack);
                _cs = sub; _pos = 0;
                RunInner(sink);
                _cs = savedCs; _pos = savedPos; _stack.Clear(); _stack.AddRange(savedStack);
            }
            private void RunInner(IGlyphSink sink)
            {
                while (_pos < _cs.Length)
                {
                    byte b0 = _cs[_pos];
                    if (b0 == 28) { _pos++; _stack.Add(ReadInt16()); continue; }
                    if (b0 >= 32 && b0 <= 246) { _stack.Add(b0 - 139); _pos++; continue; }
                    if (b0 >= 247 && b0 <= 250) { int v = (b0 - 247) * 256 + _cs[_pos + 1] + 108; _stack.Add(v); _pos += 2; continue; }
                    if (b0 >= 251 && b0 <= 254) { int v = -(b0 - 251) * 256 - _cs[_pos + 1] - 108; _stack.Add(v); _pos += 2; continue; }
                    int op;
                    if (b0 == 12) { op = 1200 + _cs[_pos + 1]; _pos += 2; }
                    else { op = b0; _pos++; }
                    switch (op)
                    {
                        case 11: return;
                        case 10: CallSubr(sink, false); break;
                        case 1107: CallSubr(sink, true); break;
                        case 14: if (_started) sink.Close(); _started = false; _stack.Clear(); return;
                        case 4: { float dy = Pop(); _y += dy; Moveto(sink); } break;
                        case 21: { float dx = Pop(); _x += dx; Moveto(sink); } break;
                        case 22: { float dy = Pop(); float dx = Pop(); _x += dx; _y += dy; Moveto(sink); } break;
                        case 5: while (_stack.Count >= 2) { float dy = Pop(); float dx = Pop(); _x += dx; _y += dy; sink.LineTo(_x, _y); } break;
                        case 8: while (_stack.Count >= 6) RrcurvetoOne(sink); break;
                        case 6: HLineTo(sink); break;
                        case 7: VLineTo(sink); break;
                        case 26: HhCurveto(sink); break;
                        case 27: VvCurveto(sink); break;
                        case 28: HvCurveto(sink); break;
                        case 29: VhCurveto(sink); break;
                        default: _stack.Clear(); break;
                    }
                }
            }
            private short ReadInt16() { short v = (short)(_cs[_pos] << 8 | _cs[_pos + 1]); _pos += 2; return v; }
            private int ReadInt32() { int v = (_cs[_pos] << 24 | _cs[_pos + 1] << 16 | _cs[_pos + 2] << 8 | _cs[_pos + 3]); _pos += 4; return v; }
        }
    }

    #endregion
}

#region ===== big-endian binary reader (zero dependencies)=====

internal sealed class BeReader
{
    private readonly byte[] _b;
    private int _pos;
    public int Position { get => _pos; set => _pos = value; }
    public int Length => _b.Length;
    public byte[] Raw => _b;

    public BeReader(byte[] data) : this(data, 0) { }
    public BeReader(byte[] data, int offset) { _b = data; _pos = offset; }

    public void Skip(int n) => _pos += n;
    public byte PeekByte() => _b[_pos];
    public byte ReadByte() => _b[_pos++];
    public sbyte ReadSByte() => (sbyte)_b[_pos++];
    public byte PeekAt(int off) => _b[_pos + off];
    public ushort ReadUInt16() { ushort v = (ushort)(_b[_pos] << 8 | _b[_pos + 1]); _pos += 2; return v; }
    public short ReadInt16() { short v = (short)(_b[_pos] << 8 | _b[_pos + 1]); _pos += 2; return v; }
    public uint ReadUInt32() { uint v = (uint)(_b[_pos] << 24 | _b[_pos + 1] << 16 | _b[_pos + 2] << 8 | _b[_pos + 3]); _pos += 4; return v; }
    public int ReadInt32() { int v = (_b[_pos] << 24 | _b[_pos + 1] << 16 | _b[_pos + 2] << 8 | _b[_pos + 3]); _pos += 4; return v; }
    public float ReadFixed() { int v = ReadInt32(); return v / 65536f; }
    public short ReadFWord() => ReadInt16();
    public float ReadF2Dot14() { short v = ReadInt16(); return v / 16384f; }
    public string ReadTag()
    {
        char[] c = new char[4];
        for (int i = 0; i < 4; i++) c[i] = (char)_b[_pos++];
        return new string(c);
    }
}

#endregion
#endif
