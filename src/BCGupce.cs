using BarcodeBakery.Common;
using System;
using System.Diagnostics;

namespace BarcodeBakery.Barcode
{
    /// <summary>
    /// UPC-E.
    /// You can provide a UPC-A code(without dash), the code will transform
    /// it into a UPC-E format if it's possible.
    /// UPC-E contains
    ///    - 1 system digits(not displayed but coded with parity)
    ///    - 6 digits
    ///    - 1 checksum digit(not displayed but coded with parity)
    ///
    /// The text returned is the UPC-E without the checksum.
    /// The checksum is always displayed.
    /// </summary>
    public class BCGupce : BCGBarcode1D
    {
        private readonly int[][][] codeParity;

        /// <summary>
        /// The UPCE value.
        /// </summary>
        protected string? upce;

        /// <summary>
        /// The label on the left.
        /// </summary>
        protected BCGLabel? labelLeft = null;

        /// <summary>
        /// The label on the center.
        /// </summary>
        protected BCGLabel? labelCenter = null;

        /// <summary>
        /// The label on the right.
        /// </summary>
        protected BCGLabel? labelRight = null;

        /// <summary>
        /// Creates a UPC-E barcode.
        /// </summary>
        public BCGupce()
            : base()
        {
            this.keys = new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };

            // Odd Parity starting with a space
            // Even Parity is the inverse (0=0012) starting with a space
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

            // Parity, 0=Odd, 1=Even for manufacturer code. Depending on 1st System Digit and Checksum
            this.codeParity = new int[][][] {
                new int[][] {
                    new int[] { 1, 1, 1, 0, 0, 0 },     /* 0,0 */
                    new int[] { 1, 1, 0, 1, 0, 0 },     /* 0,1 */
                    new int[] { 1, 1, 0, 0, 1, 0 },     /* 0,2 */
                    new int[] { 1, 1, 0, 0, 0, 1 },     /* 0,3 */
                    new int[] { 1, 0, 1, 1, 0, 0 },     /* 0,4 */
                    new int[] { 1, 0, 0, 1, 1, 0 },     /* 0,5 */
                    new int[] { 1, 0, 0, 0, 1, 1 },     /* 0,6 */
                    new int[] { 1, 0, 1, 0, 1, 0 },     /* 0,7 */
                    new int[] { 1, 0, 1, 0, 0, 1 },     /* 0,8 */
                    new int[] { 1, 0, 0, 1, 0, 1 }      /* 0,9 */
                },
                new int[][] {
                    new int[] { 0, 0, 0, 1, 1, 1 },     /* 0,0 */
                    new int[] { 0, 0, 1, 0, 1, 1 },     /* 0,1 */
                    new int[] { 0, 0, 1, 1, 0, 1 },     /* 0,2 */
                    new int[] { 0, 0, 1, 1, 1, 0 },     /* 0,3 */
                    new int[] { 0, 1, 0, 0, 1, 1 },     /* 0,4 */
                    new int[] { 0, 1, 1, 0, 0, 1 },     /* 0,5 */
                    new int[] { 0, 1, 1, 1, 0, 0 },     /* 0,6 */
                    new int[] { 0, 1, 0, 1, 0, 1 },     /* 0,7 */
                    new int[] { 0, 1, 0, 1, 1, 0 },     /* 0,8 */
                    new int[] { 0, 1, 1, 0, 1, 0 }      /* 0,9 */
                }
            };
        }

        /// <summary>
        /// Draws the barcode.
        /// </summary>
        /// <param name="image">The surface.</param>
        public override void Draw(BCGSurface image)
        {
            this.CalculateChecksum();

            Debug.Assert(this.checksumValue != null);

            // Starting Code
            this.DrawChar(image, "000", true);
            var c = this.upce!.Length; // !It has been validated
            for (var i = 0; i < c; i++)
            {
                int.TryParse(this.text[0].ToString(), out var n1);
                this.DrawChar(image, BCGupce.Inverse(this.FindCode(this.upce[i])!, this.codeParity[n1][this.checksumValue[0]][i]), false);
            }

            // Draw Center Guard Bar
            this.DrawChar(image, "00000", false);

            // Draw Right Bar
            this.DrawChar(image, "0", true);
            this.text = this.text[0] + this.upce;
            this.DrawText(image, 0, 0, this.positionX, this.thickness);

            if (this.IsDefaultEanLabelEnabled())
            {
                Debug.Assert(this.labelCenter != null);

                var dimension = this.labelCenter.GetDimension();
                this.DrawExtendedBars(image, dimension[1] - 2);
            }
        }

        /// <summary>
        /// Returns the maximal size of a barcode.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <returns>An array, [0] being the width, [1] being the height.</returns>
        public override int[] GetDimension(int width, int height)
        {
            var startlength = 3;
            var centerlength = 5;
            var textlength = 6 * 7;
            var endlength = 1;

            width += startlength + centerlength + textlength + endlength;
            height += this.thickness;
            return base.GetDimension(width, height);
        }

        /// <summary>
        /// Adds the default label.
        /// </summary>
        protected override void AddDefaultLabel()
        {
            Debug.Assert(this.upce != null);

            if (this.IsDefaultEanLabelEnabled())
            {
                this.ProcessChecksum();

                Debug.Assert(this.checksumValue != null);

                var font = this.font;

                this.labelLeft = new BCGLabel(this.text.Substring(0, 1), font, BCGLabel.Position.Left, BCGLabel.Alignment.Bottom);
                var labelLeftDimension = this.labelLeft.GetDimension();
                this.labelLeft.SetSpacing(8);
                this.labelLeft.SetOffset(labelLeftDimension[1] / 2);

                this.labelCenter = new BCGLabel(this.upce, font, BCGLabel.Position.Bottom, BCGLabel.Alignment.Left);
                var labelCenterDimension = this.labelCenter.GetDimension();
                this.labelCenter.SetOffset((this.scale * 46 - labelCenterDimension[0]) / 2 + this.scale * 2);

                this.labelRight = new BCGLabel(this.keys[this.checksumValue[0]], font, BCGLabel.Position.Right, BCGLabel.Alignment.Bottom);
                var labelRightDimension = this.labelRight.GetDimension();
                this.labelRight.SetSpacing(8);
                this.labelRight.SetOffset(labelRightDimension[1] / 2);

                this.AddLabel(this.labelLeft);
                this.AddLabel(this.labelCenter);
                this.AddLabel(this.labelRight);
            }
        }

        /// <summary>
        /// Checks if the default ean label is enabled.
        /// </summary>
        /// <returns>True if default label is enabled.</returns>
        protected virtual bool IsDefaultEanLabelEnabled()
        {
            var label = this.GetLabel();
            var font = this.font;
            return !string.IsNullOrEmpty(label) && font != null && this.defaultLabel != null;
        }

        /// <summary>
        /// Validates the input.
        /// </summary>
        protected override void Validate()
        {
            var c = this.text.Length;
            if (c == 0)
            {
                throw new BCGParseException("upce", "No data has been entered.");
            }

            // Checking if all chars are allowed
            for (var i = 0; i < c; i++)
            {
                if (ArraySearch(this.text[i], this.keys) == -1)
                {
                    throw new BCGParseException("upce", "The character '" + this.text[i] + "' is not allowed.");
                }
            }

            // Must contain 11 chars
            // Must contain 6 chars (if starting with upce directly)
            // First Chars must be 0 or 1
            if (c != 11 && c != 6)
            {
                throw new BCGParseException("upce", "You must provide a UPC-A (11 characters) or a UPC-E (6 characters).");
            }
            else if (this.text[0] != '0' && this.text[0] != '1' && c != 6)
            {
                throw new BCGParseException("upce", "UPC-A must start with 0 or 1 to be converted to UPC-E.");
            }

            // Convert part
            this.upce = "";
            if (c != 6)
            {
                // Checking if UPC-A is convertible
                var temp1 = this.text.Substring(3, 3);
                if (temp1 == "000" || temp1 == "100" || temp1 == "200") // manufacturer code ends with 100, 200 or 300
                {
                    if (this.text.Substring(6, 2) == "00") // Product must start with 00
                    {
                        this.upce = this.text.Substring(1, 2) + this.text.Substring(8, 3) + this.text.Substring(3, 1);
                    }
                }
                else if (this.text.Substring(4, 2) == "00") // manufacturer code ends with 00
                {
                    if (this.text.Substring(6, 3) == "000") // Product must start with 000
                    {
                        this.upce = this.text.Substring(1, 3) + this.text.Substring(9, 2) + "3";
                    }
                }
                else if (this.text.Substring(5, 1) == "0") // manufacturer code ends with 0
                {
                    if (this.text.Substring(6, 4) == "0000") // Product must start with 0000
                    {
                        this.upce = this.text.Substring(1, 4) + this.text.Substring(10, 1) + "4";
                    }
                }
                else // No zero leading at manufacturer code
                {
                    int.TryParse(this.text.Substring(10, 1), out var temp2);
                    if (this.text.Substring(6, 4) == "0000" && temp2 >= 5 && temp2 <= 9) // Product must start with 0000 and must end by 5, 6, 7, 8 or 9
                    {
                        this.upce = this.text.Substring(1, 5) + this.text.Substring(10, 1);
                    }
                }
            }
            else
            {
                this.upce = this.text;
            }

            if (string.IsNullOrEmpty(this.upce))
            {
                throw new BCGParseException("upce", "Your UPC-A can't be converted to UPC-E.");
            }

            if (c == 6)
            {
                string upca;

                // We convert UPC-E to UPC-A to find the checksum
                if (this.text[5] == '0' || this.text[5] == '1' || this.text[5] == '2')
                {
                    upca = this.text.Substring(0, 2) + this.text[5] + "0000" + this.text.Substring(2, 3);
                }
                else if (this.text[5] == '3')
                {
                    upca = this.text.Substring(0, 3) + "00000" + this.text.Substring(3, 2);
                }
                else if (this.text[5] == '4')
                {
                    upca = this.text.Substring(0, 4) + "00000" + this.text[4];
                }
                else
                {
                    upca = this.text.Substring(0, 5) + "0000" + this.text[5];
                }

                this.text = "0" + upca;
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
            // Odd Position = 3, Even Position = 1
            // Multiply it by the number
            // Add all of that and do 10-(?mod10)
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
                    multiplier = 1;
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
        /// Draws the extended bar.
        /// </summary>
        /// <param name="image">The surface.</param>
        /// <param name="plus">How much more we should display the bars.</param>
        protected virtual void DrawExtendedBars(BCGSurface image, int plus)
        {
            int rememberX = this.positionX;
            int rememberH = this.thickness;

            // We increase the bars
            this.thickness += (int)(plus / (float)this.scale);
            this.positionX = 0;
            this.DrawSingleBar(image, BCGBarcode.COLOR_FG);
            this.positionX += 2;
            this.DrawSingleBar(image, BCGBarcode.COLOR_FG);

            // Last Bars
            this.positionX += 46;
            this.DrawSingleBar(image, BCGBarcode.COLOR_FG);
            this.positionX += 2;
            this.DrawSingleBar(image, BCGBarcode.COLOR_FG);

            this.positionX = rememberX;
            this.thickness = rememberH;
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