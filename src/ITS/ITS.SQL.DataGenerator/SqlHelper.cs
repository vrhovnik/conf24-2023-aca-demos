using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using Bogus;
using Dapper;
using ITS.Models;
using Spectre.Console;

namespace ITS.SQL.DataGenerator;

public class SqlHelper
{
    private readonly SqlConnection connection;

    public SqlHelper(string connectionString) => 
        connection = new SqlConnection(connectionString);

    public async Task LoadTagsAsync(string[] tags, StatusContext ctx)
    {
        var dtTags = new DataTable();
        dtTags.TableName = "Tags";
        dtTags.Columns.Add("TagName", typeof(string));

        if (connection.State == ConnectionState.Closed) connection.Open();

        var tagsInDatabase = await connection.QueryAsync("SELECT TagName FROM Tags");

        foreach (var currentTag in tags)
        {
            ctx.Status($"Traversing through - current tag {currentTag} and preparing sql");
            var row = dtTags.NewRow();
            row["TagName"] = currentTag;
            var tag1 = currentTag;
            if (tagsInDatabase.FirstOrDefault(tag => tag.TagName == tag1) == null)
                dtTags.Rows.Add(row);
            ctx.Refresh();
        }

        if (await WriteBulkToDatabaseAsync(dtTags))
            ctx.Status("Inserted data to tags, continuting to categories");
        else
            ctx.Status("Check error log, tags were not inserted!");

        ctx.Refresh();
    }

    public async Task LoadCategoriesAsync(string[] categories, StatusContext ctx)
    {
        var dtCategories = new DataTable();
        dtCategories.TableName = "Categories";
        dtCategories.Columns.Add("CategoryId", typeof(int));
        dtCategories.Columns.Add("Name", typeof(string));

        if (connection.State == ConnectionState.Closed) connection.Open();
        
        var categoriesInDatabase =
            await connection.QueryAsync<Category>("SELECT CategoryId,Name FROM Categories");

        foreach (var currentCategoryName in categories)
        {
            ctx.Status($"Traversing through - current category name {currentCategoryName} and preparing sql");
            var row = dtCategories.NewRow();
            row["Name"] = currentCategoryName;
            var name = currentCategoryName;
            if (categoriesInDatabase.FirstOrDefault(cat => cat.Name == name) == null)
                dtCategories.Rows.Add(row);
            ctx.Refresh();
        }

        await WriteBulkToDatabaseAsync(dtCategories);
    }

    public async Task LoadUsersAsync(int numberOfRecords, string passwdHash, StatusContext ctx)
    {
        var users = new Faker<ItsUser>()
            .RuleFor(currentUser => currentUser.FullName,
                (faker, _) => faker.Name.FirstName() + " " + faker.Name.LastName()
            ).RuleFor(u => u.Email, (f, u) => f.Internet.Email(u.FullName))
            .RuleFor(u => u.Password, (_, _) => passwdHash)
            .GenerateLazy(numberOfRecords);

        ctx.Status($"Generated {numberOfRecords} users, adding to database");
        ctx.Refresh();

        var dtUsers = new DataTable();
        dtUsers.TableName = "Users";
        dtUsers.Columns.Add("UserId", typeof(int));
        dtUsers.Columns.Add("FullName", typeof(string));
        dtUsers.Columns.Add("Email", typeof(string));
        dtUsers.Columns.Add("Password", typeof(string));

        foreach (var currentUser in users)
        {
            ctx.Status($"Traversing through - current user {currentUser.FullName} and preparing sql");
            var row = dtUsers.NewRow();
            row["FullName"] = currentUser.FullName;
            row["Email"] = currentUser.Email;
            row["Password"] = passwdHash;
            dtUsers.Rows.Add(row);
            ctx.Refresh();
        }

        await WriteBulkToDatabaseAsync(dtUsers);
    }

    public async Task LoadTagsAndCommentForWorkTasksAsync(string[] tags, StatusContext ctx)
    {
        var dtWorkTasksTags = new DataTable();
        dtWorkTasksTags.TableName = "WorkTask2Tags";
        var dtWorkTaskIdColumn = new DataColumn();
        dtWorkTaskIdColumn.ColumnName = "WorkTaskId";
        dtWorkTaskIdColumn.DataType = typeof(int);
        var dtTagNameColumn = new DataColumn();
        dtTagNameColumn.ColumnName = "TagName";
        dtTagNameColumn.DataType = typeof(string);
        dtWorkTasksTags.Columns.Add(dtWorkTaskIdColumn);
        dtWorkTasksTags.Columns.Add(dtTagNameColumn);
        dtWorkTasksTags.PrimaryKey = new[] { dtWorkTaskIdColumn, dtTagNameColumn };

        var dtWorkTasksComments = new DataTable();
        dtWorkTasksComments.TableName = "WorkTaskComments";
        dtWorkTasksComments.Columns.Add("WorkTaskCommentId", typeof(int));
        dtWorkTasksComments.Columns.Add("UserId", typeof(int));
        dtWorkTasksComments.Columns.Add("WorkTaskId", typeof(int));
        dtWorkTasksComments.Columns.Add("Comment", typeof(string));
        dtWorkTasksComments.Columns.Add("StartDate", typeof(DateTime));

        if (connection.State == ConnectionState.Closed) connection.Open();
        
        var worktTasksInDatabase =
            await connection.QueryAsync<WorkTask>("SELECT WorkTaskId FROM WorkTasks");
        ctx.Status($"Received {worktTasksInDatabase.Count()} work tasks.");
        ctx.Refresh();
        var usersInDatabase = await connection.QueryAsync<ItsUser>("SELECT UserId as ItsUserId FROM Users");
        foreach (var workTask in worktTasksInDatabase)
        {
            ctx.Status($"Adding tags to each of the tasks");
            ctx.Refresh();

            for (var currentCounter = 0; currentCounter < new Random().Next(2, 6); currentCounter++)
            {
                var row = dtWorkTasksTags.NewRow();
                row["WorkTaskId"] = workTask.WorkTaskId;
                var currentTag = new Faker().PickRandom(tags.ToArray());
                var uniqueTagForTask = GetUniqueTagForTask(dtWorkTasksTags.Rows,
                    workTask.WorkTaskId, currentTag, 0, tags.Length, tags);
                row["TagName"] = uniqueTagForTask;
                if (!string.IsNullOrEmpty(uniqueTagForTask))
                    dtWorkTasksTags.Rows.Add(row);

                ctx.Status($"Adding tag {currentTag} to task {workTask.WorkTaskId}");
                ctx.Refresh();

                var currentRow = dtWorkTasksComments.NewRow();
                currentRow["WorkTaskId"] = workTask.WorkTaskId;
                currentRow["StartDate"] = new Faker().Date.Recent(new Random().Next(3, 24));
                currentRow["Comment"] = new Faker().Lorem.Paragraph(new Random().Next(2, 3));
                currentRow["UserId"] = new Faker().PickRandom(usersInDatabase).ItsUserId;
                dtWorkTasksComments.Rows.Add(currentRow);
                ctx.Status($"Adding comment to task {workTask.WorkTaskId}");
                ctx.Refresh();
            }
        }

        await WriteBulkToDatabaseAsync(dtWorkTasksTags);
        ctx.Status("Work tasks with tags added, adding random comments as well.");
        await WriteBulkToDatabaseAsync(dtWorkTasksComments);

        string GetUniqueTagForTask(DataRowCollection rowCollection, string workTaskId, string tagName,
            int count, int maxCount, string[] currentTags)
        {
            if (count >= maxCount) return string.Empty;

            if (rowCollection.Find(new[] { workTaskId, tagName }) == null) return tagName;

            tagName = new Faker().PickRandom(currentTags);
            return GetUniqueTagForTask(rowCollection, workTaskId, tagName, ++count, maxCount, currentTags);
        }
    }

    public async Task LoadWorkTasksAsync(int numberOfRecords, StatusContext ctx)
    {
        if (connection.State == ConnectionState.Closed) connection.Open();
        var categoriesInDatabase = await connection.QueryAsync<Category>("SELECT CategoryId,Name FROM Categories");
        ctx.Status(
            $"Added users and settings for that user. Continuing to insert work tasks. Received {categoriesInDatabase.Count()} categories");
        ctx.Refresh();

        var usersInDatabase = await connection.QueryAsync<ItsUser>("SELECT UserId as ItsUserId FROM Users");
        ctx.Status($"Received {usersInDatabase.Count()} users");
        ctx.Refresh();

        var workTasks = new Faker<WorkTask>()
            .RuleFor(workTask => workTask.Description,
                (faker, _) => faker.Lorem.Paragraph(new Random().Next(3, 10)))
            .RuleFor(u => u.Start, (f, _) => f.Date.Recent(new Random().Next(3, 40)))
            .RuleFor(u => u.End, (f, _) => f.Date.Future(new Random().Next(3, 20)))
            .RuleFor(u => u.IsPublic, (f, _) => f.Random.Bool())
            .RuleFor(u => u.IsCompleted, (f, _) => f.Random.Bool())
            .RuleFor(u => u.Category, (f, _) => f.PickRandom(categoriesInDatabase))
            .RuleFor(u => u.User, (f, _) => f.PickRandom(usersInDatabase))
            .GenerateLazy(numberOfRecords);

        var dtWorkTasks = new DataTable();
        dtWorkTasks.TableName = "WorkTasks";
        dtWorkTasks.Columns.Add("WorkTaskId", typeof(int));
        dtWorkTasks.Columns.Add("Description", typeof(string));
        dtWorkTasks.Columns.Add("CategoryId", typeof(int));
        dtWorkTasks.Columns.Add("StartDate", typeof(DateTime));
        dtWorkTasks.Columns.Add("EndDate", typeof(DateTime));
        dtWorkTasks.Columns.Add("UserId", typeof(int));
        dtWorkTasks.Columns.Add("IsPublic", typeof(bool));
        dtWorkTasks.Columns.Add("IsCompleted", typeof(bool));

        var taskCounter = 0;
        foreach (var workTask in workTasks)
        {
            ctx.Status($"Traversing through - current work task #{taskCounter++} and preparing sql");
            var row = dtWorkTasks.NewRow();
            row["Description"] = workTask.Description;
            row["StartDate"] = workTask.Start.ToShortDateString();
            row["EndDate"] = workTask.End.ToShortDateString();
            row["IsPublic"] = workTask.IsPublic;
            row["CategoryId"] = workTask.Category.CategoryId;
            row["UserId"] = workTask.User.ItsUserId;
            row["IsCompleted"] = workTask.IsCompleted;
            dtWorkTasks.Rows.Add(row);
            ctx.Refresh();
        }

        await WriteBulkToDatabaseAsync(dtWorkTasks);
    }

    public async Task LoadUserSettingsAsync(StatusContext ctx)
    {
        if (connection.State == ConnectionState.Closed) connection.Open();
        var usersInDatabase =
            await connection.QueryAsync<ItsUser>("SELECT UserId as ItsUserId FROM Users");
        ctx.Status($"and received {usersInDatabase.Count()} users.");
        ctx.Refresh();

        var dtUserSettings = new DataTable();
        dtUserSettings.TableName = "UserSetting";
        dtUserSettings.Columns.Add("UserSettingId", typeof(int));
        dtUserSettings.Columns.Add("UserId", typeof(int));
        dtUserSettings.Columns.Add("EmailNotification", typeof(bool));

        foreach (var user in usersInDatabase)
        {
            ctx.Status($"Adding settings for user {user.FullName}");

            var row = dtUserSettings.NewRow();
            row["UserId"] = user.ItsUserId;
            row["EmailNotification"] = new Faker().Random.Bool();
            dtUserSettings.Rows.Add(row);
            ctx.Refresh();
        }

        await WriteBulkToDatabaseAsync(dtUserSettings);
    }

    private async Task<bool> WriteBulkToDatabaseAsync(DataTable dt)
    {
        if (connection.State == ConnectionState.Closed) connection.Open();

        try
        {
            using var tagBulkInsert = new SqlBulkCopy(connection);
            tagBulkInsert.DestinationTableName = dt.TableName;
            await tagBulkInsert.WriteToServerAsync(dt);
        }
        catch (Exception tagException)
        {
            AnsiConsole.WriteException(tagException);
            return false;
        }

        return true;
    }

    public async Task<bool> CreateTablesInDatabaseAsync(string folderPath)
    {
        if (connection.State == ConnectionState.Closed) connection.Open();
        long countInMs = 0;
        await AnsiConsole.Status()
            .AutoRefresh(false)
            .Spinner(Spinner.Known.Bounce)
            .SpinnerStyle(Style.Parse("green bold"))
            .StartAsync("Creating database objects...", async ctx =>
            {
                var stopWatch = new Stopwatch();

                try
                {
                    foreach (var file in Directory.GetFiles(folderPath, "*.sql", SearchOption.TopDirectoryOnly))
                    {
                        stopWatch.Start();

                        var fileContent = await File.ReadAllTextAsync(file)
                            .ConfigureAwait(false);

                        ctx.Status($"Reading file {file} and executing script.");
                        ctx.Refresh();

                        await connection.ExecuteAsync(fileContent)
                            .ConfigureAwait(false);

                        stopWatch.Stop();
                        countInMs += stopWatch.ElapsedMilliseconds;
                        ctx.Status($"Script from file {file} was executed in {stopWatch.ElapsedMilliseconds} ms.");
                        ctx.Refresh();
                    }
                }
                catch (Exception e)
                {
                    AnsiConsole.WriteException(e);
                    return false;
                }

                return true;
            });

        AnsiConsole.Write(new Markup($"SQL data objects has been [bold red]created in {countInMs}[/]"));
        AnsiConsole.WriteLine();

        return true;
    }


    public async Task<bool> DropAndRecreateDatabaseAsync()
    {
        if (connection.State == ConnectionState.Closed) connection.Open();
        var dbExistsCount =
            await connection.QuerySingleOrDefaultAsync<int>(
                "SELECT count(*) FROM master.dbo.sysdatabases WHERE name = 'ITSDB'");
        if (dbExistsCount > 0)
        {
            AnsiConsole.Write(
                new Markup("Database [red]ITSDB[/] will be dropped."));
            AnsiConsole.WriteLine();
            try
            {
                await connection.ExecuteAsync("DROP DATABASE ITSDB");
            }
            catch (Exception dropException)
            {
                AnsiConsole.WriteException(dropException);
                return false;
            }
        }

        try
        {
            await connection.ExecuteAsync(
                "CREATE DATABASE ITSDB collate SQL_Latin1_General_CP1_CI_AS");
            AnsiConsole.Write(
                new Markup(
                    "Database [red]ITSDB[/] has been created, connection string will be modified."));
        }
        catch (Exception dropException)
        {
            AnsiConsole.WriteException(dropException);
            return false;
        }

        if (connection.State == ConnectionState.Open)
            connection.Close();

        connection.ConnectionString += ";Initial Catalog=ITSDB";

        AnsiConsole.Write(
            new Markup($"New connection string is [red]{connection.ConnectionString}[/]."));
        
        AnsiConsole.WriteLine();
        return true;
    }
}