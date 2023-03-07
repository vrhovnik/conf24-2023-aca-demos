create table Users
(
    UserId   int identity
        constraint Users_pk
            primary key,
    FullName nvarchar(max) not null,
    Email    nvarchar(max) not null,
    Password nvarchar(max) not null
)