namespace ACadSharp.Objects.Collections
{
	public class GroupCollection : ObjectDictionaryCollection<Group>
	{
		public GroupCollection(CadDictionary dictionary) : base(dictionary)
		{
			this._dictionary = dictionary;
		}
	}
}
