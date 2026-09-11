using System.Net.Http.Json;
using Microsoft.Data.Sqlite;

namespace VoiceAssistant.Core.Services;

public class WeatherService
{
    private readonly HttpClient _httpClient;
    private readonly string _connectionString;

    public WeatherService(string dbPath = "assistant.db")
    {
        _httpClient = new HttpClient();
        _connectionString = $"Data Source={dbPath}";
    }

    public async Task<string?> GetCurrentWeatherAsync()
    {
        var coords = GetMartaCoordinates();
        
        if (coords == null)
        {
            return "No tengo configurada la ubicacion.";
        }
        
        var (lat, lon, nombre) = coords.Value;
        
        try
        {
            var url = $"https://api.open-meteo.com/v1/forecast?latitude={lat}&longitude={lon}&current=temperature_2m,relative_humidity_2m,apparent_temperature,weather_code,wind_speed_10m&timezone=auto";

            var response = await _httpClient.GetFromJsonAsync<OpenMeteoResponse>(url);

            if (response?.Current == null) return null;

            var temp = response.Current.Temperature_2m;
            var feelsLike = response.Current.Apparent_temperature;
            var humidity = response.Current.Relative_humidity_2m;
            var wind = response.Current.Wind_speed_10m;
            var weatherCode = response.Current.Weather_code;

            var descripcion = TraducirWeatherCode(weatherCode);

            return $"En {nombre}: {descripcion}, {temp} grados (sensacion termica {feelsLike} grados), humedad {humidity}%, viento {wind} km/h.";
        }
        catch
        {
            return null;
        }
    }

    private (double lat, double lon, string nombre)? GetMartaCoordinates()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        
        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT u.Latitud, u.Longitud, u.Nombre
            FROM People p
            JOIN Ubicaciones u ON p.UbicacionId = u.Id
            WHERE p.Name LIKE '%Marta%' AND u.Activo = 1
            LIMIT 1";
        
        using var reader = command.ExecuteReader();
        
        if (reader.Read())
        {
            return (
                reader.GetDouble(0),
                reader.GetDouble(1),
                reader.GetString(2)
            );
        }
        
        return null;
    }

    private string TraducirWeatherCode(int code)
    {
        return code switch
        {
            0 => "cielo despejado",
            1 or 2 or 3 => "parcialmente nublado",
            45 or 48 => "niebla",
            51 or 53 or 55 => "llovizna",
            61 or 63 or 65 => "lluvia",
            71 or 73 or 75 => "nieve",
            80 or 81 or 82 => "chaparrones",
            95 or 96 or 99 => "tormenta",
            _ => "clima variable"
        };
    }

    private class OpenMeteoResponse
    {
        public CurrentWeather? Current { get; set; }
    }

    private class CurrentWeather
    {
        public double Temperature_2m { get; set; }
        public double Apparent_temperature { get; set; }
        public int Relative_humidity_2m { get; set; }
        public double Wind_speed_10m { get; set; }
        public int Weather_code { get; set; }
    }
}