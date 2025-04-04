﻿using Audiotool.model;
using FFMpegCore;
using System.Collections.ObjectModel;
using System.IO;
using Audiotool.builders;
using Audiotool.Converters;
using System.Linq;
using System.Windows.Threading;
using System.Windows;

namespace Audiotool.repository;

public class NativeAudioRepo
{
    private readonly List<Audio> AudioFiles = [];

    public async Task AddAudioFile(string path)
    {
        if (!WavConverter.IsFormatSupported(path))
        {
            throw new Exception($"Unsupported audio format. Supported formats: {string.Join(", ", WavConverter.SupportedFormats)}");
        }
        
        IMediaAnalysis info = await FFProbe.AnalyseAsync(path);
        if (info.PrimaryAudioStream == null)
        {
            throw new Exception("Unable to retrieve primary audio stream");
        }

        string filename = Path.GetFileNameWithoutExtension(path);

        Audio? currentAudioFile = AudioFiles.FirstOrDefault(a => a.FileName == filename);

        if (currentAudioFile != null) return;

        Audio audioFile = new()
        {
            Codec = "ADPCM",
            FilePath = path,
            FileName = filename,
            FileExtension = Path.GetExtension(path).ToLowerInvariant(),
            Samples = (int)Math.Round(info.Duration.TotalSeconds * info.PrimaryAudioStream.SampleRateHz),
            SampleRate = info.PrimaryAudioStream.SampleRateHz,
            Duration = info.Duration,
            Channels = info.PrimaryAudioStream.Channels,
            FileSize = (ulong)(info.PrimaryAudioStream.BitRate * info.Duration.TotalSeconds * info.PrimaryAudioStream.Channels)
        };


       AudioFiles.Add(audioFile);
    }

    public ObservableCollection<Audio> GetAudioFiles() => new(AudioFiles);

    public ObservableCollection<Audio> RemoveAudioFile(string fileName)
    {
        foreach (Audio audio in AudioFiles)
        {
            if (audio.FileName == fileName)
            {
                AudioFiles.Remove(audio);
                break;
            }
        }


        return new ObservableCollection<Audio>(AudioFiles);
    }

    private static void CreateFolders(string path, string dataPath, string audioDirectoryPath, string wavPath)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        if (!Directory.Exists(dataPath))
        {
            Directory.CreateDirectory(dataPath);
        }

        if (!Directory.Exists(audioDirectoryPath))
        {
            Directory.CreateDirectory(audioDirectoryPath);
        }

        if (!Directory.Exists(wavPath))
        {
            Directory.CreateDirectory(wavPath);
        }
    }

    public void BuildAWC(string SoundSet, string AudioBank, string? folderPath, ObservableCollection<Audio> _newList, bool debugFiles = true)
    {
        string path = Path.Combine(folderPath ?? AppContext.BaseDirectory, "Renewed-Audio");
        string wavPath = Path.Combine(path, "wav");
        string dataPath = Path.Combine(path, "data");
        string audioDirectoryPath = Path.Combine(path, "audiodirectory");

        CreateFolders(path, dataPath, audioDirectoryPath, wavPath);

        if (debugFiles)
        {
            string clientPath = Path.Combine(path, "client");
            if (!Directory.Exists(clientPath))
            {
                Directory.CreateDirectory(clientPath);
            }
        }

        WavConverter.ConvertToWav(AudioFiles, wavPath);
        Dat54Builder.ConstructDat54(AudioFiles, path, AudioBank, SoundSet);
        AWCBuilder.GenerateXML(AudioFiles, audioDirectoryPath, wavPath, AudioBank);

        LuaBuilder.AwcFileName = AudioBank;

        LuaBuilder.GenerateManifest(path, AudioFiles, true, SoundSet);

        MessageBox.Show("Resource has been build!");
    }
    
    public ProjectState GetProjectState(string soundSetName, string audioBankName, string outputPath)
    {
        return new ProjectState
        {
            SoundSetName = soundSetName,
            AudioBankName = audioBankName,
            OutputPath = outputPath,
            AudioFiles = new List<Audio>(AudioFiles),
            IsModified = true
        };
    }
    
    public void LoadProjectState(ProjectState project)
    {
        AudioFiles.Clear();
        foreach (var audio in project.AudioFiles)
        {
            // Skip files that no longer exist
            if (File.Exists(audio.FilePath))
            {
                AudioFiles.Add(audio);
            }
        }
    }
}
