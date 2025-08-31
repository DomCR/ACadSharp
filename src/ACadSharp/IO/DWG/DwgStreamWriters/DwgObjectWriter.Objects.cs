using ACadSharp.Objects;
using ACadSharp.Objects.Evaluations;
using CSMath;
using CSUtilities.Converters;
using CSUtilities.IO;
using CSUtilities.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace ACadSharp.IO.DWG
{
	internal partial class DwgObjectWriter : DwgSectionIO
	{
		private void writeObjects()
		{
			while (this._objects.Any())
			{
				NonGraphicalObject obj = this._objects.Dequeue();

				this.writeObject(obj);
			}
		}

		private void writeObject(NonGraphicalObject obj)
		{
			if (this.skipEntry(obj, out bool notify))
			{
				if (notify)
				{
					this.notify($"Object type not implemented {obj.GetType().FullName}", NotificationType.NotImplemented);
				}
				return;
			}

			this.writeCommonNonEntityData(obj);

			switch (obj)
			{
				case AcdbPlaceHolder acdbPlaceHolder:
					this.writeAcdbPlaceHolder(acdbPlaceHolder);
					break;
				case BookColor bookColor:
					this.writeBookColor(bookColor);
					break;
				case CadDictionaryWithDefault dictionarydef:
					this.writeCadDictionaryWithDefault(dictionarydef);
					break;
				case CadDictionary dictionary:
					this.writeDictionary(dictionary);
					break;
				case DictionaryVariable dictionaryVariable:
					this.writeDictionaryVariable(dictionaryVariable);
					break;
				case GeoData geodata:
					this.writeGeoData(geodata);
					break;
				case Group group:
					this.writeGroup(group);
					break;
				case ImageDefinitionReactor definitionReactor:
					this.writeImageDefinitionReactor(definitionReactor);
					break;
				case ImageDefinition definition:
					this.writeImageDefinition(definition);
					break;
				case Layout layout:
					this.writeLayout(layout);
					break;
				case MLineStyle style:
					this.writeMLineStyle(style);
					break;
				case MultiLeaderStyle multiLeaderStyle:
					this.writeMultiLeaderStyle(multiLeaderStyle);
					break;
				case MultiLeaderObjectContextData multiLeaderObjectContextData:
					this.writeObjectContextData(multiLeaderObjectContextData);
					this.writeAnnotScaleObjectContextData(multiLeaderObjectContextData);
					this.writeMultiLeaderAnnotContext(multiLeaderObjectContextData);
					break;
				case PdfUnderlayDefinition pdfDefinition:
					this.writePdfDefinition(pdfDefinition);
					break;
				case PlotSettings plotsettings:
					this.writePlotSettings(plotsettings);
					break;
				case RasterVariables rasterVariables:
					this.writeRasterVariables(rasterVariables);
					break;
				case Scale scale:
					this.writeScale(scale);
					break;
				case SortEntitiesTable sorttables:
					this.writeSortEntitiesTable(sorttables);
					break;
				case SpatialFilter spatialFilter:
					this.writeSpatialFilter(spatialFilter);
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

		private void writeBookColor(BookColor color)
		{
			this._writer.WriteBitShort(0);

			if (this.R2004Plus)
			{
				byte[] arr = new byte[]
				{
					color.Color.B,
					color.Color.G,
					color.Color.R,
					0b11000010
				};

				uint rgb = LittleEndianConverter.Instance.ToUInt32(arr);

				this._writer.WriteBitLong((int)rgb);

				byte flags = 0;
				if (!string.IsNullOrEmpty(color.Name))
				{
					flags = (byte)(flags | 1u);
				}

				if (!string.IsNullOrEmpty(color.BookName))
				{
					flags = (byte)(flags | 2u);
				}

				this._writer.WriteByte(flags);
				if (!string.IsNullOrEmpty(color.ColorName))
				{
					this._writer.WriteVariableText(color.ColorName);
				}

				if (!string.IsNullOrEmpty(color.BookName))
				{
					this._writer.WriteVariableText(color.BookName);
				}
			}
		}

		private void writeCadDictionaryWithDefault(CadDictionaryWithDefault dictionary)
		{
			this.writeDictionary(dictionary);

			//H 7 Default entry (hard pointer)
			this._writer.HandleReference(DwgReferenceType.HardPointer, dictionary.DefaultEntry);
		}

		private void writeDictionary(CadDictionary dictionary)
		{
			//Common:
			//Numitems L number of dictionary items
			List<NonGraphicalObject> entries = new List<NonGraphicalObject>();
			foreach (var item in dictionary)
			{
				if (this.skipEntry(item))
				{
					continue;
				}

				entries.Add(item);
			}

			this._writer.WriteBitLong(entries.Count);

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
			foreach (var item in entries)
			{
				if (this.skipEntry(item))
				{
					continue;
				}

				this._writer.WriteVariableText(item.Name);
				this._writer.HandleReference(DwgReferenceType.SoftOwnership, item.Handle);
			}

			this.addEntriesToWriter(dictionary);
		}

		private bool skipEntry(NonGraphicalObject entry)
		{
			return this.skipEntry(entry, out _);
		}

		private bool skipEntry(NonGraphicalObject entry, out bool notify)
		{
			notify = true;
			switch (entry)
			{
				case XRecord when !this.WriteXRecords:
					notify = false;
					return true;
				case EvaluationGraph:
				case Material:
				case UnknownNonGraphicalObject:
				case VisualStyle:
				case ProxyObject:
					return true;
			}

			return false;
		}

		private void addEntriesToWriter(CadDictionary dictionary)
		{
			foreach (NonGraphicalObject e in dictionary)
			{
				this._objects.Enqueue(e);
			}
		}

		private void writeDictionaryVariable(DictionaryVariable dictionaryVariable)
		{
			//Intval RC an integer value
			this._writer.WriteByte(0);

			//BS a string
			this._writer.WriteVariableText(dictionaryVariable.Value);
		}

		private void writeGeoData(GeoData geodata)
		{
			//BL Object version formats
			this._writer.WriteBitLong((int)geodata.Version);

			//H Soft pointer to host block
			this._writer.HandleReference(DwgReferenceType.SoftPointer, geodata.HostBlock);

			//BS Design coordinate type
			this._writer.WriteBitShort((short)geodata.CoordinatesType);

			switch (geodata.Version)
			{
				case GeoDataVersion.R2009:
					//3BD  Reference point 
					this._writer.Write3BitDouble(geodata.ReferencePoint);

					//BL  Units value horizontal
					this._writer.WriteBitLong((int)geodata.HorizontalUnits);

					//3BD  Design point
					this._writer.Write3BitDouble(geodata.DesignPoint);

					//3BD  Obsolete, ODA writes (0, 0, 0) 
					this._writer.Write3BitDouble(XYZ.Zero);

					//3BD  Up direction
					this._writer.Write3BitDouble(geodata.UpDirection);

					//BD Angle of north direction (radians, angle measured clockwise from the (0, 1) vector). 
					this._writer.WriteBitDouble(System.Math.PI / 2.0 - geodata.NorthDirection.GetAngle());

					//3BD  Obsolete, ODA writes(1, 1, 1)
					this._writer.Write3BitDouble(new XYZ(1, 1, 1));

					//VT  Coordinate system definition. In AutoCAD 2009 this is a “Well known text” (WKT)string containing a projected coordinate system(PROJCS).
					this._writer.WriteVariableText(geodata.CoordinateSystemDefinition);
					//VT  Geo RSS tag.
					this._writer.WriteVariableText(geodata.GeoRssTag);

					//BD Unit scale factor horizontal
					this._writer.WriteBitDouble(geodata.HorizontalUnitScale);
					geodata.VerticalUnitScale = geodata.HorizontalUnitScale;

					//VT  Obsolete, coordinate system datum name 
					this._writer.WriteVariableText(string.Empty);
					//VT  Obsolete: coordinate system WKT 
					this._writer.WriteVariableText(string.Empty);
					break;
				case GeoDataVersion.R2010:
				case GeoDataVersion.R2013:
					//3BD  Design point
					this._writer.Write3BitDouble(geodata.DesignPoint);
					//3BD  Reference point
					this._writer.Write3BitDouble(geodata.ReferencePoint);
					//BD  Unit scale factor horizontal
					this._writer.WriteBitDouble(geodata.HorizontalUnitScale);
					//BL  Units value horizontal
					this._writer.WriteBitLong((int)geodata.HorizontalUnits);
					//BD  Unit scale factor vertical 
					this._writer.WriteBitDouble(geodata.VerticalUnitScale);
					//BL  Units value vertical
					this._writer.WriteBitLong((int)geodata.HorizontalUnits);
					//3RD  Up direction
					this._writer.Write3BitDouble(geodata.UpDirection);
					//3RD  North direction
					this._writer.Write2RawDouble(geodata.NorthDirection);
					//BL Scale estimation method.
					this._writer.WriteBitLong((int)geodata.ScaleEstimationMethod);
					//BD  User specified scale factor
					this._writer.WriteBitDouble(geodata.UserSpecifiedScaleFactor);
					//B  Do sea level correction
					this._writer.WriteBit(geodata.EnableSeaLevelCorrection);
					//BD  Sea level elevation
					this._writer.WriteBitDouble(geodata.SeaLevelElevation);
					//BD  Coordinate projection radius
					this._writer.WriteBitDouble(geodata.CoordinateProjectionRadius);
					//VT  Coordinate system definition . In AutoCAD 2010 this is a map guide XML string.
					this._writer.WriteVariableText(geodata.CoordinateSystemDefinition);
					//VT  Geo RSS tag.
					this._writer.WriteVariableText(geodata.GeoRssTag);
					break;
				default:
					break;
			}

			//VT  Observation from tag
			this._writer.WriteVariableText(geodata.ObservationFromTag);
			//VT  Observation to tag
			this._writer.WriteVariableText(geodata.ObservationToTag);
			//VT  Observation coverage tag
			this._writer.WriteVariableText(geodata.ObservationCoverageTag);

			//BL Number of geo mesh points
			this._writer.WriteBitLong(geodata.Points.Count);
			foreach (var pt in geodata.Points)
			{
				//2RD Source point 
				this._writer.Write2RawDouble(pt.Source);
				//2RD Destination point 
				this._writer.Write2RawDouble(pt.Destination);
			}

			//BL Number of geo mesh faces
			this._writer.WriteBitLong(geodata.Faces.Count);
			foreach (var face in geodata.Faces)
			{
				//BL Face index 1
				this._writer.WriteBitLong(face.Index1);
				//BL Face index 2
				this._writer.WriteBitLong(face.Index2);
				//BL Face index 3
				this._writer.WriteBitLong(face.Index3);
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
			this._writer.WriteBitLong(group.Entities.Count());
			foreach (var e in group.Entities)
			{
				//the entries in the group(hard pointer)
				this._writer.HandleReference(DwgReferenceType.HardPointer, e);
			}
		}

		private void writeImageDefinitionReactor(ImageDefinitionReactor definitionReactor)
		{
			//Common:
			//Classver BL 90 class version
			this._writer.WriteBitLong(definitionReactor.ClassVersion);
		}

		private void writePdfDefinition(PdfUnderlayDefinition definition)
		{
			this._writer.WriteVariableText(definition.File);
			this._writer.WriteVariableText(definition.Page);
		}

		private void writeImageDefinition(ImageDefinition definition)
		{
			//Common:
			//Clsver BL 0 class version
			this._writer.WriteBitLong(definition.ClassVersion);
			//Imgsize 2RD 10 size of image in pixels
			this._writer.Write2RawDouble(definition.Size);
			//Filepath TV 1 path to file
			this._writer.WriteVariableText(definition.FileName);
			//Isloaded B 280 0==no, 1==yes
			this._writer.WriteBit(definition.IsLoaded);
			//Resunits RC 281 0==none, 2==centimeters, 5==inches
			this._writer.WriteByte((byte)definition.Units);
			//Pixelsize 2RD 11 size of one pixel in AutoCAD units
			this._writer.Write2RawDouble(definition.DefaultSize);
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

		private void writeMLineStyle(MLineStyle mlineStyle)
		{
			//Common:
			//Name TV Name of this style
			this._writer.WriteVariableText(mlineStyle.Name);
			//Desc TV Description of this style
			this._writer.WriteVariableText(mlineStyle.Description);

			short flags = 0;
			if (mlineStyle.Flags.HasFlag(MLineStyleFlags.DisplayJoints))
			{
				flags = (short)(flags | 1U);
			}
			if (mlineStyle.Flags.HasFlag(MLineStyleFlags.FillOn))
			{
				flags = (short)(flags | 2U);
			}
			if (mlineStyle.Flags.HasFlag(MLineStyleFlags.StartSquareCap))
			{
				flags = (short)(flags | 16U);
			}
			if (mlineStyle.Flags.HasFlag(MLineStyleFlags.StartRoundCap))
			{
				flags = (short)(flags | 0x20);
			}
			if (mlineStyle.Flags.HasFlag(MLineStyleFlags.StartInnerArcsCap))
			{
				flags = (short)(flags | 0x40);
			}
			if (mlineStyle.Flags.HasFlag(MLineStyleFlags.EndSquareCap))
			{
				flags = (short)(flags | 0x100);
			}
			if (mlineStyle.Flags.HasFlag(MLineStyleFlags.EndRoundCap))
			{
				flags = (short)(flags | 0x200);
			}
			if (mlineStyle.Flags.HasFlag(MLineStyleFlags.EndInnerArcsCap))
			{
				flags = (short)(flags | 0x400);
			}

			//Flags BS A short which reconstitutes the mlinestyle flags as defined in DXF.
			this._writer.WriteBitShort(flags);

			//fillcolor CMC Fill color for this style
			this._writer.WriteCmColor(mlineStyle.FillColor);
			//startang BD Start angle
			this._writer.WriteBitDouble(mlineStyle.StartAngle);
			//endang BD End angle
			this._writer.WriteBitDouble(mlineStyle.EndAngle);

			//linesinstyle RC Number of lines in this style
			this._writer.WriteByte((byte)mlineStyle.Elements.Count);
			foreach (MLineStyle.Element element in mlineStyle.Elements)
			{
				//Offset BD Offset of this segment
				this._writer.WriteBitDouble(element.Offset);
				//Color CMC Color of this segment
				this._writer.WriteCmColor(element.Color);
				//R2018+:
				if (this.R2018Plus)
				{
					//Line type handle H Line type handle (hard pointer)
					this._writer.HandleReference(DwgReferenceType.HardPointer, element.LineType);
				}
				//Before R2018:
				else
				{
					//TODO: Fix the Linetype index for dwgReader and DwgWriter
					//Ltindex BS Linetype index (yes, index)
					this._writer.WriteBitShort(0);
				}
			}
		}

		private void writeMultiLeaderStyle(MultiLeaderStyle mLeaderStyle)
		{
			if (this.R2010Plus)
			{
				//	BS	179	Version expected: 2
				this._writer.WriteBitShort(2);
			}

			//	BS	170	Content type (see paragraph on LEADER for more details).
			this._writer.WriteBitShort((short)mLeaderStyle.ContentType);
			//	BS	171	Draw multi-leader order (0 = draw content first, 1 = draw leader first)
			this._writer.WriteBitShort((short)mLeaderStyle.MultiLeaderDrawOrder);
			//	BS	172	Draw leader order (0 = draw leader head first, 1 = draw leader tail first)
			this._writer.WriteBitShort((short)mLeaderStyle.LeaderDrawOrder);
			//	BL	90	Maximum number of points for leader
			this._writer.WriteBitLong((short)mLeaderStyle.MaxLeaderSegmentsPoints);
			//	BD	40	First segment angle (radians)
			this._writer.WriteBitDouble(mLeaderStyle.FirstSegmentAngleConstraint);
			//	BD	41	Second segment angle (radians)
			this._writer.WriteBitDouble(mLeaderStyle.SecondSegmentAngleConstraint);
			//	BS	173	Leader type (see paragraph on LEADER for more details).
			this._writer.WriteBitShort((short)mLeaderStyle.PathType);
			//	CMC	91	Leader line color
			this._writer.WriteCmColor(mLeaderStyle.LineColor);

			//	H	340	Leader line type handle (hard pointer)
			this._writer.HandleReference(DwgReferenceType.HardPointer, mLeaderStyle.LeaderLineType);

			//	BL	92	Leader line weight
			this._writer.WriteBitLong((short)mLeaderStyle.LeaderLineWeight);
			//	B	290	Is landing enabled?
			this._writer.WriteBit(mLeaderStyle.EnableLanding);
			//	BD	42	Landing gap
			this._writer.WriteBitDouble(mLeaderStyle.LandingGap);
			//	B	291	Auto include landing (is dog-leg enabled?)
			this._writer.WriteBit(mLeaderStyle.EnableDogleg);
			//	BD	43	Landing distance
			this._writer.WriteBitDouble(mLeaderStyle.LandingDistance);
			//	TV	3	Style description
			this._writer.WriteVariableText(mLeaderStyle.Description);

			//	H	341	Arrow head block handle (hard pointer)
			this._writer.HandleReference(DwgReferenceType.HardPointer, mLeaderStyle.Arrowhead);

			//	BD	44	Arrow head size
			this._writer.WriteBitDouble(mLeaderStyle.ArrowheadSize);
			//	TV	300	Text default
			this._writer.WriteVariableText(mLeaderStyle.DefaultTextContents);

			//	H	342	Text style handle (hard pointer)
			this._writer.HandleReference(DwgReferenceType.HardPointer, mLeaderStyle.TextStyle);

			//	BS	174	Left attachment (see paragraph on LEADER for more details).
			this._writer.WriteBitShort((short)mLeaderStyle.TextLeftAttachment);
			//	BS	178	Right attachment (see paragraph on LEADER for more details).
			this._writer.WriteBitShort((short)mLeaderStyle.TextRightAttachment);
			//	BS	175	Text angle type (see paragraph on LEADER for more details).
			this._writer.WriteBitShort((short)mLeaderStyle.TextAngle);
			//	BS	176	Text alignment type
			this._writer.WriteBitShort((short)mLeaderStyle.TextAlignment);
			//	CMC	93	Text color
			this._writer.WriteCmColor(mLeaderStyle.TextColor);
			//	BD	45	Text height
			this._writer.WriteBitDouble(mLeaderStyle.TextHeight);
			//	B	292	Text frame enabled
			this._writer.WriteBit(mLeaderStyle.TextFrame);
			//	B	297	Always align text left
			this._writer.WriteBit(mLeaderStyle.TextAlignAlwaysLeft);
			//	BD	46	Align space
			this._writer.WriteBitDouble(mLeaderStyle.AlignSpace);

			//	H	343	Block handle (hard pointer)
			this._writer.HandleReference(DwgReferenceType.HardPointer, mLeaderStyle.BlockContent);

			//	CMC	94	Block color
			this._writer.WriteCmColor(mLeaderStyle.BlockContentColor);
			//	3BD	47,49,140	Block scale vector
			this._writer.Write3BitDouble(mLeaderStyle.BlockContentScale);
			//	B	293	Is block scale enabled
			this._writer.WriteBit(mLeaderStyle.EnableBlockContentScale);
			//	BD	141	Block rotation (radians)
			this._writer.WriteBitDouble(mLeaderStyle.BlockContentRotation);
			//	B	294	Is block rotation enabled
			this._writer.WriteBit(mLeaderStyle.EnableBlockContentRotation);
			//	BS	177	Block connection type (0 = MLeader connects to the block extents, 1 = MLeader connects to the block base point)
			this._writer.WriteBitShort((short)mLeaderStyle.BlockContentConnection);
			//	BD	142	Scale factor
			this._writer.WriteBitDouble(mLeaderStyle.ScaleFactor);
			//	B	295	Property changed, meaning not totally clear
			//	might be set to true if something changed after loading,
			//	or might be used to trigger updates in dependent MLeaders.
			//	sequence seems to be different in DXF
			this._writer.WriteBit(mLeaderStyle.OverwritePropertyValue);
			//	B	296	Is annotative?
			this._writer.WriteBit(mLeaderStyle.IsAnnotative);
			//	BD	143	Break size
			this._writer.WriteBitDouble(mLeaderStyle.BreakGapSize);

			if (this.R2010Plus)
			{
				//	BS	271	Attachment direction (see paragraph on LEADER for more details).
				this._writer.WriteBitShort((short)mLeaderStyle.TextAttachmentDirection);
				//	BS	273	Top attachment (see paragraph on LEADER for more details).
				this._writer.WriteBitShort((short)mLeaderStyle.TextBottomAttachment);
				//	BS	272	Bottom attachment (see paragraph on LEADER for more details).
				this._writer.WriteBitShort((short)mLeaderStyle.TextTopAttachment);
			}

			if (this.R2013Plus)
			{
				//	B	298 Undocumented, found in DXF
				this._writer.WriteBit(mLeaderStyle.UnknownFlag298);
			}
		}

		private void writeObjectContextData(ObjectContextData objectContextData) {
			//BS	70	Version.
			this._writer.WriteBitShort(objectContextData.Version);
			//B	-	Has file to extension dictionary.
			this._writer.WriteBit(objectContextData.HasFileToExtensionDictionary);
			//B	290	Default flag.
			this._writer.WriteBit(objectContextData.Default);
		}

		private void writeAnnotScaleObjectContextData(AnnotScaleObjectContextData annotScaleObjectContextData) {
			this._writer.HandleReference(DwgReferenceType.HardPointer, annotScaleObjectContextData.Scale);
		}

		private void writeMultiLeaderAnnotContext(MultiLeaderObjectContextData multiLeaderAnnotContext) {
			writeMultiLeaderAnnotContextSubObject(false, multiLeaderAnnotContext);
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

		private void writeRasterVariables(RasterVariables vars)
		{
			//Common:
			//Classver BL 90 classversion
			this._writer.WriteBitLong(vars.ClassVersion);
			//Dispfrm BS 70 displayframe
			this._writer.WriteBitShort(vars.IsDisplayFrameShown ? (short)1 : (short)0);
			//Dispqual BS 71 display quality
			this._writer.WriteBitShort((short)vars.DisplayQuality);
			//Units BS 72 units
			this._writer.WriteBitShort((short)vars.Units);
		}

		private void writeScale(Scale scale)
		{
			//BS	70	Unknown(ODA writes 0).
			this._writer.WriteBitShort(0);
			//TV	300	Name
			this._writer.WriteVariableText(scale.Name);
			//BD	140	Paper units(numerator)
			this._writer.WriteBitDouble(scale.PaperUnits);
			//BD	141	Drawing units(denominator, divided by 10).
			this._writer.WriteBitDouble(scale.DrawingUnits);
			//B	290	Has unit scale
			this._writer.WriteBit(scale.IsUnitScale);
		}

		private void writeSpatialFilter(SpatialFilter filter)
		{
			//Common:
			//Numpts BS 70 number of points
			this._writer.WriteBitShort((short)filter.BoundaryPoints.Count);
			//Repeat numpts times:
			foreach (var pt in filter.BoundaryPoints)
			{
				//pt0 2RD 10 a point on the clip boundary
				this._writer.Write2RawDouble(pt);
			}

			//Extrusion 3BD 210 extrusion
			this._writer.Write3BitDouble(filter.Normal);
			//Clipbdorg 3BD 10 clip bound origin
			this._writer.Write3BitDouble(filter.Origin);
			//Dispbound BS 71 display boundary
			this._writer.WriteBitShort((short)(filter.DisplayBoundary ? 1 : 0));
			//Frontclipon BS 72 1 if front clip on
			this._writer.WriteBitShort((short)(filter.ClipFrontPlane ? 1 : 0));
			if (filter.ClipFrontPlane)
			{
				//Frontdist BD 40 front clip dist(present if frontclipon == 1)
				this._writer.WriteBitDouble(filter.FrontDistance);
			}

			//Backclipon BS 73 1 if back clip on
			this._writer.WriteBitShort((short)(filter.ClipBackPlane ? 1 : 0));
			if (filter.ClipBackPlane)
			{
				//Backdist BD 41 back clip dist(present if backclipon == 1)
				this._writer.WriteBitDouble(filter.BackDistance);
			}

			//Invblktr 12BD 40 inverse block transformation matrix
			//(double[4][3], column major order)
			this.write4x3Matrix(filter.InverseInsertTransform);
			//clipbdtr 12BD 40 clip bound transformation matrix
			//(double[4][3], column major order)
			this.write4x3Matrix(filter.InsertTransform);
		}

		private void write4x3Matrix(Matrix4 matrix)
		{
			for (int i = 0; i < 3; i++)
			{
				for (int j = 0; j < 4; j++)
				{
					this._writer.WriteBitDouble(matrix[i, j]);
				}
			}
		}

		private void writeSortEntitiesTable(SortEntitiesTable sortEntitiesTable)
		{
			//parenthandle (soft pointer)
			this._writer.HandleReference(DwgReferenceType.SoftPointer, sortEntitiesTable.BlockOwner);

			//Common:
			//Numentries BL number of entries
			this._writer.WriteBitLong(sortEntitiesTable.Count());
			foreach (var item in sortEntitiesTable)
			{
				//Sort handle(numentries of these, CODE 0, i.e.part of the main bit stream, not of the handle bit stream!).
				//The sort handle does not have to point to an entity (but it can).
				//This is just the handle used for determining the drawing order of the entity specified by the entity handle in the handle bit stream.
				//When the sortentstable doesn’t have a
				//mapping from entity handle to sort handle, then the entity’s own handle is used for sorting.
				this._writer.Main.HandleReference(item.SortHandle);
				this._writer.HandleReference(DwgReferenceType.SoftPointer, item.Entity);
			}
		}

		private void writeXRecord(XRecord xrecord)
		{
			MemoryStream stream = new MemoryStream();
			StreamIO ms = new StreamIO(stream);
			ms.EndianConverter = new LittleEndianConverter();

			foreach (XRecord.Entry entry in xrecord.Entries)
			{
				if (entry.Value == null)
				{
					continue;
				}

				ms.Write<short, LittleEndianConverter>((short)entry.Code);
				GroupCodeValueType groupValueType = GroupCodeValue.TransformValue(entry.Code);

				switch (groupValueType)
				{
					case GroupCodeValueType.Byte:
					case GroupCodeValueType.Bool:
						ms.Write(Convert.ToByte(entry.Value, System.Globalization.CultureInfo.InvariantCulture));
						break;
					case GroupCodeValueType.Int16:
					case GroupCodeValueType.ExtendedDataInt16:
						ms.Write(Convert.ToInt16(entry.Value, System.Globalization.CultureInfo.InvariantCulture));
						break;
					case GroupCodeValueType.Int32:
					case GroupCodeValueType.ExtendedDataInt32:
						ms.Write(Convert.ToInt32(entry.Value, System.Globalization.CultureInfo.InvariantCulture));
						break;
					case GroupCodeValueType.Int64:
						ms.Write(Convert.ToInt64(entry.Value, System.Globalization.CultureInfo.InvariantCulture));
						break;
					case GroupCodeValueType.Double:
					case GroupCodeValueType.ExtendedDataDouble:
						double d = (entry.Value as double?).Value;
						ms.Write<double, LittleEndianConverter>(d);
						break;
					case GroupCodeValueType.Point3D:
						XYZ xyz = (entry.Value as XYZ?).Value;
						ms.Write<double, LittleEndianConverter>(xyz.X);
						ms.Write<double, LittleEndianConverter>(xyz.Y);
						ms.Write<double, LittleEndianConverter>(xyz.Z);
						break;
					case GroupCodeValueType.Chunk:
					case GroupCodeValueType.ExtendedDataChunk:
						byte[] array = (byte[])entry.Value;
						ms.Write((byte)array.Length);
						ms.WriteBytes(array);
						break;
					case GroupCodeValueType.Handle:
						var obj = entry.GetReference();
						if (obj == null)
						{
							this.writeStringInStream(ms, string.Empty);
						}
						else
						{
							this.writeStringInStream(ms, obj.Handle.ToString("X", System.Globalization.CultureInfo.InvariantCulture));
						}
						break;
					case GroupCodeValueType.String:
					case GroupCodeValueType.ExtendedDataString:
						string text = (string)entry.Value;
						this.writeStringInStream(ms, text);
						break;
					case GroupCodeValueType.ObjectId:
					case GroupCodeValueType.ExtendedDataHandle:
						if (entry.GetReference() == null)
						{
							ms.Write<ulong, LittleEndianConverter>(0);
						}
						else
						{
							ms.Write<ulong, LittleEndianConverter>(entry.GetReference().Handle);
						}
						break;
					default:
						throw new NotSupportedException();
				}
			}

			//Common:
			//Numdatabytes BL number of databytes
			this._writer.WriteBitLong((int)ms.Length);
			this._writer.WriteBytes(stream.GetBuffer(), 0, (int)ms.Length);

			//R2000+:
			if (this.R2000Plus)
			{
				//Cloning flag BS 280
				this._writer.WriteBitShort((short)xrecord.CloningFlags);
			}
		}

		private void writeStringInStream(StreamIO ms, string text)
		{
			if (this.R2007Plus)
			{
				if (string.IsNullOrEmpty(text))
				{
					ms.Write<short, LittleEndianConverter>(0);
					return;
				}

				ms.Write<short, LittleEndianConverter>((short)text.Length);
				ms.Write(text, System.Text.Encoding.Unicode);
			}
			else if (string.IsNullOrEmpty(text))
			{
				ms.Write<short, LittleEndianConverter>(0);
				ms.Write((byte)CadUtils.GetCodeIndex((CodePage)this._writer.Encoding.CodePage));
			}
			else
			{
				ms.Write<short, LittleEndianConverter>((short)text.Length);
				ms.Write((byte)CadUtils.GetCodeIndex((CodePage)this._writer.Encoding.CodePage));
				ms.Write(text, this._writer.Encoding);
			}
		}
	}
}