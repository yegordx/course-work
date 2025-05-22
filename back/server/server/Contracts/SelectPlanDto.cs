using System.ComponentModel.DataAnnotations;

namespace server.Contracts;

public class SelectPlanDto
{
    [Required]
    [Range(1, 12, ErrorMessage = "Тривалість має бути від 1 до 12 місяців.")]
    public int DurationMonths { get; set; }
}
