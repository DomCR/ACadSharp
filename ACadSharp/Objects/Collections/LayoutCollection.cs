namespace ACadSharp.Objects.Collections
{
	public class LayoutCollection : ObjectDictionaryCollection<Layout>
	{
		public LayoutCollection(CadDictionary dictionary) : base(dictionary)
		{
			this._dictionary = dictionary;
		}
	}
}
