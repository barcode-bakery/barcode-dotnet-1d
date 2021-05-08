using BarcodeBakery.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace BarcodeBakery.Barcode
{
    /// <summary>
    /// Code 93.
    /// </summary>
    public class BCGcode93 : BCGBarcode1D
    {
        private const int EXTENDED_1 = 39;
        private const int EXTENDED_2 = 40;
        private const int EXTENDED_3 = 41;
        private const int EXTENDED_4 = 42;

        private readonly int starting;
        private readonly int ending;

        /// <summary>
        /// The encoded data.
        /// </summary>
        protected string[]? data;

        /// <summary>
        /// Data for handling the checksum.
        /// </summary>
        protected int[]? indcheck;

        /// <summary>
        /// Creates a Code 93 barcode.
        /// </summary>
        public BCGcode93()
            : base()
        {
            this.starting = this.ending = 47; /* * */
            this.keys = new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", "-", ".", " ", "$", "/", "+", "%", "($)", "(%)", "(/)", "(+)", "*" };
            this.code = new string[] {
                "020001",     /* 0 */
                "000102",     /* 1 */
                "000201",     /* 2 */
                "000300",     /* 3 */
                "010002",     /* 4 */
                "010101",     /* 5 */
                "010200",     /* 6 */
                "000003",     /* 7 */
                "020100",     /* 8 */
                "030000",     /* 9 */
                "100002",     /* A */
                "100101",     /* B */
                "100200",     /* C */
                "110001",     /* D */
                "110100",     /* E */
                "120000",     /* F */
                "001002",     /* G */
                "001101",     /* H */
                "001200",     /* I */
                "011001",     /* J */
                "021000",     /* K */
                "000012",     /* L */
                "000111",     /* M */
                "000210",     /* N */
                "010011",     /* O */
                "020010",     /* P */
                "101001",     /* Q */
                "101100",     /* R */
                "100011",     /* S */
                "100110",     /* T */
                "110010",     /* U */
                "111000",     /* V */
                "001011",     /* W */
                "001110",     /* X */
                "011010",     /* Y */
                "012000",     /* Z */
                "010020",     /* - */
                "200001",     /* . */
                "200100",     /*   */
                "210000",     /* $ */
                "001020",     /* / */
                "002010",     /* + */
                "100020",     /* % */
                "010110",     /*($)*/
                "201000",     /*(%)*/
                "200010",     /*(/)*/
                "011100",     /*(+)*/
                "000030"      /* * */
            };
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

            var c = this.text.Length;
            for (var i = 0; i < c; i++)
            {
                var pos = ArraySearch(this.text[i], this.keys);
                if (pos == -1)
                {
                    // Search in extended?
                    var extended = BCGcode93.GetExtendedVersion(this.text[i]);
                    if (extended == null)
                    {
                        throw new BCGParseException("code93", "The character '" + this.text[i] + "' is not allowed.");
                    }
                    else
                    {
                        var extc = extended.Length;
                        for (var j = 0; j < extc; j++)
                        {
                            var v = extended[j];
                            if (v == '$')
                            {
                                indcheck.Add(BCGcode93.EXTENDED_1);
                                data.Add(this.code[BCGcode93.EXTENDED_1]);
                            }
                            else if (v == '%')
                            {
                                indcheck.Add(BCGcode93.EXTENDED_2);
                                data.Add(this.code[BCGcode93.EXTENDED_2]);
                            }
                            else if (v == '/')
                            {
                                indcheck.Add(BCGcode93.EXTENDED_3);
                                data.Add(this.code[BCGcode93.EXTENDED_3]);
                            }
                            else if (v == '+')
                            {
                                indcheck.Add(BCGcode93.EXTENDED_4);
                                data.Add(this.code[BCGcode93.EXTENDED_4]);
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

            // Checksum
            c = this.checksumValue.Length;
            for (var i = 0; i < c; i++)
            {
                this.DrawChar(image, this.code[this.checksumValue[i]], true);
            }

            // Ending *
            this.DrawChar(image, this.code[this.ending], true);

            // Draw a Final Bar
            this.DrawChar(image, "0", true);
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

            var startlength = 9;
            var textlength = 9 * this.data.Length;
            var checksumlength = 2 * 9;
            var endlength = 9 + 1; // + final bar

            width += startlength + textlength + checksumlength + endlength;
            height += this.thickness;
            return base.GetDimension(width, height);
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
                throw new BCGParseException("code93", "No data has been entered.");
            }

            base.Validate();
        }

        /// <summary>
        /// Overloaded method to calculate checksum.
        /// </summary>
        protected override void CalculateChecksum()
        {
            Debug.Assert(this.indcheck != null);

            // Checksum
            // First CheckSUM "C"
            // The "C" checksum character is the modulo 47 remainder of the sum of the weighted
            // value of the data characters. The weighting value starts at "1" for the right-most
            // data character, 2 for the second to last, 3 for the third-to-last, and so on up to 20.
            // After 20, the sequence wraps around back to 1.

            // Second CheckSUM "K"
            // Same as CheckSUM "C" but we count the CheckSum "C" at the end
            // After 15, the sequence wraps around back to 1.
            var sequenceMultiplier = new int[] { 20, 15 };
            this.checksumValue = new int[2];
            var indcheck = new int[this.indcheck.Length + 2];
            Array.Copy(this.indcheck, indcheck, this.indcheck.Length); // Clone
            for (var z = 0; z < 2; z++)
            {
                int checksum = 0;
                var i = this.indcheck.Length + z;
                var j = 0;
                for (; i > 0; i--, j++)
                {
                    var multiplier = i % sequenceMultiplier[z];
                    if (multiplier == 0)
                    {
                        multiplier = sequenceMultiplier[z];
                    }

                    checksum += indcheck[j] * multiplier;
                }

                this.checksumValue[z] = checksum % 47;
                indcheck[this.indcheck.Length + z] = this.checksumValue[z];
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

        /// <summary>
        /// Saves data into the classes.
        ///
        /// This method will save data, calculate real column number
        /// (if -1 was selected), the real error level(if -1 was
        /// selected)... It will add Padding to the end and generate
        /// the error codes.
        /// </summary>
        /// <param name="data"></param>
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
