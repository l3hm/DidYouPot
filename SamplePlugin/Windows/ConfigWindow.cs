using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;

namespace DidYouPot.Windows;

public class ConfigWindow : Window, IDisposable
{
    private Configuration Configuration;

    // We give this window a constant ID using ###.
    // This allows for labels to be dynamic, like "{FPS Counter}fps###XYZ counter window",
    // and the window ID will always be "###XYZ counter window" for ImGui
    public ConfigWindow(Plugin plugin) : base("Configuration###")
    {
        Flags = ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar |
                ImGuiWindowFlags.NoScrollWithMouse;

        Size = new Vector2(200, 200);
        SizeCondition = ImGuiCond.Always;

        Configuration = plugin.Configuration;
    }

    public void Dispose() { }

    public override void PreDraw()
    {
        // Flags must be added or removed before Draw() is being called, or they won't apply
        if (Configuration.IsConfigWindowMovable)
        {
            Flags &= ~ImGuiWindowFlags.NoMove;
        }
        else
        {
            Flags |= ImGuiWindowFlags.NoMove;
        }
    }

    public override void Draw()
    {
        var movable = Configuration.IsConfigWindowMovable;
        if (ImGui.Checkbox("Movable Config Window", ref movable))
        {
            Configuration.IsConfigWindowMovable = movable;
            Configuration.Save();
        }

        ImGui.Separator();
        ImGui.Text("Main Window Options:");

        var noMove = Configuration.MainWindowNoMove;
        if (ImGui.Checkbox("No Move", ref noMove))
        {
            Configuration.MainWindowNoMove = noMove;
            Configuration.Save();
        }

        var noResize = Configuration.MainWindowNoResize;
        if (ImGui.Checkbox("No Resize", ref noResize))
        {
            Configuration.MainWindowNoResize = noResize;
            Configuration.Save();
        }

        var noCollapse = Configuration.MainWindowNoCollapse;
        if (ImGui.Checkbox("No Collapse", ref noCollapse))
        {
            Configuration.MainWindowNoCollapse = noCollapse;
            Configuration.Save();
        }

        var noDocking = Configuration.MainWindowNoDocking;
        if (ImGui.Checkbox("No Docking", ref noDocking))
        {
            Configuration.MainWindowNoDocking = noDocking;
            Configuration.Save();
        }
    }
}
