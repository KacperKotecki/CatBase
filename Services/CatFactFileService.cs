using CatBase.Models;
using Microsoft.Extensions.Options;

namespace CatBase.Services;

public class CatFactFileService : ICatFactFileService
{
    private readonly string _outputFilePath;
    private readonly ILogger<CatFactFileService> _logger;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public CatFactFileService(IOptions<CatFactApiOptions> options, IWebHostEnvironment env, ILogger<CatFactFileService> logger)
    {
        _outputFilePath = Path.Combine(env.ContentRootPath, options.Value.OutputFileName);
        _logger = logger;
        Directory.CreateDirectory(Path.GetDirectoryName(_outputFilePath)!);
    }

    public async Task AppendFactAsync(string fact, int length)
    {
        await _lock.WaitAsync();
        try
        {
            await using var writer = new StreamWriter(_outputFilePath, append: true);
            await writer.WriteLineAsync($"> {DateTime.UtcNow:dd/MM/yyyy HH:mm:ss} {fact} [Długość: {length}]");
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<CatFactFileStats> GetStatsAsync()
    {
        var fileInfo = new FileInfo(_outputFilePath);
        if (!fileInfo.Exists)
            return new CatFactFileStats(0, 0, 0);

        try
        {
            var content = await File.ReadAllTextAsync(_outputFilePath);
            var lineCount = content.Split('\n', StringSplitOptions.RemoveEmptyEntries).Length;
            return new CatFactFileStats(
                Math.Round(fileInfo.Length / 1024.0, 2),
                content.Length,
                lineCount
            );
        }
        catch (FileNotFoundException)
        {
            return new CatFactFileStats(0, 0, 0);
        }
    }

    public void DeleteFile()
    {
        if (File.Exists(_outputFilePath))
        {
            File.Delete(_outputFilePath);
            _logger.LogWarning("Plik {Path} został usunięty", _outputFilePath);
        }
    }
}