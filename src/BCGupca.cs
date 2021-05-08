using BarcodeBakery.Common;
using System.Diagnostics;

namespace BarcodeBakery.Barcode
{
    /// <summary>
    /// UPC-A.
    /// UPC-A contains
    ///    - 2 system digits(1 not provided, a 0 is added automatically)
    ///    - 5 manufacturer code digits
    ///    - 5 product digits
    ///    - 1 checksum digit
    ///
    /// The checksum is always displayed.
    /// </summary>
    public class BCGupca : BCGean13
    {
        /// <summary>
        /// The label on the right.
        /// </summary>
        protected BCGLabel? labelRight = null;

        /// <summary>
        /// Creates a UPC-A barcode.
        /// </summary>
        public BCGupca()
            : base()
        {
        }

        /// <summary>
        /// Draws the barcode.
        /// </summary>
        /// <param name="image">The surface.</param>
        public override void Draw(BCGSurface image)
        {
            // The following code is exactly the same as EAN13. We just add a 0 in front of the code !
            this.text = "0" + this.text; // We will remove it at the end... don't worry

            base.Draw(image);

            // We remove the 0 in front, as we said :)
            this.text = this.text.Substring(1);
        }

        /// <summary>
        /// Draws the extended bar.
        /// </summary>
        /// <param name="image">The surface.</param>
        /// <param name="plus">How much more we should display the bars.</param>
        protected override void DrawExtendedBars(BCGSurface image, int plus)
        {
            Debug.Assert(this.checksumValue != null);

            string tempText = this.text + this.keys[this.checksumValue[0]];
            int rememberX = this.positionX;
            int rememberH = this.thickness;

            // We increase the bars
            // First 2 Bars
            this.thickness += (int)(plus / (float)this.scale);
            this.positionX = 0;
            this.DrawSingleBar(image, BCGBarcode.COLOR_FG);
            this.positionX += 2;
            this.DrawSingleBar(image, BCGBarcode.COLOR_FG);

            // Attemping to increase the 2 following bars
            this.positionX += 1;
            string tempValue = this.FindCode(tempText[1])!; // !It has been validated
            this.DrawChar(image, tempValue, false);

            // Center Guard Bar
            this.positionX += 36;
            this.DrawSingleBar(image, BCGBarcode.COLOR_FG);
            this.positionX += 2;
            this.DrawSingleBar(image, BCGBarcode.COLOR_FG);

            // Attemping to increase the 2 last bars
            this.positionX += 37;
            tempValue = this.FindCode(tempText[12])!; // !It has been validated
            this.DrawChar(image, tempValue, true);

            // Completly last bars
            this.DrawSingleBar(image, BCGBarcode.COLOR_FG);
            this.positionX += 2;
            this.DrawSingleBar(image, BCGBarcode.COLOR_FG);

            this.positionX = rememberX;
            this.thickness = rememberH;
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

                this.labelLeft = new BCGLabel(label.Substring(0, 1), font, BCGLabel.Position.Left, BCGLabel.Alignment.Bottom);
                this.labelLeft.SetSpacing(4 * this.scale);

                this.labelCenter1 = new BCGLabel(label.Substring(1, 5), font, BCGLabel.Position.Bottom, BCGLabel.Alignment.Left);
                var labelCenter1Dimension = this.labelCenter1.GetDimension();
                this.labelCenter1.SetOffset((this.scale * 44 - labelCenter1Dimension[0]) / 2 + this.scale * 6);

                this.labelCenter2 = new BCGLabel(label.Substring(6, 5), font, BCGLabel.Position.Bottom, BCGLabel.Alignment.Left);
                this.labelCenter2.SetOffset((this.scale * 44 - labelCenter1Dimension[0]) / 2 + this.scale * 45);

                this.labelRight = new BCGLabel(this.keys[this.checksumValue[0]], font, BCGLabel.Position.Right, BCGLabel.Alignment.Bottom);
                this.labelRight.SetSpacing(4 * this.scale);

                if (this.alignLabel)
                {
                    var labelDimension = this.labelCenter1.GetDimension();
                    this.labelLeft.SetOffset(labelDimension[1]);
                    this.labelRight.SetOffset(labelDimension[1]);
                }
                else
                {
                    var labelDimension = this.labelLeft.GetDimension();
                    this.labelLeft.SetOffset(labelDimension[1] / 2);
                    labelDimension = this.labelLeft.GetDimension();
                    this.labelRight.SetOffset(labelDimension[1] / 2);
                }

                this.AddLabel(this.labelLeft);
                this.AddLabel(this.labelCenter1);
                this.AddLabel(this.labelCenter2);
                this.AddLabel(this.labelRight);
            }
        }

        /// <summary>
        /// Check correct length.
        /// </summary>
        protected override void CheckCorrectLength()
        {
            // If we have 12 chars, just flush the last one without throwing anything
            var c = this.text.Length;
            if (c == 12)
            {
                this.text = this.text.Substring(0, 11);
            }
            else if (c != 11)
            {
                throw new BCGParseException("upca", "Must contain 11 digits, the 12th digit is automatically added.");
            }
        }
    }
}