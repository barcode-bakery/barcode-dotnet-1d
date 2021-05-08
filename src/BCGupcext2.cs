using BarcodeBakery.Common;
using System;

namespace BarcodeBakery.Barcode
{
    /// <summary>
    /// UPC Supplemental Barcode 2 digits.
    ///
    /// Working with UPC-A, UPC-E, EAN-13, EAN-8
    /// This includes 2 digits(normaly for publications)
    /// Must be placed next to UPC or EAN Code
    /// </summary>
    public class BCGupcext2 : BCGBarcode1D
    {
        private readonly int[][] codeParity;

        /// <summary>
        /// Creates a UPC supplemental 2 digits barcode.
        /// </summary>
        public BCGupcext2()
            : base()
        {
            this.keys = new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
            this.code = new string[] {
                "2100",     /* 0 */
                "1110",     /* 1 */
                "1011",     /* 2 */
                "0300",     /* 3 */
                "0021",     /* 4 */
                "0120",     /* 5 */
                "0003",     /* 6 */
                "0201",     /* 7 */
                "0102",     /* 8 */
                "2001"      /* 9 */
            };

            // Parity, 0=Odd, 1=Even. Depending on ?%4
            this.codeParity = new int[][] {
                new int[] { 0, 0 },     /* 0 */
                new int[] { 0, 1 },     /* 1 */
                new int[] { 1, 0 },     /* 2 */
                new int[] { 1, 1 }      /* 3 */
            };
        }

        /// <summary>
        /// Draws the barcode.
        /// </summary>
        /// <param name="image">The surface.</param>
        public override void Draw(BCGSurface image)
        {
            // Starting Code
            this.DrawChar(image, "001", true);

            // Code
            for (var i = 0; i < 2; i++)
            {
                int.TryParse(this.text, out var n1);
                this.DrawChar(image, BCGupcext2.Inverse(this.FindCode(this.text[i])!, this.codeParity[n1 % 4][i]), false); // !It has been validated
                if (i == 0)
                {
                    this.DrawChar(image, "00", false);    // Inter-char
                }
            }

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
            var startlength = 4;
            var textlength = 2 * 7;
            var intercharlength = 2;

            width += startlength + textlength + intercharlength;
            height += this.thickness;
            return base.GetDimension(width, height);
        }

        /// <summary>
        /// Adds the default label.
        /// </summary>
        protected override void AddDefaultLabel()
        {
            base.AddDefaultLabel();

            if (this.defaultLabel != null)
            {
                this.defaultLabel.SetPosition(BCGLabel.Position.Top);
            }
        }

        /// <summary>
        /// Validates the input.
        /// </summary>
        protected override void Validate()
        {
            var c = this.text.Length;
            if (c == 0)
            {
                throw new BCGParseException("upcext2", "No data has been entered.");
            }

            // Checking if all chars are allowed
            for (var i = 0; i < c; i++)
            {
                if (ArraySearch(this.text[i], this.keys) == -1)
                {
                    throw new BCGParseException("upcext2", "The character '" + this.text[i] + "' is not allowed.");
                }
            }

            // Must contain 2 digits
            if (c != 2)
            {
                throw new BCGParseException("upcext2", "Must contain 2 digits.");
            }

            base.Validate();
        }

        /// <summary>
        /// Inverses the string when the inverse parameter is equal to 1.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="inverse">The inverse.</param>
        /// <returns>The reversed string.</returns>
        private static string Inverse(string text, int inverse)
        {
            if (inverse == 1)
            {
                char[] reversedChars = text.ToCharArray();
                Array.Reverse(reversedChars);
                text = new string(reversedChars);
            }

            return text;
        }
    }
}