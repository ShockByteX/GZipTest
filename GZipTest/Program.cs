using System;
using System.IO;
using System.Threading;
using GZipTest.Compression;
using GZipTest.Compression.GZip;
using GZipTest.Exceptions;
using GZipTest.Helpers;

namespace GZipTest
{
    class Program
    {
        private static int _progressCursorY = -1, _progressCursorX = -1;
        private static CompressionProcessor _processor;
        static void Main(string[] args)
        {
            if (args != null && args.Length > 0)
            {
                try
                {
                    switch (args[0])
                    {
                        case "compress": _processor = new GZipCompressor(); break;
                        case "decompress": _processor = new GZipDecompressor(); break;
                        default: throw new UnrecognizedCommandException(args[0]);
                    }
                    if (args.Length != 3) throw new UnrecognizedCommandException($"{args[0]} arguments");
                    if (!File.Exists(args[1])) throw new FileNotFoundException("File not found!", args[1]);
                    if (File.Exists(args[2]) && !AskYesNo($"File {args[2]} is exists. Overwrite?")) return;
                    _processor.ProgressChanged += processor_ProgressChanged;
                    _processor.ProcessingFinished += processor_ProcessingFinished;
                    Func<Stream> srcStreamFunc = () => File.Open(args[1], FileMode.Open, FileAccess.Read);
                    Func<Stream> dstStreamFunc = () => File.Open(args[2], FileMode.Create, FileAccess.Write);
                    _processor.Run(srcStreamFunc, dstStreamFunc, 1 << 20); // BlockLength - 1 Mb
                    Console.WriteLine("Press [ESC] to cancel process..");
                    while (_processor.IsRunning && Console.ReadKey(true).Key != ConsoleKey.Escape) Thread.Sleep(10);
                    if (_processor.IsRunning) _processor.Cancel();
                    Console.ReadKey(true);
                }
                catch (UnrecognizedCommandException ex)
                {
                    PrintError(ex.Message);
                    PrintHelp();
                }
                catch (Exception ex)
                {
                    PrintError(ex.Message);
                }
            }
            else PrintHelp();
        }
        private static void processor_ProgressChanged(object sender, int value) => PrintProgress($"{value}%");
        private static void processor_ProcessingFinished(object sender, CompressionResult result)
        {
            CompressionProcessor processor = (CompressionProcessor)sender;
            try
            {
                switch (result.Type)
                {
                    case CompressionResultType.Success:
                        PrintProgress($"Finished!");
                        break;
                    case CompressionResultType.Cancelled:
                        DeleteDestinationFile();
                        PrintProgress($"Canceled!");
                        break;
                    case CompressionResultType.Fail:
                        DeleteDestinationFile();
                        throw result.Exception;
                    default: throw new NotSupportedException();
                }
                if (result.Type == CompressionResultType.Fail) throw result.Exception;
            }
            catch (Exception ex)
            {
                PrintError(ex.Message, true);
            }
        }
        private static void DeleteDestinationFile()
        {
            FileInfo file = IOHelper.GetFileFromStream(_processor.GetDestinationStream);
            if (file != null && file.Exists) file.Delete();
        }
        private static bool AskYesNo(string message)
        {
            Console.Write($"{message} (Y/N): ");
            int cursorX = Console.CursorLeft;
            int cursorY = Console.CursorTop;
            ConsoleKey key;
            do
            {
                Console.SetCursorPosition(cursorX, cursorY);
                Console.Write(" ");
                Console.SetCursorPosition(cursorX, cursorY);
                key = Console.ReadKey().Key;
            } while (key != ConsoleKey.Y && key != ConsoleKey.N);
            Console.WriteLine();
            return key == ConsoleKey.Y;
        }
        private static void PrintHelp() => Console.WriteLine("Example: GZipTest.exe compress/decompress [source file path] [destination file path]");
        private static void PrintError(string message, bool preNewLine = false) => Console.WriteLine($"{(preNewLine ? "\n" : string.Empty)}Error: {message}");
        private static void PrintProgress(string message)
        {
            if (_progressCursorY == -1)
            {
                Console.Write("Progress: ");
                _progressCursorX = Console.CursorLeft;
                _progressCursorY = Console.CursorTop;
            }
            Console.SetCursorPosition(_progressCursorX, _progressCursorY);
            Console.Write(message);
        }
    }
}
