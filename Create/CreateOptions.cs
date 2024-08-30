using CommandLine;

namespace TxpConvert.Create {

    [Verb("create", HelpText = "Create a .txp file")]
    public class CreateOptions {

        [Option('v', Required = false, HelpText = "Verbose output")]
        public bool Verbose { get; set; }

        [Option('f', Required = false, HelpText = "Override the export format", Default = TexFormat.Unset)]
        public TexFormat OverrideFormat { get; set; }

        [Option('u', Required = false, HelpText = "Sets the \"Unknown\" value for TXP sets", Default = 16843050)]
        public int Unknown { get; set; }

        [Value(0, Required = true, HelpText = "The type of .txp file to create (Single, Set, SetWithSubsets")]
        public CreateVariant Variant { get; set; }

        [Value(1, Required = true, HelpText = "The input folder")]
        public String InFolder { get; set; }

        [Value(2, Required = true, HelpText = "The output file")]
        public String OutFile { get; set; }
    }



}