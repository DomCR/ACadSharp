using ACadSharp.Entities;
using System;
using System.Linq;

namespace ACadSharp
{
	/// <summary>
	/// Represents a collection of <see cref="CadObject"/> ended by a <see cref="Entities.Seqend"/> entity
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class SeqendCollection<T> : CadObjectCollection<T>, ISeqendCollection
		where T : CadObject
	{
		public event EventHandler<CollectionChangedEventArgs> OnSeqendAdded;

		public event EventHandler<CollectionChangedEventArgs> OnSeqendRemoved;

		public Seqend Seqend
		{
			get
			{
				if (this._entries.Any())
					return this._seqend;
				else
					return null;
			}
			internal set
			{
				this._seqend = value;
				this._seqend.Owner = this.Owner;
			}
		}

		private Seqend _seqend;

		public SeqendCollection(CadObject owner) : base(owner)
		{
			this._seqend = new Seqend();
			this._seqend.Owner = owner;
		}

		/// <inheritdoc/>
		public override void Add(T item)
		{
			bool addSeqend = false;
			if (!this._entries.Any())
			{
				addSeqend = true;
			}

			base.Add(item);

			// The add could fail due an Exception
			if (addSeqend && this._entries.Any())
			{
				this.OnSeqendAdded?.Invoke(this, new CollectionChangedEventArgs(this._seqend));
			}
		}

		/// <inheritdoc/>
		public override T Remove(T item)
		{
			var e = base.Remove(item);
			if(e != null)
			{
				this.OnSeqendRemoved?.Invoke(this, new CollectionChangedEventArgs(this._seqend));
			}

			return e;
		}
	}
}
