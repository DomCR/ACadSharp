using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ACadSharp.Attributes;
using ACadSharp.Entities;

using CSMath;


namespace ACadSharp.Objects.Evaluations {

	//70	BS	0
	//71	BS	21
	//330	H	7F891
	//330	H	7F890
	//330	H	7F88F
	//330	H	7F88E
	//330	H	7F88D
	//330	H	7F88C
	//330	H	7F88B
	//330	H	7F88A
	//330	H	7F889
	//330	H	7F888
	//330	H	7F887
	//330	H	7F886
	//330	H	7F885
	//330	H	7F884
	//330	H	7F883
	//330	H	7F882
	//330	H	7F881
	//330	H	7F880
	//330	H	7F87F
	//330	H	7F87E
	//330	H	7F892
	//1010	BD	14.25497409889494
	//1020	BD	36.54781144995404
	//1030	BD	0.0

	[DxfSubClass(DxfSubclassMarker.BlockAction)]
	public abstract class BlockAction : BlockElement {

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.BlockAction;

		/// <summary>
		/// 70	BS	0
		/// </summary>
		[DxfCodeValue(17)]
		public short Value70 { get; set; }


		/// <summary>
		/// Gets the list of <see cref="Entity"/> objects affected by this <see cref="BlockAction"/>.
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Count, 71)]
		[DxfCollectionCodeValue(DxfReferenceType.Handle, 330)]
		public List<Entity> Entities { get; } = new List<Entity>();


		/// <summary>
		/// Whatever this is?
		/// </summary>
		[DxfCodeValue(1010, 1020, 1030)]
		public XYZ ActionPoint { get; set; }
	}
}
