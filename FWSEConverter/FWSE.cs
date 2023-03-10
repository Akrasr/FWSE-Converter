using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FWSEConverter
{
    class FWSE
    {
        public int Index { get; set; }
        public string Format { get; set; }
        public int Version { get; set; }
        public int FileSize { get; set; }
        public int HeaderSize { get; set; }
        public int NumChannels { get; set; }
        public int Samples { get; set; }
        public int SampleRate { get; set; }
        public int BitsPerSample { get; set; }
        public byte[] InfoData { get; set; }
        public byte[] SoundData { get; set; }

        public byte[] GopperData { get; set; }

        // Extra Data
        public TimeSpan DurationSpan { get; set; }
        public string ExpectedFileName { get; set; }
        public string DisplayFormat { get; set; }

        public FWSE(int FileIndex = 0) => Index = FileIndex;
    }
}
