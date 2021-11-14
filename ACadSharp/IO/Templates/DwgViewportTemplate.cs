using ACadSharp.Entities;
using ACadSharp.IO.DWG;
using ACadSharp.Tables;
using System;
using System.Collections.Generic;

namespace ACadSharp.IO.Templates
{
	internal class DwgViewportTemplate : DwgEntityTemplate
	{
		public ulong? ViewportHeaderHandle { get; set; }
		public ulong? BoundaryHandle { get; set; }
		public ulong? NamedUcsHandle { get; set; }
		public ulong? BaseUcsHandle { get; set; }
		public List<ulong> FrozenLayerHandles { get; set; } = new List<ulong>();
		public DwgViewportTemplate(Viewport entity) : base(entity) { }

		public override void Build(DwgDocumentBuilder builder)
		{
			base.Build(builder);

			Viewport viewport = this.CadObject as Viewport;

			if (this.ViewportHeaderHandle.HasValue && this.ViewportHeaderHandle > 0)
			{
				throw new NotImplementedException();
			}

			if (this.BoundaryHandle.HasValue && this.BoundaryHandle > 0)
			{
				var entity = builder.GetCadObject<Entity>(this.BoundaryHandle.Value);

				if (entity != null)
					viewport.Boundary = entity;
			}

			if (this.NamedUcsHandle.HasValue && this.NamedUcsHandle > 0)
			{
				throw new NotImplementedException();
			}

			if (this.NamedUcsHandle.HasValue && this.NamedUcsHandle > 0)
			{
				throw new NotImplementedException();
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
