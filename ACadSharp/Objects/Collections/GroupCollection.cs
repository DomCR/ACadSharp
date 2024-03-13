using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACadSharp.Objects.Collections
{
	public class GroupCollection : IObservableCollection<Group>
	{
		public event EventHandler<CollectionChangedEventArgs> OnAdd;
		public event EventHandler<CollectionChangedEventArgs> OnRemove;

		public IEnumerator<Group> GetEnumerator()
		{
			throw new NotImplementedException();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			throw new NotImplementedException();
		}
	}
}
