using System.Diagnostics;
using System.Text.Json;
using CatBase.Models;
using Microsoft.Extensions.Options;

namespace CatBase.Services;

public class CatFactApiService : ICatFactApiService
{
    private readonly HttpClient _httpClient;
    private readonly string _factUrl;
    private readonly ILogger<CatFactApiService> _logger;

    public CatFactApiService(HttpClient httpClient, IOptions<CatFactApiOptions> options, ILogger<CatFactApiService> logger)
    {
        _httpClient = httpClient;
        _factUrl = options.Value.FactUrl;
        _logger = logger;
    }

    public async Task<CatFactApiResult> FetchFactAsync()
    {
        var stopwatch = Stopwatch.StartNew();
        HttpResponseMessage response;

        try
        {
            response = await _httpClient.GetAsync(_factUrl);
            stopwatch.Stop();
        }
        catch (HttpRequestException ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Nie można połączyć się z API: {Url}", _factUrl);
            throw;
        }

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("API zwróciło błąd {StatusCode} dla {Url}", (int)response.StatusCode, _factUrl);
            throw new HttpRequestException($"API error: {(int)response.StatusCode}", null, response.StatusCode);
        }

        var json = await response.Content.ReadAsStringAsync();
        var fact = JsonSerializer.Deserialize<CatFactResponse>(json);

        if (string.IsNullOrWhiteSpace(fact?.Fact))
        {
            _logger.LogError("Odpowiedź z {Url} nie zawiera pola 'fact' — prawdopodobnie błędny endpoint", _factUrl);
            throw new InvalidOperationException("Nieprawidłowa struktura odpowiedzi API.");
        }

        _logger.LogInformation("Pobrano fakt ({Length} znaków) w {Time}ms", fact.Fact.Length, stopwatch.ElapsedMilliseconds);

        return new CatFactApiResult(fact.Fact, fact.Length, stopwatch.ElapsedMilliseconds);
    }
}