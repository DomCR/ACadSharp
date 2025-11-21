using ACadSharp.Objects;
using System;

namespace ACadSharp.IO.Templates
{
	internal class CadXRecordTemplate : CadTemplate<XRecord>
	{
		private readonly System.Collections.Generic.List<Tuple<int, ulong>> _entries = new();

		public CadXRecordTemplate() : base(new XRecord()) { }

		public CadXRecordTemplate(XRecord cadObject) : base(cadObject) { }

		public void AddHandleReference(int code, ulong handle)
		{
			_entries.Add(new Tuple<int, ulong>(code, handle));
		}

		protected override void build(CadDocumentBuilder builder)
		{
			base.build(builder);

			foreach (var entry in _entries)
			{
				if (builder.TryGetCadObject<CadObject>(entry.Item2, out CadObject obj))
				{
					this.CadObject.CreateEntry(entry.Item1, obj);
				}
				else
				{
					builder.Notify($"XRecord reference not found {entry.Item1}|{entry.Item2}", NotificationType.Warning);
				}
			}
		}
	}
}