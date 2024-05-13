using ACadSharp.Classes;
using ACadSharp.Entities;

namespace ACadSharp.IO.Templates
{
	internal class CadUnknownEntity : CadEntityTemplate
	{
		public CadUnknownEntity(UnkownEntity entity) : base(entity)
		{
		}

		internal class UnkownEntity : Entity
		{
			public override ObjectType ObjectType => ObjectType.UNLISTED;

			public DxfClass DxfClass { get; }
		}
	}
}
