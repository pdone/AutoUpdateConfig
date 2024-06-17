namespace AutoUpdateConfig;
internal static class Utils
{
    public static async Task<string> GetContent(string targetUrl)
    {
        Log.Info($"Get response from {targetUrl}");
        using var client = new HttpClient();
        var response = await client.GetAsync(targetUrl);

        if (response.IsSuccessStatusCode)
        {
            using var content = response.Content;
            var res = await content.ReadAsStringAsync();
            Log.Info("Get response complete");
            return res;
        }
        else
        {
            Log.Error($"Failed: {response.StatusCode}");
            return string.Empty;
        }
    }

    public static bool IsNullOrWhiteSpace(this string? value)
    {
        return string.IsNullOrWhiteSpace(value);
    }
}

internal static class Log
{
    public static void Info(string str)
    {
        Msg("INFO", str);
    }

    public static void Warn(string str)
    {
        Msg("WARN", str);
    }

    public static void Error(string str)
    {
        Msg("ERROR", str);
    }

    static void Msg(string type, string str)
    {
        Console.WriteLine($"{DateTime.Now:s} [{type}] {str}");
    }
}
