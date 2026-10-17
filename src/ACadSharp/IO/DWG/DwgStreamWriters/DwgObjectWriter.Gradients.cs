using ACadSharp.Entities;
using ACadSharp.Objects;
using ACadSharp.XData;
using CSMath;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ACadSharp.IO.DWG;

/// <summary>
/// R2000（AC1015）及更早版本无原生渐变字段（450-470）。AutoCAD 下存（down-save）
/// 渐变填充时采用私有结构：渐变定义存入独立 XRecord（"Gradient1" 定义块 +
/// "Gradient4" 名称块，均字节对齐），经填充扩展字典 ACAD_XREC_ROUNDTRIP 条目
/// 1:1 关联，并辅以 EED 标记 GradientColor1ACI(1070) 与 ACAD(1010, 0,0,0)。
/// 本文件在写出 R2000 时为内存中的渐变填充合成该结构，使 R2004+ → R2000
/// 转换不再丢失渐变色（与读取端 ApplyR2000Gradients 对称）。
/// </summary>
internal partial class DwgObjectWriter
{
	/// <summary>渐变 XRecord 在填充扩展字典中的条目名。</summary>
	private const string GradientRoundTripKey = "ACAD_XREC_ROUNDTRIP";

	/// <summary>渐变填充的 ACI 标记 EED 应用名。</summary>
	private const string GradientColor1AciApp = "GradientColor1ACI";

	/// <summary>
	/// 为所有带渐变填充的 Hatch 合成 R2000 渐变 roundtrip 结构
	/// （XRecord + 扩展字典条目 + EED 标记）。仅 R2000 及更早版本调用；
	/// 已由源文件携带 roundtrip XRecord 的填充（R2000→R2000 往返）自动跳过。
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

			// 源文件已携带 roundtrip 数据（R2000→R2000 往返）时不重复合成
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
	/// 判断填充是否已携带渐变 roundtrip XRecord
	/// （扩展字典 ACAD_XREC_ROUNDTRIP → 含 "Gradient1" 条目的 XRecord）。
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
	/// 合成渐变 roundtrip：XRecord（定义块 + 名称块）、扩展字典条目与 EED 标记。
	/// 字典条目 Add 触发文档注册，XRecord 自动获得句柄并入写出队列。
	/// </summary>
	private void synthesizeGradientRoundTrip(Hatch hatch, HatchGradientPattern gradient)
	{
		// 1) XRecord：4 条目，组码 102（字符串标签）与 1004（二进制块）
		XRecord xrecord = new XRecord();
		xrecord.CloningFlags = DictionaryCloningFlags.KeepExisting;
		xrecord.CreateEntry(102, "Gradient1");
		xrecord.CreateEntry(1004, buildGradientDefinitionChunk(gradient));
		xrecord.CreateEntry(102, "Gradient4");
		xrecord.CreateEntry(1004, buildGradientNameChunk(gradient.Name));

		// 2) 扩展字典 ACAD_XREC_ROUNDTRIP → XRecord（与填充 1:1 关联）
		CadDictionary dictionary = hatch.XDictionary ?? hatch.CreateExtendedDictionary();
		dictionary.HardOwnerFlag = true;
		dictionary.ClonningFlags = DictionaryCloningFlags.KeepExisting;
		if (!dictionary.ContainsKey(GradientRoundTripKey))
			dictionary.Add(GradientRoundTripKey, xrecord);

		// 3) EED 标记：GradientColor1ACI(1070=ACI) + ACAD(1010=0,0,0)
		ensureGradientEed(hatch);
	}

	/// <summary>
	/// 补写渐变填充的 EED 标记（缺失时才写，不覆盖源文件已有内容）：
	/// GradientColor1ACI = 1070 首渐变色的 ACI；ACAD = 1010 原点坐标。
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
	/// 渐变填充的 ACI 标记值：填充色为索引色取其索引；否则取首个渐变色的索引
	/// （真彩色经 ApproxIndex 近似）；均不可用时取 0。
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
	/// 构建 "Gradient1" 定义块（字节对齐，与读取端 DecodeGradientDefChunk 对称）：
	/// +0  RL len（= 总长 - 24）        +4  RL ver = 3
	/// +8  BL450 is_gradient = 1        +12 BL451 reserved
	/// +16 BD460 angle                  +24 BD461 shift
	/// +32 BL452 single                 +36 BD462 tint
	/// +44 BL453 ncolors                +48 起每色 32 字节：
	///   BD463 value(8) + RL = 24(4) + mode = 1(4) + rgb 0xC2RRGGBB(4) + 尾部 12B
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
			BitConverter.GetBytes(1).CopyTo(chunk, offset + 12);                   // mode = 1（真彩色）
			BitConverter.GetBytes(encodeGradientRgb(color)).CopyTo(chunk, offset + 16);  // rgb 0xC2RRGGBB
			// +20 起 12 字节尾部保持 0
			offset += 32;
		}

		return chunk;
	}

	/// <summary>
	/// 构建 "Gradient4" 名称块（字节对齐，与读取端对称）：
	/// RL len（自含，= 13 + 名称字符数）| RL 4 | RL nameLen（含 '\0'）| NAME '\0'。
	/// 名称为空时回退为 "LINEAR"（AutoCAD 默认渐变名）。
	/// </summary>
	private static byte[] buildGradientNameChunk(string name)
	{
		if (string.IsNullOrEmpty(name))
			name = "LINEAR";

		byte[] nameBytes = System.Text.Encoding.ASCII.GetBytes(name);
		int nameLen = nameBytes.Length + 1;               // 含结尾 '\0'
		int total = 12 + nameLen;

		byte[] chunk = new byte[total];
		BitConverter.GetBytes(total).CopyTo(chunk, 0);    // RL len（自含）
		BitConverter.GetBytes(4).CopyTo(chunk, 4);        // RL 4
		BitConverter.GetBytes(nameLen).CopyTo(chunk, 8);  // RL nameLen
		Array.Copy(nameBytes, 0, chunk, 12, nameBytes.Length);
		// 结尾 '\0' 保持 0
		return chunk;
	}

	/// <summary>
	/// 渐变色 → 0xC2RRGGBB（真彩色/索引色按调色板 RGB）/ 0xC0000000（ByLayer），
	/// 与读取端 DecodeGradientDefChunk 的解码规则互逆。DXF 来源优先使用文件中
	/// 存储的原值 <see cref="GradientColor.StoredColor"/>（0x00RRGGBB）以保证往返一致。
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
