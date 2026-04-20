namespace CatBase.Services;

public interface ICatFactFileService
{
    Task AppendFactAsync(string fact, int length);
    Task<CatFactFileStats> GetStatsAsync();
    void DeleteFile();
}

public record CatFactFileStats(
    double FileSizeKb,
    int CharCount
);