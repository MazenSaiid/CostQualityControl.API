using CostQualityControl.API.DTOs.Dashboard;
namespace CostQualityControl.API.Services;
public interface IDashboardService { Task<DashboardStatsDto> GetStatsAsync(); }
