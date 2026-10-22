using ACadSharp.Entities;
using System;

namespace ACadSharp.IO.DXF.DxfStreamWriter;

/// <summary>
/// [PATCH] Public interface of the streaming DXF ENTITIES section writer (for the downstream consumer's single-pass streaming export).
///
/// The interface (rather than the concrete class <see cref="DxfEntitiesStreamWriter"/>) is made public: the concrete class inherits
/// from the internal base class DxfSectionWriterBase and cannot be public; DxfWriter.BeginStreamingEntities() returns this interface,
/// and callers only depend on the interface.
///
/// Usage (low-memory single-pass export):
///   var streamer = dxfWriter.BeginStreamingEntities();   // preprocessing + create main output + create ENTITIES temp file
///   // during conversion, for each model space entity built (with Owner already set to *Model_Space):
///   streamer.WriteEntityOne(entity);                     // group codes written straight to the temp file; the entity can be GC'd immediately, never lands in doc memory
///   streamer.Finish();                                    // writes the ENTITIES section footer
///   dxfWriter.EndStreamingEntities();                    // assembles sections in order and deletes the temp file
/// </summary>
public interface IDxfEntitiesStreamWriter : IDisposable
{
	/// <summary>Number of model space entities streamed out so far (excluding in-block entities).</summary>
	long EntityCount { get; }

	/// <summary>Temp file path (the ENTITIES section group-code stream; deleted by DxfWriter after EndStreamingEntities).</summary>
	string TempPath { get; }

	/// <summary>
	/// Streams out one model space entity: allocates handles -> writes group codes to the temp file -> the entity can be dropped.
	/// The caller must have already set entity.Owner to *Model_Space (required by group code 330).
	/// </summary>
	void WriteEntityOne(Entity entity);

	/// <summary>Finalization: writes the ENTITIES section footer (0 ENDSEC) and Flushes; WriteEntityOne cannot be called afterwards.</summary>
	void Finish();
}
