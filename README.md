## ACadSharp ![Build](https://img.shields.io/github/workflow/status/DomCr/ACadSharp/Build/master) ![Test](https://img.shields.io/github/workflow/status/DomCr/ACadSharp/Build&Test/master)

C# library to read/write cad files like dxf/dwg.

#### Compatible Dwg versions:

- [ ] DWG Release 1.1 
- [ ] DWG Release 1.2
- [ ] DWG Release 1.4
- [ ] DWG Release 2.0
- [ ] DWG Release 2.10
- [ ] AC1002 - DWG Release 2.5
- [ ] AC1003 - DWG Release 2.6
- [ ] AC1004 - DWG Release 9
- [ ] AC1006 - DWG Release 10
- [ ] AC1009 - DWG Release 11/12 (LT R1/R2)
- [x] AC1012 - DWG Release 14, 14.01 (LT97/LT98)
- [x] AC1014 - DWG Release 14, 14.01 (LT97/LT98)
- [x] AC1015 - DWG AutoCAD 2000/2000i/2002
- [x] AC1018 - DWG AutoCAD 2004/2005/2006
- [x] AC1021 - DWG AutoCAD 2007/2008/2009
- [x] AC1024 - DWG AutoCAD 2010/2011/2012
- [x] AC1027 - DWG AutoCAD 2013/2014/2015/2016/2017
- [x] AC1032 - DWG AutoCAD 2018/2019/2020

#### Code Example

```c#
public static void Main()
{
	string path = "sample.dwg";
	CadDocument doc = DwgReader.Read(path, onNotification);
}

// Process a notification form the reader
private static void onNotification(object sender, NotificationEventArgs e)
{
	Console.WriteLine(e.Message);
}
```

### WIP

The dwg reader is not yet fully implemented, the NotificationHandler will send a message to inform about the objects that could not be readed.

##### Dwg entities not implemented

- ACDBPLACEHOLDER
- VP_ENT_HDR
- DIMENSION_ANG_3_Pt
- DIMENSION_ANG_2_Ln
- DIMENSION_RADIUS
- DIMENSION_DIAMETER
- DIMENSION_ORDINATE
- DIMENSION_ALIGNED
- FACE3D
- LWPOLYLINE
- POLYLINE_PFACE
- SPLINE
- XLINE
- LEADER
- HATCH
- SHAPE
- LWPOLYLINE
- ACAD_PROXY_OBJECT
- SEQEND
- XRECORD

##### Dxf

- DxfReader
- DxfWriter

##### Long term 

- DwgWriter