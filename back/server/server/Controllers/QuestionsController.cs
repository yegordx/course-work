using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using server.Contracts;
using server.Services;
using server.Models;

namespace server.Controllers;

[ApiController]
[Route("api/questions")]
public class QuestionsController : ControllerBase
{
    private readonly QuestionsService questionsService;

    public QuestionsController(QuestionsService questionsService)
    {
        this.questionsService = questionsService;
    }

    [Authorize]
    [HttpPost("upload")]
    public async Task<IActionResult> Upload([FromBody] List<QuestionDto> input)
    {
        var clientId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(clientId))
            return Unauthorized();

        try
        {
            await questionsService.UploadFaq(clientId, input);
            return Ok(new { message = "Питання збережено" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    //[Authorize]
    //[HttpGet]
    //public async Task<IActionResult> GetAll()
    //{
    //    var clientId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    //    if (string.IsNullOrEmpty(clientId))
    //        return Unauthorized();

    //    var questions = await questionsService.GetAll(clientId);

    //    var result = questions.Select(q => new QuestionDto
    //    {
    //        Id = q.Id,
    //        Question = q.Text,
    //        Answer = q.Answer
    //    });

    //    return Ok(result);
    //}

    //[Authorize]
    //[HttpDelete("{id}")]
    //public async Task<IActionResult> Delete(string id)
    //{
    //    var clientId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    //    if (string.IsNullOrEmpty(clientId))
    //        return Unauthorized();

    //    try
    //    {
    //        await questionsService.Delete(clientId, id);
    //        return Ok(new { message = "Питання видалено" });
    //    }
    //    catch (Exception ex)
    //    {
    //        return BadRequest(new { error = ex.Message });
    //    }
    //}
}
