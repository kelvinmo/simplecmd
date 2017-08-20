# SimpleCmd

SimpleCmd is a simple, lightweight command-line argument parser for .NET.
It is implemented as a Visual Studio shared project, meaning that you can
incorporate the library into your own projects without compiling a separate
assembly.

## Features

- Support for short (POSIX-style) and long (GNU-style) options
- Simple parser configuration, can be used to support Windows-style
  options
- Single file with no external dependencies

## Installation

SimpleCmd is distributed as a shared project.  This means you cannot compile
it as a standalone assembly---it must be included in another project.

1. In Visual Studio, add the `SimpleCmd` directory to your solution as an
   existing project.
2. In the project which you want to use SimpleCmd, add a reference to the
   SimpleCmd shared project.

## Usage

```csharp
using SimpleCmd;

// 1. Create an instance of the parser
SimpleCmdParser parser = new SimpleCmdParser();

// 2. Set parser options (optional)
parser.LongOptionPrefix = "--";
parser.LongOptionArgumentSeparator = "=";
parser.ShortOptionPrefix = "-";
parser.StopOptionParseTrigger = "--";
parser.MultipleShortOptions = true;
parser.StopOnFirstNonOption = false;

// or
parser.SetStyle(OptionStyle.Default)

// 3. Add options
//    You can chain multiple Add methods
parser
    .Add("a")  // short option, value not required
    .Add("long")  // long option, value not required
    .Add("long-short", "i")  // long option with short option alias,
                             // value not required
    .Add("m", true)  // short option, value required
    .Add("long-arg", true)  // long option, value required
    .Add("long-short-arg", "x", true)  // long option with short option
                                       // alias, value required

// 4. Parse the command line arguments
string[] args = new string[] { "-i", "unnamed1", "--long-arg", "named", "unnamed2", "unnamed3" };
SimpleCmdResults results = parser.Parse(args);

// 5. Check for errors
if (results.HasErrors())
{
    Tuple<ParseErrorType, string, string>[] errors = results.GetErrors();
    // Item1 is one element from the ParseErrorType enum
    // Item2 is the option name causing the error, or null
    // Item3 is the option value causing the error, or null
    // do something
}

// 6. Get values
results["long-short"]    // = true
                         // Note you must use long option anme if set
results["long-arg"]      // = "named"
results["@1"]            // = "unnamed1" (positional argument starting from 1)
results["@3"]            // = "unnamed3"
results.GetAllArguments() // = { "unnamed1", "unnamed2", "unnamed3" }
results["invalid"]       // = null
results.Contains("invalid") // = false
```

## Licence

BSD 3 Clause