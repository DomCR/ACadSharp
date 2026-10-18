using ACadSharp.Entities;
using ACadSharp.Tables;
using ACadSharp.Objects;
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

			if (builder.TryGetCadObject<Entity>(this.BoundaryHandle, out Entity entity))
			{
				this.CadObject.Boundary = entity;
			}
			else if (this.BoundaryHandle.HasValue && this.BoundaryHandle > 0)
			{
				builder.Notify($"Boundary {this.BoundaryHandle} not found for viewport {this.CadObject.Handle}", NotificationType.Warning);
			}

			void applyUcs(UCS ucs)
			{
				this.CadObject.UcsPerViewport = true;
				this.CadObject.UcsOrigin = ucs.Origin;
				this.CadObject.UcsXAxis = ucs.XAxis;
				this.CadObject.UcsYAxis = ucs.YAxis;
				this.CadObject.UcsOrthographicType = ucs.OrthographicType;
				this.CadObject.Elevation = ucs.Elevation;
			}
			if (builder.TryGetCadObject<UCS>(this.BaseUcsHandle, out var baseUcs)) applyUcs(baseUcs);
			else if (this.BaseUcsHandle > 0) builder.Notify($"Base UCS {this.BaseUcsHandle} not found for viewport {this.CadObject.Handle}", NotificationType.Warning);
			if (builder.TryGetCadObject<UCS>(this.NamedUcsHandle, out var namedUcs)) applyUcs(namedUcs);
			else if (this.NamedUcsHandle > 0) builder.Notify($"Named UCS {this.NamedUcsHandle} not found for viewport {this.CadObject.Handle}", NotificationType.Warning);
			if (builder.TryGetCadObject<VisualStyle>(this.VisualStyleHandle, out var visualStyle)) this.CadObject.VisualStyle = visualStyle;
			else if (this.VisualStyleHandle > 0) builder.Notify($"Visual style {this.VisualStyleHandle} not found for viewport {this.CadObject.Handle}", NotificationType.Warning);

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
