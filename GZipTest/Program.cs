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
        static void Main(string[] args)
        {
            if (args != null && args.Length > 0)
            {
                CompressionProcessor processor;
                try
                {
                    switch (args[0])
                    {
                        case "compress": processor = new GZipCompressor(); break;
                        case "decompress": processor = new GZipDecompressor(); break;
                        default: throw new UnrecognizedCommandException(args[0]);
                    }
                    if (args.Length != 3) throw new UnrecognizedCommandException($"Invalid command arguments.", args[0]);
                    if (!File.Exists(args[1])) throw new FileNotFoundException("File not found!", args[1]);
                    if (File.Exists(args[2]) && !AskYesNo($"File {args[2]} is exists. Overwrite?")) return;
                    processor.ProgressChanged += Processor_ProgressChanged;
                    processor.ProcessingFinished += Processor_ProcessingFinished;
                    processor.Run(() => File.Open(args[1], FileMode.Open, FileAccess.Read), () => File.Open(args[2], FileMode.Create, FileAccess.Write), 1 << 20);
                    Console.WriteLine("Press [ESC] to cancel process..");
                    while (processor.IsRunning && Console.ReadKey(true).Key != ConsoleKey.Escape) Thread.Sleep(10);
                    if (processor.IsRunning) processor.Cancel();
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
        private static void Processor_ProgressChanged(object sender, int value) => PrintProgress($"{value}%");
        private static void Processor_ProcessingFinished(object sender, CompressionResult result)
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
                        DeleteDestinationFile(processor);
                        PrintProgress($"Canceled!");
                        break;
                    case CompressionResultType.Fail:
                        DeleteDestinationFile(processor);
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
        private static void DeleteDestinationFile(CompressionProcessor processor)
        {
            FileInfo file = IOHelper.GetFileFromStream(processor.GetDestinationStream);
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
