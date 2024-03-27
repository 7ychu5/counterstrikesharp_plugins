using System;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using System.Text.Json.Serialization;

namespace se11;

/*

出售枪械指令的开关
出售血量指令的开关
限制能够出售的武器
要能够调整武器价格倍率（相对于原价格）
随机价格出售(随机价格倍率)

*/

public class weapon_price : BasePluginConfig
{
    public bool SellhpToggle = true;
    public bool SellgunToggle = true;
    public int hp_price_times = 10;
    public double gun_price_times = 0.4;
    public int weapon_m249 = 5200;
    public int weapon_negev = 1700;
    public int weapon_ak47 = 2700;
    public int weapon_aug = 3300;
    public int weapon_famas = 2050;
    public int weapon_galilar = 1800;
    public int weapon_m4a1 = 3100;
    public int weapon_m4a1_silencer = 2900;
    public int weapon_sg556 = 3000;
    public int weapon_mag7 = 1300;
    public int weapon_nova = 1050;
    public int weapon_sawedoff = 1100;
    public int weapon_xm1014 = 2000;
    public int weapon_bizon = 1400;
    public int weapon_mac10 = 1050;
    public int weapon_mp5sd = 1500;
    public int weapon_mp7 = 1500;
    public int weapon_mp9 = 1250;
    public int weapon_p90 = 2350;
    public int weapon_ump45 = 1200;
    public int weapon_awp = 4750;
    public int weapon_g3sg1 = 5000;
    public int weapon_scar20 = 5000;
    public int weapon_ssg08 = 1700;
    public int weapon_usp_silencer = 200;
    public int weapon_cz75a = 500;
    public int weapon_deagle = 700;
    public int weapon_elite = 300;
    public int weapon_fiveseven = 500;
    public int weapon_glock = 200;
    public int weapon_hkp2000 = 200;
    public int weapon_p250 = 300;
    public int weapon_revolver = 600;
    public int weapon_tec9 = 500;

}
public class se11Plugin : BasePlugin, IPluginConfig<weapon_price>
{
    public override string ModuleName => "Se11 Everything";
    public override string ModuleVersion => "0.0.1";
    public override string ModuleAuthor => "7ychu5";
    public override string ModuleDescription => "Sell Gun which on your hand & Sell HP to Cash";

    public weapon_price Config { get; set; }

    public void OnConfigParsed(weapon_price config){Config = config;}

    public override void Load(bool hotReload)
    {
        AddCommand("css_sellgun", "Sell Gun which on your hand", (player, commandInfo) =>
        {
            if(!Config.SellgunToggle){
                player.ExecuteClientCommand($"play sounds/ui/menu_invalid.vsnd");
                player.PrintToCenter("插件已关闭，请联系管理员开启");
                return;
            }
            if (player == null
            ||player.PlayerPawn == null
            || !player.PawnIsAlive
            || player.PlayerPawn.Value == null
            || !player.PlayerPawn.Value.IsValid
            || player.PlayerPawn.Value.WeaponServices == null
            || player.PlayerPawn.Value.WeaponServices.MyWeapons == null
            || !player.PlayerPawn.Value.WeaponServices.ActiveWeapon.IsValid) return;

            string weapon = player.PlayerPawn.Value.WeaponServices.ActiveWeapon.Value.DesignerName;
            if(weapon == "weapon_knife"
            ||weapon =="weapon_axe"
            ||weapon =="weapon_knife"
            ||weapon =="weapon_breachcharge"
            ||weapon =="weapon_c4"
            ||weapon =="weapon_decoy"
            ||weapon =="weapon_diversion"
            ||weapon =="weapon_flashbang"
            ||weapon =="weapon_healthshot"
            ||weapon =="weapon_hegrenade"
            ||weapon =="weapon_incgrenade"
            ||weapon =="weapon_hammer"
            ||weapon =="weapon_knifegg"
            ||weapon =="weapon_molotov"
            ||weapon =="weapon_smokegrenade"
            ||weapon =="weapon_snowball"
            ||weapon =="weapon_spanner"
            ||weapon =="weapon_tagrenade"
            ||weapon =="weapon_taser"
            ||weapon =="weapon_bumpmine"){
                player.PrintToCenter("朋友，这个不能卖");
                player.ExecuteClientCommand($"play sounds/ui/armsrace_level_down.vsnd");
                return;
            }

            if(weapon == "" || weapon == null){
                player.PrintToCenter("朋友，想要空手套白狼？");
                player.ExecuteClientCommand($"play sounds/ui/armsrace_level_down.vsnd");
                return;
            }

            double price = Int32.Parse(Config.GetType().GetField(weapon).GetValue(Config).ToString());

            price *= Config.gun_price_times;

            player.InGameMoneyServices.Account += (int)price;

            //player.PlayerPawn.Value.WeaponServices.ActiveWeapon.Value.Remove();

            CCSPlayer_ItemServices itemServices = new CCSPlayer_ItemServices(player.PlayerPawn.Value.ItemServices.Handle);
            CCSPlayer_WeaponServices weaponServices = new CCSPlayer_WeaponServices(player.PlayerPawn.Value.WeaponServices.Handle);
            
            CBaseEntity temp = weaponServices.ActiveWeapon.Value;
            itemServices.DropActivePlayerWeapon(weaponServices.ActiveWeapon.Value);

            Server.NextFrame(() =>{temp.Remove();});

            player.ExecuteClientCommand($"play sounds/ui/armsrace_level_up_e.vsnd");

            player.PrintToCenter("已出售" + weapon + " , 你赚了$"+ price.ToString() +"，欢迎下次光临！");
            Utilities.SetStateChanged(player, "CCSPlayerController", "m_pInGameMoneyServices");

        });
    }

    [ConsoleCommand("css_sellgun_toggle", "Toggle the usability of this plugin")]
    [RequiresPermissions("@css/admin")]
    [CommandHelper(minArgs: 0, whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void OnSellgunToggle(CCSPlayerController? player, CommandInfo commandInfo)
    {
        Config.SellgunToggle = !Config.SellgunToggle;
        if(Config.SellgunToggle) Server.PrintToChatAll("卖枪插件已开启");
        else Server.PrintToChatAll("卖枪插件已关闭");
    }

    [ConsoleCommand("css_sellgun_times", "Set the ratio of Gun for money")]
    [RequiresPermissions("@css/admin")]
    [CommandHelper(minArgs: 1,usage: "[times]", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void OnSellgunRatio(CCSPlayerController? player, CommandInfo commandInfo)
    {
        commandInfo.GetArg(0);
        var times = Int32.Parse(commandInfo.GetArg(1));
        Server.PrintToChatAll("枪支价格比值已调整为"+times.ToString());
        Config.gun_price_times = times;
    }

    [ConsoleCommand("css_sellhp", "Sell HP for Cash")]
    [CommandHelper(minArgs: 1, usage: "[hp]", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void OnSellhp(CCSPlayerController? player, CommandInfo commandInfo)
    {
        if(!Config.SellhpToggle){
            player.ExecuteClientCommand($"play sounds/ui/menu_invalid.vsnd");
            player.PrintToCenter("插件已关闭，请联系管理员开启");
            return;
        }
        if (player == null
        ||player.PlayerPawn == null
        || !player.PawnIsAlive
        || player.PlayerPawn.Value == null
        || !player.PlayerPawn.Value.IsValid
        || player.PlayerPawn.Value.WeaponServices == null
        || player.PlayerPawn.Value.WeaponServices.MyWeapons == null
        || !player.PlayerPawn.Value.WeaponServices.ActiveWeapon.IsValid) return;

        commandInfo.GetArg(0);
        var minus_hp = Int32.Parse(commandInfo.GetArg(1));
        var hp = player.Pawn.Value.Health;
        if(minus_hp <=0){
            player.ExecuteClientCommand($"play sounds/ui/armsrace_level_down.vsnd");
            player.PrintToCenter("朋友，想从我这儿骗血？");
            return;
        }
        else if(minus_hp >= hp){
            player.ExecuteClientCommand($"play sounds/ui/armsrace_level_down.vsnd");
            player.PrintToCenter("朋友，血量不太够");
            return;
        }
        else{
            player.Pawn.Value.Health -= minus_hp;
            int earn = minus_hp * Config.hp_price_times;
            player.InGameMoneyServices.Account += earn;
            player.ExecuteClientCommand($"play sounds/ui/armsrace_level_up_e.vsnd");
            player.PrintToCenter("已出售" + minus_hp + "血量 , 你赚了$"+ earn +"，欢迎下次光临！");
            Utilities.SetStateChanged(player.Pawn.Value, "CBaseEntity", "m_iHealth");
            Utilities.SetStateChanged(player, "CCSPlayerController", "m_pInGameMoneyServices");
        }
    }

    [ConsoleCommand("css_sellhp_times", "Set the ratio of HP for money")]
    [CommandHelper(minArgs: 1, usage: "[times]", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    [RequiresPermissions("@css/admin")]
    public void OnSellhpRatio(CCSPlayerController? player, CommandInfo commandInfo)
    {
        commandInfo.GetArg(0);
        var times = Int32.Parse(commandInfo.GetArg(1));
        Server.PrintToChatAll("血钱比值已调整为"+times.ToString());
        Config.hp_price_times = times;
    }

    [ConsoleCommand("css_sellhp_toggle", "Toggle the usability of this plugin")]
    [RequiresPermissions("@css/admin")]
    [CommandHelper(minArgs: 0, whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void OnSellhpToggle(CCSPlayerController? player, CommandInfo commandInfo)
    {
        Config.SellhpToggle = !Config.SellhpToggle;
        if(Config.SellhpToggle) Server.PrintToChatAll("卖血插件已开启");
        else Server.PrintToChatAll("卖血插件已关闭");
    }
    
}