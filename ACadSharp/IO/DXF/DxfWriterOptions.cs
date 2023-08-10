using ACadSharp.Header;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ACadSharp.IO
{
	public class DxfWriterOptions
	{
		private static readonly string[] _mainVariables = new string[]
		{
			"$ACADVER",
			"$DWGCODEPAGE",
			"$LASTSAVEDBY",
			"$HANDSEED",
			"$ANGBASE",
			"$ANGDIR",
			"$ATTMODE",
			"$AUNITS",
			"$AUPREC",
			"$CECOLOR",
			"$CELTSCALE",
			"$CELTYPE",
			"$CELWEIGHT",
			"$CLAYER",
			"$CMLJUST",
			"$CMLSCALE",
			"$CMLSTYLE",
			"$DIMSTYLE",
			"$TEXTSIZE",
			"$TEXTSTYLE",
			"$LUNITS",
			"$LUPREC",
			"$MIRRTEXT",
			"$EXTNAMES",
			"$INSBASE",
			"$INSUNITS",
			"$LTSCALE",
			"$LWDISPLAY",
			"$PDMODE",
			"$PDSIZE",
			"$PLINEGEN",
			"$PSLTSCALE",
			"$SPLINESEGS",
			"$SURFU",
			"$SURFV",
			"$TDCREATE",
			"$TDUCREATE",
			"$TDUPDATE",
			"$TDUUPDATE",
			"$TDUPDATE",
		};

		/// <summary>
		/// Flag to write all the header variables in the dxf file
		/// </summary>
		/// <remarks>
		/// Some variables are version sensitive, writing all the variables may cause a crash in other applications, to write an specific variable add it to the <see cref="AddHeaderVariable"/> set
		/// </remarks>
		/// <value>
		/// false
		/// </value>
		public bool WriteAllHeaderVariables { get; set; } = false;

		/// <summary>
		/// Header variables to write in the dxf file
		/// </summary>
		public IEnumerable<string> HeaderVariables { get { return this._headerVariables.AsEnumerable(); } }

		private HashSet<string> _headerVariables;

		public DxfWriterOptions()
		{
			this._headerVariables = new HashSet<string>(_mainVariables);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <exception cref="ArgumentException"></exception>
		public void AddHeaderVariable(string name)
		{
			Dictionary<string, CadSystemVariable> map = CadHeader.GetHeaderMap();

			if (!map.ContainsKey(name))
			{
				throw new ArgumentException($"The variable {name} does not exist in the header", nameof(name));
			}

			this._headerVariables.Add(name);
		}

		public bool RemoveHeaderVariable(string name)
		{
			if (_mainVariables.Select(v => v.ToLowerInvariant()).Contains(name.ToLowerInvariant()))
			{
				throw new ArgumentException($"The variable {name} cannot be removed from the set", nameof(name));
			}

			return this._headerVariables.Remove(name);
		}
	}
}
