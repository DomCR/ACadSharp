#if NET5_0_OR_GREATER
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ACadSharp.Text;

/// <summary>One of the three layouts used by SHX files, determined by the file header text.</summary>
public enum ShxFormat
{
    /// <summary><c>AutoCAD-86 unifont 1.0</c> — a text font indexed by Unicode code point.
    /// Every stock Western font (txt, romans, isocp, …) is a unifont.</summary>
    Unifont,

    /// <summary><c>AutoCAD-86 bigfont 1.0</c> — a text font indexed by double-byte CJK codes,
    /// whose header declares the lead-byte ranges of that encoding.</summary>
    BigFont,

    /// <summary><c>AutoCAD-86 shapes 1.0</c> — a shape (symbol) library indexed by <b>shape number</b>,
    /// the kind complex linetypes reference (e.g. ltypeshp.shx or a site symbol set).</summary>
    Shapes,
}

/// <summary>
/// Which set of glyph instructions to run. SHX encodes vertical glyphs inside the <em>same</em> glyph (gated by the <c>0x0E</c> instruction),
/// rather than in separate glyphs like <c>vmtx</c>/GSUB would.
/// </summary>
public enum ShxTextOrientation
{
    /// <summary>Horizontal text: every <c>0x0E</c>-marked instruction is skipped.</summary>
    Horizontal,

    /// <summary>Vertical text: <c>0x0E</c>-marked instructions run as usual, so <c>0x0E</c> itself is a no-op.</summary>
    Vertical,
}

/// <summary>
/// A loaded AutoCAD <c>.shx</c> shape font — the format used by DWG text styles and complex linetypes,
/// and the reason SHX text in a DWG is exported as bare path geometry (no font object, no <c>/ToUnicode</c>).
///
/// <para>Deliberately kept separate from <see cref="OpenTypeFont"/>: SHX shares no tables, <c>cmap</c> or SFNT structure with it,
/// and unlike any other format this library reads it is <b>stroked</b>, not filled — see <see cref="IsStroked"/>.</para>
///
/// <para>Shape libraries are supported as well.</para>
///
/// <para><b>Portable reimplementation with zero external dependencies.</b> The original implementation came from Fonts.Lib (SharpAstro.Fonts.Shx);
/// it has been moved into the core ACadSharp library so that image (PNG/SVG/PDF) and complex-linetype export no longer depend on the external Fonts.Lib package.
/// The parsing logic and the glyph interpreter are byte-for-byte aligned with the original, with pixel-identical output.</para>
///
/// <para>Immutable once constructed; safe to read concurrently.</para>
/// </summary>
public sealed class ShxFont
{
    // code -> raw record bytes (glyph name + instruction stream). Never mutated after construction, so concurrent reads are safe.
    // For shapes the key is the shape number (from the index table); for text fonts the key is the character code.
    private readonly Dictionary<int, byte[]> _glyphs;

    // name (lowercased, whitespace-trimmed) -> code; used by <see cref="ShxFormat.Shapes"/> only.
    private readonly Dictionary<string, int>? _names;

    /// <summary>How many bytes to scan forward in the file looking for the <c>0x1A</c> header terminator.</summary>
    private const int HeaderScanLimit = 40;

    /// <summary>The layout this file uses.</summary>
    public ShxFormat Format { get; }

    /// <summary>The file header line (terminator removed), e.g. <c>AutoCAD-86 unifont 1.0</c>.</summary>
    public string Header { get; }

    /// <summary>The face name from the font-definition record (not the file name).</summary>
    public string Name { get; }

    /// <summary>Ascent in font units, from the font-definition record.</summary>
    public int Above { get; }

    /// <summary>Descent in font units, from the font-definition record.</summary>
    public int Below { get; }

    /// <summary>The <c>modes</c> byte. 0 or 2 (in 513/520 samples); 2 means the font carries vertical glyphs.</summary>
    public int Modes { get; }

    /// <summary>True when the font carries <c>0x0E</c>-gated vertical variants.</summary>
    public bool HasVerticalForms => Modes != 0;

    /// <summary>The em, i.e. <see cref="Above"/> + <see cref="Below"/>. SHX has no unitsPerEm field, no hmtx and no kerning;
    /// this is the only scale reference the format declares. Corrupt fonts can be 0.</summary>
    public int UnitsPerEm => Above + Below;

    /// <summary>Always true. SHX glyph geometry is a <b>zero-width pen path</b> — the width comes from the graphics state (the PDF
    /// <c>w</c> operator); there is no filled outline: the bowl of an <c>O</c> is a single stroked arc, not two outlines.</summary>
    public bool IsStroked { get; }

    /// <summary>The lead-byte ranges of the double-byte encoding; empty for <see cref="ShxFormat.Unifont"/>.</summary>
    public (int Start, int End)[] LeadByteRanges { get; }

    /// <summary>All codes defined by this font, ascending.</summary>
    public int[] Codes { get; }

    private ShxFont(ShxFormat format, string header, string name, int above, int below,
        int modes, (int Start, int End)[] leadByteRanges,
        Dictionary<int, byte[]> glyphs, Dictionary<string, int>? names = null)
    {
        Format = format;
        Header = header;
        Name = name;
        Above = above;
        Below = below;
        Modes = modes;
        IsStroked = true;
        LeadByteRanges = leadByteRanges;
        _glyphs = glyphs;
        _names = names;
        var codes = new List<int>(glyphs.Keys);
        codes.Sort();
        Codes = codes.ToArray();
    }

    /// <inheritdoc cref="Load"/>
    public static ShxFont LoadFromFile(string path) => Load(File.ReadAllBytes(path));

    /// <summary>
    /// Loads an SHX file from raw bytes — a unifont/bigfont text font, or a shapes symbol library.
    /// For shapes the returned font is indexed by shape number; resolve a name first via <see cref="TryGetShapeNumber"/>.
    /// </summary>
    /// <exception cref="InvalidDataException">The file is not SHX, or its header fields do not fit in the file.</exception>
    public static ShxFont Load(ReadOnlySpan<byte> data)
    {
        var (header, offset) = ReadHeader(data);
        var lower = header.ToLowerInvariant();

        if (lower.Contains("shapes", StringComparison.Ordinal))
            return LoadShapes(data, header, offset);

        if (lower.Contains("unifont", StringComparison.Ordinal))
            return LoadUnifont(data, header, offset);

        if (lower.Contains("bigfont", StringComparison.Ordinal))
            return LoadBigFont(data, header, offset);

        throw new InvalidDataException(
            $"Not a recognised SHX font: header '{header}' is none of unifont, bigfont or shapes.");
    }

    /// <summary>
    /// The file header is ASCII, terminated by <c>0x1A</c>. Its length is <em>not</em> fixed — 25 bytes for unifont/bigfont,
    /// 24 for shapes, 23 for files that use a bare <c>\n</c>, and 26 for one font that styles itself as <c>AutoCAD-586</c> —
    /// so the terminator is scanned for rather than assuming a fixed offset.
    /// </summary>
    private static (string Text, int End) ReadHeader(ReadOnlySpan<byte> data)
    {
        var limit = Math.Min(data.Length, HeaderScanLimit);
        var mark = limit > 0 ? data[..limit].IndexOf((byte)0x1A) : -1;
        if (mark < 0)
            throw new InvalidDataException(
                "Not an SHX file: no 0x1A header terminator in the first 40 bytes.");
        var text = Encoding.ASCII.GetString(data[..mark]).TrimEnd('\r', '\n');
        return (text, mark + 1);
    }

    /// <summary>
    /// unifont: a count first, then the font-definition record, then the glyph records stored inline.
    /// 154 of 170 samples tile exactly to EOF; the other 16 carry a trailing 48-byte ASCII GUID
    /// watermark written by some authoring tool, so trailing extra bytes are ignored rather than treated as a parse error.
    /// </summary>
    private static ShxFont LoadUnifont(ReadOnlySpan<byte> data, string header, int offset)
    {
        if (offset + 6 > data.Length)
            throw new InvalidDataException("Truncated unifont header.");

        // The count includes the font-definition record.
        var count = BinaryPrimitives.ReadUInt32LittleEndian(data[offset..]);
        offset += 4;
        var defLength = BinaryPrimitives.ReadUInt16LittleEndian(data[offset..]);
        offset += 2;

        if (offset + defLength > data.Length)
            throw new InvalidDataException("unifont font-definition record runs past EOF.");
        var (name, above, below, modes) = ReadFontDefinition(data.Slice(offset, defLength));
        offset += defLength;

        var glyphs = new Dictionary<int, byte[]>();
        for (var n = 1u; n < count; n++)
        {
            // Bounds-check instead of trusting: real fonts contain truncated records, and keeping the parsed part beats throwing mid-way.
            if (offset + 4 > data.Length) break;
            var code = BinaryPrimitives.ReadUInt16LittleEndian(data[offset..]);
            var length = BinaryPrimitives.ReadUInt16LittleEndian(data[(offset + 2)..]);
            offset += 4;
            if (offset + length > data.Length) break;
            glyphs[code] = data.Slice(offset, length).ToArray();
            offset += length;
        }

        return new ShxFont(ShxFormat.Unifont, header, name, above, below, modes,
            Array.Empty<(int, int)>(), glyphs);
    }

    /// <summary>
    /// bigfont: the lead-byte ranges first, then an index table of <c>(uint16 code, uint16 length, uint32 offset)</c>
    /// entries pointing at a contiguous data area — unlike the inline records of unifont. 358 of 362 samples have the index table
    /// and the data area byte-adjacent; the other 4 are corrupt files whose entry offsets run past EOF, so each entry is range-checked and dropped individually.
    /// </summary>
    private static ShxFont LoadBigFont(ReadOnlySpan<byte> data, string header, int offset)
    {
        if (offset + 6 > data.Length)
            throw new InvalidDataException("Truncated bigfont header.");

        // The first field is 8 in 350 of 362 samples and 0 in the other 12; its purpose is unconfirmed, so it is not relied on here.
        offset += 2;
        var count = BinaryPrimitives.ReadUInt16LittleEndian(data[offset..]);
        offset += 2;
        var rangeCount = BinaryPrimitives.ReadUInt16LittleEndian(data[offset..]);
        offset += 2;

        if (offset + rangeCount * 4 > data.Length)
            throw new InvalidDataException("bigfont lead-byte ranges run past EOF.");
        var ranges = new List<(int Start, int End)>(rangeCount);
        for (var i = 0; i < rangeCount; i++)
        {
            var start = BinaryPrimitives.ReadUInt16LittleEndian(data[offset..]);
            var end = BinaryPrimitives.ReadUInt16LittleEndian(data[(offset + 2)..]);
            offset += 4;
            ranges.Add((start, end));
        }

        const int IndexEntrySize = 8;
        var name = string.Empty;
        int above = 0, below = 0, modes = 0;
        var glyphs = new Dictionary<int, byte[]>();

        for (var i = 0; i < count; i++)
        {
            var entry = offset + i * IndexEntrySize;
            if (entry + IndexEntrySize > data.Length) break;
            var code = BinaryPrimitives.ReadUInt16LittleEndian(data[entry..]);
            var length = BinaryPrimitives.ReadUInt16LittleEndian(data[(entry + 2)..]);
            var dataOffset = BinaryPrimitives.ReadUInt32LittleEndian(data[(entry + 4)..]);

            if (length == 0) continue;
            if (dataOffset > (uint)data.Length || dataOffset + length > (uint)data.Length)
                continue;
            var record = data.Slice((int)dataOffset, length);

            // Code 0 is the font definition, not a glyph.
            if (code == 0)
            {
                (name, above, below, modes) = ReadFontDefinition(record);
                continue;
            }
            glyphs[code] = record.ToArray();
        }

        return new ShxFont(ShxFormat.BigFont, header, name, above, below, modes,
            ranges.ToArray(), glyphs);
    }

    /// <summary>
    /// shapes: <c>first</c>, <c>last</c> and <c>count</c> as uint16, then an index of
    /// <c>(uint16 shape number, uint16 record size)</c> pairs, then the records laid out contiguously in index order.
    /// Each record is a NUL-terminated shape name followed by an instruction program (itself NUL-terminated, possibly padded out to the record size). There is no font-definition record,
    /// and hence no em: shapes are scaled by whatever scale the consumer applies (a linetype's <c>S=</c> factor).
    ///
    /// <para>Entries are range-checked and dropped individually rather than failing the whole library, consistent with how the other loaders treat corrupt files.</para>
    /// </summary>
    private static ShxFont LoadShapes(ReadOnlySpan<byte> data, string header, int offset)
    {
        if (offset + 6 > data.Length)
            throw new InvalidDataException("Truncated shapes header.");

        // The first and last shape numbers are informational only; the index table below is the authority on which shapes this file actually contains.
        var count = BinaryPrimitives.ReadUInt16LittleEndian(data[(offset + 4)..]);
        offset += 6;

        if (offset + count * 4 > data.Length)
            throw new InvalidDataException("shapes index table runs past EOF.");

        // The index table and the data area are separated: entries point at records in order, so the table base must be captured
        // before the data cursor advances (using one cursor for both table lookup and data fetch would corrupt every entry from the first record on).
        var tableStart = offset;
        var cursor = offset + count * 4;

        var glyphs = new Dictionary<int, byte[]>();
        var names = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < count; i++)
        {
            var table = tableStart + i * 4;
            var number = BinaryPrimitives.ReadUInt16LittleEndian(data[table..]);
            var size = BinaryPrimitives.ReadUInt16LittleEndian(data[(table + 2)..]);
            if (size == 0 || cursor + size > data.Length) continue;

            var record = data.Slice(cursor, size);
            glyphs[number] = record.ToArray();

            // The record's name is its prefix: NUL-terminated ASCII ("" when unnamed).
            var nul = record.IndexOf((byte)0);
            if (nul > 0)
            {
                var name = Encoding.ASCII.GetString(record[..nul]).Trim();
                if (name.Length > 0) names[name] = number;
            }
            cursor += size;
        }

        return new ShxFont(ShxFormat.Shapes, header, header, 0, 0, 0,
            Array.Empty<(int, int)>(), glyphs, names);
    }

    /// <summary>
    /// Resolves a shape's number from its name (case-insensitive). The record prefix of a <see cref="ShxFormat.Shapes"/> library is the name
    /// stored on the .LIN/DXF side, while the DXF side stores the number, so the two lookups have to meet in the middle.
    /// </summary>
    public bool TryGetShapeNumber(string name, out int number)
    {
        number = -1;
        if (string.IsNullOrWhiteSpace(name) || _names == null) return false;
        return _names.TryGetValue(name.Trim(), out number);
    }

    /// <summary>The name stored for the shape at <paramref name="number"/> (empty when unnamed).</summary>
    public string GetShapeName(int number)
    {
        if (!_glyphs.TryGetValue(number, out var record)) return string.Empty;
        var nul = record.AsSpan().IndexOf((byte)0);
        return nul >= 0 ? Encoding.ASCII.GetString(record.AsSpan()[..nul]).Trim() : string.Empty;
    }

    /// <summary>The font-definition record: a NUL-terminated face name followed by the single bytes <c>above</c>, <c>below</c>, <c>modes</c>. Identical in both formats.</summary>
    private static (string Name, int Above, int Below, int Modes) ReadFontDefinition(
        ReadOnlySpan<byte> record)
    {
        var nul = record.IndexOf((byte)0);
        if (nul < 0) return (string.Empty, 0, 0, 0);
        var name = Encoding.ASCII.GetString(record[..nul]);
        var tail = record[(nul + 1)..];
        var above = tail.Length > 0 ? tail[0] : 0;
        var below = tail.Length > 1 ? tail[1] : 0;
        var modes = tail.Length > 2 ? tail[2] : 0;
        return (name, above, below, modes);
    }

    /// <summary>Whether this font defines <paramref name="code"/>.</summary>
    public bool HasGlyph(int code) => _glyphs.ContainsKey(code);

    /// <summary>
    /// Whether <paramref name="b"/> opens a double-byte sequence per <see cref="LeadByteRanges"/>.
    /// Always false for unifont (no ranges).
    /// </summary>
    public bool IsLeadByte(int b)
    {
        foreach (var (start, end) in LeadByteRanges)
            if (b >= start && b <= end) return true;
        return false;
    }

    /// <summary>The raw record bytes for <paramref name="code"/> — the glyph name followed by the instructions.</summary>
    internal bool TryGetRecord(int code, out byte[] record) => _glyphs.TryGetValue(code, out record!);

    /// <summary>
    /// Emits the glyph for <paramref name="code"/> to <paramref name="sink"/> as an <b>open path in font units</b> on the baseline y=0.
    /// Returns false when the font has no such code.
    ///
    /// <para><see cref="IGlyphSink.Close"/> is never called: the result is a pen path, not a closed outline;
    /// feeding it straight to a fill rasterizer yields recognizable but wrong glyphs — self-intersecting with no pen width. For a fillable result
    /// see the approach of <see cref="TryGetStrokedOutline"/> (not carried by this portable version, which ships no stroker/rasterizer).</para>
    /// </summary>
    public bool TryGetGlyph(int code, IGlyphSink sink,
        ShxTextOrientation orientation = ShxTextOrientation.Horizontal)
    {
        ArgumentNullException.ThrowIfNull(sink);
        if (!_glyphs.TryGetValue(code, out var record)) return false;
        ShxShapeInterpreter.Execute(this, record, sink, orientation);
        return true;
    }

    /// <summary>
    /// The advance width for <paramref name="code"/> in font units — the pen X coordinate at figure end, the only
    /// advance SHX declares. No hmtx, no kerning.
    /// </summary>
    public bool TryGetAdvance(int code, out float advance,
        ShxTextOrientation orientation = ShxTextOrientation.Horizontal)
    {
        advance = 0f;
        if (!_glyphs.TryGetValue(code, out var record)) return false;
        advance = ShxShapeInterpreter.Execute(this, record, sink: null, orientation);
        return true;
    }

    /// <summary>
    /// The ink right edge xMax for <paramref name="code"/> in font units (baseline y=0, x to the right; the maximum X over all pen points).
    /// Used to strip the right-side whitespace between advance and ink for wrap decisions (AutoCAD goes by the ink of the last character crossing the limit). Returns false when the glyph is absent.
    /// Note: control points are counted too (a conservative upper bound for Béziers), which makes wrap decisions only slightly conservative (never misses an overflow).
    /// </summary>
    public bool TryGetInkRight(int code, out float inkRight,
        ShxTextOrientation orientation = ShxTextOrientation.Horizontal)
    {
        inkRight = 0f;
        if (!_glyphs.TryGetValue(code, out var record)) return false;
        var sink = new InkMaxXSink();
        ShxShapeInterpreter.Execute(this, record, sink, orientation);
        inkRight = sink.MaxX;
        return true;
    }

    public bool TryGetInkLeft(int code, out float inkLeft,
        ShxTextOrientation orientation = ShxTextOrientation.Horizontal)
    {
        inkLeft = 0f;
        if (!_glyphs.TryGetValue(code, out var record)) return false;
        var sink = new InkMinXSink();
        ShxShapeInterpreter.Execute(this, record, sink, orientation);
        inkLeft = sink.MinX;
        return true;
    }

    /// <summary>A lightweight sink recording only the ink minimum X (for line-start whitespace / ink left-alignment).</summary>
    private sealed class InkMinXSink : IGlyphSink
    {
        public float MinX = float.MaxValue;
        public void MoveTo(float x, float y) { if (x < MinX) MinX = x; }
        public void LineTo(float x, float y) { if (x < MinX) MinX = x; }
        public void QuadTo(float cx, float cy, float x, float y) { if (cx < MinX) MinX = cx; if (x < MinX) MinX = x; }
        public void CubicTo(float c1x, float c1y, float c2x, float c2y, float x, float y)
        {
            if (c1x < MinX) MinX = c1x; if (c2x < MinX) MinX = c2x; if (x < MinX) MinX = x;
        }
        public void Close() { }
    }

    /// <summary>A lightweight sink recording only the ink maximum X (for wrap decisions).</summary>
    private sealed class InkMaxXSink : IGlyphSink
    {
        public float MaxX = float.MinValue;
        public void MoveTo(float x, float y) { if (x > MaxX) MaxX = x; }
        public void LineTo(float x, float y) { if (x > MaxX) MaxX = x; }
        public void QuadTo(float cx, float cy, float x, float y) { if (cx > MaxX) MaxX = cx; if (x > MaxX) MaxX = x; }
        public void CubicTo(float c1x, float c1y, float c2x, float c2y, float x, float y)
        {
            if (c1x > MaxX) MaxX = c1x; if (c2x > MaxX) MaxX = c2x; if (x > MaxX) MaxX = x;
        }
        public void Close() { }
    }
}
#endif
