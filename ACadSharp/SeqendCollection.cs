using ACadSharp.Entities;
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
	}
}
