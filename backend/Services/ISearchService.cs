using EcommerceAPI.DTOs;

namespace EcommerceAPI.Services;

public interface ISearchService
{
    Task<SearchResult> SearchProductsAsync(SearchRequest request, int? userId = null, string? ipAddress = null);
    Task<AutocompleteResult> GetAutocompleteAsync(AutocompleteRequest request);
    Task<PopularSearchesResult> GetPopularSearchesAsync(int limit = 10);
    Task<SearchSuggestionsResult> GetSearchSuggestionsAsync(string query);
    Task LogSearchAsync(SearchRequest request, int resultCount, int? userId = null, string? ipAddress = null, string? userAgent = null);
    Task<List<string>> GetRecentSearchesAsync(int userId, int limit = 10);
}
