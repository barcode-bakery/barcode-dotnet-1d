using BarcodeBakery.Common;
using System.Diagnostics;
using System.Globalization;

namespace BarcodeBakery.Barcode
{
    /// <summary>
    /// Interleaved 2 of 5.
    /// </summary>
    public class BCGi25 : BCGBarcode1D
    {
        private bool checksum;
        private int ratio;

        /// <summary>
        /// Creates a Interleaved 2 of 5 barcode.
        /// </summary>
        public BCGi25()
            : base()
        {
            this.keys = new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
            this.code = new string[] {
                "00110",     /* 0 */
                "10001",     /* 1 */
                "01001",     /* 2 */
                "11000",     /* 3 */
                "00101",     /* 4 */
                "10100",     /* 5 */
                "01100",     /* 6 */
                "00011",     /* 7 */
                "10010",     /* 8 */
                "01010"      /* 9 */
            };

            this.SetChecksum(false);
            this.SetRatio(2);
        }

        /// <summary>
        /// Sets if we display the checksum.
        /// </summary>
        /// <param name="checksum">Displays the checksum.</param>
        public void SetChecksum(bool checksum)
        {
            this.checksum = checksum;
        }

        /// <summary>
        /// Sets the ratio of the black bar compared to the white bars.
        /// </summary>
        /// <param name="ratio">The ratio.</param>
        public void SetRatio(int ratio)
        {
            this.ratio = ratio;
        }

        /// <summary>
        /// Draws the barcode.
        /// </summary>
        /// <param name="image">The surface.</param>
        public override void Draw(BCGSurface image)
        {
            var tempText = this.text;

            // Checksum
            if (this.checksum == true)
            {
                this.CalculateChecksum();

                Debug.Assert(this.checksumValue != null);

                tempText += this.keys[this.checksumValue[0]];
            }

            // Starting Code
            this.DrawChar(image, "0000", true);

            // Chars
            var c = tempText.Length;
            for (var i = 0; i < c; i += 2)
            {
                var tempBar = "";
                var c2 = this.FindCode(tempText[i])!.Length; // !It has been validated
                for (var j = 0; j < c2; j++)
                {
                    tempBar += this.FindCode(tempText[i])!.Substring(j, 1); // !It has been validated
                    tempBar += this.FindCode(tempText[i + 1])!.Substring(j, 1); // !It has been validated
                }

                this.DrawChar(image, this.ChangeBars(tempBar), true);
            }

            // Ending Code
            this.DrawChar(image, this.ChangeBars("100"), true);
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
            var textlength = (3 + (this.ratio + 1) * 2) * this.text.Length;
            var startlength = 4;
            var checksumlength = 0;
            if (this.checksum == true)
            {
                checksumlength = (3 + (this.ratio + 1) * 2);
            }

            var endlength = 2 + (this.ratio + 1);

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
                throw new BCGParseException("i25", "No data has been entered.");
            }

            // Checking if all chars are allowed
            for (var i = 0; i < c; i++)
            {
                if (ArraySearch(this.text[i], this.keys) == -1)
                {
                    throw new BCGParseException("i25", "The character '" + this.text[i] + "' is not allowed.");
                }
            }

            // Must be even
            if (c % 2 != 0 && this.checksum == false)
            {
                throw new BCGParseException("i25", "i25 must contain an even amount of digits if checksum is false.");
            }
            else if (c % 2 == 0 && this.checksum == true)
            {
                throw new BCGParseException("i25", "i25 must contain an odd amount of digits if checksum is true.");
            }

            base.Validate();
        }

        /// <summary>
        /// Overloaded method to calculate checksum.
        /// </summary>
        protected override void CalculateChecksum()
        {
            // Calculating Checksum
            // Consider the right-most digit of the message to be in an "even" position,
            // and assign odd/even to each character moving from right to left
            // Even Position = 3, Odd Position = 1
            // Multiply it by the number
            // Add all of that and do 10-(?mod10)
            var even = true;
            this.checksumValue = new int[1] { 0 };
            var c = this.text.Length;
            for (var i = c; i > 0; i--)
            {
                int multiplier;
                if (even == true)
                {
                    multiplier = 3;
                    even = false;
                }
                else
                {
                    multiplier = 1;
                    even = true;
                }

                int.TryParse(this.text[i - 1].ToString(), out var n1);
                int.TryParse(this.keys[n1], out var n2);
                this.checksumValue[0] += n2 * multiplier;
            }

            this.checksumValue[0] = (10 - this.checksumValue[0] % 10) % 10;
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
        /// Changes the size of the bars based on the ratio.
        /// </summary>
        /// <param name="bar">The bars.</param>
        /// <returns>New bars.</returns>
        private string ChangeBars(string bar)
        {
            if (this.ratio > 1)
            {
                bar = bar.Replace('1', this.ratio.ToString(CultureInfo.InvariantCulture)[0]);
            }

            return bar;
        }
    }
}