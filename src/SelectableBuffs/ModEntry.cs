using GenericModConfigMenu;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace SelectableBuffs;

public class ModEntry : Mod
{
    public override void Entry(IModHelper helper)
    {
        I18n.Init(helper.Translation);
        Singletons.ModManifest = ModManifest;
        Singletons.Monitor = Monitor;

        try
        {
            Singletons.Config = helper.ReadConfig<ModConfig>();
        }
        catch (Exception e)
        {
            Singletons.Monitor.Log($"Failed to load config: {e}", LogLevel.Error);
        }

        helper.Events.GameLoop.GameLaunched += OnGameLaunched;
    }

    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        try
        {
            IGenericModConfigMenuApi? gmcm = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (gmcm is null)
            {
                Singletons.Monitor.Log("Couldn't retrieve GMCM api.");
            }
            else
            {
                RegisterOptions(gmcm);
            }
        }
        catch (Exception exception)
        {
            Singletons.Monitor.Log("Failed to initialize GMCM: " + exception, LogLevel.Error);
        }

        HarmonyPatches.Initialize(ModManifest.UniqueID);
    }

    private void RegisterOptions(IGenericModConfigMenuApi gmcm)
    {
        gmcm.Register(
            ModManifest,
            () => Singletons.Config = new ModConfig(),
            OnSaveConfig
            );

        gmcm.AddBoolOption(
            ModManifest,
            () => Singletons.Config.PatchStatueOfBlessings,
            value => Singletons.Config.PatchStatueOfBlessings = value,
            name: () => Game1.content.LoadString("Strings\\BigCraftables:StatueOfBlessings_Name"),
            I18n.StatueOfBlessingsTooltip
            );

        gmcm.AddBoolOption(
            ModManifest,
            () => Singletons.Config.PatchStatueOfDwarfKing,
            value => Singletons.Config.PatchStatueOfDwarfKing = value,
            name: () => Game1.content.LoadString("Strings\\BigCraftables:StatueOfTheDwarfKing_Name"),
            I18n.StatueOfDwarfKingTooltip
            );
    }

    private void OnSaveConfig()
    {
        try
        {
            Helper.WriteConfig(Singletons.Config);
        }
        catch (Exception e)
        {
            Singletons.Monitor.Log("Failed to save config: " + e, LogLevel.Error);
        }

        try
        {
            HarmonyPatches.CheckPatches();
        }
        catch (Exception e)
        {
            Singletons.Monitor.Log("Failed to check patches: " + e, LogLevel.Error);
        }
    }
}