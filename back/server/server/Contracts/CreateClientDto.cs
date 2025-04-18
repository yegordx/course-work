using System.ComponentModel.DataAnnotations;

namespace server.Contracts;

public class CreateClientDto
{
    [Required]
    [EmailAddress(ErrorMessage = "Невірний формат email.")]
    public string Email { get; set; } = null!;

    [Required]
    [MinLength(6, ErrorMessage = "Пароль повинен містити щонайменше 6 символів.")]
    public string Password { get; set; } = null!;
}



