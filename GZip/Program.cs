using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace GZip
{
    class Program
    {
        static void Main(string[] args)
        {
            var watch = Stopwatch.StartNew();
            var compressionService = new CompressionService();
            var command = args[0];
            var filePath = args[1];

            try
            {
                if (string.Equals(command, "compress") || string.Equals(command, "decompress"))
                {
                    var file = new FileInfo(filePath);
                    if (string.Equals(command, "compress"))
                    {
                        Console.WriteLine("Compressing started...");
                        compressionService.Compress(file);
                    }
                    else if (string.Equals(command, "decompress"))
                    {
                        if (file.Extension != ".gz")
                        {
                            Console.WriteLine("Incorrect file format");
                        }
                        else
                        {
                            Console.WriteLine("Decompressing started...");
                            compressionService.Decompress(file);
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Your command is incorrect. Please input correct command 'compress' or 'decompress'");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Something went wrong: {ex.InnerException?.Message ?? ex.Message} \n Please check your data and try again.");
            }

            watch.Stop();
            Console.WriteLine($"Elapsed time: {watch.Elapsed.Minutes}:{watch.Elapsed.Seconds}");
            Console.ReadKey();
        }
    }
}
