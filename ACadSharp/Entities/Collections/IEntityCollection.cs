using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace ACadSharp.Entities.Collections
{
	[Obsolete]
	public interface IEntityCollection<T> : ICollection<Entity>
		where T : Entity
	{
	}
}
