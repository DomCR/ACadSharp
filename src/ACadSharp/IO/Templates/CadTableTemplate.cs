using ACadSharp.Tables;
using ACadSharp.Tables.Collections;
using System;
using System.Collections.Generic;

namespace ACadSharp.IO.Templates
{
	internal class CadTableTemplate<T> : CadTemplate<Table<T>>, ICadTableTemplate
		where T : TableEntry
	{
		public HashSet<ulong> EntryHandles { get; } = new();

		public CadTableTemplate(Table<T> tableControl) : base(tableControl) { }

		protected override void build(CadDocumentBuilder builder)
		{
			base.build(builder);

			foreach (ulong handle in this.EntryHandles)
			{
				if (!builder.TryGetCadObject<T>(handle, out T entry))
					continue;

				try
				{
					this.CadObject.Add(entry);
				}
				catch (ArgumentException ex)
				{
					builder.Notify($"[{this.CadObject.SubclassMarker}] the entry {entry.Name} already exists", NotificationType.Error, ex);
				}
				catch (Exception ex)
				{
					builder.Notify($"Error adding the entry [handle : {handle}] [type : {typeof(T)}]", NotificationType.Error, ex);
				}
			}
		}
	}
}
