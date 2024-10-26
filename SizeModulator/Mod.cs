using PulsarModLoader;
using PulsarModLoader.Chat.Commands.CommandRouter;
using System;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
namespace SizeModulator
{
    public class Mod : PulsarMod
    {
        public override string Version => "1.2";

        public override string Author => "pokegustavo";

        public override string ShortDescription => "Changes the size of player";

        public override string HarmonyIdentifier()
        {
            return "pokegustavo.Size";
        }
    }


    public class Command : ChatCommand
    {
        public override string[] CommandAliases()
        {
            return new string[]
            {
                "size"
            };
        }
        public override string Description()
        {
            return $"Modulate size";
        }
        public override void Execute(string arguments)
        {
            if (PLNetworkManager.Instance.MyLocalPawn == null) return;
            string[] split = arguments.Split(' ');
            if (split.Length == 1)
            {
                if (float.TryParse(arguments, out float size))
                {
                    if (size > 1000000000) size = 1000000000;
                    else if (size < -1000000000) size = -1000000000;
                    PLNetworkManager.Instance.MyLocalPawn.transform.localScale = new UnityEngine.Vector3(size, size, size);
                }
            }
            else if(split.Length == 3)
            {
                if (float.TryParse(split[0],out float x) && float.TryParse(split[1], out float y) && float.TryParse(split[2], out float z))
                {
                    PLNetworkManager.Instance.MyLocalPawn.transform.localScale = new UnityEngine.Vector3(x, y, z);
                }
                else 
                {
                    PulsarModLoader.Utilities.Messaging.Notification("Invalid arguments! You must enter a single number or 3 seperate numbers");
                }
            }
            else 
            {
                PulsarModLoader.Utilities.Messaging.Notification("Invalid arguments! You must enter a single number or 3 seperate numbers");
            }
        }

        public static float GetModifier(Vector3 scale) 
        {
            return (scale.x + scale.y + scale.z) / 3;
        }
    }

    [HarmonyLib.HarmonyPatch(typeof(PLPawn), "OnPhotonSerializeView")]
    class SyncSize
    {
        static void Postfix(PLPawn __instance, PhotonStream stream, PhotonMessageInfo info)
        {
            try
            {
                if (stream.isWriting)
                {
                    stream.SendNext(__instance.transform.localScale.x);
                    stream.SendNext(__instance.transform.localScale.y);
                    stream.SendNext(__instance.transform.localScale.z);

                }
                else if(stream.Count > 20)
                {
                    float x = (float)stream.ReceiveNext();
                    float y = (float)stream.ReceiveNext();
                    float z = (float)stream.ReceiveNext();
                    __instance.transform.localScale = new UnityEngine.Vector3(x, y, z);
                }
                __instance.RecievedUpdate = true;
            }
            catch (InvalidCastException)
            {
            }
        }
    }
    [HarmonyLib.HarmonyPatch(typeof(PLPawnItem_PhasePistol), "SetupNewBoltDamage")]
    class BoltsDamage 
    {
        static void Postfix(PLPawn ___MySetupPawn, PLBolt newBolt) 
        {
            newBolt.DamageDoneMultiplier *= Command.GetModifier(___MySetupPawn.transform.localScale);
        }
    }
    [HarmonyLib.HarmonyPatch(typeof(PLPawnItem_HeldBeamPistol_WithHealing), "CalcDamageDone")]
    class HeallingBeamDamage 
    {
        static void Postfix(ref PLPawn ___MySetupPawn, ref float __result) 
        {
            __result *= Command.GetModifier(___MySetupPawn.transform.localScale);
        }
    }
    [HarmonyLib.HarmonyPatch(typeof(PLPawnItem_HeldBeamPistol_WithHealing), "CalcHealingDone")]
    class HeallingBeamHeal
    {
        static void Postfix(ref PLPawn ___MySetupPawn, ref float __result)
        {
            __result *= Command.GetModifier(___MySetupPawn.transform.localScale);
        }
    }
    [HarmonyLib.HarmonyPatch(typeof(PLPawnItem_HeldBeamPistol), "CalcDamageDone")]
    class BeamRifleDamage
    {
        static void Postfix(ref PLPawn ___MySetupPawn, ref float __result)
        {
            __result *= Command.GetModifier(___MySetupPawn.transform.localScale);
        }
    }
    [HarmonyLib.HarmonyPatch(typeof(PLGrenadeInstance),"Explode")]
    class GrenadeExplosionSize 
    {
        static bool Prefix(PLGrenadeInstance __instance) 
        {
            if (__instance.ExplosionPrefab != null)
            {
                GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(__instance.ExplosionPrefab, __instance.transform.position, __instance.transform.rotation);
                gameObject.layer = __instance.gameObject.layer;
                gameObject.transform.localScale = __instance.transform.localScale;
                foreach (object obj in gameObject.transform)
                {
                    ((Transform)obj).gameObject.layer = __instance.gameObject.layer;
                }
            }
            UnityEngine.Object.Destroy(__instance.gameObject);
            return false;
        }
    }
    [HarmonyLib.HarmonyPatch(typeof(PLPawnItem_LauncherBase),"OnActive")]
    class GrenadeSize 
    {
        static void Postfix(ref List<PLGrenadeInstance> ___GrenadeInstances, ref PLPawn ___MySetupPawn) 
        {
            if (___MySetupPawn == null) return;
            foreach(PLGrenadeInstance grenade in ___GrenadeInstances) 
            {
                if(grenade != null) 
                {
                    grenade.transform.localScale = ___MySetupPawn.transform.localScale;
                }
            }
        }
    }
    [HarmonyLib.HarmonyPatch(typeof(PLGrenadeInstance), "OnPhotonSerializeView")]
    class SyncGrenadeSize 
    {
        static void Postfix(PLGrenadeInstance __instance, PhotonStream stream) 
        {
            try
            {
                if (stream.isWriting)
                {
                    stream.SendNext(__instance.transform.localScale.x);
                    stream.SendNext(__instance.transform.localScale.y);
                    stream.SendNext(__instance.transform.localScale.z);

                }
                else
                {
                    float x = (float)stream.ReceiveNext();
                    float y = (float)stream.ReceiveNext();
                    float z = (float)stream.ReceiveNext();
                    __instance.transform.localScale = new UnityEngine.Vector3(x, y, z);
                }
            }
            catch (InvalidCastException)
            {
            }
        }
    }
    [HarmonyLib.HarmonyPatch(typeof(PLPawnItem_LauncherBase), "CalcGrenadePower")]
    class GrenadePower 
    {
        static void Postfix(PLPawn ___MySetupPawn, ref float __result) 
        {
            __result *= Command.GetModifier(___MySetupPawn.transform.localScale);
        }
    }
    [HarmonyLib.HarmonyPatch(typeof(PLPulseGrenadeInstance),"Update")]
    class PulseGrenadeDamage 
    {
        static void Postfix(PLPulseGrenadeInstance __instance, PLPlayer ___PlayerOwner) 
        {
            if (___PlayerOwner == null || ___PlayerOwner.GetPawn() == null) return;
            __instance.DamageRadius = 7 * Command.GetModifier(___PlayerOwner.GetPawn().transform.localScale);
        }
    }
    [HarmonyLib.HarmonyPatch(typeof(PLMiniGrenadeInstance), "Update")]
    class MiniGrenadeDamage
    {
        static void Postfix(PLMiniGrenadeInstance __instance, PLPlayer ___PlayerOwner)
        {
            if (___PlayerOwner == null || ___PlayerOwner.GetPawn() == null) return;
            __instance.DamageRadius = 7 * Command.GetModifier(___PlayerOwner.GetPawn().transform.localScale);
        }
    }
    [HarmonyLib.HarmonyPatch(typeof(PLStunGrenadeInstance), "Update")]
    class StunGrenadeDamage
    {
        static void Postfix(PLStunGrenadeInstance __instance, PLPlayer ___PlayerOwner)
        {
            if (___PlayerOwner == null || ___PlayerOwner.GetPawn() == null) return;
            __instance.DamageRadius = 7 * Command.GetModifier(___PlayerOwner.GetPawn().transform.localScale);
        }
    }
    [HarmonyLib.HarmonyPatch(typeof(PLRepairGrenadeInstance), "Update")]
    class RepairGrenadeDamage
    {
        static void Postfix(PLRepairGrenadeInstance __instance, PLPlayer ___PlayerOwner)
        {
            if (___PlayerOwner == null || ___PlayerOwner.GetPawn() == null) return;
            __instance.DamageRadius = 7 * Command.GetModifier(___PlayerOwner.GetPawn().transform.localScale);
        }
    }
    [HarmonyLib.HarmonyPatch(typeof(PLPawn), "Update")]
    class HealthBoost
    {
        static void Postfix(PLPawn __instance)
        {
            if (__instance.GetPlayer() != null)
            {
                float num10 = 100f;
                if (__instance.GetPlayer().RaceID == 2)
                {
                    num10 = 60f;
                }
                float num11 = num10 + (float)__instance.GetPlayer().Talents[0] * 20f;
                num11 += (float)__instance.GetPlayer().Talents[57] * 20f;
                foreach (PawnStatusEffect pawnStatusEffect5 in __instance.MyStatusEffects)
                {
                    if (pawnStatusEffect5 != null && pawnStatusEffect5.Type == EPawnStatusEffectType.HEALTH_REGEN)
                    {
                        num11 += 20f;
                    }
                }
                float value2 = num11;
                if (__instance.GetPlayer().GetClassID() != -1 && __instance.GetPlayer().GetClassID() < 5 && __instance.GetPlayer().TeamID == 0)
                {
                    PLServerClassInfo plserverClassInfo = PLServer.Instance.ClassInfos[__instance.GetPlayer().GetClassID()];
                    num11 += (float)plserverClassInfo.SurvivalBonusCounter * 5f;
                }
                num11 *= Command.GetModifier(__instance.transform.localScale);
                if (num11 < 1) num11 = 1f;
                if (__instance.MaxHealth != num11)
                {
                    __instance.Health = __instance.Health / __instance.MaxHealth * num11;
                    __instance.MaxHealth = num11;
                    __instance.MaxHealth_Normal = value2;
                }
            }
        }
    }
}
