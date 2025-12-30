namespace ACadSharp.Objects
{
	public partial class XRecord
	{
		/// <summary>
		/// Represents a single data entry containing a group code, value, and associated owner within a <see cref="XRecord"/>.
		/// </summary>
		/// <remarks>The <see cref="Entry"/> class encapsulates a code-value pair, commonly used in CAD data
		/// structures to represent individual fields or properties. Each entry is associated with a group code that
		/// determines the type and interpretation of its value. The <see cref="Owner"/> property links the entry to its
		/// parent <see cref="XRecord"/>.</remarks>
		public class Entry
		{
			/// <summary>
			/// Gets the numeric code associated with the current instance.
			/// </summary>
			public int Code { get; }

			/// <summary>
			/// Gets or sets the value associated with the current instance.
			/// </summary>
			public object Value { get; set; }

			public GroupCodeValueType GroupCode
			{
				get
				{
					return GroupCodeValue.TransformValue(this.Code);
				}
			}

			/// <summary>
			/// Gets a value indicating whether this group code is associated with a linked object, such as a handle or object
			/// ID.
			/// </summary>
			public bool HasLinkedObject
			{
				get
				{
					switch (this.GroupCode)
					{
						case GroupCodeValueType.Handle:
						case GroupCodeValueType.ObjectId:
						case GroupCodeValueType.ExtendedDataHandle:
							return true;
						default:
							return false;
					}
				}
			}

			/// <summary>
			/// Gets or sets the owner of the current record.
			/// </summary>
			public XRecord Owner { get; set; }

			internal Entry(int code, object value, XRecord owner)
			{
				this.Code = code;
				this.Value = value;
				this.Owner = owner;
			}

			/// <summary>
			/// Gets the referenced <see cref="CadObject"/> if one is linked and belongs to the same document as the owner.
			/// </summary>
			/// <returns>The referenced <see cref="CadObject"/> if a linked object exists and is associated with the same document as the
			/// owner; otherwise, <see langword="null"/>.</returns>
			public CadObject GetReference()
			{
				if (!this.HasLinkedObject)
					return null;

				if (this.Value is CadObject cadObject)
				{
					if (cadObject.Document != this.Owner.Document)
					{
						return null;
					}

					return cadObject;
				}
				else
				{
					return null;
				}
			}

			/// <inheritdoc/>
			public override string ToString()
			{
				return $"{this.Code}:{this.Value}";
			}
		}
	}
}
