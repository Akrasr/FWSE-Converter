using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace FWSEConverter
{
    class FWSEHelper
    {
        private static readonly int[] ADPCMTable = {
            7, 8, 9, 10, 11, 12, 13, 14,
            16, 17, 19, 21, 23, 25, 28, 31,
            34, 37, 41, 45, 50, 55, 60, 66,
            73, 80, 88, 97, 107, 118, 130, 143,
            157, 173, 190, 209, 230, 253, 279, 307,
            337, 371, 408, 449, 494, 544, 598, 658,
            724, 796, 876, 963, 1060, 1166, 1282, 1411,
            1552, 1707, 1878, 2066, 2272, 2499, 2749, 3024,
            3327, 3660, 4026, 4428, 4871, 5358, 5894, 6484,
            7132, 7845, 8630, 9493, 10442, 11487, 12635, 13899,
            15289, 16818, 18500, 20350, 22385, 24623, 27086, 29794,
            32767
        };

        private static readonly int[] CAPCOM_IndexTable = { 8, 6, 4, 2, -1, -1, -1, -1, -1, -1, -1, -1, 2, 4, 6, 8 };
        public static int Clamp(int Value, int Min, int Max)
        {
            return Value < Min ? Min : Value > Max ? Max : Value;
        }
        private static int IMA_MTF_ExpandNibble(int nibble, int shift, ref int sample_decoded_last, ref int step_index)
        {
            nibble = nibble >> shift & 0xF;

            int step = ADPCMTable[step_index];
            int sample = sample_decoded_last;

            int delta = step * (2 * nibble - 15);

            sample += delta;
            sample_decoded_last = sample;

            step_index += CAPCOM_IndexTable[nibble];
            step_index = Clamp(step_index, 0, 88);

            return Clamp(sample >> 4, -32768, 32767);
        }

        private static int MTF_IMA_SimplifyNible(int sample, ref int sample_predicted, ref int step_index)
        {
            int diff = (sample << 4) - sample_predicted;
            int step = ADPCMTable[step_index];

            int nibble = Clamp((int)Math.Round(diff / 2.0 / step) + 8, 0, 15);

            sample_predicted += step * (2 * nibble - 15);

            step_index += CAPCOM_IndexTable[nibble];
            step_index = Clamp(step_index, 0, 88);

            return nibble;
        }

        private static short[] DecodeFWSE(byte[] FWSEData)
        {
            int[] Result = new int[FWSEData.Length * 2];

            int sample_decoded_last = 0;
            int result_index = 0;
            int step_index = 0;

            foreach (var FWSENibble in FWSEData)
            {
                int sample = IMA_MTF_ExpandNibble(FWSENibble, 4, ref sample_decoded_last, ref step_index);
                Result[result_index] = sample; result_index++;

                sample = IMA_MTF_ExpandNibble(FWSENibble, 0, ref sample_decoded_last, ref step_index);
                Result[result_index] = sample; result_index++;
            }

            return Result.Select(i => (short)i).ToArray();
        }

        private static byte[] EncodeFWSE(short[] WAVEData)
        {
            int[] Result = new int[WAVEData.Length / 2];

            int sample_predicted = 0;
            int step_index = 0;

            int nibble_left = 0;
            int nibble_counter = 0;

            for (int i = 0; i < WAVEData.Length; i++)
            {
                int sample_encoded = MTF_IMA_SimplifyNible(WAVEData[i], ref sample_predicted, ref step_index);

                if (i % 2 == 0)
                    nibble_left = sample_encoded;
                else
                {
                    Result[nibble_counter] = (nibble_left << 4) | sample_encoded;
                    nibble_counter++;
                }
            }

            return Result.Select(i => (byte)i).ToArray();
        }

        public static WAVE ConvertToWAVE(FWSE FWSEFile)
        {
            WAVE WAVEFile = new WAVE();

            WAVEFile.ChunkID = "RIFF";
            WAVEFile.Format = "WAVE";

            WAVEFile.Subchunk1ID = "fmt ";
            WAVEFile.Subchunk1Size = (uint)FWSEFile.BitsPerSample;
            WAVEFile.AudioFormat = 1;
            WAVEFile.NumChannels = (ushort)FWSEFile.NumChannels;
            WAVEFile.SampleRate = (uint)FWSEFile.SampleRate;
            WAVEFile.BitsPerSample = 16;

            WAVEFile.ByteRate = WAVEFile.SampleRate * WAVEFile.NumChannels * WAVEFile.BitsPerSample / 8;
            WAVEFile.BlockAlign = (ushort)(WAVEFile.NumChannels * WAVEFile.BitsPerSample / 8);

            WAVEFile.Subchunk2ID = "data";
            WAVEFile.Subchunk2Size = (uint)(FWSEFile.SoundData.Length * 2) * WAVEFile.NumChannels * WAVEFile.BitsPerSample / 8;
            WAVEFile.Subchunk2Data = DecodeFWSE(FWSEFile.SoundData);

            WAVEFile.ChunkSize = 4 + 8 + WAVEFile.Subchunk1Size + 8 + WAVEFile.Subchunk2Size;

            return WAVEFile;
        }

        public static FWSE ConvertToFWSE(WAVE WAVEFile, FWSE orig, int Index = 0)
        {
            byte[] FWSEData = EncodeFWSE(WAVEFile.Subchunk2Data);

            FWSE FWSEFile = new FWSE(Index);

            FWSEFile.Format = "FWSE";
            FWSEFile.Version = 3;
            FWSEFile.FileSize = 1024 + FWSEData.Length;
            FWSEFile.HeaderSize = 1024;
            FWSEFile.NumChannels = 1;
            FWSEFile.Samples = FWSEData.Length * 2;
            FWSEFile.SampleRate = (int)WAVEFile.SampleRate;
            FWSEFile.BitsPerSample = WAVEFile.BitsPerSample;
            FWSEFile.InfoData = orig.InfoData;
            FWSEFile.SoundData = FWSEData;
            FWSEFile.GopperData = orig.GopperData;

            FWSEFile.DurationSpan = TimeSpan.FromSeconds((double)FWSEFile.Samples / FWSEFile.SampleRate);
            FWSEFile.ExpectedFileName = $"{FWSEFile.Index}.fwse";
            FWSEFile.DisplayFormat = "FWSE";

            return FWSEFile;
        }

        public static FWSE ReadFWSE(string FilePath, string FileName, int Index = 0)
        {

            try
            {
                using (FileStream FS = new FileStream(FilePath, FileMode.Open))
                {
                    using (BinaryReader BR = new BinaryReader(FS))
                    {
                        FWSE FWSEFile = new FWSE(Index);

                        for (int i = 0; i < 4; i++)
                            FWSEFile.Format += (char)BR.ReadByte();

                        FWSEFile.Version = BR.ReadInt32();
                        FWSEFile.FileSize = BR.ReadInt32();
                        FWSEFile.HeaderSize = BR.ReadInt32();
                        FWSEFile.NumChannels = BR.ReadInt32();
                        FWSEFile.Samples = BR.ReadInt32();
                        FWSEFile.SampleRate = BR.ReadInt32();
                        FWSEFile.BitsPerSample = BR.ReadInt32();
                        FWSEFile.InfoData = BR.ReadBytes(FWSEFile.HeaderSize - 32);
                        FWSEFile.SoundData = BR.ReadBytes((int)FWSEFile.FileSize - FWSEFile.HeaderSize);
                        FWSEFile.GopperData = BR.ReadBytes((int)FS.Length - FWSEFile.FileSize);

                        FWSEFile.DurationSpan = TimeSpan.FromSeconds((double)FWSEFile.Samples / FWSEFile.SampleRate);
                        FWSEFile.ExpectedFileName = $"{FWSEFile.Index}.fwse";
                        FWSEFile.DisplayFormat = "FWSE";

                        return FWSEFile;
                    }
                }
            }
            catch (Exception)
            {
                Console.WriteLine($"Error reading {FileName}: File seems to be corrupted, please refer to a valid FWSE file when using this tool.");
                return null;
            }
        }

        public static void WriteFWSE(string FilePath, FWSE FWSEFile, bool MessageBox = true)
        {
            using (FileStream FS = new FileStream(FilePath, FileMode.Create))
            {
                using (BinaryWriter BW = new BinaryWriter(FS))
                {
                    BW.Write(FWSEFile.Format.ToCharArray());
                    BW.Write(FWSEFile.Version);
                    BW.Write(FWSEFile.FileSize);
                    BW.Write(FWSEFile.HeaderSize);
                    BW.Write(FWSEFile.NumChannels);
                    BW.Write(FWSEFile.Samples);
                    BW.Write(FWSEFile.SampleRate);
                    BW.Write(FWSEFile.BitsPerSample);
                    BW.Write(FWSEFile.InfoData);
                    BW.Write(FWSEFile.SoundData);
                    BW.Write(FWSEFile.GopperData);

                    if (MessageBox)
                        Console.WriteLine("FWSE file written sucessfully!");
                }
            }
        }
    }
}
