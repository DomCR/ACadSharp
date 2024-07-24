using System;

namespace ACadSharp.Objects.Collections
{
	public class LayoutCollection : ObjectDictionaryCollection<Layout>
	{
		public LayoutCollection(CadDictionary dictionary) : base(dictionary)
		{
			this._dictionary = dictionary;
		}

		/// <inheritdoc/>
		public override bool Remove(string name, out Layout entry)
		{
			if (name.Equals(Layout.ModelLayoutName, StringComparison.InvariantCultureIgnoreCase)
				|| name.Equals(Layout.ModelLayoutName, StringComparison.InvariantCultureIgnoreCase))
			{
				throw new ArgumentException($"The Layout {name} cannot be removed.");
			}

			return base.Remove(name, out entry);
		}
	}
}
