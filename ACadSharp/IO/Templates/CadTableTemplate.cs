﻿using ACadSharp.Tables;
using ACadSharp.Tables.Collections;
using System;
using System.Collections.Generic;

namespace ACadSharp.IO.Templates
{
	internal class CadTableTemplate<T> : CadTemplate<Table<T>>, ICadTableTemplate
		where T : TableEntry
	{
		public List<ulong> EntryHandles { get; } = new List<ulong>();

		public CadTableTemplate(Table<T> tableControl) : base(tableControl) { }

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
					builder.Notify(ex.Message, NotificationType.Warning);
				}
				catch (Exception ex)
				{
					builder.Notify($"Error adding the entry [handle : {handle}] [type : {typeof(T)}]", NotificationType.Error, ex);
				}
			}
		}
	}
}
