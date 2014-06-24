namespace Nancy.Hal
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// this is https://github.com/tavis-software/UriTemplates
    /// an RFC6570-compliant level-4 UriTemplate handler
    /// </summary>
    public class UriTemplate
    {
        const string _UriReservedSymbols = ":/?#[]@!$&'()*+,;=";
        const string _UriUnreservedSymbols = "-._~";

        static Dictionary<char, OperatorInfo> _Operators = new Dictionary<char, OperatorInfo>() {
                                                                                                    {'\0', new OperatorInfo {Default = true, First = "", Seperator = ',', Named = false, IfEmpty = "",AllowReserved = false}},
                                                                                                    {'+', new OperatorInfo {Default = false, First = "", Seperator = ',', Named = false, IfEmpty = "",AllowReserved = true}},
                                                                                                    {'.', new OperatorInfo {Default = false, First = ".", Seperator = '.', Named = false, IfEmpty = "",AllowReserved = false}},
                                                                                                    {'/', new OperatorInfo {Default = false, First = "/", Seperator = '/', Named = false, IfEmpty = "",AllowReserved = false}},
                                                                                                    {';', new OperatorInfo {Default = false, First = ";", Seperator = ';', Named = true, IfEmpty = "",AllowReserved = false}},
                                                                                                    {'?', new OperatorInfo {Default = false, First = "?", Seperator = '&', Named = true, IfEmpty = "=",AllowReserved = false}},
                                                                                                    {'&', new OperatorInfo {Default = false, First = "&", Seperator = '&', Named = true, IfEmpty = "=",AllowReserved = false}},
                                                                                                    {'#', new OperatorInfo {Default = false, First = "#", Seperator = ',', Named = false, IfEmpty = "",AllowReserved = true}}
                                                                                                };

        readonly string _template;
        readonly Dictionary<string, object> _Parameters = new Dictionary<string, object>();

        enum States
        {
            CopyingLiterals,
            ParsingExpression
        }

        bool _ErrorDetected = false;
        StringBuilder _Result;
        List<string> _ParameterNames;

        public UriTemplate(string template)
        {
            this._template = template;
        }


        public void SetParameter(string name, object value)
        {
            this._Parameters[name] = value;
        }

        public void SetParameter(string name, string value)
        {
            this._Parameters[name] = value;
        }

        public void SetParameter(string name, IEnumerable<string> value)
        {
            this._Parameters[name] = value;
        }

        public void SetParameter(string name, IDictionary<string, string> value)
        {
            this._Parameters[name] = value;
        }


        public IEnumerable<string> GetParameterNames()
        {
            var parameterNames = new List<string>();
            this._ParameterNames = parameterNames;
            this.Resolve();
            this._ParameterNames = null;
            return parameterNames;
        }

        public string Resolve()
        {
            var currentState = States.CopyingLiterals;
            this._Result = new StringBuilder();
            StringBuilder currentExpression = null;
            foreach (var character in this._template.ToCharArray())
            {
                switch (currentState)
                {
                    case States.CopyingLiterals:
                        if (character == '{')
                        {
                            currentState = States.ParsingExpression;
                            currentExpression = new StringBuilder();
                        }
                        else if (character == '}')
                        {
                            throw new ArgumentException("Malformed template, unexpected } : " + this._Result.ToString());
                        }
                        else
                        {
                            this._Result.Append(character);
                        }
                        break;
                    case States.ParsingExpression:
                        if (character == '}')
                        {
                            this.ProcessExpression(currentExpression);

                            currentState = States.CopyingLiterals;
                        }
                        else
                        {
                            currentExpression.Append(character);
                        }

                        break;
                }
            }
            if (currentState == States.ParsingExpression)
            {
                this._Result.Append("{");
                this._Result.Append(currentExpression.ToString());

                throw new ArgumentException("Malformed template, missing } : " + this._Result.ToString());
            }

            if (this._ErrorDetected)
            {
                throw new ArgumentException("Malformed template : " + this._Result.ToString());
            }
            return this._Result.ToString();
        }

        void ProcessExpression(StringBuilder currentExpression)
        {

            if (currentExpression.Length == 0)
            {
                this._ErrorDetected = true;
                this._Result.Append("{}");
                return;
            }

            OperatorInfo op = GetOperator(currentExpression[0]);

            var firstChar = op.Default ? 0 : 1;


            var varSpec = new VarSpec(op);
            for (int i = firstChar; i < currentExpression.Length; i++)
            {
                char currentChar = currentExpression[i];
                switch (currentChar)
                {
                    case '*':
                        varSpec.Explode = true;
                        break;
                    case ':': // Parse Prefix Modifier
                        var prefixText = new StringBuilder();
                        currentChar = currentExpression[++i];
                        while (currentChar >= '0' && currentChar <= '9' && i < currentExpression.Length)
                        {
                            prefixText.Append(currentChar);
                            i++;
                            if (i < currentExpression.Length) currentChar = currentExpression[i];
                        }
                        varSpec.PrefixLength = int.Parse(prefixText.ToString());
                        i--;
                        break;
                    case ',':
                        var success = this.ProcessVariable(varSpec);
                        bool isFirst = varSpec.First;
                        // Reset for new variable
                        varSpec = new VarSpec(op);
                        if (success || !isFirst) varSpec.First = false;

                        break;


                    default:
                        if (this.IsVarNameChar(currentChar))
                        {
                            varSpec.VarName.Append(currentChar);
                        }
                        else
                        {
                            this._ErrorDetected = true;
                        }
                        break;
                }
            }
            this.ProcessVariable(varSpec);

        }

        bool ProcessVariable(VarSpec varSpec)
        {
            var varname = varSpec.VarName.ToString();
            if (this._ParameterNames != null) this._ParameterNames.Add(varname);

            if (!this._Parameters.ContainsKey(varname)
                || this._Parameters[varname] == null
                || (this._Parameters[varname] is IList && ((IList)this._Parameters[varname]).Count == 0)
                || (this._Parameters[varname] is IDictionary && ((IDictionary)this._Parameters[varname]).Count == 0))
                return false;

            if (varSpec.First)
            {
                this._Result.Append(varSpec.OperatorInfo.First);
            }
            else
            {
                this._Result.Append(varSpec.OperatorInfo.Seperator);
            }

            object value = this._Parameters[varname];

            // Handle Strings
            if (value is string)
            {
                var stringValue = (string)value;
                if (varSpec.OperatorInfo.Named)
                {
                    this.AppendName(varname, varSpec.OperatorInfo, string.IsNullOrEmpty(stringValue));
                }
                this.AppendValue(stringValue, varSpec.PrefixLength, varSpec.OperatorInfo.AllowReserved);
            }
            else
            {
                // Handle Lists
                var list = value as IEnumerable<string>;
                if (list != null)
                {
                    if (varSpec.OperatorInfo.Named && !varSpec.Explode) // exploding will prefix with list name
                    {
                        this.AppendName(varname, varSpec.OperatorInfo, list.Count() == 0);
                    }

                    this.AppendList(varSpec.OperatorInfo, varSpec.Explode, varname, list);
                }
                else
                {

                    // Handle associative arrays
                    var dictionary = value as IDictionary<string, string>;
                    if (dictionary != null)
                    {
                        if (varSpec.OperatorInfo.Named && !varSpec.Explode) // exploding will prefix with list name
                        {
                            this.AppendName(varname, varSpec.OperatorInfo, dictionary.Count() == 0);
                        }
                        this.AppendDictionary(varSpec.OperatorInfo, varSpec.Explode, dictionary);
                    }

                }

            }
            return true;
        }


        void AppendDictionary(OperatorInfo op, bool explode, IDictionary<string, string> dictionary)
        {
            foreach (string key in dictionary.Keys)
            {
                this._Result.Append(key);
                if (explode) this._Result.Append('=');
                else this._Result.Append(',');
                this.AppendValue(dictionary[key], 0, op.AllowReserved);

                if (explode)
                {
                    this._Result.Append(op.Seperator);
                }
                else
                {
                    this._Result.Append(',');
                }
            }
            if (dictionary.Count() > 0)
            {
                this._Result.Remove(this._Result.Length - 1, 1);
            }
        }

        void AppendList(OperatorInfo op, bool explode, string variable, IEnumerable<string> list)
        {
            foreach (string item in list)
            {
                if (op.Named && explode)
                {
                    this._Result.Append(variable);
                    this._Result.Append("=");
                }
                this.AppendValue(item, 0, op.AllowReserved);

                this._Result.Append(explode ? op.Seperator : ',');
            }
            if (list.Count() > 0)
            {
                this._Result.Remove(this._Result.Length - 1, 1);
            }
        }

        void AppendValue(string value, int prefixLength, bool allowReserved)
        {

            if (prefixLength != 0)
            {
                if (prefixLength < value.Length)
                {
                    value = value.Substring(0, prefixLength);
                }
            }

            this._Result.Append(Encode(value, allowReserved));

        }

        void AppendName(string variable, OperatorInfo op, bool valueIsEmpty)
        {
            this._Result.Append(variable);
            if (valueIsEmpty)
            {
                this._Result.Append(op.IfEmpty);
            }
            else
            {
                this._Result.Append("=");
            }
        }


        bool IsVarNameChar(char c)
        {
            return ((c >= 'A' && c <= 'z') //Alpha
                    || (c >= '0' && c <= '9') // Digit
                    || c == '_'
                    || c == '%'
                    || c == '.');
        }

        static string Encode(string p, bool allowReserved)
        {

            var result = new StringBuilder();
            foreach (char c in p)
            {
                if ((c >= 'A' && c <= 'z') //Alpha
                    || (c >= '0' && c <= '9') // Digit
                    || _UriUnreservedSymbols.IndexOf(c) != -1
                    // Unreserved symbols  - These should never be percent encoded
                    || (allowReserved && _UriReservedSymbols.IndexOf(c) != -1))
                    // Reserved symbols - should be included if requested (+)
                {
                    result.Append(c);
                }
                else
                {
#if PCL
                         result.Append(HexEscape(c));  
#else
                    var s = c.ToString();

                    var chars = s.Normalize(NormalizationForm.FormC).ToCharArray();
                    foreach (var ch in chars)
                    {
                        result.Append(HexEscape(ch));
                    }
#endif

                }
            }

            return result.ToString();


        }

        public static string HexEscape(char c)
        {
            var esc = new char[3];
            esc[0] = '%';
            esc[1] = HexDigits[(((int)c & 240) >> 4)];
            esc[2] = HexDigits[((int)c & 15)];
            return new string(esc);
        }

        static readonly char[] HexDigits = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };

        static OperatorInfo GetOperator(char operatorIndicator)
        {
            OperatorInfo op;
            switch (operatorIndicator)
            {

                case '+':
                case ';':
                case '/':
                case '#':
                case '&':
                case '?':
                case '.':
                    op = _Operators[operatorIndicator];
                    break;

                default:
                    op = _Operators['\0'];
                    break;
            }
            return op;
        }


        public class OperatorInfo
        {
            public bool Default { get; set; }
            public string First { get; set; }
            public char Seperator { get; set; }
            public bool Named { get; set; }
            public string IfEmpty { get; set; }
            public bool AllowReserved { get; set; }

        }

        public class VarSpec
        {
            readonly OperatorInfo _operatorInfo;
            public StringBuilder VarName = new StringBuilder();
            public bool Explode = false;
            public int PrefixLength = 0;
            public bool First = true;

            public VarSpec(OperatorInfo operatorInfo)
            {
                this._operatorInfo = operatorInfo;
            }

            public OperatorInfo OperatorInfo
            {
                get { return this._operatorInfo; }
            }
        }
    }
}