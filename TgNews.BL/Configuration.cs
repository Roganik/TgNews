using Microsoft.Extensions.Configuration;

namespace TgNews.BL;

public class TgNewsConfiguration
{
    private readonly IConfiguration _cfg;

    public string TgAppId => _cfg["telegram_app_id"];
    public string TgApiHash => _cfg["telegram_api_hash"];
    public string TgPhoneNumber => _cfg["telegram_phone_number"];
    public string TgBotToken => _cfg["telegram_bot_token"];
    public string TgBotForwardToChannel => _cfg["telegram_bot_forward_to_channel"];
    public string TgClientFloodAutoRetrySecondsThreshold => _cfg["telegram_client_auto_retry_flood_seconds_threshold"];
    
    public string DbFile => GetFilePath("tgNews.litedb");
    public string TgSessionFile => GetFilePath("tgNews.tg.session");
    public string TgBotSessionFile => GetFilePath("tgNews.tgBot.session");
    
    public TgNewsConfiguration(IConfiguration cfg)
    {
        _cfg = cfg;
    }
    
    private string FileDir => Path.GetDirectoryName(Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar))) 
                              ?? AppDomain.CurrentDomain.BaseDirectory;
    protected string GetFilePath(string filename) => Path.Combine(FileDir, filename);
    
    
    public string TgConfig(string what)
    {
        switch (what)
        {
            case "session_pathname": return TgSessionFile;
            case "api_id": return TgAppId;
            case "api_hash": return TgApiHash;
            case "phone_number": return TgPhoneNumber;
            default: return null;                  // let WTelegramClient decide the default config
        }
    }
    
    public string TgBotConfig(string what)
    {
        switch (what)
        {
            case "session_pathname": return TgBotSessionFile;
            case "bot_token": return TgBotToken;
            default: return TgConfig(what);                  // let WTelegramClient decide the default config
        }
    }
}