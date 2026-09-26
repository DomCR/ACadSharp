using ACadSharp.Objects;
using System;

namespace ACadSharp.IO.Templates
{
	internal class CadXRecordTemplate : CadTemplate<XRecord>
	{
		private readonly System.Collections.Generic.List<Tuple<int, ulong, XRecord.Entry>> _entries = new();

		public CadXRecordTemplate() : base(new XRecord()) { }

		public CadXRecordTemplate(XRecord cadObject) : base(cadObject) { }

		/// <summary>
		/// Registers a handle that has to be resolved once the document is built.
		/// </summary>
		/// <param name="code">Group code of the entry.</param>
		/// <param name="handle">Handle of the referenced object.</param>
		/// <param name="entry">
		/// Entry already created in the position the record has in the file, its value is filled in
		/// by <see cref="build"/>. When it is null the entry is appended instead, which changes the
		/// order of the record and should only be used when the position is not known.
		/// </param>
		public void AddHandleReference(int code, ulong handle, XRecord.Entry entry = null)
		{
			_entries.Add(new Tuple<int, ulong, XRecord.Entry>(code, handle, entry));
		}

		protected override void build(CadDocumentBuilder builder)
		{
			base.build(builder);

			foreach (var entry in _entries)
			{
				if (builder.TryGetCadObject<CadObject>(entry.Item2, out CadObject obj))
				{
					if (entry.Item3 == null)
					{
						this.CadObject.CreateEntry(entry.Item1, obj);
					}
					else
					{
						entry.Item3.Value = obj;
					}
				}
				else
				{
					builder.Notify($"XRecord reference not found {entry.Item1}|{entry.Item2}", NotificationType.Warning);
				}
			}
		}
	}
}