using System.Collections.Generic;
using System.Linq;

namespace ACadSharp.IO
{
	public class DxfWriterOptions
	{
		private static readonly string[] _mainVariables = new string[]
		{

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
		public IEnumerable<string> HeaderVariables { get; }

		private HashSet<string> _headerVariables;

		public DxfWriterOptions()
		{
			this._headerVariables = new HashSet<string>(_mainVariables);
		}

		public void AddHeaderVariable(string name)
		{
			this._headerVariables.Add(name);
		}

		public void RemoveHeaderVariable(string name)
		{
			if(_mainVariables.Select(v => v.ToLowerInvariant()).Contains(name.ToLowerInvariant()))
			{

			}

			this._headerVariables.Add(name);
		}
	}
}
