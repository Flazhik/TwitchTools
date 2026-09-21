using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TwitchTools.Api.Rewards;
using TwitchTools.Api.Settings;

namespace TwitchTools.Rewards;

public class ChangeLoadoutReward : Reward
{
    public override string Id => "dev.flazhik.twitch-tools.rewards.change-loadout";
    public override string Name => "Change loadout";
    public override string Description => "Forcefully changes loadout";

    private static readonly Dictionary<Weapon, (HashSet<string> spellings, string key)> WeaponsSettings = new()
    {
        {
            Weapon.Revolver, (["rev", "revolver"], "rev")
        },
        {
            Weapon.Shotgun, (["s", "shotgun", "shot", "sho"], "sho")
        },
        {
            Weapon.Nailgun, (["n", "nail", "nails", "nailgun", "nai"], "nai")
        },
        {
            Weapon.Railgun, (["rail", "rails", "railcannon", "railgun", "rai"], "rai")
        },
        {
            Weapon.RocketLauncher, (["rocket", "rockets", "rock", "rocketlauncher", "rl"], "rock")
        },
    };

    private static readonly Dictionary<Arm, HashSet<string>> ArmsSettings = new()
    {
        {
            Arm.Feedbacker, ["feedbacker", "fb", "bluearm"]
        },
        {
            Arm.Knuckleblaster, ["knuckleblaster", "kb", "redarm"]
        },
        {
            Arm.Whiplash, ["whiplash", "wl", "hook", "greenarm"]
        },
    };

    private RewardSetting<LoadoutPolicy> _revolverSetting;
    private RewardSetting<LoadoutPolicy> _shotgunSetting;
    private RewardSetting<LoadoutPolicy> _nailgunSetting;
    private RewardSetting<NoAltersLoadoutPolicy> _railcannonSetting;
    private RewardSetting<NoAltersLoadoutPolicy> _rocketLauncherSetting;
    private RewardSetting<NoAltersLoadoutPolicy> _feedbackerSetting;
    private RewardSetting<NoAltersLoadoutPolicy> _knuckleblasterSetting;
    private RewardSetting<NoAltersLoadoutPolicy> _whiplashSetting;

    private Dictionary<string, int> _originalLoadout;
        
    public override void RegisterSettings()
    {
        _revolverSetting = CreateSetting("revolverPolicy", "Revolver", "Revolver policy", LoadoutPolicy.AllowAll);
        _shotgunSetting = CreateSetting("shotgunPolicy", "Shotgun", "Shotgun policy", LoadoutPolicy.AllowAll);
        _nailgunSetting = CreateSetting("nailgunPolicy", "Nailgun", "Nailgun policy", LoadoutPolicy.AllowAll);
        _railcannonSetting = CreateSetting("railgunPolicy", "Railcannon", "Railcannon policy", NoAltersLoadoutPolicy.AllowToUnequip);
        _rocketLauncherSetting = CreateSetting("rocketPolicy", "Rocket launcher", "Rocket launcher policy", NoAltersLoadoutPolicy.AllowToUnequip);
        _feedbackerSetting = CreateSetting("feedbackerPolicy", "Feedbacker", "Feedbacker policy", NoAltersLoadoutPolicy.AllowToUnequip);
        _knuckleblasterSetting = CreateSetting("knuckleblasterPolicy", "Knuckleblaster", "Knuckleblaster policy", NoAltersLoadoutPolicy.AllowToUnequip);
        _whiplashSetting = CreateSetting("whiplashPolicy", "Whiplash", "Whiplash policy", NoAltersLoadoutPolicy.AllowToUnequip);
    }

    protected override void Execute(string prompt)
    {
        var loadoutPolicies = new Dictionary<Weapon, RewardSetting<LoadoutPolicy>>
        {
            { Weapon.Revolver, _revolverSetting },
            { Weapon.Shotgun, _shotgunSetting },
            { Weapon.Nailgun, _nailgunSetting }
        };
            
        var noAltersLoadoutPolicies = new Dictionary<Weapon, RewardSetting<NoAltersLoadoutPolicy>>
        {
            { Weapon.Railgun, _railcannonSetting },
            { Weapon.RocketLauncher, _rocketLauncherSetting },
        };
            
        var armsPolicies = new Dictionary<Arm, RewardSetting<NoAltersLoadoutPolicy>>
        {
            { Arm.Feedbacker, _feedbackerSetting },
            { Arm.Knuckleblaster, _knuckleblasterSetting },
            { Arm.Whiplash, _whiplashSetting }
        };
            
        var prefsManager = PrefsManager.Instance!;
        _originalLoadout = new Dictionary<string, int>();
            
        // Saving original values
        foreach (var weapon in WeaponsSettings)
        {
            for (var i = 0; i < 3; i++)
            {
                var key = $"weapon.{weapon.Value.key}{i}";
                _originalLoadout.Add(key, prefsManager.GetInt(key, 1));
            }
        }

        for (var i = 0; i < 3; i++)
        {
            var key = $"weapon.arm{i}";
            _originalLoadout.Add(key, prefsManager.GetInt(key, 1));
        }
            
        foreach (var weapon in WeaponsSettings)
        {
            foreach (var weaponSpelling in weapon.Value.spellings)
            {
                if (TryGetPromptParameter(weaponSpelling, prompt, 3) is not {} variationsStr)
                    continue;

                var weaponVariationValues = GetVariationsValues(variationsStr);
                    
                // Validation
                for (var i = 0; i < 3; i++)
                {
                    if (loadoutPolicies.TryGetValue(weapon.Key, out var setting))
                    {
                        // TODO: Custom tweak for Breezy, remove later
                        if (weapon.Key == Weapon.Nailgun && i == 2)
                            continue;
                            
                        switch (setting.Value)
                        {
                            case LoadoutPolicy.ForbidAnyChanges:
                            case LoadoutPolicy.OnlyAllowToSwitchVariations
                                when weaponVariationValues[i] == 0:
                            case LoadoutPolicy.OnlyAllowToUnequip
                                when weaponVariationValues[i] > 0:
                                continue;
                            case LoadoutPolicy.AllowAll:
                            default:
                                var weaponIndex = weapon.Key != Weapon.Revolver || i == 0 ? i 
                                    : i == 1 ? 2 : 1;
                                prefsManager.SetInt($"weapon.{weapon.Value.key}{weaponIndex}", weaponVariationValues[i]);
                                break;
                        }
                    }

                    if (!noAltersLoadoutPolicies.TryGetValue(weapon.Key, out var noAltSetting))
                        continue;
                        
                    if (noAltSetting.Value == NoAltersLoadoutPolicy.ForbidToUnequip)
                        continue;

                    prefsManager.SetInt($"weapon.{weapon.Value.key}{i}", weaponVariationValues[i] != 2 ? weaponVariationValues[i] : 1);
                }
            }
        }
            
        var armIndex = 0;
        foreach (var arm in ArmsSettings)
        {
            foreach (var armSpelling in arm.Value)
            {
                if (TryGetPromptParameter(armSpelling, prompt, 1) is not {} variationsStr)
                    continue;

                if (!armsPolicies.TryGetValue(arm.Key, out var armSettings))
                    continue;
                    
                if (armSettings.Value == NoAltersLoadoutPolicy.ForbidToUnequip)
                    continue;
                        
                prefsManager.SetInt($"weapon.arm{armIndex}", char.ToUpperInvariant(variationsStr[0]) == 'U' ? 0 : 1);
            }

            armIndex++;
        }
            
        GunSetter.Instance!.ResetWeapons();
        FistControl.Instance!.ResetFists();
        return;

        int[] GetVariationsValues(string input)
        {
            return input.ToCharArray().Select(c =>
            {
                return char.ToUpperInvariant(c) switch
                {
                    'U' => 0,
                    'E' => 1,
                    'D' => 1,
                    'A' => 2,
                    _ => 0
                };
            }).ToArray();
        }
    }
        
    protected override void CancelOut()
    {
        var prefsManager = PrefsManager.Instance!;

        foreach (var originalValue in _originalLoadout)
            prefsManager.SetInt(originalValue.Key, originalValue.Value);

        GunSetter.Instance!.ResetWeapons();
        FistControl.Instance!.ResetFists();
    }
        
    private static string TryGetPromptParameter(string parameter, string prompt, int paramsLength)
    {
        var matches = Regex.Matches(prompt, parameter + @"\s*\:\s*([ADUE]{" + paramsLength + "})", RegexOptions.IgnoreCase);
        return matches.Count == 0 ? null : matches[0].Groups[1].Value;
    }

    private enum Weapon
    {
        Revolver,
        Shotgun,
        Nailgun,
        Railgun,
        RocketLauncher
    }
        
    private enum Arm
    {
        Feedbacker,
        Knuckleblaster,
        Whiplash
    }
        
    private enum LoadoutPolicy
    {
        ForbidAnyChanges,
        OnlyAllowToSwitchVariations,
        OnlyAllowToUnequip,
        AllowAll
    }
        
    private enum NoAltersLoadoutPolicy
    {
        AllowToUnequip,
        ForbidToUnequip
    }
}