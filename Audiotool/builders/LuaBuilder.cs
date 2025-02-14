﻿using Audiotool.model;
using System.Collections.ObjectModel;
using System.IO;

namespace Audiotool.builders;

 public static class LuaBuilder
{
    private readonly static List<string> _manifest =
    [
        "-- Automatically generated by Renewed Audio Tool",
        "fx_version 'cerulean'",
        "game 'gta5'",
        "lua54 'yes'",
        "",
        "name 'Renewed Audio Tool'",
        "author 'Renewed Scripts | FjamZoo'",
        "repository 'https://github.com/Renewed-Scripts/Renewed-Audiotool'",
        "contributors 'Ehbw, CodeWalker, ChatDisabled, uShifty'",
        "version '1.0'",
        ""
    ];

    private readonly static List<string> _clientFile = [];


    public static string RelRootFolder { get; set; } = "data";

    public static string RelFileName { get; set; } = "audioexample_sounds.dat54.rel";

    public static string AwcRootFolder { get; set; } = "audiodirectory";

    public static string AwcFileName { get; set; } = "custom_sounds";

    private static void GenerateClientFile(List<Audio> AudioFiles, string soundSetName)
    {
        // Clear any existing lines
        _clientFile.Clear();

        // Lines for the loadAudioFile() function
        _clientFile.Add("local function loadAudioFile()");
        _clientFile.Add($"    if not RequestScriptAudioBank('audiodirectory/{AwcFileName}', false) then");
        _clientFile.Add($"        while not RequestScriptAudioBank('audiodirectory/{AwcFileName}', false) do");
        _clientFile.Add("            Wait(0)");
        _clientFile.Add("        end");
        _clientFile.Add("    end");
        _clientFile.Add("");
        _clientFile.Add("    return true");
        _clientFile.Add("end");
        _clientFile.Add("");

        // Lines for the RegisterCommand code
        _clientFile.Add("RegisterCommand('testaudio', function()");
        _clientFile.Add("    if loadAudioFile() then");
        _clientFile.Add("        local sounds = {");

        // Insert each user sound as ["soundName"] = true,
        foreach (Audio audioFile in AudioFiles)
        {
            _clientFile.Add($"            [\"{audioFile.FileName}\"] = true,");
        }

        _clientFile.Add("        }");
        _clientFile.Add("");
        _clientFile.Add("        for sound in pairs(sounds) do");
        _clientFile.Add("            print(\"Playing sound \" .. sound)");
        _clientFile.Add($"            PlaySoundFromEntity(-1, sound, PlayerPedId(), '{soundSetName}', 0, 0)");
        _clientFile.Add("            Wait(500)");
        _clientFile.Add("        end");
        _clientFile.Add("    end");
        _clientFile.Add("end, false)");
    }

    public static void GenerateManifest(string outputPath, List<Audio> AudioFiles, bool debugFiles, string SoundSetName)
    {
        string awcFileName = AwcFileName + ".awc";

        _manifest.Add("files {");
        _manifest.Add($"    '{RelRootFolder}/{RelFileName}',");
        _manifest.Add($"    '{AwcRootFolder}/{awcFileName}'");
        _manifest.Add("}");
        _manifest.Add("");

        _manifest.Add($"data_file 'AUDIO_WAVEPACK'  '{AwcRootFolder}'");
        _manifest.Add($"data_file 'AUDIO_SOUNDDATA' '{RelRootFolder}/audioexample_sounds.dat'");


        if (debugFiles)
        {
            _manifest.Add("");
            _manifest.Add("client_script 'client/main.lua'");
            _manifest.Add("");

            string clientPath = Path.Combine(outputPath, "client");

            string LuaFile = Path.Combine(clientPath, "main.lua");

            GenerateClientFile(AudioFiles, SoundSetName);
            using var sw2 = new StreamWriter(LuaFile);
            foreach (string line in _clientFile)
            {
                sw2.WriteLine(line);
            }
        }
        
        string manifestPath = Path.Combine(outputPath, "fxmanifest.lua");
        using var sw = new StreamWriter(manifestPath);
        foreach (string line in _manifest)
        {
            sw.WriteLine(line);
        }
    }
}