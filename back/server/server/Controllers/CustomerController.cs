using Microsoft.AspNetCore.Mvc;
using server.Services;
using server.Models;
using server.Contracts;
using MongoDB.Driver;
using System.Security.Cryptography;

namespace server.Controllers;

[ApiController]
[Route("api/customers")]
public class CustomerController : ControllerBase
{
    private readonly CustomerService customerService;

    public CustomerController(CustomerService customerService)
    {
        this.customerService = customerService;
    }

    [HttpPost("ask")]
    public async Task<IActionResult> AskQuestion([FromBody] AskQuestionRequest input)
    {
        if (string.IsNullOrWhiteSpace(input.ApiKey))
            return BadRequest(new { error = "API ключ є обов'язковим." });

        try
        {
            var answer = await customerService.ProcessCustomerRequest(input);
            return Ok(new { answer });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}