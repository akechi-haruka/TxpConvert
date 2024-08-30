using CommandLine;
using System.ComponentModel.Design;
using System.Reflection;
using TxpConvert.Create;
using TxpConvert.Extract;

namespace TxpConvert {

    public class Program {

        public const int MAGIC_TXP = 0x02505854;
        public const int MAGIC_TXP_SET = 0x03505854;
        public const int MAGIC_TXP_SUBSET = 0x04505854;
        public const int MAGIC_TXP_SUBSET2 = 0x05505854;

        public const string META_DATA_FILE_NAME = "txp_metadata.ini";
        public const string META_DATA_VERSION = "1";

        public static int Main(string[] args) {
            return Parser.Default.ParseArguments<ExtractOptions, CreateOptions>(args).MapResult<ExtractOptions, CreateOptions, int>(Extractor.RunExtract, Creator.RunCreate, errs => 1);
        }

        internal static bool IsVerbose;

        internal static void Log(string s) {
            Console.WriteLine(s);
        }

        internal static void LogVerbose(string s, params object[] args) {
            if (IsVerbose) {
                Log(String.Format(s, args));
            }
        }


    }

}