using StardewValley;
using StardewValley.GameData.Buffs;

namespace SelectableBuffs;

public static class Buffs
{
    private static KeyValuePair<string, BuffData>[]? _statueOfBlessingsBuffs;
    private static KeyValuePair<string, BuffData>[]? _dwarfStatueBuffs;

    public static KeyValuePair<string, BuffData>[] GetStatueOfBlessingsBuffs()
    {
        _statueOfBlessingsBuffs = DataLoader.Buffs(Game1.content).Where(pair => pair.Key.StartsWith("statue_of_blessings_")).ToArray();
        return _statueOfBlessingsBuffs;
    }

    public static KeyValuePair<string, BuffData>[] GetDwarfStatueBuffs()
    {
        _dwarfStatueBuffs = DataLoader.Buffs(Game1.content).Where(pair => pair.Key.StartsWith("dwarfStatue_")).ToArray();
        return _dwarfStatueBuffs;
    }
}