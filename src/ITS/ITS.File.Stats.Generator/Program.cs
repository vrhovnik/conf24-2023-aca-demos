using System.Diagnostics;
using ITS.Models;
using ITS.SQL;
using Newtonsoft.Json;
using Spectre.Console;

AnsiConsole.Write(new Rule("[red]Work Stats generator for ITS app[/]"));
var sqlConn = Environment.GetEnvironmentVariable("SQL_CONNECTION_STRING");
if (string.IsNullOrEmpty(sqlConn)) sqlConn = @"Data Source=(LocalDb)\MSSQLLocalDB;Initial Catalog=ITSDB;";

AnsiConsole.Write(new Markup($"Current [bold yellow]SQL connection[/] to database [red]{sqlConn}[/]"));
AnsiConsole.WriteLine();

var filePath = Environment.GetEnvironmentVariable("FILEPATH");
if (string.IsNullOrEmpty(filePath))
    filePath = AnsiConsole.Prompt(
        new TextPrompt<string>("Enter filepath (with name) [green]password[/]?")
            .PromptStyle("red"));

AnsiConsole.WriteLine("You have defined the following filename to store the files:");
var path = new TextPath(filePath)
{
    RootStyle = new Style(foreground: Color.Red),
    SeparatorStyle = new Style(foreground: Color.Green),
    StemStyle = new Style(foreground: Color.Blue),
    LeafStyle = new Style(foreground: Color.Yellow)
};
AnsiConsole.Write(path);

var workTasksRepo = new WorkTaskRepository(sqlConn);
var workTasks = await workTasksRepo.GetAsync();

await AnsiConsole.Status()
    .AutoRefresh(false)
    .Spinner(Spinner.Known.Dots6)
    .SpinnerStyle(Style.Parse("green bold"))
    .StartAsync("Generating data started ...", async ctx =>
    {
        ctx.Status("Getting work tasks ...");
        ctx.Refresh();

        var taskByDate = workTasks.GroupBy(task => task.Start.ToShortDateString());
        var list = new List<WorkTaskStats>();
        var currentIterationId = 1;
        
        foreach (var currentValue in taskByDate)
        {
            ctx.Status($"Reading tasks from date {currentValue.Key} and calculating stats...");
            ctx.Refresh();
            var wts = new WorkTaskStats
            {
                DateCreated = DateTime.Parse(currentValue.Key),
                WorkTaskStatId = currentIterationId.ToString()
            };

            var workTasks = currentValue.ToList();
            var mostActiveTask = workTasks.MaxBy(task => task.Comments.Count);
            if (mostActiveTask != null)
            {
                wts.MostActiveTask = mostActiveTask;
                ctx.Status($"Getting most active task {mostActiveTask.Description}");
                ctx.Refresh();
            }

            wts.NumberOfComments = workTasks.Sum(task => task.Comments.Count);
            wts.PublicTasks = workTasks.Count(task => task.IsPublic);
            wts.DailyTasks = workTasks.Count;
            wts.ClosedTasks = workTasks.Count(task => task.Start <= task.End);
            ctx.Status("Setting stats on and continuing...");
            ctx.Refresh();
            currentIterationId++;
            list.Add(wts);
        }
        ctx.Status($"Stats done, saving data to file {filePath}");
        ctx.Refresh();
        var dataToBeStored = JsonConvert.SerializeObject(list);
        await File.WriteAllTextAsync(filePath,dataToBeStored);
    });

AnsiConsole.Write(new Markup($"Data has been saved to [red]{filePath}[/]."));
AnsiConsole.WriteLine();

//open folder to see the result
Process.Start(new ProcessStartInfo("notepad",filePath));

if (AnsiConsole.Profile.Capabilities.Links)
    AnsiConsole.MarkupLine(
        "Check [link=https://https://github.com/vrhovnik/conf24-2023-aca-demos]for more information[/]");