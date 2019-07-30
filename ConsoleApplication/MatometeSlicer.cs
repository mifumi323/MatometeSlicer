using System;
using System.Collections.Generic;
using System.IO;

namespace MatometeSlicer
{
    public class MatometeSlicer
    {
        public IEnumerable<string> InputFiles { get; set; } = null;
        public int SliceBytes { get; set; } = 1024 * 1024 * 100;
        public string OutputFolder { get; set; } = null;
        public string OutputSuffix { get; set; } = "";
        public int BufferSize { get; set; } = 1024 * 1024;
        public bool AlignLine { get; set; } = false;

        private HashSet<byte> skipValues = new HashSet<byte>()
        {
            (byte)' ', (byte)'!', (byte)'"', (byte)'#', (byte)'$', (byte)'%', (byte)'&', (byte)'\'',
            (byte)'(', (byte)')', (byte)'*', (byte)'+', (byte)',', (byte)'-', (byte)'.', (byte)'/',
            (byte)'0', (byte)'1', (byte)'2', (byte)'3', (byte)'4', (byte)'5', (byte)'6', (byte)'7',
            (byte)'8', (byte)'9', (byte)':', (byte)';', (byte)'<', (byte)'=', (byte)'>', (byte)'?',
            (byte)'@', (byte)'A', (byte)'B', (byte)'C', (byte)'D', (byte)'E', (byte)'F', (byte)'G',
            (byte)'H', (byte)'I', (byte)'J', (byte)'K', (byte)'L', (byte)'M', (byte)'N', (byte)'O',
            (byte)'P', (byte)'Q', (byte)'R', (byte)'S', (byte)'T', (byte)'U', (byte)'V', (byte)'W',
            (byte)'X', (byte)'Y', (byte)'Z', (byte)'[', (byte)'\\',(byte)']', (byte)'^', (byte)'_',
            (byte)'`', (byte)'a', (byte)'b', (byte)'c', (byte)'d', (byte)'e', (byte)'f', (byte)'g',
            (byte)'h', (byte)'i', (byte)'j', (byte)'k', (byte)'l', (byte)'m', (byte)'n', (byte)'o',
            (byte)'p', (byte)'q', (byte)'r', (byte)'s', (byte)'t', (byte)'u', (byte)'v', (byte)'w',
            (byte)'x', (byte)'y', (byte)'z', (byte)'{', (byte)'|', (byte)'}', (byte)'~', (byte)'\r',
        };
        private bool[] skipTable = new bool[256];

        public void Slice()
        {
            var inputFiles = InputFiles;
            var remainBytes = 0;
            FileStream outputStream = null;
            var outputNumber = 0;
            var outputFilePath = "";
            var buffer = new byte[BufferSize];
            var actualReadBytes = 0;
            for (var i = 0; i < skipTable.Length; i++)
            {
                skipTable[i] = skipValues.Contains((byte)i);
            }
            foreach (var inputFile in inputFiles)
            {
                Console.WriteLine(inputFile);
                using (var inputStream = File.OpenRead(inputFile))
                {
                    while (true)
                    {
                        if (remainBytes == 0)
                        {
                            remainBytes = SliceBytes;
                            if (outputStream != null)
                            {
                                if (AlignLine && actualReadBytes > 0 && !IsLineEnd(buffer[actualReadBytes - 1]))
                                {
                                    while (true)
                                    {
                                        actualReadBytes = inputStream.Read(buffer, 0, 1);
                                        if (actualReadBytes == 0)
                                        {
                                            break;
                                        }
                                        var v = buffer[0];
                                        if (skipTable[v])
                                        {
                                            continue;
                                        }
                                        outputStream.WriteByte(v);
                                        if (IsLineEnd(v))
                                        {
                                            break;
                                        }
                                    }
                                }
                                outputStream.Dispose();
                            }
                            outputNumber++;
                            outputFilePath = Path.Combine(OutputFolder, $"{outputNumber:0000}{OutputSuffix}");
                            outputStream = File.OpenWrite(outputFilePath);
                        }
                        var readBytes = Math.Min(BufferSize, remainBytes);
                        actualReadBytes = inputStream.Read(buffer, 0, readBytes);
                        if (actualReadBytes == 0) break;
                        remainBytes -= actualReadBytes;
                        var actualWriteBytes = AlignLine ? Trim(buffer, actualReadBytes) : actualReadBytes;
                        outputStream.Write(buffer, 0, actualWriteBytes);
                    }
                }
            }
            if (outputStream != null)
            {
                outputStream.Dispose();
                if (new FileInfo(outputFilePath).Length == 0)
                {
                    File.Delete(outputFilePath);
                }
            }
        }

        private bool IsLineEnd(byte v) => v == '\n' || v == 0;

        private int Trim(byte[] buffer, int length)
        {
            var j = 0;
            byte prev = 0;
            for (var i = 0; i < length; i++)
            {
                var v = buffer[i];
                if (skipTable[v])
                {
                    continue;
                }
                if (v < 128 && v == prev)
                {
                    continue;
                }
                if (j != i)
                {
                    buffer[j] = buffer[i];
                }
                prev = v;
                j++;
            }

            return j;
        }
    }
}
