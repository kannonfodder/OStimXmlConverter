// See https://aka.ms/new-console-template for more information

using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
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

string? modPackName = null;
if (args.Length >= 3)
{
    modPackName = args[3];
}

Regex modPackPattern = new Regex(@"scene[\\/](?<pack>\w+)[\\/]");
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

        if (doc["scene"] != null)
        {
            var scene = new Scene();
            var sceneTag = doc["scene"];
            if (sceneTag == null) continue;
            var infoTag = sceneTag["info"];
            scene.Name = infoTag?.Attributes["name"]?.Value ?? sceneTag.Attributes["id"]?.Value!;
            scene.ModPack = modPackName ?? modPackPattern.Match(file).Groups["pack"].Value ?? "";
            
            scene.Length = Convert.ToSingle(sceneTag["anim"]?.Attributes["l"]?.Value);
            if (sceneTag["anim"]?.Attributes?["t"] != null)
            {
                if (sceneTag["anim"].Attributes["t"].Value == "T" && sceneTag["anim"].Attributes["dest"] != null)
                {
                    scene.Destination = sceneTag["anim"].Attributes["dest"].Value;
                }
            }
            var navTag = sceneTag["nav"];
            if (navTag != null)
            {
                for (var i = 0; i < navTag.ChildNodes.Count; i++)
                {
                    var tab = navTag.ChildNodes.Item(i);

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

            var speedTag = sceneTag["speed"];
            if (speedTag != null)
            {
                for (var i = 0; i < speedTag.ChildNodes.Count; i++)
                {
                    var sp = speedTag.ChildNodes.Item(i);
                    var anim = sp?["anim"];
                    if (anim == null) continue;
                    var speed = new Speed()
                    {
                        Animation = anim!.Attributes?["id"]?.Value,
                        DisplaySpeed = Convert.ToSingle(sp.Attributes["qnt"].Value),
                        PlaybackSpeed = Convert.ToSingle(anim?.Attributes?["playbackspeed"]?.Value ?? "1.0")
                    };
                    scene.Speeds.Add(speed);
                }
            }

            var metadata = sceneTag["metadata"];
            if (metadata != null)
            {
                if (metadata.Attributes["tags"] != null)
                {
                    scene.Tags.AddRange(metadata.Attributes["tags"].Value.Split(','));
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

            var actorsTag = sceneTag["actors"];
            if (actorsTag != null)
            {
                var orderedActorTags = actorsTag.ChildNodes.OfType<XmlNode>()
                    .OrderBy(at => Convert.ToInt32(at.Attributes["position"].Value));
                foreach (var actorTag in orderedActorTags)
                {
                    if (actorTag is not { Name: "actor" }) continue;
                    var actor = new Actor();
                    if (actorTag.Attributes["penisAngle"] != null)
                    {
                        actor.SosBend = Convert.ToInt32(actorTag.Attributes["penisAngle"].Value);
                    }

                    if (actorTag.Attributes["tags"] != null)
                    {
                        actor.Tags.AddRange(actorTag.Attributes["tags"].Value.Split(","));
                    }

                    if (actorTag.Attributes["feetOnGround"] != null)
                    {
                        actor.FeetOnGround = actorTag.Attributes["feetOnGround"].Value == "1";
                    }

                    if (actorTag.Attributes["scale"] != null)
                    {
                        actor.Scale = Convert.ToInt32(actorTag.Attributes["scale"].Value);
                    }

                    if (actorTag.Attributes["scaleHeight"] != null)
                    {
                        actor.ScaleHeight = Convert.ToInt32(actorTag.Attributes["scaleHeight"].Value);
                    }

                    if (actorTag.Attributes["expressionAction"] != null)
                    {
                        actor.ExpressionAction = Convert.ToInt32(actorTag.Attributes["expressionAction"].Value);
                    }

                    if (actorTag.Attributes["lookUp"] != null)
                    {
                        actor.LookUp = Convert.ToInt32(actorTag.Attributes["lookUp"].Value);
                    }

                    if (actorTag.Attributes["lookDown"] != null)
                    {
                        actor.LookDown = Convert.ToInt32(actorTag.Attributes["lookDown"].Value);
                    }

                    if (actorTag.Attributes["lookLeft"] != null)
                    {
                        actor.LookLeft = Convert.ToInt32(actorTag.Attributes["lookLeft"].Value);
                    }

                    if (actorTag.Attributes["lookRight"] != null)
                    {
                        actor.LookRight = Convert.ToInt32(actorTag.Attributes["lookRight"].Value);
                    }

                    if (actorTag.HasChildNodes)
                    {
                        for (var i = 0; i < actorTag.ChildNodes.Count; i++)
                        {
                            var child = actorTag.ChildNodes.Item(i);
                            if (child!.Name == "autotransition")
                            {
                                actor.AutoTransitions.Add(child.Attributes["type"].Value,
                                    child.Attributes["destination"].Value);
                            }
                        }
                    }

                    scene.Actors.Add(actor);
                }
            }

            var actionsTag = sceneTag["actions"];
            if (actionsTag != null)
            {
                for (var i = 0; i < actionsTag.ChildNodes.Count; i++)
                {
                    var actionTag = actionsTag.ChildNodes.Item(i);
                    if (actionTag is not { Name : "action" }) continue;
                    var action = new SceneAction
                    {
                        Actor = Convert.ToInt32(actionTag.Attributes["actor"].Value),
                        Type = actionTag.Attributes["type"].Value
                    };
                    if (actionTag.Attributes["target"] != null)
                    {
                        action.Target = Convert.ToInt32(actionTag.Attributes["target"].Value);
                    }

                    if (actionTag.Attributes["performer"] != null)
                    {
                        action.Performer = Convert.ToInt32(actionTag.Attributes["performer"].Value);
                    }

                    scene.Actions.Add(action);
                }
            }

            scenes.Add(file, scene);
        }
        else
        {
            Console.WriteLine($"{file} is not a scene xml");
        }
    }
    catch (Exception ex)
    {
        errors.Add($"{file} : {ex.Message}");
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
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
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