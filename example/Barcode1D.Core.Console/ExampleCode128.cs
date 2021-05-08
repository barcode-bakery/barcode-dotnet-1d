using BarcodeBakery.Barcode;
using BarcodeBakery.Common;
using System;
using System.Threading.Tasks;

namespace Barcode1D.Core.Console
{
    public class ExampleCode128
    {
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

            // Saves the barcode into a file.
            await drawing.FinishAsync(BCGDrawing.ImageFormat.Png, filePath);

            // Saves the barcode into a MemoryStream
            ////var memoryStream = new System.IO.MemoryStream();
            ////await drawing.FinishAsync(BCGDrawing.ImageFormat.Png, memoryStream);
        }
    }
}
