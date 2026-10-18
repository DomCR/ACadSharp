using ACadSharp.Tables;
using ACadSharp.XData;
using System;
using System.Linq;

namespace ACadSharp.IO;

internal static class TextStyleExtendedData
{
	internal static ExtendedDataDictionary ForWrite(TextStyle style)
	{
		int flags = (style.TrueType.HasFlag(FontFlags.Italic) ? 0x01000000 : 0)
			| (style.TrueType.HasFlag(FontFlags.Bold) ? 0x02000000 : 0);
		string filename = style.Filename?.Trim() ?? string.Empty;
		if (flags == 0 || style.IsShapeFile || filename.EndsWith(".shx", StringComparison.OrdinalIgnoreCase))
			return style.ExtendedData;
		string basename = filename.Replace('\\', '/').Split('/').Last();
		int extension = basename.LastIndexOf('.');
		string family = extension > 0 ? basename.Substring(0, extension) : basename;
		if (string.IsNullOrEmpty(family)) return style.ExtendedData;
		var existing = style.ExtendedData.FirstOrDefault(e => e.Key.Name.Equals(AppId.DefaultName, StringComparison.OrdinalIgnoreCase));
		AppId app = existing.Key;
		if (style.Document != null && style.Document.AppIds.TryGetValue(AppId.DefaultName, out var registered)) app = registered;
		if (app == null) return style.ExtendedData;

		var result = new ExtendedDataDictionary(style);
		foreach (var entry in style.ExtendedData)
			if (!entry.Key.Name.Equals(AppId.DefaultName, StringComparison.OrdinalIgnoreCase)) result.Add(entry.Key, entry.Value);
		var records = existing.Value?.Records.ToList() ?? new();
		if (records.Count >= 2 && records[0] is ExtendedDataString && records[1] is ExtendedDataInteger32 previous)
		{
			flags |= previous.Value & ~0x03000000;
			records.RemoveRange(0, 2);
		}
		records.Insert(0, new ExtendedDataInteger32(flags));
		records.Insert(0, new ExtendedDataString(family));
		result.Add(app, records);
		return result;
	}

	internal static void Apply(TextStyle style)
	{
		if (!style.ExtendedData.TryGet(AppId.DefaultName, out var data)) return;
		for (int i = 0; i + 1 < data.Records.Count; i++)
		{
			if (data.Records[i] is ExtendedDataString && data.Records[i + 1] is ExtendedDataInteger32 flags)
			{
				style.TrueType = ((flags.Value & 0x01000000) != 0 ? FontFlags.Italic : FontFlags.Regular)
					| ((flags.Value & 0x02000000) != 0 ? FontFlags.Bold : FontFlags.Regular);
				return;
			}
		}
	}
}
