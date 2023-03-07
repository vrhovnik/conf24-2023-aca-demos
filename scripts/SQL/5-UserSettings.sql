create table UserSetting
(
    UserSettingId     int identity
        constraint UserSetting_pk
            primary key,
    UserId            int
        constraint UserSetting_Users_UserId_fk
            references Users,
    EmailNotification bit default 0
)