using ACadSharp.IO.DWG;
using ACadSharp.Tables;
using ACadSharp.Tables.Collections;
using System;
using System.Collections.Generic;

namespace ACadSharp.IO.Templates
{
	public interface ICadTableTemplate
	{
		List<ulong> EntryHandles { get; }
	}

	internal class DwgTableTemplate<T> : CadTemplate<Table<T>>, ICadTableTemplate
		where T : TableEntry
	{
		public List<ulong> EntryHandles { get; } = new List<ulong>();

		public DwgTableTemplate(Table<T> tableControl) : base(tableControl) { }

		public override void Build(CadDocumentBuilder builder)
		{
			base.Build(builder);

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
					builder.NotificationHandler?.Invoke(
						entry,
						new NotificationEventArgs(ex.Message));
				}
				catch (Exception)
				{
					builder.NotificationHandler?.Invoke(
						entry,
						new NotificationEventArgs($"Entry not found [handle : {handle}] [type : {typeof(T)}]"));
				}
			}
		}
	}
}
