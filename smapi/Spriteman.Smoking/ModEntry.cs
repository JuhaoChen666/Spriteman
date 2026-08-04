using System.Globalization;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace Spriteman.Smoking;

public sealed class ModEntry : Mod
{
    private const string CigaretteId = "(O)Antigravity.BetelNutCrop_Cigarette";
    private const string VapeId = "(O)Antigravity.BetelNutCrop_ECigarette";
    private const string VapeJuicePodId = "(O)Antigravity.BetelNutCrop_VapeJuicePod";
    private const string AppleVapeJuicePodId = "(O)Antigravity.BetelNutCrop_AppleVapeJuicePod";
    private const string BlueberryVapeJuicePodId = "(O)Antigravity.BetelNutCrop_BlueberryVapeJuicePod";
    private const string MelonVapeJuicePodId = "(O)Antigravity.BetelNutCrop_MelonVapeJuicePod";
    private const string StrawberryVapeJuicePodId = "(O)Antigravity.BetelNutCrop_StrawberryVapeJuicePod";
    private const string CigaretteBuffId = "Antigravity.BetelNutCrop_Cigarette";
    private const string VapeBuffId = "Antigravity.BetelNutCrop_ECigarette";
    private const string AppleVapeBuffId = "Antigravity.BetelNutCrop_AppleVape";
    private const string BlueberryVapeBuffId = "Antigravity.BetelNutCrop_BlueberryVape";
    private const string MelonVapeBuffId = "Antigravity.BetelNutCrop_MelonVape";
    private const string StrawberryVapeBuffId = "Antigravity.BetelNutCrop_StrawberryVape";
    private const string VapeUsesKey = "Antigravity.BetelNutCrop/VapeUses";
    private const string VapeBuffKey = "Antigravity.BetelNutCrop/VapeBuff";
    private const string HarmonyId = "Antigravity.SpritemanSmoking";
    private const int VapeUsesPerPod = 20;
    private const int SmokingDurationMilliseconds = 1080;
    private const int CigaretteHealthCost = 5;
    private const int VapeHealthCost = 2;

    private static readonly Dictionary<string, string> VapePodBuffs = new(StringComparer.Ordinal)
    {
        [VapeJuicePodId] = VapeBuffId,
        [AppleVapeJuicePodId] = AppleVapeBuffId,
        [BlueberryVapeJuicePodId] = BlueberryVapeBuffId,
        [MelonVapeJuicePodId] = MelonVapeBuffId,
        [StrawberryVapeJuicePodId] = StrawberryVapeBuffId
    };

    private static readonly string[] VapeBuffIds = VapePodBuffs.Values.ToArray();

    private int remainingMilliseconds;
    private int nextSmokePuff;
    private bool isSmoking;
    private SmokingMode activeMode;
    private Item? activeVape;
    private string activeVapeBuffId = VapeBuffId;

    public override void Entry(IModHelper helper)
    {
        var harmony = new Harmony(HarmonyId);
        harmony.Patch(
            AccessTools.Method(typeof(StardewValley.Object), nameof(StardewValley.Object.maximumStackSize)),
            postfix: new HarmonyMethod(typeof(ModEntry), nameof(AfterGetMaximumStackSize))
        );

        helper.Events.Input.ButtonPressed += this.OnButtonPressed;
        helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
        helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
        helper.Events.GameLoop.ReturnedToTitle += this.OnReturnedToTitle;
        helper.Events.Player.InventoryChanged += this.OnInventoryChanged;
    }

    private static void AfterGetMaximumStackSize(StardewValley.Object __instance, ref int __result)
    {
        if (__instance.QualifiedItemId == VapeId)
            __result = 1;
    }

    private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
    {
        this.SplitStackedVapes(Game1.player);
    }

    private void OnInventoryChanged(object? sender, InventoryChangedEventArgs e)
    {
        if (e.IsLocalPlayer)
            this.SplitStackedVapes(e.Player);
    }

    private void SplitStackedVapes(Farmer player)
    {
        var emptySlots = new Queue<int>(
            player.Items
                .Select((item, index) => new { item, index })
                .Where(entry => entry.item is null)
                .Select(entry => entry.index)
        );

        for (int index = 0; index < player.Items.Count && emptySlots.Count > 0; index++)
        {
            Item? vape = player.Items[index];
            if (vape?.QualifiedItemId != VapeId || vape.Stack <= 1)
                continue;

            int splitCount = Math.Min(vape.Stack - 1, emptySlots.Count);
            vape.Stack -= splitCount;
            for (int copyIndex = 0; copyIndex < splitCount; copyIndex++)
            {
                Item copy = vape.getOne();
                copy.Stack = 1;
                player.Items[emptySlots.Dequeue()] = copy;
            }
        }
    }

    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (!Context.IsWorldReady || this.isSmoking || !e.Button.IsActionButton())
            return;

        Farmer player = Game1.player;
        if (!Context.IsPlayerFree || player.CurrentItem is null)
            return;

        Item currentItem = player.CurrentItem;
        if (currentItem.QualifiedItemId == VapeId)
        {
            this.Helper.Input.Suppress(e.Button);

            if (this.GetVapeUses(currentItem) <= 0)
            {
                this.TryLoadVape(player, currentItem);
                return;
            }

            this.StartSmoking(player, SmokingMode.Vape, currentItem);
            return;
        }

        if (currentItem.QualifiedItemId != CigaretteId)
            return;

        this.Helper.Input.Suppress(e.Button);
        this.StartSmoking(player, SmokingMode.Cigarette, null);
    }

    private void TryLoadVape(Farmer player, Item vape)
    {
        for (int index = 0; index < player.Items.Count; index++)
        {
            Item? item = player.Items[index];
            if (item is null || !VapePodBuffs.TryGetValue(item.QualifiedItemId, out string? buffId))
                continue;

            string podName = item.DisplayName;
            if (item.Stack > 1)
                item.Stack--;
            else
                player.Items[index] = null;

            vape.modData[VapeUsesKey] = VapeUsesPerPod.ToString(CultureInfo.InvariantCulture);
            vape.modData[VapeBuffKey] = buffId;
            this.ShowMessage("vape.loaded", HUDMessage.newQuest_type, new { pod = podName, uses = VapeUsesPerPod });
            Game1.playSound("openChest");
            return;
        }

        this.ShowMessage("vape.empty", HUDMessage.error_type);
    }

    private int GetVapeUses(Item vape)
    {
        if (!vape.modData.TryGetValue(VapeUsesKey, out string? rawUses))
            return 0;

        return int.TryParse(rawUses, NumberStyles.Integer, CultureInfo.InvariantCulture, out int uses)
            ? Math.Clamp(uses, 0, VapeUsesPerPod)
            : 0;
    }

    private string GetVapeBuffId(Item vape)
    {
        if (vape.modData.TryGetValue(VapeBuffKey, out string? buffId)
            && VapeBuffIds.Contains(buffId, StringComparer.Ordinal))
        {
            return buffId;
        }

        return VapeBuffId;
    }

    private void ConsumeVapeUse(Item vape)
    {
        int remainingUses = this.GetVapeUses(vape) - 1;
        if (remainingUses > 0)
            vape.modData[VapeUsesKey] = remainingUses.ToString(CultureInfo.InvariantCulture);
        else
        {
            vape.modData.Remove(VapeUsesKey);
            vape.modData.Remove(VapeBuffKey);
        }
    }

    private void StartSmoking(Farmer player, SmokingMode mode, Item? vape)
    {
        this.isSmoking = true;
        this.activeMode = mode;
        this.activeVape = vape;
        this.remainingMilliseconds = SmokingDurationMilliseconds;
        this.nextSmokePuff = 720;

        player.Halt();
        player.faceDirection(Game1.down);
        player.CanMove = false;
        player.FarmerSprite.animateOnce(216, 135f, 8);
        if (mode == SmokingMode.Cigarette)
            player.reduceActiveItemByOne();
        else
        {
            this.activeVapeBuffId = this.GetVapeBuffId(vape!);
            this.ConsumeVapeUse(vape!);
        }
        Game1.playSound("furnace");
    }

    private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
    {
        if (!this.isSmoking || !Context.IsWorldReady)
            return;

        int elapsed = (int)Game1.currentGameTime.ElapsedGameTime.TotalMilliseconds;
        this.remainingMilliseconds -= elapsed;

        if (this.nextSmokePuff > 0 && this.remainingMilliseconds <= this.nextSmokePuff)
        {
            this.SpawnSmokePuff(Game1.player, 720 - this.nextSmokePuff);
            this.nextSmokePuff -= 240;
        }

        if (this.remainingMilliseconds > 0)
            return;

        this.FinishSmoking(Game1.player);
    }

    private void SpawnSmokePuff(Farmer player, int horizontalOffset)
    {
        if (player.currentLocation is null)
            return;

        var smoke = new TemporaryAnimatedSprite(
            10,
            player.Position + new Vector2(28f + horizontalOffset / 24f, -72f),
            Color.White
        )
        {
            motion = new Vector2(0.18f, -0.45f),
            acceleration = new Vector2(0.002f, -0.004f),
            alphaFade = 0.012f,
            scale = 1.5f,
            scaleChange = 0.012f,
            layerDepth = (player.StandingPixel.Y + 1) / 10000f
        };

        player.currentLocation.temporarySprites.Add(smoke);
    }

    private void FinishSmoking(Farmer player)
    {
        bool wasVaping = this.activeMode == SmokingMode.Vape;
        this.isSmoking = false;
        this.remainingMilliseconds = 0;
        this.nextSmokePuff = 0;

        player.health = Math.Max(1, player.health - (wasVaping ? VapeHealthCost : CigaretteHealthCost));
        player.Stamina = Math.Min(player.MaxStamina, player.Stamina + 50f);
        if (wasVaping)
        {
            foreach (string buffId in VapeBuffIds)
                player.buffs.Remove(buffId);

            player.applyBuff(this.activeVapeBuffId);
        }
        else
        {
            player.applyBuff(CigaretteBuffId);
        }
        player.completelyStopAnimatingOrDoingAction();
        player.CanMove = true;

        if (wasVaping && this.activeVape is not null)
        {
            int uses = this.GetVapeUses(this.activeVape);
            this.ShowMessage(uses > 0 ? "vape.uses_remaining" : "vape.empty_after_use", HUDMessage.newQuest_type, new { uses });
        }

        this.activeMode = SmokingMode.None;
        this.activeVape = null;
        this.activeVapeBuffId = VapeBuffId;
    }

    private void ShowMessage(string key, int type, object? tokens = null)
    {
        string message = this.Helper.Translation.Get(key, tokens);
        Game1.addHUDMessage(new HUDMessage(message, type));
    }

    private void OnReturnedToTitle(object? sender, ReturnedToTitleEventArgs e)
    {
        this.isSmoking = false;
        this.remainingMilliseconds = 0;
        this.nextSmokePuff = 0;
        this.activeMode = SmokingMode.None;
        this.activeVape = null;
        this.activeVapeBuffId = VapeBuffId;
    }

    private enum SmokingMode
    {
        None,
        Cigarette,
        Vape
    }
}
