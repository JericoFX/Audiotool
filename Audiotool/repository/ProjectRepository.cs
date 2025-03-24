using Audiotool.model;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Audiotool.repository;

public class ProjectRepository
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task SaveProjectAsync(ProjectState project, string filePath)
    {
        try
        {
            string json = JsonSerializer.Serialize(project, _jsonOptions);
            await File.WriteAllTextAsync(filePath, json);
            project.LastSavedPath = filePath;
            project.IsModified = false;
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to save project: {ex.Message}", ex);
        }
    }

    public async Task<ProjectState> LoadProjectAsync(string filePath)
    {
        try
        {
            string json = await File.ReadAllTextAsync(filePath);
            ProjectState? project = JsonSerializer.Deserialize<ProjectState>(json, _jsonOptions);
            
            if (project == null)
                throw new Exception("Failed to deserialize project file");
                
            project.LastSavedPath = filePath;
            project.IsModified = false;
            return project;
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to load project: {ex.Message}", ex);
        }
    }
} 