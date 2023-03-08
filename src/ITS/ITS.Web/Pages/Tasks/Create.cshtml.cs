using ITS.Interfaces;
using ITS.Models;
using ITS.Web.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITS.Web.Pages.Tasks;

[Authorize]
public class CreatePageModel : BasePageModel
{
    private readonly ILogger<CreatePageModel> logger;
    private readonly IWorkTaskRepository workTaskRepository;
    private readonly IUserDataContext userDataContext;
    private readonly ICategoryRepository categoryRepository;
    private readonly ITagRepository tagRepository;

    public CreatePageModel(ILogger<CreatePageModel> logger,
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
        logger.LogInformation("Loaded Create task page at {DateLoaded}", DateTime.Now);
        Categories = await categoryRepository.GetAllAsync();
        logger.LogInformation("Loaded {CategoriesCount}", Categories.Count);
        Tags = await tagRepository.GetAllAsync();
        logger.LogInformation("Loaded {TagsCount}", Tags.Count);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var form = await Request.ReadFormAsync();
        var tagsForm = form["ddlTags"];
        foreach (var currentTag in tagsForm)
        {
            NewTask.Tags.Add(new Tag { TagName = currentTag });
        }

        var categoryForm = form["ddlCategories"];
        var category = await categoryRepository.DetailsAsync(categoryForm[0]);
        NewTask.Category = category;

        var currentUser = userDataContext.GetCurrentUser();
        NewTask.User = new ItsUser { ItsUserId = currentUser.UserId, Email = currentUser.Email };
        NewTask.End = NewTask.Start;
        NewTask.IsCompleted = false;
        
        try
        {
            await workTaskRepository.InsertAsync(NewTask);
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

    [BindProperty] public List<Category> Categories { get; set; }
    [BindProperty] public List<Tag> Tags { get; set; }
    [BindProperty] public WorkTask NewTask { get; set; }
}