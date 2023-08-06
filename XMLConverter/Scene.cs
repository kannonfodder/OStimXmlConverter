namespace XMLConverter;

public class Scene
{
    public string Name { get; set; }
    public string ModPack { get; set; }
    public float Length { get; set; }
    public string? Destination { get; set; }
    public string? Origin { get; set; }

    public List<Navigation> Navigations { get; } = new List<Navigation>();
    public List<Speed> Speeds { get; } = new List<Speed>();
    public int DefaultSpeed { get; set; }
    public bool NoRandomSelection { get; set; }
    public string Furniture { get; set; } = "None";
    public List<string> Tags { get; } = new List<string>();
    public Dictionary<string, string> AutoTransitions { get; } = new Dictionary<string, string>();
    public List<Actor> Actors { get; } = new List<Actor>();
    public List<SceneAction> Actions { get; } = new List<SceneAction>();
}

public class Navigation
{
    public string Destination { get; set; }
    public string Origin { get; set; }
    public int Priority { get; set; }
    public string Description { get; set; }
    public string Icon { get; set; }
    public string Border { get; set; }
    public bool NoWarnings { get; set; }
}

public class Speed
{
    public string Animation { get; set; }
    public float PlaybackSpeed { get; set; }
    public float DisplaySpeed { get; set; }
}

public class Actor
{
    public string Type { get; set; } = "npc";
    public string IntendedSex { get; set; } = "any";
    public int SosBend { get; set; }
    public float Scale { get; set; }
    public float ScaleHeight { get; set; } = 120.748f;
    public int ExpressionAction { get; set; }
    public int LookUp { get; set; }
    public int LookDown { get; set; }
    public int LookLeft { get; set; }
    public int LookRight { get; set; }
    public List<string> Tags { get; set; } = new List<string>();
    public bool FeetOnGround { get; set; }
    public Dictionary<string, string> AutoTransitions { get; } = new Dictionary<string, string>();
}

public class SceneAction
{
    public string Type { get; set; }
    public int Actor { get; set; }
    public int? Target { get; set; }
    public int? Performer { get; set; }
}