using BarcodeBakery.Common;
using System;
using System.Diagnostics;

namespace BarcodeBakery.Barcode
{
    /// <summary>
    /// UPC Supplemental Barcode 5 digits.
    ///
    /// Working with UPC-A, UPC-E, EAN-13, EAN-8
    /// This includes 5 digits(normaly for suggested retail price)
    /// Must be placed next to UPC or EAN Code
    /// If 90000 -> No suggested Retail Price
    /// If 99991 -> Book Complimentary(normally free)
    /// If 90001 to 98999 -> Internal Purpose of Publisher
    /// If 99990 -> Used by the National Association of College Stores to mark used books
    /// If 0xxxx -> Price Expressed in British Pounds(xx.xx)
    /// If 5xxxx -> Price Expressed in U.S.dollars(US$xx.xx)
    /// </summary>
    public class BCGupcext5 : BCGBarcode1D
    {
        private readonly int[][] codeParity;

        /// <summary>
        /// Creates a UPC supplemental 5 digits barcode.
        /// </summary>
        public BCGupcext5()
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

            // Parity, 0=Odd, 1=Even. Depending Checksum
            this.codeParity = new int[][] {
                new int[] { 1, 1, 0, 0, 0 },     /* 0 */
                new int[] { 1, 0, 1, 0, 0 },     /* 1 */
                new int[] { 1, 0, 0, 1, 0 },     /* 2 */
                new int[] { 1, 0, 0, 0, 1 },     /* 3 */
                new int[] { 0, 1, 1, 0, 0 },     /* 4 */
                new int[] { 0, 0, 1, 1, 0 },     /* 5 */
                new int[] { 0, 0, 0, 1, 1 },     /* 6 */
                new int[] { 0, 1, 0, 1, 0 },     /* 7 */
                new int[] { 0, 1, 0, 0, 1 },     /* 8 */
                new int[] { 0, 0, 1, 0, 1 }      /* 9 */
            };
        }

        /// <summary>
        /// Draws the barcode.
        /// </summary>
        /// <param name="image">The surface.</param>
        public override void Draw(BCGSurface image)
        {
            // Checksum
            this.CalculateChecksum();

            Debug.Assert(this.checksumValue != null);

            // Starting Code
            this.DrawChar(image, "001", true);

            // Code
            for (var i = 0; i < 5; i++)
            {
                this.DrawChar(image, BCGupcext5.Inverse(this.FindCode(this.text[i])!, this.codeParity[this.checksumValue[0]][i]), false); // !It has been validated
                if (i < 4)
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
            var textlength = 5 * 7;
            var intercharlength = 2 * 4;

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
                throw new BCGParseException("upcext5", "No data has been entered.");
            }

            // Checking if all chars are allowed
            for (var i = 0; i < c; i++)
            {
                if (ArraySearch(this.text[i], this.keys) == -1)
                {
                    throw new BCGParseException("upcext5", "The character '" + this.text[i] + "' is not allowed.");
                }
            }

            // Must contain 5 digits
            if (c != 5)
            {
                throw new BCGParseException("upcext5", "Must contain 5 digits.");
            }

            base.Validate();
        }

        /// <summary>
        /// Overloaded method to calculate checksum.
        /// </summary>
        protected override void CalculateChecksum()
        {
            // Calculating Checksum
            // Consider the right-most digit of the message to be in an "odd" position,
            // and assign odd/even to each character moving from right to left
            // Odd Position = 3, Even Position = 9
            // Multiply it by the number
            // Add all of that and do ?mod10
            var odd = true;
            this.checksumValue = new int[] { 0 };
            var c = this.text.Length;
            for (var i = c; i > 0; i--)
            {
                int multiplier;
                if (odd == true)
                {
                    multiplier = 3;
                    odd = false;
                }
                else
                {
                    multiplier = 9;
                    odd = true;
                }

                if (ArraySearch(this.text[i - 1], this.keys) == -1)
                {
                    return;
                }

                int.TryParse(this.text[i - 1].ToString(), out var n1);
                int.TryParse(this.keys[n1], out var n2);
                this.checksumValue[0] += n2 * multiplier;
            }

            this.checksumValue[0] = this.checksumValue[0] % 10;
        }

        /// <summary>
        /// Overloaded method to display the checksum.
        /// </summary>
        /// <returns>The checksum value.</returns>
        protected override string? ProcessChecksum()
        {
            if (this.checksumValue == null) // Calculate the checksum only once
            {
                this.CalculateChecksum();
            }

            if (this.checksumValue != null)
            {
                return this.keys[this.checksumValue[0]];
            }

            return null;
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