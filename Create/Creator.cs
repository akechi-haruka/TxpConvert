using BCnEncoder.Decoder;
using BCnEncoder.Shared;
using Microsoft.Toolkit.HighPerformance;
using System;
using System.Collections.Generic;
using System.Drawing.Interop;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TxpConvert.Create {
    internal class Creator {

        public static int RunCreate(CreateOptions opts) {

            Program.IsVerbose = opts.Verbose;

            if (!Directory.Exists(opts.InFolder)) {
                Program.Log("Input directory not found: " + opts.InFolder);
                return 1;
            }
            String md = Path.Combine(opts.InFolder, Program.META_DATA_FILE_NAME);
            if (!File.Exists(md)) {
                Program.Log("Metadata file not found in input directory");
                return 2;
            }

            IniFile metadata = new IniFile(md);

            int count = Int32.Parse(metadata.Read("Count"));
            TXP[] images = new TXP[count];

            for (int i = 0; i < count; i++) {

                String section = "texture" + i;
                uint width = UInt32.Parse(metadata.Read("Width", section));
                uint height = UInt32.Parse(metadata.Read("Height", section));
                TexFormat format = Enum.Parse<TexFormat>(metadata.Read("Format", section));
                uint index = UInt32.Parse(metadata.Read("Index", section));
                bool hflip = Boolean.Parse(metadata.Read("HFlip", section));
                bool vflip = Boolean.Parse(metadata.Read("VFlip", section));
                uint subsetinfo = UInt32.Parse(metadata.Read("SubSetInfo", section));
                int subsetindex = Int32.Parse(metadata.Read("SubSetIndex", section));

                String fn = Path.Combine(opts.InFolder, i + ".dds");
                if (!File.Exists(fn)) {
                    Program.Log("Texture " + i + " referred to by metadata file not found in input directory: " + fn);
                    return 3;
                }

                using (FileStream fs = File.OpenRead(fn)) {

                    BcDecoder decoder = new BcDecoder();
                    Memory2D<ColorRgba32> pixels = decoder.Decode2D(fs);

                    pixels = pixels.Flip(width, height, hflip, vflip);

                    if (opts.OverrideFormat != TexFormat.Unset) {
                        format = opts.OverrideFormat;
                    }

                    images[i] = new TXP(width, height, format, index, pixels) {
                        SubSetInfo = subsetinfo,
                        SubSetIndex = subsetindex
                    };

                }
            }

            Program.Log("About to store " + count + " texture(s) as " + opts.Variant);

            using (FileStream fs = File.OpenWrite(opts.OutFile)) {
                using (BinaryWriter writer = new BinaryWriter(fs)) {

                    if (opts.Variant == CreateVariant.Single) {
                        images[0].SaveTXP(writer);
                    } else if (opts.Variant == CreateVariant.Set) {
                        TXPSet set = new TXPSet(opts.Unknown, images);
                        set.SaveTXP(writer);
                    } else if (opts.Variant == CreateVariant.SetWithSubsets) {
                        TXPSet set = new TXPSet(opts.Unknown, images);
                        set.SaveTXPAsSubsets(writer);
                    }

                }
            }

            return 0;

        }

    }
}
