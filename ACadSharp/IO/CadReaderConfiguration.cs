using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACadSharp.IO
{
	public abstract class CadReaderConfiguration
	{

		/// <summary>
		/// The reader will try to continue when an exception is found, unless this setting is true
		/// </summary>
		public bool StopAtExceptions { get; set; } = false;
	}
}
