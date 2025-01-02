using System.Linq.Expressions;

namespace CommerceBack.Common;

public class PaginatedResponse<T>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    
    public int TotalCount { get; set; } = 0;

    public int TotalFiltered { get; set; }
        
    public int TotalPages => (int)Math.Ceiling((double)TotalFiltered / PageSize);
    public IEnumerable<T> Entities { get; set; } = [];

    public PaginatedResponse<T> Map<U>(PaginatedResponse<U> response, Func<U, T> mapFunction)
    {
        return new PaginatedResponse<T>()
        {
            PageNumber = response.PageNumber,
            PageSize = response.PageSize,
            TotalCount = response.TotalCount,
            TotalFiltered = response.TotalFiltered,
            Entities = response.Entities.Select(mapFunction)
        };
    }
}