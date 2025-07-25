﻿using ACadSharp.Attributes;
using ACadSharp.Entities;
using ACadSharp.Tables;
using ACadSharp.Tests.Common;
using System;
using System.Linq;
using System.Reflection;
using Xunit;

namespace ACadSharp.Tests.Internal
{
	public class DxfMapTests
	{
		public static readonly TheoryData<Type> Types;

		static DxfMapTests()
		{
			Types = new TheoryData<Type>();

			var d = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.ManifestModule.Name == "ACadSharp.dll");

			foreach (var item in d.GetTypes().Where(i => !i.IsAbstract && i.IsPublic))
			{
				if (item.IsSubclassOf(typeof(Entity))
					|| item.IsSubclassOf(typeof(TableEntry)))
				{
					if (item == typeof(UnknownEntity) 
						|| item == typeof(PdfUnderlay))
					{
						continue;
					}

					Types.Add(item);
				}
			}
		}

		[Theory]
		[MemberData(nameof(Types))]
		public void CreateMapTest(Type t)
		{
			DxfNameAttribute att = t.GetCustomAttribute<DxfNameAttribute>();
			DxfSubClassAttribute subclass = t.GetCustomAttribute<DxfSubClassAttribute>();
			CadObject obj = Factory.CreateObject(t);

			Assert.NotNull(att);

			if (subclass != null && !obj.HasDynamicSubclass)
			{
				Assert.True(obj.SubclassMarker == subclass.ClassName);
			}

			DxfMap map = DxfMap.Create(t);
		}

		[Fact]
		public void TableEntryMapTest()
		{
			var map = DxfMap.Create<AppId>();

			Assert.True(map.SubClasses.ContainsKey(DxfSubclassMarker.TableRecord));
			Assert.True(map.SubClasses.ContainsKey(DxfSubclassMarker.ApplicationId));
		}

		[Fact]
		public void PolylineMapTest()
		{
			var map = DxfMap.Create<Polyline2D>();

			Assert.True(map.SubClasses.ContainsKey(DxfSubclassMarker.Entity));
			Assert.True(map.SubClasses.ContainsKey(DxfSubclassMarker.Polyline));
		}
	}
}
