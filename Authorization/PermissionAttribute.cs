using Microsoft.AspNetCore.Authorization;

namespace CostQualityControl.API.Authorization;

public class RequirePermissionAttribute(string resource, string action) : AuthorizeAttribute(policy: $"{resource}:{action}")
{
}
