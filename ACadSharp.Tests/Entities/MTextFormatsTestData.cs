using System;
using System.Collections.Generic;
using System.Text;
using ACadSharp.Entities;

namespace ACadSharp.Tests.Entities
{
    public class MTextFormatsTestData
    {
        public MTextFormatsTestData(string input, MText.Token? expected)
            : this(input, expected == null ? null : new[] { expected })
        {
        }

        public MTextFormatsTestData(string text, MText.Token[]? parsed)
        {
            this.Text = text;
            this.Parsed = parsed;
        }

        public string Text { get; set; }
        public MText.Token[]? Parsed { get; set; }

        public virtual bool Equals(MTextFormatsTestData? other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Text == other.Text
                   && Parsed?.Equals(other.Parsed) == true;
        }
    }
}
