# CadWriter

## DwgWriter

### Write a file with a thumbnail

To store a file with a preview image, thumbnail, to visaulize the content in you can set the property `DwgWriter.Preview` with the information needed.

```C#
string image = "path to your image";    //Accepted formats bmp, wmf, png
DwgPreview preview = new DwgPreview(DwgPreview.PreviewType.Png, new byte[80], File.ReadAllBytes(image));

string path =  "path to store the dwg document";
CadDocument doc = new CadDocument();    //Your document
doc.Header.Version = version;

using (var wr = new DwgWriter(path, doc))
{
    wr.Preview = preview;
    wr.Write();
}
```

`ACadSharp` doesn't provide a direct way to generate a preivew image from a `CadDocument`. To create a preview image check [ACadSharp.Pdf](https://github.com/DomCR/ACadSharp.Pdf).