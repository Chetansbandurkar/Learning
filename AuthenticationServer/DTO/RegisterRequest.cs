using System.ComponentModel.DataAnnotations;

namespace AuthenticationServer.DTO
{
    public class RegisterRequest
    {
        [Required]
        [MaxLength(100)]
        public string FirstName { get; init; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string LastName { get; init; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public string Email { get; init; } = string.Empty;

        [Required]
        [MinLength(8)]
        public string Password { get; init; } = string.Empty;
    }
}
