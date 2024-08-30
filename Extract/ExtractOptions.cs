using CommandLine;

namespace TxpConvert.Extract {

    [Verb("extract", HelpText = "Extract a .txp file")]
    public class ExtractOptions {

        [Option('v', Required = false, HelpText = "Verbose output")]
        public bool Verbose { get; set; }

        [Option("h-flip", Required = false, HelpText = "Horizontally flip the output")]
        public bool HFlip { get; set; }

        [Option("v-flip", Required = false, HelpText = "Vertically flip the output")]
        public bool VFlip { get; set; }

        [Value(0, Required = true, HelpText = "The input file")]
        public String InFile { get; set; }

        [Value(1, Required = true, HelpText = "The output folder")]
        public String OutFolder { get; set; }
    }



}