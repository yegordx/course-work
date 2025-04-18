using System.ComponentModel.DataAnnotations;

namespace server.Contracts;

public class FaqUploadDto
{
    public List<FaqItemDto> Items { get; set; } = new();
}

public class FaqItemDto
{
    [Required(ErrorMessage = "Питання обов'язкове")]
    [MinLength(5, ErrorMessage = "Питання повинно містити щонайменше 5 символів")]
    public string Question { get; set; } = null!;

    [Required(ErrorMessage = "Відповідь обов'язкова")]
    [MinLength(3, ErrorMessage = "Відповідь повинна містити щонайменше 3 символи")]
    public string Answer { get; set; } = null!;
}