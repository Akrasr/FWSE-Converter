using System;
using System.IO;
using System.Collections.Generic;

namespace FWSEConverter
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                ShowHelp();
                return;
            }
            if (args[0] != "-e" && args[0] != "-i")
            {
                Console.WriteLine("Invalid mode");
                return;
            }
            if (!Directory.Exists(args[1]))
            {
                Console.WriteLine("Such directory doesn't exist");
                return;
            }
            if (args[0] == "-e")
            {
                ExtractWavDir(args[1]);
            }
            else InsertWavDir(args[1]);
        }

        static void ExtractWavDir(string dir)
        {
            string[] files = Directory.GetFiles(dir);
            List<string> fwseFiles = new List<string>();
            foreach (string file in files)
            {
                byte[] dat = File.ReadAllBytes(file);
                string mag = "";
                for (int i = 0; i < 4; i++)
                    mag += (char)dat[i];
                if (mag == "FWSE")
                {
                    fwseFiles.Add(file);
                }
            }
            foreach(string file in fwseFiles)
            {
                ConvertingManager.ConvertToWav(file);
            }
        }

        static void InsertWavDir(string dir)
        {
            string[] files = Directory.GetFiles(dir);
            List<string> fwseFiles = new List<string>();
            foreach (string file in files)
            {
                byte[] dat = File.ReadAllBytes(file);
                string mag = "";
                for (int i = 0; i < 4; i++)
                    mag += (char)dat[i];
                if (mag == "FWSE")
                {
                    fwseFiles.Add(file);
                }
            }
            foreach (string file in fwseFiles)
            {
                string wavPath = file + ".wav";
                ConvertingManager.ConvertToFWSE(wavPath, file, true);
            }
        }

        static void ShowHelp()
        {
            string[] help = {
            "\nFWSEConverter by Akrasr",
            "Based on LuBuCake's MTSoundTool. LuBuCake's Github Page: https://github.com/LuBuCake \n",
            "Usage: FWSETool.exe <mode> <path>\n",
            "<mode> values:\n-e : extract wavs",
            "-i : insert wavs (wav files have to be mono. Stereo sound is not supported)\n",
            "<path> is a path to the directory with fwse files\n",
            "Example: FWSETool.exe -e C:\\mca\\files"
            };
            foreach (string dat in help)
                Console.WriteLine(dat);
        }
    }
}
