using System.Collections.Generic;

namespace ACadSharp.Objects.Collections
{
	public class ScaleCollection : ObjectDictionaryCollection<Scale>
	{
		private readonly Dictionary<string, Scale> _scales = new Dictionary<string, Scale>();

		public ScaleCollection(CadDictionary dictionary) : base(dictionary)
		{
			this._dictionary = dictionary;

			foreach (Scale item in this._dictionary)
			{
				this._scales.Add(item.Name, item);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="entry"></param>
		public override void Add(Scale entry)
		{
			base.Add(entry);
		}
	}
}
