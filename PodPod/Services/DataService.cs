using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.Json;
using CommunityToolkit.Maui.Core.Extensions;
using PodPod.Models;

namespace PodPod.Services;

public class Data
{
    public static List<Podcast> podcasts { get; set; }
    public static List<Podcast> Podcasts {
         get => podcasts;
         set {
             podcasts = value;
             SaveToJsonFile(podcasts, "podcasts");
         }
    }

    public static ObservableCollection<Podcast> PodcastsObservable = new ObservableCollection<Podcast>();

    private Data(){}

    public static void init(){
        if (SaveFileExists("podcasts"))
        {
            Debug.WriteLine("Loading podcasts from file at init");
            Podcasts = LoadFromJsonFile<List<Podcast>>("podcasts");
            PodcastsObservable = Podcasts.ToObservableCollection();
        } else {
            Podcasts = new List<Podcast>();
        }
        AppPaths.InitDirectories();
    }


    public static bool SaveFileExists(string fileName)
    {
        string filePath = Path.Combine(AppPaths.DataDirectory, $"{fileName}.json");
        return File.Exists(filePath);
    }

    private static bool FirstLoad = true;
    public static async Task SaveToJsonFile<T>(T data, string fileName)
    {
        if(FirstLoad){
            FirstLoad = false;
            return;
        }

        Debug.WriteLine("Saving to file");
        try {
            _ = Task.Run(() => {
                string filePath = Path.Combine(AppPaths.DataDirectory, $"{fileName}.json");
                string json = JsonSerializer.Serialize(data);
                File.WriteAllText(filePath, json);
                Debug.WriteLine("Saved to file");
            });
        } catch (Exception e){
            Debug.WriteLine(e.Message);
            Debug.WriteLine("Failed to save to file");
        }
        return;
    }

    public static T LoadFromJsonFile<T>(string fileName)
    {
        string filePath = Path.Combine(AppPaths.DataDirectory, $"{fileName}.json");
        
        if (File.Exists(filePath)){
            try {
                string json = File.ReadAllText(filePath);
                return JsonSerializer.Deserialize<T>(json);
            }catch (Exception e){
                Console.WriteLine(e.Message);   
            }
        }
        return default;
    }
}