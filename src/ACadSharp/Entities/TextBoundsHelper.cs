using CSMath;
using System;
using System.Collections.Generic;

#if NET5_0_OR_GREATER

using ACadSharp.Tables;
using ACadSharp.Text;
using System.Collections.Concurrent;
using System.IO;
using System.Text;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Shared, self-contained helpers for estimating the axis-aligned bounding box of
	/// <see cref="TextEntity"/> and <see cref="MText"/> entities.
	/// </summary>
	/// <remarks>
	/// Upstream <c>GetBoundingBox()</c> returned a degenerate 0&times;0 point box (just the insert
	/// point), which is wrong: text occupies a real 2D extent. These helpers reproduce the
	/// conventions the ACadSharp.Formats image renderer already uses so the box matches what is
	/// actually drawn (kept conservative: a slightly large box is safe, a small one clips text):
	/// <list type="bullet">
	/// <item>CJK-aware width estimate under the cap-height convention (H = cap height, TTF
	/// font-size = H·upem/capHeight): CJK advance = 1em ≈ 1.35H, Latin ≈ 0.65H, &times; width factor.</item>
	/// <item>SHX fonts: AutoCAD convention "text height H = cap height = above region", so the
	/// vertical extent is <c>asc = H, desc = 0</c>.</item>
	/// <item>TrueType fonts: ink caps at H (cap) with a Latin descender ≈0.25H below the baseline.</item>
	/// <item>MText line gap = font line-ratio × H × group-44 spacing factor × max in-line
	/// height factor (CJK TTF ≈1.87, Latin TTF ≈1.65, SHX ≈1.67); top 1.15H / bottom 0.3H.</item>
	/// <item>Horizontal anchoring follows group codes 72/73 (TEXT) and 71 (MText attachment);
	/// rotation is applied to the box and the axis-aligned AABB is taken.</item>
	/// </list>
	/// <para>Metrics are two-tiered — the default constants are used only when no font file can be resolved:</para>
	/// <list type="number">
	/// <item>When <see cref="FontFileProvider"/> is injected and the style font resolves to a local
	/// font file, width / vertical / line-spacing all use real font metrics (<see cref="ShxFont"/>
	/// glyph advance / <see cref="TrueTypeFont"/> hmtx advance, Above/Below, Ascent/Descent/LineGap/CapHeight),
	/// using the same font resolution and cap-height convention as the ACadSharp.Formats renderer
	/// (including CJK BigFont GBK double-byte codes, CJK cap 181/175 correction, and BigFont
	/// measured glyph-height correction);</item>
	/// <item>Not injected / file missing / non-text-metrics font (shape library) → falls back to the
	/// default constants above (conservative: slightly large rather than clipped).</item>
	/// </list>
	/// </remarks>
	internal static class TextBoundsHelper
	{
		// ===================== Font file resolution (real metrics preferred) =====================

		/// <summary>
		/// Font file resolution hook (injected once by the host): font name / family name
		/// (style file name, e.g. "simplex.shx", "txt", "SimSun", "arial.ttf") → full local
		/// font file path; null / empty / file not found = unresolved.
		/// Same semantics as the renderer's <c>ImageConfiguration.FileProvider</c> (a downstream
		/// consumer strips the extension to match the SHX search directory, plus
		/// PdfFontManager.FindTtf to resolve TTF). Not injected → all metrics use the default
		/// constants.
		/// Note: font metrics are cached per resolution result; changing the hook after the first
		/// text bounding box is computed will not hot-update.
		/// </summary>
		public static Func<string, string> FontFileProvider { get; set; }

		private static readonly ConcurrentDictionary<string, FontCtx> _fontCtxCache = new(StringComparer.OrdinalIgnoreCase);
		// SHX glyph advance (font units) cached by (font, codepoint) — TryGetAdvance runs the
		// shape interpreter each call; cached lookup is O(1)
		private static readonly ConcurrentDictionary<(ShxFont, int), double> _shxAdvCache = new();
		// BigFont measured max Y (font units) cached by font name — one pass over every glyph
		// (same as the renderer's MeasuredShxAscent)
		private static readonly ConcurrentDictionary<string, double> _shxMaxYCache = new(StringComparer.Ordinal);
		private static Encoding _gbk;
		private static bool _gbkTried;

		/// <summary>GBK encoding (CJK BigFont double-byte codepoints). Returns null when CodePages is unavailable → CJK falls back to constants.</summary>
		private static Encoding GbkEncoding()
		{
			if (_gbkTried) return _gbk;
			_gbkTried = true;
			try
			{
				Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
				_gbk = Encoding.GetEncoding(936);
			}
			catch { _gbk = null; }
			return _gbk;
		}

		/// <summary>
		/// Font metric context for a single text (entity): style Latin font + style big font
		/// (CJK face). No font file resolved → <see cref="FontCtx.HasFont"/> = false, all ratios
		/// use the default constants.
		/// </summary>
		public static FontCtx ResolveFontCtx(bool shxHint, string latinFontName, string bigFontName)
		{
			string key = "L|" + (shxHint ? "S" : "T") + "|" + (latinFontName ?? "") + "|" + (bigFontName ?? "");
			if (_fontCtxCache.TryGetValue(key, out var cached)) return cached;
			var ctx = CreateFontCtx(shxHint, latinFontName, bigFontName);
			_fontCtxCache[key] = ctx;
			return ctx;
		}

		/// <summary>MTEXT inline <c>\f</c> run font (no big font — same as the renderer: inline font runs do not inherit the style big font).</summary>
		public static FontCtx ResolveRunFontCtx(string fontName)
			=> ResolveFontCtx(IsShxFont(fontName), fontName, null);

		private static FontCtx CreateFontCtx(bool shxHint, string latinFontName, string bigFontName)
		{
			var ctx = new FontCtx { ShxHint = shxHint };

			// ① Style Latin font (SHX stroke / TTF hmtx)
			string path = ProbeFontPath(latinFontName, shxHint);
			if (path != null)
			{
				bool shx = shxHint;
				if (path.EndsWith(".shx", StringComparison.OrdinalIgnoreCase)) shx = true;
				else if (path.EndsWith(".ttf", StringComparison.OrdinalIgnoreCase)
					|| path.EndsWith(".ttc", StringComparison.OrdinalIgnoreCase)
					|| path.EndsWith(".otf", StringComparison.OrdinalIgnoreCase)) shx = false;
				try
				{
					if (shx)
					{
						var f = ShxFont.LoadFromFile(path);
						// Shape libraries (complex linetype shapes) have no text metrics, not integrated
						if (f != null && (f.Format == ShxFormat.Unifont || f.Format == ShxFormat.BigFont))
							ctx.Shx = f;
					}
					else
					{
						var tf = TrueTypeFont.LoadFromFile(path);
						if (tf != null && tf.UnitsPerEm > 0)
						{
							ctx.Ttf = tf;
						// Cap-height font-size mapping: fs = H × upem / denom; denom = sCapHeight (falls back to em if missing).
						// CJK fonts (SimSun, etc.): AutoCAD equivalent cap ≈ sCapHeight × 181/175 (+3.5%, empirically calibrated),
						// exactly matching the renderer's TtfCapScale; Latin fonts are not corrected.
							double denom = tf.CapHeight > 0 ? tf.CapHeight : tf.UnitsPerEm;
							if (IsCjkTtfFamily(CanonicalTtfFamily(latinFontName))) denom *= 181.0 / 175.0;
							ctx.CapScale = tf.UnitsPerEm / denom;
							if (ctx.CapScale < 0.5) ctx.CapScale = 0.5;
							if (ctx.CapScale > 2.5) ctx.CapScale = 2.5;
							// Line spacing placeholder = (Asc − Desc + 2×LineGap) / cap (same as renderer's MTextFontLineRatio, no CJK correction)
							double lrDenom = tf.CapHeight > 0 ? tf.CapHeight : tf.UnitsPerEm;
							double lr = (tf.Ascent - tf.Descent + 2 * tf.LineGap) / lrDenom;
							ctx.TtfLineRatio = (lr > 0.5 && lr < 4.0) ? lr : 0;
							ctx.TtfDescFrac = Math.Abs(tf.Descent) / denom;
						}
					}
				}
				catch { }
			}

			// ② Style big font (CJK face, GBK double-byte code) — only needed for the SHX path
			if (ctx.Shx != null && !string.IsNullOrWhiteSpace(bigFontName))
			{
				string bpath = ProbeFontPath(bigFontName, true);
				if (bpath != null)
				{
					try
					{
						var bf = ShxFont.LoadFromFile(bpath);
						if (bf != null && bf.Format == ShxFormat.BigFont) ctx.BigFont = bf;
					}
					catch { }
				}
			}

			// Fill in default constants for the parts that could not be resolved
			ctx.Defaults();
			return ctx;
		}

		/// <summary>
		/// Font file probe: original name → canonicalized TTF family → with extension appended
		/// (.shx/.ttf); the first file that exists wins.
		/// </summary>
		private static string ProbeFontPath(string fontName, bool shxHint)
		{
			var provider = FontFileProvider;
			if (provider == null || string.IsNullOrWhiteSpace(fontName)) return null;
			string name = fontName.Trim();
			var cands = new List<string>(4);
			void Add(string s) { if (!string.IsNullOrEmpty(s) && !cands.Contains(s)) cands.Add(s); }
			Add(name);
			if (!shxHint)
			{
				Add(CanonicalTtfFamily(name));
				Add(CanonicalTtfFamily(name) + ".ttf");
			}
			Add(name + (shxHint ? ".shx" : ".ttf"));
			foreach (var c in cands)
			{
				string p = null;
				try { p = provider(c); } catch { }
				if (!string.IsNullOrEmpty(p) && File.Exists(p)) return p;
			}
			return null;
		}

		/// <summary>
		/// Canonical family name normalization (mirrors the renderer's ResolveFontFamily →
		/// CanonicalTtfFamily mapping subset): strip extension + Chinese name / alias → canonical
		/// family name (arial→Arial, the SimSun Chinese alias→SimSun, …).
		/// </summary>
		private static string CanonicalTtfFamily(string fam)
		{
			if (string.IsNullOrEmpty(fam)) return fam;
			int dot = fam.LastIndexOf('.');
			if (dot > 0) fam = fam.Substring(0, dot);
			switch (fam.ToLowerInvariant())
			{
				case "arial": return "Arial";
				case "helvetica": return "Helvetica";
				case "times new roman": case "times": return "Times New Roman";
				case "courier new": case "courier": return "Courier New";
				case "calibri": return "Calibri";
				case "simhei": case "黑体": return "SimHei";
				case "simsun": case "宋体": return "SimSun";
				case "dengxian": case "等线": return "DengXian";
				case "kaiti": case "楷体": case "楷体_gb2312": return "KaiTi";
				case "fangsong": case "仿宋": case "仿宋_gb2312": return "FangSong";
				case "simli": case "隶书": return "SimLi";
				case "microsoft yahei": case "微软雅黑": return "Microsoft YaHei";
				default:
					return fam.Length == 0 ? fam : char.ToUpperInvariant(fam[0]) + fam.Substring(1);
			}
		}

		private static readonly HashSet<string> _cjkTtfFamilies = new(StringComparer.OrdinalIgnoreCase)
			{ "SimHei", "SimSun", "DengXian", "KaiTi", "FangSong", "SimLi", "Microsoft YaHei" };

		/// <summary>CJK font family check (mirrors the renderer's PdfFontManager.IsCjkFont: the 181/175 cap correction applies only to CJK fonts).</summary>
		private static bool IsCjkTtfFamily(string family)
		{
			if (string.IsNullOrWhiteSpace(family)) return false;
			string first = family.Split(',')[0].Trim();
			if (first.Length == 0) return false;
			if (_cjkTtfFamilies.Contains(first)) return true;
			string l = first.ToLowerInvariant();
			return l.Contains("simsun") || l.Contains("simhei") || l.Contains("msyh")
				|| l.Contains("宋体") || l.Contains("黑体") || l.Contains("雅黑")
				|| l.Contains("楷体") || l.Contains("仿宋") || l.Contains("等线") || l.Contains("隶书");
		}

		/// <summary>SHX glyph advance (font units), cached by (font, codepoint).</summary>
		private static double ShxAdvanceCached(ShxFont face, int code)
		{
			var key = (face, code);
			if (_shxAdvCache.TryGetValue(key, out double a)) return a;
			a = 0;
			try
			{
				if (!face.TryGetAdvance(code, out float advF)) advF = 0f;
				a = advF;
			}
			catch { a = 0; }
			_shxAdvCache[key] = a;
			return a;
		}

		/// <summary>
		/// Denominator for SHX font-units → world-units (H=1): Unifont uses Above (or em if
		/// absent); BigFont uses max(Above, measured glyph max Y) — CJK big glyphs may exceed
		/// the declared Above (hztxt Above=9 but glyphs reach 10.58); same correction as the
		/// renderer's ShxHeightScale.
		/// </summary>
		private static double ShxDenom(ShxFont face)
		{
			int above = face.Above;
			if (face.Format == ShxFormat.BigFont && above > 0)
			{
				double my = MeasuredMaxY(face);
				if (my > above) return my;
			}
			return above > 0 ? above : (face.UnitsPerEm > 0 ? face.UnitsPerEm : 0);
		}

		/// <summary>BigFont measured max glyph Y (font units, cached by font name) — one pass over every glyph.</summary>
		private static double MeasuredMaxY(ShxFont face)
		{
			string key = face.Name ?? "";
			if (_shxMaxYCache.TryGetValue(key, out double my)) return my;
			my = 0;
			try
			{
				var sink = new MaxYSink();
				foreach (int code in face.Codes)
				{
					if (face.TryGetGlyph(code, sink, ShxTextOrientation.Horizontal) && sink.MaxY > my)
						my = sink.MaxY;
				}
			}
			catch { my = 0; }
			_shxMaxYCache[key] = my;
			return my;
		}

		private sealed class MaxYSink : IGlyphSink
		{
			public float MaxY = float.MinValue;
			public void MoveTo(float x, float y) { if (y > MaxY) MaxY = y; }
			public void LineTo(float x, float y) { if (y > MaxY) MaxY = y; }
			public void QuadTo(float cx, float cy, float x, float y) { if (cy > MaxY) MaxY = cy; if (y > MaxY) MaxY = y; }
			public void CubicTo(float c1x, float c1y, float c2x, float c2y, float x, float y)
			{ if (c1y > MaxY) MaxY = c1y; if (c2y > MaxY) MaxY = c2y; if (y > MaxY) MaxY = y; }
			public void Close() { }
		}

		/// <summary>
		/// Font metric context: when a real font is resolved (<see cref="HasFont"/>), width /
		/// vertical / line-spacing use font-file metrics; otherwise the default constants are used.
		/// All ratios are in the H=1 convention (× H gives world units; width factor / \W / \T
		/// not included).
		/// </summary>
		public sealed class FontCtx
		{
			internal bool ShxHint;
			internal ShxFont Shx;
			internal ShxFont BigFont;
			internal TrueTypeFont Ttf;
			internal double CapScale = 1.0;    // TTF: fs = H × CapScale
			internal double TtfLineRatio;      // 0 = invalid (fall back to constants)
			internal double TtfDescFrac;
			internal double LatinFrac = 0.65;
			internal double CjkFrac = 1.35;
			internal double AscFrac = 1.0;
			internal double DescFrac = 0.25;

			/// <summary>Whether a real font file was resolved (false = all default constants).</summary>
			public bool HasFont => Shx != null || Ttf != null || BigFont != null;

			/// <summary>Fill in default constants for the parts that could not be resolved (cap-height convention).</summary>
			internal void Defaults()
			{
				LatinFrac = ShxHint ? 0.72 : 0.65;
				CjkFrac = 1.35;
				AscFrac = 1.0; // cap top = H (both SHX above×s and TTF cap×s equal H)
				if (Ttf != null)
					DescFrac = TtfDescFrac;                       // real: |Descent|×s
				else if (Shx != null)
					DescFrac = ShxDenom(Shx) > 0 ? Shx.Below / ShxDenom(Shx) : 0.0; // real: Below×s
				else
					DescFrac = ShxHint ? 0.0 : 0.25;              // default constants
			}

			/// <summary>Single-character world width / H (after cap-height mapping).</summary>
			public double CharFrac(char ch)
			{
				bool cjk = IsCjkChar(ch);
				if (Ttf != null)
				{
					// hmtx advance (font units) → em × capScale; missing glyph → 1.0em/0.55em (same as renderer's TtfCharAdvanceEm)
					float adv = Ttf.GetAdvanceWidth((int)ch);
					if (adv > 0 && Ttf.UnitsPerEm > 0)
						return adv / Ttf.UnitsPerEm * CapScale;
					return (cjk ? 1.0 : 0.55) * CapScale;
				}
				if (Shx != null)
				{
					if (!cjk) return ShxFaceCharFrac(Shx, ch);
					// CJK → big font face (GBK double-byte code); no bigfont/encoding → constant (renderer also falls back to TTF width estimate)
					if (BigFont != null)
					{
						double? w = BigFontCjkFrac(ch);
						if (w != null) return w.Value;
					}
					return CjkFrac;
				}
				if (BigFont != null && cjk)
				{
					double? w = BigFontCjkFrac(ch);
					if (w != null) return w.Value;
				}
				return cjk ? CjkFrac : LatinFrac;
			}

			/// <summary>World width / H of code on an SHX face (latin or bigfont): advance × s, BigFont char width = max(advance, measured glyph height), missing glyph 0.7em.</summary>
			private double ShxFaceCharFrac(ShxFont face, int code, char ch)
			{
				bool cjk = IsCjkChar(ch);
				double advU = ShxAdvanceCached(face, code);
				if (face.Format == ShxFormat.BigFont)
				{
					double my = MeasuredMaxY(face);
					if (my > advU) advU = my;   // ShxGlyphAdvance: CJK full-width cell = max(advance, measured glyph height)
				}
				if (advU <= 0) advU = 0.7 * (face.UnitsPerEm > 0 ? face.UnitsPerEm : 1); // missing glyph 0.7em (same as renderer)
				double denom = ShxDenom(face);
				if (denom <= 0) return cjk ? CjkFrac : LatinFrac;
				return advU / denom;
			}

			private double ShxFaceCharFrac(ShxFont face, char ch) => ShxFaceCharFrac(face, ch, ch);

			private double? BigFontCjkFrac(char ch)
			{
				var gbk = GbkEncoding();
				if (gbk == null) return null;
				byte[] gb;
				try { gb = gbk.GetBytes(ch.ToString()); }
				catch { return null; }
				if (gb.Length != 2) return null;
				return ShxFaceCharFrac(BigFont, (gb[0] << 8) | gb[1], ch);
			}

			/// <summary>
			/// Single-space line-height placeholder ratio (line height = ratio × H): TTF =
			/// (Asc − Desc + 2×LineGap) / cap, SHX Unifont = (Above + 2×Below) / Above (both
			/// mirror the renderer's MTextFontLineRatio); when no real metrics are available,
			/// a constant is chosen based on <paramref name="textHasCjk"/> (1.87/1.65/1.67).
			/// </summary>
			public double LineRatio(bool textHasCjk)
			{
				if (Ttf != null && TtfLineRatio > 0) return TtfLineRatio;
				if (Shx != null && Shx.Format == ShxFormat.Unifont && Shx.Above > 0)
				{
					double r = (double)(Shx.Above + 2 * Shx.Below) / Shx.Above;
					if (r > 0.5 && r < 4.0) return r;
				}
				return ShxHint ? 1.67 : (textHasCjk ? 1.87 : 1.65);
			}
		}

		/// <summary>
		/// Known SHX font names (no extension form). Anything else without an explicit
		/// <c>.shx</c> extension is treated as a TrueType family.
		/// </summary>
		private static readonly HashSet<string> _knownShxNames =
			new(StringComparer.OrdinalIgnoreCase)
			{
				"txt", "simplex", "hztxt", "romans", "aaa", "cass", "fs", "khz",
				"rd", "rs", "intecad", "ltypeshp", "bigfont"
			};

		/// <summary>
		/// True if the given style font resolves to an SHX (shape) font, i.e. it renders through
		/// the cap-height = H convention.
		/// </summary>
		public static bool IsShxFont(string fontName)
		{
			if (string.IsNullOrWhiteSpace(fontName))
				return false;
			string f = fontName.Trim();
			if (f.EndsWith(".shx", StringComparison.OrdinalIgnoreCase))
				return true;
			if (f.EndsWith(".ttf", StringComparison.OrdinalIgnoreCase)
				|| f.EndsWith(".ttc", StringComparison.OrdinalIgnoreCase))
				return false;
			int dot = f.LastIndexOf('.');
			string fam = dot > 0 ? f.Substring(0, dot) : f;
			return _knownShxNames.Contains(fam.ToLowerInvariant());
		}

		/// <summary>Entity style → (Latin font name, big font name); when the Latin name is empty, falls back to the big font name (same as the renderer's GetStyleFont).</summary>
		private static (string Latin, string Big) StyleFonts(TextStyle style)
		{
			if (style == null) return (null, null);
			string latin = style.Filename;
			string big = style.BigFontFilename;
			if (string.IsNullOrWhiteSpace(latin) && !string.IsNullOrWhiteSpace(big))
				latin = big;
			return (latin, big);
		}

		private static bool IsCjkChar(char ch)
			=> (ch >= 0x2E80 && ch <= 0x9FFF) || (ch >= 0xFF00 && ch <= 0xFFEF)
			   || (ch >= 0x3000 && ch <= 0x303F) || (ch >= 0x20000 && ch <= 0x2FA1F);

		/// <summary>
		/// CJK-aware text width estimate in data units. Under the renderer's cap-height
		/// convention (H = cap height, TTF font-size = H·upem/capHeight): CJK advance = 1em
		/// ≈ 1.35H, Latin ≈ 0.65H, × width factor.
		/// <para>When <paramref name="fontName"/> (the style font file name) is passed and
		/// resolves to a font file, per-character real advance is used (SHX glyph / TTF hmtx);
		/// otherwise the default constants apply.</para>
		/// Does not include the width factor.
		/// </summary>
		public static double EstimateTextWidth(string s, double H, bool shx = false,
			string fontName = null, string bigFontName = null)
		{
			if (string.IsNullOrEmpty(s))
				return 0;
			var ctx = ResolveFontCtx(shx, fontName, bigFontName);
			double w = 0;
			foreach (char ch in s)
				w += ctx.CharFrac(ch);
			return w * H;
		}

		/// <summary>
		/// Vertical extent (ascent above / descent below the baseline) for the given font.
		/// Follows the renderer's cap-height convention: H is the cap height, so TTF ink tops
		/// out at H (cap) with a descender below the baseline.
		/// <para>When the font file resolves, real metrics are used: TTF desc = |Descent|×s
		/// (arial 0.296H, SimSun 0.199H); SHX desc = Below×s (simplex 7/21 = 0.333H, the old
		/// constant 0 would miss the descender); otherwise the default constants apply.</para>
		/// </summary>
		public static (double Asc, double Desc) VerticalExtent(bool shx, double H,
			string fontName = null, string bigFontName = null)
		{
			var ctx = ResolveFontCtx(shx, fontName, bigFontName);
			return (ctx.AscFrac * H, ctx.DescFrac * H);
		}

		/// <summary>
		/// em-box center offset above the baseline (data units), used to vertically center
		/// <c>VerticalAlignment.Middle</c> text on its anchor. SHX box [0,H] centers at 0.5H;
		/// the TTF box [−desc, H] centers at (1−desc)/2·H.
		/// </summary>
		public static double EmBoxCenterOffset(bool shx, double H,
			string fontName = null, string bigFontName = null)
		{
			(double a, double d) = VerticalExtent(shx, H, fontName, bigFontName);
			return (a - d) * 0.5;
		}

		/// <summary>
		/// Model-space axis-aligned bounding box for a <see cref="TextEntity"/>, anchored per
		/// group codes 72/73 and rotated by the entity rotation.
		/// </summary>
		public static BoundingBox TextBox(TextEntity t)
		{
			double H = t.Height > 0 ? t.Height : 1.0;
			double wf = t.WidthFactor > 0 ? t.WidthFactor : 1.0;
			var (fontName, bigName) = StyleFonts(t.Style);
			bool shx = IsShxFont(fontName);
			double W = EstimateTextWidth(t.Value ?? string.Empty, H, shx, fontName, bigName) * wf;
			double rot = t.Rotation; // radians
			(double asc, double desc) = VerticalExtent(shx, H, fontName, bigName);

			// DXF may be missing the 11 group code (AlignmentPoint defaults to XYZ.Zero):
			// for non-left/baseline alignment, fall back the alignment point to the insert
			// point to avoid the anchor drifting to the drawing origin and the whole box
			// being misplaced.
			XYZ alignPt = t.AlignmentPoint == XYZ.Zero ? t.InsertPoint : t.AlignmentPoint;

			// Baseline-left origin (model space), matching the renderer's TextBaseline anchoring.
			XYZ origin;
			double extraRot = 0.0;
			bool baselineLeft = t.HorizontalAlignment == TextHorizontalAlignment.Left
				&& t.VerticalAlignment == TextVerticalAlignmentType.Baseline;
			if (baselineLeft)
			{
				origin = t.InsertPoint;
			}
			else if (t.HorizontalAlignment == TextHorizontalAlignment.Aligned
				|| t.HorizontalAlignment == TextHorizontalAlignment.Fit)
			{
				origin = t.InsertPoint;
				double ddx = alignPt.X - t.InsertPoint.X;
				double ddy = alignPt.Y - t.InsertPoint.Y;
				if (ddx * ddx + ddy * ddy > 1e-18)
					extraRot = Math.Atan2(ddy, ddx);
			}
			else
			{
				double dx = t.HorizontalAlignment switch
				{
					TextHorizontalAlignment.Center => -W / 2.0,
					TextHorizontalAlignment.Middle => -W / 2.0,
					TextHorizontalAlignment.Right => -W,
					_ => 0.0
				};
				double vcf = EmBoxCenterOffset(shx, H, fontName, bigName) / H; // SHX 0.5, TTF (1−desc)/2
				double dy = t.VerticalAlignment switch
				{
					TextVerticalAlignmentType.Middle => -vcf * H,
					TextVerticalAlignmentType.Top => -H,
					_ => 0.0
				};
				double anchorY = t.VerticalAlignment == TextVerticalAlignmentType.Baseline
					? t.InsertPoint.Y
					: alignPt.Y;
				double ca = Math.Cos(rot), sa = Math.Sin(rot);
				origin = new XYZ(
					alignPt.X + dx * ca - dy * sa,
					anchorY + dx * sa + dy * ca,
					alignPt.Z);
			}

			double totalRot = rot + extraRot;
			var aabb = RotatedAabb(0.0, W, -desc, asc, totalRot);
			double z = t.InsertPoint.Z;
			return new BoundingBox(
				new XYZ(origin.X + aabb.MinX, origin.Y + aabb.MinY, z),
				new XYZ(origin.X + aabb.MaxX, origin.Y + aabb.MaxY, z));
		}

		/// <summary>
		/// Line-spacing placeholder ratio (single line height = ratio × H). Renderer
		/// MTextFontLineRatio measurements: SHX ≈1.667; TTF with CJK ≈1.87 (SimSun 328/175),
		/// Latin ≈1.65 (arial 2422/1467). When the core has no font table, an approximation is
		/// chosen based on whether the text contains CJK; when the font file is available, the
		/// real ratio is used. Final line gap = ratio × H × group-44 spacing factor × max
		/// in-line font-size factor.
		/// </summary>
		public static double LineGapFactor(string text, bool shx,
			string fontName = null, string bigFontName = null)
			=> ResolveFontCtx(shx, fontName, bigFontName).LineRatio(HasCjk(text));

		private static bool HasCjk(string s)
		{
			if (string.IsNullOrEmpty(s)) return false;
			foreach (char ch in s)
				if (IsCjkChar(ch)) return true;
			return false;
		}

		/// <summary>
		/// Model-space axis-aligned bounding box for an <see cref="MText"/> entity: anchored at
		/// the insertion point (group 10), width = max(reference width, content width), height
		/// from the number of wrapped lines. Line gap = font line-ratio × H × group-44 spacing
		/// factor × per-line visible height factor; cap top 1.15H (CJK ink 1.13H) / descender
		/// 0.3H.
		/// Inked height only: trailing <c>\H4x; </c> spaces do not raise the box (AutoCAD
		/// InkMaxHeight convention, same as the renderer).
		/// </summary>
		public static BoundingBox MTextBox(MText m)
		{
			double H = m.Height > 0 ? m.Height : 1.0;
			double refW = m.RectangleWidth > 0 ? m.RectangleWidth : 0.0;
			double rot = m.Rotation; // radians (derived from AlignmentPoint x-axis dir)

			var (fontName, bigName) = StyleFonts(m.Style);
			bool shx = IsShxFont(fontName);
			string raw = m.Value ?? string.Empty;
			var (lines, maxLineW, gapHSum, firstVisH, lastVisH) =
				EstimateMTextLayout(raw, H, refW, shx, fontName, bigName);
			int n = Math.Max(1, lines);
			double boxW = Math.Max(refW, maxLineW); // when a long word overflows the reference box, use the content width

			// Line gap: font placeholder ratio × H × group-44 factor; gapHSum = Σ the "inked"
			// font-size factors of middle lines (trailing large-font spaces do not count)
			double gap = LineGapFactor(raw, shx, fontName, bigName) * H * (m.LineSpacing > 0 ? m.LineSpacing : 1.0);
			double top = 1.15 * H * firstVisH;                    // first-line ink top (CJK 205/181 = 1.13H)
			double bottom = -(gapHSum * gap + 0.3 * H * lastVisH); // last-line descender + margin
			double boxH = top - bottom;

			// Local (layout) coordinate of the attachment point; layout origin is the text
			// block's top-left with y up (top positive, bottom negative).
			double anchorX = 0, anchorY = top;
			switch (m.AttachmentPoint)
			{
				case AttachmentPointType.TopCenter:
				case AttachmentPointType.MiddleCenter:
				case AttachmentPointType.BottomCenter: anchorX = boxW / 2.0; break;
				case AttachmentPointType.TopRight:
				case AttachmentPointType.MiddleRight:
				case AttachmentPointType.BottomRight: anchorX = boxW; break;
			}
			switch (m.AttachmentPoint)
			{
				case AttachmentPointType.MiddleLeft:
				case AttachmentPointType.MiddleCenter:
				case AttachmentPointType.MiddleRight: anchorY = top - boxH / 2.0; break;
				case AttachmentPointType.BottomLeft:
				case AttachmentPointType.BottomCenter:
				case AttachmentPointType.BottomRight: anchorY = bottom; break;
			}

			// Map the 4 local corners (x in [0,boxW], y in [bottom,top]) through
			// world = insert + R(rot) * (corner - anchor) and take the AABB.
			double c = Math.Cos(rot), s = Math.Sin(rot);
			double minX = double.MaxValue, minY = double.MaxValue;
			double maxX = double.MinValue, maxY = double.MinValue;
			double z = m.InsertPoint.Z;
			foreach (double lx in new[] { 0.0, boxW })
			{
				foreach (double ly in new[] { bottom, top })
				{
					double rx = lx - anchorX, ry = ly - anchorY;
					double wx = m.InsertPoint.X + rx * c - ry * s;
					double wy = m.InsertPoint.Y + rx * s + ry * c;
					if (wx < minX) minX = wx;
					if (wx > maxX) maxX = wx;
					if (wy < minY) minY = wy;
					if (wy > maxY) maxY = wy;
				}
			}
			return new BoundingBox(new XYZ(minX, minY, z), new XYZ(maxX, maxY, z));
		}

		/// <summary>
		/// Axis-aligned AABB of a rectangle (local x in [xL, xR], y in [yB, yT]) rotated by
		/// <paramref name="rotRad"/> about the local origin.
		/// </summary>
		public static (double MinX, double MinY, double MaxX, double MaxY) RotatedAabb(
			double xL, double xR, double yB, double yT, double rotRad)
		{
			double c = Math.Cos(rotRad), s = Math.Sin(rotRad);
			double minX = double.MaxValue, minY = double.MaxValue;
			double maxX = double.MinValue, maxY = double.MinValue;
			foreach (double x in new[] { xL, xR })
			{
				foreach (double y in new[] { yB, yT })
				{
					double X = x * c - y * s;
					double Y = x * s + y * c;
					if (X < minX) minX = X;
					if (X > maxX) maxX = X;
					if (Y < minY) minY = Y;
					if (Y > maxY) maxY = Y;
				}
			}
			return (minX, minY, maxX, maxY);
		}

		/// <summary>
		/// Width of the Latin word starting at <paramref name="start"/> (runs to the next space /
		/// format code / group brace / line break), using the current run factors and font
		/// metrics. Used for the word-boundary wrap test (a word is never split across lines).
		/// </summary>
		public static double WordWidth(string s, int start, int len, double baseH,
			double hFac, double wFac, double tFac, FontCtx ctx)
		{
			double w = 0;
			for (int k = start; k < len; k++)
			{
				char c = s[k];
				if (c == ' ' || c == '\\' || c == '{' || c == '}' || c == '~' || c == '\t')
					break;
				w += ctx.CharFrac(c) * baseH * hFac * wFac * tFac;
			}
			return w;
		}

		/// <summary>
		/// Estimate the wrapped layout of a raw MText string at a reference width, honoring the
		/// run-level width (<c>\W</c>), relative/absolute height (<c>\H</c>), tracking
		/// (<c>\T</c>), paragraph breaks (<c>\P</c>) and inline paragraph props (<c>\p…;</c>,
		/// which are NOT line breaks — a lowercase \p carries <c>\psm</c>/<c>\pxsm</c> spacing).
		/// Returns (line count, widest line width, Σ middle-line height factors, first/last
		/// line visible height factors). Height factors only count runs with ink: a trailing
		/// <c>\H4x; </c> space does NOT raise the line height (AutoCAD InkMaxHeight convention).
		/// <para>Width is computed from the current run's font metrics: starting from the style
		/// font, switching on inline <c>\f</c> (including switches inside <c>{…}</c> groups and
		/// group-end restore); runs whose font file cannot be resolved fall back to the default
		/// constants.</para>
		/// </summary>
		public static (int Lines, double MaxWidth, double GapHSum, double FirstVisH, double LastVisH)
			EstimateMTextLayout(string rawMText, double baseH, double refWidth, bool shx,
				string fontName = null, string bigFontName = null)
		{
			if (string.IsNullOrEmpty(rawMText))
				return (0, 0, 0.0, 1.0, 1.0);

			// Style font context; inline \f run switch (no big font — same as the renderer)
			FontCtx styleCtx = ResolveFontCtx(shx, fontName, bigFontName);
			FontCtx curCtx = styleCtx;
			string curFontName = fontName;
			void UseFont(string name)
			{
				curFontName = name;
				curCtx = string.IsNullOrEmpty(name) || name == fontName
					? styleCtx
					: ResolveRunFontCtx(name);
			}

			double hFac = 1.0, wFac = 1.0, tFac = 1.0;

			int lines = 1;
			int len = rawMText.Length;
			double curW = 0, maxW = 0;
			// curInk = "inked width" of the line content (includes inter-word spaces, excludes
			// trailing spaces): used for the wrap decision and maxW, aligned with the engine's
			// "right edge of the last character's ink" test — trailing spaces do not widen the
			// line nor participate in wrapping.
			double curInk = 0;
			// Per-line maximum "inked" font-size factor (a trailing \H4x; space does not raise
			// the line gap — empirically confirmed AutoCAD convention)
			double lineVisH = 1.0, firstVisH = 1.0, lastVisH = 1.0, gapHSum = 0.0;
			// Latin wraps by word (AutoCAD does not split words): only the first character of a
			// word can trigger a wrap; CJK can break per character.
			bool wordStart = true;
			// {…} format-group stack: \H/\W/\T/\f changes inside a group are restored at } to
			// the pre-group values (same semantics as the renderer's ParseMText)
			var fmtStack = new Stack<(double h, double w, double t, string font)>();
			// Visible character feed: width / wrap (trailing-space inked-width test + Latin word
			// break) / line-height statistics.
			// \U+XXXX escape sequences also go through this path (scanFrom is only used for the
			// Latin whole-word width estimate).
			void FeedVisible(char c, int scanFrom)
			{
				if (c != ' ')
					lineVisH = Math.Max(lineVisH, hFac);
				double fw = curCtx.CharFrac(c) * baseH * hFac * wFac * tFac;
				if (c == ' ')
				{
					curW += fw;
					wordStart = true;
					return;
				}
				if (refWidth > 0 && curInk > 1e-9
					&& (IsCjkChar(c) || wordStart)
					&& curInk + WordWidth(rawMText, scanFrom, len, baseH, hFac, wFac, tFac, curCtx)
						> refWidth + 1e-9)
				{
					// CJK can break per character; Latin only at word start, and the test uses
					// the whole-word width (wrap only when the entire word does not fit, matching
					// AutoCAD's no-word-split + line inked width not exceeding the reference box)
					lines++;
					maxW = Math.Max(maxW, curInk);
					gapHSum += lineVisH; lineVisH = 1.0;
					curW = fw;
					curInk = fw;
					wordStart = false;
				}
				else
				{
					curW += fw;
					curInk += fw;
					wordStart = false;
				}
			}

			int i = 0;
			while (i < len)
			{
				char ch = rawMText[i];
				// Format-group braces without a leading backslash: {\fSimSun|b0|i0|c134|p2;text} —
				// { only pushes the pre-group state (the \H/\W/\T/\f quadruple); the \f…; font
				// spec inside the group is parsed and switches the run font in the backslash
				// branch below; the old version consumed the whole segment to ';' here, eating
				// the in-group \f font switch along with it (same as the renderer's ParseMText:
				// { only pushes the stack, spec codes are then parsed separately); '}' pops and
				// restores the pre-group height/width/tracking/font.
				if (ch == '{')
				{
					fmtStack.Push((hFac, wFac, tFac, curFontName));
					i++;
					continue;
				}
				if (ch == '}')
				{
					if (fmtStack.Count > 0)
					{
						(hFac, wFac, tFac, string fname) = fmtStack.Pop();
						UseFont(fname);
					}
					i++;
					continue;
				}
				// ~ and the tab character are both in-paragraph line breaks (same as the renderer)
				if (ch == '~' || ch == '\t')
				{
					lines++; maxW = Math.Max(maxW, curInk); curW = 0; curInk = 0;
					gapHSum += lineVisH; lineVisH = 1.0;
					wordStart = true; i++; continue;
				}
				if (ch == '\\' && i + 1 < len)
				{
					char nx = rawMText[i + 1];
					// Bracketed codes: \{...\}, \[...\], \<...>
					if (nx == '{' || nx == '[' || nx == '<')
					{
						char close = nx == '{' ? '}' : (nx == '[' ? ']' : '>');
						int end = rawMText.IndexOf(close, i + 2);
						i = end < 0 ? len : end + 1;
						continue;
					}
					// Uppercase \P = paragraph break; lowercase \p = inline paragraph properties
					// (\psm/\pxsm…), not a line break.
					if (nx == 'P')
					{
						lines++; maxW = Math.Max(maxW, curInk); curW = 0; curInk = 0;
						gapHSum += lineVisH; lineVisH = 1.0;
						i += 2; continue;
					}
					if (nx == 'p')
					{
						int j = i + 2;
						while (j < len && rawMText[j] != ';' && rawMText[j] != '\\') j++;
						i = (j < len && rawMText[j] == ';') ? j + 1 : j;
						continue;
					}
					if (char.IsLetter(nx))
					{
						switch (char.ToUpperInvariant(nx))
						{
							case 'H': case 'W': case 'T': case 'A': case 'C': case 'Q': case 'K':
								{
									// Value-codes: \H2x; \W1.7; \T0.75; \A1; \C1; ...
									int j = i + 2;
									while (j < len && rawMText[j] != ';' && rawMText[j] != '\\') j++;
									string val = rawMText.Substring(i + 2, j - (i + 2));
									// \H relative (x suffix) = current height × factor; \H without
									// suffix = absolute text height (world units).
									bool rel = val.EndsWith("x") || val.EndsWith("X");
									val = val.Trim().TrimEnd('x', 'X');
									double num;
									if (double.TryParse(val, System.Globalization.NumberStyles.Float,
										System.Globalization.CultureInfo.InvariantCulture, out num))
									{
										switch (char.ToUpperInvariant(nx))
										{
											case 'H': hFac = rel ? hFac * num : num / baseH; break;
											case 'W': wFac = num; break;
											// \T n = adds (n−1)×0.1em per character (tracking), not a
											// direct multiplication by n.
											case 'T': tFac = 1.0 + (num - 1.0) * 0.1; break;
										}
									}
									i = (j < len && rawMText[j] == ';') ? j + 1 : j;
									continue;
								}
							case 'F':
								{
									// \f<fontname>|b1|i0|c134|p2; — font name = everything before
									// the first '|'; empty font name = fall back to the style font
									// (same as the renderer's ParseMText/MTextRunWidth).
									int j = i + 2;
									while (j < len && rawMText[j] != ';' && rawMText[j] != '\\') j++;
									string arg = rawMText.Substring(i + 2, j - (i + 2));
									int pipe = arg.IndexOf('|');
									string fname = (pipe >= 0 ? arg.Substring(0, pipe) : arg).Trim();
									UseFont(fname.Length > 0 ? fname : null);
									i = (j < len && rawMText[j] == ';') ? j + 1 : j;
									continue;
								}
							case 'U':
								{
									// \U+XXXX: Unicode codepoint escape (the way AutoCAD writes
									// Chinese MText) → a single visible character.
									// The old version fell into default and only skipped \U,
									// treating "+6D4B" as visible characters → inflated width.
									int j = i + 2;
									while (j < len && rawMText[j] != ';' && rawMText[j] != '\\') j++;
									string hex = rawMText.Substring(i + 2, j - (i + 2)).Trim();
									if (hex.StartsWith("+")) hex = hex.Substring(1);
									if (int.TryParse(hex, System.Globalization.NumberStyles.HexNumber,
										System.Globalization.CultureInfo.InvariantCulture, out int cp)
										&& cp > 0 && cp <= 0xFFFF)
									{
										FeedVisible((char)cp, i);
									}
									i = (j < len && rawMText[j] == ';') ? j + 1 : j;
									continue;
								}
							default: // toggle-codes: \L \l \O \o \M ... consume the letter only
								i += 2; continue;
						}
					}
				}
				FeedVisible(ch, i);
				i++;
			}
			maxW = Math.Max(maxW, curInk);
			lastVisH = lineVisH;
			if (lines == 1) firstVisH = lineVisH;
			return (lines, maxW, gapHSum, firstVisH, lastVisH);
		}

	}
}
#else

namespace ACadSharp.Entities
{
	/// <summary>
	/// Shared, self-contained helpers for estimating the axis-aligned bounding box of
	/// <see cref="TextEntity"/> and <see cref="MText"/> entities.
	/// </summary>
	/// <remarks>
	/// Upstream <c>GetBoundingBox()</c> returned a degenerate 0×0 point box (just the insert
	/// point), which is wrong: text occupies a real 2D extent. These helpers reproduce the
	/// conventions the ACadSharp.Formats image renderer already uses so the box matches what is
	/// actually drawn:
	/// <list type="bullet">
	/// <item>CJK-aware width estimate (full-width ≈1.0×H, half-width ≈0.55×H) × width factor.</item>
	/// <item>SHX fonts: AutoCAD convention "text height H = cap height = above region", so the
	/// vertical extent is <c>asc = H, desc = 0</c>.</item>
	/// <item>TrueType fonts: the renderer uses <c>font-size = H</c> (H = em box), so the vertical
	/// extent around the baseline is <c>asc = 0.8H, desc = 0.2H</c>.</item>
	/// <item>Horizontal anchoring follows group codes 72/73 (TEXT) and 71 (MText attachment);
	/// rotation is applied to the box and the axis-aligned AABB is taken.</item>
	/// </list>
	/// </remarks>
	internal static class TextBoundsHelper
	{
		/// <summary>
		/// Known SHX font names (no extension form). Anything else without an explicit
		/// <c>.shx</c> extension is treated as a TrueType family.
		/// </summary>
		private static readonly HashSet<string> _knownShxNames =
			new(StringComparer.OrdinalIgnoreCase)
			{
				"txt", "simplex", "hztxt", "romans", "aaa", "cass", "fs", "khz",
				"rd", "rs", "intecad", "ltypeshp", "bigfont"
			};

		/// <summary>
		/// True if the given style font resolves to an SHX (shape) font, i.e. it renders through
		/// the cap-height = H convention.
		/// </summary>
		public static bool IsShxFont(string fontName)
		{
			if (string.IsNullOrWhiteSpace(fontName))
				return false;
			string f = fontName.Trim();
			if (f.EndsWith(".shx", StringComparison.OrdinalIgnoreCase))
				return true;
			if (f.EndsWith(".ttf", StringComparison.OrdinalIgnoreCase)
				|| f.EndsWith(".ttc", StringComparison.OrdinalIgnoreCase))
				return false;
			int dot = f.LastIndexOf('.');
			string fam = dot > 0 ? f.Substring(0, dot) : f;
			return _knownShxNames.Contains(fam.ToLowerInvariant());
		}

		private static bool IsCjkChar(char ch)
			=> (ch >= 0x2E80 && ch <= 0x9FFF) || (ch >= 0xFF00 && ch <= 0xFFEF)
			   || (ch >= 0x3000 && ch <= 0x303F) || (ch >= 0x20000 && ch <= 0x2FA1F);

		/// <summary>
		/// CJK-aware text width estimate in data units (full-width ≈1.0×H,
		/// half-width ≈0.55×H). Does not include the width factor.
		/// </summary>
		public static double EstimateTextWidth(string s, double H)
		{
			if (string.IsNullOrEmpty(s))
				return 0;
			double w = 0;
			foreach (char ch in s)
				w += IsCjkChar(ch) ? 1.0 : 0.55;
			return w * H;
		}

		/// <summary>
		/// Vertical extent (ascent above / descent below the baseline) for the given font type.
		/// </summary>
		public static (double Asc, double Desc) VerticalExtent(bool shx, double H)
		{
			// SHX: cap height == H, no descender below baseline for the cap-mapped model.
			// TTF: H is the em box. The renderer draws with font-size = H (em), so the box is
			// the em-box. Generic Latin TTF (Arial: asc 0.905em / desc 0.212em) ≈ 0.9 / 0.21;
			// the older 0.8 / 0.2 (total = H) under-sized the box ~11% vs the real em-box, so
			// it is corrected here (the core does not load per-font metrics, so this is the
			// standard Latin approximation).
			return shx ? (H, 0.0) : (0.9 * H, 0.21 * H);
		}

		/// <summary>
		/// em-box center offset above the baseline (data units), used to vertically center
		/// <c>VerticalAlignment.Middle</c> text on its anchor. SHX box [0,H] centers at 0.5H;
		/// the generic TTF box [−0.2H,0.8H] centers at 0.3H.
		/// </summary>
		public static double EmBoxCenterOffset(bool shx, double H)
		{
			(double a, double d) = VerticalExtent(shx, H);
			return (a - d) * 0.5;
		}

		/// <summary>
		/// Model-space axis-aligned bounding box for a <see cref="TextEntity"/>, anchored per
		/// group codes 72/73 and rotated by the entity rotation.
		/// </summary>
		public static BoundingBox TextBox(TextEntity t)
		{
			double H = t.Height > 0 ? t.Height : 1.0;
			double wf = t.WidthFactor > 0 ? t.WidthFactor : 1.0;
			bool shx = IsShxFont(t.Style?.Filename);
			double W = EstimateTextWidth(t.Value ?? string.Empty, H) * wf;
			double rot = t.Rotation; // radians
			(double asc, double desc) = VerticalExtent(shx, H);

			// Baseline-left origin (model space), matching the renderer's TextBaseline anchoring.
			XYZ origin;
			double extraRot = 0.0;
			bool baselineLeft = t.HorizontalAlignment == TextHorizontalAlignment.Left
				&& t.VerticalAlignment == TextVerticalAlignmentType.Baseline;
			if (baselineLeft)
			{
				origin = t.InsertPoint;
			}
			else if (t.HorizontalAlignment == TextHorizontalAlignment.Aligned
				|| t.HorizontalAlignment == TextHorizontalAlignment.Fit)
			{
				origin = t.InsertPoint;
				double ddx = t.AlignmentPoint.X - t.InsertPoint.X;
				double ddy = t.AlignmentPoint.Y - t.InsertPoint.Y;
				if (ddx * ddx + ddy * ddy > 1e-18)
					extraRot = Math.Atan2(ddy, ddx);
			}
			else
			{
				double dx = t.HorizontalAlignment switch
				{
					TextHorizontalAlignment.Center => -W / 2.0,
					TextHorizontalAlignment.Middle => -W / 2.0,
					TextHorizontalAlignment.Right => -W,
					_ => 0.0
				};
				double vcf = EmBoxCenterOffset(shx, H) / H; // SHX 0.5, TTF 0.3
				double dy = t.VerticalAlignment switch
				{
					TextVerticalAlignmentType.Middle => -vcf * H,
					TextVerticalAlignmentType.Top => -H,
					_ => 0.0
				};
				double anchorY = t.VerticalAlignment == TextVerticalAlignmentType.Baseline
					? t.InsertPoint.Y
					: t.AlignmentPoint.Y;
				double ca = Math.Cos(rot), sa = Math.Sin(rot);
				origin = new XYZ(
					t.AlignmentPoint.X + dx * ca - dy * sa,
					anchorY + dx * sa + dy * ca,
					t.AlignmentPoint.Z);
			}

			double totalRot = rot + extraRot;
			var aabb = RotatedAabb(0.0, W, -desc, asc, totalRot);
			double z = t.InsertPoint.Z;
			return new BoundingBox(
				new XYZ(origin.X + aabb.MinX, origin.Y + aabb.MinY, z),
				new XYZ(origin.X + aabb.MaxX, origin.Y + aabb.MaxY, z));
		}

		/// <summary>
		/// Model-space axis-aligned bounding box for an <see cref="MText"/> entity: anchored at
		/// the attachment point, width = max(reference width, content width), height from the
		/// number of wrapped lines at the reference width (3-on-5 line spacing).
		/// </summary>
		public static BoundingBox MTextBox(MText m)
		{
			double H = m.Height > 0 ? m.Height : 1.0;
			double refW = m.RectangleWidth > 0 ? m.RectangleWidth : 0.0;
			double rot = m.Rotation; // radians (derived from AlignmentPoint)

			bool shx = IsShxFont(m.Style?.Filename);
			int n = Math.Max(1, EstimateMTextLines(m.Value ?? string.Empty, H, refW, shx));
			double boxW = refW > 0 ? refW : Math.Max(EstimateTextWidth(m.PlainText ?? string.Empty, H), H);

			double gap = 1.5 * H;
			double top = 0.8 * H;
			double bottom = -((n - 1) * gap + 0.2 * H);
			double boxH = top - bottom;

			// Local (layout) coordinate of the attachment point; layout origin is the text
			// block's top-left with y up (top positive, bottom negative).
			double anchorX = 0, anchorY = top;
			switch (m.AttachmentPoint)
			{
				case AttachmentPointType.TopCenter:
				case AttachmentPointType.MiddleCenter:
				case AttachmentPointType.BottomCenter: anchorX = boxW / 2.0; break;
				case AttachmentPointType.TopRight:
				case AttachmentPointType.MiddleRight:
				case AttachmentPointType.BottomRight: anchorX = boxW; break;
			}
			switch (m.AttachmentPoint)
			{
				case AttachmentPointType.MiddleLeft:
				case AttachmentPointType.MiddleCenter:
				case AttachmentPointType.MiddleRight: anchorY = top - boxH / 2.0; break;
				case AttachmentPointType.BottomLeft:
				case AttachmentPointType.BottomCenter:
				case AttachmentPointType.BottomRight: anchorY = bottom; break;
			}

			// Map the 4 local corners (x in [0,boxW], y in [bottom,top]) through
			// world = insert + R(rot) * (corner - anchor) and take the AABB.
			double c = Math.Cos(rot), s = Math.Sin(rot);
			double minX = double.MaxValue, minY = double.MaxValue;
			double maxX = double.MinValue, maxY = double.MinValue;
			double z = m.InsertPoint.Z;
			foreach (double lx in new[] { 0.0, boxW })
			{
				foreach (double ly in new[] { bottom, top })
				{
					double rx = lx - anchorX, ry = ly - anchorY;
					double wx = m.InsertPoint.X + rx * c - ry * s;
					double wy = m.InsertPoint.Y + rx * s + ry * c;
					if (wx < minX) minX = wx;
					if (wx > maxX) maxX = wx;
					if (wy < minY) minY = wy;
					if (wy > maxY) maxY = wy;
				}
			}
			return new BoundingBox(new XYZ(minX, minY, z), new XYZ(maxX, maxY, z));
		}

		/// <summary>
		/// Axis-aligned AABB of a rectangle (local x in [xL, xR], y in [yB, yT]) rotated by
		/// <paramref name="rotRad"/> about the local origin.
		/// </summary>
		public static (double MinX, double MinY, double MaxX, double MaxY) RotatedAabb(
			double xL, double xR, double yB, double yT, double rotRad)
		{
			double c = Math.Cos(rotRad), s = Math.Sin(rotRad);
			double minX = double.MaxValue, minY = double.MaxValue;
			double maxX = double.MinValue, maxY = double.MinValue;
			foreach (double x in new[] { xL, xR })
			{
				foreach (double y in new[] { yB, yT })
				{
					double X = x * c - y * s;
					double Y = x * s + y * c;
					if (X < minX) minX = X;
					if (X > maxX) maxX = X;
					if (Y < minY) minY = Y;
					if (Y > maxY) maxY = Y;
				}
			}
			return (minX, minY, maxX, maxY);
		}

		/// <summary>
		/// Estimate the number of rendered lines of a raw MText string at a reference width,
		/// honoring the run-level width (<c>\W</c>), relative height (<c>\H</c>), tracking
		/// (<c>\T</c>) factors and paragraph breaks (<c>\P</c>). This is a self-contained
		/// approximation (no SHX glyph table), so it is close to but not identical to the
		/// renderer's exact line breaking; it is used only to size the MText bounding box.
		/// </summary>
		public static int EstimateMTextLines(string rawMText, double baseH, double refWidth, bool shx)
		{
			if (string.IsNullOrEmpty(rawMText))
				return 0;

			// Per-char Latin width as a fraction of H. SHX unifont letters are noticeably wider
			// than proportional TTF (~0.55H); use a generous 0.72H so the estimate does not
			// undercount the wrap (a bounding box that is too short is worse than slightly large).
			double latinFrac = shx ? 0.72 : 0.55;
			double hFac = 1.0, wFac = 1.0, tFac = 1.0;

			int lines = 1;
			double curW = 0;
			int i = 0;
			int len = rawMText.Length;
			while (i < len)
			{
				char ch = rawMText[i];
				if (ch == '\\' && i + 1 < len)
				{
					char nx = rawMText[i + 1];
					// Bracketed codes: \{...\}, \[...\], \&lt;...\&gt;
					if (nx == '{' || nx == '[' || nx == '<')
					{
						char close = nx == '{' ? '}' : (nx == '[' ? ']' : '>');
						int end = rawMText.IndexOf(close, i + 2);
						i = end < 0 ? len : end + 1;
						continue;
					}
					if (char.IsLetter(nx))
					{
						switch (char.ToUpperInvariant(nx))
						{
							case 'P': // paragraph break → new line
								lines++; curW = 0; i += 2; continue;
							case 'H': case 'W': case 'T': case 'A': case 'C': case 'Q': case 'K':
								{
									// Value-codes: \H2x; \W1.7; \T0.75; \A1; \C1; ...
									int j = i + 2;
									while (j < len && rawMText[j] != ';' && rawMText[j] != '\\') j++;
									string val = rawMText.Substring(i + 2, j - (i + 2));
									double num;
									if (double.TryParse(val, System.Globalization.NumberStyles.Float,
										System.Globalization.CultureInfo.InvariantCulture, out num))
									{
										switch (char.ToUpperInvariant(nx))
										{
											case 'H': hFac = num; break;
											case 'W': wFac = num; break;
											case 'T': tFac = num; break;
										}
									}
									i = (j < len && rawMText[j] == ';') ? j + 1 : j;
									continue;
								}
							case 'F':
								{
									// \f<fontspec>; — skip the font spec (no width-factor change).
									int j = i + 2;
									while (j < len && rawMText[j] != ';' && rawMText[j] != '\\') j++;
									i = (j < len && rawMText[j] == ';') ? j + 1 : j;
									continue;
								}
							default: // toggle-codes: \L \l \O \o \M ... consume the letter only
								i += 2; continue;
						}
					}
					i++;
					continue;
				}

				// Regular (visible) character.
				double cw = (IsCjkChar(ch) ? 1.0 : latinFrac) * baseH * hFac * wFac * tFac;
				if (ch == ' ')
				{
					curW += cw;
				}
				else if (refWidth > 0 && curW > 1e-9 && curW + cw > refWidth + 1e-9)
				{
					lines++;
					curW = cw;
				}
				else
				{
					curW += cw;
				}
				i++;
			}
			return Math.Max(1, lines);
		}

	}
}
#endif
