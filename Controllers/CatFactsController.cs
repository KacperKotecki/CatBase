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

    public CatFactsController(HttpClient httpClient, IOptions<CatFactApiOptions> options)
    {
        _httpClient = httpClient;
        _factUrl = options.Value.FactUrl;
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
        var json = await response.Content.ReadAsStringAsync();
        var fact = JsonSerializer.Deserialize<CatFactResponse>(json);


        var path = Path.Combine(Directory.GetCurrentDirectory(), "output", "catfacts.txt");
        if(!System.IO.File.Exists(path))
        {
            System.IO.Directory.CreateDirectory(Path.GetDirectoryName(path) ?? string.Empty);
            System.IO.File.Create(path).Dispose();  
        }

          await using (var writer = new StreamWriter(path, append: true))
            {
                await writer.WriteLineAsync($"> {DateTime.UtcNow} {fact?.Fact} [Długość: {fact?.Length}]");
            };
            var fileContent = await System.IO.File.ReadAllTextAsync(path);
            int totalCharCount = fileContent.Length;
            

            System.IO.FileInfo fileInfo = new System.IO.FileInfo(path);

        return Json(new
        {
            fact = fact?.Fact,
            length = fact?.Length,
            fileSizeKb = Math.Round(fileInfo.Length / 1024.0, 2),
            timeToResponseMs = time,
            charCount = totalCharCount
        });
    }

    [HttpDelete]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteFile()
    {
        var path = Path.Combine(Directory.GetCurrentDirectory(), "output", "catfacts.txt");
        if(System.IO.File.Exists(path))
        {
            System.IO.File.Delete(path);
        }
        return Ok();
    }
}
