using System.ComponentModel.DataAnnotations;

namespace SignalProcessing.Api.Models;

/// <summary>
/// Pagination parameters for list endpoints.
/// </summary>
public record PaginationParams
{
    /// <summary>
    /// Page number (1-based).
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Page must be at least 1")]
    public int Page { get; init; } = 1;

    /// <summary>
    /// Number of items per page.
    /// </summary>
    [Range(1, 100, ErrorMessage = "PageSize must be between 1 and 100")]
    public int PageSize { get; init; } = 10;
}
