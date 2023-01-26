using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FWSEConverter
{
    class ConvertingManager
    {
        public static void ConvertToWav(string fwsepath)
        {
            string wavepath = fwsepath + ".wav";
            FWSE fwse = FWSEHelper.ReadFWSE(fwsepath, "MFF");
            WAVE wave = FWSEHelper.ConvertToWAVE(fwse);
            WAVEHelper.WriteWAVE(wavepath, wave);
        }

        public static void ConvertToFWSE(string wavpath, string origpath, bool rewrite = false)
        {
            WAVE wave = WAVEHelper.ReadWAVE(wavpath, "wvu");
            FWSE fwse = FWSEHelper.ReadFWSE(origpath, "MFF");
            FWSE res = FWSEHelper.ConvertToFWSE(wave, fwse);
            string fwsepath = rewrite ? origpath : wavpath + ".fwse";
            FWSEHelper.WriteFWSE(fwsepath, res);
        }

        public static void ReConvertToWav(string wavpath)
        {
            string wavepath = wavpath + ".wav";
            WAVE wave = WAVEHelper.ReadWAVE(wavpath, "wvu");
            WAVEHelper.WriteWAVE(wavepath, wave);
        }

        public static void ConvertToWav(string fwsepath, string savepath)
        {
            string wavepath = savepath;
            FWSE fwse = FWSEHelper.ReadFWSE(fwsepath, "MFF");
            WAVE wave = FWSEHelper.ConvertToWAVE(fwse);
            WAVEHelper.WriteWAVE(wavepath, wave);
        }


    }
}
