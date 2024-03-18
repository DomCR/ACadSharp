namespace ACadSharp.Objects.Collections
{
	public class MLineStyleCollection : ObjectDictionaryCollection<MLineStyle>
	{
		public MLineStyleCollection(CadDictionary dictionary) : base(dictionary)
		{
			this._dictionary = dictionary;
		}
	}
}
