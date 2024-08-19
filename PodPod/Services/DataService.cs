
using AuthenticationServices;
using PodPod.Models;

namespace PodPod.Services;

public class Data
{

    private Data(){}

    public static void init(){
        Data.Podcasts = new List<Podcast>();
    }

    public static List<Podcast> Podcasts { get; set; }
}