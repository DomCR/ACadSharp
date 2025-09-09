namespace ACadSharp.Objects
{
	public partial class XRecord
	{
		public class Entry
		{
			public int Code { get; }

			public object Value { get; set; }

			public GroupCodeValueType GroupCode
			{
				get
				{
					return GroupCodeValue.TransformValue(this.Code);
				}
			}

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

			public XRecord Owner { get; set; }

			internal Entry(int code, object value, XRecord owner)
			{
				this.Code = code;
				this.Value = value;
				this.Owner = owner;
			}

			public CadObject GetReference()
			{
				if (!this.HasLinkedObject)
					return null;

				if (this.Value is CadObject cadObject)
				{
					if(cadObject.Document != this.Owner.Document)
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
				return $"{Code}:{Value}";
			}
		}
	}
}
