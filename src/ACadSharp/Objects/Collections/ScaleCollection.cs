﻿namespace ACadSharp.Objects.Collections
{
	public class ScaleCollection : ObjectDictionaryCollection<Scale>
	{
		public ScaleCollection(CadDictionary dictionary) : base(dictionary)
		{
			this._dictionary = dictionary;
		}
	}
}
