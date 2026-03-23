using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.GameData.Buffs;
using StardewValley.Menus;
using StardewValley.TokenizableStrings;

namespace SelectableBuffs;

public static class Buffs
{
    private static KeyValuePair<string, BuffData>[]? _statueOfBlessingsBuffs;
    private static KeyValuePair<string, BuffData>[]? _dwarfStatueBuffs;

    private static KeyValuePair<string, BuffData>[] GetStatueOfBlessingsBuffs()
    {
        _statueOfBlessingsBuffs = DataLoader.Buffs(Game1.content).Where(pair => pair.Key.StartsWith("statue_of_blessings_")).ToArray();
        return _statueOfBlessingsBuffs;
    }

    private static KeyValuePair<string, BuffData>[] GetDwarfStatueBuffs()
    {
        _dwarfStatueBuffs = DataLoader.Buffs(Game1.content).Where(pair => pair.Key.StartsWith("dwarfStatue_")).ToArray();
        return _dwarfStatueBuffs;
    }

    public static List<SelectionOption> GetStatueOfBlessingsBuffsAsOptions()
    {
        List<SelectionOption> options = new();
        foreach (var (key, buffData) in GetStatueOfBlessingsBuffs())
        {
            string name = TokenParser.ParseText(buffData.DisplayName);
            string description = GetBuffDescription(key, buffData);
            string title = description.Length < 1 ? name : name + Environment.NewLine + description;

            Texture2D texture = buffData.IconTexture == "TileSheets\\BuffsIcons" ? Game1.buffsIcons : Game1.content.Load<Texture2D>(buffData.IconTexture);
            Rectangle sourceRect = Game1.getSourceRectForStandardTileSheet(texture, buffData.IconSpriteIndex, 16, 16);
            Sprite sprite = new Sprite(texture, sourceRect, new SliceSettings(4f));

            options.Add(new SelectionOption(key, title, sprite));
        }

        return options;
    }

    public static List<SelectionOption> GetDwarfStatueBuffsAsOptions()
    {
        List<SelectionOption> options = new();
        foreach (var (key, buffData) in GetDwarfStatueBuffs())
        {
            string title = TokenParser.ParseText(buffData.Description);
            Texture2D texture = buffData.IconTexture == "TileSheets\\BuffsIcons" ? Game1.buffsIcons : Game1.content.Load<Texture2D>(buffData.IconTexture);
            Rectangle sourceRect = Game1.getSourceRectForStandardTileSheet(texture, buffData.IconSpriteIndex, 16, 16);
            Sprite sprite = new Sprite(texture, sourceRect, new SliceSettings(4f));
            options.Add(new SelectionOption(key, title, sprite));
        }

        return options;
    }

    private static string GetBuffDescription(string id, BuffData buffData)
    {
        if (buffData.Description != null)
        {
            string parsedDescription = TokenParser.ParseText(buffData.Description);
            return id == "statue_of_blessings_3" ? string.Format(parsedDescription, "3") : parsedDescription;
        }

        string description = string.Empty;
        foreach (var attribute in BuffsDisplay.displayAttributes)
        {
            float value = attribute.Value(new Buff(id));
            if (value == 0)
            {
                continue;
            }

            description += description.Length < 1 ? attribute.Description(value) : Environment.NewLine + attribute.Description(value);
        }

        return description;
    }
}