using ACadSharp.Entities;

namespace ACadSharp
{
	/// <summary>
	/// Represents a collection of <see cref="CadObject"/> ended by a <see cref="Entities.Seqend"/> entity
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class SeqendCollection<T> : CadObjectCollection<T>, ISeqendColleciton
		where T : CadObject
	{
		public Seqend Seqend
		{
			get { return _seqend; }
			internal set
			{
				this._seqend = value;
				this._seqend.Owner = this.Owner;
			}
		}

		private Seqend _seqend;

		public SeqendCollection(CadObject owner) : base(owner)
		{
			this.Seqend = new Seqend();
		}
	}
}
