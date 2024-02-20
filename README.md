# HFST.net

A C# implementation of HFST. The code is based on [HFST for Java](https://github.com/hfst/hfst-optimized-lookup/tree/master/hfst-optimized-lookup-java).

## Installation

You can install the [NuGet package](https://www.nuget.org/packages/MikaHamalainen.hfst/) in Visual Studio or use the command-line tools

    dotnet add package MikaHamalainen.hfst

After installing the package, you can import it like this:

    using hfst;

## Usage

You can load a transducer like this

    var transducerFile = "/path/to/transducer.hfstol";
    var t = new HFST(transducerFile);

HFST can be initialized with a string path, FileStream or BinaryReader.

You can run the transducer like so:

    var analyses = t.Lookup("koira+N+Sg+Ine");
    foreach (var analysis in analyses)
    {
        Console.WriteLine($"{input}\t{analysis}\t{analysis.Weight}");
    }

Output:

    koira+N+Sg+Ine	koirassa	0

You can get the analysis as a string by calling analysis.ToString()

