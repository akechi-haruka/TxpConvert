using BCnEncoder.Decoder;
using BCnEncoder.Encoder;
using BCnEncoder.Shared;
using Microsoft.Toolkit.HighPerformance;

namespace TxpConvert;

public class TXP {
    public uint Width { get; private set; }
    public uint Height { get; private set; }
    public TexFormat Format { get; private set; }
    public uint Index { get; private set; }
    public byte[] Data { get; private set; }
    public uint SubSetInfo { get; internal set; }
    public int SubSetIndex { get; internal set; }

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
        uint len = reader.ReadUInt32();
        Data = new byte[len];
        if (reader.Read(Data) != len) {
            throw new IOException("Reached end of file before data was fully read");
        }
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

    public void SaveAsDDS(string filename, bool hflip = false, bool vflip = false, uint sx = 0, uint sy = 0, uint ex = 0, uint ey = 0) {
        BcDecoder decoder = new BcDecoder();
        Memory2D<ColorRgba32> pixels = decoder.DecodeRaw2D(Data, (int)Width, (int)Height, Format.ToBCnEncoderFormat());

        pixels = pixels.Flip(Width, Height, hflip, vflip);
        if (sx > 0 || sy > 0 || ex > 0 || ey > 0) {
            Program.LogVerbose("Splitting image from " + Width + "x" + Height + " to " + sx + "/" + sy + " [" + ex + "/" + ey + "], " + (ex - sx) + "x" + (ey - sy));
            if (sx > ex) {
                (sx, ex) = (ex, sx);
            }

            if (sy > ey) {
                (sy, ey) = (ey, sy);
            }

            pixels = pixels.SliceCopy(sx, sy, ey - sy, ex - sx);
        }

        BcEncoder encoder = new BcEncoder {
            OutputOptions = {
                GenerateMipMaps = false,
                Quality = CompressionQuality.BestQuality,
                Format = CompressionFormat.Rgba,
                FileFormat = OutputFileFormat.Dds
            }
        };

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