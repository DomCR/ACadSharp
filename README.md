ACadSharp ![Build&Test](https://github.com/DomCr/ACadSharp/actions/workflows/build_n_test.yml/badge.svg) ![License](https://img.shields.io/github/license/DomCr/ACadSharp) ![nuget](https://img.shields.io/nuget/v/Acadsharp)
---

C# library to read/write cad files like dxf/dwg.

### Features

ACadSharp allows to read or create CAD files using .Net and also extract or modify existing content in the files, the main features may be listed as: 

- Read/Write Dxf binary files
- Read/Write Dxf ASCII files
- Read Dwg files
- Write Dwg files
- Extract/Modify the geometric information from the different [entities](https://help.autodesk.com/view/OARX/2021/ENU/?guid=GUID-7D07C886-FD1D-4A0C-A7AB-B4D21F18E484) in the model
- Control over the table elements like Blocks, Layers and Styles, allows you to read, create or modify the different tables

#### Compatible Dwg/Dxf versions:

|      | DxfReader | DxfWriter | DwgReader | DwgWriter |
------ | :-------: | :-------: | :-------: | :-------: |
AC1009 |    :x:    |   :x:     |    :x:    |    :x:    |
AC1012 |    :heavy_check_mark:    |   :heavy_check_mark:     |    :x:    |    :x:    |
AC1014 |    :heavy_check_mark:    |   :heavy_check_mark:     |    :heavy_check_mark:    |    :heavy_check_mark:    |
AC1015 |    :heavy_check_mark:    |   :heavy_check_mark:     |    :heavy_check_mark:    |    :heavy_check_mark:    |
AC1018 |    :heavy_check_mark:    |   :heavy_check_mark:     |    :heavy_check_mark:    |    :heavy_check_mark:    |
AC1021 |    :heavy_check_mark:    |   :heavy_check_mark:     |    :heavy_check_mark:    |    :x:                   |
AC1024 |    :heavy_check_mark:    |   :heavy_check_mark:     |    :heavy_check_mark:    |    :heavy_check_mark:    |
AC1027 |    :heavy_check_mark:    |   :heavy_check_mark:     |    :heavy_check_mark:    |    :x:                   |
AC1032 |    :heavy_check_mark:    |   :heavy_check_mark:     |    :heavy_check_mark:    |    :heavy_check_mark:    |

### Current development

#### Dwg Writer 

- **AC1014**
    - Produces a valid file but some entities are missing in the model.
- **AC1015**
    - This version depens on the implementation of VP_ENT_HDR to work properly with the different `Viewports`
- **AC1018** - **MOST STABLE - RECOMENDED**
- **AC1021** - **NOT IMPLEMENTED**
    - This is a particular and isolated DWG version, it uses a different compression system and file distribution, due this difficulties, this version will not be implemented any time soon.
- **AC1024** - **STABLE**
- **AC1027** - **NOT IMPLEMENTED**
- **AC1032** - **STABLE**

Code Example
---

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

For more examples [check](https://github.com/DomCR/ACadSharp/tree/master/ACadSharp.Examples).

Contributing
---

Please feel free to fork this repo and send a pull request if you want to contribute to this project.

Notice that this project is in an alpha version, not all the features are implemented and there can be bugs due to this so any PR with a bug fix will not have a priority.

If you want to contribute you can check the Dxf documentation [here](https://help.autodesk.com/view/OARX/2021/ENU/?guid=GUID-235B22E0-A567-4CF6-92D3-38A2306D73F3). 

License
---

This project is licensed under the MIT License - see the [LICENSE](https://github.com/DomCR/ACadSharp/blob/master/LICENSE) file for details