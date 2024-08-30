using BCnEncoder.Decoder;
using BCnEncoder.Encoder;
using BCnEncoder.Shared;
using BCnEncoder.Shared.ImageFiles;
using Microsoft.Toolkit.HighPerformance;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Interop;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TxpConvert {
    
    public class TXP {

        public uint Width { get; private set; }
        public uint Height { get; private set; }
        public TexFormat Format { get; private set; }
        public uint Index { get; private set; }
        public byte[] Data { get; private set; }

        public TXP(BinaryReader reader) {
            Read(reader);
            Program.LogVerbose("TXP@{0}: {1}x{2} {3}", Index, Width, Height, Format);
        }

        public TXP(uint width, uint height, TexFormat format, uint index, Memory2D<ColorRgba32> pixels) {
            Width = width;
            Height = height;
            Format = format;
            Index = index;
            Read(pixels);
            Program.LogVerbose("TXP@{0}: {1}x{2} {3}", Index, Width, Height, Format);
        }

        public void Read(BinaryReader reader) {
            Width = reader.ReadUInt32();
            Height = reader.ReadUInt32();
            Format = (TexFormat)reader.ReadUInt32();
            Index = reader.ReadUInt32();
            Data = new byte[reader.ReadUInt32()];
            reader.Read(Data);
        }

        public void Read(Memory2D<ColorRgba32> pixels) {

            BcEncoder encoder = new BcEncoder();

            encoder.OutputOptions.GenerateMipMaps = false;
            encoder.OutputOptions.Quality = CompressionQuality.Fast;
            encoder.OutputOptions.Format = Format.ToBCnEncoderFormat();

            DateTime start = DateTime.Now;

            Program.LogVerbose("Reading TXP to index {0}: {1}x{2} {3}", Index, Width, Height, Format);

            Data = encoder.EncodeToRawBytes(pixels, 0, out _, out _);

            Program.LogVerbose("Took " + (DateTime.Now - start));
            Program.LogVerbose("Encoded length = " + Data.Length);
        }

        public void SaveAsDDS(string filename, bool hflip = false, bool vflip = false) {

            BcDecoder decoder = new BcDecoder();
            Memory2D<ColorRgba32> pixels = decoder.DecodeRaw2D(Data, (int)Width, (int)Height, Format.ToBCnEncoderFormat());

            pixels = pixels.Flip(Width, Height, hflip, vflip);

            BcEncoder encoder = new BcEncoder();

            encoder.OutputOptions.GenerateMipMaps = false;
            encoder.OutputOptions.Quality = CompressionQuality.BestQuality;
            encoder.OutputOptions.Format = CompressionFormat.Rgba;
            encoder.OutputOptions.FileFormat = OutputFileFormat.Dds;

            using FileStream fs = File.OpenWrite(filename);
            encoder.EncodeToStream(pixels, fs);

        }

        internal void SaveTXP(BinaryWriter writer) {
            writer.Write(Program.MAGIC_TXP);
            writer.Write(Width);
            writer.Write(Height);
            writer.Write((uint)Format);
            writer.Write(Index);
            writer.Write(Data.Length);
            writer.Write(Data);
        }
    }
}
