using System.Net.Mime;
using HashidsNet;
using ITS.Core;
using ITS.Interfaces;
using ITS.Models;
using ITS.Web.ReportApi.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Document = QuestPDF.Fluent.Document;
using IContainer = QuestPDF.Infrastructure.IContainer;

namespace ITS.Web.ReportApi.Controllers;

[Route("tasks-api-reports")]
[Produces(MediaTypeNames.Application.Json)]
[ApiController]
public class TaskApiController : BaseSqlController
{
    private readonly ILogger<TaskApiController> logger;
    private readonly IWorkTaskRepository workTaskRepository;
    private readonly IWorkTaskCommentRepository workTaskCommentRepository;
    private readonly IUserRepository userRepository;
    private readonly AppOptions webOptions;
    
    public TaskApiController(ILogger<TaskApiController> logger,
        IWorkTaskRepository workTaskRepository,
        IWorkTaskCommentRepository workTaskCommentRepository,
        IUserRepository userRepository,
        IOptions<AppOptions> generalWebOption) 
    {
        this.logger = logger;
        this.workTaskRepository = workTaskRepository;
        this.workTaskCommentRepository = workTaskCommentRepository;
        this.userRepository = userRepository;
        webOptions = generalWebOption.Value;
    }
    
    [HttpGet]
    [Route("stats/{userId}")]
    [Produces(typeof(IEnumerable<WorkTask>))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ServiceFilter(typeof(ApiKeyAuthFilter))]
    public async Task<IActionResult> GetActiveTasksForUser(string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            logger.LogError("User is not specified, returning bad request at {DateCreated}", DateTime.Now);
            return BadRequest("Specify user identification in order to get items");
        }
        
        var hashids = new Hashids(webOptions.HashSalt);
        if (!hashids.TryDecodeSingle(userId, out var itsUserId))
        {
            logger.LogError("User ID was not in correct format");
            return BadRequest("User ID was not in correct format. Check if your data is the right or connection has been correctly set.");
        }

        try
        {
            var userWorkTasks = await workTaskRepository.WorkTasksForUserAsync(itsUserId);
            return Ok(userWorkTasks.Where(currentUser => !currentUser.IsCompleted));
        }
        catch (Exception e)
        {
            logger.LogError(e.Message);
            return BadRequest($"Data was not retrieved based on {userId}");
        }
    }

    [Route("pdf/{userId}")]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ServiceFilter(typeof(ApiKeyAuthFilter))]
    public async Task<IActionResult> DownloadPdfAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            logger.LogWarning("User is not specified, returning bad request at {DateCreated}", DateTime.Now);
            return BadRequest("Specify user identification in order to get PDF");
        }
        
        var hashids = new Hashids(webOptions.HashSalt);
        if (!hashids.TryDecodeSingle(userId, out var itsUserId))
        {
            logger.LogError("User ID was not in correct format");
            return BadRequest("User ID was not in correct format. Check if your data is the right or connection has been correctly set.");
        }
        
        logger.LogInformation("Download PDF for user {UserId} called at {DateLoaded}", itsUserId, DateTime.Now);
        var workTasks = await workTaskRepository.WorkTasksForUserAsync(itsUserId);
        var user = await userRepository.DetailsAsync(userId);
        logger.LogInformation("Received {NumberOfActiveTasks} tasks for user {UserId}", workTasks.Count,
            user.FullName);
        var generatePdf = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(20));

                    page.Header()
                        .Text($"Work tasks for user {user.FullName}!")
                        .SemiBold().FontSize(36).FontColor(Colors.Blue.Medium);

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });
                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Text("Description");
                                header.Cell().Element(CellStyle).AlignRight().Text("Started");
                                header.Cell().Element(CellStyle).AlignRight().Text("Ended");
                                header.Cell().Element(CellStyle).AlignRight().Text("Category");

                                static IContainer CellStyle(IContainer container) =>
                                    container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1)
                                        .BorderColor(Colors.Black);
                            });
                            foreach (var item in workTasks)
                            {
                                table.Cell().Element(CellStyle)
                                    .Text(item.Description)
                                    .WrapAnywhere();
                                table.Cell().Element(CellStyle)
                                    .AlignCenter()
                                    .Text(item.Start.ToShortDateString());
                                table.Cell().Element(CellStyle)
                                    .Text(item.End.ToShortDateString());
                                table.Cell().Element(CellStyle).AlignCenter()
                                    .Text(item.Category.Name);

                                static IContainer CellStyle(IContainer container) =>
                                    container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                                        .PaddingVertical(5);
                            }
                        });
                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Page ");
                            x.CurrentPageNumber();
                        });
                });
            })
            .GeneratePdf();
        return File(generatePdf, "application/pdf");
    }

    [HttpGet]
    [Route("app-health")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public override IActionResult CheckDbHealth()
    {
        var isAliveAsync = workTaskRepository.IsAlive();
        if (isAliveAsync)
        {
            logger.LogInformation("Application is alive and running at {DateCreated}", DateTime.Now);
            return Ok($"Database connection is alive at {DateTime.Now}");
        }

        logger.LogError("Database connection is invalid and cannot connect. Check connection string and logs.");
        return BadRequest($"Database cannot be reached at {DateTime.Now}");
    }
}