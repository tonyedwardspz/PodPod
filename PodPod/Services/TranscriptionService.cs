using System;
using FFMpegCore;
using PodPod.Models;
using Whisper.net;
using Whisper.net.Ggml;

namespace PodPod.Services;

public static class TranscriptionService
{

    public static async Task<Episode> StartTranslationAsync(string filePath, Episode episode)
    {
        return await Task.Run(() => TranscribePodcastEpisode(filePath, episode));
    }

    public static async Task<Episode> TranscribePodcastEpisode(string filePath, Episode episode)
    {

        var spanDataList = new List<Dictionary<string, object>>();
        try
        {
            var ggmlType = GgmlType.Base;
            var modelFileName = "ggml-base.en.bin";
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


            using var processor = whisperFactory.CreateBuilder()
                .WithLanguage("auto")
                .Build();

            Console.WriteLine("Starting transcription at " + DateTime.Now);

            // This section processes the audio file and prints the results (start time, end time and text) to the console.
            await foreach (var result in processor.ProcessAsync(WavStream))
            {
                spanDataList.Add(new Dictionary<string, object>
                {
                    { "Start", result.Start },
                    { "End", result.End },
                    { "Text", result.Text }
                });
            }
            Console.WriteLine("Finished transcription at " + DateTime.Now);
            episode.Transcription = spanDataList;
            
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
        return episode;
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
