using BarcodeBakery.Common;
using System.Diagnostics;

namespace BarcodeBakery.Barcode
{
    /// <summary>
    /// EAN-8.
    /// </summary>
    public class BCGean8 : BCGBarcode1D
    {
        /// <summary>
        /// The label on the left.
        /// </summary>
        protected BCGLabel? labelLeft = null;

        /// <summary>
        /// The label on the right.
        /// </summary>
        protected BCGLabel? labelRight = null;

        /// <summary>
        /// Creates a EAN-8 barcode.
        /// </summary>
        public BCGean8()
            : base()
        {
            this.keys = new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };

            // Left-Hand Odd Parity starting with a space
            // Right-Hand is the same of Left-Hand starting with a bar
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

            var tempText = this.text + this.keys[this.checksumValue[0]];

            // Starting Code
            this.DrawChar(image, "000", true);

            // Draw First 4 Chars (Left-Hand)
            for (var i = 0; i < 4; i++)
            {
                this.DrawChar(image, this.FindCode(tempText[i])!, false); // !It has been validated
            }

            // Draw Center Guard Bar
            this.DrawChar(image, "00000", false);

            // Draw Last 4 Chars (Right-Hand)
            for (var i = 4; i < 8; i++)
            {
                this.DrawChar(image, this.FindCode(tempText[i])!, true); // !It has been validated
            }

            // Draw Right Guard Bar
            this.DrawChar(image, "000", true);
            this.DrawText(image, 0, 0, this.positionX, this.thickness);

            if (this.IsDefaultEanLabelEnabled())
            {
                Debug.Assert(this.labelRight != null);

                var dimension = this.labelRight.GetDimension();
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
            var textlength = 8 * 7;
            var endlength = 3;

            width += startlength + centerlength + textlength + endlength;
            height += this.thickness;
            return base.GetDimension(width, height);
        }

        /// <summary>
        /// Adds the default label.
        /// </summary>
        protected override void AddDefaultLabel()
        {
            if (this.IsDefaultEanLabelEnabled())
            {
                this.ProcessChecksum();

                Debug.Assert(this.checksumValue != null);

                var label = this.GetLabel();
                Debug.Assert(label != null);
                var font = this.font;

                this.labelLeft = new BCGLabel(label.Substring(0, 4), font, BCGLabel.Position.Bottom, BCGLabel.Alignment.Left);
                var labelLeftDimension = this.labelLeft.GetDimension();
                this.labelLeft.SetOffset((this.scale * 30 - labelLeftDimension[0]) / 2 + this.scale * 2);

                this.labelRight = new BCGLabel(label.Substring(4, 3) + this.keys[this.checksumValue[0]], font, BCGLabel.Position.Bottom, BCGLabel.Alignment.Left);
                var labelRightDimension = this.labelRight.GetDimension();
                this.labelRight.SetOffset((this.scale * 30 - labelRightDimension[0]) / 2 + this.scale * 34);

                this.AddLabel(this.labelLeft);
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
                throw new BCGParseException("ean8", "No data has been entered.");
            }

            // Checking if all chars are allowed
            for (var i = 0; i < c; i++)
            {
                if (ArraySearch(this.text[i], this.keys) == -1)
                {
                    throw new BCGParseException("ean8", "The character '" + this.text[i] + "' is not allowed.");
                }
            }

            // If we have 8 chars just flush the last one
            if (c == 8)
            {
                this.text = this.text.Substring(0, 7);
            }
            else if (c != 7)
            {
                throw new BCGParseException("ean8", "Must contain 7 digits, the 8th digit is automatically added.");
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

            // Center Guard Bar
            this.positionX += 30;
            this.DrawSingleBar(image, BCGBarcode.COLOR_FG);
            this.positionX += 2;
            this.DrawSingleBar(image, BCGBarcode.COLOR_FG);

            // Last Bars
            this.positionX += 30;
            this.DrawSingleBar(image, BCGBarcode.COLOR_FG);
            this.positionX += 2;
            this.DrawSingleBar(image, BCGBarcode.COLOR_FG);

            this.positionX = rememberX;
            this.thickness = rememberH;
        }
    }
}
