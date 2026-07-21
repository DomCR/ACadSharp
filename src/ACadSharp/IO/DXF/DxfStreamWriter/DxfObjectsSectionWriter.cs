using ACadSharp.Entities;
using ACadSharp.Objects;
using ACadSharp.Objects.AEC;
using ACadSharp.Objects.Evaluations;
using ACadSharp.Tables;
using CSMath;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ACadSharp.IO.DXF;

internal class DxfObjectsSectionWriter : DxfSectionWriterBase
{
	public override string SectionName { get { return DxfFileToken.ObjectsSection; } }

	private readonly HashSet<ulong> _writtenHandles = new();

	public DxfObjectsSectionWriter(IDxfStreamWriter writer, CadDocument document, CadObjectHolder holder, DxfWriterConfiguration configuration)
		: base(writer, document, holder, configuration)
	{
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
				continue;
			}

			//Not compatible dictionaries
			if (item.Name == CadDictionary.AcadMaterial)
			{
				continue;
			}

			//Skip the entries that will not be written to avoid dangling references
			if (!this.isObjectSupported(item))
			{
				continue;
			}

			this._writer.Write(3, item.Name);
			if (dict.HardOwnerFlag)
			{
				this._writer.Write(360, item.Handle);
			}
			else
			{
				this._writer.Write(350, item.Handle);
			}

			this.Holder.Objects.Enqueue(item);
		}

		if (dict is CadDictionaryWithDefault withDefault)
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
		this._writer.Write(71, (short)style.Elements.Count(), map);
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

	protected void writeObject<T>(T co)
		where T : CadObject
	{
		if (!this.isObjectSupported(co))
		{
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
			case AcdbPlaceHolder acdbPlaceHolder:
				this.writeAcdbPlaceHolder(acdbPlaceHolder);
				return;
			case BookColor bookColor:
				this.writeBookColor(bookColor);
				return;
			case CadDictionary cadDictionary:
				this.writeDictionary(cadDictionary);
				return;
			case BlockRepresentationData representationData:
				this.writeBlockRepresentationData(representationData);
				break;
			case BlockMoveAction moveAction:
				this.writeBlockMoveAction(moveAction);
				break;
			case BlockRotationAction rotationAction:
				this.writeBlockRotationAction(rotationAction);
				break;
			case BlockStretchAction stretchAction:
				this.writeBlockStretchAction(stretchAction);
				break;
			case BlockScaleAction scaleAction:
				this.writeBlockScaleAction(scaleAction);
				break;
			case BlockLookupAction lookupAction:
				this.writeBlockLookupAction(lookupAction);
				break;
			case BlockRotationParameter blockRotationParameter:
				this.writeBlockRotationParameter(blockRotationParameter);
				break;
			case BlockVisibilityParameter blockVisibilityParameter:
				this.writeBlockVisibilityParameter(blockVisibilityParameter);
				break;
			case BlockLookupParameter blockLookupParameter:
				this.writeBlockLookupParameter(blockLookupParameter);
				break;
			case BlockPointParameter blockPointParameter:
				this.writeBlockPointParameter(blockPointParameter);
				break;
			case DictionaryVariable dictvar:
				this.writeDictionaryVariable(dictvar);
				break;
			case DimensionAssociation dimAssociation:
				this.writeDimensionAssociation(dimAssociation);
				break;
			case DynamicBlockPurgePreventer dynamicBlockPurgePreventer:
				this.writeDynamicBlockPurge(dynamicBlockPurgePreventer);
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
			case ImageDefinitionReactor reactor:
				this.writeImageDefinitionReactor(reactor);
				return;
			case Layout layout:
				this.writeLayout(layout);
				break;
			case EvaluationGraph evaluationGraph:
				this.writeEvaluationGraph(evaluationGraph);
				break;
			case Field field:
				this.writeField(field);
				break;
			case FieldList fieldList:
				this.writeFieldList(fieldList);
				break;
			case MLineStyle mlStyle:
				this.writeMLineStyle(mlStyle);
				break;
			case MultiLeaderStyle multiLeaderlStyle:
				this.writeMultiLeaderStyle(multiLeaderlStyle);
				break;
			case BlockGripLocationComponent blockGripExpression:
				this.writeBlockGripLocationComponent(blockGripExpression);
				break;
			case BlockXYGrip blockXYGrip:
				this.writeBlockGripBase(blockXYGrip, DxfSubclassMarker.BlockXYGrip);
				break;
			case BlockRotationGrip blockRotationGrip:
				this.writeBlockRotationGrip(blockRotationGrip);
				break;
			case BlockLinearGrip blockLinearGrip:
				this.writeBlockLinearGrip(blockLinearGrip);
				break;
			case BlockLookupGrip blockLookupGrip:
				this.writeBlockGripBase(blockLookupGrip, DxfSubclassMarker.BlockLookupGrip);
				break;
			case BlockVisibilityGrip blockVisibilityGrip:
				this.writeBlockVisibilityGrip(blockVisibilityGrip);
				break;
			case BlockLinearParameter blockLinearParameter:
				this.writeBlockLinearParameter(blockLinearParameter);
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
			case TableStyle tableStyle:
				this.writeTableStyle(tableStyle);
				break;
			case XRecord record:
				this.writeXRecord(record);
				break;
			default:
				throw new NotImplementedException($"Object not implemented : {co.GetType().FullName}");
		}

		this.writeExtendedData(co.ExtendedData);
	}

	private void writeBlockLookupAction(BlockLookupAction lookupAction)
	{
		DxfClassMap map = DxfClassMap.Create<BlockLookupAction>();

		this.writeBlockAction(lookupAction);

		this._writer.Write(100, DxfSubclassMarker.BlockLookupAction);

		int nrows = lookupAction.Columns.FirstOrDefault()?.Rows.Count ?? 0;
		int ncols = lookupAction.Columns.Count;

		this._writer.Write(92, nrows);
		this._writer.Write(93, ncols);

		this._writer.Write(301, string.Empty);
		foreach (var col in lookupAction.Columns)
		{
			foreach (var row in col.Rows)
			{
				this._writer.Write(302, row);
			}
		}

		foreach (var col in lookupAction.Columns)
		{
			this._writer.Write(303, string.Empty);
			this._writer.Write(94, col.NodeId);
			this._writer.Write(95, col.ValueType);
			this._writer.Write(96, col.Type);
			this._writer.Write(282, (byte)(col.IsLookupProperty ? 1 : 0));
			this._writer.Write(305, col.UnmatchedName);
			this._writer.Write(281, (byte)(col.IsReadOnly ? 0 : 1));
			this._writer.Write(304, col.ConnectionName);
		}

		this._writer.Write(280, (byte)(lookupAction.UnknownFlag ? 1 : 0));
	}

	protected void writePdfUnderlayDefinition(PdfUnderlayDefinition definition)
	{
		DxfClassMap map = DxfClassMap.Create<PlotSettings>();

		this._writer.Write(100, DxfSubclassMarker.UnderlayDefinition);

		this._writer.Write(1, definition.File, map);
		this._writer.Write(2, definition.Page, map);
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

	protected override void writeSection()
	{
		while (this.Holder.Objects.Any())
		{
			CadObject item = this.Holder.Objects.Dequeue();

			//An object can be enqueued multiple times, by the dictionary
			//that owns it and by the objects that reference it (e.g. fields
			//in a field list), write it only once
			if (!this._writtenHandles.Add(item.Handle))
			{
				continue;
			}

			this.writeObject(item);
		}
	}

	protected void writeXRecord(XRecord record)
	{
		this._writer.Write(DxfCode.Subclass, DxfSubclassMarker.XRecord);

		this._writer.Write(280, record.CloningFlags);

		foreach (var e in record.Entries)
		{
			switch (e.GroupCode)
			{
				case GroupCodeValueType.None:
					break;
				case GroupCodeValueType.Point3D:
					if (e.Value is IVector v)
					{
						this._writer.Write(e.Code, v);
					}
					else
					{
						this._writer.Write(e.Code, e.Value);
					}
					break;
				case GroupCodeValueType.Handle:
				case GroupCodeValueType.ObjectId:
				case GroupCodeValueType.ExtendedDataHandle:
					var obj = e.Value as IHandledCadObject;
					this._writer.Write(e.Code, obj.Handle);
					break;
				default:
					this._writer.Write(e.Code, e.Value);
					break;
			}
		}
	}

	private bool isObjectSupported(CadObject co)
	{
		switch (co)
		{
			case UnknownNonGraphicalObject:
				return false;
			case AecWallStyle:
			case AecCleanupGroup:
			case AecBinRecord:
			case DimensionAssociation:
			case Material:
			case MultiLeaderObjectContextData:
			case VisualStyle:
			case ProxyObject:
			case MTextAttributeObjectContextData:
			case BlockReferenceObjectContextData:
				this.notify($"Object not implemented : {co.GetType().FullName}", NotificationType.NotImplemented);
				return false;
			case EvaluationGraph when this.Configuration.WriteDynamicBlockData:
			case BlockRepresentationData when this.Configuration.WriteDynamicBlockData:
			case DynamicBlockPurgePreventer when this.Configuration.WriteDynamicBlockData:
			default:
				return true;
		}
	}

	private void writeAcdbPlaceHolder(AcdbPlaceHolder acdbPlaceHolder)
	{
	}

	private void writeBlock1PtParameter(Block1PtParameter parameter)
	{
		DxfClassMap map = DxfClassMap.Create<Block1PtParameter>();

		this.writeBlockParameter(parameter);

		this._writer.Write(100, DxfSubclassMarker.Block1PtParameter);

		this._writer.Write(1010, parameter.Location, map);
		this._writer.Write(93, parameter.GripId, map);

		this.writeEvalParameterProperty(parameter.DisplacementX, 170, 91, 301);
		this.writeEvalParameterProperty(parameter.DisplacementY, 171, 92, 302);
	}

	private void writeBlock2PtParameter(Block2PtParameter parameter)
	{
		DxfClassMap map = DxfClassMap.Create<Block2PtParameter>();

		this.writeBlockParameter(parameter);

		this._writer.Write(100, DxfSubclassMarker.Block2PtParameter);

		this._writer.Write(1010, parameter.FirstPoint, map);
		this._writer.Write(1011, parameter.SecondPoint, map);

		this._writer.Write(170, (short)4, map);
		foreach (var gripId in parameter.GripIds)
		{
			this._writer.Write(91, gripId, map);
		}

		this.writeEvalParameterProperty(parameter.FirstPointDisplacementX, 0);
		this.writeEvalParameterProperty(parameter.FirstPointDisplacementY, 1);
		this.writeEvalParameterProperty(parameter.SecondPointDisplacementX, 2);
		this.writeEvalParameterProperty(parameter.SecondPointDisplacementY, 3);

		this._writer.Write(177, (short)parameter.BaseLocation, map);
	}

	private void writeBlockAction(BlockAction action)
	{
		DxfClassMap map = DxfClassMap.Create<BlockAction>();

		this.writeBlockElement(action);

		this._writer.Write(100, DxfSubclassMarker.BlockAction);

		this._writer.Write(70, (short)action.ParametersIds.Count);
		foreach (int parameterId in action.ParametersIds)
		{
			this._writer.Write(91, parameterId);
		}

		this._writer.Write(71, (short)action.Entities.Count);
		foreach (Entity e in action.Entities)
		{
			this._writer.WriteHandle(330, e);
		}

		this._writer.Write(1010, action.LabelPosition, map);
	}

	private void writeBlockActionBasePt(BlockActionBasePt action)
	{
		DxfClassMap map = DxfClassMap.Create<BlockActionBasePt>();

		this.writeBlockAction(action);

		this._writer.Write(100, DxfSubclassMarker.BlockActionBasePt);

		this._writer.Write(92, action.UpdateBaseX.Id);
		this._writer.Write(93, action.UpdateBaseY.Id);
		this._writer.Write(301, action.UpdateBaseX.Name);
		this._writer.Write(302, action.UpdateBaseY.Name);

		this._writer.Write(1011, action.BasePoint, map);
		this._writer.Write(280, action.Value280 ? (short)1 : (short)0, map);
		this._writer.Write(1012, action.Value1012, map);
	}

	private void writeBlockElement(BlockElement element)
	{
		DxfClassMap map = DxfClassMap.Create<BlockElement>();

		this.writeEvaluationExpression(element);

		this._writer.Write(100, DxfSubclassMarker.BlockElement);

		this._writer.Write(300, element.ElementName, map);

		//Version?? (always the same, matches with AcDbEvalExpr)
		this._writer.Write(98, 33, map);
		this._writer.Write(99, 329, map);

		this._writer.Write(1071, element.Value1071, map);
	}

	private void writeBlockGrip(BlockGrip grip)
	{
		DxfClassMap map = DxfClassMap.Create<BlockGrip>();

		this.writeBlockElement(grip);

		this._writer.Write(100, DxfSubclassMarker.BlockGrip);

		this._writer.Write(91, grip.ExpressionId1, map);
		this._writer.Write(92, grip.ExpressionId2, map);
		this._writer.Write(1010, grip.Location, map);
		this._writer.Write(280, (short)(grip.Cycling ? 1 : 0), map);
		this._writer.Write(93, grip.Value93, map);
	}

	private void writeBlockGripBase(BlockGrip grip, string subclass)
	{
		this.writeBlockGrip(grip);

		this._writer.Write(100, subclass);
	}

	private void writeBlockGripLocationComponent(BlockGripLocationComponent blockGripExpression)
	{
		DxfClassMap map = DxfClassMap.Create<BlockGripLocationComponent>();

		this.writeEvaluationExpression(blockGripExpression);

		this._writer.Write(100, DxfSubclassMarker.BlockGripExpression);

		this._writer.Write(91, blockGripExpression.Connection.Id, map);
		this._writer.Write(300, blockGripExpression.Connection.Name, map);
	}

	private void writeBlockLinearGrip(BlockLinearGrip grip)
	{
		DxfClassMap map = DxfClassMap.Create<BlockLinearGrip>();

		this.writeBlockGrip(grip);

		this._writer.Write(100, DxfSubclassMarker.BlockLinearGrip);

		this._writer.Write(140, grip.XDistance, map);
		this._writer.Write(141, grip.YDistance, map);
		this._writer.Write(142, grip.ZDistance, map);
	}

	private void writeBlockLinearParameter(BlockLinearParameter parameter)
	{
		DxfClassMap map = DxfClassMap.Create<BlockLinearParameter>();

		this.writeBlock2PtParameter(parameter);

		this._writer.Write(100, DxfSubclassMarker.BlockLinearParameter);

		this._writer.Write(305, parameter.Label, map);
		this._writer.Write(306, parameter.Description, map);

		this._writer.Write(140, parameter.LabelOffset, map);

		this.writeParameterValueSet(parameter.ValueSet, 307, 96, 141, 175);
	}

	private void writeBlockLookupParameter(BlockLookupParameter parameter)
	{
		DxfClassMap map = DxfClassMap.Create<BlockLookupParameter>();

		this.writeBlock1PtParameter(parameter);

		this._writer.Write(100, DxfSubclassMarker.BlockLookupParameter);

		this._writer.Write(303, parameter.Label, map);
		this._writer.Write(304, parameter.Description, map);
		this._writer.Write(94, parameter.ActionId, map);
	}

	private void writeBlockMoveAction(BlockMoveAction moveAction)
	{
		DxfClassMap map = DxfClassMap.Create<BlockMoveAction>();

		this.writeBlockAction(moveAction);

		this._writer.Write(100, DxfSubclassMarker.BlockMoveAction);

		this.writeEvalConnection(moveAction.XDelta, 92, 301);
		this.writeEvalConnection(moveAction.YDelta, 93, 302);

		this._writer.Write(140, moveAction.DistanceMultiplier, map);
		this._writer.Write(141, moveAction.AngleOffset, map);
		this._writer.Write(280, (byte)moveAction.UnknownFlag, map);
	}

	private void writeBlockParameter(BlockParameter parameter)
	{
		DxfClassMap map = DxfClassMap.Create<BlockParameter>();

		this.writeBlockElement(parameter);

		this._writer.Write(100, DxfSubclassMarker.BlockParameter);

		this._writer.Write(280, (byte)(parameter.ShowProperties ? 1 : 0), map);
		this._writer.Write(281, (byte)(parameter.ChainActions ? 1 : 0), map);
	}

	private void writeBlockPointParameter(BlockPointParameter parameter)
	{
		DxfClassMap map = DxfClassMap.Create<BlockPointParameter>();

		this.writeBlock1PtParameter(parameter);

		this._writer.Write(100, DxfSubclassMarker.BlockPointParameter);

		this._writer.Write(303, parameter.Label, map);
		this._writer.Write(304, parameter.Description, map);
		this._writer.Write(1011, parameter.LabelPosition, map);
	}

	private void writeBlockRepresentationData(BlockRepresentationData representationData)
	{
		DxfClassMap map = DxfClassMap.Create<BlockRepresentationData>();

		this._writer.Write(100, DxfSubclassMarker.BlockRepresentationData);

		this._writer.Write(70, representationData.Version, map);

		this._writer.WriteHandle(340, representationData.Block, map);
	}

	private void writeBlockRotationAction(BlockRotationAction rotationAction)
	{
		DxfClassMap map = DxfClassMap.Create<BlockRotationAction>();

		this.writeBlockActionBasePt(rotationAction);

		this._writer.Write(100, DxfSubclassMarker.BlockRotationAction);

		this.writeEvalConnection(rotationAction.Connection, 94, 303);
	}

	private void writeBlockRotationGrip(BlockRotationGrip blockRotationGrip)
	{
		DxfClassMap map = DxfClassMap.Create<BlockRotationGrip>();

		this.writeBlockGrip(blockRotationGrip);

		this._writer.Write(100, DxfSubclassMarker.BlockRotationGrip);
	}

	private void writeBlockRotationParameter(BlockRotationParameter parameter)
	{
		DxfClassMap map = DxfClassMap.Create<BlockRotationParameter>();

		this.writeBlock2PtParameter(parameter);

		this._writer.Write(100, DxfSubclassMarker.BlockRotationParameter);

		this._writer.Write(305, parameter.Label, map);
		this._writer.Write(306, parameter.Description, map);

		this._writer.Write(1011, parameter.Point, map);

		this._writer.Write(140, parameter.LabelOffset, map);

		this.writeParameterValueSet(parameter.ValueSet, 307, 96, 141, 175);
	}

	private void writeBlockScaleAction(BlockScaleAction scaleAction)
	{
		DxfClassMap map = DxfClassMap.Create<BlockScaleAction>();

		this.writeBlockActionBasePt(scaleAction);

		this._writer.Write(100, DxfSubclassMarker.BlockScaleAction);

		this._writer.Write(94, scaleAction.ScaleConnection.Id);
		this._writer.Write(95, scaleAction.XScaleConnection.Id);
		this._writer.Write(96, scaleAction.YScaleConnection.Id);

		this._writer.Write(303, scaleAction.ScaleConnection.Name);
		this._writer.Write(304, scaleAction.XScaleConnection.Name);
		this._writer.Write(305, scaleAction.YScaleConnection.Name);
	}

	private void writeBlockStretchAction(BlockStretchAction stretchAction)
	{
		DxfClassMap map = DxfClassMap.Create<BlockStretchAction>();

		this.writeBlockAction(stretchAction);

		this._writer.Write(100, DxfSubclassMarker.BlockStretchAction);

		this.writeEvalConnection(stretchAction.EndXDelta, 92, 301);
		this.writeEvalConnection(stretchAction.EndYDelta, 93, 302);

		this._writer.Write(72, stretchAction.Boundary.Count, map);
		foreach (var pt in stretchAction.Boundary)
		{
			this._writer.Write(1011, pt);
		}

		this._writer.Write(73, stretchAction.StretchBindings.Count, map);
		foreach (var binding in stretchAction.StretchBindings)
		{
			this._writer.WriteHandle(331, binding.Entity);

			this._writer.Write(74, binding.PointIndexes.Count);
			foreach (var index in binding.PointIndexes)
			{
				this._writer.Write(94, index);
			}
		}

		this._writer.Write(75, stretchAction.StretchNodes.Count, map);
		foreach (var node in stretchAction.StretchNodes)
		{
			this._writer.Write(95, node.NodeId);
			this._writer.Write(76, node.PointIndexes.Count);
			foreach (var index in node.PointIndexes)
			{
				this._writer.Write(94, index);
			}
		}

		this._writer.Write(140, stretchAction.DistanceMultiplier, map);
		this._writer.Write(141, stretchAction.AngleOffset, map);
		this._writer.Write(280, (byte)stretchAction.UnknownFlag, map);
	}

	private void writeBlockVisibilityGrip(BlockVisibilityGrip grip)
	{
		DxfClassMap map = DxfClassMap.Create<BlockVisibilityGrip>();

		this.writeBlockGrip(grip);

		this._writer.Write(100, DxfSubclassMarker.BlockVisibilityGrip);
	}

	private void writeBlockVisibilityParameter(BlockVisibilityParameter parameter)
	{
		DxfClassMap map = DxfClassMap.Create<BlockVisibilityParameter>();

		this.writeBlock1PtParameter(parameter);

		this._writer.Write(100, DxfSubclassMarker.BlockVisibilityParameter);

		this._writer.Write(281, (byte)(parameter.Value281 ? 1 : 0));
		this._writer.Write(301, parameter.Label);
		this._writer.Write(302, parameter.Description);
		this._writer.Write(91, parameter.Value91 ? 1 : 0);

		this._writer.Write(93, parameter.Entities.Count);
		foreach (var e in parameter.Entities)
		{
			this._writer.WriteHandle(331, e);
		}

		this._writer.Write(92, parameter.States.Count);
		foreach (var state in parameter.States.Values)
		{
			this._writer.Write(303, state.Name);

			this._writer.Write(94, state.Entities.Count);
			foreach (var e in state.Entities)
			{
				this._writer.WriteHandle(332, e);
			}

			this._writer.Write(95, state.Expressions.Count);
			foreach (var expression in state.Expressions)
			{
				this._writer.WriteHandle(333, expression);
			}
		}
	}

	private void writeCellStyle(TableStyle.CellStyle cellStyle)
	{
		if (cellStyle.TextStyle != null)
		{
			this._writer.WriteName(7, cellStyle.TextStyle);
		}
		else
		{
			this._writer.Write(7, TextStyle.DefaultName);
		}

		this._writer.Write(140, cellStyle.TextHeight);
		this._writer.Write(170, cellStyle.CellAlignment);

		this._writer.Write(62, cellStyle.ContentColor.GetApproxIndex());
		this._writer.Write(63, cellStyle.BackgroundColor.GetApproxIndex());
		this._writer.Write(283, cellStyle.IsFillColorOn ? 1 : 0);

		this._writer.Write(90, (int)cellStyle.Type);
		this._writer.Write(91, (int)cellStyle.ValueDataType);

		this.writeCellStyleBorder(cellStyle.TopBorder, 0);
		this.writeCellStyleBorder(cellStyle.HorizontalInsideBorder, 1);
		this.writeCellStyleBorder(cellStyle.BottomBorder, 2);
		this.writeCellStyleBorder(cellStyle.LeftBorder, 3);
		this.writeCellStyleBorder(cellStyle.VerticalInsideBorder, 4);
		this.writeCellStyleBorder(cellStyle.RightBorder, 5);
	}

	private void writeCellStyleBorder(TableStyle.CellBorder border, int i)
	{
		this._writer.Write(274 + i, (short)border.LineWeight);
		this._writer.Write(284 + i, border.IsInvisible ? 0 : 1);
		this._writer.Write(64 + i, border.Color.GetApproxIndex());
	}

	private void writeDimensionAssociation(DimensionAssociation dimAssociation)
	{
		DxfClassMap map = DxfClassMap.Create<DimensionAssociation>();

		this._writer.Write(100, DxfSubclassMarker.DimensionAssociation);

		this._writer.WriteHandle(330, dimAssociation.Dimension, map);
		this._writer.Write(90, (int)dimAssociation.AssociativityFlags, map);
		this._writer.Write(70, dimAssociation.IsTransSpace, map);
		this._writer.Write(71, (short)dimAssociation.RotatedDimensionType, map);

		if (dimAssociation.AssociativityFlags.HasFlag(AssociativityFlags.FirstPointReference))
		{
			this.writeOsnapPointRef(dimAssociation.FirstPointRef);
		}

		if (dimAssociation.AssociativityFlags.HasFlag(AssociativityFlags.SecondPointReference))
		{
			this.writeOsnapPointRef(dimAssociation.SecondPointRef);
		}

		if (dimAssociation.AssociativityFlags.HasFlag(AssociativityFlags.ThirdPointReference))
		{
			this.writeOsnapPointRef(dimAssociation.ThirdPointRef);
		}

		if (dimAssociation.AssociativityFlags.HasFlag(AssociativityFlags.FourthPointReference))
		{
			this.writeOsnapPointRef(dimAssociation.FourthPointRef);
		}
	}

	private void writeDxfValuePair(DxfValuePair pair)
	{
		this._writer.Write(70, pair.Code);
		this._writer.Write(pair.Code, pair.Value);
	}

	private void writeDynamicBlockPurge(DynamicBlockPurgePreventer dynamicBlockPurgePreventer)
	{
		DxfClassMap map = DxfClassMap.Create<DynamicBlockPurgePreventer>();

		this._writer.Write(100, DxfSubclassMarker.AcDbDynamicBlockPurgePreventer);

		this._writer.Write(70, dynamicBlockPurgePreventer.Version);
	}

	private void writeEvalConnection(EvalConnection connection, int idCode, int nameCode)
	{
		this._writer.Write(idCode, connection.Id);
		this._writer.Write(nameCode, connection.Name);
	}

	private void writeEvalParameterProperty(EvalParameterProperty property, int code)
	{
		this._writer.Write(171 + code, (short)property.Connections.Count);

		foreach (var conn in property.Connections)
		{
			this._writer.Write(92 + code, conn.Id);
			this._writer.Write(301 + code, conn.Name);
		}
	}

	private void writeEvalParameterProperty(EvalParameterProperty property, int countCode, int idCode, int strCode)
	{
		this._writer.Write(countCode, (short)property.Connections.Count);
		foreach (var conn in property.Connections)
		{
			this._writer.Write(idCode, conn.Id);
			this._writer.Write(strCode, conn.Name);
		}
	}

	private void writeEvaluationExpression(EvaluationExpression exp)
	{
		DxfClassMap map = DxfClassMap.Create<EvaluationExpression>();

		this._writer.Write(100, DxfSubclassMarker.EvalGraphExpr);

		this._writer.Write(90, exp.Id, map);
		this._writer.Write(98, exp.Value98, map);
		this._writer.Write(99, exp.Value99, map);

		if (exp.EvaluatedValue != null)
		{
			this._writer.Write(1, string.Empty);
			this.writeDxfValuePair(exp.EvaluatedValue);
		}
	}

	private void writeEvaluationGraph(EvaluationGraph evaluationGraph)
	{
		DxfClassMap map = DxfClassMap.Create<EvaluationGraph>();

		this._writer.Write(100, DxfSubclassMarker.EvalGraph);

		this._writer.Write(96, evaluationGraph.Value96, map);
		this._writer.Write(97, evaluationGraph.Value97, map);

		var nodes = evaluationGraph.Nodes.ToArray();
		for (int i = 0; i < nodes.Length; i++)
		{
			var n = nodes[i];

			this._writer.Write(91, i);
			this._writer.Write(93, n.Flags);
			this._writer.Write(95, n.Id);
			this._writer.WriteHandle(360, n.Expression);
			this._writer.Write(92, n.Data1);
			this._writer.Write(92, n.Data2);
			this._writer.Write(92, n.Data3);
			this._writer.Write(92, n.Data4);

			if (n.Expression != null)
			{
				this.Holder.Objects.Enqueue(n.Expression);
			}
		}

		for (int i = 0; i < evaluationGraph.Edges.Count; i++)
		{
			var e = evaluationGraph.Edges[i];

			this._writer.Write(92, i);
			this._writer.Write(93, e.Flags);
			this._writer.Write(94, e.TrackedCount);
			this._writer.Write(91, e.FromNodeIndex);
			this._writer.Write(91, e.ToNodeIndex);
			this._writer.Write(92, e.Data1);
			this._writer.Write(92, e.Data2);
			this._writer.Write(92, e.Data3);
			this._writer.Write(92, e.Data4);
			this._writer.Write(92, e.Data5);
		}
	}

	private void writeField(Field field)
	{
		this._writer.Write(DxfCode.Subclass, DxfSubclassMarker.Field);

		this._writer.Write(1, field.EvaluatorId);

		writeLongTextValue(2, 3, field.FieldCode);

		this._writer.Write(90, field.Children.Count);
		foreach (var item in field.Children)
		{
			this._writer.WriteHandle(360, item);
		}

		this._writer.Write(97, field.CadObjects.Count);
		foreach (var item in field.CadObjects)
		{
			this._writer.WriteHandle(331, item);
		}

		this._writer.Write(91, (int)field.EvaluationOptionFlags);
		this._writer.Write(92, (int)field.FilingOptionFlags);
		this._writer.Write(94, (int)field.FieldStateFlags);
		this._writer.Write(95, (int)field.EvaluationStatusFlags);
		this._writer.Write(96, field.EvaluationErrorCode);
		this._writer.Write(300, field.EvaluationErrorMessage);

		this._writer.Write(93, field.Values.Count);
		foreach (var item in field.Values)
		{
			this._writer.Write(6, item.Key);
			this.writeCadValue(item.Value);
		}

		this._writer.Write(7, "ACFD_FIELD_VALUE");
		this.writeCadValue(field.Value);

		this.writeLongTextValue(301, 9, field.FormatString);
		this._writer.Write(98, field.FormatString.Length);
	}

	private void writeFieldList(FieldList fieldList)
	{
		this._writer.Write(DxfCode.Subclass, DxfSubclassMarker.IdSet);

		this._writer.Write(90, fieldList.Fields.Count);
		foreach (Field field in fieldList.Fields)
		{
			this._writer.WriteHandle(330, field);
			this.Holder.Objects.Enqueue(field);
		}

		this._writer.Write(DxfCode.Subclass, DxfSubclassMarker.FieldList);
	}

	private void writeImageDefinitionReactor(ImageDefinitionReactor reactor)
	{
		this._writer.Write(DxfCode.Subclass, DxfSubclassMarker.RasterImageDefReactor);

		this._writer.Write(90, reactor.ClassVersion);
		this._writer.WriteHandle(330, reactor.Image);
	}

	private void writeOsnapPointRef(DimensionAssociation.OsnapPointRef osnapPoint)
	{
		if (osnapPoint == null)
		{
			return;
		}

		this._writer.Write(1, DimensionAssociation.OsnapPointRefClassName);

		this._writer.Write(72, (short)osnapPoint.ObjectOsnapType);
		this._writer.WriteHandle(331, osnapPoint.Geometry);
		this._writer.Write(73, (short)osnapPoint.SubentType);
		this._writer.Write(91, osnapPoint.GsMarker);
		this._writer.Write(40, osnapPoint.GeometryParameter);

		this._writer.Write(10, osnapPoint.OsnapPoint);

		this._writer.Write(75, (short)(osnapPoint.HasLastPointRef ? 1 : 0));
	}

	private void writeParameterValueSet(ParameterValueSet valueSet, int startCode, int typeCode, int valueCode, int countCode)
	{
		this._writer.Write(startCode, string.Empty);
		this._writer.Write(typeCode, (int)valueSet.Type);
		this._writer.Write(valueCode, valueSet.Minimum);
		this._writer.Write(valueCode + 1, valueSet.Maximum);
		this._writer.Write(valueCode + 2, valueSet.Increment);
		this._writer.Write(countCode, valueSet.AllowedValues.Count);
		foreach (var item in valueSet.AllowedValues)
		{
			this._writer.Write(valueCode + 3, item);
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

	private void writeTableStyle(TableStyle style)
	{
		DxfClassMap map = DxfClassMap.Create<TableStyle>();

		this._writer.Write(100, DxfSubclassMarker.TableStyle);

		this._writer.Write(3, style.Description, map);

		this._writer.Write(70, style.FlowDirection, map);
		this._writer.Write(71, style.Flags, map);

		this._writer.Write(40, style.HorizontalCellMargin, map);
		this._writer.Write(41, style.VerticalCellMargin, map);

		this._writer.Write(280, style.SuppressTitle ? 1 : 0, map);
		this._writer.Write(281, style.SuppressHeaderRow ? 1 : 0, map);

		this.writeCellStyle(style.DataCellStyle);
		this.writeCellStyle(style.TitleCellStyle);
		this.writeCellStyle(style.HeaderCellStyle);
	}
}