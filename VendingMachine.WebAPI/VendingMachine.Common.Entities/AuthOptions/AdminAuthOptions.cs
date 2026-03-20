using System.ComponentModel.DataAnnotations;

namespace VendingMachine.WebAPI.Options
{
    public class AdminAuthOptions
    {
        public const string SectionName = "AuthOptions";

        [Required]
        public string Issuer { get; set; } = string.Empty;

        [Required]
        public string Audience { get; set; } = string.Empty;

        [Required]
        [MinLength(32)]
        public string SecretKey { get; set; } = string.Empty;

        [Range(1, 1440)]
        public int AccessTokenLifetimeMinutes { get; set; } = 60;

        [Required]
        public string UserName { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = string.Empty;
    }
}
