namespace ITS.Core;

public class AuthOptions : BaseSettings
{
    public AuthOptions() => SectionName = nameof(AuthOptions);
    public const string ApiKeyHeaderName = "X-Api-Key";
}