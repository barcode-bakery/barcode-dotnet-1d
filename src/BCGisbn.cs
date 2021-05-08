using BarcodeBakery.Common;
using System.Globalization;

namespace BarcodeBakery.Barcode
{
    /// <summary>
    /// ISBN-10 and ISBN-13
    ///
    /// You can provide an ISBN with 10 digits with or without the checksum.
    /// You can provide an ISBN with 13 digits with or without the checksum.
    /// Calculate the ISBN based on the EAN-13 encoding.
    ///
    /// The checksum is always displayed.
    /// </summary>
    public class BCGisbn : BCGean13
    {
        /// <summary>
        /// GS1 Prefix.
        /// </summary>
        public enum GS1
        {
            /// <summary>
            /// Auto Prefix.
            /// </summary>
            Auto = 0,

            /// <summary>
            /// Prefix 978.
            /// </summary>
            PREFIX978 = 1,

            /// <summary>
            /// Prefix 979.
            /// </summary>
            PREFIX979 = 2
        }

        private GS1 gs1;

        /// <summary>
        /// Creates a ISBN barcode.
        /// </summary>
        public BCGisbn()
            : base()
        {
            Initialize(GS1.Auto);
        }

        /// <summary>
        /// Creates a ISBN barcode with a specific GS1.
        /// </summary>
        /// <param name="gs1">The GS1.</param>
        public BCGisbn(GS1 gs1)
            : base()
        {
            Initialize(gs1);
        }

        private void Initialize(GS1 gs1)
        {
            this.SetGS1(gs1);
        }

        /// <summary>
        /// Adds the default label.
        /// </summary>
        protected override void AddDefaultLabel()
        {
            if (this.IsDefaultEanLabelEnabled())
            {
                var isbn = this.CreateISBNText();
                var font = this.font;

                var topLabel = new BCGLabel(isbn, font, BCGLabel.Position.Top, BCGLabel.Alignment.Center);

                this.AddLabel(topLabel);
            }

            base.AddDefaultLabel();
        }

        /// <summary>
        /// Sets the first numbers of the barcode.
        ///  - <see cref="GS1.Auto"/>: Adds 978 before the code
        ///  - <see cref="GS1.PREFIX978"/>: Adds 978 before the code
        ///  - <see cref="GS1.PREFIX979"/>: Adds 979 before the code
        /// </summary>
        /// <param name="gs1">The GS1 code.</param>
        public void SetGS1(GS1 gs1)
        {
            if (gs1 != GS1.Auto && gs1 != GS1.PREFIX978 && gs1 != GS1.PREFIX979)
            {
                throw new BCGArgumentException("The GS1 argument must be GS1.Auto, GS1.PREFIX978, or GS1.PREFIX979", nameof(gs1));
            }

            this.gs1 = gs1;
        }

        /// <summary>
        /// Check chars allowed.
        /// </summary>
        protected override void CheckCharsAllowed()
        {
            var c = this.text.Length;

            // Special case, if we have 10 digits, the last one can be X
            if (c == 10)
            {
                if (ArraySearch(this.text[9], this.keys) == -1 && this.text[9] != 'X')
                {
                    throw new BCGParseException("isbn", "The character '" + this.text[9] + "' is not allowed.");
                }

                // Drop the last char
                this.text = this.text.Substring(0, 9);
            }

            base.CheckCharsAllowed();
        }

        /// <summary>
        /// Check correct length.
        /// </summary>
        protected override void CheckCorrectLength()
        {
            var c = this.text.Length;

            // If we have 13 chars just flush the last one
            if (c == 13)
            {
                this.text = this.text.Substring(0, 12);
            }
            else if (c == 9 || c == 10)
            {
                if (c == 10)
                {
                    // Before dropping it, we check if it's legal
                    if (ArraySearch(this.text[9], this.keys) == -1 && this.text[9] != 'X')
                    {
                        throw new BCGParseException("isbn", "The character '" + this.text[9] + "' is not allowed.");
                    }

                    this.text = this.text.Substring(0, 9);
                }

                if (this.gs1 == GS1.Auto || this.gs1 == GS1.PREFIX978)
                {
                    this.text = "978" + this.text;
                }
                else if (this.gs1 == GS1.PREFIX979)
                {
                    this.text = "979" + this.text;
                }
            }
            else if (c != 12)
            {
                throw new BCGParseException("isbn", "The code parsed must be 9, 10, 12, or 13 digits long.");
            }
        }

        /// <summary>
        /// Creates the ISBN text.
        /// </summary>
        /// <returns>The ISBN text.</returns>
        private string CreateISBNText()
        {
            var isbn = "";
            if (!string.IsNullOrEmpty(this.text))
            {
                // We try to create the ISBN Text... the hyphen really depends the ISBN agency.
                // We just put one before the checksum and one after the GS1 if present.
                var c = this.text.Length;
                if (c == 12 || c == 13)
                {
                    // If we have 13 characters now, just transform it temporarily to find the checksum...
                    // Further in the code we take care of that anyway.
                    var lastCharacter = "";
                    if (c == 13)
                    {
                        lastCharacter = this.text[12].ToString();
                        this.text = this.text.Substring(0, 12);
                    }

                    var checksum = this.ProcessChecksum();
                    isbn = "ISBN " + this.text.Substring(0, 3) + "-" + this.text.Substring(3, 9) + "-" + checksum;

                    // Put the last character back
                    if (c == 13)
                    {
                        this.text += lastCharacter;
                    }
                }
                else if (c == 9 || c == 10)
                {
                    var checksum = 0;
                    for (var i = 10; i >= 2; i--)
                    {
                        checksum += this.text[10 - i] * i;
                    }

                    checksum = 11 - checksum % 11;

                    isbn = "ISBN " + this.text.Substring(0, 9) + '-' + (checksum == 10 ? "X" : checksum.ToString(CultureInfo.InvariantCulture));
                }
            }

            return isbn;
        }
    }
}