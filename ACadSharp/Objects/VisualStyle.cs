using ACadSharp.Attributes;
using System;

namespace ACadSharp.Objects
{
	/// <summary>
	/// Represents a <see cref="VisualStyle"/> object
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.ObjectVisualStyle"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.VisualStyle"/>
	/// </remarks>
	[DxfName(DxfFileToken.ObjectVisualStyle)]
	[DxfSubClass(DxfSubclassMarker.VisualStyle)]
	public class VisualStyle : CadObject
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.UNLISTED;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.ObjectVisualStyle;

		/// <summary>
		/// Description
		/// </summary>
		[DxfCodeValue(2)]
		public string Description { get; set; }

		//70	Type

		//71	Face lighting model

		//0 =Invisible

		//1 = Visible

		//2 = Phong

		//3 = Gooch

		//72

		//Face lighting quality

		//0 = No lighting

		//1 = Per face lighting

		//2 = Per vertex lighting

		//73

		//Face color mode

		//0 = No color

		//1 = Object color

		//2 = Background color

		//3 = Custom color

		//4 = Mono color

		//5 = Tinted

		//6 = Desaturated

		//90

		//Face modifiers

		//0 = No modifiers

		//1 = Opacity

		//2 = Specular

		//40

		//Face opacity level

		//41

		//Face specular level

		//62, 63

		//Color

		//421

		//Face style mono color

		//74

		//Edge style model

		//0 = No edges

		//1 = Isolines

		//2 = Facet edges

		//91

		//Edge style

		//64

		//Edge intersection color

		//65

		//Edge obscured color

		//75

		//Edge obscured linetype

		//175

		//Edge intersection linetype

		//42

		//Edge crease angle

		//92

		//Edge modifiers

		//66

		//Edge color

		//43

		//Edge opacity level

		//76

		//Edge width

		//77

		//Edge overhang

		//78

		//Edge jitter

		//67

		//Edge silhouette color

		//79

		//Edge silhouette width

		//170

		//Edge halo gap

		//171

		//Number of edge isolines

		//290

		//Edge hide precision flag

		//174

		//Edge style apply flag

		//93

		//Display style display settings

		//44

		//Brightness

		//173

		//Shadow type

		//291

		//Internal use only flag
	}
}
