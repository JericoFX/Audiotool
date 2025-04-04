﻿using System.Collections.ObjectModel;
using System.IO;
using Audiotool.model;
using FFMpegCore;

namespace Audiotool.Converters;

public static class WavConverter
{
    public static void ConvertToWav(List<Audio> audioFiles, string outputFolder)
    {   
        foreach (Audio audio in audioFiles)
        {
            string outputPath = Path.Combine(outputFolder, $"{audio.FileName}.wav");
            
            // Always convert to ensure consistent format, even if input is WAV
            FFMpegArguments ff = FFMpegArguments
                .FromFileInput(audio.FilePath);
            _ = ff.OutputToFile(outputPath, true, opt =>
            {
                opt.WithAudioSamplingRate(audio.SampleRate)
                    .WithoutMetadata()
                    .WithCustomArgument("-fflags +bitexact -flags:v +bitexact -flags:a +bitexact")
                    .WithAudioCodec("pcm_s16le")
                    .ForceFormat("wav")
                    .UsingMultithreading(true);
                if (audio.Channels != 1)
                    opt.WithCustomArgument("-ac 1");
            }).ProcessSynchronously();

            audio.FileSize = (ulong)new FileInfo(outputPath).Length;
        }
    }
    
    public static readonly string[] SupportedFormats = new string[]
    {
        ".mp3", ".wav", ".ogg", ".flac", ".aac", ".m4a", ".wma"
    };
    
    public static bool IsFormatSupported(string filePath)
    {
        string extension = Path.GetExtension(filePath).ToLowerInvariant();
        return Array.Exists(SupportedFormats, format => format == extension);
    }
}