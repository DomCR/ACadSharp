using ACadSharp.Entities;
using ACadSharp.Objects;
using ACadSharp.XData;
using CSMath;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ACadSharp.IO.DWG;

/// <summary>
/// R2000 (AC1015) and earlier versions have no native gradient fields (450-470). When
/// AutoCAD saves (down-saves) a gradient fill, it uses a private structure: the gradient
/// definition is stored in a separate XRecord ("Gradient1" definition chunk +
/// "Gradient4" name chunk, both byte-aligned), associated 1:1 via the fill extension
/// dictionary ACAD_XREC_ROUNDTRIP entry, augmented by the EED markers GradientColor1ACI(1070)
/// and ACAD(1010, 0,0,0). This file synthesizes that structure for in-memory gradient
/// fills when writing R2000, so that the R2004+ → R2000 conversion no longer loses the
/// gradient colors (symmetric with the read-side ApplyR2000Gradients).
/// </summary>
internal partial class DwgObjectWriter
{
	/// <summary>Entry name of the gradient XRecord in the fill extension dictionary.</summary>
	private const string GradientRoundTripKey = "ACAD_XREC_ROUNDTRIP";

	/// <summary>EED app name of the ACI marker for gradient fills.</summary>
	private const string GradientColor1AciApp = "GradientColor1ACI";

	/// <summary>
	/// Synthesizes the R2000 gradient roundtrip structure (XRecord + extension dictionary
	/// entry + EED markers) for every Hatch with a gradient fill. Called only for R2000 and
	/// earlier versions; fills that already carry a roundtrip XRecord in the source file
	/// (R2000→R2000 roundtrip) are automatically skipped.
	/// </summary>
	private void writeR2000GradientRoundTrips()
	{
		if (this.R2004Plus)
			return;

		List<Hatch> hatches = this._document.GetCadObjects<Hatch>().ToList();
		foreach (Hatch hatch in hatches)
		{
			HatchGradientPattern gradient = hatch.GradientColor;
			if (gradient == null || !gradient.Enabled)
				continue;

			if (gradient.Colors.Count < 1 || gradient.Colors.Count > 16)
			{
				this.notify($"Hatch {hatch.Handle} gradient has {gradient.Colors.Count} colors, cannot synthesize the R2000 round-trip record", NotificationType.Warning);
				continue;
			}

			// Do not re-synthesize when the source file already carries roundtrip data (R2000→R2000 roundtrip)
			if (hasGradientRoundTrip(hatch))
				continue;

			try
			{
				synthesizeGradientRoundTrip(hatch, gradient);
			}
			catch (Exception ex)
			{
				this.notify($"Failed to synthesize the R2000 gradient round-trip for hatch {hatch.Handle}: {ex.Message}", NotificationType.Warning);
			}
		}
	}

	/// <summary>
	/// Determines whether the fill already carries a gradient roundtrip XRecord
	/// (extension dictionary ACAD_XREC_ROUNDTRIP → XRecord containing a "Gradient1" entry).
	/// </summary>
	private static bool hasGradientRoundTrip(Hatch hatch)
	{
		if (hatch.XDictionary == null)
			return false;
		if (!hatch.XDictionary.TryGetEntry<XRecord>(GradientRoundTripKey, out XRecord xrecord) || xrecord == null)
			return false;
		return xrecord.Entries.Any(e => e.Value is string s && s == "Gradient1");
	}

	/// <summary>
	/// Synthesizes the gradient roundtrip: XRecord (definition chunk + name chunk), the
	/// extension dictionary entry and the EED markers. Adding the dictionary entry triggers
	/// document registration, and the XRecord automatically gets a handle and joins the write queue.
	/// </summary>
	private void synthesizeGradientRoundTrip(Hatch hatch, HatchGradientPattern gradient)
	{
		// 1) XRecord: 4 entries, group codes 102 (string tag) and 1004 (binary chunk)
		XRecord xrecord = new XRecord();
		xrecord.CloningFlags = DictionaryCloningFlags.KeepExisting;
		xrecord.CreateEntry(102, "Gradient1");
		xrecord.CreateEntry(1004, buildGradientDefinitionChunk(gradient));
		xrecord.CreateEntry(102, "Gradient4");
		xrecord.CreateEntry(1004, buildGradientNameChunk(gradient.Name));

		// 2) Extension dictionary ACAD_XREC_ROUNDTRIP → XRecord (1:1 association with the fill)
		CadDictionary dictionary = hatch.XDictionary ?? hatch.CreateExtendedDictionary();
		dictionary.HardOwnerFlag = true;
		dictionary.ClonningFlags = DictionaryCloningFlags.KeepExisting;
		if (!dictionary.ContainsKey(GradientRoundTripKey))
			dictionary.Add(GradientRoundTripKey, xrecord);

		// 3) EED markers: GradientColor1ACI(1070=ACI) + ACAD(1010=0,0,0)
		ensureGradientEed(hatch);
	}

	/// <summary>
	/// Fills in the EED markers for a gradient fill (only written when missing, never
	/// overwriting existing source-file content):
	/// GradientColor1ACI = 1070 ACI of the first gradient color; ACAD = 1010 origin coordinate.
	/// </summary>
	private static void ensureGradientEed(Hatch hatch)
	{
		var byName = hatch.ExtendedData.GetExtendedDataByName();

		if (!byName.ContainsKey(GradientColor1AciApp))
		{
			hatch.ExtendedData.Add(GradientColor1AciApp,
				new ExtendedData(new ExtendedDataRecord[]
				{
					new ExtendedDataInteger16(gradientFirstAci(hatch))
				}));
		}

		if (!byName.ContainsKey("ACAD"))
		{
			hatch.ExtendedData.Add("ACAD",
				new ExtendedData(new ExtendedDataRecord[]
				{
					new ExtendedDataCoordinate(new XYZ(0, 0, 0))
				}));
		}
	}

	/// <summary>
	/// ACI marker value of a gradient fill: if the fill color is an indexed color, use its
	/// index; otherwise use the index of the first gradient color (true colors approximated
	/// via ApproxIndex); if neither is available, use 0.
	/// </summary>
	private static short gradientFirstAci(Hatch hatch)
	{
		short index = hatch.Color.Index;
		if (index >= 0)
			return index;

		GradientColor first = hatch.GradientColor?.Colors?.FirstOrDefault();
		if (first != null)
		{
			if (!first.Color.IsTrueColor)
				return first.Color.Index;

			var rgb = first.Color.GetRgb();
			return Color.ApproxIndex(rgb[0], rgb[1], rgb[2]);
		}

		return 0;
	}

	/// <summary>
	/// Builds the "Gradient1" definition chunk (byte-aligned, symmetric with the read-side DecodeGradientDefChunk):
	/// +0  RL len (= total length - 24)    +4  RL ver = 3
	/// +8  BL450 is_gradient = 1           +12 BL451 reserved
	/// +16 BD460 angle                     +24 BD461 shift
	/// +32 BL452 single                    +36 BD462 tint
	/// +44 BL453 ncolors                   +48 32 bytes per color:
	///   BD463 value(8) + RL = 24(4) + mode = 1(4) + rgb 0xC2RRGGBB(4) + 12B tail
	/// </summary>
	private static byte[] buildGradientDefinitionChunk(HatchGradientPattern gradient)
	{
		int ncolors = gradient.Colors.Count;
		int total = 48 + ncolors * 32;
		byte[] chunk = new byte[total];

		BitConverter.GetBytes(total - 24).CopyTo(chunk, 0);       // RL len
		BitConverter.GetBytes(3).CopyTo(chunk, 4);                // RL ver
		BitConverter.GetBytes(1).CopyTo(chunk, 8);                // BL450 is_gradient
		BitConverter.GetBytes(gradient.Reserved).CopyTo(chunk, 12);   // BL451 reserved
		BitConverter.GetBytes(gradient.Angle).CopyTo(chunk, 16);      // BD460 angle
		BitConverter.GetBytes(gradient.Shift).CopyTo(chunk, 24);      // BD461 shift
		BitConverter.GetBytes(gradient.IsSingleColorGradient ? 1 : 0).CopyTo(chunk, 32);  // BL452 single
		BitConverter.GetBytes(gradient.ColorTint).CopyTo(chunk, 36);  // BD462 tint
		BitConverter.GetBytes(ncolors).CopyTo(chunk, 44);             // BL453 ncolors

		int offset = 48;
		foreach (GradientColor color in gradient.Colors)
		{
			BitConverter.GetBytes(color.Value).CopyTo(chunk, offset);              // BD463 value
			BitConverter.GetBytes(24).CopyTo(chunk, offset + 8);                   // RL = 24
			BitConverter.GetBytes(1).CopyTo(chunk, offset + 12);                   // mode = 1 (true color)
			BitConverter.GetBytes(encodeGradientRgb(color)).CopyTo(chunk, offset + 16);  // rgb 0xC2RRGGBB
			// 12-byte tail from +20 stays 0
			offset += 32;
		}

		return chunk;
	}

	/// <summary>
	/// Builds the "Gradient4" name chunk (byte-aligned, symmetric with the read side):
	/// RL len (self-contained, = 13 + name character count) | RL 4 | RL nameLen (incl. '\0') | NAME '\0'.
	/// When the name is empty, falls back to "LINEAR" (AutoCAD's default gradient name).
	/// </summary>
	private static byte[] buildGradientNameChunk(string name)
	{
		if (string.IsNullOrEmpty(name))
			name = "LINEAR";

		byte[] nameBytes = System.Text.Encoding.ASCII.GetBytes(name);
		int nameLen = nameBytes.Length + 1;               // includes the trailing '\0'
		int total = 12 + nameLen;

		byte[] chunk = new byte[total];
		BitConverter.GetBytes(total).CopyTo(chunk, 0);    // RL len (self-contained)
		BitConverter.GetBytes(4).CopyTo(chunk, 4);        // RL 4
		BitConverter.GetBytes(nameLen).CopyTo(chunk, 8);  // RL nameLen
		Array.Copy(nameBytes, 0, chunk, 12, nameBytes.Length);
		// trailing '\0' stays 0
		return chunk;
	}

	/// <summary>
	/// Gradient color → 0xC2RRGGBB (true color / indexed color via palette RGB) /
	/// 0xC0000000 (ByLayer), the inverse of the read-side DecodeGradientDefChunk decoding rules.
	/// For DXF sources, prefer the original value stored in the file
	/// <see cref="GradientColor.StoredColor"/> (0x00RRGGBB) to guarantee roundtrip consistency.
	/// </summary>
	private static uint encodeGradientRgb(GradientColor color)
	{
		Color c = color.Color;
		if (c.IsByLayer)
			return 0xC0000000u;

		if (color.StoredColor >= 0)
			return 0xC2000000u | ((uint)color.StoredColor & 0x00FFFFFFu);

		var rgb = c.GetRgb();
		return 0xC2000000u | ((uint)rgb[0] << 16) | ((uint)rgb[1] << 8) | rgb[2];
	}
}
