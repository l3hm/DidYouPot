using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using Lumina.Excel.Sheets;

namespace DidYouPot.Windows;

public class MainWindow : Window, IDisposable
{
    private Plugin Plugin;

    private bool wasInCombat = false;

    private static readonly HashSet<uint> DebuffIds = new()
    {
        4514, 4371, 4370, 3964, 3304, 3166, 2911, 2522,
        2404, 2092, 1090, 1016, 696, 628, 215, 62
    };
    private const uint PotStatusId = 49;

    private sealed class MemberStats
    {
        public string Name = string.Empty;
        public int Pots = 0;
        public int Deaths = 0;
        public int Debuffs = 0;
        public bool WasUnconsciousLastTick = false;

        public HashSet<uint> ActiveDebuffIds { get; } = new();
        public bool HasPotStatus = false;
    }

    private readonly Dictionary<long, MemberStats> current = new();

    public MainWindow(Plugin plugin) : base("Check Damage Ressources##")
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(100, 100),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        Plugin = plugin;
    }

    public void Dispose() { }

    public override void Draw()
    {
        if (ImGui.Button("Show Settings"))
        {
            Plugin.ToggleConfigUI();
        }

        ImGui.Spacing();

        bool inCombat = Plugin.Condition[ConditionFlag.InCombat];

        if (inCombat && !wasInCombat)
        {
            current.Clear();
        }
        wasInCombat = inCombat;

        if (inCombat)
            UpdatePartyOnce();

        if (ImGui.BeginTable("dyp_table", 4, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingStretchProp))
        {
            ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthStretch, 2f);
            ImGui.TableSetupColumn("Pots");
            ImGui.TableSetupColumn("Deaths");
            ImGui.TableSetupColumn("Debuffs");
            ImGui.TableHeadersRow();

            foreach (var kv in current.Values)
            {
                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0); ImGui.TextUnformatted(kv.Name);
                ImGui.TableSetColumnIndex(1); ImGui.TextUnformatted(kv.Pots.ToString());
                ImGui.TableSetColumnIndex(2); ImGui.TextUnformatted(kv.Deaths.ToString());
                ImGui.TableSetColumnIndex(3); ImGui.TextUnformatted(kv.Debuffs.ToString());
            }

            ImGui.EndTable();
        }

        ImGui.Spacing();

        if (ImGui.Button("Reset Encounter"))
            current.Clear();

        ImGui.SameLine();
        ImGui.TextDisabled(inCombat ? "In combat" : "Out of combat");

        ImGui.TextDisabled($"party size: {Plugin.PartyList.Length} | tracked: {current.Count}");
    }

    private void UpdatePartyOnce()
    {
        for (int i = 0; i < Plugin.PartyList.Length; i++)
        {
            var pm = Plugin.PartyList[i];
            if (pm is null) continue;

            long key = pm.ObjectId;

            if (!current.TryGetValue(key, out var stats))
            {
                string name = pm.Name.TextValue;
                stats = new MemberStats { Name = name };
                current[key] = stats;
            }
            else
            {
                string nameNow = pm.Name.TextValue;
                if (!string.Equals(stats.Name, nameNow, StringComparison.Ordinal))
                    stats.Name = nameNow;
            }

            bool unconscious = pm.CurrentHP == 0;
            if (unconscious && !stats.WasUnconsciousLastTick)
                stats.Deaths++;
            stats.WasUnconsciousLastTick = unconscious;

            var currentDebuffs = new HashSet<uint>();
            bool hasPotNow = false;

            foreach (var status in pm.Statuses)
            {
                if (status is null) continue;
                uint id = status.StatusId;
                if (id == 0) continue;

                if (id == PotStatusId)
                {
                    hasPotNow = true;
                    if (!stats.HasPotStatus)
                    {
                        stats.Pots++;
                        stats.HasPotStatus = true;
                    }
                }

                if (DebuffIds.Contains(id))
                {
                    currentDebuffs.Add(id);
                    if (!stats.ActiveDebuffIds.Contains(id))
                    {
                        stats.ActiveDebuffIds.Add(id);
                        stats.Debuffs++;
                    }
                }
            }

            if (!hasPotNow && stats.HasPotStatus)
                stats.HasPotStatus = false;

            if (stats.ActiveDebuffIds.Count > 0)
                stats.ActiveDebuffIds.RemoveWhere(id => !currentDebuffs.Contains(id));
        }
    }
    public override void PreDraw()
    {
        Flags = ImGuiWindowFlags.None;

        if (Plugin.Configuration.MainWindowNoMove)
            Flags |= ImGuiWindowFlags.NoMove;
        if (Plugin.Configuration.MainWindowNoResize)
            Flags |= ImGuiWindowFlags.NoResize;
        if (Plugin.Configuration.MainWindowNoCollapse)
            Flags |= ImGuiWindowFlags.NoCollapse;
        if (Plugin.Configuration.MainWindowNoDocking)
            Flags |= ImGuiWindowFlags.NoDocking;
    }
}

