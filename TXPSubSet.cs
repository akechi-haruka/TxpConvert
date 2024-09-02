using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TxpConvert {
    
    public class TXPSubSet {

        public TXP[] Textures { get; private set; }
        public uint Info { get; private set; }
        public uint MipmapCount { get; private set; }
        public uint Depth { get; private set; }
        public bool isYCbCr { get; private set; }

        public TXPSubSet(params TXP[] textures) {
            Textures = textures;
        }

        public TXPSubSet(BinaryReader reader, long baseOffset = 0) {
            Read(reader, baseOffset);
        }

        public void Read(BinaryReader reader, long baseOffset = 0) {
            Textures = new TXP[reader.ReadInt32()];
            Program.LogVerbose("Format: TXPSubSet: len=" + Textures.Length);
            Info = reader.ReadUInt32();
            MipmapCount = Info & 0xFF;
            Depth = (Info >> 8) & 0xFF;
            Program.LogVerbose("Info={0}, MipmapCount={1}, Depth={2}", Info, MipmapCount, Depth);
            isYCbCr = (MipmapCount == 2 && Depth == 1) || (MipmapCount == 3 && Depth == 1);
            // todo? isEmcs -> MipmapCount++;
            uint[] textureOffsets = new uint[Textures.Length];
            for (int i = 0; i < textureOffsets.Length; i++) {
                textureOffsets[i] = reader.ReadUInt32();
                Program.LogVerbose("TXPSubSet offset {0} = {1}", i, textureOffsets[i]);
            }
            int k = 0;
            for (int i = 0; i < Depth; i++) {
                for (int j = 0; j < MipmapCount; j++) {
                    if (j == 0 || ((j == 1 || j == 2) && isYCbCr)) {
                        long save = reader.BaseStream.Position;
                        reader.BaseStream.Seek(baseOffset + textureOffsets[k], SeekOrigin.Begin);
                        Textures[k] = new TXP(reader) {
                            SubSetInfo = Info,
                            SubSetIndex = k
                        };
                        reader.BaseStream.Seek(save, SeekOrigin.Begin);
                    }
                    k++;
                }
            }
        }

    }
}
