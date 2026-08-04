using System.Globalization;
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
    private const string CigaretteBuffId = "Antigravity.BetelNutCrop_Cigarette";
    private const string VapeBuffId = "Antigravity.BetelNutCrop_ECigarette";
    private const string VapeUsesKey = "Antigravity.BetelNutCrop/VapeUses";
    private const int VapeUsesPerPod = 20;
    private const int SmokingDurationMilliseconds = 1080;
    private const int CigaretteHealthCost = 5;
    private const int VapeHealthCost = 2;

    private int remainingMilliseconds;
    private int nextSmokePuff;
    private bool isSmoking;
    private SmokingMode activeMode;
    private Item? activeVape;

    public override void Entry(IModHelper helper)
    {
        helper.Events.Input.ButtonPressed += this.OnButtonPressed;
        helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
        helper.Events.GameLoop.ReturnedToTitle += this.OnReturnedToTitle;
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
            if (item?.QualifiedItemId != VapeJuicePodId)
                continue;

            if (item.Stack > 1)
                item.Stack--;
            else
                player.Items[index] = null;

            vape.modData[VapeUsesKey] = VapeUsesPerPod.ToString(CultureInfo.InvariantCulture);
            this.ShowMessage("vape.loaded", HUDMessage.newQuest_type, new { uses = VapeUsesPerPod });
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

    private void ConsumeVapeUse(Item vape)
    {
        int remainingUses = this.GetVapeUses(vape) - 1;
        if (remainingUses > 0)
            vape.modData[VapeUsesKey] = remainingUses.ToString(CultureInfo.InvariantCulture);
        else
            vape.modData.Remove(VapeUsesKey);
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
            this.ConsumeVapeUse(vape!);
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
        player.applyBuff(wasVaping ? VapeBuffId : CigaretteBuffId);
        player.completelyStopAnimatingOrDoingAction();
        player.CanMove = true;

        if (wasVaping && this.activeVape is not null)
        {
            int uses = this.GetVapeUses(this.activeVape);
            this.ShowMessage(uses > 0 ? "vape.uses_remaining" : "vape.empty_after_use", HUDMessage.newQuest_type, new { uses });
        }

        this.activeMode = SmokingMode.None;
        this.activeVape = null;
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
    }

    private enum SmokingMode
    {
        None,
        Cigarette,
        Vape
    }
}
