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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleCmd;

namespace Tests
{
 
    [TestClass]
    public class ParseTests
    {
        public SimpleCmdParser MakeParser()
        {
            SimpleCmdParser parser = new SimpleCmdParser();
            parser
                .Add("a")
                .Add("b")
                .Add("long")
                .Add("long-2")
                .Add("long-short", "i")
                .Add("long-short-2", "j")
                .Add("m", true)
                .Add("n", true)
                .Add("long-arg", true)
                .Add("long-arg-2", true)
                .Add("long-short-arg", "x", true)
                .Add("long-short-arg-2", "y", true)
                ;
            return parser;
        }

        [TestMethod]
        public void TestArguments()
        {
            SimpleCmdParser parser = MakeParser();
            string[] args = new string[] { "--long-short", "unnamed1", "--long-arg", "named", "unnamed2", "unnamed3" };

            SimpleCmdResults results = parser.Parse(args);
            string[] parsed = results.GetAllArguments();

            Assert.AreEqual(false, results.HasErrors());
            Assert.AreEqual(3, parsed.Length);
            Assert.AreEqual("unnamed1", parsed[0]);
            Assert.AreEqual("unnamed2", parsed[1]);
            Assert.AreEqual("unnamed3", parsed[2]);
            Assert.AreEqual("unnamed1", results["@1"]);
            Assert.AreEqual("unnamed2", results["@2"]);
            Assert.AreEqual("unnamed3", results["@3"]);
        }

        [TestMethod]
        public void TestNull()
        {
            SimpleCmdParser parser = MakeParser();
            SimpleCmdResults results = parser.Parse(null);
            string[] parsed = results.GetAllArguments();

            Assert.AreEqual(false, results.HasErrors());
            Assert.AreEqual(0, parsed.Length);
        }

        [TestMethod]
        public void TestNoArguments()
        {
            SimpleCmdParser parser = MakeParser();
            string[] args = new string[] { "--long-short", "--long-arg", "named", "-a" };

            SimpleCmdResults results = parser.Parse(args);
            string[] parsed = results.GetAllArguments();

            Assert.AreEqual(false, results.HasErrors());
            Assert.AreEqual(0, parsed.Length);
        }

        [TestMethod]
        public void TestLongOptionValues()
        {
            SimpleCmdParser parser = MakeParser();
            string[] args = new string[] { "unnamed1", "--long-arg=named1", "unnamed2", "--long-arg-2", "named2", "unnamed3" };

            SimpleCmdResults results = parser.Parse(args);
            Assert.AreEqual("named1", results["long-arg"]);
            Assert.AreEqual("named2", results["long-arg-2"]);
        }

        [TestMethod]
        public void TestLongOptionValuesWithSeparator()
        {
            SimpleCmdParser parser = MakeParser();
            string[] args = new string[] { "unnamed1", "--long-arg=named1=value1", "unnamed2", "--long-arg-2", "named2=value2", "unnamed3" };

            SimpleCmdResults results = parser.Parse(args);
            Assert.AreEqual("named1=value1", results["long-arg"]);
            Assert.AreEqual("named2=value2", results["long-arg-2"]);
        }

        [TestMethod]
        public void TestNullLongOptionArgumentSeparator()
        {
            SimpleCmdParser parser = MakeParser();
            parser.LongOptionArgumentSeparator = null;
            string[] args = new string[] { "unnamed1", "--long-arg=named1", "unnamed2", "--long-arg-2", "named2", "unnamed3" };

            SimpleCmdResults results = parser.Parse(args);
            Assert.AreEqual(null, results["long-arg"]);
        }

        [TestMethod]
        public void TestShortOptionsValues()
        {
            SimpleCmdParser parser = MakeParser();
            string[] args = new string[] { "-mnamed1", "unnamed1", "-n", "named2", "unnamed2", "unnamed3" };
            SimpleCmdResults results = parser.Parse(args);
            Assert.AreEqual(false, results.HasErrors());
            Assert.AreEqual("named1", results["m"]);
            Assert.AreEqual("named2", results["n"]);
        }

        [TestMethod]
        public void TestLongShortOptions()
        {
            SimpleCmdParser parser = MakeParser();
            string[] args = new string[] { "--long-short", "-j", "unnamed1" };
            SimpleCmdResults results = parser.Parse(args);
            Assert.AreEqual(false, results.HasErrors());
            Assert.AreEqual(true, results["long-short"]);
            Assert.AreEqual(true, results["long-short-2"]);
            Assert.AreEqual(null, results["j"]);
        }

        [TestMethod]
        public void TestMultipleShortOptionsNoValues()
        {
            SimpleCmdParser parser = MakeParser();
            parser.MultipleShortOptions = true;
            string[] args = new string[] { "-ab", "unnamed1", "unnamed2", "unnamed3" };
            SimpleCmdResults results = parser.Parse(args);
            Assert.AreEqual(false, results.HasErrors());
            Assert.AreEqual(true, results["a"]);
            Assert.AreEqual(true, results["b"]);
        }

        [TestMethod]
        public void TestMultipleShortOptionsWithValues()
        {
            SimpleCmdParser parser = MakeParser();
            parser.MultipleShortOptions = true;
            string[] args = new string[] { "-amvalue", "unnamed1", "unnamed2", "unnamed3" };
            SimpleCmdResults results = parser.Parse(args);
            Assert.AreEqual(false, results.HasErrors());
            Assert.AreEqual(true, results["a"]);
            Assert.AreEqual("value", results["m"]);
        }

        [TestMethod]
        public void TestStopOptionParseTrigger()
        {
            SimpleCmdParser parser = MakeParser();
            string[] args = new string[] { "unnamed1", "-a", "--long-arg=named1", "unnamed2", "--", "-z", "--not-an-option" };
            SimpleCmdResults results = parser.Parse(args);
            Assert.AreEqual(false, results.HasErrors());
            Assert.AreEqual(true, results["a"]);
            Assert.AreEqual("named1", results["long-arg"]);
            Assert.AreEqual(3, results.StopOptionParsePosition);
            Assert.AreEqual("-z", results["@3"]);
            Assert.AreEqual("--not-an-option", results["@4"]);
        }

        [TestMethod]
        public void TestStopOptionParsePosition()
        {
            SimpleCmdParser parser = MakeParser();
            string[] args = new string[] { "unnamed1", "-a", "--long-arg=named1", "unnamed2", "--" };
            SimpleCmdResults results = parser.Parse(args);
            Assert.AreEqual(false, results.HasErrors());
            Assert.AreEqual(0, results.StopOptionParsePosition);
        }

        [TestMethod]
        public void TestStopOnFirstNonOption()
        {
            SimpleCmdParser parser = MakeParser();
            parser.StopOnFirstNonOption = true;
            string[] args = new string[] { "-a", "--long-arg=named1", "unnamed", "-z", "--not-an-option" };
            SimpleCmdResults results = parser.Parse(args);
            Assert.AreEqual(false, results.HasErrors());
            Assert.AreEqual(true, results["a"]);
            Assert.AreEqual("named1", results["long-arg"]);
            Assert.AreEqual(2, results.StopOptionParsePosition);
            Assert.AreEqual("-z", results["@2"]);
            Assert.AreEqual("--not-an-option", results["@3"]);
        }

        [TestMethod]
        public void FailIfOptionNotFound()
        {
            SimpleCmdParser parser = MakeParser();
            string[] args = new string[] { "-z", "--invalid-arg=named1", "--invalid-long" };
            SimpleCmdResults results = parser.Parse(args);
            Tuple<ParseErrorType, string, string>[] errors = results.GetErrors();

            Assert.AreEqual(true, results.HasErrors());
            Assert.AreEqual(3, errors.Length);
            Assert.AreEqual(ParseErrorType.InvalidOption, errors[0].Item1);
            Assert.AreEqual("z", errors[0].Item2);
            Assert.AreEqual(ParseErrorType.InvalidOption, errors[1].Item1);
            Assert.AreEqual("invalid-arg", errors[1].Item2);
            Assert.AreEqual(ParseErrorType.InvalidOption, errors[2].Item1);
            Assert.AreEqual("invalid-long", errors[2].Item2);
        }

        [TestMethod]
        public void FailIfDuplicate()
        {
            SimpleCmdParser parser = MakeParser();
            string[] args = new string[] { "-a", "-a" };
            SimpleCmdResults results = parser.Parse(args);
            Tuple<ParseErrorType, string, string>[] errors = results.GetErrors();

            Assert.AreEqual(true, results.HasErrors());
            Assert.AreEqual(ParseErrorType.DuplicateOptions, errors[0].Item1);
        }

        [TestMethod]
        public void FailOnMissingOptionValue()
        {
            SimpleCmdParser parser = MakeParser();
            string[] args = new string[] { "-m", "--long-arg" };
            SimpleCmdResults results = parser.Parse(args);
            Tuple<ParseErrorType, string, string>[] errors = results.GetErrors();

            Assert.AreEqual(true, results.HasErrors());
            Assert.AreEqual(2, errors.Length);
            Assert.AreEqual(ParseErrorType.MissingOptionValue, errors[0].Item1);
            Assert.AreEqual(ParseErrorType.MissingOptionValue, errors[1].Item1);
        }

        [TestMethod]
        public void FailOnUnexpectedOptionValue()
        {
            SimpleCmdParser parser = MakeParser();
            parser.MultipleShortOptions = false;
            string[] args = new string[] { "-awxy", "--long=invalid" };
            SimpleCmdResults results = parser.Parse(args);
            Tuple<ParseErrorType, string, string>[] errors = results.GetErrors();

            Assert.AreEqual(true, results.HasErrors());
            Assert.AreEqual(2, errors.Length);
            Assert.AreEqual(ParseErrorType.InvalidOption, errors[0].Item1);
            Assert.AreEqual("wxy", errors[0].Item2);
            Assert.AreEqual(ParseErrorType.UnexpectedOptionValue, errors[1].Item1);
        }
    }
}
