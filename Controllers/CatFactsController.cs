using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.RateLimiting;
using CatBase.Models;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;

namespace CatBase.Controllers;

public class CatFactsController : Controller
{
    private readonly HttpClient _httpClient;
    private readonly string _factUrl;
    private readonly string _outputFilePath;
    private readonly ILogger<CatFactsController> _logger;

    public CatFactsController(HttpClient httpClient, IOptions<CatFactApiOptions> options, ILogger<CatFactsController> logger, IWebHostEnvironment env)
    {
        _httpClient = httpClient;
        _factUrl = options.Value.FactUrl;
        _outputFilePath = Path.Combine(env.ContentRootPath, options.Value.OutputFileName);
        _logger = logger;
    }
    public IActionResult Index()
    {
        return View();
    }
    [HttpGet]
    [EnableRateLimiting("get-fact")]
    public async Task<IActionResult> GetFact()
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
            return StatusCode(503, "Serwis zewnętrzny jest niedostępny. Spróbuj ponownie.");
        }

        var time = stopwatch.ElapsedMilliseconds;

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("API zwróciło błąd {StatusCode} dla {Url}", (int)response.StatusCode, _factUrl);
            return StatusCode((int)response.StatusCode, "Nie udało się pobrać faktu o kocie.");
        }

        var json = await response.Content.ReadAsStringAsync();
        var fact = JsonSerializer.Deserialize<CatFactResponse>(json);

        if (string.IsNullOrWhiteSpace(fact?.Fact))
        {
            _logger.LogError("Odpowiedź z {Url} nie zawiera pola 'fact' — prawdopodobnie błędny endpoint", _factUrl);
            return BadRequest("Nie udało się pobrać faktu o kocie.");
        }

        _logger.LogInformation("Pobrano fakt ({Length} znaków) w {Time}ms", fact.Fact.Length, time);

        Directory.CreateDirectory(Path.GetDirectoryName(_outputFilePath)!);

        await using (var writer = new StreamWriter(_outputFilePath, append: true))
        {
            await writer.WriteLineAsync($"> {DateTime.UtcNow:dd/MM/yyyy HH:mm:ss} {fact.Fact} [Długość: {fact.Length}]");
        }

        var fileContent = await System.IO.File.ReadAllTextAsync(_outputFilePath);
        var fileInfo = new FileInfo(_outputFilePath);

        return Json(new
        {
            fact = fact.Fact,
            length = fact.Length,
            fileSizeKb = Math.Round(fileInfo.Length / 1024.0, 2),
            timeToResponseMs = time,
            charCount = fileContent.Length
        });
    }

    [HttpDelete]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteFile()
    {
        if (System.IO.File.Exists(_outputFilePath))
        {
            System.IO.File.Delete(_outputFilePath);
            _logger.LogWarning("Plik catfacts.txt został usunięty");
        }
        return Ok();
    }
}
