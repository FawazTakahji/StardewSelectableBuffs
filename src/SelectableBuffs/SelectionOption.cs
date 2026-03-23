using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SelectableBuffs;

public class SelectionOption
{
    public string Key { get; set; }
    public string Title { get; set; }
    public Sprite Sprite { get; set; }

    public SelectionOption(string key, string title, Sprite sprite)
    {
        Key = key;
        Title = title;
        Sprite = sprite;
    }
}

public record Sprite(Texture2D Texture, Rectangle SourceRect, SliceSettings SliceSettings);

public record SliceSettings(float Scale);

