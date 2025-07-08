using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ACadSharp.Attributes;


namespace ACadSharp.Objects.Evaluations {

	// No properties

	[DxfSubClass(DxfSubclassMarker.BlockGripLocationComponent)]
	internal class BlockGripLocationComponent : BlockGripExpression {

		// /// <inheritdoc/>
		// public override string ObjectName => DxfFileToken.??;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.BlockGripLocationComponent;
	}
}
