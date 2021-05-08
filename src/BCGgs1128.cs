using BarcodeBakery.Common;
using BarcodeBakery.Common.GS1;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;

namespace BarcodeBakery.Barcode
{
    /// <summary>
    /// Calculate the GS1-128 based on the Code-128 encoding.
    /// </summary>
    public class BCGgs1128 : BCGcode128
    {
        enum YStatus
        {
            NoVariable,
            AlreadySet,
            NotSet
        }

        private const int MAX_ID_FORMATTED = 6;
        private const int MAX_ID_NOT_FORMATTED = 4;
        private const int MAX_GS1128_CHARS = 48;

        private bool strictMode;
        private bool allowsUnknownIdentifier;
        private bool noLengthLimit;

        private List<string?> identifiersId = default!;
        private List<string> identifiersContent = default!;
        private Dictionary<string, AIData>? identifiersAi = null;

        /// <summary>
        /// Creates a GS1-128 barcode.
        /// </summary>
        public BCGgs1128()
            : this("C")
        {
        }

        /// <summary>
        /// Creates a GS1-128 barcode. Allowing to force a start table.
        /// </summary>
        /// <param name="start">The start table.</param>
        public BCGgs1128(string start)
            : base(start)
        {
            this.strictMode = true;
            SetTilde(true);
            this.allowsUnknownIdentifier = false;
            this.noLengthLimit = false;
        }

        /// <summary>
        /// Gets the content checksum for an identifier.
        /// Do not pass the identifier code.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <returns>The checksum.</returns>
        public static int GetAiContentChecksum(string content)
        {
            return CalculateChecksumMod10(content);
        }

        /// <summary>
        /// Enables or disables the strict mode.
        /// </summary>
        /// <param name="strictMode">Strict mode.</param>
        public void SetStrictMode(bool strictMode)
        {
            this.strictMode = strictMode;
        }

        /// <summary>
        /// Gets if the strict mode is activated.
        /// </summary>
        /// <returns>True if enabled.</returns>
        public bool GetStrictMode()
        {
            return this.strictMode;
        }

        /// <summary>
        /// Allows unknown identifiers.
        /// </summary>
        /// <param name="allow">Allows the unknown identifier.</param>
        public void SetAllowsUnknownIdentifier(bool allow)
        {
            this.allowsUnknownIdentifier = allow;
        }

        /// <summary>
        /// Gets if unkmown identifiers are allowed.
        /// </summary>
        /// <returns>True if enabled.</returns>
        public bool GetAllowsUnknownIdentifier()
        {
            return this.allowsUnknownIdentifier;
        }

        /// <summary>
        /// Removes the limit of 48 characters.
        /// </summary>
        /// <param name="noLengthLimit">No limit.</param>
        public void SetNoLengthLimit(bool noLengthLimit)
        {
            this.noLengthLimit = noLengthLimit;
        }

        /// <summary>
        /// Gets if the limit of 48 characters is removed.
        /// </summary>
        /// <returns>True if enabled.</returns>
        public bool GetNoLengthLimit()
        {
            return this.noLengthLimit;
        }

        /// <summary>
        /// Sets the list of application identifiers.
        /// </summary>
        /// <param name="aiDatas">Application identifiers.</param>
        public void SetApplicationIdentifiers(IEnumerable<AIData> aiDatas)
        {
            this.identifiersAi = aiDatas.ToDictionary(m => m.AI);
        }

        /// <summary>
        /// Gets the list of application identifiers.
        /// </summary>
        /// <returns>Application Identifiers.</returns>
        public IEnumerable<AIData>? GetApplicationIdentifiers()
        {
            return identifiersAi?.Values;
        }

        /// <summary>
        /// Parses the text before displaying it.
        /// </summary>
        /// <param name="text">The text.</param>
        public override void Parse(string text)
        {
            Parse(new Input(text));
        }

        /// <summary>
        /// Sets the GS1 input. Will parse the input accordingly.
        /// </summary>
        /// <param name="input">The input.</param>
        public void Parse(Input input)
        {
            Parse(new Input[] { input });
        }

        /// <summary>
        /// Sets the GS1 inputs. Will parse the input accordingly.
        /// </summary>
        /// <param name="inputs">The inputs.</param>
        public void Parse(Input[] inputs)
        {
            this.identifiersId = new List<string?>();
            this.identifiersContent = new List<string>();

            var data = this.ParseGs1128(inputs);
            if (data == null)
            {
                throw new BCGParseException("The input did not result in any data.");
            }

            base.Parse(data);
        }

        /// <summary>
        /// You cannot use this method for GS1-128.
        /// </summary>
        /// <param name="input">The input.</param>
        public new void Parse(BCGDataInput<Code> input)
        {
            throw new BCGParseException("Do not use this method if you use GS1-128.");
        }

        /// <summary>
        /// You cannot use this method for GS1-128.
        /// </summary>
        /// <param name="input">The input.</param>
        public new void Parse(BCGDataInput<Code>[] input)
        {
            throw new BCGParseException("Do not use this method if you use GS1-128.");
        }

        /// <summary>
        /// Formats data for gs1-128.
        /// </summary>
        /// <returns>Final formatted data.</returns>
        private string FormatGs1128()
        {
            var formattedText = "~F1";
            var formattedLabel = "";
            var c = this.identifiersId.Count;

            for (var i = 0; i < c; i++)
            {
                if (i > 0)
                {
                    formattedLabel += " ";
                }

                if (this.identifiersId[i] != null)
                {
                    formattedLabel += "(" + this.identifiersId[i] + ")";
                }

                formattedText += this.identifiersId[i];

                formattedLabel += this.identifiersContent[i];
                formattedText += this.identifiersContent[i];

                AIData? aiData = null;
                var ai = this.identifiersId[i];
                if (ai != null && !this.identifiersAi!.TryGetValue(ai, out aiData) && ai.Length > 3) // ! If we have an AI, we have the identifier.
                {
                    var identifierWithVar = ai.Substring(0, ai.Length - 1) + 'y';
                    aiData = identifiersAi.ContainsKey(identifierWithVar) ? identifiersAi[identifierWithVar] : null;
                }

                /* We'll check if we need to add a ~F1 (<GS>) char */
                /* If we use the legacy mode, we always add a ~F1 (<GS>) char between AIs */
                if (aiData != null)
                {
                    if ((this.identifiersContent[i].Length < aiData.MaxLength && (i + 1) != c) || (!this.strictMode && (i + 1) != c))
                    {
                        formattedText += "~F1";
                    }
                }
                else if (this.allowsUnknownIdentifier && this.identifiersId[i] == null && (i + 1) != c)
                {
                    /* If this id is unknown, we add a ~F1 (<GS>) char */
                    formattedText += "~F1";
                }
            }

            if (this.noLengthLimit == false)
            {
                var calculableCharacters = formattedText.Replace("~F1", ((char)29).ToString());
                calculableCharacters = calculableCharacters.Replace("(", "");
                calculableCharacters = calculableCharacters.Replace(")", "");

                if (calculableCharacters.Length - 1 > MAX_GS1128_CHARS)
                {
                    throw new BCGParseException("gs1128", $"The barcode can't contain more than {MAX_GS1128_CHARS} characters.");
                }
            }

            if (this.label == AUTO_LABEL)
            {
                this.label = formattedLabel;
            }

            return formattedText;
        }

        /// <summary>
        /// Parses the inputs.
        /// </summary>
        /// <param name="inputs">The inputs.</param>
        /// <returns>Final formatted data.</returns>
        private string? ParseGs1128(Input[] inputs)
        {
            /* We format correctly what the user gives */
            var formatArray = new List<string>();
            foreach (var content in inputs)
            {
                if (content.AI != null)
                {
                    formatArray.Add("(" + content.AI + ")" + content.Content);
                }
                else
                {
                    formatArray.Add(content.Content);
                }
            }


            var textCount = formatArray.Count;
            for (var cmpt = 0; cmpt < textCount; cmpt++)
            {
                /* We parse the content of the array */
                if (!this.ParseContent(formatArray[cmpt]))
                {
                    return null;
                }
            }

            return this.FormatGs1128();
        }

        /// <summary>
        /// Splits the id and the content for each application identifiers (AIs).
        /// </summary>
        /// <param name="text">The unformatted text.</param>
        /// <returns>True on success.</returns>
        private bool ParseContent(string text)
        {
            string? content;
            var yAlreadySet = YStatus.NoVariable;
            string? realNameId = null;
            var separatorsFound = 0;
            var checksumAdded = 0;
            var decimalPointRemoved = 0;
            var toParse = text.Replace("~F1", ((char)29).ToString());
            var nbCharToParse = toParse.Length;
            int nbCharId;
            var isFormatted = toParse[0] == '(';
            var maxCharId = isFormatted ? MAX_ID_FORMATTED : MAX_ID_NOT_FORMATTED;
            var id = toParse.Substring(0, Math.Min(maxCharId, nbCharToParse)).ToLower();
            id = isFormatted ? this.FindIdFormatted(id, ref yAlreadySet, ref realNameId) : this.FindIdNotFormatted(id, ref yAlreadySet, ref realNameId);

            if (id == null)
            {
                if (this.allowsUnknownIdentifier == false)
                {
                    return false;
                }

                id = null;
                nbCharId = 0;
                content = toParse;
            }
            else
            {
                // The code will go in the above IF if we have not set the identifiers.
                Debug.Assert(realNameId != null);
                Debug.Assert(this.identifiersAi != null);

                nbCharId = id.Length + (isFormatted ? 2 : 0);
                var n = Math.Min(this.identifiersAi[realNameId].MaxLength, nbCharToParse);
                content = Utility.SafeSubstring(toParse, nbCharId, n);

                if (id != null)
                {
                    /* If we have an AI with an "y" var, we check if there is a decimal point in the next *MAXLENGTH* characters */
                    /* if there is one, we take an extra character */
                    if (yAlreadySet != YStatus.NoVariable)
                    {
                        if (content.Contains(".") || content.Contains(","))
                        {
                            n++;
                            if (n <= nbCharToParse)
                            {
                                /* We take an extra char */
                                content = toParse.Substring(nbCharId, n);
                            }
                        }
                    }
                }
            }

            /* We check for separator */
            var separator = content.IndexOf((char)29);
            if (separator >= 0)
            {
                content = content.Substring(0, separator);
                separatorsFound++;
            }

            if (id != null)
            {
                Debug.Assert(realNameId != null);

                /* We check the conformity */
                if (!this.CheckConformity(ref content, id, realNameId))
                {
                    return false;
                }

                /* We check the checksum */
                if (!this.CheckChecksum(ref content, id, realNameId, ref checksumAdded))
                {
                    return false;
                }

                /* We check the vars */
                if (!this.CheckVars(ref content, ref id, yAlreadySet, ref decimalPointRemoved))
                {
                    return false;
                }
            }

            this.identifiersId.Add(id);
            this.identifiersContent.Add(content);

            var nbCharLastContent = (((content.Length + nbCharId) - checksumAdded) + decimalPointRemoved) + separatorsFound;
            if (nbCharToParse - nbCharLastContent > 0)
            {
                /* If there is more than one content in this array, we parse again */
                var otherContent = toParse.Substring(nbCharLastContent, nbCharToParse - nbCharLastContent);
                var nbCharOtherContent = otherContent.Length;

                if (otherContent[0] == (char)29)
                {
                    otherContent = otherContent.Substring(1);
                    nbCharOtherContent--;
                }

                if (nbCharOtherContent > 0)
                {
                    text = otherContent;
                    return this.ParseContent(text);
                }
            }

            return true;
        }

        /// <summary>
        /// Checks if an id exists.
        /// </summary>
        /// <param name="id">The AI.</param>
        /// <param name="yAlreadySet">Y Status.</param>
        /// <param name="realNameId">The real AI.</param>
        /// <returns>True if the AI exists.</returns>
        private bool IdExists(string id, ref YStatus yAlreadySet, [NotNullWhen(true)] ref string? realNameId)
        {
            var yFound = id.Length > 3 && id[3] == 'y';
            var idVarAdded = id.Substring(0, id.Length - 1) + "y";

            if (this.identifiersAi != null)
            {
                if (this.identifiersAi.ContainsKey(id))
                {
                    if (yFound)
                    {
                        yAlreadySet = YStatus.NotSet;
                    }

                    realNameId = id;
                    return true;
                }
                else if (!yFound && this.identifiersAi.ContainsKey(idVarAdded))
                {
                    /* if the id don't exist, we try to find this id with "y" at the last char */
                    yAlreadySet = YStatus.AlreadySet;
                    realNameId = idVarAdded;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Finds ID with formatted content.
        /// </summary>
        /// <param name="id">The AI.</param>
        /// <param name="yAlreadySet">Y Status.</param>
        /// <param name="realNameId">The real AI.</param>
        /// <returns>The ID if found.</returns>
        private string? FindIdFormatted(string id, ref YStatus yAlreadySet, ref string? realNameId)
        {
            var pos = id.IndexOf(')');
            if (pos == -1)
            {
                throw new BCGParseException("gs1128", "Identifiers must have no more than 4 characters.");
            }
            else
            {
                if (pos < 3)
                {
                    throw new BCGParseException("gs1128", "Identifiers must have at least 2 characters.");
                }

                id = id.Substring(1, pos - 1);
                if (this.IdExists(id, ref yAlreadySet, ref realNameId))
                {
                    return id;
                }

                if (this.allowsUnknownIdentifier == false)
                {
                    throw new BCGParseException("gs1128", $"The identifier {id} doesn't exist. Have you installed the default AI with \"InstallDefaultApplicationIdentifiers()\"? Or allow unknown identifiers with \"SetAllowsUnknownIdentifier(true)\".");
                }

                return null;
            }
        }

        /// <summary>
        /// Finds ID with non-formatted content.
        /// </summary>
        /// <param name="id">The AI.</param>
        /// <param name="yAlreadySet">Y Status.</param>
        /// <param name="realNameId">The real AI.</param>
        /// <returns>The ID if found.</returns>
        private string? FindIdNotFormatted(string id, ref YStatus yAlreadySet, ref string? realNameId)
        {
            var tofind = id;

            while (tofind.Length >= 2)
            {
                if (this.IdExists(tofind, ref yAlreadySet, ref realNameId))
                {
                    return tofind;
                }
                else
                {
                    tofind = tofind.Substring(0, tofind.Length - 1);
                }
            }

            if (this.allowsUnknownIdentifier == false)
            {
                throw new BCGParseException("gs1128", $"Error in formatting, can't find an identifier. Have you installed the default AI with \"InstallDefaultApplicationIdentifiers()\"? Or allow unknown identifiers with \"SetAllowsUnknownIdentifier(true)\".");
            }

            return null;
        }

        /// <summary>
        /// Checks confirmity of the content.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="id">The AI.</param>
        /// <param name="realNameId">The real AI.</param>
        /// <returns>True if valid.</returns>
        private bool CheckConformity(ref string content, string id, string realNameId)
        {
            Debug.Assert(this.identifiersAi != null);
            switch (this.identifiersAi[realNameId].KindOfData)
            {
                case KindOfData.Numeric:
                    content = content.Replace(',', '.');
                    if (!Regex.IsMatch(content, "^[0-9.]+$"))
                    {
                        throw new BCGParseException("gs1128", $"The value of \"{id}\" must be numerical.");
                    }

                    break;
                case KindOfData.DateTime:
                    var validDateTime = true;
                    if (Regex.IsMatch(content, "^[0-9]{8,12}$"))
                    {
                        var year = content.Substring(0, 2);
                        var month = content.Substring(2, 2);
                        var day = content.Substring(4, 2);
                        var hour = content.Substring(6, 2);
                        var minute = content.Length >= 10 ? content.Substring(8, 2) : null;
                        var second = content.Length >= 12 ? content.Substring(10, 2) : null;

                        /* day can be 00 if we only need month and year */
                        if (int.Parse(month) < 1
                            || int.Parse(month) > 12
                            || int.Parse(day) > 31
                            || int.Parse(hour) > 23
                            || (minute != null && int.Parse(minute) > 59)
                            || (second != null && int.Parse(second) > 59)
                        )
                        {
                            validDateTime = false;
                        }
                    }
                    else
                    {
                        validDateTime = false;
                    }

                    if (!validDateTime)
                    {
                        throw new BCGParseException("gs1128", $"The value of \"{id}\" must be in YYMMDDHHMMSS format. Some AI might not allow seconds.");
                    }

                    break;
                case KindOfData.Date:
                    var validDate = true;
                    if (Regex.IsMatch(content, "^[0-9]{6}$"))
                    {
                        var year = content.Substring(0, 2);
                        var month = content.Substring(2, 2);
                        var day = content.Substring(4, 2);

                        /* day can be 00 if we only need month and year */
                        if (int.Parse(month) < 1 || int.Parse(month) > 12 || int.Parse(day) < 0 || int.Parse(day) > 31)
                        {
                            validDate = false;
                        }
                    }
                    else
                    {
                        validDate = false;
                    }

                    if (!validDate)
                    {
                        throw new BCGParseException("gs1128", $"The value of \"{id}\" must be in YYMMDD format.");
                    }

                    break;
            }

            // We check the length of the content
            var nbCharContent = content.Length;
            var checksumChar = 0;
            var minlengthContent = this.identifiersAi[realNameId].MinLength;
            var maxlengthContent = this.identifiersAi[realNameId].MaxLength;

            if (this.identifiersAi[realNameId].Checksum)
            {
                checksumChar++;
            }

            if (nbCharContent < (minlengthContent - checksumChar))
            {
                if (minlengthContent == maxlengthContent)
                {
                    throw new BCGParseException("gs1128", $"The value of \"{id}\" must contain {minlengthContent} character(s).");
                }
                else
                {
                    throw new BCGParseException("gs1128", $"The value of \"{id}\" must contain between {minlengthContent} and {maxlengthContent} character(s).");
                }
            }

            return true;
        }

        /// <summary>
        /// Verifies the checksum.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="id">The AI.</param>
        /// <param name="realNameId">The real AI.</param>
        /// <param name="checksumAdded">The checksum was added.</param>
        /// <returns>True if valid.</returns>
        private bool CheckChecksum(ref string content, string id, string realNameId, ref int checksumAdded)
        {
            Debug.Assert(this.identifiersAi != null);
            if (this.identifiersAi[realNameId].Checksum)
            {
                var nbCharContent = content.Length;
                var minlengthContent = this.identifiersAi[realNameId].MinLength;
                if (nbCharContent == (minlengthContent - 1))
                {
                    /* we need to calculate the checksum */
                    content += GetAiContentChecksum(content);
                    checksumAdded++;
                }
                else if (nbCharContent == minlengthContent)
                {
                    /* we need to check the checksum */
                    var checksum = GetAiContentChecksum(content.Substring(0, content.Length - 1));
                    if (!int.TryParse(content[nbCharContent - 1].ToString(), out var potentialChecksum) || potentialChecksum != checksum)
                    {
                        throw new BCGParseException("gs1128", $"The checksum of \"({id}) {content}\" must be: {checksum}");
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Checks vars "y".
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="id">The AI.</param>
        /// <param name="yAlreadySet">Y Status.</param>
        /// <param name="decimalPointRemoved">The decimal point was removed.</param>
        /// <returns>True if valid.</returns>
        private bool CheckVars(ref string content, ref string id, YStatus yAlreadySet, ref int decimalPointRemoved)
        {
            var nbCharContent = content.Length;
            /* We check for "y" var in AI */
            if (yAlreadySet == YStatus.AlreadySet)
            {
                /* We'll check if we have a decimal point */
                if (content.Contains("."))
                {
                    throw new BCGParseException("gs1128", "If you do not use any \"y\" variable, you have to insert a whole number.");
                }
            }
            else if (yAlreadySet != YStatus.NoVariable)
            {
                /* We need to replace the "y" var with the position of the decimal point */
                var pos = content.IndexOf(".");
                if (pos == -1)
                {
                    pos = nbCharContent - 1;
                }

                id = id.ToLower().Replace("y", (nbCharContent - (pos + 1)).ToString());
                content = content.Replace(".", ""); ;
                decimalPointRemoved++;
            }

            return true;
        }

        /// <summary>
        /// Checksum Mod10.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <returns>The checksum.</returns>
        private static int CalculateChecksumMod10(string content)
        {
            // Calculating Checksum
            // Consider the right-most digit of the message to be in an "odd" position,
            // and assign odd/even to each character moving from right to left
            // Odd Position = 3, Even Position = 1
            // Multiply it by the number
            // Add all of that and do 10-(?mod10)
            var odd = true;
            var checksumValue = 0;
            var c = content.Length;
            int multiplier;

            for (var i = c; i > 0; i--)
            {
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

                checksumValue += (int.Parse(content[i - 1].ToString()) * multiplier);
            }

            return (10 - checksumValue % 10) % 10;
        }
    }
}
