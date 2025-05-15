using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using server.Contracts;
using server.Services;
using server.Services.Authorization;

namespace server.Controllers;

[ApiController]
[Route("api/clients")]
public class ClientsController : ControllerBase
{
    private readonly ClientsService clientsService;
    private readonly JwtOptions jwtOptions;

    public ClientsController(ClientsService clientsService, IOptions<JwtOptions> jwtOptions)
    {
        this.clientsService = clientsService;
        this.jwtOptions = jwtOptions.Value;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] CreateClientDto request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var token = await clientsService.Register(request.Email, request.Password);

            Response.Cookies.Append("access_token", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddHours(jwtOptions.ExpiersHours)
            });

            return Ok();
        }
        catch (Exception ex) {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login( [FromBody] CreateClientDto request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var token = await clientsService.Login(request.Email, request.Password);

            Response.Cookies.Append("access_token", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddHours(jwtOptions.ExpiersHours)
            });

            return Ok(token);
        }
        catch (Exception ex) 
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize]
    [HttpPost("plan")]
    public async Task<IActionResult> SelectPlan([FromBody] SelectPlanDto request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var clientId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(clientId)) return Unauthorized();

        try
        {
            await clientsService.SelectPlan(clientId, request.DurationMonths);
            return Ok(new { message = "Тариф встановлено" });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [Authorize]
    [HttpPost("embedding-provider")]
    public async Task<IActionResult> SetEmbeddingProvider([FromBody] SetProviderDto request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var clientId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(clientId)) return Unauthorized();

        try
        {
            await clientsService.SetEmbeddingProvider(clientId, request.ProviderName, request.ProvidedKey);
            return Ok(new { message = "Провайдера встановлено" });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }


    [Authorize]
    [HttpGet("key")]
    public async Task<IActionResult> GetApiKey()
    {
        var clientId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(clientId))
            return Unauthorized();

        try
        {
            var apiKey = await clientsService.GetApiKey(clientId);
            return Ok(new { apiKey });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [Authorize]
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var clientId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(clientId))
            return Unauthorized();

        try
        {
            var profile = await clientsService.GetProfile(clientId);
            return Ok(profile);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("access_token", new CookieOptions
        {
            Path = "/",
            Secure = true,
            SameSite = SameSiteMode.None
        });

        return NoContent();
    }
}
