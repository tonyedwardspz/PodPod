using System;
using FFMpegCore;
using Whisper.net;
using Whisper.net.Ggml;

namespace PodPod.Services;

public static class TranscriptionService
{
    public static async void TranscribePodcastEpisode(string filePath)
    {
        try
        {
            var ggmlType = GgmlType.Base;
            var modelFileName = "ggml-base.en.bin";

            // create a path from the folder and the file name
            string modelPath = Path.Combine(FileSystem.AppDataDirectory, "Raw/model", modelFileName);
            string destPath = Path.Combine(FileSystem.AppDataDirectory, modelFileName);

            using (var stream = await FileSystem.OpenAppPackageFileAsync(modelFileName))
            {
                using (var destStream = File.Create(destPath))
                {
                    await stream.CopyToAsync(destStream);
                }
            }

            if (!File.Exists(destPath))
            {
                Console.WriteLine("Model not found");
                await DownloadModel(modelFileName, ggmlType);
            }
            else
            {
                Console.WriteLine("Model found");
            }

            var WavPath = ConvertMp3ToWav(filePath);
            using var WavStream = File.OpenRead(WavPath);

            // This section creates the whisperFactory object which is used to create the processor object.
            using var whisperFactory = WhisperFactory.FromPath(destPath);

            // This section creates the processor object which is used to process the audio file, it uses language `auto` to detect the language of the audio file.
            using var processor = whisperFactory.CreateBuilder()
                .WithLanguage("auto")
                .Build();

            Console.WriteLine("Starting transcription at " + DateTime.Now);

            // This section processes the audio file and prints the results (start time, end time and text) to the console.
            await foreach (var result in processor.ProcessAsync(WavStream))
            {
                Console.WriteLine($"{result.Start}->{result.End}: {result.Text}");
                // Create a new label with a formatted string to match the cosole log abovve
                // Add the label to the UI
                Label label = new Label();
                label.FormattedText = new FormattedString
                {
                    Spans = {
                        new Span { Text = $"{result.Start}->{result.End}: ", FontAttributes = FontAttributes.Bold },
                        new Span { Text = result.Text }
                    }
                };

                // Add the label to the UI
                
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("-----------------");
            Console.WriteLine(e.Message);
            Console.WriteLine("-----------------");
            Console.WriteLine(e.StackTrace);
            Console.WriteLine("-----------------");
            Console.WriteLine(e.InnerException);
        }
    }

    private static async Task DownloadModel(string fileName, GgmlType ggmlType)
    {
        Console.WriteLine($"Downloading Model {fileName}");
        using var modelStream = await WhisperGgmlDownloader.GetGgmlModelAsync(ggmlType);
        using var fileWriter = File.OpenWrite(fileName);
        await modelStream.CopyToAsync(fileWriter);
    }

    private static string ConvertMp3ToWav(string inputFilePath)
    {
        Console.WriteLine("Converting MP3 to WAV");
        GlobalFFOptions.Configure(new FFOptions { BinaryFolder = "/opt/homebrew/bin", TemporaryFilesFolder = "/tmp", WorkingDirectory = "/tmp" });

        try {
            var outputFilePath = Path.ChangeExtension(inputFilePath, ".wav");
            Console.WriteLine("Output file path: " + outputFilePath);

            if (File.Exists(outputFilePath))
            {
                Console.WriteLine("WAV file already exists");
                File.Delete(outputFilePath);
            }

            var outputStream = new MemoryStream();

            FFMpegArguments
                .FromFileInput(inputFilePath)
                .OutputToFile(outputFilePath, false, options => options
                .WithCustomArgument("-ac 1 -ar 16000 -sample_fmt s16"))
                .ProcessSynchronously();

            outputStream.Position = 0;
            var size = outputStream.Length;
            Console.WriteLine("WAV file created: ", size);

            return outputFilePath;
        } catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return null;
        }
    }
}
