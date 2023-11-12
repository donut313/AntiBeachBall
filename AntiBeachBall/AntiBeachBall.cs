using TerrariaApi.Server;
using Terraria;
using TShockAPI;
using Microsoft.Xna.Framework;
using TShockAPI.Configuration;
using Newtonsoft.Json;
using TShockAPI.Hooks;

namespace AntiBeachBall;
// set up config manager
public class ConfigManager {
    public int MaxBeachBallMessages = 5;
    public bool Kick = false;
    public bool Disable = true;
    public bool SendMessageToAll = false;
    public bool SendMessagToPlayer = true;
    public void write()
    {
        File.WriteAllText(AntiBeachBall.path,JsonConvert.SerializeObject(this,Formatting.Indented));
    }
    public ConfigManager read() {
        // check if our config does not exist
        if (!File.Exists(AntiBeachBall.path))
        {
            return new ConfigManager();
        }
        return JsonConvert.DeserializeObject<ConfigManager>(File.ReadAllText(AntiBeachBall.path));
    }
}
/// <summary>
/// The main plugin class should always be decorated with an ApiVersion attribute. The current API Version is 2.1
/// </summary>
[ApiVersion(2, 1)]
public class AntiBeachBall : TerrariaPlugin
{
    /// <summary>
    /// The name of the plugin.
    /// </summary>
    public override string Name => "Anti Beach Ball Spam";

    /// <summary>
    /// The version of the plugin in its current state.
    /// </summary>
    public override Version Version => new Version(1, 0, 0);

    /// <summary>
    /// The author(s) of the plugin.
    /// </summary>
    public override string Author => "donut313";

    /// <summary>
    /// A short, one-line, description of the plugin's purpose.
    /// </summary>
    public override string Description => "A simple tshock plugin that checks if beach balls where sent a lot and if so deletes all active beach balls.";
    public static string path = Path.Combine(TShock.SavePath, "AntiBeachBall.json");
    public static ConfigManager config = new ConfigManager();
    /// <summary>
    /// The plugin's constructor
    /// Set your plugin's order (optional) and any other constructor logic here
    /// </summary>
    ///
    public AntiBeachBall(Main game) : base(game)
    {
    }
    public int beachballmessagessent = 0;
    // hook into the chat to see how many beach ball messages have been sent
    void OnServerChat(ServerBroadcastEventArgs args)
    {
        // check if the color of the message is yellow and if it's a beach ball message
        if (args.Color == new Color(255, 240, 20, 255) && args.Message._text == "Game.BallBounceResult")
        {
            // add 1 to the beach ball message counter
            beachballmessagessent++;
            // check if more then max amount of beach ball messages have been sent
            if (beachballmessagessent >= config.MaxBeachBallMessages)
            {
            TSPlayer player = null;
                // reset beach balls sent count
                beachballmessagessent = 0;
                // kill all beach balls
            // go through all projectiles
            for (int n = 0; n < 1000; n++)
            {
                // check if it's a beach ball
                if (Terraria.Main.projectile[n].type == 155 && Main.projectile[n].active)
                {
             player = TShockAPI.TSPlayer.FindByNameOrID(Terraria.Main.projectile[n].owner.ToString())[0];
                    // kill tthe beach balls or kick player
                    if (!config.Kick)
                {
                        // kill their beach balls
                        Main.projectile[n].Kill();
                        TShockAPI.TSPlayer.All.RemoveProjectile(n,Terraria.Main.projectile[n].owner);
                        // disable them
                        if (config.Disable && player.Active)
                        {
                        player.Disable("Beach Ball Spam");
                        }
                    }
                    else {
                        // kick the player that spammed the beach balls
                        if (player.Active)
                        {
                    player.Kick("Beach Ball Spam",false,false,null,false);
                        }
                        else {
                            // kill the beach balls from that player becuase they are logging out but they stll appear
                            Main.projectile[n].Kill();
                            TShockAPI.TSPlayer.All.RemoveProjectile(n,Terraria.Main.projectile[n].owner);
                        }
                    return;
                    }
                }
            }
            // send message
            if (config.SendMessageToAll)
            {
            TShockAPI.TSPlayer.All.SendInfoMessage("Killing all beach balls.");
            }
            else if (config.SendMessagToPlayer && player != null) {
                player.SendWarningMessage("Your not allowed to use beach balls to spam the chat.");
            }
        }
        }
        else {
            // if this message is not a beach ball message subtract 1 from the beach ball counter
            if (beachballmessagessent != 0)
            {
                beachballmessagessent--;
            }
        }
    }
    /// <summary>
    /// Performs plugin initialization logic.
    /// Add your hooks, config file read/writes, etc here
    /// </summary>
    public override void Initialize()
    {
        // set up server chat hook
        ServerApi.Hooks.ServerBroadcast.Register(this,OnServerChat);

        // set up config
        GeneralHooks.ReloadEvent += OnReload;
        if (File.Exists(path))
            config = config.read();
        else
            config.write();
    }
    private void OnReload(ReloadEventArgs args)
    {
        if (File.Exists(path))
             config = config.read();
        else
            config.write();
    }
    /// <summary>
    /// Performs plugin cleanup logic
    /// Remove your hooks and perform general cleanup here
    /// </summary>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            //unhook
            //dispose child objects
            //set large objects to null
            ServerApi.Hooks.ServerBroadcast.Deregister(this, OnServerChat);
        }
        base.Dispose(disposing);
    }
}
