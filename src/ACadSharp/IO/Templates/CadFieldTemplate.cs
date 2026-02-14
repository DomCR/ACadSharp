using ACadSharp.Objects;
using System.Collections.Generic;

namespace ACadSharp.IO.Templates;

internal class CadFieldTemplate : CadTemplate<Field>
{
	public List<ulong> CadObjectsHandles { get; set; } = new();

	public List<ulong> ChildrenHandles { get; set; } = new();

	public CadFieldTemplate(Field obj) : base(obj)
	{
	}
}