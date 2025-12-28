using EcommerceAPI.DTOs;

namespace EcommerceAPI.Services;

public interface IDashboardService
{
    Task<DashboardStatsDto> GetDashboardStatsAsync();
}
