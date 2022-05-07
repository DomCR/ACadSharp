using ACadSharp.Entities;
using System.Collections.Generic;

namespace ACadSharp.IO
{
	internal class CadObjectHolder
	{
		public Queue<Entity> Entities { get; } = new Queue<Entity>();

		public Queue<CadObject> Objects { get; } = new Queue<CadObject>();
	}
}
