using ACadSharp.Objects;
using CSUtilities.Converters;
using CSUtilities.IO;
using System;
using System.IO;
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
				case Material:
				case MultiLeaderAnnotContext:
				case SortEntitiesTable:
				case VisualStyle:
				case XRecord:
					this.notify($"Object type not implemented {obj.GetType().FullName}", NotificationType.NotImplemented);
					return;
			}

			this.writeCommonNonEntityData(obj);

			switch (obj)
			{
				case AcdbPlaceHolder acdbPlaceHolder:
					this.writeAcdbPlaceHolder(acdbPlaceHolder);
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
				case Group group:
					this.writeGroup(group);
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
				case PlotSettings plotsettings:
					this.writePlotSettings(plotsettings);
					break;
				case Scale scale:
					this.writeScale(scale);
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

		private void writeCadDictionaryWithDefault(CadDictionaryWithDefault dictionary)
		{
			this.writeDictionary(dictionary);

			//H 7 Default entry (hard pointer)
			this._writer.HandleReference(DwgReferenceType.HardPointer, dictionary.DefaultEntry);
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

		private void writeDictionaryVariable(DictionaryVariable dictionaryVariable)
		{
			//Intval RC an integer value
			this._writer.WriteByte(0);

			//BS a string
			this._writer.WriteVariableText(dictionaryVariable.Value);
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
			if (!R2010Plus)
			{
				return;
			}

			//	BS	179	Version expected: 2
			this._writer.WriteBitShort(2);

			//	BS	170	Content type (see paragraph on LEADER for more details).
			this._writer.WriteBitShort((short)mLeaderStyle.ContentType);
			//	BS	171	Draw multi-leader order (0 = draw content first, 1 = draw leader first)
			this._writer.WriteBitShort((short)mLeaderStyle.MultiLeaderDrawOrder);
			//	BS	172	Draw leader order (0 = draw leader head first, 1 = draw leader tail first)
			this._writer.WriteBitShort((short)mLeaderStyle.LeaderDrawOrder);
			//	BL	90	Maximum number of points for leader
			this._writer.WriteBitShort((short)mLeaderStyle.MaxLeaderSegmentsPoints);
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
			if (R2010Plus)
			{
				//	IF IsNewFormat OR DXF file
				//	BS	175	Text angle type (see paragraph on LEADER for more details).
				this._writer.WriteBitShort((short)mLeaderStyle.TextAngle);
				//	END IF IsNewFormat OR DXF file
			}
			//	BS	176	Text alignment type
			this._writer.WriteBitShort((short)mLeaderStyle.TextAlignment);
			//	CMC	93	Text color
			this._writer.WriteCmColor(mLeaderStyle.TextColor);
			//	BD	45	Text height
			this._writer.WriteBitDouble(mLeaderStyle.TextHeight);
			//	B	292	Text frame enabled
			this._writer.WriteBit(mLeaderStyle.TextFrame);
			if (R2010Plus)
			{
				//	IF IsNewFormat OR DXF file
				//	B	297	Always align text left
				this._writer.WriteBit(mLeaderStyle.TextAlignAlwaysLeft);
				//	END IF IsNewFormat OR DXF file
			}
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

			//	BS	271	Attachment direction (see paragraph on LEADER for more details).
			this._writer.WriteBitShort((short)mLeaderStyle.TextAttachmentDirection);
			//	BS	273	Top attachment (see paragraph on LEADER for more details).
			this._writer.WriteBitShort((short)mLeaderStyle.TextBottomAttachment);
			//	BS	272	Bottom attachment (see paragraph on LEADER for more details).
			this._writer.WriteBitShort((short)mLeaderStyle.TextTopAttachment);
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

		private void writeScale(Scale scale)
		{
			//BS	70	Unknown(ODA writes 0).
			this._writer.WriteBitShort(scale.Unknown);
			//TV	300	Name
			this._writer.WriteVariableText(scale.Name);
			//BD	140	Paper units(numerator)
			this._writer.WriteBitDouble(scale.PaperUnits);
			//BD	141	Drawing units(denominator, divided by 10).
			this._writer.WriteBitDouble(scale.DrawingUnits);
			//B	290	Has unit scale
			this._writer.WriteBit(scale.IsUnitScale);
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

				ms.Write<short>((short)entry.Code);
				GroupCodeValueType groupValueType = GroupCodeValue.TransformValue(entry.Code);

				switch (groupValueType)
				{
					case GroupCodeValueType.None:
						break;
					case GroupCodeValueType.String:
						break;
					case GroupCodeValueType.Point3D:
						break;
					case GroupCodeValueType.Double:
						break;
					case GroupCodeValueType.Int16:
						break;
					case GroupCodeValueType.Int32:
						break;
					case GroupCodeValueType.Int64:
						break;
					case GroupCodeValueType.Handle:
						break;
					case GroupCodeValueType.ObjectId:
						break;
					case GroupCodeValueType.Bool:
						break;
					case GroupCodeValueType.Chunk:
						break;
					case GroupCodeValueType.Comment:
						break;
					case GroupCodeValueType.ExtendedDataString:
						break;
					case GroupCodeValueType.ExtendedDataChunk:
						break;
					case GroupCodeValueType.ExtendedDataHandle:
						break;
					case GroupCodeValueType.ExtendedDataDouble:
						break;
					case GroupCodeValueType.ExtendedDataInt16:
						break;
					case GroupCodeValueType.ExtendedDataInt32:
						break;
					default:
						break;
				}
			}

			//Common:
			//Numdatabytes BL number of databytes
			this._writer.WriteBitLong((int)ms.Length);
			this._writer.WriteBytes(stream.GetBuffer());

			//R2000+:
			if (this.R2000Plus)
			{
				//Cloning flag BS 280
				this._writer.WriteBitShort((short)xrecord.ClonningFlags);
			}

		}
	}
}
