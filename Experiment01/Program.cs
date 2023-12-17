using System;
using System.IO;
using System.Linq;
using System.Text;
using Utility;
using Utility.Collections;
using Utility.IO;

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
            var baseDirectory = new DirectoryPath(args[0]);
            var commentSize = 0x7ffe;
            var commentBytes = Encoding.UTF8.GetBytes($"This comment is {commentSize:N0} bytes long.\n" + new string(RandomSequence.GetAsciiCharSequence().Take(commentSize).ToArray())).AsReadOnly()[..commentSize];

            baseDirectory.GetFile("comment.txt").WriteAllBytes(commentBytes);

            WriteContentFile(baseDirectory, (0xffffffffLU - 64 * 1024UL * 5 + 64 * 1024LU * 2) / 4);
            WriteContentFile(baseDirectory, (0xffffffffLU - 64 * 1024UL * 5 + 64 * 1024LU * 1) / 4);
            WriteContentFile(baseDirectory, (0xffffffffLU - 64 * 1024UL * 5 + 64 * 1024LU * 0) / 4);
            WriteContentFile(baseDirectory, (0xffffffffLU - 64 * 1024UL * 5 - 64 * 1024LU * 1) / 4);
            WriteContentFile(baseDirectory, (0xffffffffLU - 64 * 1024UL * 5 - 64 * 1024LU * 2) / 4);

            //WriteContentFile(baseDirectory, 0xffffffffLU - 613LU + 64 * 1024LU * 2); // PKZIPを使用して、コメントなしで圧縮すると65538ボリュームのZIPファイルになる
            //WriteContentFile(baseDirectory, 0xffffffffLU - 613LU + 64 * 1024LU * 1); // PKZIPを使用して、コメントなしで圧縮すると65537ボリュームのZIPファイルになる
            //WriteContentFile(baseDirectory, 0xffffffffLU - 613LU + 64 * 1024LU * 0); // PKZIPを使用して、コメントなしで圧縮すると65536ボリュームのZIPファイルになる
            //WriteContentFile(baseDirectory, 0xffffffffLU - 613LU - 64 * 1024LU * 1); // PKZIPを使用して、コメントなしで圧縮すると65535ボリュームのZIPファイルになる
            //WriteContentFile(baseDirectory, 0xffffffffLU - 613LU - 64 * 1024LU * 2); // PKZIPを使用して、コメントなしで圧縮すると65534ボリュームのZIPファイルになる

            Console.WriteLine("Completed.");
            Console.Beep();
            _ = Console.ReadLine();
        }

        private static void WriteContentFile(DirectoryPath baseDirectory, ulong fileSize)
        {
            using (var outStream = new FileStream(baseDirectory.GetFile($"content {fileSize:N0} (0x{fileSize:x16}) bytes.txt").FullName, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                const int BUFFER_LENGTH = 1024 * 1024;
                while (fileSize > 0)
                {
                    var length = checked((int)fileSize.Minimum((ulong)BUFFER_LENGTH));
                    var data = RandomSequence.GetAsciiCharSequence().Select(c => (byte)(int)c).Take(length).ToArray();
                    outStream.WriteBytes(data);
                    checked
                    {
                        fileSize -= (ulong)data.Length;
                    }
                }
            }
        }
    }
}
