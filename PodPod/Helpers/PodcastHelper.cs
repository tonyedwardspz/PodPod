using System;
using PodPod.Models;
using PodPod.Services;
namespace PodPod.Helpers
{
	public class PodcastHelper
	{
		public static List<Episode> GetLatestEpisodes(int count = 50)
		{
			
			var allEpisodes = Data.Podcasts
				.SelectMany(p => p.Episodes)
				.OrderByDescending(e => e.Published)
				.Take(count)
				.ToList();

			return allEpisodes;
		}
	}
}

