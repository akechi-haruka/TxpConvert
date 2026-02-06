using BCnEncoder.Decoder;
using BCnEncoder.Shared;
using Microsoft.Toolkit.HighPerformance;

namespace TxpConvert.Create;

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

        TXPTableEntry[] table = null;
        if (Boolean.TryParse(metadata.Read("HasTable"), out bool hasTable) && hasTable) {
            Program.Log("Table data found, copying sprites into textures...");
            int spriteCount = Int32.Parse(metadata.Read("SpriteCount"));
            table = new TXPTableEntry[spriteCount];
            for (int i = 0; i < spriteCount; i++) {
                String section = "sprite" + i;
                table[i] = new TXPTableEntry() {
                    Index = UInt32.Parse(metadata.Read("Index", section)),
                    TxpIndex = Int32.Parse(metadata.Read("TXPIndex", section)),
                    StartX = UInt32.Parse(metadata.Read("StartX", section)),
                    StartY = UInt32.Parse(metadata.Read("StartY", section)),
                    EndX = UInt32.Parse(metadata.Read("EndX", section)),
                    EndY = UInt32.Parse(metadata.Read("EndY", section)),
                    FileName = metadata.Read("FileName", section),
                    TextureName = metadata.Read("TextureName", section)
                };
            }
        }

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
            String fileName = metadata.Read("FileName", section);

            String fn = Path.Combine(opts.InFolder, fileName + ".dds");
            Program.LogVerbose("Reading " + fn);
            if (!File.Exists(fn)) {
                fn = Path.Combine(opts.InFolder, i + ".dds");
                if (!File.Exists(fn)) {
                    Program.Log("Texture " + i + " (" + fileName + ") referred to by metadata file not found in input directory: " + fn);
                    return 3;
                }
            }

            using (FileStream fs = File.OpenRead(fn)) {
                BcDecoder decoder = new BcDecoder();
                Memory2D<ColorRgba32> pixels = decoder.Decode2D(fs);

                if (table != null) {
                    foreach (TXPTableEntry sprite in table.Where(t => t.TxpIndex == i)) {
                        string fnSprite = Path.Combine(opts.InFolder, "sprites", sprite.TextureName + ".dds");
                        Program.LogVerbose("Reading " + fnSprite);
                        if (!File.Exists(fnSprite)) {
                            Program.Log("Sprite " + sprite.TextureName + " in texture " + i + " (" + fileName + ") referred to by metadata file not found in input directory: " + fnSprite);
                            return 4;
                        }

                        pixels = MergeSpriteIntoTexture(fnSprite, sprite, pixels);
                    }
                }

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

    private static Memory2D<ColorRgba32> MergeSpriteIntoTexture(string fnSprite, TXPTableEntry sprite, Memory2D<ColorRgba32> pixels) {
        using (FileStream fsSprite = File.OpenRead(fnSprite)) {
            BcDecoder decoderSprite = new BcDecoder();
            Memory2D<ColorRgba32> pixelsSprite = decoderSprite.Decode2D(fsSprite);

            uint sx = sprite.StartX;
            uint sy = sprite.StartY;
            uint ex = sprite.EndX;
            uint ey = sprite.EndY;
                
            if (sx > ex) {
                (sx, ex) = (ex, sx);
            }

            if (sy > ey) {
                (sy, ey) = (ey, sy);
            }

            uint w = ex - sx;
            uint h = ey - sy;

            if (!(pixelsSprite.Width == w && pixelsSprite.Height == h)) {
                throw new IOException("The input sprite " + fnSprite + " does not have the dimensions as specified in the metadata file. Expected " + w + "x" + h + ", Given " + pixelsSprite.Width + "x" + pixelsSprite.Height);
            }

            Program.LogVerbose("Copying sprite " + sprite.TextureName + " of " + w + "x" + h + " to texture at " + sx + "/" + sy + " [" + ex + "/" + ey + "], size " + pixels.Width + "x" + pixels.Height);

            return pixels.CopyMerge(pixelsSprite, sx, sy, h, w);
        }
    }
}