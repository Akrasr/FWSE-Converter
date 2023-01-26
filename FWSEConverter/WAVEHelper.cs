using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace FWSEConverter
{
    class WAVEHelper
    {
        public static void WriteWAVE(string FilePath, WAVE WAVEFile, bool MessageBox = true)
        {
            using (FileStream FS = new FileStream(FilePath, FileMode.Create))
            {
                using (BinaryWriter BW = new BinaryWriter(FS))
                {
                    BW.Write(WAVEFile.ChunkID.ToCharArray());
                    BW.Write(WAVEFile.ChunkSize);
                    BW.Write(WAVEFile.Format.ToCharArray());

                    BW.Write(WAVEFile.Subchunk1ID.ToCharArray());
                    BW.Write(WAVEFile.Subchunk1Size);
                    BW.Write(WAVEFile.AudioFormat);
                    BW.Write(WAVEFile.NumChannels);
                    BW.Write(WAVEFile.SampleRate);
                    BW.Write(WAVEFile.ByteRate);
                    BW.Write(WAVEFile.BlockAlign);
                    BW.Write(WAVEFile.BitsPerSample);

                    BW.Write(WAVEFile.Subchunk2ID.ToCharArray());
                    BW.Write(WAVEFile.Subchunk2Size);

                    foreach (short Sample in WAVEFile.Subchunk2Data)
                        BW.Write(Sample);

                    if (MessageBox)
                        Console.WriteLine("WAVE file written sucessfully!");
                }
            }
        }

        public static WAVE ReadWAVE(string FilePath, string FileName)
        {
                using (FileStream FS = new FileStream(FilePath, FileMode.Open))
                {
                    using (BinaryReader BR = new BinaryReader(FS))
                    {
                        WAVE WAVEFile = new WAVE();

                        for (int i = 0; i < 4; i++)
                            WAVEFile.ChunkID += (char)BR.ReadByte();

                        WAVEFile.ChunkSize = BR.ReadUInt32();

                        for (int i = 0; i < 4; i++)
                            WAVEFile.Format += (char)BR.ReadByte();

                        for (int i = 0; i < 4; i++)
                            WAVEFile.Subchunk1ID += (char)BR.ReadByte();

                        WAVEFile.Subchunk1Size = BR.ReadUInt32();
                        WAVEFile.AudioFormat = BR.ReadUInt16();
                        WAVEFile.NumChannels = BR.ReadUInt16();
                        if (WAVEFile.NumChannels != 1)
                            throw new Exception("Error reading " + FilePath + ". Only mono sound files are supported.");
                        WAVEFile.SampleRate = BR.ReadUInt32();
                        WAVEFile.ByteRate = BR.ReadUInt32();
                        WAVEFile.BlockAlign = BR.ReadUInt16();
                        WAVEFile.BitsPerSample = BR.ReadUInt16();

                        for (int i = 0; i < 4; i++)
                            WAVEFile.Subchunk2ID += (char)BR.ReadByte();
                        while (WAVEFile.Subchunk2ID != "data")
                        {
                            int namelen = BR.ReadInt32();
                            BR.ReadBytes(namelen);
                            WAVEFile.Subchunk2ID = "";
                            for (int i = 0; i < 4; i++)
                                WAVEFile.Subchunk2ID += (char)BR.ReadByte();
                        }
                        WAVEFile.Subchunk2Size = BR.ReadUInt32();

                        long Samples = WAVEFile.Subchunk2Size / WAVEFile.NumChannels / (WAVEFile.BitsPerSample / 8);

                        WAVEFile.Subchunk2Data = new short[Samples];

                        for (int i = 0; i < Samples; i++)
                            WAVEFile.Subchunk2Data[i] = BR.ReadInt16();

                        return WAVEFile;
                    }
                }
        }
    }
}
