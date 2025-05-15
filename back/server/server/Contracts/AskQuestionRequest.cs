namespace server.Contracts;

public class AskQuestionRequest
{
    public string ApiKey { get; set; } = null!;
    public string Request { get; set; } = null!;
}

