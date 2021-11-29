using ACadSharp.Entities;
using ACadSharp.IO.DWG;
using System;
using System.Collections.Generic;

namespace ACadSharp.IO.Templates
{
	internal class DwgViewportEntityControlTemplate : DwgTemplate<ViewportCollection>
	{
		public List<ulong> EntryHandles { get; } = new List<ulong>();

		public DwgViewportEntityControlTemplate(ViewportCollection collection) : base(collection) { }

		public override void Build(DwgDocumentBuilder builder)
		{
			base.Build(builder);

			foreach (ulong handle in this.EntryHandles)
			{
				try
				{
					Viewport entry = builder.GetCadObject<Viewport>(handle);
					if (entry != null)
						this.CadObject.Add(builder.GetCadObject<Viewport>(handle));
				}
				catch (Exception)
				{
					//TODO: report the exceptions in the NotificationEventHandler
				}
			}
		}
	}
}
