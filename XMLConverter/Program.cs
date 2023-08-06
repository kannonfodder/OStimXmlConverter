// See https://aka.ms/new-console-template for more information

using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml;
using XMLConverter;

Console.WriteLine("Hello, World!");
var inputPath = "";
var outputPath = "";
if (args.Length < 1)
{
    Console.WriteLine("Enter input filepath");
    inputPath = Console.ReadLine()!;
}
else
{
    inputPath = args[0];
}

outputPath = inputPath;
if (args.Length < 2)
{
    Console.Write("Enter output filepath");
    var line = Console.ReadLine();
    if (!string.IsNullOrEmpty(line))
    {
        outputPath = line;
    }
}

var errors = new List<string>();
//Grab XMLS
var files = Directory.GetFiles(inputPath, "*.xml", new EnumerationOptions() { RecurseSubdirectories = true });
var scenes = new Dictionary<string, Scene>();
foreach (var file in files)
{
    Console.WriteLine($"Loading {file}");

    XmlDocument doc = new XmlDocument();
    try
    {
        doc.Load(file);
    }
    catch (Exception ex)
    {
        errors.Add($"{file} : {ex.Message}");
    }

    if (doc["scene"] != null)
    {
        var scene = new Scene();
        var sceneTag = doc["scene"];
        scene.Name = sceneTag!["info"]!.Attributes["name"]?.Value ?? doc["scene"]?.Attributes["id"]?.Value!;
        //TODO: ModPack - Animator name?
        scene.Length = Convert.ToSingle(sceneTag["anim"]?.Attributes["l"]?.Value);
        //TODO: Handle Destination/Origin
        if (sceneTag["nav"] != null)
        {
            for (var i = 0; i < sceneTag["nav"]?.ChildNodes.Count; i++)
            {
                var tab = sceneTag["nav"]!.ChildNodes.Item(i);

                if (tab is not { Name: "tab" }) continue;

                for (var j = 0; j < tab.ChildNodes.Count; j++)
                {
                    var page = tab.ChildNodes.Item(j);
                    if (page is not { Name: "page" }) continue;
                    for (var k = 0; k < page.ChildNodes.Count; k++)
                    {
                        var option = page.ChildNodes.Item(k);
                        if (option is not { Name: "option" }) continue;
                        scene.Navigations.Add(new Navigation()
                        {
                            Destination = option!.Attributes!["go"]?.Value,
                            //TODO: Convert navigation texts
                            Description = option!.Attributes["text"].Value
                        });
                    }
                }
            }
        }

        if (sceneTag["speed"] != null)
        {
            for (var i = 0; i < sceneTag["speed"]!.ChildNodes.Count; i++)
            {
                var sp = sceneTag["speed"]!.ChildNodes.Item(i);
                if (sp == null) continue;
                var anim = sp["anim"];
                if(anim == null) continue;
                var speed = new Speed()
                {
                    Animation = anim!.Attributes?["id"]?.Value,
                    DisplaySpeed = Convert.ToSingle(sp.Attributes["qnt"].Value),
                    PlaybackSpeed = 1.0f
                };
                scene.Speeds.Add(speed);
            }
        }

        if (sceneTag["metadata"] != null)
        {
            var metadata = sceneTag["metadata"];
            if (metadata?.Attributes["tags"] != null)
            {
                scene.Tags.AddRange(metadata!.Attributes["tags"].Value.Split(','));    
            }

            if (metadata.Attributes["noRandomSelection"] != null)
            {
                scene.NoRandomSelection = metadata.Attributes["noRandomSelection"].Value == "1";
            }

            if (metadata.Attributes["furniture"] != null)
            {
                scene.Furniture = metadata.Attributes["furniture"].Value;
            }
        }


        scenes.Add(file, scene);
    }
    else
    {
        Console.WriteLine($"{file} is not a scene xml");
    }
}


//Write object to Json


foreach (var (k, v) in scenes)
{
    var path = Path.Combine(outputPath, $"{Path.GetFileNameWithoutExtension(k)}.json");
    Console.WriteLine($"Writing {path}");
    var jsonString = JsonSerializer.Serialize(v, new JsonSerializerOptions()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
        WriteIndented = true
    });
    File.WriteAllText(path, jsonString);
}

//output errors
if (errors.Count == 0)
{
    Console.WriteLine("Success! All files converted");
}
else
{
    Console.WriteLine("Errors occurred during conversion process");
    foreach (var error in errors)
    {
        Console.WriteLine(error);
    }
}