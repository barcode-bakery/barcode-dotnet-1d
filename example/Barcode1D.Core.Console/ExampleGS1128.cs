using BarcodeBakery.Barcode;
using BarcodeBakery.Common;
using System;
using System.Threading.Tasks;

namespace Barcode1D.Core.Console
{
    public class ExampleGS1128
    {
        public static async Task CreateAsync(string filePath, string? text = null)
        {
            // Loading Font
            var font = new BCGFont("Arial", 18);

            // Don't forget to sanitize user inputs
            text = text?.Length > 0 ? text : "011234567891234";

            // The arguments are R, G, B for color.
            var colorBlack = new BCGColor(0, 0, 0);
            var colorWhite = new BCGColor(255, 255, 255);

            Exception? drawException = null;
            BCGBarcode? barcode = null;
            try
            {
                var code = new BCGgs1128();
                code.SetScale(2); // Resolution
                code.SetThickness(30); // Thickness
                code.SetForegroundColor(colorBlack); // Color of bars
                code.SetBackgroundColor(colorWhite); // Color of spaces
                code.SetFont(font); // Font

                // Installing the default GS1 from the BarcodeBakery.BarcodeGS1128 package.
                code.InstallDefaultApplicationIdentifiers();

                // You can use one of the following to encode the data.

                // Well defined AI with parentheses.
                ////code.Parse("(01)12345678912343(92)123~F1(15)880422");

                // No parentheses around the AI.
                ////code.Parse("011234567891234392123~F115880422");

                // A separated list of identifers with their content. Note, no ~F1 necessary.
                ////code.Parse(new Input[] { new Input("(01)12345678912343"), new Input("(92)123"), new Input("(15)880422") });

                // A mixed of entry from above.
                ////code.Parse(new Input[] { new Input("0112345678912343"), new Input("(92)123~F115880422") });

                // The preferred way if you know your AI and content, well separated.
                ////code.Parse(new Input[] { new Input("01", "12345678912343"), new Input("92", "123"), new Input("15", "880422") });

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
