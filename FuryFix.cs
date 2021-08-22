using System.Reflection;
using Modding;
using UnityEngine;
using System.Collections;
using SFCore.Utils;

namespace FuryFix
{
    public class FuryFix : Mod
    {
        internal static FuryFix Instance;

        private PlayerData _pd;
        private HeroController _hero;
        private PlayMakerFSM _slashFsm;

        public override string GetVersion() => Assembly.GetExecutingAssembly().GetName().Version.ToString();

        public override void Initialize()
        {
            Instance = this;

            ModHooks.AfterSavegameLoadHook += OnAfterSavegameLoad;
            ModHooks.SetPlayerBoolHook += OnSetPlayerBoolHook;
            On.HeroController.Attack += OnHeroControllerAttack;
        }

        private bool OnSetPlayerBoolHook(string originalSet, bool orig)
        {
            if (originalSet == "equippedCharm_6")
            {
                if (orig)
                {
                    
                }
                else
                {
                }
            }
            return orig;
        }

        private void OnHeroControllerAttack(On.HeroController.orig_Attack orig, HeroController self, GlobalEnums.AttackDirection attackDir)
        {
            //Log(nameof(self.playerData.maxHealth) + self.playerData.maxHealth);
            //Log(nameof(self.playerData.healthBlue) + self.playerData.healthBlue);
            //int tmpMaxHealth = self.playerData.maxHealth;
            //int tmpHealthBlue = self.playerData.healthBlue;
            //bool tmpJoniBeam = ReflectionExtensions.GetAttr<HeroController, bool>(self, "joniBeam");
            //self.playerData.maxHealth = tmpMaxHealth + 1;
            //self.playerData.healthBlue = 0;
            //ReflectionExtensions.SetAttr<HeroController, bool>(self, "joniBeam", false);
            
            if (self.playerData.GetBool("equippedCharm_6"))
            {
                self.playerData.SetInt("maxHealth", self.playerData.GetInt("maxHealth") + 1);
                orig(self, attackDir);
                self.playerData.SetInt("maxHealth", self.playerData.GetInt("maxHealth") - 1);
            }
            else
            {
                orig(self, attackDir);
            }

            //self.playerData.maxHealth = tmpMaxHealth;
            //self.playerData.healthBlue = tmpHealthBlue;
            //ReflectionExtensions.SetAttr<HeroController, bool>(self, "joniBeam", tmpJoniBeam);
        }

        private void OnAfterSavegameLoad(SaveGameData data)
        {
            GameManager.instance.StartCoroutine(FixFury());
        }

        private IEnumerator FixFury()
        {
            yield return new WaitWhile(() => !GameObject.Find("Charm Effects"));

            _pd = PlayerData.instance;
            _hero = HeroController.instance;
            _slashFsm = _hero.normalSlashFsm;
            GameObject furyGo = GameObject.Find("Charm Effects");
            PlayMakerFSM furyFsm = furyGo.LocateMyFSM("Fury");
            furyFsm.ChangeTransition("Init", "FINISHED", "Pause");
            furyFsm.ChangeTransition("Check HP", "CANCEL", "Pause");
            furyFsm.ChangeTransition("Deactivate", "FINISHED", "Pause");
            furyFsm.ChangeTransition("Activate", "HERO HEALED FULL", "Recheck");
            furyFsm.ChangeTransition("Stay Furied", "HERO HEALED FULL", "Recheck");
            furyFsm.SetState("Pause");

            //bool furyCharmCheck = false;
            //bool slashFuryCheck = false;

            //while (true)
            //{
            //    yield return new WaitWhile(() => (pd.GetBool("equippedCharm_6"))); // && !((furyFSM.ActiveStateName == "Activate") || (furyFSM.ActiveStateName == "Stay Furied"))
            //    //furyCharmCheck = PlayerData.instance.GetBool("equippedCharm_6");
            //    //slashFuryCheck = ReflectionExtensions.GetAttr<NailSlash, bool>(hero.normalSlash, "fury");
            //    //if (!furyCharmCheck && slashFuryCheck)
            //    //{
            //        furyFSM.SendEvent("ALL CHARMS END");
            //    //}
            //    //yield return null;
            //}

            bool furyCharm = false;
            int health = 0;
            bool furyEffect = false;

            var furyState = furyFsm.GetState("Activate");
            var furyActions = furyState.Actions;
            var notFuryState = furyFsm.GetState("Deactivate");
            var notFuryActions = notFuryState.Actions;
            while (true)
            {
                furyCharm = _pd.GetBool("equippedCharm_6");
                health = _pd.GetInt("health");
                furyEffect = furyCharm && (health == 1);

                if (furyEffect)
                {
                    for (int i = 2; i < furyActions.Length; i++)
                    {
                        furyState.FinishAction(furyActions[i]);
                    }
                    for (int i = 13; i <= 19; i++)
                    {
                        (furyActions[i] as HutongGames.PlayMaker.Actions.SetFsmBool).gameObject.GameObject.Value.LocateMyFSM("nailart_damage").SetState("Init");
                    }
                }
                else
                {
                    notFuryState.FinishAction(notFuryActions[1]);
                    for (int i = 3; i < notFuryActions.Length; i++)
                    {
                        notFuryState.FinishAction(notFuryActions[i]);
                    }
                    for (int i = 13; i <= 19; i++)
                    {
                        (furyActions[i] as HutongGames.PlayMaker.Actions.SetFsmBool).gameObject.GameObject.Value.LocateMyFSM("nailart_damage").SetState("Init");
                    }
                }
                /* 13 - 19 */
                yield return null;
            }
        }
    }
}
