namespace Core.Application;

public interface IInitService
{
    Task<CustomApiResponse<object>> InitAreasAsync(string contentRootPath);

    Task<CustomApiResponse<object>> InitProjectAsync();
}
