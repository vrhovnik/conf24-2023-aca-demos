using ITS.Interfaces;
using ITS.Models;
using ITS.Web.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITS.Web.Pages.Tasks;

[Authorize]
public class DeletePageModel : BasePageModel
{
    private readonly ILogger<DeletePageModel> logger;
    private readonly IWorkTaskRepository workTaskRepository;

    public DeletePageModel(ILogger<DeletePageModel> logger, IWorkTaskRepository workTaskRepository)
    {
        this.logger = logger;
        this.workTaskRepository = workTaskRepository;
    }

    public async Task OnGetAsync()
    {
        logger.LogInformation("Loading delete page at {DateLoaded} with {WorkTaskId}", DateTime.Now, WorkTaskId);
        WorkTask = await workTaskRepository.DetailsAsync(WorkTaskId);
        logger.LogInformation("Worktask loaded {Description}", WorkTask.Description);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        try
        {
            await workTaskRepository.DeleteAsync(WorkTaskId);
            Message = "Worktask was deleted";
        }
        catch (Exception e)
        {
            logger.LogError(e.Message);
            Message = "Worktask was not deleted, check logs.";
            return Page();
        }

        return RedirectToPage("/User/Dashboard");
    }

    [BindProperty(SupportsGet = true)] public string WorkTaskId { get; set; }
    [BindProperty] public WorkTask WorkTask { get; set; }
}