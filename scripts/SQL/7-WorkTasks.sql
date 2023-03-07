create table WorkTasks
(
    WorkTaskId  int identity
        constraint WorkTasks_pk
            primary key,
    Description nvarchar(max) not null,
    CategoryId  int
        constraint WorkTasks_Category_CategoryId_fk
            references Categories,
    StartDate   datetime,
    EndDate     datetime,
    UserId      int
        constraint WorkTasks_Users_UserId_fk
            references Users,
    IsPublic    bit,
    IsCompleted    bit
)