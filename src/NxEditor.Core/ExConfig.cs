﻿using Avalonia.SettingsFactory.Core;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NxEditor.Core;

public class ExConfig : ISettingsBase
{
    private static readonly string _path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NX-Editor", "config.json");

    public static ExConfig Shared { get; } = Load();
    public bool RequiresInput { get; set; } = true;

    [JsonIgnore]
    [Setting("Game Path", "The absolute path to your Totk game dump\n(e.g. F:\\Games\\Totk\\RomFS)")]
    public static string GamePath {
        get => TotkConfig.Shared.GamePath;
        set => TotkConfig.Shared.GamePath = value;
    }

    [Setting(UiType.Dropdown, "Resource:Langs", Name = "Game Region/Language")]
    public required string Lang { get; set; }

    [Setting("Load Resources from Disk", "Loads a customizable copy of the resource files from disk instead of donwloading the latest version from the live server")]
    public bool LoadResourcesFromDisk { get; set; }

    [Setting("Single Instance Lock", "Files opened through the shell will be opened as a tab in the current instance, rather than starting a second running application", Category = "Lock Settings")]
    public bool UseSingleInstance { get; set; } = true;

    [Setting("Single File Lock", "Restrict two files with the same path to be loaded concurrently", Category = "Lock Settings")]
    public bool UseSingleFileLock { get; set; } = false;

    [Setting(UiType.Dropdown, "Dark", "Light", Category = "Appearance")]
    public string Theme { get; set; } = "Dark";

    public ISettingsBase Save()
    {
        TotkConfig.Shared.Save();
        RequiresInput = false;

        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
        using FileStream fs = File.Create(_path);
        JsonSerializer.Serialize(fs, this);
        return this;
    }

    public static ExConfig Load()
    {
        if (!File.Exists(_path)) {
            return Create();
        }

        using FileStream fs = File.OpenRead(_path);
        return JsonSerializer.Deserialize<ExConfig>(fs) ?? Create();
    }

    private static ExConfig Create()
    {
        ExConfig config = new() {
            Lang = "USen",
            RequiresInput = true,
        };

        config.Save();
        return config;
    }

    public static bool ValidatePath(string path, string key)
    {
        if (path == null || File.Exists(path)) {
            return false;
        }

        return key switch {
            "GamePath" => File.Exists(Path.Combine(path, "Pack", "ZsDic.pack.zs")),
            _ => false,
        };
    }
}
