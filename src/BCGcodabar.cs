using BarcodeBakery.Common;
using System;
using System.Globalization;

namespace BarcodeBakery.Barcode
{
    /// <summary>
    /// Codabar.
    /// </summary>
    public class BCGcodabar : BCGBarcode1D
    {
        /// <summary>
        /// Creates a Codabar barcode.
        /// </summary>
        public BCGcodabar()
            : base()
        {
            this.keys = new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "-", "$", ":", "/", ".", "+", "A", "B", "C", "D" };
            this.code = new string[] {
                "00000110",     /* 0 */
                "00001100",     /* 1 */
                "00010010",     /* 2 */
                "11000000",     /* 3 */
                "00100100",     /* 4 */
                "10000100",     /* 5 */
                "01000010",     /* 6 */
                "01001000",     /* 7 */
                "01100000",     /* 8 */
                "10010000",     /* 9 */
                "00011000",     /* - */
                "00110000",     /* $ */
                "10001010",     /* : */
                "10100010",     /* / */
                "10101000",     /* . */
                "00111110",     /* + */
                "00110100",     /* A */
                "01010010",     /* B */
                "00010110",     /* C */
                "00011100"      /* D */
            };
        }

        /// <summary>
        /// Parses the text before displaying it.
        /// </summary>
        /// <param name="text">The text.</param>
        public override void Parse(string text)
        {
            base.Parse(text.ToUpper(CultureInfo.CurrentCulture));    // Only Capital Letters are Allowed
        }

        /// <summary>
        /// Draws the barcode.
        /// </summary>
        /// <param name="image">The surface.</param>
        public override void Draw(BCGSurface image)
        {
            var c = this.text.Length;
            for (var i = 0; i < c; i++)
            {
                this.DrawChar(image, this.FindCode(this.text[i])!, true); // !It has been validated
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
            var textLength = 0;
            var c = this.text.Length;
            for (var i = 0; i < c; i++)
            {
                var index = this.FindIndex(this.text[i]);
                if (index != -1)
                {
                    textLength += 8;
                    textLength += SubstrCount(this.code[index], "1");
                }
            }

            width += textLength;
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
                throw new BCGParseException("codabar", "No data has been entered.");
            }

            // Checking if all chars are allowed
            for (var i = 0; i < c; i++)
            {
                if (ArraySearch(this.text[i], this.keys) == -1)
                {
                    throw new BCGParseException("codabar", "The character '" + this.text[i] + "' is not allowed.");
                }
            }

            // Must start by A, B, C or D
            if (c == 0 || (this.text[0] != 'A' && this.text[0] != 'B' && this.text[0] != 'C' && this.text[0] != 'D'))
            {
                throw new BCGParseException("codabar", "The text must start by the character A, B, C, or D.");
            }

            // Must end by A, B, C or D
            var c2 = c - 1;
            if (c2 == 0 || (this.text[c2] != 'A' && this.text[c2] != 'B' && this.text[c2] != 'C' && this.text[c2] != 'D'))
            {
                throw new BCGParseException("codabar", "The text must end by the character A, B, C, or D.");
            }

            base.Validate();
        }

        private static int SubstrCount(string haystack, string needle)
        {
            int count = 0;
            int pos = 0;
            while ((pos = haystack.IndexOf(needle, pos, StringComparison.CurrentCulture)) != -1)
            {
                pos += needle.Length;
                count++;
            }

            return count;
        }
    }
}