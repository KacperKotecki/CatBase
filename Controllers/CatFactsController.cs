using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using CatBase.Models;
using System.Text.Json;

namespace CatBase.Controllers;

public class CatFactsController : Controller
{
    private readonly HttpClient _httpClient;
    private readonly string _factUrl;
    private readonly ILogger<CatFactsController> _logger;

    public CatFactsController(HttpClient httpClient, IOptions<CatFactApiOptions> options, ILogger<CatFactsController> logger)
    {
        _httpClient = httpClient;
        _factUrl = options.Value.FactUrl;
        _logger = logger;
    }
    public IActionResult Index()
    {
        return View();
    }

    public async Task<IActionResult> GetFact()
    {
        var stopwatch = Stopwatch.StartNew();
        var response = await _httpClient.GetAsync(_factUrl);
        stopwatch.Stop();
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

        var path = Path.Combine(Directory.GetCurrentDirectory(), "output", "catfacts.txt");

        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        await using (var writer = new StreamWriter(path, append: true))
        {
            await writer.WriteLineAsync($"> {DateTime.UtcNow:dd/MM/yyyy HH:mm:ss} {fact.Fact} [Długość: {fact.Length}]");
        }

        var fileContent = await System.IO.File.ReadAllTextAsync(path);
        var fileInfo = new FileInfo(path);

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
        var path = Path.Combine(Directory.GetCurrentDirectory(), "output", "catfacts.txt");
        if (System.IO.File.Exists(path))
        {
            System.IO.File.Delete(path);
            _logger.LogWarning("Plik catfacts.txt został usunięty");
        }
        return Ok();
    }
}
