using BarcodeBakery.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace BarcodeBakery.Barcode
{
    /// <summary>
    /// Code 128. A, B or C.
    /// If you display the checksum on the label, you may obtain incorrect characters, since some characters are not displayable.
    /// </summary>
    public class BCGcode128 : BCGBarcode1D
    {
        /// <summary>
        /// The Table.
        /// </summary>
        public enum Code
        {
            /// <summary>
            /// Auto selection.
            /// </summary>
            Auto = 0,

            /// <summary>
            /// Table A.
            /// </summary>
            A,

            /// <summary>
            /// Table B.
            /// </summary>
            B,

            /// <summary>
            /// Table C.
            /// </summary>
            C
        }

        private const int KEYA_FNC3 = 96;
        private const int KEYA_FNC2 = 97;
        private const int KEYA_SHIFT = 98;
        private const int KEYA_CODEC = 99;
        private const int KEYA_CODEB = 100;
        private const int KEYA_FNC4 = 101;
        private const int KEYA_FNC1 = 102;

        private const int KEYB_FNC3 = 96;
        private const int KEYB_FNC2 = 97;
        private const int KEYB_SHIFT = 98;
        private const int KEYB_CODEC = 99;
        private const int KEYB_FNC4 = 100;
        private const int KEYB_CODEA = 101;
        private const int KEYB_FNC1 = 102;

        private const int KEYC_CODEB = 100;
        private const int KEYC_CODEA = 101;
        private const int KEYC_FNC1 = 102;

        private const int KEY_STARTA = 103;
        private const int KEY_STARTB = 104;
        private const int KEY_STARTC = 105;

        private const int KEY_STOP = 106;

        /// <summary>
        /// The keys for the table A.
        /// </summary>
        protected string keysA = null!; // !Assigned in the constructor.

        /// <summary>
        /// The keys for the table B.
        /// </summary>
        protected string keysB = null!; // !Assigned in the constructor.

        /// <summary>
        /// The keys for the table C.
        /// </summary>
        protected string keysC = null!; // !Assigned in the constructor.

        private string? startingText;

        /// <summary>
        /// The encoded data.
        /// </summary>
        protected string[]? data;

        /// <summary>
        /// Data for handling the checksum.
        /// </summary>
        protected int[]? indcheck;

        /// <summary>
        /// The last table used.
        /// </summary>
        protected char lastTable;

        /// <summary>
        /// Indicates if we are in tilde mode.
        /// </summary>
        protected bool tilde;

        private int[][] shift = null!; // !Assigned in the constructor.
        private int[][] latch = null!; // !Assigned in the constructor.
        private int[][] fnc = null!; // !Assigned in the constructor.

        private IDictionary<Code, string> METHOD = null!; // !Assigned in the constructor.

        /// <summary>
        /// Creates a Code 128 barcode.
        /// </summary>
        public BCGcode128()
            : base()
        {
            Initialize(null);
        }

        /// <summary>
        /// Constructor. Allowing to force a start table.
        /// </summary>
        /// <param name="start">The start table.</param>
        public BCGcode128(string start)
            : base()
        {
            Initialize(start);
        }

        private void Initialize(string? start)
        {
            /* CODE 128 A */
            this.keysA = " !\"#$%&\'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_";
            for (int i = 0; i < 32; i++)
            {
                this.keysA += ((char)i).ToString();
            }

            /* CODE 128 B */
            this.keysB = " !\"#$%&\'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~" + ((char)127).ToString();

            /* CODE 128 C */
            this.keysC = "0123456789";

            this.code = new string[] {
                "101111",     /* 00 */
                "111011",     /* 01 */
                "111110",     /* 02 */
                "010112",     /* 03 */
                "010211",     /* 04 */
                "020111",     /* 05 */
                "011102",     /* 06 */
                "011201",     /* 07 */
                "021101",     /* 08 */
                "110102",     /* 09 */
                "110201",     /* 10 */
                "120101",     /* 11 */
                "001121",     /* 12 */
                "011021",     /* 13 */
                "011120",     /* 14 */
                "002111",     /* 15 */
                "012011",     /* 16 */
                "012110",     /* 17 */
                "112100",     /* 18 */
                "110021",     /* 19 */
                "110120",     /* 20 */
                "102101",     /* 21 */
                "112001",     /* 22 */
                "201020",     /* 23 */
                "200111",     /* 24 */
                "210011",     /* 25 */
                "210110",     /* 26 */
                "201101",     /* 27 */
                "211001",     /* 28 */
                "211100",     /* 29 */
                "101012",     /* 30 */
                "101210",     /* 31 */
                "121010",     /* 32 */
                "000212",     /* 33 */
                "020012",     /* 34 */
                "020210",     /* 35 */
                "001202",     /* 36 */
                "021002",     /* 37 */
                "021200",     /* 38 */
                "100202",     /* 39 */
                "120002",     /* 40 */
                "120200",     /* 41 */
                "001022",     /* 42 */
                "001220",     /* 43 */
                "021020",     /* 44 */
                "002012",     /* 45 */
                "002210",     /* 46 */
                "022010",     /* 47 */
                "202010",     /* 48 */
                "100220",     /* 49 */
                "120020",     /* 50 */
                "102002",     /* 51 */
                "102200",     /* 52 */
                "102020",     /* 53 */
                "200012",     /* 54 */
                "200210",     /* 55 */
                "220010",     /* 56 */
                "201002",     /* 57 */
                "201200",     /* 58 */
                "221000",     /* 59 */
                "203000",     /* 60 */
                "110300",     /* 61 */
                "320000",     /* 62 */
                "000113",     /* 63 */
                "000311",     /* 64 */
                "010013",     /* 65 */
                "010310",     /* 66 */
                "030011",     /* 67 */
                "030110",     /* 68 */
                "001103",     /* 69 */
                "001301",     /* 70 */
                "011003",     /* 71 */
                "011300",     /* 72 */
                "031001",     /* 73 */
                "031100",     /* 74 */
                "130100",     /* 75 */
                "110003",     /* 76 */
                "302000",     /* 77 */
                "130001",     /* 78 */
                "023000",     /* 79 */
                "000131",     /* 80 */
                "010031",     /* 81 */
                "010130",     /* 82 */
                "003101",     /* 83 */
                "013001",     /* 84 */
                "013100",     /* 85 */
                "300101",     /* 86 */
                "310001",     /* 87 */
                "310100",     /* 88 */
                "101030",     /* 89 */
                "103010",     /* 90 */
                "301010",     /* 91 */
                "000032",     /* 92 */
                "000230",     /* 93 */
                "020030",     /* 94 */
                "003002",     /* 95 */
                "003200",     /* 96 */
                "300002",     /* 97 */
                "300200",     /* 98 */
                "002030",     /* 99 */
                "003020",     /* 100*/
                "200030",     /* 101*/
                "300020",     /* 102*/
                "100301",     /* 103*/
                "100103",     /* 104*/
                "100121",     /* 105*/
                "122000"      /*STOP*/
            };
            this.SetStart(start);
            this.SetTilde(true);

            // Latches and Shifts
            this.latch = new int[][] {
                new int[] {-1,                          BCGcode128.KEYA_CODEB,  BCGcode128.KEYA_CODEC},
                new int[] {BCGcode128.KEYB_CODEA,       -1,                     BCGcode128.KEYB_CODEC},
                new int[] {BCGcode128.KEYC_CODEA,       BCGcode128.KEYC_CODEB,  -1}
            };
            this.shift = new int[][] {
                new int[] {-1,                          BCGcode128.KEYA_SHIFT},
                new int[] {BCGcode128.KEYB_SHIFT,       -1}
            };
            this.fnc = new int[][] {
                new int[] {BCGcode128.KEYA_FNC1,        BCGcode128.KEYA_FNC2,   BCGcode128.KEYA_FNC3,   BCGcode128.KEYA_FNC4},
                new int[] {BCGcode128.KEYB_FNC1,        BCGcode128.KEYB_FNC2,   BCGcode128.KEYB_FNC3,   BCGcode128.KEYB_FNC4},
                new int[] {BCGcode128.KEYC_FNC1,        -1,                     -1,                     -1}
            };

            // Method available
            this.METHOD = new Dictionary<Code, string>
            {
                [Code.Auto] = "#",
                [Code.A] = "A",
                [Code.B] = "B",
                [Code.C] = "C"
            };
        }

        /// <summary>
        /// Specifies the start code. Can be <see cref="Code.A"/>, <see cref="Code.B"/>, <see cref="Code.C"/>, or <see cref="Code.Auto"/>.
        ///  - Table A: Capitals + ASCII 0-31 + punct
        ///  - Table B: Capitals + LowerCase + punct
        ///  - Table C: Numbers
        ///
        /// If <see cref="Code.Auto"/> is specified, the table selection is automatically made.
        /// The default is <see cref="Code.Auto"/>.
        /// </summary>
        /// <param name="table">The table.</param>
        public void SetStart(Code table)
        {
            if (this.METHOD.TryGetValue(table, out var value))
            {
                this.SetStart(value);
            }
            else
            {
                throw new BCGArgumentException("The starting table must be A, B, C or null.", nameof(table));
            }
        }

        /// <summary>
        /// Specifies the start code. Can be 'A', 'B', 'C', or null.
        ///  - Table A: Capitals + ASCII 0-31 + punct
        ///  - Table B: Capitals + LowerCase + punct
        ///  - Table C: Numbers
        ///
        /// If null is specified, the table selection is automatically made.
        /// The default is null.
        /// </summary>
        /// <param name="table">The table.</param>
        public void SetStart(string? table)
        {
            if (table != "A" && table != "B" && table != "C" && table != null)
            {
                throw new BCGArgumentException("The starting table must be A, B, C or null.", nameof(table));
            }

            this.startingText = table;
        }

        /// <summary>
        /// Gets the tilde.
        /// </summary>
        /// <returns>True if enabled.</returns>
        public bool GetTilde()
        {
            return this.tilde;
        }

        /// <summary>
        /// Accepts tilde to be process as a special character.
        /// If true, you can do this:
        ///  - ~~    : to make ONE tilde
        ///  - ~Fx   : to insert FCNx.x is equal from 1 to 4.
        /// </summary>
        /// <param name="accept">Accept the tilde as special character.</param>
        public void SetTilde(bool accept)
        {
            this.tilde = accept;
        }

        /// <summary>
        /// Parses the input with specific tables.
        /// </summary>
        /// <param name="input">The input.</param>
        public void Parse(BCGDataInput<Code> input)
        {
            Parse(new BCGDataInput<Code>[] { input });
        }

        /// <summary>
        /// Parses the input with specific tables.
        /// </summary>
        /// <param name="input">The input.</param>
        public void Parse(BCGDataInput<Code>[] input)
        {
            this.SetStartFromText(input[0].Mode, input[0].Data);

            this.text = "";
            var seq = "";

            var currentMode = this.startingText;
            foreach (var inp in input)
            {
                if (inp.Mode == Code.Auto)
                {
                    seq += this.GetSequence(inp.Data, ref currentMode);
                    this.text += inp.Data;
                }
                else
                {
                    seq += this.InvokeSetParse(inp.Mode, inp.Data, ref currentMode);
                    this.text += inp.Data;
                }
            }

            if (!string.IsNullOrEmpty(seq))
            {
                var bitstream = this.CreateBinaryStream(this.text, seq);
                this.SetData(bitstream);
            }

            this.AddDefaultLabel();
        }

        /// <summary>
        /// Parses the text before displaying it.
        /// </summary>
        /// <param name="text">The text.</param>
        public override void Parse(string text)
        {
            this.Parse(new BCGDataInput<Code>(Code.Auto, text));
        }

        private string? InvokeSetParse(Code mode, string arg1, ref string? arg2)
        {
            return mode switch
            {
                Code.A => this.SetParseA(arg1, ref arg2),
                Code.B => this.SetParseB(arg1, ref arg2),
                Code.C => this.SetParseC(arg1, ref arg2),
                _ => null,
            };
        }

        /// <summary>
        /// Draws the barcode.
        /// </summary>
        /// <param name="image">The surface.</param>
        public override void Draw(BCGSurface image)
        {
            Debug.Assert(this.data != null);

            var c = this.data.Length;
            for (var i = 0; i < c; i++)
            {
                this.DrawChar(image, this.data[i], true);
            }

            this.DrawChar(image, "1", true);
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

            // Contains start + text + checksum + stop
            var textlength = this.data.Length * 11;
            var endlength = 2; // + final bar

            width += textlength + endlength;
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
                throw new BCGParseException("code128", "No data has been entered.");
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
            // First Char (START)
            // + Starting with the first data character following the start character,
            // take the value of the character (between 0 and 102, inclusive) multiply
            // it by its character position (1) and add that to the running checksum.
            // Modulated 103
            this.checksumValue = new int[] { this.indcheck[0] };
            var c = this.indcheck.Length;
            for (var i = 1; i < c; i++)
            {
                this.checksumValue[0] += this.indcheck[i] * i;
            }

            this.checksumValue[0] = this.checksumValue[0] % 103;
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
                if (this.lastTable == 'C')
                {
                    return this.checksumValue[0].ToString();
                }

                string keys = (this.lastTable == 'A') ? this.keysA : this.keysB;
                return keys[this.checksumValue[0]].ToString();
            }

            return null;
        }

        /// <summary>
        /// Specifies the <see cref="startingText"/> table if none has been specified earlier.
        /// </summary>
        /// <param name="mode">The mode.</param>
        /// <param name="text">The text.</param>
        private void SetStartFromText(Code mode, string text)
        {
            if (this.startingText == null)
            {
                // If we have a forced table at the start, we get that one...
                if (mode != Code.Auto)
                {
                    this.startingText = this.METHOD[mode];
                    return;
                }

                // At this point, we had an "automatic" table selection...
                // If we can get at least 4 numbers, go in C; otherwise go in B.
                var tmp = BCGcode128.Escape(this.keysC);
                var length = text.Length;
                if (length >= 4 && Regex.Match(text.Substring(0, 4), "[" + tmp + "]").Success)
                {
                    this.startingText = "C";
                }
                else
                {
                    if (length > 0 && this.keysB.IndexOf(text[0]) != -1)
                    {
                        this.startingText = "B";
                    }
                    else
                    {
                        this.startingText = "A";
                    }
                }
            }
        }

        /// <summary>
        /// Extracts the ~ value from the <paramref name="text"/> at the <paramref name="pos"/>.
        /// If the tilde is not ~~, ~F1, ~F2, ~F3, ~F4; an error is raised.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="pos">The posision.</param>
        /// <returns>Extracted tilde value.</returns>
        private static string ExtractTilde(string text, int pos)
        {
            if (text[pos] == '~')
            {
                if (text.Length > pos + 1)
                {
                    // Do we have a tilde?
                    if (text[pos + 1] == '~')
                    {
                        return "~~";
                    }
                    else if (text[pos + 1] == 'F')
                    {
                        // Do we have a number after?
                        if (text.Length > pos + 2)
                        {
                            if (int.TryParse(text[pos + 2].ToString(), out int v) && v >= 1 && v <= 4)
                            {
                                return "~F" + v;
                            }
                            else
                            {
                                throw new BCGParseException("code128", "Bad ~F. You must provide a number from 1 to 4.");
                            }
                        }
                        else
                        {
                            throw new BCGParseException("code128", "Bad ~F. You must provide a number from 1 to 4.");
                        }
                    }
                    else
                    {
                        throw new BCGParseException("code128", "Wrong code after the ~.");
                    }
                }
                else
                {
                    throw new BCGParseException("code128", "Wrong code after the ~.");
                }
            }
            else
            {
                throw new BCGParseException("code128", "There is no ~ at this location.");
            }
        }

        /// <summary>
        /// Gets the "dotted" sequence for the <paramref name="text"/> based on the <paramref name="currentMode"/>.
        /// There is also a check if we use the special tilde ~
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="currentMode">The current mode.</param>
        /// <returns>The sequence.</returns>
        private string GetSequenceParsed(string text, string currentMode)
        {
            int length;
            if (this.tilde)
            {
                var sequence = "";
                var previousPos = 0;
                int pos;
                while ((pos = text.IndexOf("~", previousPos, StringComparison.Ordinal)) != -1)
                {
                    var tildeData = BCGcode128.ExtractTilde(text, pos);

                    var simpleTilde = (tildeData == "~~");
                    if (simpleTilde && currentMode != "B")
                    {
                        throw new BCGParseException("code128", "The Table " + currentMode + " doesn't contain the character ~.");
                    }

                    // At this point, we know we have ~Fx
                    if (tildeData != "~F1" && currentMode == "C")
                    {
                        // The mode C doesn't support ~F2, ~F3, ~F4
                        throw new BCGParseException("code128", "The Table C doesn't contain the function " + tildeData + ".");
                    }

                    length = pos - previousPos;
                    if (currentMode == "C")
                    {
                        if (length % 2 == 1)
                        {
                            throw new BCGParseException("code128", "The text '" + text + "' must have an even number of character to be encoded in Table C.");
                        }
                    }

                    sequence += new string('.', length);
                    sequence += ".";
                    sequence += (!simpleTilde) ? "F" : "";
                    previousPos = pos + tildeData.Length;
                }

                // Flushing
                length = text.Length - previousPos;
                if (currentMode == "C")
                {
                    if (length % 2 == 1)
                    {
                        throw new BCGParseException("code128", "The text '" + text + "' must have an even number of character to be encoded in Table C.");
                    }
                }

                sequence += new string('.', length);

                return sequence;
            }
            else
            {
                return new string('.', text.Length);
            }
        }

        /// <summary>
        /// Parses the text and returns the appropriate sequence for the Table A.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="currentMode">The current mode.</param>
        /// <returns>The sequence.</returns>
        private string SetParseA(string text, ref string? currentMode)
        {
            var tmp = BCGcode128.Escape(this.keysA);

            // If we accept the ~ for special character, we must allow it.
            if (this.tilde)
            {
                tmp += "~";
            }

            var match = Regex.Match(text, "[^" + tmp + "]");
            if (match.Success)
            {
                // We found something not allowed
                throw new BCGParseException("code128", "The text '" + text + "' can't be parsed with the Table A. The character '" + match.Value + "' is not allowed.");
            }
            else
            {
                var latch = (currentMode == "A") ? "" : "0";
                currentMode = "A";

                return latch + this.GetSequenceParsed(text, currentMode);
            }
        }

        /// <summary>
        /// Parses the text and returns the appropriate sequence for the Table B.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="currentMode">The current mode.</param>
        /// <returns>The sequence.</returns>
        private string SetParseB(string text, ref string? currentMode)
        {
            var tmp = BCGcode128.Escape(this.keysB);

            var match = Regex.Match(text, "[^" + tmp + "]");
            if (match.Success)
            {
                // We found something not allowed
                throw new BCGParseException("code128", "The text '" + text + "' can't be parsed with the Table B. The character '" + match.Value + "' is not allowed.");
            }
            else
            {
                var latch = (currentMode == "B") ? "" : "1";
                currentMode = "B";

                return latch + this.GetSequenceParsed(text, currentMode);
            }
        }

        /// <summary>
        /// Parses the text and returns the appropriate sequence for the Table C.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="currentMode">The current mode.</param>
        /// <returns>The sequence.</returns>
        private string SetParseC(string text, ref string? currentMode)
        {
            var tmp = Regex.Escape(this.keysC);

            // If we accept the ~ for special character, we must allow it.
            if (this.tilde)
            {
                tmp += "~F";
            }

            var match = Regex.Match(text, "[^" + tmp + "]");
            if (match.Success)
            {
                // We found something not allowed
                throw new BCGParseException("code128", "The text '" + text + "' can't be parsed with the Table C. The character '" + match.Value + "' is not allowed.");
            }
            else
            {
                var latch = (currentMode == "C") ? "" : "2";
                currentMode = "C";

                return latch + this.GetSequenceParsed(text, currentMode);
            }
        }

        /// <summary>
        /// Depending on the <paramref name="text"/>, it will return the correct
        /// sequence to encode the text.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="startingText">The starting text.</param>
        /// <returns>The sequence.</returns>
        private string GetSequence(string text, ref string? startingText)
        {
            var e = 10000;
            var latLen = new int[][] {
                new int[] {0, 1, 1},
                new int[] {1, 0, 1},
                new int[] {1, 1, 0}
            };
            var shftLen = new int[][] {
                new int[] {e, 1, e},
                new int[] {1, e, e},
                new int[] {e, e, e}
            };
            var charSiz = new int[] { 2, 2, 1 };

            var startA = e;
            var startB = e;
            var startC = e;
            if (startingText == "A") { startA = 0; }
            if (startingText == "B") { startB = 0; }
            if (startingText == "C") { startC = 0; }

            var curLen = new int[] { startA, startB, startC };
            var curSeq = new string[] { "", "", "" };

            var nextNumber = false;
            var xLen = text.Length;

            for (var x = 0; x < xLen; x++)
            {
                var input = text[x];

                // 1.
                for (var i = 0; i < 3; i++)
                {
                    for (var j = 0; j < 3; j++)
                    {
                        if ((curLen[i] + latLen[i][j]) < curLen[j])
                        {
                            curLen[j] = curLen[i] + latLen[i][j];
                            curSeq[j] = curSeq[i] + j;
                        }
                    }
                }

                // 2.
                var nxtLen = new int[] { e, e, e };
                var nxtSeq = new Dictionary<int, string>();

                // 3.
                var flag = false;
                var posArray = new List<int>();

                // Special case, we do have a tilde and we process them
                if (this.tilde && input == '~')
                {
                    var tildeData = BCGcode128.ExtractTilde(text, x);

                    if (tildeData == "~~")
                    {
                        // We simply skip a tilde
                        posArray.Add(1);
                        x++;
                    }
                    else if (tildeData.Substring(0, 2) == "~F")
                    {
                        int.TryParse(tildeData[2].ToString(), out var v);
                        posArray.Add(0);
                        posArray.Add(1);
                        if (v == 1)
                        {
                            posArray.Add(2);
                        }

                        x += 2;
                        flag = true;
                    }
                }
                else
                {
                    var pos = this.keysA.IndexOf(input);
                    if (pos != -1)
                    {
                        posArray.Add(0);
                    }

                    pos = this.keysB.IndexOf(input);
                    if (pos != -1)
                    {
                        posArray.Add(1);
                    }

                    // Do we have the next char a number?? OR a ~F1
                    pos = this.keysC.IndexOf(input);
                    if (nextNumber || (pos != -1 && text.Length > x + 1 && this.keysC.IndexOf(text[x + 1]) != -1))
                    {
                        nextNumber = !nextNumber;
                        posArray.Add(2);
                    }
                }

                var c = posArray.Count;
                for (var i = 0; i < c; i++)
                {
                    if ((curLen[posArray[i]] + charSiz[posArray[i]]) < nxtLen[posArray[i]])
                    {
                        nxtLen[posArray[i]] = curLen[posArray[i]] + charSiz[posArray[i]];
                        nxtSeq[posArray[i]] = curSeq[posArray[i]] + ".";
                    }

                    for (var j = 0; j < 2; j++)
                    {
                        if (j == posArray[i]) { continue; }
                        if ((curLen[j] + shftLen[j][posArray[i]] + charSiz[posArray[i]]) < nxtLen[j])
                        {
                            nxtLen[j] = curLen[j] + shftLen[j][posArray[i]] + charSiz[posArray[i]];
                            nxtSeq[j] = curSeq[j] + ((char)(posArray[i] + 65)).ToString() + ".";
                        }
                    }
                }

                if (c == 0)
                {
                    // We found an unsuported character
                    throw new BCGParseException("code128", "Character " + input + " not supported.");
                }

                if (flag)
                {
                    for (var i = 0; i < 5; i++)
                    {
                        if (nxtSeq.ContainsKey(i))
                        {
                            nxtSeq[i] += "F";
                        }
                    }
                }

                // 4.
                for (var i = 0; i < 3; i++)
                {
                    curLen[i] = nxtLen[i];
                    if (nxtSeq.ContainsKey(i))
                    {
                        curSeq[i] = (string)nxtSeq[i];
                    }
                }
            }

            // Every curLen under $e are possible but we take the smallest !
            var m = e;
            var k = -1;
            for (var i = 0; i < 3; i++)
            {
                if (curLen[i] < m)
                {
                    k = i;
                    m = curLen[i];
                }
            }

            if (k == -1)
            {
                return "";
            }

            return curSeq[k];
        }

        /// <summary>
        /// Depending on the sequence <paramref name="seq"/> given (returned from <see cref="GetSequence(string, ref string)"/>),
        /// this method will return the code stream in an array. Each char will be a
        /// string of bit based on the Code 128.
        ///
        /// Each letter from the sequence represents bits.
        ///
        /// 0 to 2 are latches
        /// A to B are Shift + Letter
        /// . is a char in the current encoding
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="seq">The sequence.</param>
        /// <returns>The stream.</returns>
        private object CreateBinaryStream(string text, string seq)
        {
            var c = seq.Length;

            var data = new List<string>(); // code stream
            var indcheck = new List<int>(); // index for checksum

            int currentEncoding = 0;
            if (this.startingText == "A")
            {
                currentEncoding = 0;
                indcheck.Add(BCGcode128.KEY_STARTA);
                this.lastTable = 'A';
            }
            else if (this.startingText == "B")
            {
                currentEncoding = 1;
                indcheck.Add(BCGcode128.KEY_STARTB);
                this.lastTable = 'B';
            }
            else if (this.startingText == "C")
            {
                currentEncoding = 2;
                indcheck.Add(BCGcode128.KEY_STARTC);
                this.lastTable = 'C';
            }

            data.Add(this.code[103 + currentEncoding]);

            var temporaryEncoding = -1;
            var i = 0;
            var counter = 0;
            for (; i < c; i++)
            {
                var input = seq[i];
                if (!int.TryParse(input.ToString(), out int inputI))
                {
                    inputI = 0;
                }

                if (input == '.')
                {
                    this.EncodeChar(data, currentEncoding, seq, text, ref i, ref counter, indcheck);
                    if (temporaryEncoding != -1)
                    {
                        currentEncoding = temporaryEncoding;
                        temporaryEncoding = -1;
                    }
                }
                else if (input == 'A' && input == 'B')
                {
                    // We shift
                    var encoding = inputI - 65;
                    var shift = this.shift[currentEncoding][encoding];
                    indcheck.Add(shift);
                    data.Add(this.code[shift]);
                    if (temporaryEncoding == -1)
                    {
                        temporaryEncoding = currentEncoding;
                    }

                    currentEncoding = encoding;
                }
                else if (inputI >= 0 && inputI <= 3)
                {
                    temporaryEncoding = -1;

                    // We latch
                    var latch = this.latch[currentEncoding][inputI];
                    if (latch != -1)
                    {
                        indcheck.Add(latch);
                        this.lastTable = (char)(65 + inputI);
                        data.Add(this.code[latch]);
                        currentEncoding = inputI;
                    }
                }
            }

            return new object[] { indcheck.ToArray(), data.ToArray() };
        }

        /// <summary>
        /// Encodes characters, base on its encoding and sequence.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="encoding">The encoding.</param>
        /// <param name="seq">The sequence.</param>
        /// <param name="text">The text.</param>
        /// <param name="i">The position.</param>
        /// <param name="counter">The counter.</param>
        /// <param name="indcheck">The checksum counter.</param>
        private void EncodeChar(List<string> data, int encoding, string seq, string text, ref int i, ref int counter, List<int> indcheck)
        {
            if (seq.Length > i + 1 && seq[i + 1] == 'F')
            {
                // We have a flag !!
                if (text[counter + 1] == 'F')
                {
                    int.TryParse(text[counter + 2].ToString(), out var number);
                    int fnc = this.fnc[encoding][number - 1];
                    indcheck.Add(fnc);
                    data.Add(this.code[fnc]);

                    // Skip F + number
                    counter += 2;
                }
                else
                {
                    // Not supposed
                }

                i++;
            }
            else
            {
                if (encoding == 2)
                {
                    // We take 2 numbers in the same time
                    int.TryParse(text.Substring(counter, 2), out var code);
                    indcheck.Add(code);
                    data.Add(this.code[code]);
                    counter++;
                    i++;
                }
                else
                {
                    string keys = (encoding == 0) ? this.keysA : this.keysB;
                    int pos = keys.IndexOf(text[counter]);
                    indcheck.Add(pos);
                    data.Add(this.code[pos]);
                }
            }

            counter++;
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

            Debug.Assert(this.checksumValue != null);

            string[] temp = new string[this.data.Length + 2];
            Array.Copy(this.data, temp, this.data.Length);
            temp[this.data.Length] = this.code[this.checksumValue![0]];
            temp[this.data.Length + 1] = this.code[BCGcode128.KEY_STOP];
            this.data = temp;
        }

        private static string Escape(string data)
        {
            // .NET will not escape } and ]
            return Regex.Escape(data)
                .Replace("]", "\\]")
                .Replace("}", "\\}");
        }
    }
}