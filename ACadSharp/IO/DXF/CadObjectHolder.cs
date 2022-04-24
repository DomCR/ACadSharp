using ACadSharp.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACadSharp.IO
{
	internal class CadObjectHolder
	{
		public List<Entity> Entities { get; } = new List<Entity>();

		public List<CadObject> Objects { get; } = new List<CadObject>();
	}
}
