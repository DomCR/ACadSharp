using ACadSharp.Entities;
using ACadSharp.Tables;
using ACadSharp.XData;
using System.Collections.Generic;
using System.Linq;

namespace ACadSharp.Objects;

/// <summary>
/// Reads and writes the annotative state of an object the way AutoCAD stores it: an <see cref="AppName"/> extended data
/// record that flags the object as annotative, and, in the object's extension dictionary, a
/// <see cref="ContextDataManagerName"/> dictionary holding an <see cref="AnnotationScalesName"/> dictionary with one
/// <see cref="AnnotScaleObjectContextData"/> per annotation scale (named <c>*A1</c>, <c>*A2</c>, ...).
/// </summary>
public static class AnnotativeData
{
	/// <summary>
	/// The application name of the extended data that flags an object as annotative.
	/// </summary>
	public const string AppName = "AcadAnnotative";

	/// <summary>
	/// The extension dictionary entry that holds the object's context data.
	/// </summary>
	public const string ContextDataManagerName = "AcDbContextDataManager";

	/// <summary>
	/// The context data manager entry that holds one context data per annotation scale.
	/// </summary>
	public const string AnnotationScalesName = "ACDB_ANNOTATIONSCALES";

	private const string _dataName = "AnnotativeData";

	/// <summary>
	/// Adds <paramref name="contextData"/> to <paramref name="owner"/>'s annotation scales, creating the extension
	/// dictionary entries when they are missing, under the next free <c>*A</c> name.
	/// </summary>
	/// <returns>The name the context data was added under.</returns>
	public static string AddContextData(CadObject owner, AnnotScaleObjectContextData contextData)
	{
		CadDictionary scales = getScales(owner, create: true);

		int i = 1;
		while (scales.ContainsKey($"*A{i}"))
		{
			i++;
		}

		string name = $"*A{i}";
		// The writers name a dictionary entry by the object's own name.
		contextData.Name = name;
		scales.Add(contextData);
		return name;
	}

	/// <summary>
	/// The context data of <paramref name="owner"/>, one per annotation scale; empty when it has none.
	/// </summary>
	public static IEnumerable<AnnotScaleObjectContextData> GetContextData(CadObject owner)
	{
		CadDictionary scales = getScales(owner, create: false);
		if (scales == null)
		{
			return Enumerable.Empty<AnnotScaleObjectContextData>();
		}

		return scales.OfType<AnnotScaleObjectContextData>().ToList();
	}

	/// <summary>
	/// True when <paramref name="entity"/> carries the <see cref="AppName"/> extended data with its flag set.
	/// </summary>
	public static bool IsAnnotative(Entity entity)
	{
		if (!entity.ExtendedData.TryGet(AppName, out ExtendedData data))
		{
			return false;
		}

		ExtendedDataInteger16 flag = data.Records.OfType<ExtendedDataInteger16>().LastOrDefault();
		return flag != null && flag.Value == 1;
	}

	/// <summary>
	/// Flags <paramref name="entity"/> as annotative (or not) with the <see cref="AppName"/> extended data, registering
	/// the application in the document's <see cref="AppId"/> table when it is missing. The entity must belong to a
	/// document.
	/// </summary>
	public static void SetAnnotative(Entity entity, bool annotative)
	{
		CadDocument doc = entity.Document ?? throw new System.InvalidOperationException("The entity must belong to a document.");
		if (!doc.AppIds.TryGetValue(AppName, out AppId app))
		{
			app = new AppId(AppName);
			doc.AppIds.Add(app);
		}

		ExtendedDataRecord[] records =
		{
			new ExtendedDataString(_dataName),
			new ExtendedDataControlString(false),
			new ExtendedDataInteger16(1),
			new ExtendedDataInteger16((short)(annotative ? 1 : 0)),
			new ExtendedDataControlString(true),
		};
		if (entity.ExtendedData.TryGet(app, out ExtendedData existing))
		{
			existing.Records.Clear();
			existing.Records.AddRange(records);
		}
		else
		{
			entity.ExtendedData.Add(app, new ExtendedData(records));
		}
	}

	private static CadDictionary getScales(CadObject owner, bool create)
	{
		CadDictionary xdictionary = owner.XDictionary;
		if (xdictionary == null)
		{
			if (!create)
			{
				return null;
			}

			xdictionary = owner.CreateExtendedDictionary();
		}

		return child(child(xdictionary, ContextDataManagerName, create), AnnotationScalesName, create);
	}

	private static CadDictionary child(CadDictionary parent, string name, bool create)
	{
		if (parent == null)
		{
			return null;
		}

		if (parent.TryGetEntry(name, out CadDictionary found))
		{
			return found;
		}

		if (!create)
		{
			return null;
		}

		CadDictionary made = new CadDictionary(name);
		parent.Add(made);
		return made;
	}
}
