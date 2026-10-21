#if NET5_0_OR_GREATER
namespace ACadSharp.Text;

/// <summary>
/// Receives glyph outline path commands without allocating a full <c>Outline</c> object.
/// Hot-path API for rasterizers and other consumers (one sink instance per call, not thread-safe).
///
/// <para>TrueType outlines emit only <see cref="QuadTo"/> (quadratic); CFF/Type-2 charprograms emit only
/// <see cref="CubicTo"/> (cubic). A sink consuming both formats must implement all methods.</para>
/// </summary>
public interface IGlyphSink
{
    void MoveTo(float x, float y);
    void LineTo(float x, float y);
    void QuadTo(float cx, float cy, float x, float y);
    void CubicTo(float c1x, float c1y, float c2x, float c2y, float x, float y);
    /// <summary>Called when the current outline closes back to its start point.</summary>
    void Close();
}
#endif
