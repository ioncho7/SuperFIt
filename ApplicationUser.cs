using Microsoft.AspNetCore.Identity;

namespace SuperFit.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string DisplayName { get; set; } = string.Empty;
    }
}
