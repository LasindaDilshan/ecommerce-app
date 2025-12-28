using Microsoft.EntityFrameworkCore;
using EcommerceAPI.Data;
using EcommerceAPI.DTOs;
using EcommerceAPI.Models;

namespace EcommerceAPI.Services;

public class SearchService : ISearchService
{
    private readonly ApplicationDbContext _context;

    public SearchService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SearchResult> SearchProductsAsync(SearchRequest request, int? userId = null, string? ipAddress = null)
    {
        var query = _context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.ProductImages)
            .Where(p => p.IsActive);

        // Apply search term with relevance scoring
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTermLower = request.SearchTerm.ToLower();
            query = query.Where(p =>
                EF.Functions.Like(p.Name.ToLower(), $"%{searchTermLower}%") ||
                EF.Functions.Like(p.Description.ToLower(), $"%{searchTermLower}%") ||
                EF.Functions.Like(p.SKU.ToLower(), $"%{searchTermLower}%") ||
                EF.Functions.Like(p.Category.Name.ToLower(), $"%{searchTermLower}%")
            );
        }

        // Apply filters
        if (request.CategoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == request.CategoryId.Value);
        }

        if (request.MinPrice.HasValue)
        {
            query = query.Where(p =>
                (p.DiscountPrice.HasValue ? p.DiscountPrice.Value : p.Price) >= request.MinPrice.Value);
        }

        if (request.MaxPrice.HasValue)
        {
            query = query.Where(p =>
                (p.DiscountPrice.HasValue ? p.DiscountPrice.Value : p.Price) <= request.MaxPrice.Value);
        }

        if (request.InStockOnly == true)
        {
            query = query.Where(p => p.StockQuantity > 0);
        }

        if (request.OnSaleOnly == true)
        {
            query = query.Where(p => p.DiscountPrice.HasValue && p.DiscountPrice < p.Price);
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply sorting
        query = request.SortBy?.ToLower() switch
        {
            "priceasc" => query.OrderBy(p => p.DiscountPrice ?? p.Price),
            "pricedesc" => query.OrderByDescending(p => p.DiscountPrice ?? p.Price),
            "name" => query.OrderBy(p => p.Name),
            "newest" => query.OrderByDescending(p => p.Id),
            "relevance" or _ => !string.IsNullOrWhiteSpace(request.SearchTerm)
                ? query.OrderByDescending(p =>
                    (EF.Functions.Like(p.Name.ToLower(), $"{request.SearchTerm.ToLower()}%") ? 100 : 0) +
                    (EF.Functions.Like(p.Name.ToLower(), $"%{request.SearchTerm.ToLower()}%") ? 50 : 0) +
                    (EF.Functions.Like(p.Description.ToLower(), $"%{request.SearchTerm.ToLower()}%") ? 25 : 0)
                )
                : query.OrderByDescending(p => p.IsFeatured).ThenBy(p => p.Name)
        };

        // Apply pagination
        var products = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new ProductSearchDto
            {
                Id = p.Id,
                Name = p.Name,
                NameHighlight = p.Name,
                Description = p.Description,
                DescriptionHighlight = p.Description,
                Price = p.Price,
                DiscountPrice = p.DiscountPrice,
                StockQuantity = p.StockQuantity,
                SKU = p.SKU,
                ImageUrl = p.ImageUrl,
                IsActive = p.IsActive,
                IsFeatured = p.IsFeatured,
                CategoryId = p.CategoryId,
                CategoryName = p.Category.Name,
                RelevanceScore = 0
            })
            .ToListAsync();

        // Fetch ratings in a single query to avoid N+1 (optimized)
        var productIds = products.Select(p => p.Id).ToList();
        var ratings = await _context.Reviews
            .AsNoTracking()
            .Where(r => productIds.Contains(r.ProductId))
            .GroupBy(r => r.ProductId)
            .Select(g => new { ProductId = g.Key, AvgRating = g.Average(r => (double)r.Rating), Count = g.Count() })
            .ToDictionaryAsync(x => x.ProductId, x => new { x.AvgRating, x.Count });

        foreach (var product in products)
        {
            if (ratings.TryGetValue(product.Id, out var ratingInfo))
            {
                product.Rating = ratingInfo.AvgRating;
                product.ReviewCount = ratingInfo.Count;
            }
        }

        // Apply highlighting and relevance scoring after query execution
        foreach (var product in products)
        {
            product.NameHighlight = HighlightText(product.Name, request.SearchTerm);
            product.DescriptionHighlight = HighlightText(TruncateDescription(product.Description, 150), request.SearchTerm);
            product.RelevanceScore = CalculateRelevanceScore(product.Name, product.Description, request.SearchTerm);
        }

        // Log search
        await LogSearchAsync(request, totalCount, userId, ipAddress, null);

        // Get facets (categories, price ranges)
        var facets = await GetFacetsAsync(request);

        // Get search suggestions
        var suggestions = await GetSimilarSearchTermsAsync(request.SearchTerm);

        return new SearchResult
        {
            Products = products,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            Facets = facets,
            SuggestedTerms = suggestions
        };
    }

    public async Task<AutocompleteResult> GetAutocompleteAsync(AutocompleteRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Query) || request.Query.Length < 2)
        {
            return new AutocompleteResult();
        }

        var queryLower = request.Query.ToLower();
        var suggestions = new List<AutocompleteSuggestion>();

        // Get product suggestions
        var products = await _context.Products
            .Where(p => p.IsActive && EF.Functions.Like(p.Name.ToLower(), $"{queryLower}%"))
            .OrderByDescending(p => p.IsFeatured)
            .Take(request.Limit / 2)
            .Select(p => new AutocompleteSuggestion
            {
                Text = p.Name,
                Type = "product",
                ProductId = p.Id,
                ImageUrl = p.ImageUrl,
                Price = p.DiscountPrice ?? p.Price
            })
            .ToListAsync();

        suggestions.AddRange(products);

        // Get category suggestions
        var categories = await _context.Categories
            .Where(c => EF.Functions.Like(c.Name.ToLower(), $"{queryLower}%"))
            .Take(request.Limit / 4)
            .Select(c => new AutocompleteSuggestion
            {
                Text = c.Name,
                Type = "category",
                CategoryId = c.Id
            })
            .ToListAsync();

        suggestions.AddRange(categories);

        // Get popular search terms
        var popularSearches = await _context.SearchLogs
            .Where(s => EF.Functions.Like(s.SearchTerm.ToLower(), $"{queryLower}%"))
            .GroupBy(s => s.SearchTerm)
            .Select(g => new
            {
                Term = g.Key,
                Count = g.Count()
            })
            .OrderByDescending(x => x.Count)
            .Take(request.Limit / 4)
            .ToListAsync();

        foreach (var search in popularSearches)
        {
            if (!suggestions.Any(s => s.Text.Equals(search.Term, StringComparison.OrdinalIgnoreCase)))
            {
                suggestions.Add(new AutocompleteSuggestion
                {
                    Text = search.Term,
                    Type = "search"
                });
            }
        }

        return new AutocompleteResult
        {
            Suggestions = suggestions.Take(request.Limit).ToList()
        };
    }

    public async Task<PopularSearchesResult> GetPopularSearchesAsync(int limit = 10)
    {
        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);

        var popularSearches = await _context.SearchLogs
            .Where(s => s.CreatedAt >= thirtyDaysAgo && !string.IsNullOrEmpty(s.SearchTerm))
            .GroupBy(s => s.SearchTerm.ToLower())
            .Select(g => new PopularSearch
            {
                SearchTerm = g.First().SearchTerm,
                SearchCount = g.Count(),
                ResultsCount = (int)g.Average(s => s.ResultsCount)
            })
            .OrderByDescending(s => s.SearchCount)
            .Take(limit)
            .ToListAsync();

        return new PopularSearchesResult
        {
            PopularSearches = popularSearches
        };
    }

    public async Task<SearchSuggestionsResult> GetSearchSuggestionsAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return new SearchSuggestionsResult();
        }

        var suggestions = await GetSimilarSearchTermsAsync(query, 5);

        return new SearchSuggestionsResult
        {
            Suggestions = suggestions
        };
    }

    public async Task LogSearchAsync(SearchRequest request, int resultCount, int? userId = null, string? ipAddress = null, string? userAgent = null)
    {
        if (string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            return;
        }

        var searchLog = new SearchLog
        {
            UserId = userId,
            SearchTerm = request.SearchTerm,
            ResultsCount = resultCount,
            Category = request.CategoryId?.ToString(),
            MinPrice = request.MinPrice,
            MaxPrice = request.MaxPrice,
            SortBy = request.SortBy,
            SortOrder = "asc",
            HasFilters = request.CategoryId.HasValue || request.MinPrice.HasValue || request.MaxPrice.HasValue,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            CreatedAt = DateTime.UtcNow
        };

        _context.SearchLogs.Add(searchLog);
        await _context.SaveChangesAsync();
    }

    public async Task<List<string>> GetRecentSearchesAsync(int userId, int limit = 10)
    {
        return await _context.SearchLogs
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => s.SearchTerm)
            .Distinct()
            .Take(limit)
            .ToListAsync();
    }

    // Helper methods
    private static string HighlightText(string text, string? searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm) || string.IsNullOrWhiteSpace(text))
        {
            return text;
        }

        var index = text.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase);
        if (index >= 0)
        {
            var highlighted = text.Insert(index + searchTerm.Length, "</mark>")
                                 .Insert(index, "<mark>");
            return highlighted;
        }

        return text;
    }

    private static string TruncateDescription(string description, int maxLength = 150)
    {
        if (string.IsNullOrEmpty(description) || description.Length <= maxLength)
        {
            return description;
        }

        return description.Substring(0, maxLength) + "...";
    }

    private static double CalculateRelevanceScore(string name, string description, string? searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return 0;
        }

        double score = 0;
        var searchLower = searchTerm.ToLower();
        var nameLower = name.ToLower();
        var descLower = description?.ToLower() ?? "";

        // Exact match in name
        if (nameLower.Equals(searchLower))
        {
            score += 100;
        }
        // Starts with in name
        else if (nameLower.StartsWith(searchLower))
        {
            score += 75;
        }
        // Contains in name
        else if (nameLower.Contains(searchLower))
        {
            score += 50;
        }

        // Contains in description
        if (descLower.Contains(searchLower))
        {
            score += 25;
        }

        return score;
    }

    private async Task<List<FacetDto>> GetFacetsAsync(SearchRequest request)
    {
        var facets = new List<FacetDto>();

        // Pre-fetch category counts in a single query to avoid N+1
        var categoryCounts = await _context.Products
            .AsNoTracking()
            .Where(p => p.IsActive)
            .GroupBy(p => p.CategoryId)
            .Select(g => new { CategoryId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.CategoryId, x => x.Count);

        // Category facets - now using pre-fetched counts
        var categories = await _context.Categories
            .AsNoTracking()
            .Where(c => categoryCounts.Keys.Contains(c.Id))
            .Select(c => new FacetValue
            {
                Value = c.Id.ToString(),
                Label = c.Name,
                Count = 0, // Will be set below
                IsSelected = request.CategoryId == c.Id
            })
            .ToListAsync();

        // Set counts from pre-fetched data
        foreach (var category in categories)
        {
            if (int.TryParse(category.Value, out int catId) && categoryCounts.TryGetValue(catId, out int count))
            {
                category.Count = count;
            }
        }

        if (categories.Any())
        {
            facets.Add(new FacetDto
            {
                Field = "category",
                Label = "Category",
                Values = categories
            });
        }

        // Price range facets - consolidated into single query
        var priceCounts = await _context.Products
            .AsNoTracking()
            .Where(p => p.IsActive)
            .GroupBy(p =>
                (p.DiscountPrice ?? p.Price) <= 25 ? "0-25" :
                (p.DiscountPrice ?? p.Price) <= 50 ? "25-50" :
                (p.DiscountPrice ?? p.Price) <= 100 ? "50-100" : "100+")
            .Select(g => new { Range = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Range, x => x.Count);

        var priceRanges = new List<FacetValue>
        {
            new() { Value = "0-25", Label = "$0 - $25", Count = priceCounts.GetValueOrDefault("0-25", 0) },
            new() { Value = "25-50", Label = "$25 - $50", Count = priceCounts.GetValueOrDefault("25-50", 0) },
            new() { Value = "50-100", Label = "$50 - $100", Count = priceCounts.GetValueOrDefault("50-100", 0) },
            new() { Value = "100+", Label = "$100+", Count = priceCounts.GetValueOrDefault("100+", 0) }
        };

        facets.Add(new FacetDto
        {
            Field = "price",
            Label = "Price Range",
            Values = priceRanges
        });

        return facets;
    }

    private async Task<List<string>> GetSimilarSearchTermsAsync(string searchTerm, int limit = 5)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return new List<string>();
        }

        // Get similar search terms based on Levenshtein distance
        var allSearchTerms = await _context.SearchLogs
            .Where(s => s.SearchTerm.Length >= searchTerm.Length - 2 &&
                       s.SearchTerm.Length <= searchTerm.Length + 2)
            .Select(s => s.SearchTerm)
            .Distinct()
            .Take(100)
            .ToListAsync();

        var similar = allSearchTerms
            .Where(term => !term.Equals(searchTerm, StringComparison.OrdinalIgnoreCase))
            .Select(term => new
            {
                Term = term,
                Distance = CalculateLevenshteinDistance(searchTerm.ToLower(), term.ToLower())
            })
            .Where(x => x.Distance <= 3)
            .OrderBy(x => x.Distance)
            .Take(limit)
            .Select(x => x.Term)
            .ToList();

        return similar;
    }

    private static int CalculateLevenshteinDistance(string source, string target)
    {
        if (string.IsNullOrEmpty(source))
        {
            return string.IsNullOrEmpty(target) ? 0 : target.Length;
        }

        if (string.IsNullOrEmpty(target))
        {
            return source.Length;
        }

        var sourceLength = source.Length;
        var targetLength = target.Length;
        var distance = new int[sourceLength + 1, targetLength + 1];

        for (var i = 0; i <= sourceLength; i++)
        {
            distance[i, 0] = i;
        }

        for (var j = 0; j <= targetLength; j++)
        {
            distance[0, j] = j;
        }

        for (var i = 1; i <= sourceLength; i++)
        {
            for (var j = 1; j <= targetLength; j++)
            {
                var cost = target[j - 1] == source[i - 1] ? 0 : 1;
                distance[i, j] = Math.Min(
                    Math.Min(distance[i - 1, j] + 1, distance[i, j - 1] + 1),
                    distance[i - 1, j - 1] + cost);
            }
        }

        return distance[sourceLength, targetLength];
    }
}
