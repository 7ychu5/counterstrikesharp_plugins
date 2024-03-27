using System.Globalization;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Commands;

using System.Numerics;

namespace cashpay;

public class cashpay : BasePlugin
{
    public override string ModuleName => "[CashPay]";
    public override string ModuleVersion => "0.0.1";
    public override string ModuleAuthor => "7ychu5";
    public override string ModuleDescription => "Pay your game cash to the other player";

    public static bool PayToggle = true;

    //public static bool PayLimit = true;
    public override void Load(bool hotReload)
    {
        RegisterEventHandler<EventPlayerPing>((@event, info) =>
        {
            Vector3 origin;
            origin.X = @event.X;
            origin.Y = @event.Y;
            origin.Z = @event.Z;

            double min_distance = 32.0;
            CCSPlayerController victim = null;

            var playerEntities = Utilities.GetPlayers();

            foreach (var player in playerEntities)
            {
                if (player.PlayerPawn.Value is null) continue;
                if (player.PlayerPawn.Value.AbsOrigin is null) continue;
                Vector3 ply_origin;
                ply_origin.X = player.PlayerPawn.Value.AbsOrigin.X;
                ply_origin.Y = player.PlayerPawn.Value.AbsOrigin.Y;
                ply_origin.Z = player.PlayerPawn.Value.AbsOrigin.Z + 40;

                if(Distance3D(ply_origin, origin) <= min_distance){
                    min_distance = Distance3D(ply_origin, origin);
                    victim = player;
                }
                
            }

            var host = @event.Userid;

            if (host.PlayerPawn.Value is null) return HookResult.Continue;
            if (victim is null) return HookResult.Continue;
            if (victim.PlayerPawn.Value is null) return HookResult.Continue;

            if(host.PlayerPawn.Value.TeamNum != victim.PlayerPawn.Value.TeamNum){
                Pay(host,victim,-50);
            }
            else{
                Pay(host,victim,100);
            }

            return HookResult.Continue;
        });
    }

    [ConsoleCommand("css_pay", "Pay Cash to sb.")]
    [CommandHelper(minArgs: 2, usage: "[userid],[cashnum]", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void OnPay(CCSPlayerController? player, CommandInfo commandInfo)
    {
        var _userid = commandInfo.GetArg(1);
        var _cashnum = commandInfo.GetArg(2);
        int userid = Int32.Parse(_userid);
        int cashnum = Int32.Parse(_cashnum);

        var victim = Utilities.GetPlayerFromUserid(userid);

        if (player == null
            ||victim == null
            ||player.InGameMoneyServices == null
            ||victim.InGameMoneyServices == null
            || player.PlayerPawn.Value == null
            || !player.PlayerPawn.Value.IsValid
            || player.PlayerPawn.Value.Health <= 0
            || victim.PlayerPawn.Value == null
            || !victim.PlayerPawn.Value.IsValid
            || victim.PlayerPawn.Value.Health <= 0) return;

        if(cashnum <= 0){
            player.ExecuteClientCommand($"play sounds/ui/armsrace_level_down.vsnd");
            player.PrintToCenter("Illegal Cash number");
            return;
        }

        Pay(player,victim,cashnum);

        return;
    }

    [ConsoleCommand("css_pay_force", "force sb. Pay Cash to sb.")]
    [RequiresPermissions("@css/admin")]
    [CommandHelper(minArgs: 3, usage: "[userid1],[userid2],[cashnum]", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void OnPayForce(CCSPlayerController? player, CommandInfo commandInfo)
    {
        var _userid = commandInfo.GetArg(1);
        var _userid_2 = commandInfo.GetArg(2);
        var _cashnum = commandInfo.GetArg(3);
        int userid = Int32.Parse(_userid);
        int userid_2 = Int32.Parse(_userid_2);
        int cashnum = Int32.Parse(_cashnum);

        player = Utilities.GetPlayerFromUserid(userid);
        var victim = Utilities.GetPlayerFromUserid(userid_2);

        if (player == null
            ||victim == null
            ||player.InGameMoneyServices == null
            ||victim.InGameMoneyServices == null
            || player.PlayerPawn.Value == null
            || !player.PlayerPawn.Value.IsValid
            || player.PlayerPawn.Value.Health <= 0
            || victim.PlayerPawn.Value == null
            || !victim.PlayerPawn.Value.IsValid
            || victim.PlayerPawn.Value.Health <= 0) return;

        if(cashnum <= 0){
            player.ExecuteClientCommand($"play sounds/ui/armsrace_level_down.vsnd");
            player.PrintToCenter("Get More Money First");
            return;
        }

        if(cashnum >= player.InGameMoneyServices.Account) cashnum = player.InGameMoneyServices.Account;

        Pay(player,victim,cashnum);

        return;
    }


    [ConsoleCommand("css_pay_toggle", "Toggle the switch of the plugin")]
    [RequiresPermissions("@css/admin")]
    //[CommandHelper(minArgs: 0, usage: "[toggle]", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void OnPayToggle(CCSPlayerController? player, CommandInfo commandInfo)
    {
        PayToggle = !PayToggle;
        if(PayToggle) Server.PrintToChatAll(ModuleName+"CashPay Plugin TurnON");
        else Server.PrintToChatAll(ModuleName+"CashPay Plugin TurnOff");
        return;
    }

    public void Pay(CCSPlayerController player, CCSPlayerController victim , int cashnum)
    {
        if(!PayToggle){
            player.ExecuteClientCommand($"play sounds/ui/menu_invalid.vsnd");
            player.PrintToCenter("CashPay Plugin had been TurnOff");
            return;
        }
        
        if (victim.InGameMoneyServices is null) return;
        if (player.InGameMoneyServices is null) return;

        if(cashnum > 0 && cashnum >= player.InGameMoneyServices.Account) cashnum = player.InGameMoneyServices.Account;
        if(cashnum < 0 && cashnum*-1 >= victim.InGameMoneyServices.Account) cashnum = (victim.InGameMoneyServices.Account*-1);

        if(cashnum == 0){
            if(victim.InGameMoneyServices.Account == 0) victim.PrintToCenter("You have been bankruptcy !");
            if(player.InGameMoneyServices.Account == 0) player.PrintToCenter("You got no money !");
            return;
        } 

        player.InGameMoneyServices.Account -= cashnum;
        victim.InGameMoneyServices.Account += cashnum;
        player.ExecuteClientCommand($"play sounds/ui/armsrace_level_up_e.vsnd");
        victim.ExecuteClientCommand($"play sounds/ui/armsrace_level_up_e.vsnd");
        if(cashnum > 0){
            player.PrintToCenter("Pay " + victim.PlayerName.ToString() + " $"+ cashnum.ToString());
            victim.PrintToCenter("Receive " + player.PlayerName.ToString() + " $"+ cashnum.ToString());
        }
        else{
            victim.PrintToCenter("stolen by " + player.PlayerName.ToString() + " $"+ (cashnum*-1).ToString());
            player.PrintToCenter("steal from " + victim.PlayerName.ToString() + " $"+ (cashnum*-1).ToString());
        }

        Utilities.SetStateChanged(player, "CCSPlayerController", "m_pInGameMoneyServices");
        Utilities.SetStateChanged(victim, "CCSPlayerController", "m_pInGameMoneyServices");
    }

    private static double Distance3D(Vector3 vec1, Vector3 vec2)
    {
        var a = Math.Pow(vec2.X - vec1.X,2);
        var b = Math.Pow(vec2.Y - vec1.Y,2);
        var c = Math.Pow(vec2.Z - vec1.Z,2);
        return Math.Sqrt(a+b+c);
    }
}
