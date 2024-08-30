using System;
using System.Collections.Generic;
using System.Drawing.Interop;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TxpConvert {
    
    public class TXPSet {

        public TXP[] Textures { get; private set; }
        public int Unknown { get; private set; }

        public TXPSet(int unknown, params TXP[] textures) {
            Textures = textures;
            Unknown = unknown;
        }

        public TXPSet(BinaryReader reader, long baseOffset = 0) {
            Read(reader, baseOffset);
        }

        public void Read(BinaryReader reader, long baseOffset = 0) {
            Textures = new TXP[reader.ReadInt32()];
            Unknown = reader.ReadInt32();
            Program.LogVerbose("TXPSet Count = " + Textures.Length);
            Program.LogVerbose("TXPSet Unknown = " + Unknown);
            for (int i = 0; i < Textures.Length; i++) {
                uint offset = reader.ReadUInt32();
                Program.LogVerbose("TXPSet Offset = " + offset);
                long save = reader.BaseStream.Position;
                reader.BaseStream.Seek(baseOffset + offset, SeekOrigin.Begin);
                long innerOffset = baseOffset + offset;
                uint magic = reader.ReadUInt32();
                if (magic == Program.MAGIC_TXP_SUBSET || magic == Program.MAGIC_TXP_SUBSET2) {
                    Program.Log("Inner Format: TXP SubSet");
                    Textures[i] = new TXPSubSet(reader, innerOffset + 4).Textures[0];
                } else if (magic == Program.MAGIC_TXP) {
                    Program.Log("Inner Format: TXP");
                    Textures[i] = new TXP(reader);
                } else {
                    throw new IOException("TXP inside TXPSet has invalid magic: " + magic.ToString("X2"));
                }
                reader.BaseStream.Seek(save, SeekOrigin.Begin);
            }
        }

        internal void SaveTXP(BinaryWriter writer) {
            writer.Write(Program.MAGIC_TXP_SET);
            writer.Write(Textures.Length);
            writer.Write(Unknown);
            int offset = 16;
            foreach (TXP tex in Textures) {
                writer.Write(offset);
                offset += tex.Data.Length + 24;
            }
            foreach (TXP tex in Textures) {
                tex.SaveTXP(writer);
            }
        }

        internal void SaveTXPAsSubsets(BinaryWriter writer) {
            writer.Write(Program.MAGIC_TXP_SET);
            writer.Write(Textures.Length);
            writer.Write(Unknown);
            int offset = 12 + Textures.Length * 4;
            foreach (TXP tex in Textures) {
                writer.Write(offset);
                offset += tex.Data.Length + 24 + 16;
            }
            foreach (TXP tex in Textures) {
                writer.Write(Program.MAGIC_TXP_SUBSET);
                writer.Write((uint)1); // texture count
                writer.Write((1 << 8) | 1); // info (mipmap count = 1, depth = 1)
                writer.Write((uint)16); // offset
                tex.SaveTXP(writer);
            }
        }
    }
}
