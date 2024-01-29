using Chatapp.Shared;

using Microsoft.AspNetCore.Mvc;

namespace FileAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ImageController : ControllerBase
{
  private readonly ChatDbContext _chatDb;
  private readonly ILogger _logger;
  private readonly IConfiguration _configuration;

  public ImageController(ChatDbContext chatDb, ILogger<ImageController> logger, IConfiguration configuration)
  {
    _chatDb = chatDb;
    _logger = logger;
    _configuration = configuration;
  }

  [HttpPost]
  public async Task<ActionResult> SaveFileToDrive()
  {
    _logger.LogInformation("Adding message to database");
    await Task.Delay(1000);
    return Ok();
  }
}
