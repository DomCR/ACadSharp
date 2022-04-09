using ACadSharp.Entities;
using ACadSharp.IO.DWG;
using ACadSharp.Tables;
using System;
using System.Collections.Generic;

namespace ACadSharp.IO.Templates
{
	internal class DwgViewportTemplate : CadEntityTemplate
	{
		public ulong? ViewportHeaderHandle { get; set; }

		public ulong? BoundaryHandle { get; set; }
		
		public ulong? NamedUcsHandle { get; set; }
		
		public ulong? BaseUcsHandle { get; set; }
		
		public List<ulong> FrozenLayerHandles { get; set; } = new List<ulong>();
		
		public DwgViewportTemplate(Viewport entity) : base(entity) { }

		public override void Build(CadDocumentBuilder builder)
		{
			base.Build(builder);

			Viewport viewport = this.CadObject as Viewport;

			if (this.ViewportHeaderHandle.HasValue && this.ViewportHeaderHandle > 0)
			{
				builder.NotificationHandler?.Invoke(null, new NotificationEventArgs($"ViewportHeaderHandle not implemented for Viewport, handle {this.ViewportHeaderHandle}"));
			}

			if (this.BoundaryHandle.HasValue && this.BoundaryHandle > 0)
			{
				var entity = builder.GetCadObject<Entity>(this.BoundaryHandle.Value);

				if (entity != null)
					viewport.Boundary = entity;
			}

			if (this.NamedUcsHandle.HasValue && this.NamedUcsHandle > 0)
			{
				builder.NotificationHandler?.Invoke(null, new NotificationEventArgs($"Named ucs not implemented for Viewport, handle {this.NamedUcsHandle}"));
			}

			if (this.BaseUcsHandle.HasValue && this.BaseUcsHandle > 0)
			{
				builder.NotificationHandler?.Invoke(null, new NotificationEventArgs($"Base ucs not implemented for Viewport, handle {this.BaseUcsHandle}"));
			}

			foreach (var handle in this.FrozenLayerHandles)
			{
				var layer = builder.GetCadObject<Layer>(handle);

				if (layer != null)
					viewport.FrozenLayers.Add(layer);
			}
		}
	}
}
