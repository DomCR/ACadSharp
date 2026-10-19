using ACadSharp.Header;
using ACadSharp.IO.DXF.DxfStreamWriter;
using System.Collections.Generic;
using System.Linq;

namespace ACadSharp.IO.DXF
{
	internal class DxfHeaderSectionWriter : DxfSectionWriterBase
	{
		private static readonly HashSet<string> _r12Variables = new HashSet<string>(@"
			$ACADVER $INSBASE $EXTMIN $EXTMAX $LIMMIN $LIMMAX $ORTHOMODE $REGENMODE
			$FILLMODE $QTEXTMODE $MIRRTEXT $DRAGMODE $LTSCALE $OSMODE $ATTMODE $TEXTSIZE
			$TRACEWID $TEXTSTYLE $CLAYER $CELTYPE $CECOLOR $DIMSCALE $DIMASZ $DIMEXO
			$DIMDLI $DIMRND $DIMDLE $DIMEXE $DIMTP $DIMTM $DIMTXT $DIMCEN $DIMTSZ
			$DIMTOL $DIMLIM $DIMTIH $DIMTOH $DIMSE1 $DIMSE2 $DIMTAD $DIMZIN $DIMBLK
			$DIMASO $DIMSHO $DIMPOST $DIMAPOST $DIMALT $DIMALTD $DIMALTF $DIMLFAC
			$DIMTOFL $DIMTVP $DIMTIX $DIMSOXD $DIMSAH $DIMBLK1 $DIMBLK2 $DIMSTYLE
			$DIMCLRD $DIMCLRE $DIMCLRT $DIMTFAC $DIMGAP $LUNITS $LUPREC $SKETCHINC
			$FILLETRAD $AUNITS $AUPREC $MENU $ELEVATION $PELEVATION $THICKNESS $LIMCHECK
			$BLIPMODE $CHAMFERA $CHAMFERB $SKPOLY $TDCREATE $TDUPDATE $TDINDWG $TDUSRTIMER
			$USRTIMER $ANGBASE $ANGDIR $PDMODE $PDSIZE $PLINEWID $COORDS $SPLFRAME
			$SPLINETYPE $SPLINESEGS $ATTDIA $ATTREQ $HANDLING $HANDSEED $SURFTAB1
			$SURFTAB2 $SURFTYPE $SURFU $SURFV $UCSNAME $UCSORG $UCSXDIR $UCSYDIR
			$PUCSNAME $PUCSORG $PUCSXDIR $PUCSYDIR $USERI1 $USERI2 $USERI3 $USERI4
			$USERI5 $USERR1 $USERR2 $USERR3 $USERR4 $USERR5 $WORLDVIEW $SHADEDGE
			$SHADEDIF $TILEMODE $MAXACTVP $PLIMCHECK $PEXTMIN $PEXTMAX $PLIMMIN
			$PLIMMAX $UNITMODE $VISRETAIN $PLINEGEN $PSLTSCALE
		".Split((char[])null, System.StringSplitOptions.RemoveEmptyEntries));

		public override string SectionName { get { return DxfFileToken.HeaderSection; } }

		public CadHeader Header { get { return this._document.Header; } }

		public DxfHeaderSectionWriter(IDxfStreamWriter writer, CadDocument document, CadObjectHolder holder, DxfWriterConfiguration configuration)
			: base(writer, document, holder, configuration)
		{
		}

		protected override void writeSection()
		{
			Dictionary<string, CadSystemVariable> map = CadHeader.GetHeaderMap();

			foreach (KeyValuePair<string, CadSystemVariable> item in map)
			{
				if (this.Version <= ACadVersion.AC1009 && !_r12Variables.Contains(item.Key)) continue;
				if (!this.Configuration.WriteAllHeaderVariables && !this.Configuration.HeaderVariables.Contains(item.Key))
					continue;

				if (item.Value.ReferenceType.HasFlag(DxfReferenceType.Ignored))
					continue;

				if (item.Value.GetValue(this.Header) == null)
					continue;

				this._writer.Write(DxfCode.CLShapeText, item.Key);

				if (item.Key == "$HANDSEED")    //Not very elegant but by now...
				{
					this._writer.Write(DxfCode.Handle, this._document.Header.HandleSeed);
					continue;
				}

				if (item.Key == "$CECOLOR")
				{
					this._writer.Write(62, this._document.Header.CurrentEntityColor.GetApproxIndex());
					continue;
				}

				foreach (var csv in item.Value.DxfCodes)
				{
					object value = item.Value.GetSystemValue(csv, this._document.Header);

					if (value == null)
						continue;

					this._writer.Write((DxfCode)csv, value);
				}
			}
		}
	}
}
