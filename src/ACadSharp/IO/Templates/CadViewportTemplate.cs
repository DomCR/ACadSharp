using ACadSharp.Entities;
using ACadSharp.Objects;
using ACadSharp.Tables;
using System.Collections.Generic;

namespace ACadSharp.IO.Templates
{
	internal class CadViewportTemplate : CadEntityTemplate<Viewport>
	{
		public ulong? ViewportHeaderHandle { get; set; }

		public ulong? BoundaryHandle { get; set; }

		public ulong? NamedUcsHandle { get; set; }

		public ulong? BaseUcsHandle { get; set; }

		public ulong? VisualStyleHandle { get; set; }

		public short? ViewportId { get; set; }

		public ulong? BlockHandle { get; set; }

		public HashSet<ulong> FrozenLayerHandles { get; set; } = new();

		public CadViewportTemplate() : base(new Viewport()) { }

		public CadViewportTemplate(Viewport entity) : base(entity) { }

		protected override void build(CadDocumentBuilder builder)
		{
			base.build(builder);

			if (this.ViewportHeaderHandle.HasValue && this.ViewportHeaderHandle > 0)
			{
				builder.Notify($"ViewportHeaderHandle not implemented for Viewport, handle {this.ViewportHeaderHandle}");
			}

			if (builder.TryGetCadObject<Entity>(this.BoundaryHandle, out Entity entity))
			{
				this.CadObject.Boundary = entity;
			}
			else if (this.BoundaryHandle.HasValue && this.BoundaryHandle > 0)
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

			if (this.CadObject.XDictionary != null &&
				this.CadObject.XDictionary.TryGetEntry(Viewport.ASDK_XREC_ANNOTATION_SCALE_INFO, out XRecord record))
			{
				foreach (XRecord.Entry item in record.Entries)
				{
					if (item.Code == 340)
					{
						if (builder.TryGetCadObject((ulong?)item.Value, out Scale scale))
						{
							this.CadObject.Scale = scale;
						}
					}
				}
			}

			foreach (var handle in this.FrozenLayerHandles)
			{
				if (builder.TryGetCadObject(handle, out Layer layer))
				{
					this.CadObject.FrozenLayers.Add(layer);
				}
				else
				{
					builder.Notify($"Frozen layer {handle} not found for viewport {this.CadObject.Handle}", NotificationType.Warning);
				}
			}
		}
	}
}
