using ACadSharp.Objects;
using ACadSharp.Tables;

namespace ACadSharp.Entities
{
	public interface IEntity : IHandledCadObject
	{
		/// <summary>
		/// Specifies the layer for an object.
		/// </summary>
		Layer Layer { get; set; }

		/// <summary>
		/// The True Color object of the object.
		/// </summary>
		/// <remarks>
		/// This property is used to change an object's color. Colors are identified by an AcCmColor object.
		/// This object can hold an RGB value, an ACI number (an integer from 1 to 255), or a named color.
		/// Using an RGB value, you can choose from millions of colors.
		/// </remarks>
		Color Color { get; set; }

		/// <summary>
		/// Specifies the lineweight of an individual object or the default lineweight for the drawing.
		/// </summary>
		LineweightType LineWeight { get; set; }

		/// <summary>
		/// Linetype scale for this entity.
		/// </summary>
		/// <remarks>
		/// This must be a positive, non-negative number.
		/// </remarks>
		double LinetypeScale { get; set; }

		/// <summary>
		/// Specifies the visibility of an object or the application.
		/// </summary>
		/// <remarks>
		/// If you specify an object to be invisible, it will be invisible regardless of the application 
		/// visible setting. Other factors can also cause an object to be invisible; 
		/// for example, an object will not be displayed if its layer is off or frozen.
		/// </remarks>
		bool IsInvisible { get; set; }

		/// <summary>
		/// Transparency value.
		/// </summary>
		Transparency Transparency { get; set; }

		/// <summary>
		/// Linetype name (present if not BYLAYER). 
		/// The special name BYBLOCK indicates a floating linetype (optional)
		/// </summary>
		LineType LineType { get; set; }

		/// <summary>
		/// Material object (present if not BYLAYER)
		/// </summary>
		Material Material { get; set; }

		/// <summary>
		/// Match entity properties to another entity
		/// </summary>
		/// <param name="entity"></param>
		void MatchProperties(IEntity entity);
	}
}
