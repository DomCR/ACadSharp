using ACadSharp.Objects;
using ACadSharp.Tables;
using CSMath;

namespace ACadSharp.Entities
{
	public interface IEntity : IHandledCadObject, IGeometricEntity
	{
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
		/// Specifies the visibility of an object or the application.
		/// </summary>
		/// <remarks>
		/// If you specify an object to be invisible, it will be invisible regardless of the application
		/// visible setting. Other factors can also cause an object to be invisible;
		/// for example, an object will not be displayed if its layer is off or frozen.
		/// </remarks>
		bool IsInvisible { get; set; }

		/// <summary>
		/// Specifies the layer for an object.
		/// </summary>
		Layer Layer { get; set; }

		/// <summary>
		/// Linetype name (present if not BYLAYER).
		/// The special name BYBLOCK indicates a floating linetype (optional)
		/// </summary>
		LineType LineType { get; set; }

		/// <summary>
		/// Linetype scale for this entity.
		/// </summary>
		/// <remarks>
		/// This must be a positive, non-negative number.
		/// </remarks>
		double LineTypeScale { get; set; }

		/// <summary>
		/// Specifies the lineweight of an individual object or the default lineweight for the drawing.
		/// </summary>
		LineWeightType LineWeight { get; set; }

		/// <summary>
		/// Material object (present if not BYLAYER)
		/// </summary>
		Material Material { get; set; }

		/// <summary>
		/// Transparency value.
		/// </summary>
		Transparency Transparency { get; set; }

		/// <summary>
		/// Get the active color for the entity, process the colors like <see cref="Color.ByBlock"/> and <see cref="Color.ByLayer"/>.
		/// </summary>
		/// <returns></returns>
		Color GetActiveColor();

		/// Get the active line type for the entity, process the line types like <see cref="LineType.ByBlock"/> and <see cref="LineType.ByLayer"/>.
		/// </summary>
		/// <returns></returns>
		LineType GetActiveLineType();

		/// <summary>
		/// Get the active line weight for the entity, process the line weights like <see cref="LineWeightType.ByBlock"/> and <see cref="LineWeightType.ByLayer"/>.
		/// </summary>
		/// <returns></returns>
		LineWeightType GetActiveLineWeightType();

		/// <summary>
		/// Match the properties of this entity with another entity.
		/// </summary>
		/// <param name="entity"></param>
		void MatchProperties(IEntity entity);
	}
}