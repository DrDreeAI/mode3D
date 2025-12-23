using UnityEngine;
using Mode3D.Weather;

/// <summary>
/// Script de test pour vérifier que l'API météo fonctionne
/// À attacher à un GameObject vide pour tester
/// </summary>
public class WeatherTester : MonoBehaviour
{
    private WeatherManager weatherManager;

    void Start()
    {
        Debug.Log("=== WEATHER TESTER DÉMARRÉ ===");

        // Créer le WeatherManager
        GameObject weatherGO = new GameObject("WeatherManager_Test");
        weatherManager = weatherGO.AddComponent<WeatherManager>();

        // Test après 2 secondes
        Invoke("TestWeather", 2f);
    }

    void TestWeather()
    {
        Debug.Log("=== DÉBUT TEST MÉTÉO ===");
        Debug.Log("Test pour Paris, 3 jours");

        weatherManager.FetchForecast("Paris", 3,
            onSuccess: (forecasts) => {
                Debug.Log($"<color=green>✅ SUCCÈS ! {forecasts.Count} prévisions reçues</color>");
                foreach (var forecast in forecasts)
                {
                    Debug.Log($"  📅 {forecast.date:dd/MM/yyyy} : {forecast.weatherData.temperature}°C - {forecast.weatherData.description}");
                }
            },
            onError: (error) => {
                Debug.LogError($"<color=red>❌ ERREUR : {error}</color>");
            }
        );
    }
}
