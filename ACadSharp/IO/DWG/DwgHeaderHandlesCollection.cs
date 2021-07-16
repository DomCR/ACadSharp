using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace ACadSharp.IO.DWG
{
	internal class DwgHeaderHandlesCollection
	{
		public ulong? CMATERIAL { get { return getHandle(); } set { setHandle(value: value); } }
		public ulong? CLAYER { get { return getHandle(); } set { setHandle(value: value); } }
		public ulong? TEXTSTYLE { get { return getHandle(); } set { setHandle(value: value); } }
		public ulong? CELTYPE { get { return getHandle(); } set { setHandle(value: value); } }
		public ulong? DIMSTYLE { get { return getHandle(); } set { setHandle(value: value); } }
		public ulong? CMLSTYLE { get { return getHandle(); } set { setHandle(value: value); } }
		public ulong? UCSNAME_PSPACE { get { return getHandle(); } set { setHandle(value: value); } }
		public ulong? UCSNAME_MSPACE { get { return getHandle(); } set { setHandle(value: value); } }
		public ulong? PUCSORTHOREF { get { return getHandle(); } set { setHandle(value: value); } }
		public ulong? PUCSBASE { get { return getHandle(); } set { setHandle(value: value); } }
		public ulong? UCSORTHOREF { get { return getHandle(); } set { setHandle(value: value); } }
		public ulong? DIMTXSTY { get { return getHandle(); } set { setHandle(value: value); } }
		public ulong? DIMLDRBLK { get { return getHandle(); } set { setHandle(value: value); } }
		public ulong? DIMBLK { get { return getHandle(); } set { setHandle(value: value); } }
		public ulong? DIMBLK1 { get { return getHandle(); } set { setHandle(value: value); } }
		public ulong? DIMBLK2 { get { return getHandle(); } set { setHandle(value: value); } }
		public ulong? DICTIONARY_LAYOUTS { get { return getHandle(); } set { setHandle(value: value); } }
		public ulong? DICTIONARY_PLOTSETTINGS { get { return getHandle(); } set { setHandle(value: value); } }
		public ulong? DICTIONARY_PLOTSTYLES { get { return getHandle(); } set { setHandle(value: value); } }
		public ulong? CPSNID { get { return getHandle(); } set { setHandle(value: value); } }
		public ulong? PAPER_SPACE { get { return getHandle(); } set { setHandle(value: value); } }
		public ulong? MODEL_SPACE { get { return getHandle(); } set { setHandle(value: value); } }
		public ulong? BYLAYER { get { return getHandle(); } set { setHandle(value: value); } }
		public ulong? BYBLOCK { get { return getHandle(); } set { setHandle(value: value); } }
		public ulong? CONTINUOUS { get { return getHandle(); } set { setHandle(value: value); } }
		public ulong? DIMLTYPE { get { return getHandle(); } set { setHandle(value: value); } }
		public ulong? DIMLTEX1 { get { return getHandle(); } set { setHandle(value: value); } }
		public ulong? DIMLTEX2 { get { return getHandle(); } set { setHandle(value: value); } }
		public ulong? VIEWPORT_ENTITY_HEADER_CONTROL_OBJECT { get { return getHandle(); } set { setHandle(value: value); } }
		public ulong? DICTIONARY_ACAD_GROUP { get { return getHandle(); } set { setHandle(value: value); } }
		public ulong? DICTIONARY_ACAD_MLINESTYLE { get { return getHandle(); } set { setHandle(value: value); } }
		public ulong? DICTIONARY_NAMED_OBJECTS { get { return getHandle(); } set { setHandle(value: value); } }
		public ulong? BLOCK_CONTROL_OBJECT { get { return getHandle(); } set { setHandle(value: value); } }
		public ulong? LAYER_CONTROL_OBJECT { get { return getHandle(); } set { setHandle(value: value); } }
		public ulong? STYLE_CONTROL_OBJECT { get { return getHandle(); } set { setHandle(value: value); } }
		public ulong? LINETYPE_CONTROL_OBJECT { get { return getHandle(); } set { setHandle(value: value); } }
		public ulong? VIEW_CONTROL_OBJECT { get { return getHandle(); } set { setHandle(value: value); } }
		public ulong? UCS_CONTROL_OBJECT { get { return getHandle(); } set { setHandle(value: value); } }
		public ulong? VPORT_CONTROL_OBJECT { get { return getHandle(); } set { setHandle(value: value); } }
		public ulong? APPID_CONTROL_OBJECT { get { return getHandle(); } set { setHandle(value: value); } }
		public ulong? DIMSTYLE_CONTROL_OBJECT { get { return getHandle(); } set { setHandle(value: value); } }
		public ulong? DICTIONARY_MATERIALS { get { return getHandle(); } set { setHandle(value: value); } }
		public ulong? DICTIONARY_COLORS { get { return getHandle(); } set { setHandle(value: value); } }
		public ulong? DICTIONARY_VISUALSTYLE { get { return getHandle(); } set { setHandle(value: value); } }
		public ulong? INTERFEREOBJVS { get { return getHandle(); } set { setHandle(value: value); } }
		public ulong? INTERFEREVPVS { get { return getHandle(); } set { setHandle(value: value); } }
		public ulong? DRAGVS { get { return getHandle(); } set { setHandle(value: value); } }
		public ulong? UCSBASE { get { return getHandle(); } set { setHandle(value: value); } }

		private Dictionary<string, ulong?> _handles = new Dictionary<string, ulong?>();

		public ulong? GetHandle(string name)
		{
			return _handles[name];
		}
		public void SetHandle(string name, ulong? value)
		{
			_handles[name] = value;
		}

		public List<ulong?> GetHandles()
		{
			return new List<ulong?>(_handles.Values);
		}

		private ulong? getHandle([CallerMemberName] string name = null)
		{
			return GetHandle(name);
		}
		private void setHandle([CallerMemberName] string name = null, ulong? value = 0)
		{
			SetHandle(name, value);
		}
	}
}
