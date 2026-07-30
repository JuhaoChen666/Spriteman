using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace Spriteman.Smoking;

public sealed class ModEntry : Mod
{
    private const string CigaretteId = "(O)Antigravity.BetelNutCrop_Cigarette";
    private const string CigaretteBuffId = "Antigravity.BetelNutCrop_Cigarette";
    private const int SmokingDurationMilliseconds = 1080;

    private int remainingMilliseconds;
    private int nextSmokePuff;
    private bool isSmoking;

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
        if (!Context.IsPlayerFree || player.CurrentItem?.QualifiedItemId != CigaretteId)
            return;

        this.Helper.Input.Suppress(e.Button);
        this.StartSmoking(player);
    }

    private void StartSmoking(Farmer player)
    {
        this.isSmoking = true;
        this.remainingMilliseconds = SmokingDurationMilliseconds;
        this.nextSmokePuff = 720;

        player.Halt();
        player.faceDirection(Game1.down);
        player.CanMove = false;
        player.FarmerSprite.animateOnce(216, 135f, 8);
        player.reduceActiveItemByOne();
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
        this.isSmoking = false;
        this.remainingMilliseconds = 0;
        this.nextSmokePuff = 0;

        player.health = Math.Max(1, player.health - 5);
        player.Stamina = Math.Min(player.MaxStamina, player.Stamina + 50f);
        player.applyBuff(CigaretteBuffId);
        player.completelyStopAnimatingOrDoingAction();
    }

    private void OnReturnedToTitle(object? sender, ReturnedToTitleEventArgs e)
    {
        this.isSmoking = false;
        this.remainingMilliseconds = 0;
        this.nextSmokePuff = 0;
    }
}
