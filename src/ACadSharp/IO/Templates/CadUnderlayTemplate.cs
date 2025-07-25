﻿using ACadSharp.Entities;
using ACadSharp.Objects;

namespace ACadSharp.IO.Templates
{
	internal class CadUnderlayTemplate<T> : CadEntityTemplate
		where T : UnderlayDefinition
	{
		public ulong? DefinitionHandle { get; set; }

		public CadUnderlayTemplate(UnderlayEntity<T> entity) : base(entity)
		{
		}

		public override void Build(CadDocumentBuilder builder)
		{
			base.Build(builder);

			UnderlayEntity<T> underlay = this.CadObject as UnderlayEntity<T>;

			if (builder.TryGetCadObject(this.DefinitionHandle, out T definition))
			{
				underlay.Definition = definition;
			}
			else
			{
				builder.Notify($"UnderlayDefinition not found for {this.CadObject.Handle}", NotificationType.Warning);
			}
		}
	}
}
