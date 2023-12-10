using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Experiment01
{
    internal class Program
    {
        static Program()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        private static void Main()
        {
            Console.WriteLine($"{nameof(Path.VolumeSeparatorChar)}='{Path.VolumeSeparatorChar}'");
            Console.WriteLine($"{nameof(Path.DirectorySeparatorChar)}='{Path.DirectorySeparatorChar}'");
            Console.WriteLine($"{nameof(Path.AltDirectorySeparatorChar)}='{Path.AltDirectorySeparatorChar}'");
            Console.WriteLine($"{nameof(Path.PathSeparator)}='{Path.PathSeparator}'");
            Console.WriteLine($"{nameof(Path.GetInvalidFileNameChars)}()=[{FromCharSequenceToString(Path.GetInvalidFileNameChars())}]");
            Console.WriteLine($"{nameof(Path.GetInvalidPathChars)}()=[{FromCharSequenceToString(Path.GetInvalidPathChars())}]");

            _ = Console.ReadLine();
        }

        private static string FromCharSequenceToString(IEnumerable<char> characters)
            => string.Join(
                ", ",
                characters
                    .OrderBy(c => c)
                    .Select(c =>
                        c is < ' ' or '\x7f'
                        ? $"'\\x{(int)c:x2}'"
                        : c == '\''
                        ? "\\'"
                        : $"'{c}'"));
    }
}
