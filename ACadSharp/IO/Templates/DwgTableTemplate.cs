using ACadSharp.IO.DWG;
using ACadSharp.Tables;
using ACadSharp.Tables.Collections;
using System;
using System.Collections.Generic;

namespace ACadSharp.IO.Templates
{
	internal class DwgTableTemplate<T> : DwgTemplate<Table<T>>
		where T : TableEntry
	{
		public List<ulong> EntryHandles { get; } = new List<ulong>();

		public DwgTableTemplate(Table<T> tableControl) : base(tableControl) { }

		public override void Build(DwgDocumentBuilder builder)
		{
			base.Build(builder);

			foreach (ulong handle in this.EntryHandles)
			{
				try
				{
					T entry = builder.GetCadObject<T>(handle);
					if (entry != null)
						this.CadObject.Add(builder.GetCadObject<T>(handle));
				}
				catch (Exception)
				{
					//TODO: report the exceptions in the NotificationEventHandler
				}
			}
		}
	}
}
