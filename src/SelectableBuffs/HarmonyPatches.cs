using System.Reflection;
using HarmonyLib;
using Microsoft.Xna.Framework;
using SelectableBuffs.ViewModels;
using StardewModdingAPI;
using StardewUI.Framework;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Constants;
using StardewObject = StardewValley.Object;

namespace SelectableBuffs;

public class HarmonyPatches
{
    private static Harmony _harmony = null!;

    public static void Initialize(string uniqueId)
    {
        _harmony = new Harmony(uniqueId);

        if (Singletons.Config.PatchStatueOfBlessings)
        {
            TryPatchStatueOfBlessings();
        }
        if (Singletons.Config.PatchStatueOfDwarfKing)
        {
            TryPatchStatueOfDwarfKing();
        }
    }

    public static void CheckPatches()
    {
        List<MethodBase> methods = _harmony.GetPatchedMethods().ToList();
        MethodBase checkForActionOnBlessedStatue = AccessTools.Method(typeof(StardewObject), "CheckForActionOnBlessedStatue");
        MethodBase checkForAction = AccessTools.Method(typeof(StardewObject), nameof(StardewObject.checkForAction));

        if (methods.Any(x => x == checkForActionOnBlessedStatue))
        {
            if (!Singletons.Config.PatchStatueOfBlessings)
            {
                TryUnpatchStatueOfBlessings(checkForActionOnBlessedStatue);
            }
        }
        else if (Singletons.Config.PatchStatueOfBlessings)
        {
            TryPatchStatueOfBlessings(checkForActionOnBlessedStatue);
        }

        if (methods.Any(x => x == checkForAction))
        {
            if (!Singletons.Config.PatchStatueOfDwarfKing)
            {
                TryUnpatchStatueOfDwarfKing(checkForAction);
            }
        }
        else if (Singletons.Config.PatchStatueOfDwarfKing)
        {
            TryPatchStatueOfDwarfKing(checkForAction);
        }
    }

    private static void TryPatchStatueOfBlessings(MethodBase? original = null)
    {
        try
        {
            _harmony.Patch(
                original: original ?? AccessTools.Method(typeof(StardewObject), "CheckForActionOnBlessedStatue"),
                prefix: new HarmonyMethod(typeof(ObjectPatch), nameof(ObjectPatch.CheckForActionOnBlessedStatue_Prefix))
            );
        }
        catch (Exception e)
        {
            Singletons.Monitor.Log("Failed to patch Statue of Blessings: " + e, LogLevel.Error);
        }
    }

    private static void TryUnpatchStatueOfBlessings(MethodBase original)
    {
        try
        {
            _harmony.Unpatch(original, AccessTools.Method(typeof(ObjectPatch), nameof(ObjectPatch.CheckForActionOnBlessedStatue_Prefix)));
        }
        catch (Exception e)
        {
            Singletons.Monitor.Log("Failed to unpatch Statue of Blessings: " + e, LogLevel.Error);
        }
    }

    private static void TryPatchStatueOfDwarfKing(MethodBase? original = null)
    {
        try
        {
            _harmony.Patch(
                original: original ?? AccessTools.Method(typeof(StardewObject), nameof(StardewObject.checkForAction)),
                prefix: new HarmonyMethod(typeof(ObjectPatch), nameof(ObjectPatch.CheckForAction_Prefix))
            );
        }
        catch (Exception e)
        {
            Singletons.Monitor.Log("Failed to patch Statue of Dwarf King: " + e, LogLevel.Error);
        }
    }

    private static void TryUnpatchStatueOfDwarfKing(MethodBase original)
    {
        try
        {
            _harmony.Unpatch(original, AccessTools.Method(typeof(ObjectPatch), nameof(ObjectPatch.CheckForAction_Prefix)));
        }
        catch (Exception e)
        {
            Singletons.Monitor.Log("Failed to unpatch Statue of Dwarf King: " + e, LogLevel.Error);
        }
    }
}

public class ObjectPatch
{
    public static bool CheckForActionOnBlessedStatue_Prefix(StardewObject __instance, ref bool __result, Farmer who, GameLocation location, bool justCheckingForActivitiy)
    {
        if (Singletons.IsBlessingMenuOpen || Singletons.IsDwarfMenuOpen || justCheckingForActivitiy)
        {
            __result = true;
            return false;
        }
        if (Singletons.ViewEngine is null)
        {
            Singletons.Monitor.Log("Can't show menu because ViewEngine is null.", LogLevel.Warn);
            return true;
        }
        if (who.stats.Get(StatKeys.Mastery(0)) < 1U)
        {
            Game1.showGlobalMessage(Game1.content.LoadString("Strings\\1_6_Strings:MasteryRequirement"));
            Game1.playSound("cancel", null);
            __result = true;
            return false;
        }
        if (who.hasBuffWithNameContainingString("statue_of_blessings_") || who.hasBeenBlessedByStatueToday)
        {
            __result = false;
            return false;
        }

        List<SelectionOption> buffs;
        try
        {
            buffs = Buffs.GetStatueOfBlessingsBuffsAsOptions();
            Singletons.IsBlessingMenuOpen = true;
        }
        catch (Exception e)
        {
            Singletons.Monitor.Log("Failed to get buffs: " + e, LogLevel.Error);
            return true;
        }

        SelectionViewModel context = new SelectionViewModel(I18n.ChooseBlessing(), buffs, s =>
        {
            if (s != "canceled")
            {
                ApplyBlessing(__instance, who, location, s);
            }

            Singletons.IsBlessingMenuOpen = false;
        });
        IMenuController controller = Singletons.ViewEngine.CreateMenuControllerFromAsset($"Mods/{Singletons.ModManifest.UniqueID}/views/SelectionView", context);
        context.SetController(controller);
        Game1.activeClickableMenu = controller.Menu;

        __result = true;
        return false;
    }

    public static bool CheckForAction_Prefix(StardewObject __instance, ref bool __result, Farmer who, bool justCheckingForActivity)
    {
        if (justCheckingForActivity
            || __instance.QualifiedItemId.Length != 24
            || __instance.QualifiedItemId != "(BC)StatueOfTheDwarfKing")
        {
            return true;
        }
        if (Singletons.IsBlessingMenuOpen || Singletons.IsDwarfMenuOpen)
        {
            __result = true;
            return false;
        }
        if (Singletons.ViewEngine is null)
        {
            Singletons.Monitor.Log("Can't show menu because ViewEngine is null.", LogLevel.Warn);
            return true;
        }

        if (who.stats.Get(StatKeys.Mastery(3)) < 1U)
        {
            Game1.showGlobalMessage(Game1.content.LoadString("Strings\\1_6_Strings:MasteryRequirement"));
            Game1.playSound("cancel");
        }
        else if (!who.hasBuffWithNameContainingString("dwarfStatue"))
        {
            List<SelectionOption> buffs;
            try
            {
                buffs = Buffs.GetDwarfStatueBuffsAsOptions();
                Singletons.IsDwarfMenuOpen = true;
            }
            catch (Exception e)
            {
                Singletons.Monitor.Log("Failed to get buffs: " + e, LogLevel.Error);
                return true;
            }

            SelectionViewModel context = new SelectionViewModel(I18n.ChosePower(), buffs, s =>
            {
                if (s != "canceled")
                {
                    who.applyBuff(s);
                }

                Singletons.IsDwarfMenuOpen = false;
            });
            IMenuController controller = Singletons.ViewEngine.CreateMenuControllerFromAsset($"Mods/{Singletons.ModManifest.UniqueID}/views/SelectionView", context);
            context.SetController(controller);
            Game1.activeClickableMenu = controller.Menu;
        }
        else
        {
            __instance.shakeTimer = 400;
            Game1.playSound("cancel");
        }

        __result = true;
        return false;
    }

    private static void ApplyBlessing(StardewObject __instance, Farmer who, GameLocation location, string blessing)
    {
        who.applyBuff(blessing);
        who.hasBeenBlessedByStatueToday = true;
        Game1.playSound("statue_of_blessings", null);
        __instance.showNextIndex.Value = true;
        location.critters ??= new List<Critter>();

        location.critters.Add(new Butterfly(
            location,
            __instance.TileLocation + new Vector2(1f, 0f),
            false,
            false,
            163,
            false)
        );
        location.critters.Add(new Butterfly(
            location,
            __instance.TileLocation + new Vector2(0.33f, 0.25f),
            false,
            false,
            163,
            false)
        );
        location.critters.Add(new Butterfly(
            location,
            __instance.TileLocation + new Vector2(1.58f, 0.25f),
            false,
            false,
            163,
            false));
        location.temporarySprites.Add(new TemporaryAnimatedSprite(
            "LooseSprites\\Cursors_1_6",
            new Rectangle(221, 225, 15, 31),
            9000f,
            1,
            1,
            __instance.TileLocation * 64f + new Vector2(1f, -16f) * 4f,
            false,
            false,
            Math.Max(0f, ((__instance.TileLocation.Y + 1f) * 64f - 20f) / 10000f) + __instance.TileLocation.X * 1E-05f,
            0.02f,
            Color.White,
            4f,
            0f,
            0f,
            0f,
            false)
        );
        for (int j = 0; j < 6; j++)
        {
            Utility.makeTemporarySpriteJuicier(new TemporaryAnimatedSprite(
                "LooseSprites\\Cursors_1_6",
                new Rectangle(144, 249, 7, 7),
                (float)Game1.random.Next(100, 200),
                6,
                1,
                __instance.TileLocation * 64f + new Vector2((float)(32 + Game1.random.Next(-64, 64)),
                    (float)Game1.random.Next(-64, 64)),
                false,
                false,
                Math.Max(0f, ((__instance.TileLocation.Y + 1f) * 64f - 24f) / 10000f) + __instance.TileLocation.X * 1E-05f,
                0f,
                (Game1.random.NextDouble() < 0.5) ? new Color(255, 180, 210) : Color.White,
                4f,
                0f,
                0f,
                0f,
                false),
                location,
                4,
                64,
                64);
        }
    }
}