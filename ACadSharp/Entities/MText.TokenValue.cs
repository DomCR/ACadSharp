using ACadSharp.Attributes;
using System;
using System.Collections.Generic;

namespace ACadSharp.Entities
{
    public partial class MText
    {
        public class TokenValue : Token
        {
            public IReadOnlyList<ReadOnlyMemory<char>> Values { get; internal set; }
            public string CombinedValues => string.Concat(Values);

            public TokenValue()
            {
            }

            public TokenValue(MText.Format fontState, ReadOnlyMemory<char> value)
                : base(fontState)
            {
                Values = new[] { value };
            }

            public TokenValue(MText.Format fontState, ReadOnlyMemory<char>[] values)
                : base(fontState)
            {
                Values = values;
            }
            internal TokenValue(MText.Format fontState, string value)
                : base(fontState)
            {
                Values = new[] { value.AsMemory() };
            }

            internal TokenValue(string value)
                : base(new MText.Format())
            {
                Values = new[] { value.AsMemory() };
            }

            public override string ToString()
            {
                return CombinedValues;
            }


            public override bool Equals(object? obj)
            {
                if (ReferenceEquals(null, obj))
                {
                    return false;
                }

                if (ReferenceEquals(this, obj))
                {
                    return true;
                }

                if (obj.GetType() != this.GetType())
                {
                    return false;
                }

                return ((TokenValue)obj).CombinedValues == CombinedValues;
            }
        }


    }
}
