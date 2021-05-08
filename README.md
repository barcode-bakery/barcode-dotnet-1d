<p align="center"><a href="https://www.barcodebakery.com" target="_blank">
    <img src="https://www.barcodebakery.com/images/BCG-Logo-SQ-GitHub.svg">
</a></p>

[Barcode Bakery][1] is library written in .NET Standard, [PHP][32] and Node.JS which allows you to generate barcodes on the fly on your server for displaying or saving.

The library has minimal dependencies in each language in order to be supported on a wide variety of web servers.

The library is available for free for non-commercial use; however you must [purchase a license][2] if you plan to use it in a commercial environment.

Installation
------------
There are two ways to install our library:

* With PowerShell, get the file from NuGet, run the following command:
```
PM> Install-Package BarcodeBakery.Barcode1D
```
* Or, download the library on our [website][3], and follow our [developer's guide][4].

Requirements
------------
* .NET Standard 2.0
* .NET Core 2.0+
* .NET Framework 4.6.1+

Example usages
--------------
For a full example of how to use each symbology type, visit our [API page][5].

### Saving a Code 128 to a `MemoryStream`
```csharp
public static async Task CreateAsync(string filePath, string? text = null)
{
    // Loading Font
    var font = new BCGFont("Arial", 18);

    // Don't forget to sanitize user inputs
    text = text?.Length > 0 ? text : "a123";

    // The arguments are R, G, B for color.
    var colorBlack = new BCGColor(0, 0, 0);
    var colorWhite = new BCGColor(255, 255, 255);

    Exception? drawException = null;
    BCGBarcode? barcode = null;
    try
    {
        var code = new BCGcode128();
        code.SetScale(2); // Resolution
        code.SetThickness(30); // Thickness
        code.SetForegroundColor(colorBlack); // Color of bars
        code.SetBackgroundColor(colorWhite); // Color of spaces
        code.SetFont(font); // Font
        code.SetStart(null);
        code.SetTilde(true);
        code.Parse(text); // Text
        barcode = code;
    }
    catch (Exception exception)
    {
        drawException = exception;
    }

    var drawing = new BCGDrawing(barcode, colorWhite);
    if (drawException != null)
    {
        drawing.DrawException(drawException);
    }

    // Saves the barcode into a MemoryStream
    var memoryStream = new System.IO.MemoryStream();
    await drawing.FinishAsync(BCGDrawing.ImageFormat.Png, memoryStream);
}
```

### Saving the image to a file
Replace the last line of the previous code with the following:
```csharp
// Saves the barcode into a file.
await drawing.FinishAsync(BCGDrawing.ImageFormat.Png, filePath);
```

This will generate the following:
<br />
<img src="https://www.barcodebakery.com/images/code-128-github.png">

Supported types
---------------
* [Codabar][6]
* [Code 11][7]
* [Code 128][8]
* [Code 39][9]
* [Code 39 Extended][10]
* [Code 93][11]
* [EAN-13][12]
* [EAN-8][13]
* [GS1-128 (EAN-128)][14]
* [Interleaved 2 of 5][16]
* [ISBN-10 / ISBN-13][17]
* [MSI Plessey][18]
* [Other (Custom)][19]
* [Postnet][20]
* [Standard 2 of 5][21]
* [UPC Extension 2][22]
* [UPC Extension 5][23]
* [UPC-A][24]
* [UPC-E][25]

Other libraries available for purchase
--------------------------------------
* [Aztec][26]
* [Databar Expanded][27]
* [DataMatrix][28]
* [MaxiCode][29]
* [PDF417][30]
* [QRCode][31]


[1]: https://www.barcodebakery.com
[2]: https://www.barcodebakery.com/en/purchase
[3]: https://www.barcodebakery.com/en/download
[4]: https://www.barcodebakery.com/en/docs/dotnet/guide
[5]: https://www.barcodebakery.com/en/docs/dotnet/barcode/1d
[6]: https://www.barcodebakery.com/en/docs/dotnet/barcode/codabar/api
[7]: https://www.barcodebakery.com/en/docs/dotnet/barcode/code11/api
[8]: https://www.barcodebakery.com/en/docs/dotnet/barcode/code128/api
[9]: https://www.barcodebakery.com/en/docs/dotnet/barcode/code39/api
[10]: https://www.barcodebakery.com/en/docs/dotnet/barcode/code39extended/api
[11]: https://www.barcodebakery.com/en/docs/dotnet/barcode/code93/api
[12]: https://www.barcodebakery.com/en/docs/dotnet/barcode/ean13/api
[13]: https://www.barcodebakery.com/en/docs/dotnet/barcode/ean8/api
[14]: https://www.barcodebakery.com/en/docs/dotnet/barcode/gs1128/api
[16]: https://www.barcodebakery.com/en/docs/dotnet/barcode/i25/api
[17]: https://www.barcodebakery.com/en/docs/dotnet/barcode/isbn/api
[18]: https://www.barcodebakery.com/en/docs/dotnet/barcode/msi/api
[19]: https://www.barcodebakery.com/en/docs/dotnet/barcode/othercode/api
[20]: https://www.barcodebakery.com/en/docs/dotnet/barcode/postnet/api
[21]: https://www.barcodebakery.com/en/docs/dotnet/barcode/s25/api
[22]: https://www.barcodebakery.com/en/docs/dotnet/barcode/upcext2/api
[23]: https://www.barcodebakery.com/en/docs/dotnet/barcode/upcext5/api
[24]: https://www.barcodebakery.com/en/docs/dotnet/barcode/upca/api
[25]: https://www.barcodebakery.com/en/docs/dotnet/barcode/upce/api
[26]: https://www.barcodebakery.com/en/docs/dotnet/barcode/aztec/api
[27]: https://www.barcodebakery.com/en/docs/dotnet/barcode/databarexpanded/api
[28]: https://www.barcodebakery.com/en/docs/dotnet/barcode/datamatrix/api
[29]: https://www.barcodebakery.com/en/docs/dotnet/barcode/maxicode/api
[30]: https://www.barcodebakery.com/en/docs/dotnet/barcode/pdf417/api
[31]: https://www.barcodebakery.com/en/docs/dotnet/barcode/qrcode/api
[32]: https://github.com/barcode-bakery/barcode-php-1d/
