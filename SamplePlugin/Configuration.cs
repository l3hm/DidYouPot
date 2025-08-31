using Dalamud.Configuration;
using System;

namespace DidYouPot;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public bool IsConfigWindowMovable { get; set; } = true;
    public bool SomePropertyToBeSavedAndWithADefault { get; set; } = true;

    public bool MainWindowNoMove { get; set; } = false;
    public bool MainWindowNoResize { get; set; } = false;
    public bool MainWindowNoCollapse { get; set; } = false;
    public bool MainWindowNoDocking { get; set; } = false;

    // The below exist just to make saving less cumbersome
    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }
}
