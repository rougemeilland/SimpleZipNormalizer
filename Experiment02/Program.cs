using System;
using Utility.IO;

namespace Experiment02
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var file = new FilePath(".temp");
            try
            {
                using (var writer = file.CreateText())
                {
                    writer.WriteLine("Hello.");
                }

                var now = DateTime.UtcNow;

                file.LastWriteTimeUtc = now;
                Console.WriteLine(file.LastWriteTimeUtc.ToLocalTime());

                file.LastWriteTimeUtc -= TimeSpan.FromDays(365);
                Console.WriteLine(file.LastWriteTimeUtc.ToLocalTime());
            }
            finally
            {
                file.Delete();
                Console.WriteLine($"file.Exists = {file.Exists}");
            }

            _ = Console.ReadLine();
        }
    }
}
