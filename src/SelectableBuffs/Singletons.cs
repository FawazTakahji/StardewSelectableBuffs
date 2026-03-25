using StardewModdingAPI;
using StardewUI.Framework;

namespace SelectableBuffs;

public static class Singletons
{
    public static ModConfig Config { get; set; } = null!;
    public static IManifest ModManifest { get; set; } = null!;
    public static IMonitor Monitor { get; set; } = null!;

    public static IViewEngine? ViewEngine = null;

    public static bool IsBlessingMenuOpen = false;
    public static bool IsDwarfMenuOpen = false;
}