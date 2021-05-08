using BarcodeBakery.Common;
using System.Diagnostics;
using System.Globalization;

namespace BarcodeBakery.Barcode
{
    /// <summary>
    /// MSI Plessey.
    /// </summary>
    public class BCGmsi : BCGBarcode1D
    {
        private int checksum;

        /// <summary>
        /// Creates a MSI Plessey barcode.
        /// </summary>
        public BCGmsi()
            : base()
        {
            this.keys = new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
            this.code = new string[] {
                "01010101",     /* 0 */
                "01010110",     /* 1 */
                "01011001",     /* 2 */
                "01011010",     /* 3 */
                "01100101",     /* 4 */
                "01100110",     /* 5 */
                "01101001",     /* 6 */
                "01101010",     /* 7 */
                "10010101",     /* 8 */
                "10010110"      /* 9 */
            };

            this.SetChecksum(0);
        }

        /// <summary>
        /// Sets how many checksums we display. 0 to 2.
        /// </summary>
        /// <param name="checksum">The amount of checksums.</param>
        public void SetChecksum(int checksum)
        {
            if (checksum < 0 && checksum > 2)
            {
                throw new BCGArgumentException("The checksum must be between 0 and 2 included.", nameof(checksum));
            }

            this.checksum = checksum;
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
            this.DrawChar(image, "10", true);

            // Chars
            var c = this.text.Length;
            for (var i = 0; i < c; i++)
            {
                this.DrawChar(image, this.FindCode(this.text[i])!, true); // !It has been validated
            }

            c = this.checksumValue.Length;
            for (var i = 0; i < c; i++)
            {
                this.DrawChar(image, this.FindCode(this.checksumValue[i].ToString(CultureInfo.InvariantCulture))!, true); // !It has been validated
            }

            // Ending Code
            this.DrawChar(image, "010", true);
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
            var textlength = 12 * this.text.Length;
            var startlength = 3;
            var checksumlength = this.checksum * 12;
            var endlength = 4;

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
                throw new BCGParseException("msi", "No data has been entered.");
            }

            // Checking if all chars are allowed
            for (var i = 0; i < c; i++)
            {
                if (ArraySearch(this.text[i], this.keys) == -1)
                {
                    throw new BCGParseException("msi", "The character '" + this.text[i] + "' is not allowed.");
                }
            }
        }

        /// <summary>
        /// Overloaded method to calculate checksum.
        /// </summary>
        protected override void CalculateChecksum()
        {
            // Forming a new number
            // If the original number is even, we take all even position
            // If the original number is odd, we take all odd position
            // 123456 = 246
            // 12345 = 135
            // Multiply by 2
            // Add up all the digit in the result (270 : 2+7+0)
            // Add up other digit not used.
            // 10 - (? Modulo 10). If result = 10, change to 0
            var lastText = this.text;
            this.checksumValue = new int[this.checksum];
            for (var i = 0; i < this.checksum; i++)
            {
                var newText = "";
                var newNumber = 0;
                var c = lastText.Length;
                int starting;
                if (c % 2 == 0)
                { // Even
                    starting = 1;
                }
                else
                {
                    starting = 0;
                }

                for (var j = starting; j < c; j += 2)
                {
                    newText += lastText[j];
                }

                int.TryParse(newText, out var n1);
                newText = ((int)(n1 * 2)).ToString(CultureInfo.InvariantCulture);
                var c2 = newText.Length;
                for (var j = 0; j < c2; j++)
                {
                    int.TryParse(newText[j].ToString(), out var n2);
                    newNumber += n2;
                }

                for (var j = (starting == 0) ? 1 : 0; j < c; j += 2)
                {
                    int.TryParse(lastText[j].ToString(), out var n3);
                    newNumber += n3;
                }

                newNumber = (10 - newNumber % 10) % 10;
                this.checksumValue[i] = newNumber;
                lastText += newNumber;
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
                string ret = "";
                int c = this.checksumValue.Length;
                for (int i = 0; i < c; i++)
                {
                    ret += this.keys[this.checksumValue[i]];
                }

                return ret;
            }

            return null;
        }
    }
}