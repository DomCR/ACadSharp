using ACadSharp.Tables;

namespace ACadSharp.Entities
{
	public interface IEntity : IHandledCadObject
	{
		Layer Layer { get; set; }
		
		Color Color { get; set; }
		
		LineweightType LineWeight { get; set; }
		
		double LinetypeScale { get; set; }
		
		bool IsInvisible { get; set; }
		
		Transparency Transparency { get; set; }

		/// <summary>
		/// Linetype name (present if not BYLAYER). 
		/// The special name BYBLOCK indicates a floating linetype (optional)
		/// </summary>
		LineType LineType { get; set; }

		/// <summary>
		/// Match entity properties to another entity
		/// </summary>
		/// <param name="entity"></param>
		void MatchProperties(IEntity entity);
	}
}
