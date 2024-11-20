namespace ACadSharp.Objects.Collections
{
	public class ColorCollection : ObjectDictionaryCollection<ImageDefinition>
	{
		public ColorCollection(CadDictionary dictionary) : base(dictionary)
		{
			this._dictionary = dictionary;
		}
	}
}
