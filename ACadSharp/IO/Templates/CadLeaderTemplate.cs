using ACadSharp.Entities;

namespace ACadSharp.IO.Templates
{
	internal class CadLeaderTemplate : DwgEntityTemplate
	{
		public CadLeaderTemplate(Leader entity) : base(entity) { }

		public double Dimasz { get; internal set; }
		public ulong DIMSTYLEHandle { get; internal set; }
		public ulong AnnotationHandle { get; internal set; }
	}
}
