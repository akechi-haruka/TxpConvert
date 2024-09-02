using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TxpConvert.Extract {
    internal class Extractor {

        public static int RunExtract(ExtractOptions opts) {

            Program.IsVerbose = opts.Verbose;

            if (!File.Exists(opts.InFile)) {
                Program.Log("Input file not found: " + opts.InFile);
                return 1;
            }
            if (!Directory.Exists(opts.OutFolder)) {
                Directory.CreateDirectory(opts.OutFolder);
            }

            BinaryReader reader = new BinaryReader(File.OpenRead(opts.InFile));
            TXP[] arr = AutoReadTXPFile(reader);
            Program.Log("Texture Count: " + arr.Length);

            string mn = Path.Combine(opts.OutFolder, Program.META_DATA_FILE_NAME);
            Program.Log("Creating metadata file at: " + mn);
            IniFile metadata = new IniFile(mn);

            metadata.Write("Version", Program.META_DATA_VERSION);
            metadata.Write("Count", arr.Length);

            for (int i = 0; i < arr.Length; i++) {
                TXP txp = arr[i];
                string fn = Path.Combine(opts.OutFolder, i + ".dds");
                Program.Log("Saving: " + fn);
                txp.SaveAsDDS(fn, opts.HFlip, opts.VFlip);

                string section = "texture" + i;
                metadata.Write("Width", txp.Width, section);
                metadata.Write("Height", txp.Height, section);
                metadata.Write("Format", txp.Format, section);
                metadata.Write("Index", txp.Index, section);
                metadata.Write("HFlip", opts.HFlip, section);
                metadata.Write("VFlip", opts.VFlip, section);
                metadata.Write("SubSetInfo", txp.SubSetInfo, section);
                metadata.Write("SubSetIndex", txp.SubSetIndex, section);
            }

            return 0;

        }

        public static TXP[] AutoReadTXPFile(BinaryReader reader) {
            uint magic = reader.ReadUInt32();
            if (magic == Program.MAGIC_TXP) {
                Program.Log("Format: TXP");
                return new TXP[] { new TXP(reader) };
            } else if (magic == Program.MAGIC_TXP_SET) {
                Program.Log("Format: TXP Set");
                return new TXPSet(reader, 0).Textures;
            } else {
                throw new IOException("Invalid file (magic=" + magic.ToString("X2"));
            }
        }

    }
}
