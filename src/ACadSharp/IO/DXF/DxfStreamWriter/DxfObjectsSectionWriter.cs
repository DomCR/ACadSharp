using ACadSharp.Entities;
using ACadSharp.Objects;
using ACadSharp.Objects.Evaluations;
using CSMath;
using CSUtilities.Converters;
using System;
using System.Linq;

namespace ACadSharp.IO.DXF
{
	internal class DxfObjectsSectionWriter : DxfSectionWriterBase
	{
		public override string SectionName { get { return DxfFileToken.ObjectsSection; } }

		public DxfObjectsSectionWriter(IDxfStreamWriter writer, CadDocument document, CadObjectHolder holder, DxfWriterConfiguration configuration)
			: base(writer, document, holder, configuration)
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
				case EvaluationGraph:
				case Material:
				case MultiLeaderObjectContextData:
				case VisualStyle:
				case ImageDefinitionReactor:
				case UnknownNonGraphicalObject:
				case ProxyObject:
				case XRecord:
					this.notify($"Object not implemented : {co.GetType().FullName}");
					return;
			}


			if (co is XRecord && !this.Configuration.WriteXRecords)
			{
				return;
			}

			this._writer.Write(DxfCode.Start, co.ObjectName);

			this.writeCommonObjectData(co);

			switch (co)
			{
				case BookColor bookColor:
					this.writeBookColor(bookColor);
					return;
				case CadDictionary cadDictionary:
					this.writeDictionary(cadDictionary);
					return;
				case DictionaryVariable dictvar:
					this.writeDictionaryVariable(dictvar);
					break;
				case GeoData geodata:
					this.writeGeoData(geodata);
					break;
				case Group group:
					this.writeGroup(group);
					break;
				case ImageDefinition imageDefinition:
					this.writeImageDefinition(imageDefinition);
					return;
				case Layout layout:
					this.writeLayout(layout);
					break;
				case MLineStyle mlStyle:
					this.writeMLineStyle(mlStyle);
					break;
				case MultiLeaderStyle multiLeaderlStyle:
					this.writeMultiLeaderStyle(multiLeaderlStyle);
					break;
				case PlotSettings plotSettings:
					this.writePlotSettings(plotSettings);
					break;
				case PdfUnderlayDefinition pdfUnderlayDefinition:
					this.writePdfUnderlayDefinition(pdfUnderlayDefinition);
					break;
				case RasterVariables rasterVariables:
					this.writeRasterVariables(rasterVariables);
					break;
				case Scale scale:
					this.writeScale(scale);
					break;
				case SpatialFilter spatialFilter:
					this.writeSpatialFilter(spatialFilter);
					break;
				case SortEntitiesTable sortensTable:
					this.writeSortentsTable(sortensTable);
					break;
				case XRecord record:
					this.writeXRecord(record);
					break;
				default:
					throw new NotImplementedException($"Object not implemented : {co.GetType().FullName}");
			}

			this.writeExtendedData(co.ExtendedData);
		}

		protected void writeBookColor(BookColor color)
		{
			this._writer.Write(DxfCode.Subclass, DxfSubclassMarker.DbColor);

			this._writer.Write(62, color.Color.GetApproxIndex());
			this._writer.WriteTrueColor(420, color.Color);
			this._writer.Write(430, $"{color.Name}${color.BookName}");
		}

		protected void writeDictionary(CadDictionary dict)
		{
			this._writer.Write(DxfCode.Subclass, DxfSubclassMarker.Dictionary);

			this._writer.Write(280, dict.HardOwnerFlag);
			this._writer.Write(281, (int)dict.ClonningFlags);

			foreach (NonGraphicalObject item in dict)
			{
				if (item is XRecord && !this.Configuration.WriteXRecords)
				{
					return;
				}

				this._writer.Write(3, item.Name);
				this._writer.Write(350, item.Handle);
			}

			//Add the entries as objects
			foreach (CadObject item in dict)
			{
				this.Holder.Objects.Enqueue(item);
			}

			if(dict is CadDictionaryWithDefault withDefault)
			{
				this._writer.Write(100, DxfSubclassMarker.DictionaryWithDefault);
				this._writer.WriteHandle(340, withDefault.DefaultEntry);
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

		protected void writePdfUnderlayDefinition(PdfUnderlayDefinition definition)
		{
			DxfClassMap map = DxfClassMap.Create<PlotSettings>();

			this._writer.Write(100, DxfSubclassMarker.UnderlayDefinition);

			this._writer.Write(1, definition.File, map);
			this._writer.Write(2, definition.Page, map);
		}

		protected void writeRasterVariables(RasterVariables variables)
		{
			DxfClassMap map = DxfClassMap.Create<RasterVariables>();

			this._writer.Write(100, DxfSubclassMarker.RasterVariables);

			this._writer.Write(90, variables.ClassVersion, map);
			this._writer.Write(70, variables.IsDisplayFrameShown ? 1 : 0, map);
			this._writer.Write(71, (short)variables.DisplayQuality, map);
			this._writer.Write(72, (short)variables.DisplayQuality, map);
		}

		protected void writeScale(Scale scale)
		{
			this._writer.Write(100, DxfSubclassMarker.Scale);

			this._writer.Write(70, 0);
			this._writer.Write(300, scale.Name);
			this._writer.Write(140, scale.PaperUnits);
			this._writer.Write(141, scale.DrawingUnits);
			this._writer.Write(290, scale.IsUnitScale ? (short)1 : (short)0);
		}

		protected void writeGeoData(GeoData geodata)
		{
			DxfClassMap map = DxfClassMap.Create<GeoData>();

			this._writer.Write(100, DxfSubclassMarker.GeoData, map);

			switch (this.Version)
			{
				case ACadVersion.Unknown:
				case ACadVersion.MC0_0:
				case ACadVersion.AC1_2:
				case ACadVersion.AC1_4:
				case ACadVersion.AC1_50:
				case ACadVersion.AC2_10:
				case ACadVersion.AC1002:
				case ACadVersion.AC1003:
				case ACadVersion.AC1004:
				case ACadVersion.AC1006:
				case ACadVersion.AC1009:
				case ACadVersion.AC1012:
				case ACadVersion.AC1014:
				case ACadVersion.AC1015:
				case ACadVersion.AC1018:
				case ACadVersion.AC1021:
					this._writer.Write(90, 1, map);
					break;
				case ACadVersion.AC1024:
					this._writer.Write(90, 2, map);
					break;
				case ACadVersion.AC1027:
				case ACadVersion.AC1032:
					this._writer.Write(90, 3, map);
					break;
			}

			if (geodata.HostBlock != null)
			{
				this._writer.Write(330, geodata.HostBlock.Handle, map);
			}

			this._writer.Write(70, (short)geodata.CoordinatesType, map);

			if (this.Version <= ACadVersion.AC1021)
			{
				this._writer.Write(40, geodata.ReferencePoint.Y, map);
				this._writer.Write(41, geodata.ReferencePoint.X, map);
				this._writer.Write(42, geodata.ReferencePoint.Z, map);
				this._writer.Write(91, (int)geodata.HorizontalUnits, map);

				this._writer.Write(10, geodata.DesignPoint, map);
				this._writer.Write(11, XYZ.Zero, map);

				this._writer.Write(210, geodata.UpDirection, map);

				this._writer.Write(52, MathHelper.RadToDeg(System.Math.PI / 2.0 - geodata.NorthDirection.GetAngle()), map);

				this._writer.Write(43, 1.0, map);
				this._writer.Write(44, 1.0, map);
				this._writer.Write(45, 1.0, map);

				this._writer.Write(301, geodata.CoordinateSystemDefinition, map);
				this._writer.Write(302, geodata.GeoRssTag, map);

				this._writer.Write(46, geodata.UserSpecifiedScaleFactor, map);

				this._writer.Write(303, string.Empty, map);
				this._writer.Write(304, string.Empty, map);

				this._writer.Write(305, geodata.ObservationFromTag, map);
				this._writer.Write(306, geodata.ObservationToTag, map);
				this._writer.Write(307, geodata.ObservationCoverageTag, map);

				this._writer.Write(93, geodata.Points.Count, map);
				foreach (var pt in geodata.Points)
				{
					this._writer.Write(12, pt.Source, map);
					this._writer.Write(13, pt.Destination, map);
				}
				this._writer.Write(96, geodata.Faces.Count, map);
				foreach (var face in geodata.Faces)
				{
					this._writer.Write(97, face.Index1, map);
					this._writer.Write(98, face.Index2, map);
					this._writer.Write(99, face.Index3, map);
				}
				this._writer.Write(3, "CIVIL3D_DATA_BEGIN", map);

				this._writer.Write(292, false, map);
				this._writer.Write(14, geodata.ReferencePoint.Convert<XY>(), map);
				this._writer.Write(15, geodata.ReferencePoint.Convert<XY>(), map);
				this._writer.Write(93, 0, map);
				this._writer.Write(94, 0, map);
				this._writer.Write(293, false, map);

				this._writer.Write(16, XY.Zero, map);
				this._writer.Write(17, XY.Zero, map);

				this._writer.Write(54, MathHelper.RadToDeg(System.Math.PI / 2.0 - geodata.NorthDirection.GetAngle()), map);
				this._writer.Write(140, System.Math.PI / 2.0 - geodata.NorthDirection.GetAngle(), map);

				this._writer.Write(95, (int)geodata.ScaleEstimationMethod, map);
				this._writer.Write(141, geodata.UserSpecifiedScaleFactor, map);
				this._writer.Write(294, geodata.EnableSeaLevelCorrection, map);
				this._writer.Write(142, geodata.SeaLevelElevation, map);
				this._writer.Write(143, geodata.CoordinateProjectionRadius, map);

				this._writer.Write(4, "CIVIL3D_DATA_END", map);
			}
			else
			{
				this._writer.Write(10, geodata.DesignPoint, map);
				this._writer.Write(11, geodata.ReferencePoint, map);
				this._writer.Write(40, geodata.VerticalUnitScale, map);
				this._writer.Write(91, (int)geodata.HorizontalUnits, map);
				this._writer.Write(41, geodata.VerticalUnitScale, map);
				this._writer.Write(92, (int)geodata.VerticalUnits, map);

				this._writer.Write(210, geodata.UpDirection, map);

				this._writer.Write(12, geodata.NorthDirection, map);

				this._writer.Write(95, geodata.ScaleEstimationMethod, map);
				this._writer.Write(141, geodata.UserSpecifiedScaleFactor, map);
				this._writer.Write(294, geodata.EnableSeaLevelCorrection, map);
				this._writer.Write(142, geodata.SeaLevelElevation, map);
				this._writer.Write(143, geodata.CoordinateProjectionRadius, map);

				this.writeLongTextValue(301, 303, geodata.CoordinateSystemDefinition);

				this._writer.Write(302, geodata.GeoRssTag, map);
				this._writer.Write(305, geodata.ObservationFromTag, map);
				this._writer.Write(306, geodata.ObservationToTag, map);
				this._writer.Write(307, geodata.ObservationCoverageTag, map);

				this._writer.Write(93, geodata.Points.Count, map);
				foreach (var pt in geodata.Points)
				{
					this._writer.Write(13, pt.Source, map);
					this._writer.Write(14, pt.Destination, map);
				}

				this._writer.Write(96, geodata.Faces.Count, map);

				foreach (var face in geodata.Faces)
				{
					this._writer.Write(97, face.Index1, map);
					this._writer.Write(98, face.Index2, map);
					this._writer.Write(99, face.Index3, map);
				}
			}
		}

		protected void writeGroup(Group group)
		{
			this._writer.Write(100, DxfSubclassMarker.Group);

			this._writer.Write(300, group.Description);
			this._writer.Write(70, group.IsUnnamed ? (short)1 : (short)0);
			this._writer.Write(71, group.Selectable ? (short)1 : (short)0);

			foreach (Entity entity in group.Entities)
			{
				this._writer.WriteHandle(340, entity);
			}
		}

		protected void writeImageDefinition(ImageDefinition definition)
		{
			DxfClassMap map = DxfClassMap.Create<ImageDefinition>();

			this._writer.Write(100, DxfSubclassMarker.RasterImageDef);

			this._writer.Write(90, definition.ClassVersion, map);
			this._writer.Write(1, definition.FileName, map);

			this._writer.Write(10, definition.Size, map);

			this._writer.Write(280, definition.IsLoaded ? 1 : 0, map);

			this._writer.Write(281, (byte)definition.Units, map);
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

			this._writer.WriteHandle(330, layout.AssociatedBlock, map);
		}

		protected void writeMLineStyle(MLineStyle style)
		{
			DxfClassMap map = DxfClassMap.Create<MLineStyle>();

			this._writer.Write(100, DxfSubclassMarker.MLineStyle);

			this._writer.Write(2, style.Name, map);

			this._writer.Write(70, (short)style.Flags, map);

			this._writer.Write(3, style.Description, map);

			this._writer.Write(62, style.FillColor.GetApproxIndex(), map);

			this._writer.Write(51, style.StartAngle, map);
			this._writer.Write(52, style.EndAngle, map);
			this._writer.Write(71, (short)style.Elements.Count, map);
			foreach (MLineStyle.Element element in style.Elements)
			{
				this._writer.Write(49, element.Offset, map);
				this._writer.Write(62, element.Color.Index, map);
				this._writer.Write(6, element.LineType.Name, map);
			}
		}

		protected void writeMultiLeaderStyle(MultiLeaderStyle style)
		{
			DxfClassMap map = DxfClassMap.Create<MultiLeaderStyle>();

			this._writer.Write(100, DxfSubclassMarker.MLeaderStyle);

			this._writer.Write(179, 2);
			//	this._writer.Write(2, style.Name, map);
			this._writer.Write(170, (short)style.ContentType, map);
			this._writer.Write(171, (short)style.MultiLeaderDrawOrder, map);
			this._writer.Write(172, (short)style.LeaderDrawOrder, map);
			this._writer.Write(90, style.MaxLeaderSegmentsPoints, map);
			this._writer.Write(40, style.FirstSegmentAngleConstraint, map);
			this._writer.Write(41, style.SecondSegmentAngleConstraint, map);
			this._writer.Write(173, (short)style.PathType, map);
			this._writer.WriteCmColor(91, style.LineColor, map);
			this._writer.WriteHandle(340, style.LeaderLineType);
			this._writer.Write(92, (short)style.LeaderLineWeight, map);
			this._writer.Write(290, style.EnableLanding, map);
			this._writer.Write(42, style.LandingGap, map);
			this._writer.Write(291, style.EnableDogleg, map);
			this._writer.Write(43, style.LandingDistance, map);
			this._writer.Write(3, style.Description, map);
			this._writer.WriteHandle(341, style.Arrowhead);
			this._writer.Write(44, style.ArrowheadSize, map);
			this._writer.Write(300, style.DefaultTextContents, map);
			this._writer.WriteHandle(342, style.TextStyle);
			this._writer.Write(174, (short)style.TextLeftAttachment, map);
			this._writer.Write(178, (short)style.TextRightAttachment, map);
			this._writer.Write(175, style.TextAngle, map);
			this._writer.Write(176, (short)style.TextAlignment, map);
			this._writer.WriteCmColor(93, style.TextColor, map);
			this._writer.Write(45, style.TextHeight, map);
			this._writer.Write(292, style.TextFrame, map);
			this._writer.Write(297, style.TextAlignAlwaysLeft, map);
			this._writer.Write(46, style.AlignSpace, map);
			this._writer.WriteHandle(343, style.BlockContent);
			this._writer.WriteCmColor(94, style.BlockContentColor, map);

			//	Write 3 doubles since group codes do not conform vector group codes
			this._writer.Write(47, style.BlockContentScale.X, map);
			this._writer.Write(49, style.BlockContentScale.Y, map);
			this._writer.Write(140, style.BlockContentScale.Z, map);

			this._writer.Write(293, style.EnableBlockContentScale, map);
			this._writer.Write(141, style.BlockContentRotation, map);
			this._writer.Write(294, style.EnableBlockContentRotation, map);
			this._writer.Write(177, (short)style.BlockContentConnection, map);
			this._writer.Write(142, style.ScaleFactor, map);
			this._writer.Write(295, style.OverwritePropertyValue, map);
			this._writer.Write(296, style.IsAnnotative, map);
			this._writer.Write(143, style.BreakGapSize, map);
			this._writer.Write(271, (short)style.TextAttachmentDirection, map);
			this._writer.Write(272, (short)style.TextBottomAttachment, map);
			this._writer.Write(273, (short)style.TextTopAttachment, map);
			this._writer.Write(298, false); //	undocumented
		}

		private void writeSpatialFilter(SpatialFilter filter)
		{
			DxfClassMap map = DxfClassMap.Create<SpatialFilter>();

			this._writer.Write(100, DxfSubclassMarker.Filter);
			this._writer.Write(100, DxfSubclassMarker.SpatialFilter);

			this._writer.Write(70, (short)filter.BoundaryPoints.Count, map);
			foreach (var pt in filter.BoundaryPoints)
			{
				this._writer.Write(10, pt, map);
			}

			this._writer.Write(210, filter.Normal, map);
			this._writer.Write(11, filter.Origin, map);
			this._writer.Write(71, (short)(filter.DisplayBoundary ? 1 : 0), map);

			this._writer.Write(72, filter.ClipFrontPlane ? 1 : 0, map);
			if (filter.ClipFrontPlane)
			{
				this._writer.Write(40, filter.FrontDistance, map);
			}

			this._writer.Write(73, filter.ClipBackPlane ? 1 : 0, map);
			if (filter.ClipBackPlane)
			{
				this._writer.Write(41, filter.BackDistance, map);
			}

			double[] array = new double[24]
			{
				filter.InverseInsertTransform.M00, filter.InverseInsertTransform.M01, filter.InverseInsertTransform.M02, filter.InverseInsertTransform.M03,
				filter.InverseInsertTransform.M10, filter.InverseInsertTransform.M11, filter.InverseInsertTransform.M12, filter.InverseInsertTransform.M13,
				filter.InverseInsertTransform.M20, filter.InverseInsertTransform.M21, filter.InverseInsertTransform.M22, filter.InverseInsertTransform.M23,
				filter.InsertTransform.M00, filter.InsertTransform.M01, filter.InsertTransform.M02, filter.InsertTransform.M03,
				filter.InsertTransform.M10, filter.InsertTransform.M11, filter.InsertTransform.M12, filter.InsertTransform.M13,
				filter.InsertTransform.M20, filter.InsertTransform.M21, filter.InsertTransform.M22, filter.InsertTransform.M23
			};

			for (int i = 0; i < array.Length; i++)
			{
				this._writer.Write(40, array[i]);
			}
		}

		private void writeSortentsTable(SortEntitiesTable e)
		{
			this._writer.Write(DxfCode.Subclass, DxfSubclassMarker.SortentsTable);

			this._writer.WriteHandle(330, e.BlockOwner);

			foreach (SortEntitiesTable.Sorter item in e)
			{
				this._writer.WriteHandle(331, item.Entity);
				this._writer.Write(5, item.SortHandle);
			}
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
