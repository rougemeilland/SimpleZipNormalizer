using System;

namespace ZipUtility.IO.Compression
{
    public enum DeflateCompressionLevel
        : Int32
    {
        Level0 = 0,
        Level1 = 1,
        Level2 = 2,
        Level3 = 3,
        Level4 = 4,
        Level5 = 5,
        Level6 = 6,
        Level7 = 7,
        Level8 = 8,
        Level9 = 9,
        SuperFast = Level0,
        Fast = Level3,
        Normal = Level5,
        Minimum = Level0,
        Maximum = Level9,
    }
}
