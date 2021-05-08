using BarcodeBakery.Common;
using System.Diagnostics;

namespace BarcodeBakery.Barcode
{
    /// <summary>
    /// Standard 2 of 5.
    /// </summary>
    public class BCGs25 : BCGBarcode1D
    {
        private bool checksum;

        /// <summary>
        /// Creates a Standard 2 of 5 barcode.
        /// </summary>
        public BCGs25()
            : base()
        {
            this.keys = new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
            this.code = new string[] {
                "0000202000",     /* 0 */
                "2000000020",     /* 1 */
                "0020000020",     /* 2 */
                "2020000000",     /* 3 */
                "0000200020",     /* 4 */
                "2000200000",     /* 5 */
                "0020200000",     /* 6 */
                "0000002020",     /* 7 */
                "2000002000",     /* 8 */
                "0020002000"      /* 9 */
            };

            this.SetChecksum(false);
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
            this.DrawChar(image, "101000", true);

            // Chars
            var c = tempText.Length;
            for (var i = 0; i < c; i++)
            {
                this.DrawChar(image, this.FindCode(tempText[i])!, true); // !It has been validated
            }

            // Ending Code
            this.DrawChar(image, "10001", true);
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
            var startlength = 8;
            var textlength = c * 14;
            var checksumlength = 0;
            if (c % 2 != 0)
            {
                checksumlength = 14;
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
                throw new BCGParseException("s25", "No data has been entered.");
            }

            // Checking if all chars are allowed
            for (var i = 0; i < c; i++)
            {
                if (ArraySearch(this.text[i], this.keys) == -1)
                {
                    throw new BCGParseException("s25", "The character '" + this.text[i] + "' is not allowed.");
                }
            }

            // Must be even
            if (c % 2 != 0 && this.checksum == false)
            {
                throw new BCGParseException("s25", "s25 must contain an even amount of digits if checksum is false.");
            }
            else if (c % 2 == 0 && this.checksum == true)
            {
                throw new BCGParseException("s25", "s25 must contain an odd amount of digits if checksum is true.");
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
            this.checksumValue = new int[] { 0 };
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
    }
}