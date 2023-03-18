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
    private readonly IWorkStatsRepository workStatsRepository;
    private readonly AuthOptions authOptions;
    
    public TaskApiController(ILogger<TaskApiController> logger,
        IWorkTaskRepository workTaskRepository,
        IWorkTaskCommentRepository workTaskCommentRepository,
        IUserRepository userRepository,
        IWorkStatsRepository workStatsRepository,
        IOptions<AuthOptions> authOptionValue) 
    {
        this.logger = logger;
        this.workTaskRepository = workTaskRepository;
        this.workTaskCommentRepository = workTaskCommentRepository;
        this.userRepository = userRepository;
        this.workStatsRepository = workStatsRepository;
        authOptions = authOptionValue.Value;
    }
    
    [HttpGet]
    [Route("stats/{userId}")]
    [Produces(typeof(UserStats))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ServiceFilter(typeof(ApiKeyAuthFilter))]
    public async Task<IActionResult> GetStatsForUserAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            logger.LogError("User is not specified, returning bad request at {DateCreated}", DateTime.Now);
            return BadRequest("Specify user identification in order to get items");
        }
        
        var hashids = new Hashids(authOptions.HashSalt);
        if (!hashids.TryDecodeSingle(userId, out var itsUserId))
        {
            logger.LogError("User ID was not in correct format");
            return BadRequest("User ID was not in correct format. Check if your data is the right or connection has been correctly set.");
        }

        userId = itsUserId.ToString();
        
        var itsUser = await userRepository.DetailsAsync(userId);
        var userStats = new UserStats
        {
            User = itsUser
        };

        var userWorkTasks = await workTaskRepository.WorkTasksForUserAsync(userId);
        
        userStats.ActiveTasks = userWorkTasks.Count(workTask => !workTask.IsCompleted);
        userStats.PublicTasks = userWorkTasks.Count(workTask => workTask.IsPublic);
        userStats.ClosedTasks = userWorkTasks.Count(workTask => workTask.End > workTask.Start);
        var commentsForUser = await workTaskCommentRepository.GetCommentsForUserAsync(userId);
        userStats.NumberOfComments = commentsForUser.Count;
        var mostActiveTask = await workTaskRepository.MostActiveTaskAsync(userId);
        userStats.MostActiveTask = mostActiveTask;
        
        return Ok(userStats);
    }

    [Route("pdf/{userId}")]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ServiceFilter(typeof(ApiKeyAuthFilter))]
    public async Task<IActionResult> DownloadPdfAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            logger.LogWarning("User is not specified, returning bad request at {DateCreated}", DateTime.Now);
            return BadRequest("Specify user identification in order to get PDF");
        }
        
        var hashids = new Hashids(authOptions.HashSalt);
        if (!hashids.TryDecodeSingle(userId, out var itsUserId))
        {
            logger.LogError("User ID was not in correct format");
            return BadRequest("User ID was not in correct format. Check if your data is the right or connection has been correctly set.");
        }

        userId = itsUserId.ToString();
        
        logger.LogInformation("Download PDF for user {UserId} called at {DateLoaded}", itsUserId, DateTime.Now);
        var workTasks = await workTaskRepository.WorkTasksForUserAsync(userId);
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

    [Route("public-stats/most-active")]
    [HttpGet]
    [Produces(typeof(WorkTask))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetMostActiveTaskDetailsAsync()
    {
        var workTaskStatsList = await workStatsRepository.GetAllAsync();

        if (workTaskStatsList == null)
        {
            logger.LogError("There is no data");
            return BadRequest("No data");
        }

        var task = workTaskStatsList.MaxBy(stats => stats.DateCreated);
        var detailedTask = await workTaskRepository.DetailsAsync(task.MostActiveTask.WorkTaskId);
        return Ok(detailedTask);
    }

    [Route("public-stats/pdf")]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DownloadPublicPdfAsync()
    {
        var workTaskStatsList = await workStatsRepository.GetAllAsync();

        if (workTaskStatsList == null)
        {
            logger.LogError("There is no data");
            return BadRequest("No data");
        }

        var generatePdf = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(20));

                    page.Header()
                        .Text($"Public task stats!")
                        .SemiBold().FontSize(36).FontColor(Colors.Blue.Medium);

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(120);
                                columns.RelativeColumn(120);
                                columns.ConstantColumn(120);
                            });
                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Text("Public tasks");
                                header.Cell().Element(CellStyle).AlignRight().Text("Closed tasks");
                                header.Cell().Element(CellStyle).AlignRight().Text("Comments");
                                static IContainer CellStyle(IContainer container) =>
                                    container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1)
                                        .BorderColor(Colors.Black);
                            });
                            foreach (var item in workTaskStatsList)
                            {
                                table.Cell().Element(CellStyle)
                                    .Text(item.PublicTasks.ToString())
                                    .WrapAnywhere();
                                table.Cell().Element(CellStyle)
                                    .AlignCenter()
                                    .Text(item.ClosedTasks.ToString());
                                table.Cell().Element(CellStyle)
                                    .Text(item.NumberOfComments.ToString());
                                
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
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ServiceFilter(typeof(ApiKeyAuthFilter))]
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