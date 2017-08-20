/*
 * SimpleCmd
 *
 * Copyright (C) Kelvin Mo 2017
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 *
 * 1. Redistributions of source code must retain the above copyright
 *    notice, this list of conditions and the following disclaimer.
 *
 * 2. Redistributions in binary form must reproduce the above
 *    copyright notice, this list of conditions and the following
 *    disclaimer in the documentation and/or other materials provided
 *    with the distribution.
 *
 * 3. The name of the author may not be used to endorse or promote
 *    products derived from this software without specific prior
 *    written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE AUTHOR ``AS IS'' AND ANY EXPRESS
 * OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
 * DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE
 * GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER
 * IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR
 * OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN
 * IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */


using System;
using System.Collections.Generic;

namespace SimpleCmd
{
    public enum OptionStyle
    {
        Default,
        Windows,
        Powershell
    }

    public enum ParseErrorType
    {
        InvalidOption,
        DuplicateOptions,
        MissingOptionValue,
        UnexpectedOptionValue
    }

    public class SimpleCmdParser
    {
        #region --- Fields
        private string _longOptionPrefix = "--";

        private string _shortOptionPrefix = "-";

        private string _longOptionArgumentSeparator = "=";

        private string _stopOptionParseTrigger = "--";

        private bool _multipleShortOptions = true;

        private bool _stopOnFirstNonOption = false;

        private IDictionary<string, SimpleCmdOption> options = new Dictionary<string, SimpleCmdOption>();

        #endregion

        #region --- Properties

        /// <summary>
        /// The prefix to be used for the default (long) option processing (e.g. <code>--</code>)
        /// </summary>
        public string LongOptionPrefix
        {
            get { return _longOptionPrefix; }
            set { _longOptionPrefix = value; }
        }

        /// <summary>
        /// The separator used in long option processing to separate out the name of the
        /// option and its value (e.g. <code>=</code>).  Can be set to null to disable
        /// separation (i.e. the value must be included as a separate arugment)
        /// </summary>
        public string LongOptionArgumentSeparator
        {
            get { return _longOptionArgumentSeparator; }
            set { _longOptionArgumentSeparator = value; }
        }

        /// <summary>
        /// The prefix to be used for "short option" processing (e.g. <code>-</code>).  Can be
        /// set to null to disable short option processing.
        /// </summary>
        public string ShortOptionPrefix
        {
            get { return _shortOptionPrefix; }
            set { _shortOptionPrefix = value; }
        }

        /// <summary>
        /// The argument that, if encountered, will stop option parsing (e.g. <code>--</code>)
        /// for the remainder of the argument string.
        /// </summary>
        public string StopOptionParseTrigger
        {
            get { return _stopOptionParseTrigger; }
            set { _stopOptionParseTrigger = value; }
        }

        /// <summary>
        /// Whether a short option string (e.g. <code>-abcde</code>) will be decomposed into
        /// individual short options (e.g. <code>-a -b -c -d -e</code>).
        /// </summary>
        /// <remarks>
        /// If one of the short options included requires a value, then the remainder
        /// of the argument is interpreted as the value for the option.  For example, if
        /// <code>c</code> above requires a value, then the string will be decomposed
        /// into <code>-a -b -c de</code>
        /// </remarks>
        public bool MultipleShortOptions
        {
            get { return _multipleShortOptions; }
            set { _multipleShortOptions = value; }
        }

        /// <summary>
        /// Whether to stop option parsing when the first non-option argument is encountered.
        /// </summary>
        public bool StopOnFirstNonOption
        {
            get { return _stopOnFirstNonOption; }
            set { _stopOnFirstNonOption = value; }
        }

        #endregion

        #region --- Build methods

        /// <summary>
        /// Adds an option
        /// </summary>
        /// <param name="option">the option to add</param>
        /// <returns>itself</returns>
        public SimpleCmdParser Add(SimpleCmdOption option)
        {
            options[option.Name1] = option;
            if (option.Name2 != null) options[option.Name2] = option;
            return this;
        }

        /// <summary>
        /// Adds an option
        /// </summary>
        /// <param name="name1">the long name of the option</param>
        /// <param name="name2">the short name of the option</param>
        /// <param name="requiresValue">true if a value is required</param>
        /// <returns>itself</returns>
        public SimpleCmdParser Add(string name1, string name2, bool requiresValue)
        {
            return Add(new SimpleCmdOption(name1, name2, requiresValue));
        }

        /// <summary>
        /// Adds an option
        /// </summary>
        /// <param name="name">the name of the option</param>
        /// <param name="requiresValue">true if a value is required</param>
        /// <returns>itself</returns>
        public SimpleCmdParser Add(string name, bool requiresValue)
        {
            return Add(new SimpleCmdOption(name, requiresValue));
        }

        /// <summary>
        /// Adds an option
        /// </summary>
        /// <param name="name1">the long name of the option</param>
        /// <param name="name2">the short name of the option</param>
        /// <returns>itself</returns>
        public SimpleCmdParser Add(string name1, string name2)
        {
            return Add(new SimpleCmdOption(name1, name2));
        }

        /// <summary>
        /// Adds an option
        /// </summary>
        /// <param name="name">the name of the option</param>
        /// <returns>itself</returns>
        public SimpleCmdParser Add(string name)
        {
            return Add(new SimpleCmdOption(name));
        }

        /// <summary>
        /// Convenience methods to set various parser options
        /// </summary>
        /// <param name="style"></param>
        /// <returns></returns>
        public SimpleCmdParser SetStyle(OptionStyle style)
        {
            switch (style)
            {
                case OptionStyle.Default:
                    LongOptionPrefix = "--";
                    LongOptionArgumentSeparator = "=";
                    ShortOptionPrefix = "-";
                    StopOptionParseTrigger = "--";
                    MultipleShortOptions = true;
                    StopOnFirstNonOption = false;
                    break;
                case OptionStyle.Windows:
                    LongOptionPrefix = "/";
                    LongOptionArgumentSeparator = ":";
                    ShortOptionPrefix = null;
                    StopOptionParseTrigger = null;
                    MultipleShortOptions = false;
                    StopOnFirstNonOption = false;
                    break;
                case OptionStyle.Powershell:
                    LongOptionPrefix = "-";
                    LongOptionArgumentSeparator = null;
                    ShortOptionPrefix = null;
                    StopOptionParseTrigger = null;
                    MultipleShortOptions = false;
                    StopOnFirstNonOption = false;
                    break;
            }
            return this;
        }

        #endregion

        /// <summary>
        /// Parse the command line arguments
        /// </summary>
        /// <param name="args">string array containing command line arguments</param>
        /// <returns>a SimpleCmdResults object</returns>
        public SimpleCmdResults Parse(string[] args)
        {
            bool parseOptions = true;

            string[] longOptionSplitter;
            string arg;
            SimpleCmdOption option;

            string name;
            object value;

            SimpleCmdResults results = new SimpleCmdResults();
            if (args == null) return results;

            if (_longOptionArgumentSeparator != null)
            {
                longOptionSplitter = new string[] { _longOptionArgumentSeparator };
            }
            else
            {
                longOptionSplitter = null;
            }

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == null) continue;

                name = null;
                value = null;

                if (parseOptions && (_stopOptionParseTrigger != null) && (args[i] == _stopOptionParseTrigger))  // stop option parsing (--)
                {
                    parseOptions = false;
                }
                else if (parseOptions && IsLongOption(args[i]))  // long option
                {
                    arg = args[i].Substring(_longOptionPrefix.Length);

                    // Check for separator (e.g. =) and split when required
                    if (_longOptionArgumentSeparator != null)
                    {
                        string[] components = arg.Split(longOptionSplitter, 2, StringSplitOptions.RemoveEmptyEntries);
                        name = components[0];
                        if (components.Length == 2) value = components[1];
                    }
                    else
                    {
                        name = arg;
                    }

                    option = GetOption(name, results);
                    if (option == null) continue;

                    if (option.RequiresValue)
                    {
                        if (value == null)
                        {
                            if (i + 1 < args.Length && IsArgument(args[i + 1]))
                            {
                                value = args[i + 1];
                                i++;
                            }
                            else
                            {
                                results.AddError(ParseErrorType.MissingOptionValue, name);
                                continue;
                            }
                        } // else value has already been set
                    }
                    else if (value != null)
                    {
                        // We have an argument, but we should not be
                        results.AddError(ParseErrorType.UnexpectedOptionValue, name, (string)value);
                    }
                    else
                    {
                        value = true;
                    }
                   
                    results.AddOption(name, value);
                }
                else if (parseOptions && IsShortOption(args[i]))  // short option
                {
                    arg = args[i].Substring(_shortOptionPrefix.Length);

                    int stop = (_multipleShortOptions) ? arg.Length : 1;

                    for (int j = 0; j < stop; j++)
                    {
                        name = arg.Substring(j, 1);
                        option = GetOption(name, results);
                        if (option == null) continue;

                        if (option.RequiresValue)
                        {
                            if (j < arg.Length - 1) // If there are additional characters, set that as the value
                            {
                                value = arg.Substring(j + 1);
                            }
                            else // Otherwise, get the next argument and set the value
                            {
                                if (i + 1 < args.Length && IsArgument(args[i + 1]))
                                {
                                    value = args[i + 1];
                                    i++;
                                }
                                else
                                {
                                    results.AddError(ParseErrorType.MissingOptionValue, name);
                                    continue;
                                }
                            }

                            results.AddOption(option.GetLongName(), value);
                            break;
                        }
                        else
                        {
                            results.AddOption(option.GetLongName(), true);
                            if (!_multipleShortOptions && arg.Length > 1)
                            {
                                results.AddError(ParseErrorType.InvalidOption, arg.Substring(j + 1));
                            }
                        }
                    }
                }
                else  // argument
                {
                    results.AddArgument(args[i]);
                    if (_stopOnFirstNonOption) parseOptions = false;
                }
            }

            return results;
        }

        protected virtual bool IsLongOption(string s)
        {
            if (_longOptionPrefix == null) return false;
            return s.StartsWith(_longOptionPrefix) && (s != _longOptionPrefix);
        }

        protected virtual bool IsShortOption(string s)
        {
            if (_shortOptionPrefix == null) return false;
            return s.StartsWith(_shortOptionPrefix) && !IsLongOption(s) && (s != _shortOptionPrefix);
        }

        protected virtual bool IsArgument(string s)
        {
            if (IsLongOption(s)) return false;
            if (IsShortOption(s)) return false;
            if ((_stopOptionParseTrigger != null) && (s == _stopOptionParseTrigger)) return false;
            return true;
        }

        protected virtual SimpleCmdOption GetOption(string name, SimpleCmdResults results)
        {
            if (!options.ContainsKey(name))
            {
                results.AddError(ParseErrorType.InvalidOption, name);
                return null;
            }
            return options[name];

        }
    }

    public class SimpleCmdOption
    {
        private string _name1;
        private string _name2 = null;
        private bool _requiresValue = false;
        private string _help;
        private string _argumentName;

        public string Name1 { get => _name1; }
        public string Name2 { get => _name2; }
        public bool RequiresValue { get => _requiresValue; set => _requiresValue = value; }
        public string Help { get => _help; set => _help = value; }
        public string ArgumentName { get => _argumentName; set => _argumentName = value; }

        public SimpleCmdOption(string name1, string name2, bool requiresValue)
        {
            if (name1 == null)
            {
                throw new ArgumentException("name1 is required", "name1");
            }
            if (name2 != null)
            {
                // Either name1 or name2 should have length 1
                if (name1.Length > 1 && name2.Length > 1)
                {
                    throw new ArgumentException("Either name1 or name2 must be short option", "name2");
                }
                // Either name1 or name2 should have length greater than 1
                if (name1.Length == 1 && name2.Length == 1)
                {
                    throw new ArgumentException("Either name1 or name2 must be long option", "name2");
                }
            }
            _name1 = name1;
            _name2 = name2;
            _requiresValue = requiresValue;
        }

        public SimpleCmdOption(string name, bool requiresValue) : this(name, null, requiresValue)
        {
        }

        public SimpleCmdOption(string name1, string name2) : this(name1, name2, false)
        {
        }

        public SimpleCmdOption(string name) : this(name, null, false)
        {
        }

        internal virtual string GetLongName()
        {
            if (Name2 == null) return Name1;
            if (Name1.Length == 1) return Name2;
            if (Name2.Length == 1) return Name1;
            return null; // This should never happen
        }
    }

    /// <summary>
    /// Results from the SimpleCmdParser.Parse() method
    /// </summary>
    public class SimpleCmdResults
    {
        private IList<string> args = new List<string>();
        private IDictionary<string, object> options = new Dictionary<string, object>();
        private IList<Tuple<ParseErrorType, string, string>> errors = new List<Tuple<ParseErrorType, string, string>>();

        /// <summary>
        /// Returns the value of an option or an argument based on a specified query string.
        /// </summary>
        /// <remarks>
        /// <para>The query is specified as follows:</para>
        /// <list type="definition">
        /// <item>
        ///     <term>option</term>
        ///     <description>the name of the option.  If the option has both a long name and a 
        ///     short name, only the long name can be specified.</description>
        /// </item>
        /// <item>
        ///     <term>positional argument</term>
        ///     <description>the character <code>@</code>, followed by a number starting from 1.
        ///     For example, to get the second positional argument, use <code>@2</code></description>
        /// </item>
        /// </list>
        /// </remarks>
        /// <param name="query">the query string</param>
        /// <returns>the argument as a string, the value as a string (if the option requires a value), true (if the option does not
        /// require a value) or null (if the option is not set)</returns>
        public object this[string query]
        {
            get { return GetOptionValue(query); }
        }

        /// <summary>
        /// Returns whether an option or argument is set.
        /// </summary>
        /// <param name="query">the query (see the documentation for this[])</param>
        /// <returns>true if the option or argument is set</returns>
        public bool Contains(string query)
        {
            return GetOptionValue(query) != null;
        }

        protected object GetOptionValue(string query)
        {
            if (query.StartsWith("@"))
            {
                if (int.TryParse(query.Substring(1), out int i))
                {
                    return args[i - 1];
                }
                else
                {
                    return null;
                }
            }
            else if (!options.ContainsKey(query))
            {
                return null;
            }
            else
            {
                return options[query];
            }
        }

        /// <summary>
        /// Returns the non-option arguments
        /// </summary>
        /// <returns>a string array containing the non-option arguments</returns>
        public string[] GetAllArguments()
        {
            string[] result = new string[args.Count];
            args.CopyTo(result, 0);
            return result;
        }

        /// <summary>
        /// Returns whether parse errors have been encountered.  If true, the errors
        /// can be obtained from the GetErrors method
        /// </summary>
        /// <returns>true if parse errors have been encountered</returns>
        public bool HasErrors()
        {
            return errors.Count > 0;
        }

        /// <summary>
        /// Returns a list of errors.  Returns an array of 3-tuples.  The first element
        /// contains a value from the ParseErrorType enum containing a description of
        /// the error.  The second element contains the name of the option causing the error,
        /// or null.  The second element contains the value of the option causing the error,
        /// or null.
        /// </summary>
        /// <returns></returns>
        public Tuple<ParseErrorType, string, string>[] GetErrors()
        {
            Tuple<ParseErrorType, string, string>[] result = new Tuple<ParseErrorType, string, string>[errors.Count];
            errors.CopyTo(result, 0);
            return result;
        }

        /// <summary>
        /// Add a named option
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        internal void AddOption(string name, object value)
        {
            if (options.ContainsKey(name))
            {
                // Duplicate, add error instead
                AddError(ParseErrorType.DuplicateOptions, name);
            }
            else
            {
                options[name] = value;
            }
        }

        /// <summary>
        /// Add an unnamed argument
        /// </summary>
        /// <param name="value">the value of the argument</param>
        internal void AddArgument(string value)
        {
            args.Add(value);
        }

        internal void AddError(ParseErrorType type, string name, string value)
        {
            errors.Add(new Tuple<ParseErrorType, string, string>(type, name, value));
        }

        internal void AddError(ParseErrorType type, string name)
        {
            AddError(type, name, null);
        }
    }
}
