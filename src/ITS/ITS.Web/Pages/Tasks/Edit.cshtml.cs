using ITS.Interfaces;
using ITS.Models;
using ITS.Web.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITS.Web.Pages.Tasks;

[Authorize]
public class EditPageModel : BasePageModel
{
    private readonly ILogger<CreatePageModel> logger;
    private readonly IWorkTaskRepository workTaskRepository;
    private readonly IUserDataContext userDataContext;
    private readonly ICategoryRepository categoryRepository;
    private readonly ITagRepository tagRepository;
    
    public EditPageModel(ILogger<CreatePageModel> logger,
        IWorkTaskRepository workTaskRepository,
        IUserDataContext userDataContext,
        ICategoryRepository categoryRepository,
        ITagRepository tagRepository)
    {
        this.logger = logger;
        this.workTaskRepository = workTaskRepository;
        this.userDataContext = userDataContext;
        this.categoryRepository = categoryRepository;
        this.tagRepository = tagRepository;
    }
    
    public async Task OnGetAsync()
    {
        logger.LogInformation("Loaded Update task page at {DateLoaded} with {WorkTaskId}", DateTime.Now, WorkTaskId);
        Categories = await categoryRepository.GetAllAsync();
        logger.LogInformation("Loaded {CategoriesCount}", Categories.Count);
        Tags = await tagRepository.GetAllAsync();
        logger.LogInformation("Loaded {TagsCount}", Tags.Count);
        EditTask = await workTaskRepository.DetailsAsync(WorkTaskId);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var form = await Request.ReadFormAsync();
        var tagsForm = form["ddlTags"];
        foreach (var currentTag in tagsForm)
        {
            EditTask.Tags.Add(new Tag { TagName = currentTag });
        }

        var categoryForm = form["ddlCategories"];
        var category = await categoryRepository.DetailsAsync(categoryForm[0]);
        EditTask.Category = category;

        var currentUser = userDataContext.GetCurrentUser();
        EditTask.User = new ItsUser { ItsUserId = currentUser.UserId, Email = currentUser.Email };
        
        try
        {
            EditTask.WorkTaskId = WorkTaskId;
            await workTaskRepository.UpdateAsync(EditTask);
            Message = "Worktask was updated";
        }
        catch (Exception e)
        {
            logger.LogError(e.Message);
            Message = "Error with inserting new task, check logs.";
            Categories = await categoryRepository.GetAllAsync();
            Tags = await tagRepository.GetAllAsync();
            return Page();
        }

        return RedirectToPage("/User/Dashboard");
    }
    
    [BindProperty(SupportsGet = true)] public string WorkTaskId { get; set; }
    [BindProperty] public WorkTask EditTask { get; set; }
    [BindProperty] public List<Category> Categories { get; set; }
    [BindProperty] public List<Tag> Tags { get; set; }
}