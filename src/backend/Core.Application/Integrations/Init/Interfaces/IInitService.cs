namespace Core.Application;

public interface IInitService
{
    Task<CustomApiResponse<object>> InitAreasAsync(string contentRootPath);
}
