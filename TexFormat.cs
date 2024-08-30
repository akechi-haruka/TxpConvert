using BCnEncoder.Shared;
using Microsoft.Toolkit.HighPerformance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TxpConvert {
    
    public enum TexFormat {
        Unset = 0,
        R8G8B8 = 1,
        R5G5B5 = 3,
        R5G5B5A1 = 4,
        R4G4B4 = 5,
        DXT1 = 6,
        DXT1_ALT2 = 7,
        DXT3 = 8,
        DXT5 = 9,
        BC4 = 10,
        BC5 = 11,
        DXT1_ALT3 = 101,
        DXT1_ALT4 = 102,
        DXT1_ALT5 = 103,
        DXT5_ALT2 = 109,
        DXT5_ALT3 = 110,
        BC4_ALT2 = 112,
        BC5_ALT2 = 115,
        BC6S = 127,
        BC7 = 130,
        BC7_ALT2 = 131
    }

    public static class TexFormatExtensions {

        public static CompressionFormat ToBCnEncoderFormat(this TexFormat format) {
            switch (format) {
                case TexFormat.R8G8B8:
                    return CompressionFormat.Rgb;
                case TexFormat.R5G5B5A1:
                    return CompressionFormat.Rg;
                case TexFormat.DXT1:
                case TexFormat.DXT1_ALT2:
                case TexFormat.DXT1_ALT3:
                case TexFormat.DXT1_ALT4:
                case TexFormat.DXT1_ALT5:
                    return CompressionFormat.Bc1;
                case TexFormat.DXT3:
                    return CompressionFormat.Bc2;
                case TexFormat.DXT5:
                case TexFormat.DXT5_ALT2:
                case TexFormat.DXT5_ALT3:
                    return CompressionFormat.Bc3;
                case TexFormat.BC4:
                case TexFormat.BC4_ALT2:
                    return CompressionFormat.Bc4;
                case TexFormat.BC5:
                case TexFormat.BC5_ALT2:
                    return CompressionFormat.Bc5;
                case TexFormat.BC6S:
                    return CompressionFormat.Bc6S;
                case TexFormat.BC7:
                case TexFormat.BC7_ALT2:
                    return CompressionFormat.Bc7;
                default:
                    throw new IOException("Unsupported compression: " + format);
            }
        }

        public static Memory2D<ColorRgba32> Flip(this Memory2D<ColorRgba32> pixels, uint width, uint height, bool hflip, bool vflip) {
            if (hflip || vflip) {
                ColorRgba32[,] arr = pixels.ToArray();
                if (hflip) {
                    ColorRgba32[,] pixels2 = new ColorRgba32[height, width];
                    for (int x = 0; x < width; x++) {
                        for (int y = 0; y < height; y++) {
                            pixels2[y, x] = arr[y, width - x - 1];
                        }
                    }
                    arr = pixels2;
                }
                if (vflip) {
                    ColorRgba32[,] pixels2 = new ColorRgba32[height, width];
                    for (int x = 0; x < width; x++) {
                        for (int y = 0; y < height; y++) {
                            pixels2[y, x] = arr[height - y - 1, x];
                        }
                    }
                    arr = pixels2;
                }
                pixels = new Memory2D<ColorRgba32>(arr);
            }
            return pixels;
        }

    }
}
