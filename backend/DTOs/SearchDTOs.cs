namespace EcommerceAPI.DTOs;

public class SearchRequest
{
    public string SearchTerm { get; set; } = string.Empty;
    public int? CategoryId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public double? MinRating { get; set; }
    public bool? InStockOnly { get; set; } = false;
    public bool? OnSaleOnly { get; set; } = false;
    public List<string>? Brands { get; set; }
    public string? SortBy { get; set; } = "Relevance"; // Relevance, Name, PriceAsc, PriceDesc, Rating, Newest
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class SearchResult
{
    public List<ProductSearchDto> Products { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPrevious => PageNumber > 1;
    public bool HasNext => PageNumber < TotalPages;
    public List<FacetDto> Facets { get; set; } = new();
    public string? DidYouMean { get; set; }
    public List<string> SuggestedTerms { get; set; } = new();
}

public class ProductSearchDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string NameHighlight { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string DescriptionHighlight { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public int StockQuantity { get; set; }
    public string SKU { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; }
    public bool IsFeatured { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public double? Rating { get; set; }
    public int ReviewCount { get; set; }
    public double RelevanceScore { get; set; }
    public bool OnSale => DiscountPrice.HasValue && DiscountPrice < Price;
}

public class FacetDto
{
    public string Field { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public List<FacetValue> Values { get; set; } = new();
}

public class FacetValue
{
    public string Value { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public int Count { get; set; }
    public bool IsSelected { get; set; } = false;
}

public class AutocompleteRequest
{
    public string Query { get; set; } = string.Empty;
    public int Limit { get; set; } = 10;
}

public class AutocompleteResult
{
    public List<AutocompleteSuggestion> Suggestions { get; set; } = new();
}

public class AutocompleteSuggestion
{
    public string Text { get; set; } = string.Empty;
    public string Type { get; set; } = "product"; // product, category, brand
    public string? ImageUrl { get; set; }
    public int? ProductId { get; set; }
    public int? CategoryId { get; set; }
    public decimal? Price { get; set; }
}

public class PopularSearchesResult
{
    public List<PopularSearch> PopularSearches { get; set; } = new();
}

public class PopularSearch
{
    public string SearchTerm { get; set; } = string.Empty;
    public int SearchCount { get; set; }
    public int ResultsCount { get; set; }
}

public class SearchSuggestionsRequest
{
    public string Query { get; set; } = string.Empty;
}

public class SearchSuggestionsResult
{
    public List<string> Suggestions { get; set; } = new();
    public string? DidYouMean { get; set; }
}
