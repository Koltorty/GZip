using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GZip
{
    public class CompressionService
    {
        public void Compress(FileInfo file)
        {
            var arrays = Arrays(file);

            try
            {
                //поток для записи сжатого файла
                using (FileStream targetStream = File.Create(file.FullName + ".gz"))
                {
                    //поток для компрессии
                    using (GZipStream compressionStream = new GZipStream(targetStream, CompressionMode.Compress))
                    {
                        using (Stream syncStream = Stream.Synchronized(compressionStream))
                        {
                            for (int i = 0; i < file.Length; i += 1000000)
                            {
                                Thread thread = new Thread(() =>
                                {
                                    byte[] buffer = arrays.Dequeue();
                                    syncStream.Write(buffer, 0, buffer.Length);
                                });
                                thread.Start();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Something went wrong: {ex.InnerException?.Message ?? ex.Message}");
            }

            var info = new FileInfo(file.FullName + ".gz");
            Console.WriteLine($"Compressed {file.Name} from {file.Length.ToString()} to {info.Length.ToString()}");
        }

        public void Decompress(FileInfo file)
        {
            try
            {
                //поток для чтения сжатого файла
                using (FileStream sourceStream = file.OpenRead())
                {
                    string currentFileName = file.FullName;
                    string newFileName = currentFileName.Remove(currentFileName.Length - file.Extension.Length);

                    //поток для записи восстановленного файла
                    using (FileStream targetStream = File.Create(newFileName))
                    {
                        //поток для декомпрессии
                        using (GZipStream decompressionStream = new GZipStream(sourceStream, CompressionMode.Decompress))
                        {
                            using (Stream syncStream = Stream.Synchronized(decompressionStream))
                            {
                                byte[] buffer = new byte[1000000];
                                for (int i = 0; i < sourceStream.Length; i += 1000000)
                                {
                                    syncStream.Read(buffer, 0, buffer.Length);
                                    Thread thread = new Thread(() =>
                                    {
                                        targetStream.Write(buffer, 0, buffer.Length);
                                    });
                                    thread.Start();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Something went wrong: {ex.InnerException.Message ?? ex.Message}");
            }

            Console.WriteLine($"Decompressed {file.Name}");
        }

        private static Queue<byte[]> Arrays(FileInfo file)
        {
            Queue<byte[]> arrays = new Queue<byte[]>();
            using (FileStream sourceStream = file.OpenRead())
            {
                for (int i = 0; i < sourceStream.Length; i += 1000000)
                {
                    byte[] buffer = new byte[1000000];
                    sourceStream.Read(buffer, 0, buffer.Length);
                    arrays.Enqueue(buffer);
                }
            }

            return arrays;
        }
    }
}
