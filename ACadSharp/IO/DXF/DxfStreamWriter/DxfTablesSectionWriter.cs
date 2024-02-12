using ACadSharp.Tables;
using ACadSharp.Tables.Collections;
using System;
using System.Linq;

namespace ACadSharp.IO.DXF
{
	internal class DxfTablesSectionWriter : DxfSectionWriterBase
	{
		public override string SectionName { get { return DxfFileToken.TablesSection; } }

		public DxfTablesSectionWriter(IDxfStreamWriter writer, CadDocument document, CadObjectHolder holder) : base(writer, document, holder)
		{
		}

		protected override void writeSection()
		{
			this.writeTable(this._document.VPorts);
			this.writeTable(this._document.LineTypes);
			this.writeTable(this._document.Layers);
			this.writeTable(this._document.TextStyles);
			this.writeTable(this._document.Views);
			this.writeTable(this._document.UCSs);
			this.writeTable(this._document.AppIds);
			this.writeTable(this._document.DimensionStyles, DxfSubclassMarker.DimensionStyleTable);
			this.writeTable(this._document.BlockRecords);
		}

		private void writeTable<T>(Table<T> table, string subclass = null)
			where T : TableEntry
		{
			this._writer.Write(DxfCode.Start, DxfFileToken.EntityTable);
			this._writer.Write(DxfCode.SymbolTableName, table.ObjectName);

			this.writeCommonObjectData(table);

			this._writer.Write(DxfCode.Subclass, DxfSubclassMarker.Table);

			this._writer.Write(70, table.Count);

			if (!string.IsNullOrEmpty(subclass))
			{
				this._writer.Write(DxfCode.Subclass, subclass);
			}

			foreach (T entry in table)
			{
				writeEntry(entry);
			}

			this._writer.Write(DxfCode.Start, DxfFileToken.EndTable);
		}

		private void writeEntry<T>(T entry)
			where T : TableEntry
		{
			DxfMap map = DxfMap.Create<T>();

			this._writer.Write(DxfCode.Start, entry.ObjectName);

			this.writeCommonObjectData(entry);

			this._writer.Write(DxfCode.Subclass, DxfSubclassMarker.TableRecord);
			this._writer.Write(DxfCode.Subclass, entry.SubclassMarker);
			this._writer.Write(DxfCode.SymbolTableName, entry.Name);
			this._writer.Write(70, entry.Flags);

			switch (entry)
			{
				case AppId appId:
					break;
				case BlockRecord block:
					this.writeBlockRecord(block, map.SubClasses[block.SubclassMarker]);
					break;
				case DimensionStyle style:
					this.writeDimensionStyle(style, map.SubClasses[style.SubclassMarker]);
					break;
				case Layer layer:
					this.writeLayer(layer, map.SubClasses[layer.SubclassMarker]);
					break;
				case LineType ltype:
					this.writeLineType(ltype, map.SubClasses[ltype.SubclassMarker]);
					break;
				case TextStyle textStyle:
					this.writeTextStyle(textStyle, map.SubClasses[textStyle.SubclassMarker]);
					break;
				case UCS ucs:
					this.writeUcs(ucs, map.SubClasses[ucs.SubclassMarker]);
					break;
				case View view:
					this.writeView(view, map.SubClasses[view.SubclassMarker]);
					break;
				case VPort vport:
					this.writeVPort(vport, map.SubClasses[vport.SubclassMarker]);
					break;
#if TEST
				default:
					throw new NotImplementedException();
#endif
			}

			this.writeExtendedData(entry);
		}

		private void writeBlockRecord(BlockRecord block, DxfClassMap map)
		{
			this._writer.WriteHandle(340, block.Layout, map);

			this._writer.Write(70, (short)block.Units, map);
			this._writer.Write(280, (byte)(block.IsExplodable ? 1u : 0u), map);
			this._writer.Write(281, (byte)(block.CanScale ? 1u : 0u), map);
		}

		private void writeDimensionStyle(DimensionStyle style, DxfClassMap map)
		{
			this._writer.Write(3, style.PostFix, map);
			this._writer.Write(4, style.AlternateDimensioningSuffix, map);

			this._writer.Write(40, style.ScaleFactor, map);
			this._writer.Write(41, style.ArrowSize, map);
			this._writer.Write(42, style.ExtensionLineOffset, map);
			this._writer.Write(43, style.DimensionLineIncrement, map);
			this._writer.Write(44, style.ExtensionLineExtension, map);
			this._writer.Write(45, style.Rounding, map);
			this._writer.Write(46, style.DimensionLineExtension, map);
			this._writer.Write(47, style.PlusTolerance, map);
			this._writer.Write(48, style.MinusTolerance, map);
			this._writer.Write(49, style.FixedExtensionLineLength, map);
			this._writer.Write(50, style.JoggedRadiusDimensionTransverseSegmentAngle, map);

			if (style.TextBackgroundFillMode != 0)
			{
				this._writer.Write(69, (short)style.TextBackgroundFillMode, map);
				this._writer.Write(70, style.TextBackgroundColor.Index, map);
			}
			else
			{
				this._writer.Write(70, 0, map);
			}

			if (style.ArcLengthSymbolPosition != ArcLengthSymbolPosition.AboveDimensionText)
			{
				this._writer.Write(90, (int)style.ArcLengthSymbolPosition);
			}

			this._writer.Write(140, style.TextHeight);
			this._writer.Write(141, style.CenterMarkSize);
			this._writer.Write(142, style.TickSize);
			this._writer.Write(143, style.AlternateUnitScaleFactor);
			this._writer.Write(144, style.LinearScaleFactor);
			this._writer.Write(145, style.TextVerticalPosition);
			this._writer.Write(146, style.ToleranceScaleFactor);
			this._writer.Write(147, style.DimensionLineGap);
			this._writer.Write(148, style.AlternateUnitRounding);

			this._writer.Write(71, (short)(style.GenerateTolerances ? 1 : 0));
			this._writer.Write(72, (short)(style.LimitsGeneration ? 1 : 0));
			this._writer.Write(73, (short)(style.TextInsideHorizontal ? 1 : 0));
			this._writer.Write(74, (short)(style.TextOutsideHorizontal ? 1 : 0));
			this._writer.Write(75, (short)(style.SuppressFirstExtensionLine ? 1 : 0));
			this._writer.Write(76, (short)(style.SuppressSecondExtensionLine ? 1 : 0));
			this._writer.Write(77, (short)style.TextVerticalAlignment);
			this._writer.Write(78, (short)style.ZeroHandling);
			this._writer.Write(79, (short)style.AngularZeroHandling);

			this._writer.Write(170, (short)(style.AlternateUnitDimensioning ? 1 : 0));
			this._writer.Write(171, style.AlternateUnitDecimalPlaces);
			this._writer.Write(172, (short)(style.TextOutsideExtensions ? 1 : 0));
			this._writer.Write(173, (short)(style.SeparateArrowBlocks ? 1 : 0));
			this._writer.Write(174, (short)(style.TextInsideExtensions ? 1 : 0));
			this._writer.Write(175, (short)(style.SuppressOutsideExtensions ? 1 : 0));
			this._writer.Write(176, style.DimensionLineColor.Index);
			this._writer.Write(177, style.ExtensionLineColor.Index);
			this._writer.Write(178, style.TextColor.Index);
			this._writer.Write(179, style.AngularDimensionDecimalPlaces);

			this._writer.Write(271, style.DecimalPlaces);
			this._writer.Write(272, style.ToleranceDecimalPlaces);
			this._writer.Write(273, (short)style.AlternateUnitFormat);
			this._writer.Write(274, style.AlternateUnitToleranceDecimalPlaces);
			this._writer.Write(275, (short)style.AngularUnit);
			this._writer.Write(276, (short)style.FractionFormat);
			this._writer.Write(277, (short)style.LinearUnitFormat);
			this._writer.Write(278, (short)style.DecimalSeparator);
			this._writer.Write(279, (short)style.TextMovement);
			this._writer.Write(280, (byte)style.TextHorizontalAlignment);
			this._writer.Write(281, style.SuppressFirstDimensionLine);
			this._writer.Write(282, style.SuppressSecondDimensionLine);
			this._writer.Write(283, (byte)style.ToleranceAlignment);
			this._writer.Write(284, (byte)style.ToleranceZeroHandling);
			this._writer.Write(285, (byte)style.AlternateUnitZeroHandling);
			this._writer.Write(286, (byte)style.AlternateUnitToleranceZeroHandling);
			this._writer.Write(287, (byte)style.DimensionFit);
			this._writer.Write(288, style.CursorUpdate);
			this._writer.Write(289, (byte)style.DimensionTextArrowFit);
			this._writer.Write(290, style.IsExtensionLineLengthFixed);

			this._writer.WriteHandle(340, style.Style, map);
			this._writer.WriteHandle(341, style.LeaderArrow, map);
			this._writer.WriteHandle(342, style.ArrowBlock, map);
			this._writer.WriteHandle(343, style.DimArrow1, map);
			this._writer.WriteHandle(344, style.DimArrow2, map);

			this._writer.Write(371, style.DimensionLineWeight);
			this._writer.Write(372, style.ExtensionLineWeight);
		}

		private void writeLayer(Layer layer, DxfClassMap map)
		{
			if (layer.IsOn)
			{
				this._writer.Write(62, layer.Color.Index, map);
			}
			else
			{
				this._writer.Write(62, (short)-layer.Color.Index, map);
			}

			if (layer.Color.IsTrueColor)
			{
				this._writer.Write(420, (uint)layer.Color.TrueColor, map);
			}

			this._writer.Write(6, layer.LineType.Name, map);

			this._writer.Write(290, layer.PlotFlag, map);

			this._writer.Write(370, (short)layer.LineWeight, map);

			//this._writer.Write(390, layer.PlotStyleName, map);
			this._writer.Write(390, (ulong)0, map);
		}

		private void writeLineType(LineType linetype, DxfClassMap map)
		{
			this._writer.Write(3, linetype.Description, map);

			this._writer.Write(72, (short)linetype.Alignment, map);
			this._writer.Write(73, (short)linetype.Segments.Count(), map);
			this._writer.Write(40, linetype.PatternLen);

			foreach (LineType.Segment s in linetype.Segments)
			{
				this._writer.Write(49, s.Length);
				this._writer.Write(74, (short)s.Shapeflag);

				if (s.Shapeflag != LinetypeShapeFlags.None)
				{
					if (s.Shapeflag.HasFlag(LinetypeShapeFlags.Shape))
					{
						this._writer.Write(75, s.ShapeNumber);
					}
					if (s.Shapeflag.HasFlag(LinetypeShapeFlags.Text))
					{
						this._writer.Write(75, (short)0);
					}

					if (s.Style == null)
					{
						this._writer.Write(340, 0uL);
					}
					else
					{
						this._writer.Write(340, s.Style.Handle);
					}

					this._writer.Write(46, s.Scale);
					this._writer.Write(50, s.Rotation * MathUtils.DegToRad);
					this._writer.Write(44, s.Offset.X);
					this._writer.Write(45, s.Offset.Y);
					this._writer.Write(9, s.Text);
				}
			}
		}

		private void writeTextStyle(TextStyle textStyle, DxfClassMap map)
		{
			if (!string.IsNullOrEmpty(textStyle.Filename))
			{
				this._writer.Write(3, textStyle.Filename, map);
			}

			if (!string.IsNullOrEmpty(textStyle.BigFontFilename))
			{
				this._writer.Write(4, textStyle.BigFontFilename);
			}

			this._writer.Write(40, textStyle.Height, map);
			this._writer.Write(41, textStyle.Width, map);
			this._writer.Write(42, textStyle.LastHeight, map);
			this._writer.Write(50, textStyle.ObliqueAngle, map);
			this._writer.Write(71, textStyle.MirrorFlag, map);
		}

		private void writeUcs(UCS ucs, DxfClassMap map)
		{
			this._writer.Write(10, ucs.Origin.X, map);
			this._writer.Write(20, ucs.Origin.Y, map);
			this._writer.Write(30, ucs.Origin.Z, map);

			this._writer.Write(11, ucs.XAxis.X, map);
			this._writer.Write(21, ucs.XAxis.Y, map);
			this._writer.Write(31, ucs.XAxis.Z, map);

			this._writer.Write(12, ucs.YAxis.X, map);
			this._writer.Write(22, ucs.YAxis.Y, map);
			this._writer.Write(32, ucs.YAxis.Z, map);

			this._writer.Write(71, ucs.OrthographicType, map);
			this._writer.Write(79, ucs.OrthographicViewType, map);
			this._writer.Write(146, ucs.Elevation, map);
		}

		private void writeView(View view, DxfClassMap map)
		{
			this._writer.Write(40, view.Height, map);
			this._writer.Write(41, view.Width, map);

			this._writer.Write(42, view.LensLength, map);
			this._writer.Write(43, view.FrontClipping, map);
			this._writer.Write(44, view.BackClipping, map);

			this._writer.Write(10, view.Center, map);
			this._writer.Write(11, view.Direction, map);
			this._writer.Write(12, view.Target, map);

			this._writer.Write(50, view.Angle, map);

			this._writer.Write(71, (short)view.ViewMode);
			this._writer.Write(72, view.IsUcsAssociated, map);
			this._writer.Write(79, (short)view.UcsOrthographicType);

			this._writer.Write(281, (byte)view.RenderMode);

			this._writer.Write(110, view.UcsOrigin);
			this._writer.Write(111, view.UcsXAxis);
			this._writer.Write(112, view.UcsYAxis);

			this._writer.Write(146, view.UcsElevation);
		}

		private void writeVPort(VPort vport, DxfClassMap map)
		{
			this._writer.Write(10, vport.BottomLeft, map);

			this._writer.Write(11, vport.TopRight, map);

			this._writer.Write(12, vport.Center, map);

			this._writer.Write(13, vport.SnapBasePoint, map);

			this._writer.Write(14, vport.SnapSpacing, map);

			this._writer.Write(15, vport.GridSpacing, map);

			this._writer.Write(16, vport.Direction, map);

			this._writer.Write(17, vport.Target, map);

			this._writer.Write(40, vport.ViewHeight);
			this._writer.Write(41, vport.AspectRatio);

			this._writer.Write(75, vport.SnapOn ? (short)1 : (short)0);
			this._writer.Write(76, vport.ShowGrid ? (short)1 : (short)0);
		}
	}
}
