using System.Diagnostics;
using FFMpegCore;
using FFMpegCore.Enums;
using Microsoft.Maui.Storage;
using OPMLCore.NET;
using Podly.FeedParser;
using Whisper.net;
using Whisper.net.Ggml;

namespace PodPod;

public partial class MainPage : ContentPage
{
	int count = 0;

	public MainPage()
	{
		InitializeComponent();

		
	}

	private async void StartTest(){
        Console.WriteLine("Starting test");
		//var opmlPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads", "PocketCasts.opml");
        //var opmlPath = Path.Combine(FileSystem.AppDataDirectory, "podcastlist.opml");
        var fileName = "podcastlist.opml";
        string destPath = Path.Combine(FileSystem.AppDataDirectory, fileName);

        using (var stream = await FileSystem.OpenAppPackageFileAsync(fileName))
        {
            using (var destStream = File.Create(destPath))
            {
                await stream.CopyToAsync(destStream);
            }
        }
        
        try
        {
            Opml opml = new Opml(destPath);

            foreach (Outline outline in opml.Body.Outlines)
            {
                //Console.WriteLine(outline.Text);
                //Console.WriteLine(outline.XMLUrl);

                //output child's output node
                foreach (Outline childOutline in outline.Outlines)
                {
                    if (childOutline.Text == "The Rest Is History")
                    {
                        var feed = GetPodcastFeed(childOutline.XMLUrl);
                        foreach (var item in feed.Items)
                        {
                            if (item.Title.StartsWith("75"))
                            {
                                var filePath = DownloadPodcastEpisode(item.MediaUrl, item.Title);
                                if (filePath != null)
                                {
                                    Console.WriteLine("Downloaded " + item.Title + " to " + filePath);
                                    await TranscribePodcastEpisode(filePath);
                                }
                                else
                                {
                                    Console.WriteLine("Failed to download " + item.Title);
                                }
                            }
                        }
                    }
                }
            }
        } catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
        

        
	}

	private void OnCounterClicked(object sender, EventArgs e)
	{
        StartTest();
    }

	private static IFeed GetPodcastFeed(string url)
    {
        var factory = new HttpFeedFactory();
        var feed = factory.CreateFeed(new Uri(url));

        //Console.WriteLine(feed.Title);
        //Console.WriteLine(feed.LastUpdated);

        foreach (var item in feed.Items)
        {
            //Console.WriteLine(item.Title + " " + item.MediaUrl);
            // Console.WriteLine(item.MediaUrl);
            // Console.WriteLine(item.Content);
        }
        return feed;
    }

    private static string DownloadPodcastEpisode(string url, string title)
    {
        using (var client = new HttpClient())
        {
            var response = client.GetAsync(new Uri(url)).Result;
            if (response.IsSuccessStatusCode)
            {
                string filePath = Path.Combine(FileSystem.AppDataDirectory, title + ".mp3");
                using (var stream = response.Content.ReadAsStreamAsync().Result)
                {
                    
                    //var filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads", title + ".mp3");
                    using (var fileStream = File.Create(filePath))
                    {
                        stream.CopyTo(fileStream);
                        
                    }
                }
                return filePath;
            } else {
                return null;
            }
        }
    }

    private  async static Task<string> TranscribePodcastEpisode(string filePath)
    {
        try {
            var ggmlType = GgmlType.Base;
            var modelFileName = "ggml-base.en.bin";

            // create a path from the folder and the file name
            string modelPath = Path.Combine(FileSystem.AppDataDirectory, "Raw/model", modelFileName);
            string destPath = Path.Combine(FileSystem.AppDataDirectory,  modelFileName);

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
            } else
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
            }

            return "Success";
        } catch (Exception e)
        {
            Console.WriteLine("-----------------");
            Console.WriteLine(e.Message);
            Console.WriteLine("-----------------");
            Console.WriteLine(e.StackTrace);
            Console.WriteLine("-----------------");
            Console.WriteLine(e.InnerException);

            return "Failed";
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

            // check if the file already exists and return it if it does
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

            // Check exit code for success
            // if (process.ExitCode == 0)
            // {
            //     Console.WriteLine("Conversion successful!");
            //     return outputFilePath;
            // }
            // else
            // {
            //     Console.WriteLine($"Conversion failed with exit code {process.ExitCode}");
            //     return null;
            // }
        } catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return null;
        }
    }
}