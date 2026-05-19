using StardewModdingAPI;

namespace SelectableBuffs;

public static class Singletons
{
    public static ModConfig Config { get; set; } = null!;
    public static IManifest ModManifest { get; set; } = null!;
    public static IMonitor Monitor { get; set; } = null!;
}