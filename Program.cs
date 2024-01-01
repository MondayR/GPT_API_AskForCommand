using System.Text;
using Newtonsoft.Json;


if (args.Length > 0)
{
    HttpClient client = new HttpClient();

    client.DefaultRequestHeaders.Add("authorization", "Bearer <将这段内容替换为你自己的API key>");

    var message1 = new messageBody
    {
        role = "system",
        content = "你是一个在windows命令行的使用上拥有非常丰富经验的程序员，每当我遇到需要使用但记不住的命令行指令时，你都能给我最简洁并且准确的答案,请将你觉得正确的答案返回在“【”和“】”中间以方便我获取，然后再外面告诉我这个语句的含义以及相关参数的用法",
    };
    var message2 = new messageBody
    {
        role = "user",
        content = args[0],
    };
    messageBody[] messages = { message1, message2 };

    var requestParams = new {
        model = "gpt-3.5-turbo",
        messages
    };

    var content = new StringContent(JsonConvert.SerializeObject(requestParams),
        Encoding.UTF8, "application/json");

    HttpResponseMessage response = await client.PostAsync("https://api.openai.com/v1/chat/completions", content);

    string responseString = await response.Content.ReadAsStringAsync();

    try
    {
        var dyData = JsonConvert.DeserializeObject<dynamic>(responseString);

        string guess = GuessCommand(dyData!.choices[0].message.content);
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"---> 我觉得你想使用的命令是: {guess}，命令已经自动复制完成，你可以直接粘贴使用");
        Console.ResetColor();

    }
    catch(Exception ex)
    {
        Console.WriteLine($"---> JSON字符串反序列化失败: {ex.Message}");
    }
}
else
{
    Console.WriteLine("---> 你需要输入一些内容才可以开始，请在两个英文引号内输入你需要提问的内容");
}

static string GuessCommand(string raw)
{
    Console.WriteLine("---> GPT-3 API 返回的内容是:");
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine(raw);

    int startIndex = raw.IndexOf("【");
    int endIndex = raw.IndexOf("】");
    string guess = raw;
    if (startIndex != -1 && endIndex != -1 && endIndex > startIndex)
    {
        guess = raw.Substring(startIndex + 1, endIndex - startIndex - 1);
    }
    Console.ResetColor();

    TextCopy.ClipboardService.SetText(guess);

    return guess;
}

class messageBody { 
    public string role { get; set; } = "";
    public string content { get; set; } = "";
}
