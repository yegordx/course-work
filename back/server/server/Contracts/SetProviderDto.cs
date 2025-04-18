using System.ComponentModel.DataAnnotations;

namespace server.Contracts;

public class SetProviderDto
{
    [Required(ErrorMessage = "Ім'я провайдера обов’язкове")]
    [RegularExpression("^(OpenAI|Cohere|HuggingFace)$", ErrorMessage = "Провайдер має бути OpenAI, Cohere або HuggingFace")]
    public string ProviderName { get; set; } = null!;

    [Required(ErrorMessage = "API ключ обов’язковий")]
    [MinLength(10, ErrorMessage = "API ключ має бути не менше 10 символів")]
    public string ProvidedKey { get; set; } = null!;
}
