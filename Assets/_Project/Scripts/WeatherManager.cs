using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Mode3D.Weather
{
	/// <summary>
	/// Gestionnaire de météo utilisant l'API WeatherAPI.com avec prévisions
	/// </summary>
	public class WeatherManager : MonoBehaviour
	{
		private const string API_KEY = "c9d7634e370347fb9d6101237251912";
		private const string BASE_URL = "https://api.weatherapi.com/v1/forecast.json";

		[Header("Settings")]
		[SerializeField] private bool autoFetchOnStart = false;
		[SerializeField] private string defaultCity = "Paris";

		[Serializable]
		public class WeatherData
		{
			public string cityName;
			public float temperature;
			public float feelsLike;
			public int humidity;
			public float windSpeed;
			public string description;
			public string weatherMain;
			public DateTime date;
		}

		[Serializable]
		public class DailyForecast
		{
			public DateTime date;
			public WeatherData weatherData;
		}

		void Start()
		{
			if (autoFetchOnStart && !string.IsNullOrEmpty(defaultCity))
			{
				FetchForecast(defaultCity, 7, null, null);
			}
		}

		/// <summary>
		/// Récupère les prévisions météo pour une ville et plusieurs jours
		/// </summary>
		public void FetchForecast(string cityName, int days, Action<List<DailyForecast>> onSuccess, Action<string> onError)
		{
			StartCoroutine(FetchForecastCoroutine(cityName, days, onSuccess, onError));
		}

		/// <summary>
		/// Récupère la météo pour une date spécifique
		/// </summary>
		public void FetchWeatherForDate(string cityName, DateTime targetDate, Action<WeatherData> onSuccess, Action<string> onError)
		{
			StartCoroutine(FetchWeatherForDateCoroutine(cityName, targetDate, onSuccess, onError));
		}

		private IEnumerator FetchForecastCoroutine(string cityName, int days, Action<List<DailyForecast>> onSuccess, Action<string> onError)
		{
			// Limiter à 14 jours max (limite de l'API)
			days = Mathf.Min(days, 14);

			string url = $"{BASE_URL}?key={API_KEY}&q={UnityWebRequest.EscapeURL(cityName)}&days={days}&lang=fr";

			Debug.Log($"[WeatherManager] Requête API: {url}");

			using (UnityWebRequest request = UnityWebRequest.Get(url))
			{
				yield return request.SendWebRequest();

				if (request.result == UnityWebRequest.Result.Success)
				{
					try
					{
						string jsonResponse = request.downloadHandler.text;
						Debug.Log($"[WeatherManager] Réponse JSON reçue: {jsonResponse.Substring(0, Mathf.Min(500, jsonResponse.Length))}...");

						List<DailyForecast> forecasts = ParseForecastJson(jsonResponse);

						if (forecasts.Count > 0)
						{
							Debug.Log($"[WeatherManager] {forecasts.Count} prévisions parsées avec succès");
							onSuccess?.Invoke(forecasts);
						}
						else
						{
							string error = "Aucune prévision trouvée dans la réponse";
							Debug.LogError($"[WeatherManager] {error}");
							onError?.Invoke(error);
						}
					}
					catch (Exception e)
					{
						string error = $"Erreur lors du parsing de la météo: {e.Message}";
						Debug.LogError($"[WeatherManager] {error}");
						onError?.Invoke(error);
					}
				}
				else
				{
					string error = $"Erreur lors de la récupération de la météo: {request.error}";
					Debug.LogError($"[WeatherManager] {error}");
					onError?.Invoke(error);
				}
			}
		}

		private IEnumerator FetchWeatherForDateCoroutine(string cityName, DateTime targetDate, Action<WeatherData> onSuccess, Action<string> onError)
		{
			DateTime today = DateTime.Today;
			int daysFromToday = (targetDate.Date - today).Days;

			// Vérifier si la date est dans le futur et dans la limite de 14 jours
			if (daysFromToday < 0)
			{
				onError?.Invoke("La météo n'est pas prévisible à cette date (date passée)");
				yield break;
			}

			if (daysFromToday > 14)
			{
				onError?.Invoke("La météo n'est pas prévisible à cette date");
				yield break;
			}

			// Récupérer les prévisions jusqu'à la date cible
			int daysToFetch = Mathf.Min(daysFromToday + 1, 14);

			string url = $"{BASE_URL}?key={API_KEY}&q={UnityWebRequest.EscapeURL(cityName)}&days={daysToFetch}&lang=fr";

			using (UnityWebRequest request = UnityWebRequest.Get(url))
			{
				yield return request.SendWebRequest();

				if (request.result == UnityWebRequest.Result.Success)
				{
					try
					{
						string jsonResponse = request.downloadHandler.text;
						List<DailyForecast> forecasts = ParseForecastJson(jsonResponse);

						// Chercher la date cible dans les prévisions
						foreach (var forecast in forecasts)
						{
							if (forecast.date.Date == targetDate.Date)
							{
								Debug.Log($"[WeatherManager] Météo pour {targetDate:dd/MM/yyyy}: {forecast.weatherData.temperature}°C, {forecast.weatherData.description}");
								onSuccess?.Invoke(forecast.weatherData);
								yield break;
							}
						}

						// Si on arrive ici, la date n'a pas été trouvée
						onError?.Invoke("La météo n'est pas prévisible à cette date");
					}
					catch (Exception e)
					{
						string error = $"Erreur lors du parsing de la météo: {e.Message}";
						Debug.LogError(error);
						onError?.Invoke(error);
					}
				}
				else
				{
					string error = $"Erreur lors de la récupération de la météo: {request.error}";
					Debug.LogError(error);
					onError?.Invoke(error);
				}
			}
		}

		/// <summary>
		/// Parse le JSON manuellement car JsonUtility ne supporte pas bien les structures complexes
		/// </summary>
		private List<DailyForecast> ParseForecastJson(string json)
		{
			List<DailyForecast> forecasts = new List<DailyForecast>();

			try
			{
				// Extraire le nom de la ville
				string cityName = ExtractJsonValue(json, "\"name\"");

				// Trouver le tableau forecastday
				int forecastIndex = json.IndexOf("\"forecastday\":");
				if (forecastIndex == -1)
				{
					Debug.LogError("[WeatherManager] forecastday non trouvé dans le JSON");
					return forecasts;
				}

				string forecastSection = json.Substring(forecastIndex);
				int arrayStart = forecastSection.IndexOf('[');
				int arrayEnd = FindMatchingBracket(forecastSection, arrayStart);

				if (arrayStart == -1 || arrayEnd == -1)
				{
					Debug.LogError("[WeatherManager] Tableau forecastday mal formaté");
					return forecasts;
				}

				string forecastArray = forecastSection.Substring(arrayStart + 1, arrayEnd - arrayStart - 1);

				// Découper chaque jour
				List<string> dayObjects = SplitJsonObjects(forecastArray);

				foreach (string dayJson in dayObjects)
				{
					try
					{
						// Extraire les données du jour
						string dateStr = ExtractJsonValue(dayJson, "\"date\"");
						DateTime date = DateTime.Parse(dateStr);

						// Trouver la section "day"
						int dayIndex = dayJson.IndexOf("\"day\":");
						if (dayIndex == -1) continue;

						string daySection = dayJson.Substring(dayIndex);
						int dayObjStart = daySection.IndexOf('{');
						int dayObjEnd = FindMatchingBrace(daySection, dayObjStart);

						if (dayObjStart == -1 || dayObjEnd == -1) continue;

						string dayData = daySection.Substring(dayObjStart, dayObjEnd - dayObjStart + 1);

						// Extraire les valeurs
						float avgTemp = float.Parse(ExtractJsonValue(dayData, "\"avgtemp_c\""), System.Globalization.CultureInfo.InvariantCulture);
						int humidity = int.Parse(ExtractJsonValue(dayData, "\"avghumidity\""));
						float windKph = float.Parse(ExtractJsonValue(dayData, "\"maxwind_kph\""), System.Globalization.CultureInfo.InvariantCulture);

						// Extraire la condition
						string conditionText = ExtractJsonValue(dayData, "\"text\"");

						DailyForecast forecast = new DailyForecast
						{
							date = date,
							weatherData = new WeatherData
							{
								cityName = cityName,
								temperature = avgTemp,
								feelsLike = avgTemp,
								humidity = humidity,
								windSpeed = windKph / 3.6f, // km/h vers m/s
								description = conditionText,
								weatherMain = conditionText,
								date = date
							}
						};

						forecasts.Add(forecast);
						Debug.Log($"[WeatherManager] Jour parsé: {date:dd/MM/yyyy} - {avgTemp}°C - {conditionText}");
					}
					catch (Exception e)
					{
						Debug.LogWarning($"[WeatherManager] Erreur parsing d'un jour: {e.Message}");
					}
				}
			}
			catch (Exception e)
			{
				Debug.LogError($"[WeatherManager] Erreur parsing JSON: {e.Message}");
			}

			return forecasts;
		}

		private string ExtractJsonValue(string json, string key)
		{
			int keyIndex = json.IndexOf(key);
			if (keyIndex == -1) return "";

			int colonIndex = json.IndexOf(':', keyIndex);
			if (colonIndex == -1) return "";

			int valueStart = colonIndex + 1;
			while (valueStart < json.Length && (json[valueStart] == ' ' || json[valueStart] == '\t' || json[valueStart] == '\n'))
				valueStart++;

			if (valueStart >= json.Length) return "";

			char firstChar = json[valueStart];

			if (firstChar == '"')
			{
				// String value
				int valueEnd = json.IndexOf('"', valueStart + 1);
				if (valueEnd == -1) return "";
				return json.Substring(valueStart + 1, valueEnd - valueStart - 1);
			}
			else
			{
				// Number or boolean
				int valueEnd = valueStart;
				while (valueEnd < json.Length && json[valueEnd] != ',' && json[valueEnd] != '}' && json[valueEnd] != ']')
					valueEnd++;
				return json.Substring(valueStart, valueEnd - valueStart).Trim();
			}
		}

		private int FindMatchingBracket(string json, int startIndex)
		{
			int depth = 0;
			for (int i = startIndex; i < json.Length; i++)
			{
				if (json[i] == '[') depth++;
				else if (json[i] == ']')
				{
					depth--;
					if (depth == 0) return i;
				}
			}
			return -1;
		}

		private int FindMatchingBrace(string json, int startIndex)
		{
			int depth = 0;
			for (int i = startIndex; i < json.Length; i++)
			{
				if (json[i] == '{') depth++;
				else if (json[i] == '}')
				{
					depth--;
					if (depth == 0) return i;
				}
			}
			return -1;
		}

		private List<string> SplitJsonObjects(string jsonArray)
		{
			List<string> objects = new List<string>();
			int objStart = -1;
			int depth = 0;

			for (int i = 0; i < jsonArray.Length; i++)
			{
				char c = jsonArray[i];

				if (c == '{')
				{
					if (depth == 0) objStart = i;
					depth++;
				}
				else if (c == '}')
				{
					depth--;
					if (depth == 0 && objStart != -1)
					{
						objects.Add(jsonArray.Substring(objStart, i - objStart + 1));
						objStart = -1;
					}
				}
			}

			return objects;
		}

		/// <summary>
		/// Retourne l'emoji correspondant à la condition météo
		/// </summary>
		public static string GetWeatherEmoji(string weatherDescription)
		{
			if (string.IsNullOrEmpty(weatherDescription)) return "🌤️";

			string desc = weatherDescription.ToLower();

			if (desc.Contains("ensoleillé") || desc.Contains("sunny") || desc.Contains("clear") || desc.Contains("dégagé"))
				return "☀️";
			if (desc.Contains("nuageux") || desc.Contains("cloudy") || desc.Contains("overcast") || desc.Contains("couvert"))
				return "☁️";
			if (desc.Contains("pluie") || desc.Contains("rain") || desc.Contains("drizzle") || desc.Contains("bruine"))
				return "🌧️";
			if (desc.Contains("orage") || desc.Contains("thunder"))
				return "⛈️";
			if (desc.Contains("neige") || desc.Contains("snow"))
				return "❄️";
			if (desc.Contains("brouillard") || desc.Contains("fog") || desc.Contains("mist") || desc.Contains("brume"))
				return "🌫️";
			if (desc.Contains("partiellement") || desc.Contains("partly"))
				return "⛅";

			return "🌤️";
		}
	}
}
