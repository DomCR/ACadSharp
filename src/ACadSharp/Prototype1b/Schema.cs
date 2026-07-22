namespace ACadSharp.Prototype1b
{
	public class Schema
    {
        public uint Index { get; set; }
        public string Name { get; set; }
        public ulong[] Indices { get; set; }
        // TODO: Find out what PropertyDescriptors are
        public SchemaProperty[] Properties { get; set; }
    }
}
