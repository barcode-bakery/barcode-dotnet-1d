using BarcodeBakery.Common;
using System;
using System.Diagnostics;

namespace BarcodeBakery.Barcode
{
    /// <summary>
    /// Code 11.
    /// </summary>
    public class BCGcode11 : BCGBarcode1D
    {
        /// <summary>
        /// Creates a Code 11 barcode.
        /// </summary>
        public BCGcode11()
            : base()
        {
            this.keys = new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "-" };
            this.code = new string[] { // 0 added to add an extra space
                "000010",     /* 0 */
                "100010",     /* 1 */
                "010010",     /* 2 */
                "110000",     /* 3 */
                "001010",     /* 4 */
                "101000",     /* 5 */
                "011000",     /* 6 */
                "000110",     /* 7 */
                "100100",     /* 8 */
                "100000",     /* 9 */
                "001000"      /* - */
            };
        }

        /// <summary>
        /// Draws the barcode.
        /// </summary>
        /// <param name="image">The surface.</param>
        public override void Draw(BCGSurface image)
        {
            // Starting Code
            this.DrawChar(image, "001100", true);

            // Chars
            var c = this.text.Length;
            for (var i = 0; i < c; i++)
            {
                this.DrawChar(image, this.FindCode(this.text[i])!); // !It has been validated, true);
            }

            // Checksum
            this.CalculateChecksum();

            Debug.Assert(this.checksumValue != null);

            c = this.checksumValue.Length;
            for (var i = 0; i < c; i++)
            {
                this.DrawChar(image, this.code[this.checksumValue[i]], true);
            }

            // Ending Code
            this.DrawChar(image, "00110", true);
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
            var startlength = 8;

            var textlength = 0;
            var c = this.text.Length;
            for (var i = 0; i < c; i++)
            {
                textlength += this.GetIndexLength(this.FindIndex(this.text[i]));
            }

            var checksumlength = 0;
            this.CalculateChecksum();

            Debug.Assert(this.checksumValue != null);

            c = this.checksumValue.Length;
            for (var i = 0; i < c; i++)
            {
                checksumlength += this.GetIndexLength(this.checksumValue[i]);
            }

            var endlength = 7;

            width += startlength + textlength + checksumlength + endlength;
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
                throw new BCGParseException("code11", "No data has been entered.");
            }

            // Checking if all chars are allowed
            for (var i = 0; i < c; i++)
            {
                if (ArraySearch(this.text[i], this.keys) == -1)
                {
                    throw new BCGParseException("code11", "The character '" + this.text[i] + "' is not allowed.");
                }
            }

            base.Validate();
        }

        /// <summary>
        /// Overloaded method to calculate checksum.
        /// </summary>
        protected override void CalculateChecksum()
        {
            // Checksum
            // First CheckSUM "C"
            // The "C" checksum character is the modulo 11 remainder of the sum of the weighted
            // value of the data characters. The weighting value starts at "1" for the right-most
            // data character, 2 for the second to last, 3 for the third-to-last, and so on up to 20.
            // After 10, the sequence wraps around back to 1.

            // Second CheckSUM "K"
            // Same as CheckSUM "C" but we count the CheckSum "C" at the end
            // After 9, the sequence wraps around back to 1.
            var sequenceMultiplier = new int[] { 10, 9 };
            var tempText = this.text;
            this.checksumValue = new int[2];
            for (var z = 0; z < 2; z++)
            {
                var c = tempText.Length;

                // We don't display the K CheckSum if the original text had a length less than 10
                if (c <= 10 && z == 1)
                {
                    break;
                }

                var checksum = 0;
                var i = c;
                var j = 0;
                for (; i > 0; i--, j++)
                {
                    var multiplier = i % sequenceMultiplier[z];
                    if (multiplier == 0)
                    {
                        multiplier = sequenceMultiplier[z];
                    }

                    checksum += this.FindIndex(tempText[j]) * multiplier;
                }

                this.checksumValue[z] = checksum % 11;
                tempText += this.keys[this.checksumValue[z]];
            }
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
                var ret = "";
                var c = this.checksumValue.Length;
                for (var i = 0; i < c; i++)
                {
                    ret += this.keys[this.checksumValue[i]];
                }

                return ret;
            }

            return null;
        }

        private static int SubstrCount(string haystack, string needle)
        {
            var count = 0;
            var pos = 0;
            while ((pos = haystack.IndexOf(needle, pos, StringComparison.Ordinal)) != -1)
            {
                pos += needle.Length;
                count++;
            }

            return count;
        }

        private int GetIndexLength(int index)
        {
            var length = 0;
            if (index != -1)
            {
                length += 6;
                length += SubstrCount(this.code[index], "1");
            }

            return length;
        }
    }
}