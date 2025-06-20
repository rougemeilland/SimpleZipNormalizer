﻿using System;
using System.Linq;
using System.Text;
using Palmtree.Collections;
using Palmtree.IO;

namespace Experiment01
{
    internal static class Program
    {
        static Program()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        private static void Main(string[] args)
        {
            var baseDirectory = new DirectoryPath(args[0]);
            foreach (var n in new[] { 0x7ffeU, 0x7fffU, 0x8000U, 0xfffeU, 0xffffU, 0x10000U })
            {
                var baseDirectory2 = baseDirectory.GetSubDirectory($"{n:N0} files").Create();
                for (var count = 0; count < n; ++count)
                {
                    var file = baseDirectory2.GetFile($"file-{count:N0}.txt");
                    using var writer = file.CreateText();
                    writer.Write(new string([.. RandomSequence.GetAsciiCharSequence().Take(16)]));
                }
            }

            Console.WriteLine("Completed.");
            Console.Beep();
            _ = Console.ReadLine();
        }
    }
}
