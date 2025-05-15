namespace server.Contracts;

public class QuestionDto
{
    public string? Id { get; set; }
    public string Question { get; set; } = null!;
    public string Answer { get; set; } = null!;
}
