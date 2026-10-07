namespace CostQualityControl.API.DTOs.Auth;
public record AuthResponse(string Token, string Username, string FullName, string Role, DateTime Expires);
