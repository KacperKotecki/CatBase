using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using CatBase.Services;

namespace CatBase.Controllers;

public class CatFactsController : Controller
{
    private readonly ICatFactApiService _apiService;
    private readonly ICatFactFileService _fileService;

    public CatFactsController(ICatFactApiService apiService, ICatFactFileService fileService)
    {
        _apiService = apiService;
        _fileService = fileService;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    [EnableRateLimiting("get-fact")]
    public async Task<IActionResult> GetFact()
    {
        CatFactApiResult fact;
        try
        {
            fact = await _apiService.FetchFactAsync();
        }
        catch (HttpRequestException ex) when (ex.StatusCode is null)
        {
            return StatusCode(503, "Serwis zewnętrzny jest niedostępny. Spróbuj ponownie.");
        }
        catch (HttpRequestException ex)
        {
            return StatusCode((int)ex.StatusCode!, "Nie udało się pobrać faktu o kocie.");
        }
        catch (InvalidOperationException)
        {
            return BadRequest("Nie udało się pobrać faktu o kocie.");
        }

        await _fileService.AppendFactAsync(fact.Fact, fact.Length);
        var stats = await _fileService.GetStatsAsync();

        return Json(new
        {
            fact = fact.Fact,
            length = fact.Length,
            fileSizeKb = stats.FileSizeKb,
            timeToResponseMs = fact.TimeToResponseMs,
            charCount = stats.CharCount
        });
    }

    [HttpDelete]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteFile()
    {
        _fileService.DeleteFile();
        return Ok();
    }
}

