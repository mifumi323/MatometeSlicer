using System.IO;

namespace MatometeSlicer
{
    class Program
    {
        static void Main(string[] args)
        {
            var ms = new MatometeSlicer()
            {
                InputFiles = Directory.GetFiles(args[0]),
                OutputFolder = args[1],
                OutputSuffix = args.Length > 2 ? args[2] : @".txt",
                SliceBytes = args.Length > 3 ? int.Parse(args[3]) : 1024 * 1024 * 10,
                AlignLine = args.Length > 4 ? bool.Parse(args[4]) : false,
            };
            ms.Slice();
        }
    }
}
