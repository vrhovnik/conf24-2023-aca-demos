create table WorkTaskComments
(
    WorkTaskCommentId         int identity
        constraint WorkTaskComments_pk
            primary key nonclustered,
    UserId                    int      not null
        constraint WorkTaskComments_Users_UserId_fk
            references Users,
    WorkTaskId                int      not null
        constraint WorkTaskComments_WorkTasks_WorkTaskId_fk
            references WorkTasks,
    Comment                   nvarchar(max),
    StartDate                 datetime not null
)