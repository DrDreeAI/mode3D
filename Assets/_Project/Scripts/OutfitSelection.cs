using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mode3D.Destinations
{
	[Serializable]
	public class DayOutfit
	{
		public DateTime date;
		public List<OutfitType> outfits = new List<OutfitType>();
		public float temperature;
		public string weather;
	}

	public enum OutfitType
	{
		Chill,
		Sport,
		Business
	}

	public class OutfitSelection : MonoBehaviour
	{
		public static OutfitSelection Instance { get; private set; }

		public List<DayOutfit> dailyOutfits = new List<DayOutfit>();
		public DateTime startDate;
		public DateTime endDate;
		public string selectedDestination;

		private void Awake()
		{
			if (Instance == null)
			{
				Instance = this;
				DontDestroyOnLoad(gameObject);
			}
			else
			{
				Destroy(gameObject);
			}
		}

		public void InitializeDays(DateTime start, DateTime end, string destination)
		{
			startDate = start;
			endDate = end;
			selectedDestination = destination;
			dailyOutfits.Clear();

			DateTime current = start;
			while (current <= end)
			{
				DayOutfit day = new DayOutfit
				{
					date = current,
					temperature = GetMockTemperature(current, destination),
					weather = GetMockWeather(current, destination)
				};
				dailyOutfits.Add(day);
				current = current.AddDays(1);
			}
		}

		private float GetMockTemperature(DateTime date, string destination)
		{
			// Températures simulées selon destination
			System.Random random = new System.Random(date.DayOfYear + destination.GetHashCode());
			
			switch (destination.ToLower())
			{
				case "dubai": return 25f + random.Next(0, 15);
				case "paris": return 10f + random.Next(0, 15);
				case "newyork": return 5f + random.Next(0, 20);
				case "londres": return 8f + random.Next(0, 12);
				default: return 15f + random.Next(0, 10);
			}
		}

		private string GetMockWeather(DateTime date, string destination)
		{
			// Météo simulée
			System.Random random = new System.Random(date.DayOfYear + destination.GetHashCode());
			string[] weathers = { "☀️ Ensoleillé", "⛅ Partiellement nuageux", "☁️ Nuageux", "🌧️ Pluvieux", "⛈️ Orageux" };
			
			int index = random.Next(0, weathers.Length);
			return weathers[index];
		}

		public void AddOutfitToDay(int dayIndex, OutfitType outfit)
		{
			if (dayIndex >= 0 && dayIndex < dailyOutfits.Count)
			{
				if (!dailyOutfits[dayIndex].outfits.Contains(outfit))
				{
					dailyOutfits[dayIndex].outfits.Add(outfit);
				}
			}
		}

		public void RemoveOutfitFromDay(int dayIndex, OutfitType outfit)
		{
			if (dayIndex >= 0 && dayIndex < dailyOutfits.Count)
			{
				dailyOutfits[dayIndex].outfits.Remove(outfit);
			}
		}
	}
}

