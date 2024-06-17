using System.Reflection;
using System.Text.Json;
using AutoUpdateConfig;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

try
{
    // 示例配置文件
    const string exampleConfig = "app.example.json";
    // 本程序的配置文件
    const string programConfig = "app.json";

    if (!File.Exists(exampleConfig))
    {
        Log.Error($"Not fonud \"{exampleConfig}\"");
        goto EXIT;
    }

    if (!File.Exists(programConfig))
    {
        File.Copy(exampleConfig, programConfig);
    }

    // 读取程序配置文件内容
    var strConfig = File.ReadAllText(programConfig);

    // 程序配置字典
    var ditcConfig = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(strConfig);
    if (ditcConfig == null)
    {
        Log.Error("Load config failed");
        goto EXIT;
    }

    // 订阅地址 用于下载最新的yaml
    var downloadLink = ditcConfig["downloadLink"].ToString();
    if (downloadLink.IsNullOrWhiteSpace())
    {
        Log.Error("Parameter \"downloadLink\" is empty");
        goto EXIT;
    }
    // 合并后最终的文件名
    var saveName = ditcConfig["saveName"].ToString();
    if (saveName == null)
    {
        Log.Error("Parameter \"saveName\" is empty");
        goto EXIT;
    }
    // 自定义的配置项
    var custom = ditcConfig["custom"].Deserialize<Dictionary<string, object>>();
    if (custom == null)
    {
        Log.Error("Parameter \"custom\" is empty");
        goto EXIT;
    }

    var strLastYaml = await Utils.GetContent(downloadLink);
    if (strLastYaml.IsNullOrWhiteSpace())
    {
        Log.Error("Empty data");
        goto EXIT;
    }

    // 创建一个 Deserializer 对象
    var deserializer = new DeserializerBuilder()
        .WithNamingConvention(HyphenatedNamingConvention.Instance)
        .Build();

    // 反序列化 YAML 为对象
    var dictLast = deserializer.Deserialize<Dictionary<string, object>>(strLastYaml);

    var keys = new List<string>();
    foreach (var kvp in custom)
    {
        var key = kvp.Key;
        var val = (JsonElement)kvp.Value;
        Merge(key, val, ref dictLast, ref keys);
    }

    // 将修改或新增的项移动到文件头部
    var newDict = new Dictionary<string, object>();
    foreach (var key in keys)
    {
        if (dictLast != null)
        {
            newDict.Add(key, dictLast[key]);
            // 移除新添加的项
            dictLast.Remove(key);
        }
    }

    InsertDictionary(newDict, dictLast);

    // 启用混合端口时 无需再分开设置
    if (custom.ContainsKey("mixed-port"))
    {
        newDict.Remove("port");
        newDict.Remove("socks-port");
    }

    // 创建序列化器实例
    var serializer = new SerializerBuilder().Build();

    // 将对象序列化为 YAML 字符串
    var strNewYaml = serializer.Serialize(newDict);

    var ver = Assembly.GetExecutingAssembly().GetName().Version;
    if (ver != null)
    {
        strNewYaml =
            $"# Auto Update Config v{ver.Major}.{ver.Minor}.{ver.Build}{Environment.NewLine}"
            + $"# Project url https://github.com/pdone/AutoUpdateConfig{Environment.NewLine}"
            + $"# Update time {DateTime.Now:F}{Environment.NewLine}"
            + strNewYaml;
    }

    File.WriteAllText(saveName, strNewYaml);

    Log.Info($"Results of the merger \"{saveName}\"");
}
catch (Exception ex)
{
    Log.Error($"{ex}");
    goto EXIT;
}

EXIT:
var sec = 3;
Log.Info($"Exit in {sec} seconds");
Task.Delay(sec * 1000).Wait();

static void Merge(string key, JsonElement jo, ref Dictionary<string, object> dictLast, ref List<string> kesy)
{
    switch (jo.ValueKind)
    {
        case JsonValueKind.Array:
            var list = new List<string>();
            for (var i = 0; i < jo.GetArrayLength(); i++)
            {
                list.Add(jo[i].GetString());
            }
            dictLast[key] = list;
            kesy.Add(key);
            break;
        case JsonValueKind.String:
            if (dictLast.TryAdd(key, jo.GetString()))
            {
                kesy.Add(key);
            }
            else
            {
                dictLast[key] = jo.GetString();
            }
            break;
        case JsonValueKind.Number:
            if (dictLast.TryAdd(key, jo.GetInt32()))
            {
                kesy.Add(key);
            }
            else
            {
                dictLast[key] = jo.GetInt32();
            }
            break;
        case JsonValueKind.True:
        case JsonValueKind.False:
            if (dictLast.TryAdd(key, jo.GetBoolean()))
            {
                kesy.Add(key);
            }
            else
            {
                dictLast[key] = jo.GetBoolean();
            }
            break;
        case JsonValueKind.Object:
        case JsonValueKind.Undefined:
        case JsonValueKind.Null:
        default:
            break;
    }
}

static void InsertDictionary(Dictionary<string, object> target, Dictionary<string, object> source, bool overwrite = true)
{
    foreach (var kvp in source)
    {
        if (overwrite || !target.ContainsKey(kvp.Key))
        {
            target[kvp.Key] = kvp.Value;
        }
    }
}
