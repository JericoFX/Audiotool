using Microsoft.Win32;
using Audiotool.repository;
using System.Collections.ObjectModel;
using Audiotool.model;
using System.Windows;
using System;
using System.Threading.Tasks;
using System.IO;
using Audiotool;
using Audiotool.Converters;

namespace Audiotool.viewmodel;

public class NativeAudio : ViewModelBase
{
    private ObservableCollection<Audio> _audioFiles;

    public ObservableCollection<Audio> AudioFiles
    {
        get { 
            return _audioFiles; 
        }
        set { 
            _audioFiles = value;
            OnPropertyChanged();
            CurrentProject.IsModified = true;
            UpdateWindowTitle();
        }
    }

    private Audio _selectedAudio;

    public Audio SelectedAudio
    {
        get { return _selectedAudio; }
        set { 
            _selectedAudio = value;
            OnPropertyChanged();
        }
    }

    private string _soundSetName;

    public string SoundSetName
    {
        get { 
            return _soundSetName; 
        }
        set {
            _soundSetName = value;
            OnPropertyChanged();
            CurrentProject.IsModified = true;
            UpdateWindowTitle();
        }
    }

    private string _audioBankName;

    public string AudioBankName
    {
        get
        {
            return _audioBankName;
        }
        set
        {
            _audioBankName = value;
            OnPropertyChanged();
            CurrentProject.IsModified = true;
            UpdateWindowTitle();
        }
    }

    private string _outputPath;

    public string OutputPath
    {
        get { return _outputPath; }
        set { 
            _outputPath = value;
            OnPropertyChanged();
            CurrentProject.IsModified = true;
            UpdateWindowTitle();
        }
    }

    private string _projectName = "New Project";
    public string ProjectName
    {
        get { return _projectName; }
        set
        {
            _projectName = value;
            OnPropertyChanged();
            CurrentProject.IsModified = true;
            UpdateWindowTitle();
        }
    }

    private string _windowTitle = "Audiotool";
    public string WindowTitle
    {
        get { return _windowTitle; }
        set
        {
            _windowTitle = value;
            OnPropertyChanged();
        }
    }

    private ProjectState _currentProject = new();
    public ProjectState CurrentProject
    {
        get { return _currentProject; }
        set
        {
            _currentProject = value;
            OnPropertyChanged();
            UpdateWindowTitle();
        }
    }

    private readonly NativeAudioRepo _repo;
    private readonly ProjectRepository _projectRepo = new();

    private string _supportedFormatsText = string.Empty;
    public string SupportedFormatsText
    {
        get { return _supportedFormatsText; }
        set
        {
            _supportedFormatsText = value;
            OnPropertyChanged();
        }
    }

    public RelayCommand AddFilesCommand => new(execute => SelectAudioFiles(), canExecute => true);
    public RelayCommand DeleteCommand => new(execute => RemoveAudioFile(), canExecute => SelectedAudio != null);
    public RelayCommand ExportCommand => new(execute => _repo.BuildAWC(SoundSetName, AudioBankName, OutputPath, AudioFiles), canExecute => AudioFiles != null && AudioFiles.Count > 0);
    public RelayCommand OutputFolderCommand => new(execute => SetOutputFolder(), canExecute => true);
    public RelayCommand NewProjectCommand => new(execute => NewProject(), canExecute => true);
    public RelayCommand SaveProjectCommand => new(execute => SaveProject(false), canExecute => true);
    public RelayCommand SaveProjectAsCommand => new(execute => SaveProject(true), canExecute => true);
    public RelayCommand OpenProjectCommand => new(execute => OpenProject(), canExecute => true);

    private void UpdateWindowTitle()
    {
        string modified = CurrentProject.IsModified ? "*" : "";
        string projectName = string.IsNullOrEmpty(ProjectName) ? "New Project" : ProjectName;
        WindowTitle = $"Audiotool - {projectName}{modified}";
    }

    private void NewProject()
    {
        if (CurrentProject.IsModified)
        {
            MessageBoxResult result = MessageBox.Show("Save changes to the current project?", "Unsaved Changes", MessageBoxButton.YesNoCancel);
            
            if (result == MessageBoxResult.Cancel)
                return;
                
            if (result == MessageBoxResult.Yes)
            {
                bool saved = SaveProject(false);
                if (!saved) return;
            }
        }
        
        SoundSetName = "special_soundset";
        AudioBankName = "custom_sounds";
        OutputPath = string.Empty;
        ProjectName = "New Project";
        AudioFiles = new ObservableCollection<Audio>();
        CurrentProject = new ProjectState
        {
            SoundSetName = SoundSetName,
            AudioBankName = AudioBankName,
            OutputPath = OutputPath,
            ProjectName = ProjectName,
            IsModified = false
        };
    }

    private bool SaveProject(bool saveAs)
    {
        string filePath = CurrentProject.LastSavedPath;
        
        if (string.IsNullOrEmpty(filePath) || saveAs)
        {
            SaveFileDialog dialog = new()
            {
                Filter = "Audio Project Files (*.atp)|*.atp",
                Title = "Save Project",
                DefaultExt = ".atp",
                FileName = ProjectName
            };
            
            if (dialog.ShowDialog() != true)
                return false;
                
            filePath = dialog.FileName;
        }
        
        try
        {
            CurrentProject.SoundSetName = SoundSetName;
            CurrentProject.AudioBankName = AudioBankName;
            CurrentProject.OutputPath = OutputPath;
            CurrentProject.ProjectName = ProjectName = Path.GetFileNameWithoutExtension(filePath);
            CurrentProject.AudioFiles = new List<Audio>(_repo.GetAudioFiles());
            
            Task.Run(async () => 
            {
                await _projectRepo.SaveProjectAsync(CurrentProject, filePath);
            }).GetAwaiter().GetResult();
            
            UpdateWindowTitle();
            MessageBox.Show("Project saved successfully.", "Save Project", MessageBoxButton.OK, MessageBoxImage.Information);
            return true;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to save project: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }
    }

    private void OpenProject()
    {
        if (CurrentProject.IsModified)
        {
            MessageBoxResult result = MessageBox.Show("Save changes to the current project?", "Unsaved Changes", MessageBoxButton.YesNoCancel);
            
            if (result == MessageBoxResult.Cancel)
                return;
                
            if (result == MessageBoxResult.Yes)
            {
                bool saved = SaveProject(false);
                if (!saved) return;
            }
        }
        
        OpenFileDialog dialog = new()
        {
            Filter = "Audio Project Files (*.atp)|*.atp",
            Title = "Open Project"
        };
        
        if (dialog.ShowDialog() != true)
            return;
            
        try
        {
            ProjectState project = Task.Run(async () => 
            {
                return await _projectRepo.LoadProjectAsync(dialog.FileName);
            }).GetAwaiter().GetResult();
            
            // Update the model and UI
            _repo.LoadProjectState(project);
            SoundSetName = project.SoundSetName;
            AudioBankName = project.AudioBankName;
            OutputPath = project.OutputPath;
            ProjectName = project.ProjectName;
            AudioFiles = _repo.GetAudioFiles();
            CurrentProject = project;
            
            UpdateWindowTitle();
            MessageBox.Show("Project loaded successfully.", "Open Project", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to load project: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void SetOutputFolder()
    {
        var dialog = new OpenFolderDialog();
        var result = dialog.ShowDialog();

        if (result == true)
        {
            OutputPath = dialog.FolderName;
        }
    }

    private void RemoveAudioFile() 
    {
        if (SelectedAudio != null)
        {
            AudioFiles = _repo.RemoveAudioFile(SelectedAudio.FileName);
        }
    }

    private void SelectAudioFiles()
    {
        OpenFileDialog dialog = new()
        {
            Multiselect = true,
            Filter = $"Supported Audio Files ({string.Join(", ", WavConverter.SupportedFormats)})|{string.Join(";", WavConverter.SupportedFormats.Select(f => $"*{f}"))}"
        };

        if (dialog.ShowDialog() != true || dialog.FileNames.Length <= 0) return;

        foreach (string path in dialog.FileNames)
        {
            try
            {
                Task.Run(async () => await _repo.AddAudioFile(path)).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding file {Path.GetFileName(path)}: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        AudioFiles = _repo.GetAudioFiles();
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    public NativeAudio()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    {
        SoundSetName = "special_soundset";
        AudioBankName = "custom_sounds";
        _repo = new NativeAudioRepo();
        _audioFiles = new ObservableCollection<Audio>();
        
        UpdateWindowTitle();
    }
}

