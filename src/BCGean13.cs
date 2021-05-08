using BarcodeBakery.Common;
using System;
using System.Diagnostics;

namespace BarcodeBakery.Barcode
{
    /// <summary>
    /// EAN-13.
    /// </summary>
    public class BCGean13 : BCGBarcode1D
    {
        /// <summary>
        /// Holds the data about the code parity.
        /// </summary>
        protected readonly int[][] codeParity;

        /// <summary>
        /// The label on the left.
        /// </summary>
        protected BCGLabel? labelLeft = null;

        /// <summary>
        /// The label on the left center.
        /// </summary>
        protected BCGLabel? labelCenter1 = null;

        /// <summary>
        /// The label on the right center.
        /// </summary>
        protected BCGLabel? labelCenter2 = null;

        /// <summary>
        /// Indicates if we align the default labels.
        /// </summary>
        protected bool alignLabel;

        /// <summary>
        /// Creates a EAN-13 barcode.
        /// </summary>
        public BCGean13()
            : base()
        {
            this.keys = new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };

            // Left-Hand Odd Parity starting with a space
            // Left-Hand Even Parity is the inverse (0=0012) starting with a space
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

            // Parity, 0=Odd, 1=Even for manufacturer code. Depending on 1st System Digit
            this.codeParity = new int[][] {
                new int[] { 0, 0, 0, 0, 0 },     /* 0 */
                new int[] { 0, 1, 0, 1, 1 },     /* 1 */
                new int[] { 0, 1, 1, 0, 1 },     /* 2 */
                new int[] { 0, 1, 1, 1, 0 },     /* 3 */
                new int[] { 1, 0, 0, 1, 1 },     /* 4 */
                new int[] { 1, 1, 0, 0, 1 },     /* 5 */
                new int[] { 1, 1, 1, 0, 0 },     /* 6 */
                new int[] { 1, 0, 1, 0, 1 },     /* 7 */
                new int[] { 1, 0, 1, 1, 0 },     /* 8 */
                new int[] { 1, 1, 0, 1, 0 }      /* 9 */
            };

            this.AlignDefaultLabel(true);
        }

        /// <summary>
        /// Aligns the default label.
        /// </summary>
        /// <param name="align">Aligns the label.</param>
        public void AlignDefaultLabel(bool align)
        {
            this.alignLabel = align;
        }

        /// <summary>
        /// Draws the barcode.
        /// </summary>
        /// <param name="image">The surface.</param>
        public override void Draw(BCGSurface image)
        {
            this.DrawBars(image);
            this.DrawText(image, 0, 0, this.positionX, this.thickness);

            if (this.IsDefaultEanLabelEnabled())
            {
                Debug.Assert(this.labelCenter1 != null);

                var dimension = this.labelCenter1.GetDimension();
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
            var textlength = 12 * 7;
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

                var label = this.GetLabel()!; // !Validated in IsDefaultEanLabelEnabled
                var font = this.font;

                this.labelLeft = new BCGLabel(label.Substring(0, 1), font, BCGLabel.Position.Left, BCGLabel.Alignment.Bottom);
                this.labelLeft.SetSpacing(4 * this.scale);

                this.labelCenter1 = new BCGLabel(label.Substring(1, 6), font, BCGLabel.Position.Bottom, BCGLabel.Alignment.Left);
                var labelCenter1Dimension = this.labelCenter1.GetDimension();
                this.labelCenter1.SetOffset((this.scale * 44 - labelCenter1Dimension[0]) / 2 + this.scale * 2);

                this.labelCenter2 = new BCGLabel(label.Substring(7, 5) + this.keys[this.checksumValue[0]], font, BCGLabel.Position.Bottom, BCGLabel.Alignment.Left);
                this.labelCenter2.SetOffset((this.scale * 44 - labelCenter1Dimension[0]) / 2 + this.scale * 48);

                if (this.alignLabel)
                {
                    var labelDimension = this.labelCenter1.GetDimension();
                    this.labelLeft.SetOffset(labelDimension[1]);
                }
                else
                {
                    var labelDimension = this.labelLeft.GetDimension();
                    this.labelLeft.SetOffset(labelDimension[1] / 2);
                }

                this.AddLabel(this.labelLeft);
                this.AddLabel(this.labelCenter1);
                this.AddLabel(this.labelCenter2);
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
                throw new BCGParseException("ean13", "No data has been entered.");
            }

            this.CheckCharsAllowed();
            this.CheckCorrectLength();

            base.Validate();
        }

        /// <summary>
        /// Check chars allowed.
        /// </summary>
        protected virtual void CheckCharsAllowed()
        {
            // Checking if all chars are allowed
            var c = this.text.Length;
            for (var i = 0; i < c; i++)
            {
                if (ArraySearch(this.text[i], this.keys) == -1)
                {
                    throw new BCGParseException("ean13", "The character '" + this.text[i] + "' is not allowed.");
                }
            }
        }

        /// <summary>
        /// Check correct length.
        /// </summary>
        protected virtual void CheckCorrectLength()
        {
            // If we have 13 chars, just flush the last one without throwing anything
            var c = this.text.Length;
            if (c == 13)
            {
                this.text = this.text.Substring(0, 12);
            }
            else if (c != 12)
            {
                throw new BCGParseException("ean13", "Must contain 12 digits, the 13th digit is automatically added.");
            }
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
        /// Draws the bars.
        /// </summary>
        /// <param name="image">The surface.</param>
        protected void DrawBars(BCGSurface image)
        {
            // Checksum
            this.CalculateChecksum();
            Debug.Assert(this.checksumValue != null);

            var tempText = this.text + this.keys[this.checksumValue[0]];

            // Starting Code
            this.DrawChar(image, "000", true);

            // Draw Second Code
            this.DrawChar(image, this.FindCode(tempText[1])!, false); // !It has been validated

            // Draw Manufacturer Code
            for (var i = 0; i < 5; i++)
            {
                int.TryParse(tempText[0].ToString(), out var n1);
                this.DrawChar(image, BCGean13.Inverse(this.FindCode(tempText[i + 2])!, this.codeParity[n1][i]), false); // !It has been validated
            }

            // Draw Center Guard Bar
            this.DrawChar(image, "00000", false);

            // Draw Product Code
            for (var i = 7; i < 13; i++)
            {
                this.DrawChar(image, this.FindCode(tempText[i])!, true); // !It has been validated
            }

            // Draw Right Guard Bar
            this.DrawChar(image, "000", true);
        }

        /// <summary>
        /// Draws the extended bar.
        /// </summary>
        /// <param name="image">The surface.</param>
        /// <param name="plus">How much more we should display the bars.</param>
        protected virtual void DrawExtendedBars(BCGSurface image, int plus)
        {
            var rememberX = this.positionX;
            var rememberH = this.thickness;

            // We increase the bars
            this.thickness += (int)(plus / (float)this.scale);
            this.positionX = 0;
            this.DrawSingleBar(image, BCGBarcode.COLOR_FG);
            this.positionX += 2;
            this.DrawSingleBar(image, BCGBarcode.COLOR_FG);

            // Center Guard Bar
            this.positionX += 44;
            this.DrawSingleBar(image, BCGBarcode.COLOR_FG);
            this.positionX += 2;
            this.DrawSingleBar(image, BCGBarcode.COLOR_FG);

            // Last Bars
            this.positionX += 44;
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