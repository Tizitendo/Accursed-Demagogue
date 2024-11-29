using MelonLoader;
using UnityEngine;
using HarmonyLib;
using System.Runtime.CompilerServices;
using static Sony.NP.Matching;
using LB;
using UnityEngine.InputSystem;
using Unity.Mathematics;
using MelonLoader.TinyJSON;
using System;
using UnityEngine.AI;
using static MelonLoader.MelonLogger;
using System.Reflection;
using System.Collections;
using static LB.PerceptionService;
using static UnityEngine.Rendering.VolumeComponent;
using System.Reflection.Emit;

[assembly: MelonInfo(typeof(Accursed_Demagogue.Core), "Accursed Demagogue", "1.0.0", "Onyx", null)]
[assembly: MelonGame("Funktronic Labs", "The Light Brigade")]

namespace Accursed_Demagogue
{
    public class Core : MelonMod
    {
        static CustomMenuItemButton buttonDifficulty5 = new CustomMenuItemButton();
        static bool DiffSelected = false;
        static bool GotKill = false;
        static float pricemul = 3;
        static float speedmul = 1.3f;
        static float Paralyzecool;
        static Localization localization;

        public override void OnInitializeMelon()
        {
            LoggerInstance.Msg("Initialized.");
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            if (sceneName != "TitleScreenScene" && sceneName != "MetaSceneApplicationStart" && sceneName != "MetaSceneTransitionLoading" && sceneName != "MetaSceneStart")
            {
                MelonLogger.Msg(sceneName);
                if (Services.metagame.run.difficulty == 5)
                {
                    Services.metagame.run.livesMax = 2;
                    if (Services.metagame.run.lives > Services.metagame.run.livesMax)
                    {
                        Services.metagame.run.lives = Services.metagame.run.livesMax;
                    }
                    DiffSelected = true;

                    localization.languages[localization.languageIndex].data[1181] = "Inject to gain +10hp or +20HP overload.";
                    localization.languages[localization.languageIndex].data[1153] = "Hand-rolled pre-war cigar. Heals for +7 Health";
                    localization.languages[localization.languageIndex].data[1157] = "An exquisite pre - war golden hand-rolled cigar, perfectly preserved. Heals for +60";
                    localization.languages[localization.languageIndex].data[1161] = "Drink to recover +5 HP. Has 3 charges.";
                    localization.languages[localization.languageIndex].data[1163] = "Drink to recover +15 HP. Has 3 charges.";
                }
                else
                {
                    DiffSelected = false;
                }

            }
        }

        //Difficulty Selector Menu
        [HarmonyPatch(typeof(FatherNPCTrigger), "OnPrayerComplete")]
        private static class AddDifficulty
        {
            private static void Postfix(FatherNPCTrigger __instance)
            {
                MethodInfo privateMethod = typeof(FatherNPCTrigger).GetMethod("OnButtonPressed", BindingFlags.NonPublic | BindingFlags.Instance);

                if (Services.metagame.run.difficulty == 5)
                {
                    buttonDifficulty5.label.text = string.Format("<color=#F4FF3D>{0}{1}</color>", "<sprite name=\"Icon_Soul_Color\"> ", "Accursed Demagogue");
                }
                else
                {
                    buttonDifficulty5.label.text = "Accursed Demagogue";
                }

                buttonDifficulty5.onClick.AddListener(delegate ()
                {
                    DiffSelected = true;
                    privateMethod.Invoke(__instance, new object[] { 5 });
                });
            }
        }
        [HarmonyPatch(typeof(FatherNPCTrigger), "Awake")]
        private static class SetupDifficultyPanel
        {
            private static void Postfix(FatherNPCTrigger __instance)
            {
                __instance.menuCanvas.transform.position += new Vector3(0, 0.1f, 0);
                buttonDifficulty5 = GameObject.Instantiate(__instance.buttonDifficulty0);
                buttonDifficulty5.name = "ButtonDifficulty5";
                buttonDifficulty5.transform.SetParent(__instance.buttonDifficulty0.transform.parent);
                buttonDifficulty5.transform.eulerAngles = __instance.buttonDifficulty0.transform.eulerAngles;
                buttonDifficulty5.transform.localPosition = __instance.buttonDifficulty0.transform.localPosition + new Vector3(0, -0.6f, 0);
                buttonDifficulty5.transform.localScale = __instance.buttonDifficulty0.transform.localScale;

                __instance.menuCanvas.transform.localScale += new Vector3(0, 0.1f, 0);
                __instance.buttonDifficulty0.transform.localScale += new Vector3(0, -0.05f, 0);
                __instance.buttonDifficulty1.transform.localScale += new Vector3(0, -0.05f, 0);
                __instance.buttonDifficulty2.transform.localScale += new Vector3(0, -0.05f, 0);
                __instance.buttonDifficulty3.transform.localScale += new Vector3(0, -0.05f, 0);
                __instance.buttonDifficulty4.transform.localScale += new Vector3(0, -0.05f, 0);
                buttonDifficulty5.transform.localScale += new Vector3(0, -0.05f, 0);

                __instance.buttonDifficulty0.transform.position = new Vector3(-12.4423f, 4.8f, 19.2188f);
                __instance.buttonDifficulty1.transform.position = new Vector3(-12.4423f, 4.68f, 19.2188f);
                __instance.buttonDifficulty2.transform.position = new Vector3(-12.4423f, 4.56f, 19.2188f);
                __instance.buttonDifficulty3.transform.position = new Vector3(-12.4423f, 4.44f, 19.2188f);
                __instance.buttonDifficulty4.transform.position = new Vector3(-12.4423f, 4.32f, 19.2188f);
                buttonDifficulty5.transform.position = new Vector3(-12.4423f, 4.20f, 19.2188f);
            }
        }
        [HarmonyPatch(typeof(FatherNPCTrigger), "GetDifficultyColor")]
        private static class DifficultyColor
        {
            private static void Postfix(FatherNPCTrigger __instance, ref Color __result, int difficulty)
            {
                if (difficulty == 5)
                {
                    __result = new Color(0.3f, 0.1f, 1.0f, 1.0f);
                }
            }
        }
        [HarmonyPatch(typeof(Localization), "Get", new Type[] { typeof(string) })]
        private static class AddDifficultyName
        {
            private static void Postfix(Localization __instance, ref string __result, string key)
            {
                localization = __instance;
                if (key == string.Format("UI_DIFFICULTYSELECTOR_DIFFICULTY_{0}", 5))
                {
                    __result = "Accursed Demagogue";
                }
            }
        }

        //other Difficulty settings
        [HarmonyPatch(typeof(MetagameService), "Awake")]
        private static class IncreaseMaxDifficulty
        {
            private static void Postfix()
            {
                MetagameService.DifficultyMax = 5;
            }
        }

        //unique Difficulty changes

        [HarmonyPatch(typeof(ItemConfig), "GetStorePrice")]
        private static class ChangePrices
        {
            private static void Postfix(ref int __result)
            {
                __result = (int)(__result * pricemul);
            }
        }
        [HarmonyPatch(typeof(AIActor), "GetSpeedMultiplier")]
        private static class speedmultiplier
        {
            private static void Postfix(ref float __result)
            {
                if (DiffSelected)
                {
                    __result *= speedmul;
                }
            }
        }
        [HarmonyPatch(typeof(BossFather), "Awake")]
        private static class speedmulFather
        {
            private static void Prefix(BossFather __instance)
            {
                if (DiffSelected)
                {
                    var runSpeed = AccessTools.Field(__instance.GetType(), "runSpeed");
                    runSpeed.SetValue(__instance, (float)(runSpeed.GetValue(__instance)) * 1.5f);
                }
            }
        }
        [HarmonyPatch(typeof(EncounterTokens), "ScaleBonusTokensByWFCConfig")]
        private static class ReduceChests
        {
            private static void Postfix(ref EncounterTokens __result)
            {
                if (DiffSelected)
                {
                    //MelonLogger.Msg(__result.tokensChest);
                    //MelonLogger.Msg(__result.tokensLoot);
                    __result.tokensChest = Math.Max(__result.tokensChest - 1, UnityEngine.Random.Range(0, 2));
                    __result.tokensLoot = Math.Min(__result.tokensLoot, 20);
                    //MelonLogger.Msg(__result.tokensChest);
                }
            }
        }
        [HarmonyPatch(typeof(Actor), "OnDamageEffectParalyze")]
        private static class ParalyzeCooldown
        {
            private static bool Prefix(Actor __instance)
            {
                if (DiffSelected)
                {
                    if (Paralyzecool < Time.time)
                    {
                        Paralyzecool = Time.time + 3;
                        return true;
                    }
                    return false;
                }
                return true;
            }
        }

        //Healing item changes
        [HarmonyPatch(typeof(Canteen), "Awake")]
        private static class ReduceCanteenHealing
        {
            private static void Postfix(Canteen __instance)
            {
                if (DiffSelected)
                {
                    if (__instance.healthAdd == 10)
                    {
                        __instance.healthAdd = 5;
                    }
                    else
                    {
                        __instance.healthAdd = 15;
                    }
                }
            }
        }
        [HarmonyPatch(typeof(Canteen), "DoObsidianHealing")]
        public static class ReduceObsidianCanteenHealing
        {
            static void Prefix(PlayerActor player)
            {
                if (DiffSelected)
                {
                    player.SetUnpurifiedSoulsHelper((int)(player.GetUnpurifiedSoulsHelper()/16));
                }
            }
        }
        [HarmonyPatch(typeof(Consumable_Medkit), "OnConsumableApplyEffect")]
        private static class ReduceSyringeHealing
        {
            private static void Prefix(Consumable_Medkit __instance)
            {
                if (DiffSelected)
                {
                    PlayerActor playerActor = Tracked<PlayerActor>.FindFirst();
                    __instance.healOverload = 20 + (Math.Max(Math.Min(playerActor.health, playerActor.maxHealth) - playerActor.maxHealth, -5)) * 2;
                }
            }
        }
        [HarmonyPatch(typeof(Cigar), "DoExhale")]
        private static class ReduceCigarHeal
        {
            static void Postfix(Cigar __instance)
            {
                if (DiffSelected)
                {
                    var hitsTakenField = AccessTools.Field(__instance.GetType(), "hitsTaken");
                    int hitsTaken = (int)hitsTakenField.GetValue(__instance);
                    int maxhits = 6;
                    if (__instance.golden)
                    {
                        maxhits = 29;
                    }
                    if (hitsTaken >= maxhits)
                    {
                        var dissolveMethod = AccessTools.Method(__instance.GetType(), "Dissolve");
                        dissolveMethod.Invoke(__instance, null);
                    }
                }
            }
        }
        [HarmonyPatch(typeof(LootBreakableSoulContainer), "CustomLootSpawn")]
        private static class StopExtraHealth
        {
            private static void Prefix(LootBreakableSoulContainer __instance)
            {
                if (DiffSelected)
                {
                    PlayerActor playerActor = Tracked<PlayerActor>.FindFirst();
                    playerActor.health -= __instance.bonusHealth;
                }
            }
        }

        [HarmonyPatch(typeof(PlayerActor), "UpdateLowHealthState")]
        private static class RecoverHealthOnKill
        {
            private static void Postfix(PlayerActor __instance, ref float ___lowHealthDamageRegenTime)
            {
                if (DiffSelected)
                {
                    ___lowHealthDamageRegenTime = float.MaxValue;
                    if (GotKill)
                    {
                        ___lowHealthDamageRegenTime = Time.time - 1;
                    }
                    GotKill = false;
                }
            }
        }
        [HarmonyPatch(typeof(AIActor), "OnDamageDeath")]
        private static class CheckForKill
        {
            private static void Postfix(AIActor __instance)
            {
                MelonLogger.Msg(localization.languages[localization.languageIndex].data[1161]);
                GotKill = true;
            }
        }

        [HarmonyPatch(typeof(PlayerActor), "OnDamageApply")]
        private static class ReduceMaxHealthOnDamage
        {
            private static void Postfix(PlayerActor __instance)
            {
                if (DiffSelected)
                {
                    if (__instance.maxHealth > 1)
                    {
                        if (Services.metagame.run != null)
                        {
                            Services.metagame.run.accumulatedBonusMaxHP -= 1;
                        }
                        __instance.maxHealth -= 1;
                    }
                }
            }
        }
    }
}