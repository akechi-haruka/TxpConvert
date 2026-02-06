using System.Text;

namespace TxpConvert;

public class TXPTableEntry {
    public const int MAGIC = 0x14090116;

    public uint Index;
    public int TxpIndex;
    public String FileName;
    public String TextureName;
    public uint MergedSizeX;
    public uint MergedSizeY;
    public uint StartX;
    public uint StartY;
    public uint EndX;
    public uint EndY;

    public TXPTableEntry() {
    }

    public TXPTableEntry(BinaryReader reader) {
        Read(reader);
    }

    private void Read(BinaryReader reader) {
        Index = reader.ReadUInt32();
        FileName = Encoding.ASCII.GetString(reader.ReadBytes(reader.ReadInt32()));
        TextureName = Encoding.ASCII.GetString(reader.ReadBytes(reader.ReadInt32()));
        MergedSizeX = reader.ReadUInt32();
        MergedSizeY = reader.ReadUInt32();
        StartX = reader.ReadUInt32();
        StartY = reader.ReadUInt32();
        EndX = reader.ReadUInt32();
        EndY = reader.ReadUInt32();
    }

    public override string ToString() {
        return Index + ", " + TxpIndex + ", " + TextureName + ", " + FileName + ", " + MergedSizeX + ", " + MergedSizeY + ", " + StartX + ", " + StartY + ", " + EndX + ", " + EndY;
    }
}