using ACadSharp.Entities;
using ACadSharp.Objects;
using System;
using System.Linq;

namespace ACadSharp.IO.DXF
{
	internal class DxfObjectsSectionWriter : DxfSectionWriterBase
	{
		public override string SectionName { get { return DxfFileToken.ObjectsSection; } }

		public DxfObjectsSectionWriter(IDxfStreamWriter writer, CadDocument document, CadObjectHolder holder) : base(writer, document, holder)
		{
		}

		protected override void writeSection()
		{
			while (this.Holder.Objects.Any())
			{
				CadObject item = this.Holder.Objects.Dequeue();

				this.writeObject(item);
			}
		}

		protected void writeObject<T>(T co)
			where T : CadObject
		{
			switch (co)
			{
				case AcdbPlaceHolder:
				case Material:
				case MultiLeaderStyle:
				case SortEntitiesTable:
				case Scale:
				case VisualStyle:
				//case XRecrod:	//TODO: XRecord Understand how it works for the reader
					this.notify($"Object not implemented : {co.GetType().FullName}");
					return;
			}

			this._writer.Write(DxfCode.Start, co.ObjectName);

			this.writeCommonObjectData(co);

			switch (co)
			{
				case CadDictionary cadDictionary:
					this.writeDictionary(cadDictionary);
					return;
				case DictionaryVariable dictvar:
					this.writeDictionaryVariable(dictvar);
					break;
				case Group group:
					this.writeGroup(group); 
					break;
				case Layout layout:
					this.writeLayout(layout);
					break;
				case MLStyle mlStyle:
					this.writeMLStyle(mlStyle);
					break;
				case PlotSettings plotSettings:
					this.writePlotSettings(plotSettings);
					break;
				case SortEntitiesTable sortensTable:
					//this.writeSortentsTable(sortensTable);
					break;
				case XRecord record:
					this.writeXRecord(record);
					break;
				default:
					throw new NotImplementedException($"Object not implemented : {co.GetType().FullName}");
			}

			this.writeExtendedData(co);
		}

		protected void writeDictionary(CadDictionary e)
		{
			this._writer.Write(DxfCode.Subclass, DxfSubclassMarker.Dictionary);

			this._writer.Write(280, e.HardOwnerFlag);
			this._writer.Write(281, (int)e.ClonningFlags);

			System.Diagnostics.Debug.Assert(e.EntryNames.Length == e.EntryHandles.Length);
			for (int i = 0; i < e.EntryNames.Length; i++)
			{
				this._writer.Write(3, e.EntryNames[i]);
				this._writer.Write(350, e.EntryHandles[i]);
			}

			//Add the entries as objects
			foreach (CadObject item in e)
			{
				this.Holder.Objects.Enqueue(item);
			}
		}

		protected void writeDictionaryVariable(DictionaryVariable dictvar)
		{
			DxfClassMap map = DxfClassMap.Create<DictionaryVariable>();

			this._writer.Write(100, DxfSubclassMarker.DictionaryVariables);

			this._writer.Write(1, dictvar.Value, map);
			this._writer.Write(280, dictvar.ObjectSchemaNumber, map);
		}

		protected void writePlotSettings(PlotSettings plot)
		{
			DxfClassMap map = DxfClassMap.Create<PlotSettings>();

			this._writer.Write(100, DxfSubclassMarker.PlotSettings);

			this._writer.Write(1, plot.PageName, map);
			this._writer.Write(2, plot.SystemPrinterName, map);

			this._writer.Write(4, plot.PaperSize, map);

			this._writer.Write(6, plot.PlotViewName, map);
			this._writer.Write(7, plot.StyleSheet, map);

			this._writer.Write(40, plot.UnprintableMargin.Left, map);
			this._writer.Write(41, plot.UnprintableMargin.Bottom, map);
			this._writer.Write(42, plot.UnprintableMargin.Right, map);
			this._writer.Write(43, plot.UnprintableMargin.Top, map);
			this._writer.Write(44, plot.PaperWidth, map);
			this._writer.Write(45, plot.PaperHeight, map);
			this._writer.Write(46, plot.PlotOriginX, map);
			this._writer.Write(47, plot.PlotOriginY, map);
			this._writer.Write(48, plot.WindowLowerLeftX, map);
			this._writer.Write(49, plot.WindowLowerLeftY, map);

			this._writer.Write(140, plot.WindowUpperLeftX, map);
			this._writer.Write(141, plot.WindowUpperLeftY, map);
			this._writer.Write(142, plot.NumeratorScale, map);
			this._writer.Write(143, plot.DenominatorScale, map);

			this._writer.Write(70, (short)plot.Flags, map);

			this._writer.Write(72, (short)plot.PaperUnits, map);
			this._writer.Write(73, (short)plot.PaperRotation, map);
			this._writer.Write(74, (short)plot.PlotType, map);
			this._writer.Write(75, plot.ScaledFit, map);
			this._writer.Write(76, (short)plot.ShadePlotMode, map);
			this._writer.Write(77, (short)plot.ShadePlotResolutionMode, map);
			this._writer.Write(78, plot.ShadePlotDPI, map);
			this._writer.Write(147, plot.PrintScale, map);

			this._writer.Write(148, plot.PaperImageOrigin.X, map);
			this._writer.Write(149, plot.PaperImageOrigin.Y, map);
		}

		protected void writeGroup(Group group)
		{
			this._writer.Write(100, DxfSubclassMarker.Group);

			this._writer.Write(300, group.Description);
			this._writer.Write(70, group.IsUnnamed ? (short)1 : (short)0);
			this._writer.Write(71, group.Selectable ? (short)1 : (short)0);

			foreach (Entity entity in group.Entities.Values)
			{
				this._writer.WriteHandle(340, entity);
			}
		}

		protected void writeLayout(Layout layout)
		{
			DxfClassMap map = DxfClassMap.Create<Layout>();

			this.writePlotSettings(layout);

			this._writer.Write(100, DxfSubclassMarker.Layout);

			this._writer.Write(1, layout.Name, map);

			//this._writer.Write(70, (short) 1,map);
			this._writer.Write(71, layout.TabOrder, map);

			this._writer.Write(10, layout.MinLimits, map);
			this._writer.Write(11, layout.MaxLimits, map);
			this._writer.Write(12, layout.InsertionBasePoint, map);
			this._writer.Write(13, layout.Origin, map);
			this._writer.Write(14, layout.MinExtents, map);
			this._writer.Write(15, layout.MaxExtents, map);
			this._writer.Write(16, layout.XAxis, map);
			this._writer.Write(17, layout.YAxis, map);

			this._writer.Write(146, layout.Elevation, map);

			this._writer.Write(76, (short)0, map);

			this._writer.WriteHandle(330, layout.AssociatedBlock.Owner, map);
		}

		protected void writeMLStyle(MLStyle style)
		{
			DxfClassMap map = DxfClassMap.Create<MLStyle>();

			this._writer.Write(100, DxfSubclassMarker.MLineStyle);

			this._writer.Write(2, style.Name, map);

			this._writer.Write(70, (short)style.Flags, map);

			this._writer.Write(3, style.Description, map);

			this._writer.Write(62, style.FillColor.Index, map);

			this._writer.Write(51, style.StartAngle, map);
			this._writer.Write(52, style.EndAngle, map);
			this._writer.Write(71, (short)style.Elements.Count, map);
			foreach (MLStyle.Element element in style.Elements)
			{
				this._writer.Write(49, element.Offset, map);
				this._writer.Write(62, element.Color.Index, map);
				this._writer.Write(6, element.LineType.Name, map);
			}
		}

		private void writeSortentsTable(SortEntitiesTable e)
		{
			if (e.BlockOwner == null)
			{
				//In some cases the block onwer is null in the files, this has to be checked
				this.notify("SortEntitiesTable with handle {e.Handle} has no block owner", NotificationType.Warning);
				return;
			}

			this._writer.Write(DxfCode.Start, e.ObjectName);

			this.writeCommonObjectData(e);

			this._writer.Write(DxfCode.Subclass, DxfSubclassMarker.XRecord);

			this._writer.Write(330, e.BlockOwner.Handle);
		}

		protected void writeXRecord(XRecord e)
		{
			this._writer.Write(DxfCode.Subclass, DxfSubclassMarker.XRecord);

			foreach (var item in e.Entries)
			{
				this._writer.Write(item.Code, item.Value);
			}
		}
	}
}
