using BarcodeBakery.Common;
using System.Globalization;

namespace BarcodeBakery.Barcode
{
    /// <summary>
    /// PostNet.
    /// A postnet is composed of either 5, 9 or 11 digits used by US postal service.
    /// </summary>
    public class BCGpostnet : BCGBarcode1D
    {
        /// <summary>
        /// Creates a PostNet barcode.
        /// </summary>
        public BCGpostnet()
            : base()
        {
            this.keys = new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
            this.code = new string[] {
                "11000",     /* 0 */
                "00011",     /* 1 */
                "00101",     /* 2 */
                "00110",     /* 3 */
                "01001",     /* 4 */
                "01010",     /* 5 */
                "01100",     /* 6 */
                "10001",     /* 7 */
                "10010",     /* 8 */
                "10100"      /* 9 */
            };

            this.SetThickness(9);
        }

        /// <summary>
        /// Draws the barcode.
        /// </summary>
        /// <param name="image">The surface.</param>
        public override void Draw(BCGSurface image)
        {
            // Checksum
            var checksum = 0;
            var c = this.text.Length;
            for (var i = 0; i < c; i++)
            {
                int.TryParse(this.text[i].ToString(), out var n1);
                checksum += n1;
            }

            checksum = 10 - (checksum % 10);

            // Starting Code
            this.DrawChar(image, "1");

            // Code
            for (var i = 0; i < c; i++)
            {
                this.DrawChar(image, this.FindCode(this.text[i])!); // !It has been validated
            }

            // Checksum
            this.DrawChar(image, this.FindCode(checksum.ToString(CultureInfo.InvariantCulture))!); // !It has been validated

            // Ending Code
            this.DrawChar(image, "1");
            this.DrawText(image, 0, 0, this.positionX, this.thickness);
        }

        /// <summary>
        /// Returns the maximal size of a barcode.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <returns>An array, [0] being the width, [1] being the height.</returns>
        public override int[] GetDimension(int width, int height)
        {
            var c = this.text.Length;
            var startlength = 3;
            var textlength = c * 5 * 3;
            var checksumlength = 5 * 3;
            var endlength = 3;

            // We remove the white on the right
            var removelength = -1.56;

            width += startlength + textlength + checksumlength + endlength + (int)removelength;
            height += this.thickness;
            return base.GetDimension(width, height);
        }

        /// <summary>
        /// Validates the input.
        /// </summary>
        protected override void Validate()
        {
            var c = this.text.Length;
            if (c == 0)
            {
                throw new BCGParseException("postnet", "No data has been entered.");
            }

            // Checking if all chars are allowed
            for (var i = 0; i < c; i++)
            {
                if (ArraySearch(this.text[i], this.keys) == -1)
                {
                    throw new BCGParseException("postnet", "The character '" + this.text[i] + "' is not allowed.");
                }
            }

            // Must contain 5, 9 or 11 chars
            if (c != 5 && c != 9 && c != 11)
            {
                throw new BCGParseException("postnet", "Must contain 5, 9, or 11 characters.");
            }

            base.Validate();
        }

        /// <summary>
        /// Overloaded method for drawing special barcode.
        /// </summary>
        /// <param name="image">The surface.</param>
        /// <param name="code">The code.</param>
        /// <param name="startBar">True if we begin with a space.</param>
        protected override void DrawChar(BCGSurface image, string code, bool startBar)
        {
            var c = code.Length;
            for (var i = 0; i < c; i++)
            {
                var posY = 0;
                if (code[i] == '0')
                {
                    posY = (int)(this.thickness - (this.thickness / 2.5));
                }

                this.DrawFilledRectangle(image, this.positionX, posY, (int)(this.positionX + 0.44), this.thickness - 1, BCGBarcode.COLOR_FG);
                this.positionX += 3;
            }
        }
    }
}