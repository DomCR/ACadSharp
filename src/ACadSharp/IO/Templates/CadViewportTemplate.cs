using ACadSharp.Entities;
using ACadSharp.Tables;
using System.Collections.Generic;

namespace ACadSharp.IO.Templates
{
	internal class CadViewportTemplate : CadEntityTemplate
	{
		public ulong? ViewportHeaderHandle { get; set; }

		public ulong? BoundaryHandle { get; set; }

		public ulong? NamedUcsHandle { get; set; }

		public ulong? BaseUcsHandle { get; set; }

		public ulong? VisualStyleHandle { get; set; }

		public short? ViewportId { get; internal set; }

		public List<ulong> FrozenLayerHandles { get; set; } = new List<ulong>();

		public CadViewportTemplate() : base(new Viewport()) { }

		public CadViewportTemplate(Viewport entity) : base(entity) { }

		public override void Build(CadDocumentBuilder builder)
		{
			base.Build(builder);

			Viewport viewport = this.CadObject as Viewport;

			if (this.ViewportHeaderHandle.HasValue && this.ViewportHeaderHandle > 0)
			{
				builder.Notify($"ViewportHeaderHandle not implemented for Viewport, handle {this.ViewportHeaderHandle}");
			}

			if (builder.TryGetCadObject<Entity>(this.BoundaryHandle, out Entity entity))
			{
				viewport.Boundary = entity;
			}
			else if(this.BoundaryHandle.HasValue  && this.BoundaryHandle > 0)
			{
				builder.Notify($"Boundary {this.BoundaryHandle} not found for viewport {this.CadObject.Handle}", NotificationType.Warning);
			}

			if (this.NamedUcsHandle.HasValue && this.NamedUcsHandle > 0)
			{
				builder.Notify($"Named ucs not implemented for Viewport, handle {this.NamedUcsHandle}");
			}

			if (this.BaseUcsHandle.HasValue && this.BaseUcsHandle > 0)
			{
				builder.Notify($"Base ucs not implemented for Viewport, handle {this.BaseUcsHandle}");
			}

			foreach (var handle in this.FrozenLayerHandles)
			{
				if (builder.TryGetCadObject(handle, out Layer layer))
				{
					viewport.FrozenLayers.Add(layer);
				}
				else
				{
					builder.Notify($"Frozen layer {handle} not found for viewport {this.CadObject.Handle}", NotificationType.Warning);
				}
			}
		}
	}
}
