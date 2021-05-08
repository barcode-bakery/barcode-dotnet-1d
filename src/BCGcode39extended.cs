using BarcodeBakery.Common;
using System.Collections.Generic;
using System.Diagnostics;

namespace BarcodeBakery.Barcode
{
    /// <summary>
    /// Code 39 Extended.
    /// </summary>
    public class BCGcode39extended : BCGcode39
    {
        private const int EXTENDED_1 = 39;
        private const int EXTENDED_2 = 40;
        private const int EXTENDED_3 = 41;
        private const int EXTENDED_4 = 42;

        /// <summary>
        /// The encoded data.
        /// </summary>
        protected string[]? data;

        /// <summary>
        /// Data for handling the checksum.
        /// </summary>
        protected int[]? indcheck;

        /// <summary>
        /// Creates a Code 39 Extended barcode.
        /// </summary>
        public BCGcode39extended()
            : base()
        {
            // We just put parenthesis around special characters.
            this.keys[BCGcode39extended.EXTENDED_1] = "($)";
            this.keys[BCGcode39extended.EXTENDED_2] = "(/)";
            this.keys[BCGcode39extended.EXTENDED_3] = "(+)";
            this.keys[BCGcode39extended.EXTENDED_4] = "(%)";
        }

        /// <summary>
        /// Parses the text before displaying it.
        /// </summary>
        /// <param name="text">The text.</param>
        public override void Parse(string text)
        {
            this.text = text;

            var data = new List<string>();
            var indcheck = new List<int>();

            int c = this.text.Length;
            for (var i = 0; i < c; i++)
            {
                var pos = ArraySearch(this.text[i], this.keys);
                if (pos == -1)
                {
                    // Search in extended?
                    var extended = BCGcode39extended.GetExtendedVersion(this.text[i]);
                    if (extended == null)
                    {
                        throw new BCGParseException("code39extended", "The character '" + this.text[i] + "' is not allowed.");
                    }
                    else
                    {
                        var extc = extended.Length;
                        for (var j = 0; j < extc; j++)
                        {
                            var v = extended[j];
                            if (v == '$')
                            {
                                indcheck.Add(BCGcode39extended.EXTENDED_1);
                                data.Add(this.code[BCGcode39extended.EXTENDED_1]);
                            }
                            else if (v == '%')
                            {
                                indcheck.Add(BCGcode39extended.EXTENDED_2);
                                data.Add(this.code[BCGcode39extended.EXTENDED_2]);
                            }
                            else if (v == '/')
                            {
                                indcheck.Add(BCGcode39extended.EXTENDED_3);
                                data.Add(this.code[BCGcode39extended.EXTENDED_3]);
                            }
                            else if (v == '+')
                            {
                                indcheck.Add(BCGcode39extended.EXTENDED_4);
                                data.Add(this.code[BCGcode39extended.EXTENDED_4]);
                            }
                            else
                            {
                                var pos2 = ArraySearch(v, this.keys);
                                indcheck.Add(pos2);
                                data.Add(this.code[pos2]);
                            }
                        }
                    }
                }
                else
                {
                    indcheck.Add(pos);
                    data.Add(this.code[pos]);
                }
            }

            this.SetData(new object[] { indcheck.ToArray(), data.ToArray() });
            this.AddDefaultLabel();
        }

        /// <summary>
        /// Draws the barcode.
        /// </summary>
        /// <param name="image">The surface.</param>
        public override void Draw(BCGSurface image)
        {
            Debug.Assert(this.data != null);
            Debug.Assert(this.checksumValue != null);

            // Starting *
            this.DrawChar(image, this.code[this.starting], true);
            var c = this.data.Length;
            for (var i = 0; i < c; i++)
            {
                this.DrawChar(image, this.data[i], true);
            }

            // Checksum (rarely used)
            if (this.checksum == true)
            {
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
            Debug.Assert(this.data != null);

            var textlength = 13 * this.data.Length;
            var startlength = 13;
            var checksumlength = 0;
            if (this.checksum == true)
            {
                checksumlength = 13;
            }

            var endlength = 13;

            width += startlength + textlength + checksumlength + endlength;
            height += this.thickness;
            return base.Get1DDimension(width, height);
        }

        /// <summary>
        /// Validates the input.
        /// </summary>
        protected override void Validate()
        {
            Debug.Assert(this.data != null);

            var c = this.data.Length;
            if (c == 0)
            {
                throw new BCGParseException("code39extended", "No data has been entered.");
            }

            base.Validate();
        }

        /// <summary>
        /// Overloaded method to calculate checksum.
        /// </summary>
        protected override void CalculateChecksum()
        {
            Debug.Assert(this.indcheck != null);

            this.checksumValue = new int[1] { 0 };
            int c = this.indcheck.Length;
            for (int i = 0; i < c; i++)
            {
                this.checksumValue[0] += this.indcheck[i];
            }

            this.checksumValue[0] = this.checksumValue[0] % 43;
        }

        /// <summary>
        /// Saves data into the classes.
        ///
        /// This method will save data, calculate real column number
        /// (if -1 was selected), the real error level(if -1 was
        /// selected)... It will add Padding to the end and generate
        /// the error codes.
        /// </summary>
        /// <param name="data">The data.</param>
        private void SetData(object data)
        {
            this.indcheck = (int[])((object[])data)[0];
            this.data = (string[])((object[])data)[1];
            this.CalculateChecksum();
        }

        /// <summary>
        /// Returns the extended reprensentation of the character.
        /// </summary>
        /// <param name="val">The value.</param>
        /// <returns>The representation.</returns>
        private static string? GetExtendedVersion(char val)
        {
            int o = (int)val;
            if (o == 0)
            {
                return "%U";
            }
            else if (o >= 1 && o <= 26)
            {
                return "$" + (char)(o + 64);
            }
            else if ((o >= 33 && o <= 44) || o == 47 || o == 48)
            {
                return "/" + (char)(o + 32);
            }
            else if (o >= 97 && o <= 122)
            {
                return "+" + (char)(o - 32);
            }
            else if (o >= 27 && o <= 31)
            {
                return "%" + (char)(o + 38);
            }
            else if (o >= 59 && o <= 63)
            {
                return "%" + (char)(o + 11);
            }
            else if (o >= 91 && o <= 95)
            {
                return "%" + (char)(o - 16);
            }
            else if (o >= 123 && o <= 127)
            {
                return "%" + (char)(o - 43);
            }
            else if (o == 64)
            {
                return "%V";
            }
            else if (o == 96)
            {
                return "%W";
            }
            else if (o > 127)
            {
                return null;
            }
            else
            {
                return val.ToString();
            }
        }
    }
}