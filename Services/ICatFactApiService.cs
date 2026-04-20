namespace CatBase.Services;

public interface ICatFactApiService
{
    Task<CatFactApiResult> FetchFactAsync();
}

public record CatFactApiResult(
    string Fact,
    int Length,
    long TimeToResponseMs
);