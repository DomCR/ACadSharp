using ACadSharp.Objects;

namespace ACadSharp.IO.Templates;

internal class CadXRecordTemplate : CadTemplate<XRecord>
{
	private readonly System.Collections.Generic.List<XRecord.Entry> _entries = new();

	public CadXRecordTemplate() : base(new XRecord())
	{
	}

	public CadXRecordTemplate(XRecord cadObject) : base(cadObject)
	{
	}

	public void AddHandleReference(XRecord.Entry entry)
	{
		_entries.Add(entry);
	}

	protected override void build(CadDocumentBuilder builder)
	{
		base.build(builder);

		foreach (var entry in _entries)
		{
			if (!(entry.Value is ulong handle))
			{
				continue;
			}

			if (builder.TryGetCadObject<CadObject>(handle, out CadObject obj))
			{
				entry.Value = obj;
			}
			else
			{
				builder.Notify($"XRecord with handle {this.CadObject.Handle} reference with handle {handle} not found.", NotificationType.Warning);
			}
		}
	}
}