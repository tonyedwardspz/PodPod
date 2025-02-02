﻿using FFMpegCore;
using PodPod.Models;
using Whisper.net;

namespace PodPod.Services;

public static class TranscriptionService
{

    public static async Task<bool> StartTranscriptionAsync(Episode episode, string seriesName)
    {
        return await Task.Run(() => TranscribePodcastEpisode(episode.MediaURL, episode, seriesName));
    }

    public static async Task<bool> TranscribePodcastEpisode(string filePath, Episode episode, string seriesName)
    {
        
        var spanDataList = new List<Dictionary<string, object>>();
        try
        {
            var modelFileName = "ggml-base.en.bin";
            string modelPath = Path.Combine(AppPaths.DataDirectory, "Raw/model", modelFileName);
            string destPath = Path.Combine(AppPaths.DataDirectory, modelFileName);

            if (!File.Exists(destPath))
            {
                Console.WriteLine("Moving model to the right place");
                using (var stream = await FileSystem.OpenAppPackageFileAsync(modelFileName))
                {
                    using (var destStream = File.Create(destPath))
                    {
                        await stream.CopyToAsync(destStream);
                    }
                }
            }

            var WavPath = await ConvertMp3ToWav(filePath, episode.FileName);
            using var WavStream = File.OpenRead(WavPath);

            episode.TranscriptionButtonText = "Transcribing";

            using var whisperFactory = WhisperFactory.FromPath(destPath);
            using var processor = whisperFactory.CreateBuilder()
                .WithLanguage("en")
                .Build();

            Console.WriteLine("Starting transcription at " + DateTime.Now);
            List<TranscriptionItem> items = new List<TranscriptionItem>();
            await foreach (var result in processor.ProcessAsync(WavStream))
            {
                items.Add(new TranscriptionItem
                {
                    Start = result.Start,
                    End = result.End,
                    Text = result.Text
                });
            }
            
            Console.WriteLine("Finished transcription at " + DateTime.Now);
            Transcription transcript = new Transcription { Items = items};
            episode.TranscriptionButtonText = "Transcribed";
            episode.Transcription = transcript;

            _ = Task.Run(() => episode.SaveTranscription(seriesName, episode.Transcription));
            _ = Task.Run(() => File.Delete(WavPath));
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
        return false;
    }

    private async static Task<string> ConvertMp3ToWav(string inputFilePath, string fileName)
    {
        Console.WriteLine("Converting MP3 to WAV");

        try {
            var outputFilePath = Path.Combine(AppPaths.TempDirectory, fileName + Guid.NewGuid().ToString() + ".wav");

            if (File.Exists(outputFilePath))
            {
                Console.WriteLine("WAV file already exists");
                return outputFilePath;
            }

            await Task.Run(() =>
            {
                GlobalFFOptions.Configure(new FFOptions { BinaryFolder = "/opt/homebrew/bin", TemporaryFilesFolder = "/tmp", WorkingDirectory = "/tmp" });
                var outputStream = new MemoryStream();

                FFMpegArguments
                    .FromFileInput(inputFilePath)
                    .OutputToFile(outputFilePath, false, options => options
                    .WithCustomArgument("-ac 1 -ar 16000 -sample_fmt s16"))
                    .ProcessSynchronously();

                outputStream.Position = 0;
                Console.WriteLine("WAV file created: ");
            });

            return outputFilePath;
        } catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return null;
        }
    }
}
