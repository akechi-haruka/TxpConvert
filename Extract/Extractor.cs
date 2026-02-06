namespace TxpConvert.Extract;

internal class Extractor {
    public static int RunExtract(ExtractOptions opts) {
        Program.IsVerbose = opts.Verbose;

        if (!File.Exists(opts.InFile)) {
            Program.Log("Input file not found: " + opts.InFile);
            return 1;
        }

        if (opts.TableFile != null && !File.Exists(opts.TableFile)) {
            Program.Log("Table file not found: " + opts.TableFile);
            return 1;
        }

        if (!Directory.Exists(opts.OutFolder)) {
            Directory.CreateDirectory(opts.OutFolder);
        }

        TXPTable txpTable = null;
        if (opts.TableFile != null) {
            BinaryReader tableReader = new BinaryReader(File.OpenRead(opts.TableFile));
            txpTable = ReadTableFile(tableReader);

            Program.Log("Table File Filename Count: " + txpTable.FileNamesToTXPIndex.Count);
            Program.Log("Table File Data Count: " + txpTable.Table.Length);
        }

        BinaryReader reader = new BinaryReader(File.OpenRead(opts.InFile));
        TXP[] arr = AutoReadTXPFile(reader);
        Program.Log("Texture Count: " + arr.Length);

        if (txpTable != null && arr.Length != txpTable.FileNamesToTXPIndex.Count) {
            throw new IOException("TXP texture count unequal table texture count (" + arr.Length + " / " + txpTable.FileNamesToTXPIndex.Count + ")");
        }

        string mn = Path.Combine(opts.OutFolder, Program.META_DATA_FILE_NAME);
        Program.Log("Creating metadata file at: " + mn);
        IniFile metadata = new IniFile(mn);

        metadata.Write("Version", Program.META_DATA_VERSION);
        metadata.Write("Count", arr.Length);
        if (txpTable != null) {
            metadata.Write("HasTable", true);
            metadata.Write("SpriteCount", txpTable.Table.Length);
        }

        for (int i = 0; i < arr.Length; i++) {
            TXP txp = arr[i];
            string fn;
            if (txpTable != null) {
                fn = Path.Combine(opts.OutFolder, txpTable.FileNamesToTXPIndex[i] + ".dds");
            } else {
                fn = Path.Combine(opts.OutFolder, i + ".dds");
            }

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
            if (txpTable != null) {
                metadata.Write("TXPBaseIndex", i, section);
                metadata.Write("FileName", txpTable.FileNamesToTXPIndex[i], section);
            }
        }

        if (txpTable != null) {
            string tableOutFolder = Path.Combine(opts.OutFolder, "sprites");
            if (!Directory.Exists(tableOutFolder)) {
                Directory.CreateDirectory(tableOutFolder);
            }

            foreach (TXPTableEntry sprite in txpTable.Table) {
                TXP txp = arr[sprite.TxpIndex];
                string fn = Path.Combine(tableOutFolder, sprite.TextureName + ".dds");
                Program.Log("Saving sprite: " + sprite.TextureName + " from " + sprite.FileName + " (file index " + sprite.TxpIndex + ")");
                txp.SaveAsDDS(fn, false, true, sprite.StartX, sprite.StartY, sprite.EndX, sprite.EndY);

                string section = "sprite" + sprite.Index;
                metadata.Write("Index", sprite.Index, section);
                metadata.Write("TXPIndex", sprite.TxpIndex, section);
                metadata.Write("StartX", sprite.StartX, section);
                metadata.Write("StartY", sprite.StartY, section);
                metadata.Write("EndX", sprite.EndX, section);
                metadata.Write("EndY", sprite.EndY, section);
                metadata.Write("FileName", sprite.FileName, section);
                metadata.Write("TextureName", sprite.TextureName, section);
            }
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
            throw new IOException("Invalid file (magic=" + magic.ToString("X2") + ")");
        }
    }

    public static TXPTable ReadTableFile(BinaryReader reader) {
        uint magic = reader.ReadUInt32();
        if (magic != TXPTableEntry.MAGIC) {
            throw new IOException("Invalid file (magic=" + magic.ToString("X2") + ")");
        }

        uint entryCount = reader.ReadUInt32();

        TXPTableEntry[] ret = new TXPTableEntry[entryCount];
        for (int i = 0; i < entryCount; i++) {
            TXPTableEntry tableEntry = new TXPTableEntry(reader);
            ret[tableEntry.Index] = tableEntry;
        }

        List<string> txpFileNames = new List<string>();
        foreach (TXPTableEntry table in ret) {
            if (!txpFileNames.Contains(table.FileName)) {
                txpFileNames.Add(table.FileName);
            }
        }

        txpFileNames.Sort();

        foreach (TXPTableEntry table in ret) {
            table.TxpIndex = txpFileNames.IndexOf(table.FileName);
        }

        for (int i = 0; i < txpFileNames.Count; i++) {
            Program.LogVerbose("txp[" + i + "]: " + txpFileNames[i]);
        }

        for (int i = 0; i < ret.Length; i++) {
            Program.LogVerbose("table[" + i + "]: " + ret[i]);
        }

        return new TXPTable() {
            FileNamesToTXPIndex = txpFileNames,
            Table = ret
        };
    }
}