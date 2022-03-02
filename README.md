## ACadSharp ![Build](https://img.shields.io/github/workflow/status/DomCr/ACadSharp/Build/master) ![Build&Test](https://github.com/DomCr/ACadSharp/actions/workflows/build_n_test.yml/badge.svg) ![License](https://img.shields.io/github/license/DomCr/ACadSharp)

C# library to read/write cad files like dxf/dwg.

#### Compatible Dwg/Dxf versions:

- [ ] Release 1.1 
- [ ] Release 1.2
- [ ] Release 1.4
- [ ] Release 2.0
- [ ] Release 2.10
- [ ] AC1002 - Release 2.5
- [ ] AC1003 - Release 2.6
- [ ] AC1004 - Release 9
- [ ] AC1006 - Release 10
- [ ] AC1009 - Release 11/12 (LT R1/R2)
- [x] AC1012 - Release 14, 14.01 (LT97/LT98)
- [x] AC1014 - Release 14, 14.01 (LT97/LT98)
- [x] AC1015 - AutoCAD 2000/2000i/2002
- [x] AC1018 - AutoCAD 2004/2005/2006
- [x] AC1021 - AutoCAD 2007/2008/2009
- [x] AC1024 - AutoCAD 2010/2011/2012
- [x] AC1027 - AutoCAD 2013/2014/2015/2016/2017
- [x] AC1032 - AutoCAD 2018/2019/2020

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

WIP
---

The dwg/dxf readers are not yet fully implemented, the NotificationHandler will send a message to inform about the objects that could not be readed or any other error in the process.

##### Document integration

- Set tests to check the final document integrity

##### Dwg entities not implemented

- ACDBPLACEHOLDER
- VP_ENT_HDR
- LWPOLYLINE
- POLYLINE_PFACE
- LWPOLYLINE
- ACAD_PROXY_OBJECT
- VIEW


##### Long term 

- DxfWriter
- DwgWriter

Contributing
------------

Please feel free to fork this repo and send a pull request if you want to contribute to this project.

Notice that this project is in an alpha version, not all the features are implemented and there can be bugs due to this so any PR with a bug fix will not have a priority.

If you want to contribute you can check the Dxf documentation [here](https://help.autodesk.com/view/OARX/2021/ENU/?guid=GUID-235B22E0-A567-4CF6-92D3-38A2306D73F3). 