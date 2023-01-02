using ACadSharp.Attributes;
using System.Collections.Generic;

namespace ACadSharp.Entities
{
    public partial class MText
    {
        public abstract class Token
        {
            public MText.Format Format { get; internal set; }

            protected Token(MText.Format format)
            {
                Format = format;
            }

            protected Token()
            {
            }
        }
    }
}
