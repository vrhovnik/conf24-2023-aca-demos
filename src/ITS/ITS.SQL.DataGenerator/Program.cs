using System.IO.Compression;
using ITS.Core;
using ITS.SQL.DataGenerator;
using Spectre.Console;

AnsiConsole.Write(new FigletText("Data generator for ITS demo app").Centered().Color(Color.Red));
var sqlConn = Environment.GetEnvironmentVariable("SQL_CONNECTION_STRING");
if (string.IsNullOrEmpty(sqlConn)) sqlConn = @"Data Source=(LocalDb)\MSSQLLocalDB";

AnsiConsole.Write(new Markup($"Current [bold yellow]SQL connection[/] to database [red]{sqlConn}[/]"));
AnsiConsole.WriteLine();

var folderRoot = Environment.GetEnvironmentVariable("FOLDER_ROOT");
if (string.IsNullOrEmpty(folderRoot))
{
    var pathToDownload = Environment.GetEnvironmentVariable("DOWNLOADURL") ??
                         "https://github.com/vrhovnik/conf24-2023-aca-demos/archive/refs/heads/main.zip";
    var httpClient = new HttpClient();
    var currentZipFile = await httpClient.GetByteArrayAsync(pathToDownload);
    File.WriteAllBytes("src.zip", currentZipFile);
    ZipFile.ExtractToDirectory("src.zip", "src", true);
    folderRoot = "src/conf24-2023-aca-demos-main";
}

AnsiConsole.WriteLine("You have defined the following path:");
var path = new TextPath(folderRoot)
{
    RootStyle = new Style(foreground: Color.Red),
    SeparatorStyle = new Style(foreground: Color.Green),
    StemStyle = new Style(foreground: Color.Blue),
    LeafStyle = new Style(foreground: Color.Yellow)
};
AnsiConsole.Write(path);

var dropIt = Environment.GetEnvironmentVariable("DROP_DATABASE") is not null &&
             Convert.ToBoolean(Environment.GetEnvironmentVariable("DROP_DATABASE"));
if (Environment.GetEnvironmentVariable("DROP_DATABASE") is null)
    dropIt = AnsiConsole.Confirm(
        "Do you wish to recreate database? Create backup before continuing. If there is an existing ITSDB, it will be dropped. Are you sure to continue with recreation?");

var sqlHelper = new SqlHelper(sqlConn);

if (dropIt)
{
    if (!await sqlHelper.DropAndRecreateDatabaseAsync())
    {
        AnsiConsole.WriteLine("We couldn't drop and recreate database, check logs and retry again.");
        return;
    }
}

dropIt = Environment.GetEnvironmentVariable("CREATE_TABLES") is not null &&
         Convert.ToBoolean(Environment.GetEnvironmentVariable("CREATE_TABLES"));
if (Environment.GetEnvironmentVariable("CREATE_TABLES") is null)
    dropIt = AnsiConsole.Confirm("Do you wish to create tables in database?");

if (dropIt)
{
    AnsiConsole.Write(new Markup("Recreating data table objects in database."));
    AnsiConsole.WriteLine();
    if (!await sqlHelper.CreateTablesInDatabaseAsync(Path.Join(folderRoot, "/scripts/SQL/")))
    {
        AnsiConsole.WriteLine("Not all objects were created in database ITSDB, check logs");
        return;
    }
}

var defaultPassword = Environment.GetEnvironmentVariable("DEFAULT_PASSWORD");
if (string.IsNullOrEmpty(defaultPassword))
    defaultPassword = AnsiConsole.Prompt(
        new TextPrompt<string>("Enter DEFAULT [green]password[/]?")
            .PromptStyle("red")
            .Secret());

var passwdHash = PasswordHash.CreateHash(defaultPassword);

var numberOfRecords = string.IsNullOrEmpty(Environment.GetEnvironmentVariable("RECORD_NUMBER"))
    ? AnsiConsole.Prompt(
        new TextPrompt<int>("Enter [green]record number[/] to generate")
            .PromptStyle("green"))
    : int.Parse(Environment.GetEnvironmentVariable("RECORD_NUMBER"));

AnsiConsole.Write(new Markup($"Defined [bold yellow]{numberOfRecords}[/] record to be generated."));
AnsiConsole.WriteLine();

await AnsiConsole.Status()
    .AutoRefresh(false)
    .Spinner(Spinner.Known.Dots6)
    .SpinnerStyle(Style.Parse("green bold"))
    .StartAsync("Generating data started ...", async ctx =>
    {
        //0. make sure DB is created and structure populated
        ctx.Status("Getting tags and inserting tags ...");
        ctx.Refresh();

        //1. Tags
        var tags = await GetDataFromFileAsync("data/tag.data");
        ctx.Status("Reading from file data/tag.data");
        ctx.Refresh();

        await sqlHelper.LoadTagsAsync(tags, ctx);

        //2. Categories
        var categories = await GetDataFromFileAsync("data/categories.data");
        ctx.Status($"Reading from file data/categories.data - found {categories.Length} categories.");
        ctx.Refresh();
        await sqlHelper.LoadCategoriesAsync(categories, ctx);

        //3. Users
        await sqlHelper.LoadUsersAsync(numberOfRecords, passwdHash, ctx);
        
        //4. user settings
        await sqlHelper.LoadUserSettingsAsync(ctx);
        
        //5. Work tasks
        await sqlHelper.LoadWorkTasksAsync(numberOfRecords, ctx);

        //6. Tags and comments with worktasks
        await sqlHelper.LoadTagsAndCommentForWorkTasksAsync(tags, ctx);
    });

AnsiConsole.Write(new Markup(
    $"SQL data objects were [bold red]created[/] and data was [bold red]inserted[/]  - check it via SQL tools created"));

if (AnsiConsole.Profile.Capabilities.Links)
    AnsiConsole.MarkupLine(
        "Check [link=https://https://github.com/vrhovnik/conf24-2023-aca-demos]for more information[/]");


async Task<string[]> GetDataFromFileAsync(string filename, char delimiter = ',')
{
    var filePath = Path.Join(folderRoot, filename);
    var currentFile = await File.ReadAllTextAsync(filePath);
    return currentFile.Split(delimiter);
}