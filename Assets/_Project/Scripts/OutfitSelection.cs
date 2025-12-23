using System;
using System.Collections;
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

		private Mode3D.Weather.WeatherManager weatherManager;

		private void Awake()
		{
			if (Instance == null)
			{
				Instance = this;
				DontDestroyOnLoad(gameObject);

				// Créer le WeatherManager
				GameObject weatherGO = new GameObject("WeatherManager");
				weatherGO.transform.SetParent(transform);
				weatherManager = weatherGO.AddComponent<Mode3D.Weather.WeatherManager>();
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
					temperature = 0f, // Sera mis à jour par l'API
					weather = "⏳ Chargement..."
				};
				dailyOutfits.Add(day);
				current = current.AddDays(1);
			}

			// Récupérer la météo réelle via l'API
			StartCoroutine(FetchRealWeatherForAllDays());
		}

		private IEnumerator FetchRealWeatherForAllDays()
		{
			if (weatherManager == null) yield break;

			int tripDuration = dailyOutfits.Count;
			bool weatherFetched = false;

			// Récupérer les prévisions pour tous les jours du voyage
			weatherManager.FetchForecast(selectedDestination, tripDuration,
				onSuccess: (forecasts) => {
					// Associer chaque prévision à chaque jour
					for (int i = 0; i < dailyOutfits.Count; i++)
					{
						var day = dailyOutfits[i];

						// Trouver la prévision correspondante à cette date
						var forecast = forecasts.Find(f => f.date.Date == day.date.Date);

						if (forecast != null)
						{
							day.temperature = forecast.weatherData.temperature;
							day.weather = Mode3D.Weather.WeatherManager.GetWeatherEmoji(forecast.weatherData.description) + " " + forecast.weatherData.description;
							Debug.Log($"Météo jour {i+1} ({day.date:dd/MM}): {day.temperature}°C - {day.weather}");
						}
						else
						{
							// Si pas de prévision pour ce jour (>14 jours)
							day.temperature = 0f;
							day.weather = "⚠️ La météo n'est pas prévisible à cette date";
						}
					}
					weatherFetched = true;
					Debug.Log($"Prévisions météo récupérées pour {selectedDestination}: {forecasts.Count} jours");
				},
				onError: (error) => {
					Debug.LogWarning($"Erreur météo API, utilisation de données simulées: {error}");
					UseMockWeatherData();
					weatherFetched = true;
				}
			);

			// Attendre que la météo soit récupérée
			float timeout = 10f;
			float elapsed = 0f;
			while (!weatherFetched && elapsed < timeout)
			{
				yield return new WaitForSeconds(0.1f);
				elapsed += 0.1f;
			}

			if (!weatherFetched)
			{
				Debug.LogWarning("Timeout météo API, utilisation de données simulées");
				UseMockWeatherData();
			}
		}

		private void UseMockWeatherData()
		{
			foreach (var day in dailyOutfits)
			{
				day.temperature = GetMockTemperature(day.date, selectedDestination);
				day.weather = GetMockWeather(day.date, selectedDestination);
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

