create table WorkTask2Tags
(
    WorkTaskId int           not null,
    TagName    nvarchar(200) not null,
    constraint WorkTask2Tags_pk
        primary key (WorkTaskId, TagName)
)