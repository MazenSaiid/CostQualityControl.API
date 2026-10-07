namespace CostQualityControl.API.DTOs.Auth;

public record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword,
    string ConfirmPassword
);
