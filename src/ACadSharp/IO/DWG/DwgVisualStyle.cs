using ACadSharp.Objects;
using ACadSharp.IO.DWG.DwgStreamReaders;
using ACadSharp.IO.DWG.DwgStreamWriters;
using System;

namespace ACadSharp.IO.DWG;

// AcDbVisualStyle is a fixed sequence. R2010 adds an operation after each
// value; R2013 adds 30 more properties. Ported from acad-ts DwgVisualStyle.
internal static class DwgVisualStyle
{
	private sealed class Field
	{
		internal readonly string Type;
		internal readonly Func<VisualStyle, object> Get;
		internal readonly Action<VisualStyle, object> Set;
		internal Field(string type, Func<VisualStyle, object> get, Action<VisualStyle, object> set)
		{ this.Type = type; this.Get = get; this.Set = set; }
	}
	private static readonly Field[] _fields = {
		new Field("BL", style => style.FaceLightingModel, (style, value) => style.FaceLightingModel = (FaceLightingModelType)Convert.ToInt32(value)),
		new Field("BL", style => style.FaceLightingQuality, (style, value) => style.FaceLightingQuality = (FaceLightingQualityType)Convert.ToInt32(value)),
		new Field("BL", style => style.FaceColorMode, (style, value) => style.FaceColorMode = (FaceColorMode)Convert.ToInt32(value)),
		new Field("BS", style => style.FaceModifiers, (style, value) => style.FaceModifiers = (FaceModifierType)Convert.ToInt32(value)),
		new Field("BD", style => style.FaceOpacityLevel, (style, value) => style.FaceOpacityLevel = Convert.ToDouble(value)),
		new Field("BD", style => style.FaceSpecularLevel, (style, value) => style.FaceSpecularLevel = Convert.ToDouble(value)),
		new Field("CMC", style => style.FaceStyleMonoColor, (style, value) => style.FaceStyleMonoColor = (Color)value),
		new Field("BL", style => style.EdgeStyleModel, (style, value) => style.EdgeStyleModel = (EdgeStyleModel)Convert.ToInt32(value)),
		new Field("BL", style => style.EdgeStyle, (style, value) => style.EdgeStyle = Convert.ToInt32(value)),
		new Field("CMC", style => style.EdgeIntersectionColor, (style, value) => style.EdgeIntersectionColor = (Color)value),
		new Field("CMC", style => style.EdgeObscuredColor, (style, value) => style.EdgeObscuredColor = (Color)value),
		new Field("BL", style => style.EdgeObscuredLineType, (style, value) => style.EdgeObscuredLineType = Convert.ToInt32(value)),
		new Field("BL", style => style.EdgeIntersectionLineType, (style, value) => style.EdgeIntersectionLineType = Convert.ToInt32(value)),
		new Field("BD", style => style.EdgeCreaseAngle, (style, value) => style.EdgeCreaseAngle = Convert.ToDouble(value)),
		new Field("BL", style => style.EdgeModifiers, (style, value) => style.EdgeModifiers = Convert.ToInt32(value)),
		new Field("CMC", style => style.EdgeColorValue, (style, value) => style.EdgeColorValue = (Color)value),
		new Field("BD", style => style.OpacityLevel, (style, value) => style.OpacityLevel = Convert.ToDouble(value)),
		new Field("BL", style => style.EdgeWidth, (style, value) => style.EdgeWidth = Convert.ToInt32(value)),
		new Field("BL", style => style.EdgeOverhang, (style, value) => style.EdgeOverhang = Convert.ToInt32(value)),
		new Field("BL", style => style.EdgeJitter, (style, value) => style.EdgeJitter = Convert.ToInt32(value)),
		new Field("CMC", style => style.EdgeSilhouetteColor, (style, value) => style.EdgeSilhouetteColor = (Color)value),
		new Field("BL", style => style.EdgeSilhouetteWidth, (style, value) => style.EdgeSilhouetteWidth = Convert.ToInt32(value)),
		new Field("BL", style => style.HaloGap, (style, value) => style.HaloGap = Convert.ToInt16(value)),
		new Field("BL", style => style.EdgeIsolineCount, (style, value) => style.EdgeIsolineCount = Convert.ToInt16(value)),
		new Field("B", style => style.PrecisionFlag, (style, value) => style.PrecisionFlag = (bool)value),
		new Field("BL", style => style.DisplaySettings, (style, value) => style.DisplaySettings = Convert.ToInt32(value)),
		new Field("BD", style => style.Brightness, (style, value) => style.Brightness = Convert.ToDouble(value)),
		new Field("BL", style => style.ShadowType, (style, value) => style.ShadowType = Convert.ToInt16(value))
	};
	private static readonly Field[] _legacyFields = {
		new Field("BL", style => style.FaceLightingModel, (style, value) => style.FaceLightingModel = (FaceLightingModelType)Convert.ToInt32(value)),
		new Field("BL", style => style.FaceLightingQuality, (style, value) => style.FaceLightingQuality = (FaceLightingQualityType)Convert.ToInt32(value)),
		new Field("BL", style => style.FaceColorMode, (style, value) => style.FaceColorMode = (FaceColorMode)Convert.ToInt32(value)),
		new Field("BD", style => style.FaceOpacityLevel, (style, value) => style.FaceOpacityLevel = Convert.ToDouble(value)),
		new Field("BD", style => style.FaceSpecularLevel, (style, value) => style.FaceSpecularLevel = Convert.ToDouble(value)),
		new Field("CMC", style => style.FaceStyleMonoColor, (style, value) => style.FaceStyleMonoColor = (Color)value),
		new Field("BL", style => style.FaceModifiers, (style, value) => style.FaceModifiers = (FaceModifierType)Convert.ToInt32(value)),
		new Field("BL", style => style.EdgeStyleModel, (style, value) => style.EdgeStyleModel = (EdgeStyleModel)Convert.ToInt32(value)),
		new Field("BL", style => style.EdgeStyle, (style, value) => style.EdgeStyle = Convert.ToInt32(value)),
		new Field("CMC", style => style.EdgeIntersectionColor, (style, value) => style.EdgeIntersectionColor = (Color)value),
		new Field("CMC", style => style.EdgeObscuredColor, (style, value) => style.EdgeObscuredColor = (Color)value),
		new Field("BL", style => style.EdgeObscuredLineType, (style, value) => style.EdgeObscuredLineType = Convert.ToInt32(value)),
		new Field("BD", style => style.EdgeCreaseAngle, (style, value) => style.EdgeCreaseAngle = Convert.ToDouble(value)),
		new Field("BL", style => style.EdgeModifiers, (style, value) => style.EdgeModifiers = Convert.ToInt32(value)),
		new Field("CMC", style => style.EdgeColorValue, (style, value) => style.EdgeColorValue = (Color)value),
		new Field("BD", style => style.OpacityLevel, (style, value) => style.OpacityLevel = Convert.ToDouble(value)),
		new Field("BS", style => style.EdgeWidth, (style, value) => style.EdgeWidth = Convert.ToInt32(value)),
		new Field("BS", style => style.EdgeOverhang, (style, value) => style.EdgeOverhang = Convert.ToInt32(value)),
		new Field("BL", style => style.EdgeJitter, (style, value) => style.EdgeJitter = Convert.ToInt32(value)),
		new Field("CMC", style => style.EdgeSilhouetteColor, (style, value) => style.EdgeSilhouetteColor = (Color)value),
		new Field("BS", style => style.EdgeSilhouetteWidth, (style, value) => style.EdgeSilhouetteWidth = Convert.ToInt32(value)),
		new Field("RC", style => style.HaloGap, (style, value) => style.HaloGap = Convert.ToInt16(value)),
		new Field("BS", style => style.EdgeIsolineCount, (style, value) => style.EdgeIsolineCount = Convert.ToInt16(value)),
		new Field("B", style => style.PrecisionFlag, (style, value) => style.PrecisionFlag = (bool)value),
		new Field("BS", style => style.EdgeApplyStyleFlag, (style, value) => style.EdgeApplyStyleFlag = Convert.ToInt16(value)),
		new Field("BS", style => style.EdgeIntersectionLineType, (style, value) => style.EdgeIntersectionLineType = Convert.ToInt32(value)),
		new Field("BL", style => style.DisplaySettings, (style, value) => style.DisplaySettings = Convert.ToInt32(value)),
		new Field("BL", style => style.Brightness, (style, value) => style.Brightness = Convert.ToDouble(value)),
		new Field("BL", style => style.ShadowType, (style, value) => style.ShadowType = Convert.ToInt16(value))
	};
	private static readonly string[] _extendedTypes = { "B", "B", "B", "B", "B", "B", "B", "B", "B", "BL", "BD", "BD", "BL", "CMC", "BL", "BL", "CMC", "B", "BL", "BL", "BL", "B", "BL", "CMC", "BD", "BL", "TV", "B", "BD", "BD" };

	internal static void Read(IDwgStreamReader reader, ACadVersion version, VisualStyle style)
	{
		style.Description = reader.ReadVariableText();
		style.Type = reader.ReadBitLong();
		bool modern = version >= ACadVersion.AC1024;
		if (modern) { style.DwgLightingModel = reader.ReadBitShort(); style.InternalFlag = reader.ReadBit(); }
		var fields = modern ? _fields : _legacyFields;
		for (int i = 0; i < fields.Length; i++)
		{
			fields[i].Set(style, readValue(reader, fields[i].Type));
			if (modern) style.DwgPropertyOperations[i] = reader.ReadBitShort();
		}
		if (!modern)
		{
			if (version >= ACadVersion.AC1021) style.DwgLegacyUnknown = reader.ReadBitDouble();
			style.InternalFlag = reader.ReadBit();
		}
		if (version >= ACadVersion.AC1027)
			for (int i = 0; i < _extendedTypes.Length; i++)
				style.DwgExtendedProperties[i] = (readValue(reader, _extendedTypes[i]), reader.ReadBitShort());
	}

	internal static void Write(IDwgStreamWriter writer, ACadVersion version, VisualStyle style)
	{
		writer.WriteVariableText(style.Description);
		writer.WriteBitLong(style.Type);
		bool modern = version >= ACadVersion.AC1024;
		if (modern) { writer.WriteBitShort(style.DwgLightingModel); writer.WriteBit(style.InternalFlag); }
		var fields = modern ? _fields : _legacyFields;
		for (int i = 0; i < fields.Length; i++)
		{
			writeValue(writer, fields[i].Type, fields[i].Get(style));
			if (modern) writer.WriteBitShort(style.DwgPropertyOperations[i]);
		}
		if (!modern)
		{
			if (version >= ACadVersion.AC1021) writer.WriteBitDouble(style.DwgLegacyUnknown);
			writer.WriteBit(style.InternalFlag);
		}
		if (version >= ACadVersion.AC1027)
		{
			for (int i = 0; i < _extendedTypes.Length; i++)
			{
				var property = style.DwgExtendedProperties[i];
				writeValue(writer, _extendedTypes[i], property.Value);
				writer.WriteBitShort(property.Value == null ? (short)1 : property.Operation);
			}
		}
	}

	private static object readValue(IDwgStreamReader reader, string type) => type switch {
		"BL" => reader.ReadBitLong(), "BS" => reader.ReadBitShort(), "BD" => reader.ReadBitDouble(),
		"B" => reader.ReadBit(), "RC" => reader.ReadByte(), "CMC" => reader.ReadCmColor(),
		"TV" => reader.ReadVariableText(), _ => throw new InvalidOperationException(type),
	};

	private static void writeValue(IDwgStreamWriter writer, string type, object value)
	{
		switch (type)
		{
			case "BL": writer.WriteBitLong(Convert.ToInt32(value)); break;
			case "BS": writer.WriteBitShort(Convert.ToInt16(value)); break;
			case "BD": writer.WriteBitDouble(Convert.ToDouble(value)); break;
			case "B": writer.WriteBit(Convert.ToBoolean(value)); break;
			case "RC": writer.WriteByte(Convert.ToByte(value)); break;
			case "CMC": writer.WriteCmColor(value is Color color ? color : Color.ByLayer); break;
			case "TV": writer.WriteVariableText(value as string ?? string.Empty); break;
			default: throw new InvalidOperationException(type);
		}
	}
}
