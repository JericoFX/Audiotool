using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Audiotool.model;

public class ProjectState
{
    public string SoundSetName { get; set; } = "special_soundset";
    public string AudioBankName { get; set; } = "custom_sounds";
    public string OutputPath { get; set; } = string.Empty;
    public List<Audio> AudioFiles { get; set; } = new List<Audio>();
    public string ProjectName { get; set; } = "New Project";
    public string LastSavedPath { get; set; } = string.Empty;

    [JsonIgnore]
    public bool IsModified { get; set; } = false;
} 