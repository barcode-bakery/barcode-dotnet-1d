using BarcodeBakery.Common;

namespace BarcodeBakery.Barcode
{
    /// <summary>
    /// Other Code.
    /// Starting with a bar and altern to space, bar, ...
    /// 0 is the smallest.
    /// </summary>
    public class BCGothercode : BCGBarcode1D
    {
        /// <summary>
        /// Creates an other type barcode.
        /// </summary>
        public BCGothercode()
            : base()
        {
            this.keys = new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
        }

        /// <summary>
        /// Draws the barcode.
        /// </summary>
        /// <param name="image">The surface.</param>
        public override void Draw(BCGSurface image)
        {
            this.DrawChar(image, this.text, true);
            this.DrawText(image, 0, 0, this.positionX, this.thickness);
        }

        /// <summary>
        /// Gets the label.
        /// If the label was set to <see cref="BCGBarcode1D.AUTO_LABEL"/>, the label will display the value from the text parsed.
        /// </summary>
        /// <returns>The label string.</returns>
        public override string? GetLabel()
        {
            var label = this.label;
            if (this.label == BCGBarcode1D.AUTO_LABEL)
            {
                label = "";
            }

            return label;
        }

        /// <summary>
        /// Returns the maximal size of a barcode.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <returns>An array, [0] being the width, [1] being the height.</returns>
        public override int[] GetDimension(int width, int height)
        {
            var array = this.text.ToCharArray();
            var textlength = array.Length;
            for (var i = 0; i < array.Length; i++)
            {
                int.TryParse(array[i].ToString(), out var n1);
                textlength += n1;
            }

            width += textlength;
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
                throw new BCGParseException("othercode", "No data has been entered.");
            }

            // Checking if all chars are allowed
            for (var i = 0; i < c; i++)
            {
                if (ArraySearch(this.text[i], this.keys) == -1)
                {
                    throw new BCGParseException("othercode", "The character '" + this.text[i] + "' is not allowed.");
                }
            }

            base.Validate();
        }
    }
}