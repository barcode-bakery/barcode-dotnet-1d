using BarcodeBakery.Common;
using System.Diagnostics;
using System.Globalization;

namespace BarcodeBakery.Barcode
{
    /// <summary>
    /// Code 39.
    /// </summary>
    public class BCGcode39 : BCGBarcode1D
    {
        /// <summary>
        /// Code index for the starting character.
        /// </summary>
        protected int starting;

        /// <summary>
        /// Code index for the ending character.
        /// </summary>
        protected int ending;

        /// <summary>
        /// Indicates if we display the checksum.
        /// </summary>
        protected bool checksum;

        /// <summary>
        /// Creates a Code 39 barcode.
        /// </summary>
        public BCGcode39()
            : base()
        {
            this.starting = this.ending = 43;
            this.keys = new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", "-", ".", " ", "$", "/", "+", "%", "*" };
            this.code = new string[] { // 0 added to add an extra space
                "0001101000",     /* 0 */
                "1001000010",     /* 1 */
                "0011000010",     /* 2 */
                "1011000000",     /* 3 */
                "0001100010",     /* 4 */
                "1001100000",     /* 5 */
                "0011100000",     /* 6 */
                "0001001010",     /* 7 */
                "1001001000",     /* 8 */
                "0011001000",     /* 9 */
                "1000010010",     /* A */
                "0010010010",     /* B */
                "1010010000",     /* C */
                "0000110010",     /* D */
                "1000110000",     /* E */
                "0010110000",     /* F */
                "0000011010",     /* G */
                "1000011000",     /* H */
                "0010011000",     /* I */
                "0000111000",     /* J */
                "1000000110",     /* K */
                "0010000110",     /* L */
                "1010000100",     /* M */
                "0000100110",     /* N */
                "1000100100",     /* O */
                "0010100100",     /* P */
                "0000001110",     /* Q */
                "1000001100",     /* R */
                "0010001100",     /* S */
                "0000101100",     /* T */
                "1100000010",     /* U */
                "0110000010",     /* V */
                "1110000000",     /* W */
                "0100100010",     /* X */
                "1100100000",     /* Y */
                "0110100000",     /* Z */
                "0100001010",     /* - */
                "1100001000",     /* . */
                "0110001000",     /*   */
                "0101010000",     /* $ */
                "0101000100",     /* / */
                "0100010100",     /* + */
                "0001010100",     /* % */
                "0100101000"      /* * */
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
        /// Parses the text before displaying it.
        /// </summary>
        /// <param name="text">The text.</param>
        public override void Parse(string text)
        {
            base.Parse(text.ToUpper(CultureInfo.CurrentCulture));	// Only Capital Letters are Allowed
        }

        /// <summary>
        /// Draws the barcode.
        /// </summary>
        /// <param name="image">The surface.</param>
        public override void Draw(BCGSurface image)
        {
            // Starting *
            this.DrawChar(image, this.code[this.starting], true);

            // Chars
            var c = this.text.Length;
            for (var i = 0; i < c; i++)
            {
                this.DrawChar(image, this.FindCode(this.text[i])!, true); // !It has been validated
            }

            // Checksum (rarely used)
            if (this.checksum == true)
            {
                this.CalculateChecksum();

                Debug.Assert(this.checksumValue != null);

                this.DrawChar(image, this.code[this.checksumValue[0] % 43], true);
            }

            // Ending *
            this.DrawChar(image, this.code[this.ending], true);
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
            var textlength = 13 * this.text.Length;
            var startlength = 13;
            var checksumlength = 0;
            if (this.checksum == true)
            {
                checksumlength = 13;
            }

            var endlength = 13;

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
                throw new BCGParseException("code39", "No data has been entered.");
            }

            // Checking if all chars are allowed
            for (var i = 0; i < c; i++)
            {
                if (ArraySearch(this.text[i], this.keys) == -1)
                {
                    throw new BCGParseException("code39", "The character '" + this.text[i] + "' is not allowed.");
                }
            }

            if (this.text.IndexOf('*') >= 0)
            {
                throw new BCGParseException("code39", "The character '*' is not allowed.");
            }

            base.Validate();
        }

        /// <summary>
        /// Overloaded method to calculate checksum.
        /// </summary>
        protected override void CalculateChecksum()
        {
            this.checksumValue = new int[1] { 0 };
            var c = this.text.Length;
            for (var i = 0; i < c; i++)
            {
                this.checksumValue[0] += this.FindIndex(this.text[i]);
            }

            this.checksumValue[0] = this.checksumValue[0] % 43;
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