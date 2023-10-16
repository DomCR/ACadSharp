using ACadSharp.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ACadSharp.IO.DWG
{
	internal partial class DwgObjectWriter : DwgSectionIO
	{
		private void writeObjects()
		{
			while (this._objects.Any())
			{
				CadObject obj = this._objects.Dequeue();

				this.writeObject(obj);
			}
		}

		private void writeObject(CadObject obj)
		{
			switch (obj)
			{
				case DictionaryVariable:
				case MLStyle:
				case Scale:
				case SortEntitiesTable:
				case XRecord:
					return;
			}

			this.writeCommonNonEntityData(obj);

			switch (obj)
			{
				case AcdbPlaceHolder acdbPlaceHolder:
					this.writeAcdbPlaceHolder(acdbPlaceHolder);
					break;
				case CadDictionary dictionary:
					this.writeDictionary(dictionary);
					break;
				case Group group:
					this.writeGroup(group);
					break;
				case Layout layout:
					this.writeLayout(layout);
					break;
				case PlotSettings plotsettings:
					this.writePlotSettings(plotsettings);
					break;
				case XRecord record:
					this.writeXRecord(record);
					break;
				default:
					throw new NotImplementedException($"Object not implemented : {obj.GetType().FullName}");
			}

			this.registerObject(obj);
		}

		private void writeAcdbPlaceHolder(AcdbPlaceHolder acdbPlaceHolder)
		{
		}

		private void writeDictionary(CadDictionary dictionary)
		{
			//Common:
			//Numitems L number of dictonary items
			this._writer.WriteBitLong(dictionary.Count());

			//R14 Only:
			if (this._version == ACadVersion.AC1014)
			{
				//Unknown R14 RC Unknown R14 byte, has always been 0
				this._writer.WriteByte(0);
			}

			//R2000 +:
			if (this.R2000Plus)
			{
				//Cloning flag BS 281
				this._writer.WriteBitShort((short)dictionary.ClonningFlags);
				this._writer.WriteByte((byte)(dictionary.HardOwnerFlag ? 1u : 0u));
			}

			//Common:
			foreach (var name in dictionary.EntryNames)
			{
				this._writer.WriteVariableText(name);
			}

			foreach (var handle in dictionary.EntryHandles)
			{
				this._writer.HandleReference(DwgReferenceType.SoftOwnership, handle);
			}

			this.addEntriesToWriter(dictionary);
		}

		private void addEntriesToWriter(CadDictionary dictionary)
		{
			foreach (CadObject e in dictionary)
			{
				this._objects.Enqueue(e);
			}
		}

		private void writeGroup(Group group)
		{
			//Str TV name of group
			this._writer.WriteVariableText(group.Description);

			//Unnamed BS 1 if group has no name
			this._writer.WriteBitShort((short)(group.IsUnnamed ? 1 : 0));
			//Selectable BS 1 if group selectable
			this._writer.WriteBitShort((short)(group.Selectable ? 1 : 0));

			//Numhandles BL # objhandles in this group
			this._writer.WriteBitLong(group.Entities.Count);
			foreach (ulong h in group.Entities.Keys)
			{
				//the entries in the group(hard pointer)
				this._writer.HandleReference(DwgReferenceType.HardPointer, h);
			}
		}

		private void writeLayout(Layout layout)
		{
			this.writePlotSettings(layout);

			//Common:
			//Layout name TV 1 layout name
			this._writer.WriteVariableText(layout.Name);
			//Tab order BL 71 layout tab order
			this._writer.WriteBitLong(layout.TabOrder);
			//Flag BS 70 layout flags
			this._writer.WriteBitShort((short)layout.LayoutFlags);
			//Ucs origin 3BD 13 layout ucs origin
			this._writer.Write3BitDouble(layout.Origin);
			//Limmin 2RD 10 layout minimum limits
			this._writer.Write2RawDouble(layout.MinLimits);
			//Limmax 2RD 11 layout maximum limits
			this._writer.Write2RawDouble(layout.MinLimits);
			//Inspoint 3BD 12 layout insertion base point
			this._writer.Write3BitDouble(layout.InsertionBasePoint);
			this._writer.Write3BitDouble(layout.XAxis);
			this._writer.Write3BitDouble(layout.YAxis);
			this._writer.WriteBitDouble(layout.Elevation);
			this._writer.WriteBitShort((short)layout.UcsOrthographicType);
			this._writer.Write3BitDouble(layout.MinExtents);
			this._writer.Write3BitDouble(layout.MaxExtents);

			//R2004 +:
			if (this.R2004Plus)
			{
				//Viewport count RL # of viewports in this layout
				this._writer.WriteBitLong(layout.Viewports.Count());
			}

			//Common:
			//330 associated paperspace block record handle(soft pointer)
			this._writer.HandleReference(DwgReferenceType.SoftPointer, layout.AssociatedBlock);
			//331 last active viewport handle(soft pointer)
			this._writer.HandleReference(DwgReferenceType.SoftPointer, layout.Viewport);

			//If not present and 76 code is non-zero, then base UCS is taken to be WORLD
			if (layout.UcsOrthographicType == OrthographicType.None)
			{
				//346 base ucs handle(hard pointer)
				this._writer.HandleReference(DwgReferenceType.HardPointer, null);
				//345 named ucs handle(hard pointer)
				this._writer.HandleReference(DwgReferenceType.HardPointer, layout.UCS);
			}
			else
			{
				//346 base ucs handle(hard pointer)
				this._writer.HandleReference(DwgReferenceType.HardPointer, layout.BaseUCS);
				//345 named ucs handle(hard pointer)
				this._writer.HandleReference(DwgReferenceType.HardPointer, null);
			}

			//R2004+:
			if (this.R2004Plus)
			{
				foreach (Entities.Viewport viewport in layout.Viewports)
				{
					//Viewport handle(repeats Viewport count times) (soft pointer)
					this._writer.HandleReference(DwgReferenceType.SoftPointer, viewport);
				}
			}
		}

		private void writePlotSettings(PlotSettings plot)
		{
			//Common:
			//Page setup name TV 1 plotsettings page setup name
			this._writer.WriteVariableText(plot.PageName);
			//Printer / Config TV 2 plotsettings printer or configuration file
			this._writer.WriteVariableText(plot.SystemPrinterName);
			//Plot layout flags BS 70 plotsettings plot layout flag
			this._writer.WriteBitShort((short)plot.Flags);

			//Left Margin BD 40 plotsettings left margin in millimeters
			this._writer.WriteBitDouble(plot.UnprintableMargin.Left);
			//Bottom Margin BD 41 plotsettings bottom margin in millimeters
			this._writer.WriteBitDouble(plot.UnprintableMargin.Bottom);
			//Right Margin BD 42 plotsettings right margin in millimeters
			this._writer.WriteBitDouble(plot.UnprintableMargin.Right);
			//Top Margin BD 43 plotsettings top margin in millimeters
			this._writer.WriteBitDouble(plot.UnprintableMargin.Top);

			//Paper Width BD 44 plotsettings paper width in millimeters
			this._writer.WriteBitDouble(plot.PaperWidth);
			//Paper Height BD 45 plotsettings paper height in millimeters
			this._writer.WriteBitDouble(plot.PaperHeight);

			//Paper Size TV 4 plotsettings paper size
			this._writer.WriteVariableText(plot.PaperSize);

			//Plot origin 2BD 46,47 plotsettings origin offset in millimeters
			this._writer.WriteBitDouble(plot.PlotOriginX);
			this._writer.WriteBitDouble(plot.PlotOriginY);

			//Paper units BS 72 plotsettings plot paper units
			this._writer.WriteBitShort((short)plot.PaperUnits);
			//Plot rotation BS 73 plotsettings plot rotation
			this._writer.WriteBitShort((short)plot.PaperRotation);
			//Plot type BS 74 plotsettings plot type
			this._writer.WriteBitShort((short)plot.PlotType);

			//Window min 2BD 48,49 plotsettings plot window area lower left
			this._writer.WriteBitDouble(plot.WindowLowerLeftX);
			this._writer.WriteBitDouble(plot.WindowLowerLeftY);
			//Window max 2BD 140,141 plotsettings plot window area upper right
			this._writer.WriteBitDouble(plot.WindowUpperLeftX);
			this._writer.WriteBitDouble(plot.WindowUpperLeftY);

			//R13 - R2000 Only:
			if (this._version >= ACadVersion.AC1012 && this._version <= ACadVersion.AC1015)
			{
				//Plot view name T 6 plotsettings plot view name
				this._writer.WriteVariableText(plot.PlotViewName);
			}

			//Common:
			//Real world units BD 142 plotsettings numerator of custom print scale
			this._writer.WriteBitDouble(plot.NumeratorScale);
			//Drawing units BD 143 plotsettings denominator of custom print scale
			this._writer.WriteBitDouble(plot.DenominatorScale);
			//Current style sheet TV 7 plotsettings current style sheet
			this._writer.WriteVariableText(plot.StyleSheet);
			//Scale type BS 75 plotsettings standard scale type
			this._writer.WriteBitShort((short)plot.ScaledFit);
			//Scale factor BD 147 plotsettings scale factor
			this._writer.WriteBitDouble(plot.StandardScale);
			//Paper image origin 2BD 148,149 plotsettings paper image origin
			this._writer.Write2BitDouble(plot.PaperImageOrigin);

			//R2004+:
			if (this.R2004Plus)
			{
				//Shade plot mode BS 76
				this._writer.WriteBitShort((short)plot.ShadePlotMode);
				//Shade plot res.Level BS 77
				this._writer.WriteBitShort((short)plot.ShadePlotResolutionMode);
				//Shade plot custom DPI BS 78
				this._writer.WriteBitShort(plot.ShadePlotDPI);

				//6 plot view handle(hard pointer)
				this._writer.HandleReference(DwgReferenceType.HardPointer, null);
			}

			//R2007 +:
			if (this.R2007Plus)
			{
				//Visual Style handle(soft pointer)
				this._writer.HandleReference(DwgReferenceType.SoftPointer, null);
			}
		}

		private void writeXRecord(XRecord xrecord)
		{
			//Common:
			//Numdatabytes BL number of databytes
		}

		private void writeXRecordEntry(XRecord.Entry entry)
		{
		}
	}
}
