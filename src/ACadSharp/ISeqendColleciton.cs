using ACadSharp.Entities;
using System;
using System.Collections;

namespace ACadSharp
{
	public interface ISeqendCollection : IEnumerable
	{
		public event EventHandler<CollectionChangedEventArgs> OnSeqendAdded;

		public event EventHandler<CollectionChangedEventArgs> OnSeqendRemoved;

		Seqend Seqend { get; }
	}
}
