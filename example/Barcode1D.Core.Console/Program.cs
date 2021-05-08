using System.Threading.Tasks;

namespace Barcode1D.Core.Console
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // See each barcode file to see how you can save to a file or a MemoryStream.
            await ExampleCodabar.CreateAsync("barcode_codabar.png");
            await ExampleCode11.CreateAsync("barcode_code11.png");
            await ExampleCode128.CreateAsync("barcode_code128.png");
            await ExampleCode39.CreateAsync("barcode_code39.png");
            await ExampleCode39extended.CreateAsync("barcode_code39extended.png");
            await ExampleCode93.CreateAsync("barcode_code93.png");
            await ExampleEan13.CreateAsync("barcode_ean13.png");
            await ExampleEan8.CreateAsync("barcode_ean8.png");
            await ExampleGS1128.CreateAsync("barcode_gs1128.png");
            await ExampleI25.CreateAsync("barcode_i25.png");
            await ExampleIsbn.CreateAsync("barcode_isbn.png");
            await ExampleMsi.CreateAsync("barcode_msi.png");
            await ExampleOtherCode.CreateAsync("barcode_othercode.png");
            await ExamplePostnet.CreateAsync("barcode_postnet.png");
            await ExampleS25.CreateAsync("barcode_s25.png");
            await ExampleUpca.CreateAsync("barcode_upca.png");
            await ExampleUpce.CreateAsync("barcode_upce.png");
            await ExampleUpcext2.CreateAsync("barcode_upcext2.png");
            await ExampleUpcext5.CreateAsync("barcode_upcext5.png");
        }
    }
}
