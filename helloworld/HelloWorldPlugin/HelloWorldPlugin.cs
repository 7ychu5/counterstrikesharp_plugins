using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Timers;
using Microsoft.Extensions.Logging;
using System.Drawing;
using System.Numerics;

using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace HelloWorldPlugin;

public class HelloWorldPlugin : BasePlugin
{
    public override string ModuleName => "Hello World Plugin";

    public override string ModuleVersion => "0.0.1";

    public Timer? timer;

    public int times;

    public override void Load(bool hotReload)
    {
        //var server = ValveInterface.Server;

        RegisterListener<Listeners.OnEntitySpawned>(entity =>
        {
            if (entity.DesignerName != "smokegrenade_projectile") return;

            var projectile = new CSmokeGrenadeProjectile(entity.Handle);

            // Changes smoke grenade colour to a random colour each time.
            Server.NextFrame(() =>
            {
                projectile.SmokeColor.X = Random.Shared.NextSingle() * 255.0f;
                projectile.SmokeColor.Y = Random.Shared.NextSingle() * 255.0f;
                projectile.SmokeColor.Z = Random.Shared.NextSingle() * 255.0f;
                Logger.LogInformation("Smoke grenade spawned with color {SmokeColor}", projectile.SmokeColor);
            });
        });

        RegisterListener<Listeners.OnMapStart>(name =>
        {
            times = 0;

            //var timer = AddTimer(0.1f, () =>{times++;Server.PrintToConsole(times.ToString());}, TimerFlags.REPEAT);
        });

        RegisterEventHandler<EventRoundPrestart>((@event, info) =>
        {
            Server.PrecacheModel("models/chicken/chicken.vmdl");
            return HookResult.Continue;
        });

        RegisterEventHandler<EventBulletImpact>((@event, info) =>
        {
            Vector3 origin;
            origin.X = @event.X;
            origin.Y = @event.Y;
            origin.Z = @event.Z;

            Logger.LogInformation(origin.ToString());




            return HookResult.Continue;
        });

        RegisterEventHandler<EventDecoyStarted>((@event, info) =>
        {
            Vector3 origin;
            origin.X = @event.X;
            origin.Y = @event.Y;
            origin.Z = @event.Z;


            var playerEntities = Utilities.GetPlayers();

            foreach (var player in playerEntities)
            {
                Vector3 ply_origin;
                ply_origin.X = player.PlayerPawn.Value.AbsOrigin.X;
                ply_origin.Y = player.PlayerPawn.Value.AbsOrigin.Y;
                ply_origin.Z = player.PlayerPawn.Value.AbsOrigin.Z;

                if(Distance3D(ply_origin, origin) <= 384.00)
                {
                    Vector3 reletive = origin - ply_origin;
                    player.PlayerPawn.Value.AbsVelocity.X += reletive.X*4;
                    player.PlayerPawn.Value.AbsVelocity.Y += reletive.Y*4;
                    player.PlayerPawn.Value.AbsVelocity.Z += reletive.Z*4;
                    sucking(0, 50, origin);
                }
            }

            return HookResult.Continue;
        });

        void sucking(int time_tick, int count ,Vector3 origin)
        {
            if(time_tick!=times){
                time_tick=times;
                var playerEntities = Utilities.GetPlayers();

                foreach (var player in playerEntities)
                {
                    Vector3 ply_origin;
                    ply_origin.X = player.PlayerPawn.Value.AbsOrigin.X;
                    ply_origin.Y = player.PlayerPawn.Value.AbsOrigin.Y;
                    ply_origin.Z = player.PlayerPawn.Value.AbsOrigin.Z;

                    if(Distance3D(ply_origin, origin) <= 384.00)
                    {
                        Vector3 reletive = origin - ply_origin;
                        player.PlayerPawn.Value.AbsVelocity.X += reletive.X*2;
                        player.PlayerPawn.Value.AbsVelocity.Y += reletive.Y*2;
                        player.PlayerPawn.Value.AbsVelocity.Z += reletive.Z*2;
                    }
                }
                Server.PrintToConsole(time_tick.ToString());
                Server.PrintToConsole(times.ToString());
                Server.PrintToConsole("////////////////////////");
                count--;
            }

            if(count >= 0){sucking(time_tick, count, origin);}
        }



        RegisterEventHandler<EventPlayerHurt>((@event, info) =>
        {
            var player = @event.Userid;
            var attacker = @event.Attacker;
            var dmg_hp = @event.DmgHealth;
            var remain_hp = @event.Health;
            if (attacker is null || player is null || attacker == player)
            {
                Server.PrintToConsole(attacker.ToString());
                Server.PrintToConsole(player.ToString());
                Server.PrintToConsole("Some Error happen,r u want to suicide?");
                Server.PrintToConsole("//////////////////////////////////////");
                return HookResult.Continue;
            }

            var ply_pawn = attacker.PlayerPawn.Value;
            var activeWeapon = ply_pawn.WeaponServices?.ActiveWeapon.Value;
            activeWeapon.Clip1 = 250;


            Server.NextFrame(() =>
            {
                player.PlayerPawn.Value.Render = Color.FromArgb(Random.Shared.Next(0, 255),
                Random.Shared.Next(0, 255), Random.Shared.Next(0, 255));
                //player.PlayerPawn.Value.SetModel("models/chicken/chicken.vmdl");
            });

            //var hm = Utilities.CreateEntityByName<CEnvScreenOverlay>("env_screenoverlay");
            //hm.Remove();

            ply_pawn.Health += dmg_hp;
            @event.Attacker.PrintToCenter("伤害" + dmg_hp + "生命值，目标仍余" + remain_hp + "生命值");

            return HookResult.Continue;


        });


    }

    private static double Distance3D(Vector3 vec1, Vector3 vec2)
    {
        var a = Math.Pow(vec2.X - vec1.X,2);
        var b = Math.Pow(vec2.Y - vec1.Y,2);
        var c = Math.Pow(vec2.Z - vec1.Z,2);
        return Math.Sqrt(a+b+c);
    }
    private static double Distance2D(Vector3 vec1, Vector3 vec2)
    {
        var a = Math.Pow(vec2.X - vec1.X,2);
        var b = Math.Pow(vec2.Y - vec1.Y,2);
        return Math.Sqrt(a+b);
    }
}