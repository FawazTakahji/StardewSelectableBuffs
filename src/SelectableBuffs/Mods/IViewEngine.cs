using System.ComponentModel;
using System.Runtime.CompilerServices;
using StardewValley.Menus;

namespace StardewUI.Framework;

/// <summary>
/// Public API for StardewUI, abstracting away all implementation details of views and trees.
/// </summary>
public interface IViewEngine
{
    /// <summary>
    /// Creates a menu from the StarML stored in a game asset, as provided by a mod via SMAPI or Content Patcher, and
    /// returns a controller for customizing the menu's behavior.
    /// </summary>
    /// <remarks>
    /// The menu that is created is the same as the result of <see cref="CreateMenuFromMarkup(string, object?)"/>. The
    /// menu is not automatically shown; to show it, use <see cref="Game1.activeClickableMenu"/> or equivalent.
    /// </remarks>
    /// <param name="assetName">The name of the StarML view asset in the content pipeline, e.g.
    /// <c>Mods/MyMod/Views/MyView</c>.</param>
    /// <param name="context">The context, or "model", for the menu's view, which holds any data-dependent values.
    /// <b>Note:</b> The type must implement <see cref="INotifyPropertyChanged"/> in order for any changes to this data
    /// to be automatically reflected in the UI.</param>
    /// <returns>A controller object whose <see cref="IMenuController.Menu"/> is the created menu and whose other
    /// properties can be used to change menu-level behavior.</returns>
    IMenuController CreateMenuControllerFromAsset(string assetName, object? context = null);

    /// <summary>
    /// Starts monitoring this mod's directory for changes to assets managed by any of the <c>Register</c> methods, e.g.
    /// views and sprites.
    /// </summary>
    /// <remarks>
    /// <para>
    /// If the <paramref name="sourceDirectory"/> argument is specified, and points to a directory with the same asset
    /// structure as the mod, then an additional sync will be set up such that files modified in the
    /// <c>sourceDirectory</c> while the game is running will be copied to the active mod directory and subsequently
    /// reloaded. In other words, pointing this at the mod's <c>.csproj</c> directory allows hot reloading from the
    /// source files instead of the deployed mod's files.
    /// </para>
    /// <para>
    /// Hot reload may impact game performance and should normally only be used during development and/or in debug mode.
    /// </para>
    /// </remarks>
    /// <param name="sourceDirectory">Optional source directory to watch and sync changes from. If not specified, or not
    /// a valid source directory, then hot reload will only pick up changes from within the live mod directory.</param>
    void EnableHotReloading(string? sourceDirectory = null);

    /// <summary>
    /// Begins preloading assets found in this mod's registered asset directories.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Preloading is performed in the background, and can typically help reduce first-time latency for showing menus or
    /// drawables, without any noticeable lag in game startup.
    /// </para>
    /// <para>
    /// Must be called after asset registration (<see cref="RegisterViews"/>, <see cref="RegisterSprites"/> and so on)
    /// in order to be effective, and must not be called more than once per mod otherwise errors or crashes may occur.
    /// </para>
    /// </remarks>
    void PreloadAssets();

    /// <summary>
    /// Declares that the specified context types will be used as either direct arguments or subproperties in one or
    /// more subsequent <c>CreateMenu</c> or <c>CreateDrawable</c> APIs, and instructs the framework to begin inspecting
    /// those types and optimizing for later use.
    /// </summary>
    /// <remarks>
    /// Data binding to mod-defined types uses reflection, which can become expensive when loading a very complex menu
    /// and/or binding to a very complex model for the first time. Preloading can perform this work in the background
    /// instead of causing latency when opening the menu.
    /// </remarks>
    /// <param name="types">The types that the mod expects to use as context.</param>
    void PreloadModels(params Type[] types);

    /// <summary>
    /// Registers a mod directory to be searched for view (StarML) assets. Uses the <c>.sml</c> extension.
    /// </summary>
    /// <param name="assetPrefix">The prefix for all asset names, e.g. <c>Mods/MyMod/Views</c>. This can be any value
    /// but the same prefix must be used in <c>include</c> elements and in API calls to create views.</param>
    /// <param name="modDirectory">The physical directory where the asset files are located, relative to the mod
    /// directory. Typically a path such as <c>assets/views</c> or <c>assets/ui/views</c>.</param>
    public void RegisterViews(string assetPrefix, string modDirectory);
}

/// <summary>
/// Wrapper for a mod-managed <see cref="IClickableMenu"/> that allows further customization of menu-level properties
/// not accessible to StarML or data binding.
/// </summary>
public interface IMenuController : IDisposable
{
    /// <summary>
    /// Event raised after the menu has been closed.
    /// </summary>
    event Action Closed;

    /// <summary>
    /// Gets the menu, which can be opened using <see cref="Game1.activeClickableMenu"/>, or as a child menu.
    /// </summary>
    IClickableMenu Menu { get; }

    /// <summary>
    /// Closes the menu.
    /// </summary>
    /// <remarks>
    /// This method allows programmatic closing of the menu. It performs the same action that would be performed by
    /// pressing one of the configured menu keys (e.g. ESC), clicking the close button, etc., and follows the same
    /// rules, i.e. will not allow closing if <see cref="CanClose"/> is <c>false</c>.
    /// </remarks>
    void Close();
}

/// <summary>
/// Extensions for the <see cref="IViewEngine"/> interface.
/// </summary>
internal static class ViewEngineExtensions
{
    /// <summary>
    /// Starts monitoring this mod's directory for changes to assets managed by any of the <see cref="IViewEngine"/>'s
    /// <c>Register</c> methods, e.g. views and sprites, and attempts to set up an additional sync from the mod's
    /// project (source) directory to the deployed mod directory so that hot reloads can be initiated from the IDE.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Callers should normally omit the <paramref name="callerFilePath"/> parameter in their call; this will cause it
    /// to be replaced at compile time with the actual file path of the caller, and used to automatically detect the
    /// project path.
    /// </para>
    /// <para>
    /// If detection/sync fails due to an unusual project structure, consider providing an exact path directly to
    /// <see cref="IViewEngine.EnableHotReloading(string)"/> instead of using this extension.
    /// </para>
    /// <para>
    /// Hot reload may impact game performance and should normally only be used during development and/or in debug mode.
    /// </para>
    /// </remarks>
    /// <param name="viewEngine">The view engine API.</param>
    /// <param name="callerFilePath">Do not pass in this argument, so that <see cref="CallerFilePathAttribute"/> can
    /// provide the correct value on build.</param>
    public static void EnableHotReloadingWithSourceSync(
        this IViewEngine viewEngine,
        [CallerFilePath] string? callerFilePath = null
    )
    {
        viewEngine.EnableHotReloading(FindProjectDirectory(callerFilePath));
    }

    // Attempts to determine the project root directory given the path to an arbitrary source file by walking up the
    // directory tree until it finds a directory containing a file with .csproj extension.
    private static string? FindProjectDirectory(string? sourceFilePath)
    {
        if (string.IsNullOrEmpty(sourceFilePath))
        {
            return null;
        }
        for (var dir = Directory.GetParent(sourceFilePath); dir is not null; dir = dir.Parent)
        {
            if (dir.EnumerateFiles("*.csproj").Any())
            {
                return dir.FullName;
            }
        }
        return null;
    }
}