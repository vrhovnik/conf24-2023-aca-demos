using ITS.Interfaces;
using ITS.Models;
using ITS.Web.Base;
using Microsoft.AspNetCore.Mvc;

namespace ITS.Web.Pages.Tasks;

public class DetailsPageModel : BasePageModel
{
    private readonly ILogger<DetailsPageModel> logger;
    private readonly IWorkTaskRepository workTaskRepository;
    private readonly IWorkTaskCommentRepository workTaskCommentRepository;
    private readonly IUserDataContext userDataContext;

    public DetailsPageModel(ILogger<DetailsPageModel> logger, IWorkTaskRepository workTaskRepository,
        IWorkTaskCommentRepository workTaskCommentRepository, IUserDataContext userDataContext)
    {
        this.logger = logger;
        this.workTaskRepository = workTaskRepository;
        this.workTaskCommentRepository = workTaskCommentRepository;
        this.userDataContext = userDataContext;
    }

    public async Task OnGetAsync()
    {
        logger.LogInformation("Loading delete page at {DateLoaded} with {WorkTaskId}", DateTime.Now, WorkTaskId);
        WorkTask = await workTaskRepository.DetailsAsync(WorkTaskId);
        logger.LogInformation("Worktask loaded {Description} with {NumberOfComments}", WorkTask.Description,
            WorkTask.Comments.Count);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        try
        {
            WorkTask = await workTaskRepository.DetailsAsync(WorkTaskId);
            NewComment.AssignedTask = WorkTask;
            
            var user = userDataContext.GetCurrentUser();
            logger.LogInformation("Adding user {FullName} to comment for worktask {WorkTaskId}", user.Fullname,
                WorkTaskId);
            WorkTask.User = new ItsUser { ItsUserId = user.UserId };
            
            await workTaskCommentRepository.InsertAsync(NewComment);
            
            logger.LogInformation("Comment was added for worktask {WorkTaskId}", WorkTaskId);
            Message = $"Worktask comment was added at {DateTime.Now}";
        }
        catch (Exception e)
        {
            logger.LogError(e.Message);
            Message = "Worktask comment was not added, check logs.";
            return Page();
        }

        return RedirectToPage("/Tasks/Details", new { WorkTaskId });
    }

    [BindProperty(SupportsGet = true)] public string WorkTaskId { get; set; }
    [BindProperty] public WorkTask WorkTask { get; set; }
    [BindProperty] public WorkTaskComment NewComment { get; set; }
}