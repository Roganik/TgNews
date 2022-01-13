namespace TgNews.Worker;

public class WorkerConfiguration
{
    private readonly IConfiguration _cfg;
    public int ForwarderCooldownSeconds => IntOrDefault("app_job_forwarder_cooldown_seconds", 300);

    public WorkerConfiguration(IConfiguration cfg)
    {
        _cfg = cfg;
    }
    
    private int IntOrDefault(string cfgKey, int defaultValue)
    {
        var cfgValue = _cfg[cfgKey];
        if (int.TryParse(cfgValue, out var cfgInt))
        {
            return cfgInt;
        }

        return defaultValue;
    }
}