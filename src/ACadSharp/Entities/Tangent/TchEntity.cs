using ACadSharp.Classes;
using ACadSharp.IO.DWG.Tangent;
using CSMath;
using System.Collections.Generic;

namespace ACadSharp.Entities.Tangent
{
	/// <summary>
	/// Base class for every entity that comes from a Tianzheng drawing.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Tianzheng is a Chinese architectural package built on ObjectARX. Its entities are ordinary
	/// custom objects, so the common entity data — layer, colour, handle — is read the usual way;
	/// what differs is everything the class writes for itself, which is obfuscated and uses a
	/// layout that is not documented anywhere.
	/// </para>
	/// <para>
	/// Because that reading process differs, the result is a distinct type rather than something
	/// reshaped into an existing one. A Tianzheng text carries what a <see cref="TextEntity"/>
	/// carries, but it was written by <c>TDbText::dwgInFields</c> and not by the <c>TEXT</c> reader,
	/// so it is a <see cref="TchText"/> and
	/// <see cref="TchText.ToTextEntity"/> is how it becomes one when that is what a caller wants.
	/// </para>
	/// <para>
	/// The base class exists so that the writers can leave all of these alone with a single check.
	/// ACadSharp cannot write a Tianzheng class correctly — it has no <c>dwgOutFields</c> for one —
	/// so they are skipped, and one case covers every class now and every class added later.
	/// </para>
	/// </remarks>
	public abstract class TchEntity : Entity
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.UNDEFINED;

		/// <summary>
		/// Gets the class this entity was read from.
		/// </summary>
		/// <remarks>
		/// The entity type says how the object was read; this says which Tianzheng class it is.
		/// Two objects of different classes share a type when they are read the same way, which is
		/// why the class is kept rather than encoded in the type.
		/// </remarks>
		public DxfClass DxfClass { get; }

		/// <summary>
		/// Gets the part of the object data that lies beyond the common entity data, as it is stored
		/// in the file.
		/// </summary>
		/// <remarks>
		/// The bytes written by the class's own <c>dwgOutFields</c>, kept verbatim. Only populated
		/// when the reader is configured to keep unknown entities.
		/// </remarks>
		public byte[] RawData { get; internal set; }

		/// <summary>
		/// Gets the property blocks the object carries, if it carries any.
		/// </summary>
		public TchDataMap Properties { get; internal set; }

		/// <inheritdoc/>
		public override string ObjectName => this.DxfClass?.DxfName ?? "TCH_UNKNOWN";

		/// <inheritdoc/>
		public override string SubclassMarker => this.DxfClass?.CppClassName ?? DxfSubclassMarker.Entity;

		internal TchEntity(DxfClass dxfClass)
		{
			this.DxfClass = dxfClass;
		}

		/// <summary>
		/// Converts this object into the standard entities it stands for.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Some Tianzheng classes store the shape they draw, and conversion is a direct mapping.
		/// Others store parameters from which the shape is derived — a wall keeps a centre line and
		/// a thickness and its two side lines are computed — and for those this needs the drawing
		/// logic of the class.
		/// </para>
		/// <para>
		/// Classes that have neither give an empty result rather than throwing, so a document can
		/// be converted without checking every object first.
		/// </para>
		/// </remarks>
		/// <returns>The standard entities, or empty when there is nothing to convert.</returns>
		public abstract IEnumerable<Entity> ToStandardEntities();
	}
}
