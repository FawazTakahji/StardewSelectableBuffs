using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewUI.Framework;

namespace SelectableBuffs;

public class ModEntry : Mod
{
    public override void Entry(IModHelper helper)
    {
        I18n.Init(helper.Translation);
        Singletons.ModManifest = ModManifest;
        Singletons.Monitor = Monitor;

        helper.Events.GameLoop.GameLaunched += OnGameLaunched;
    }

    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        try
        {
            Singletons.ViewEngine = Helper.ModRegistry.GetApi<IViewEngine>("focustense.StardewUI");
            if (Singletons.ViewEngine is null)
            {
                Singletons.Monitor.Log("Couldn't initialize IViewEngine api.", LogLevel.Error);
                return;
            }

            Singletons.ViewEngine.RegisterViews($"Mods/{Singletons.ModManifest.UniqueID}/views", "assets/views");
#if DEBUG
            Singletons.ViewEngine.EnableHotReloadingWithSourceSync();
#else
            Singletons.ViewEngine.PreloadAssets();
            Singletons.ViewEngine.PreloadModels(typeof(ViewModels.SelectionViewModel));
#endif
        }
        catch (Exception ex)
        {
            Singletons.Monitor.Log("Failed to initialize StardewUI: " + ex, LogLevel.Error);
            return;
        }

        try
        {
            HarmonyPatches.Initialize(ModManifest.UniqueID);
        }
        catch (Exception ex)
        {
            Singletons.Monitor.Log("Failed to initialize Harmony patches: " + ex, LogLevel.Error);
        }
    }
}