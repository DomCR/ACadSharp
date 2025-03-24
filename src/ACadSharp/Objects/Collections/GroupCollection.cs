using ACadSharp.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ACadSharp.Objects.Collections
{
	public class GroupCollection : ObjectDictionaryCollection<Group>
	{
		public GroupCollection(CadDictionary dictionary) : base(dictionary)
		{
			this._dictionary = dictionary;
		}

		/// <inheritdoc/>
		public override void Add(Group entry)
		{
			foreach (var e in entry.Entities)
			{
				if (e.Document != this._dictionary.Document)
				{
					throw new InvalidOperationException("Entities in a group must be in the same document as the group being added.");
				}
			}

			if (entry.IsUnnamed)
			{
				int next = 0;
				foreach (Group group in this.Where(g => g.IsUnnamed))
				{
					var num = int.Parse(Regex.Match(group.Name, @"\d+").Value);
					if (num > next)
					{
						next = num;
					}
				}

				entry.Name = $"*D{next++}";
			}

			base.Add(entry);
		}

		/// <summary>
		/// Creates a group with a collection of entities.
		/// </summary>
		/// <remarks>
		/// The entities must be assigned to the owner <see cref="CadDocument"/> of this collection.
		/// </remarks>
		/// <param name="entities"></param>
		/// <returns></returns>
		public Group CreateGroup(IEnumerable<Entity> entities)
		{
			return this.CreateGroup(string.Empty, entities);
		}

		/// <summary>
		/// Creates a group with a collection of entities.
		/// </summary>
		/// <remarks>
		/// The entities must be assigned to the owner <see cref="CadDocument"/> of this collection.
		/// </remarks>
		/// <param name="name"></param>
		/// <param name="entities"></param>
		/// <returns></returns>
		public Group CreateGroup(string name, IEnumerable<Entity> entities)
		{
			var group = new Group(name);
			this.Add(group);
			group.AddRange(entities);
			return group;
		}
	}
}
