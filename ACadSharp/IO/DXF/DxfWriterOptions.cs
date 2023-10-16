using ACadSharp.Header;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ACadSharp.IO
{
	public class DxfWriterOptions
	{
		/// <summary>
		/// Variables that must be writen in a dxf file
		/// </summary>
		public static readonly string[] Variables = new string[]
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
			"$TDINDWG",
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
			this._headerVariables = new HashSet<string>(Variables);
		}

		/// <summary>
		/// Add a Header variable name to be added in the dxf document
		/// </summary>
		/// <remarks>
		/// The name of the variable must exist in <see cref="CadHeader.GetHeaderMap"/>
		/// </remarks>
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

		/// <summary>
		/// Remove a Header variable name so is not added in the dxf document
		/// </summary>
		/// <remarks>
		/// The name cannot be a in the <see cref="Variables"/> list
		/// </remarks>
		/// <param name="name"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentException"></exception>
		public bool RemoveHeaderVariable(string name)
		{
			if (Variables.Select(v => v.ToLowerInvariant()).Contains(name.ToLowerInvariant()))
			{
				throw new ArgumentException($"The variable {name} cannot be removed from the set", nameof(name));
			}

			return this._headerVariables.Remove(name);
		}
	}
}
