using System;
using System.IO;
using System.Linq;
using System.Text;
using Utility;
using Utility.Collections;
using Utility.IO;
using Utility.Linq;

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
            using (var ms = new MemoryStream())
            {
                _ = ms.Seek(0, SeekOrigin.Begin);
                using (var outStream = ms.AsOutputByteStream(true).AsRandomAccess<ulong>().WithCache(8))
                {
                    outStream.Seek(0);
                    outStream.WriteBytes(new byte[] { 1, 2, 3, 4, 5 });
                    outStream.Seek(3);
                    outStream.WriteBytes(new byte[] { 6, 7, 8, 9, 10, 11,});
                    outStream.Flush();
                }

                Console.WriteLine($"random-write: [ {$"{string.Join(", ", ms.ToArray())}]"}");

                _ = ms.Seek(0, SeekOrigin.Begin);
                using (var outStream = ms.AsOutputByteStream(true).AsRandomAccess<ulong>().WithCache(8))
                {
                    outStream.Seek(0);
                    outStream.WriteBytes(new byte[] { 11, 12, 13, 14, 15, });
                    outStream.Seek(5);
                    outStream.WriteBytes(new byte[] { 16, 17, 18, 19, 20, 21,});
                    outStream.Flush();
                }

                Console.WriteLine($"random-write: [ {$"{string.Join(", ", ms.ToArray())}]"}");

                _ = ms.Seek(0, SeekOrigin.Begin);
                using (var outStream = ms.AsOutputByteStream(true).AsRandomAccess<ulong>().WithCache(8))
                {
                    outStream.Seek(0);
                    outStream.WriteBytes(new byte[] { 21, 22, 23, 24, 25, });
                    outStream.Seek(7);
                    outStream.WriteBytes(new byte[] { 26, 27, 28, 29, 30, 31,});
                    outStream.Flush();
                }

                Console.WriteLine($"random-write: [ {$"{string.Join(", ", ms.ToArray())}]"}");
            }

            using (var ms = new MemoryStream(Enumerable.Range(1, 20).Select(n => (byte)n).ToArray()))
            {
                _ = ms.Seek(0, SeekOrigin.Begin);
                using (var inStream = ms.AsInputByteStream(true).AsRandomAccess<ulong>().WithCache(8))
                {
                    inStream.Seek(0);
                    var data1 = inStream.ReadBytes(8);
                    inStream.Seek(5);
                    var data2 = inStream.ReadBytes(8);
                    Console.WriteLine($"random-read: [ {$"{string.Join(", ", data1.AsEnumerable().Concat(data2.AsEnumerable()))}]"}");
                }

                _ = ms.Seek(0, SeekOrigin.Begin);
                using (var inStream = ms.AsInputByteStream(true).AsRandomAccess<ulong>().WithCache(8))
                {
                    inStream.Seek(0);
                    var data1 = inStream.ReadBytes(8);
                    inStream.Seek(7);
                    var data2 = inStream.ReadBytes(8);
                    Console.WriteLine($"random-read: [ {$"{string.Join(", ", data1.AsEnumerable().Concat(data2.AsEnumerable()))}]"}");
                }

                _ = ms.Seek(0, SeekOrigin.Begin);
                using (var inStream = ms.AsInputByteStream(true).AsRandomAccess<ulong>().WithCache(8))
                {
                    inStream.Seek(0);
                    var data1 = inStream.ReadBytes(8);
                    inStream.Seek(8);
                    var data2 = inStream.ReadBytes(8);
                    Console.WriteLine($"random-read: [ {$"{string.Join(", ", data1.AsEnumerable().Concat(data2.AsEnumerable()))}]"}");
                }

                _ = ms.Seek(0, SeekOrigin.Begin);
                using (var inStream = ms.AsInputByteStream(true).AsRandomAccess<ulong>().WithCache(8))
                {
                    inStream.Seek(0);
                    var data1 = inStream.ReadBytes(8);
                    inStream.Seek(9);
                    var data2 = inStream.ReadBytes(8);
                    Console.WriteLine($"random-read: [ {$"{string.Join(", ", data1.AsEnumerable().Concat(data2.AsEnumerable()))}]"}");
                }
            }

            Console.WriteLine("Completed.");
            Console.Beep();
            _ = Console.ReadLine();
        }
    }
}
