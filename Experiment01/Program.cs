using System;
using System.Text;
using Utility;
using Utility.Text;

namespace Experiment01
{
    internal class Program
    {
        static Program()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        private static void Main(string[] args)
        {
            var shiftJisEncoding = Encoding.GetEncoding("shift_jis");

            foreach (ShiftJisChar c in ShiftJisChar.EnumerateAllCharacters())
            {
                try
                {
                    var sourceBytes = c.ToByteArray();
                    var s = shiftJisEncoding.GetString(sourceBytes);
                    var bytes = shiftJisEncoding.GetBytes(s);
                    var c2 = bytes.Length >= 2 ? new ShiftJisChar(bytes[0], bytes[1]) : new ShiftJisChar(bytes[0]);
                    if (!bytes.SequenceEqual(sourceBytes.Span))
                        Console.WriteLine($"{c} => {c2}");
                }
                catch (Exception)
                {
                    //Console.WriteLine(c);
                }
            }

            Console.WriteLine("Completed.");
            _ = Console.ReadLine();
        }
    }
}
