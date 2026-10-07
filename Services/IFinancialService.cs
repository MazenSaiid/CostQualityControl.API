using CostQualityControl.API.DTOs.Financials;

namespace CostQualityControl.API.Services;

public interface IFinancialService
{
    Task<FinancialSummaryDto> GetSummaryAsync();
}
