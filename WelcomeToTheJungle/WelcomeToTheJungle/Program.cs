using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;

namespace WelcomeToTheJungle
{
    class Program
    {
        private static bool IsSpider()
        {
            return Player.Instance.Model == "EliseSpider";
        }

        private static AIHeroClient Me => Player.Instance;

        private static readonly Item Zhonya = new Item(ItemId.Zhonyas_Hourglass);

        private static Spell.Targeted Q1 = new Spell.Targeted(SpellSlot.Q, 625);
        private static Spell.Targeted Q2 = new Spell.Targeted(SpellSlot.Q, 475);
        private static Spell.Skillshot W1 = new Spell.Skillshot(SpellSlot.W, 950, SkillShotType.Circular);
        private static Spell.Active W2 = new Spell.Active(SpellSlot.W);
        private static Spell.Skillshot E1 = new Spell.Skillshot(SpellSlot.E, 1075, SkillShotType.Linear, 250, 1600, 80) { AllowedCollisionCount = 0 };
        private static Spell.Active E2 = new Spell.Active(SpellSlot.E);
        private static Spell.Active R = new Spell.Active(SpellSlot.R);

        private static List<Action> _actionList = new List<Action>();

        static void Main(string[] args)
        {
            Loading.OnLoadingComplete += GameLoad;
        }

        private static void GameLoad(EventArgs args)
        {
            Console.WriteLine("Elise Loaded");

            //Smite
            var smite = Player.Spells.FirstOrDefault(s => s.Name.ToLower().Contains("summonersmite"));
            if (smite != null)
            {
                Smite = new Spell.Targeted(smite.Slot, 570);
                PlayerHasSmite = true;
            }

            LoadMenu();

            Game.OnUpdate += OnTick;
            Obj_AI_Base.OnProcessSpellCast += OnSpellCast;
        }

        #region Menu
        public static Menu FirstMenu,
         ComboMenu,
         HarassMenu,
         LaneClearMenu,
         LasthitMenu,
         JungleClearMenu,
         SummonerMenu,
         MiscMenu,
         DrawingMenu,
         SettingsMenu;

        #endregion

        private static void LoadMenu()
        {
            FirstMenu = MainMenu.AddMenu("Elise", "articunoElise");
            ComboMenu = FirstMenu.AddSubMenu("• Combo", "combo");
            LaneClearMenu = FirstMenu.AddSubMenu("• LaneClear", "LaneClear");
            JungleClearMenu = FirstMenu.AddSubMenu("• JungleClear", "JungleClear");
            SummonerMenu = FirstMenu.AddSubMenu("• Summoner Spells", "activatorSummonerspells");

            ComboMenu.AddGroupLabel("Spells");
            ComboMenu.CreateCheckBox("Use Q", "qUse");
            ComboMenu.CreateCheckBox("Use W", "wUse");
            ComboMenu.CreateCheckBox("Use E", "eUse");
            ComboMenu.CreateCheckBox("Use R", "rUse");
            ComboMenu.CreateCheckBox("Use Smart Combo", "smartCombo");

            LaneClearMenu.AddGroupLabel("Spells");
            LaneClearMenu.CreateCheckBox("Use Q", "qUse");
            LaneClearMenu.CreateCheckBox("Use W", "wUse");
            LaneClearMenu.CreateCheckBox("Use E", "eUse");
            LaneClearMenu.CreateCheckBox("Use R", "rUse");
            LaneClearMenu.AddGroupLabel("Settings");
            LaneClearMenu.CreateSlider("Mana must be lower than [{0}%] to use Harass spells", "manaSlider", 30);

            JungleClearMenu.AddGroupLabel("Spells");
            JungleClearMenu.CreateCheckBox("Use Q", "qUse");
            JungleClearMenu.CreateCheckBox("Use W", "wUse");
            JungleClearMenu.CreateCheckBox("Use E", "eUse");
            JungleClearMenu.CreateCheckBox("Use R", "rUse");
            JungleClearMenu.AddGroupLabel("Settings");
            JungleClearMenu.CreateSlider("Mana must be lower than [{0}%] to use Harass spells", "manaSlider", 30);

            MiscMenu.AddGroupLabel("Settings");

            #region SummonerSpells

            if (PlayerHasSmite)
            {
                SummonerMenu.AddGroupLabel("Smite");
                SummonerMenu.CreateKeyBind("Disable Smite", "smiteKeybind", 'Z', 'U');

                var combo = SummonerMenu.Add("comboBox", new ComboBox("Smite mode", new List<string> { "Use Prediction", " Dont use prediction" }));
                var label = SummonerMenu.Add("comboBoxText", new Label("aaa"));
                switch (combo.CurrentValue)
                {
                    case 0:
                        label.CurrentValue = "It will try to predict the health of the jungle minion to have a fast Smite";
                        break;
                    case 1:
                        label.CurrentValue = "It will only use if the jungle minion health is lower than the smite damage";
                        break;
                }
                combo.OnValueChange += delegate (ValueBase<int> sender, ValueBase<int>.ValueChangeArgs args)
                {
                    switch (sender.CurrentValue)
                    {
                        case 0:
                            label.CurrentValue = "It will try to predict the health of the jungle minion to have a fast Smite";
                            break;
                        case 1:
                            label.CurrentValue = "It will only use if the jungle minion health is lower than the smite damage";
                            break;
                    }
                };
                SummonerMenu.AddSeparator();
                SummonerMenu.CreateSlider("Subtract [{0}] to the smite damage calculation", "sliderDMGSmite", 15, 0, 50);
                SummonerMenu.AddSeparator();
                SummonerMenu.CreateCheckBox("Use smite on champions", "smiteUseOnChampions");

                switch (Game.MapId)
                {
                    case GameMapId.TwistedTreeline:
                        SummonerMenu.AddLabel("Epic");
                        SummonerMenu.CreateCheckBox("Smite Spider Boss", "monster" + "TT_Spiderboss");
                        SummonerMenu.AddLabel("Normal");
                        SummonerMenu.CreateCheckBox("Smite Golem", "monster" + "TTNGolem");
                        SummonerMenu.CreateCheckBox("Smite Wolf", "monster" + "TTNWolf");
                        SummonerMenu.CreateCheckBox("Smite Wraith", "monster" + "TTNWraith", false);
                        break;
                    case GameMapId.SummonersRift:
                        SummonerMenu.AddLabel("Epic");
                        SummonerMenu.CreateCheckBox("Smite Baron", "monster" + "SRU_Baron");
                        SummonerMenu.CreateCheckBox("Smite Water Dragon", "monster" + "SRU_Dragon_Water");
                        SummonerMenu.CreateCheckBox("Smite Fire Dragon", "monster" + "SRU_Dragon_Fire");
                        SummonerMenu.CreateCheckBox("Smite Earth Dragon", "monster" + "SRU_Dragon_Earth");
                        SummonerMenu.CreateCheckBox("Smite Air Dragon", "monster" + "SRU_Dragon_Air");
                        SummonerMenu.CreateCheckBox("Smite Elder Dragon", "monster" + "SRU_Dragon_Elder");
                        SummonerMenu.CreateCheckBox("Smite RiftHearald", "monster" + "SRU_RiftHerald");
                        SummonerMenu.AddLabel("Normal");
                        SummonerMenu.CreateCheckBox("Smite Blue", "monster" + "SRU_Blue");
                        SummonerMenu.CreateCheckBox("Smite Red", "monster" + "SRU_Red");
                        SummonerMenu.CreateCheckBox("Smite Crab", "monster" + "Sru_Crab", false);
                        SummonerMenu.AddLabel("Meh...");
                        SummonerMenu.CreateCheckBox("Smite Gromp", "monster" + "SRU_Gromp", false);
                        SummonerMenu.CreateCheckBox("Smite Murkwolf", "monster" + "SRU_Murkwolf", false);
                        SummonerMenu.CreateCheckBox("Smite Razorbeak", "monster" + "SRU_Razorbeak", false);
                        SummonerMenu.CreateCheckBox("Smite Krug", "monster" + "SRU_Krug", false);
                        break;
                }
            }
            #endregion
        }

        private static void OnTick(EventArgs args)
        {
            if (Player.Instance.IsDead) return;

            if (_actionList.Count > 0)
            {
                var action = _actionList.FirstOrDefault();

                action?.Invoke();

                _actionList.Remove(action);

                return;
            }

            /*if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass)) OnHarass();
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear) || permaHarass) OnLaneClear();
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LastHit)) OnLastHit();
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Flee)) OnFlee();
            */
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear)) OnLaneClear();
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear)) OnJungleClear();

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo)) OnCombo();
        }

        private static void OnSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsAlly || Me.IsDead) return;

            foreach (var dangerousSpell in DangerousSpells.Spells)
            {

                if(args.Target != null && args.Target.IsMe)
                    Core.DelayAction(Escaper, dangerousSpell.Delay);

                if (dangerousSpell.Hero.ToString() == sender.BaseSkinName && args.Slot == dangerousSpell.Slot && args.End.Distance(Me) < 100)
                {
                    Core.DelayAction(Escaper, dangerousSpell.Delay);
                }
            }
        }

        private static void OnLaneClear()
        {
            var range = Player.Instance.GetAutoAttackRange();

            if (IsSpider())
            {
                if (E2.IsReady())
                {
                    range = E2.Range;
                }
            }
            else
            {
                if (W1.IsReady())
                {
                    range = W1.Range;
                }
                else if (E1.IsReady())
                {
                    range = E1.Range;
                }
                else if (Q1.IsReady())
                {
                    range = Q1.Range;
                }
            }

            var target = EntityManager.MinionsAndMonsters.EnemyMinions.FirstOrDefault(t => t.Distance(Me) <= range);

            if (target == null || !target.IsValidTarget() || target.IsDead) return;

            if (IsSpider())
            {
                if (!Q2.IsReady() && !E2.IsReady() && !Me.HasBuff("EliseSpiderW") && !Me.IsInAutoAttackRange(target) && LaneClearMenu.GetCheckBoxValue("rUse"))
                {
                    R.Cast();
                }

                if (Q2.IsInRange(target) && Q2.IsReady() && LaneClearMenu.GetCheckBoxValue("qUse"))
                {
                    Q2.Cast(target);

                    if (W2.IsReady() && LaneClearMenu.GetCheckBoxValue("wUse"))
                    {
                        W2.Cast();
                    }
                }

                if (W2.IsReady() && Me.IsInAutoAttackRange(target) && LaneClearMenu.GetCheckBoxValue("wUse"))
                {
                    W2.Cast();
                }

                if (E2.IsInRange(target) && E2.IsReady() && (!Q2.IsReady() || !Q2.IsInRange(target)) && LaneClearMenu.GetCheckBoxValue("eUse"))
                {
                    E2.Cast();
                    E2.Cast(target);
                }
            }
            else
            {
                if (W1.IsInRange(target) && W1.IsReady() && LaneClearMenu.GetCheckBoxValue("wUse"))
                {
                    var pred = W1.GetPrediction(target);

                    if (pred.HitChancePercent >= 70)
                    {
                        W1.Cast(pred.CastPosition);
                    }
                }

                if (Q1.IsInRange(target) && Q1.IsReady() && LaneClearMenu.GetCheckBoxValue("qUse"))
                {
                    Q1.Cast(target);
                }

                if (!Q1.IsReady() && !W1.IsReady() || !target.CanMove)
                {
                    if (LaneClearMenu.GetCheckBoxValue("rUse"))
                        R.Cast();
                }
            }
        }

        private static void OnJungleClear()
        {
            Smiter();
            var range = Player.Instance.GetAutoAttackRange();

            if (IsSpider())
            {
                if (E2.IsReady())
                {
                    range = E2.Range;
                }
            }
            else
            {
                if (W1.IsReady())
                {
                    range = W1.Range;
                }
                else if (E1.IsReady())
                {
                    range = E1.Range;
                }
                else if (Q1.IsReady())
                {
                    range = Q1.Range;
                }
            }

            Obj_AI_Base target = null;

            try
            {
                target = EntityManager.MinionsAndMonsters.Monsters.Aggregate((curMax, x) => ((curMax == null && x.Distance(Me) <= range) || x.MaxHealth > curMax.MaxHealth ? x : curMax));
            }
            catch (Exception e)
            {
                target = EntityManager.MinionsAndMonsters.Monsters.FirstOrDefault();
            }


            if (target == null || !target.IsValidTarget() || target.IsDead) return;

            if (MonsterEpic.Contains(target.BaseSkinName) && target.Health < SmiteDamage()*2)
            {
                var totalDmg = 0d;

                if (PlayerHasSmite && Smite.IsReady())
                    totalDmg += SmiteDamage();

                if (IsSpider())
                {
                    if (Q2.IsReady())
                    {
                        totalDmg += Me.GetSpellDamage(target, SpellSlot.Q);
                    }

                    if (totalDmg > target.Health)
                    {
                        _actionList.Add(() => Q2.Cast(target));
                        _actionList.Add(() => Smite.Cast(target));
                    }
                }
                else
                {
                    if (Q1.IsReady())
                    {
                        totalDmg += Me.GetSpellDamage(target, SpellSlot.Q);
                    }

                    if (totalDmg > target.Health)
                    {
                        _actionList.Add(() => Q1.Cast(target));
                        _actionList.Add(() => Smite.Cast(target));
                    }

                    if (R.IsReady())
                    {
                        totalDmg += Me.GetSpellDamage(target, SpellSlot.Q);

                        if (totalDmg > target.Health)
                        {
                            if(W1.IsReady())
                                _actionList.Add(() => W1.Cast(target.Position));
                            _actionList.Add(() => Q1.Cast(target));
                            _actionList.Add(() => R.Cast());
                            _actionList.Add(() => Q2.Cast(target));
                            _actionList.Add(() => Smite.Cast(target));
                        }
                    }
                }
            }
            else
            {
                if (IsSpider())
                {
                    if (!Q2.IsReady() && !E2.IsReady() && !Me.HasBuff("EliseSpiderW") && !Me.IsInAutoAttackRange(target) && JungleClearMenu.GetCheckBoxValue("rUse"))
                    {
                        R.Cast();
                    }

                    if (Q2.IsInRange(target) && Q2.IsReady() && JungleClearMenu.GetCheckBoxValue("qUse"))
                    {
                        Q2.Cast(target);

                        if (W2.IsReady() && JungleClearMenu.GetCheckBoxValue("wUse"))
                        {
                            W2.Cast();
                        }
                    }

                    if (W2.IsReady() && Me.IsInAutoAttackRange(target) && JungleClearMenu.GetCheckBoxValue("wUse"))
                    {
                        W2.Cast();
                    }
                }
                else
                {
                    if (W1.IsInRange(target) && W1.IsReady() && JungleClearMenu.GetCheckBoxValue("wUse"))
                    {
                        W1.Cast(W1.GetPrediction(target).CastPosition);
                    }

                    if (Q1.IsInRange(target) && Q1.IsReady() && JungleClearMenu.GetCheckBoxValue("qUse"))
                    {
                        Q1.Cast(target);
                    }

                    if ((!Q1.IsReady() && !W1.IsReady()) || !target.CanMove)
                    {
                        if (JungleClearMenu.GetCheckBoxValue("rUse"))
                            R.Cast();
                    }
                }
            }
        }

        private static void OnCombo()
        {
            var range = 675f;

            if (IsSpider())
            {
                if (E2.IsReady())
                {
                    range = E2.Range;
                }
            }
            else
            {
                if (W1.IsReady())
                {
                    range = W1.Range;
                }
                else if (E1.IsReady())
                {
                    range = E1.Range;
                }
                else if (Q1.IsReady())
                {
                    range = Q1.Range;
                }
            }

            var target = TargetSelector.GetTarget(range, DamageType.Magical);

            if (target == null || !target.IsValidTarget() || target.IsDead) return;

            if (IsSpider())
            {
                if (!Q2.IsReady() && !E2.IsReady() && !Me.HasBuff("EliseSpiderW") && !Me.IsInAutoAttackRange(target) && ComboMenu.GetCheckBoxValue("rUse"))
                {
                    R.Cast();
                }

                if (Me.HasBuff("EliseSpiderE"))
                {
                    Player.IssueOrder(GameObjectOrder.AttackUnit, target);
                }

                if (Q2.IsInRange(target) && Q2.IsReady() && ComboMenu.GetCheckBoxValue("qUse"))
                {
                    Q2.Cast(target);

                    if (W2.IsReady() && ComboMenu.GetCheckBoxValue("wUse"))
                    {
                        W2.Cast();
                    }
                }

                if (W2.IsReady() && Me.IsInAutoAttackRange(target) && ComboMenu.GetCheckBoxValue("wUse"))
                {
                    W2.Cast();
                }

                if (target.Distance(Me) < 700 && E2.IsReady() && target.CanMove && !Me.IsInAutoAttackRange(target) && (!Q2.IsInRange(target) || Q2.IsReady()) && ComboMenu.GetCheckBoxValue("eUse"))
                {
                    E2.Cast();
                }
            }
            else
            {
                if (E1.IsInRange(target) && E1.IsReady() && ComboMenu.GetCheckBoxValue("eUse"))
                {
                    var pred = E1.GetPrediction(target);

                    if (pred.HitChancePercent >= 75)
                    {
                        E1.Cast(pred.CastPosition);
                    }
                }

                if (W1.IsInRange(target) && W1.IsReady() && ComboMenu.GetCheckBoxValue("wUse"))
                {
                    W1.Cast(W1.GetPrediction(target).CastPosition);
                }

                if (Q1.IsInRange(target) && Q1.IsReady() && ComboMenu.GetCheckBoxValue("qUse"))
                {
                    Q1.Cast(target);
                }

                if (((!Q1.IsReady() && !W1.IsReady()) || !target.CanMove) && ComboMenu.GetCheckBoxValue("rUse"))
                {
                    R.Cast();
                }
            }
        }

        #region Util

        private static void Escaper()
        {
            if (IsSpider() && E2.IsReady())
            {
                _actionList.Add(() => E2.Cast());
                return;
            }

            if (!IsSpider() && R.IsReady())
            {
                _actionList.Add(() => R.Cast());
                _actionList.Add(() => E2.Cast());
                return;
            }

            if (Zhonya.IsOwned() && Zhonya.IsReady())
            {
                Zhonya.Cast();
            }
        }

        #endregion

        #region Smite

        private static void Smiter()
        {
            if (!PlayerHasSmite || !Smite.IsReady() || Smite == null) return;

            const int sliderSafeDmg = 30;
            Obj_AI_Base getJungleMinion = null;

            if (IsSpider() && Q2.IsReady())
            {
                getJungleMinion = EntityManager.MinionsAndMonsters.GetJungleMonsters()
                    .FirstOrDefault(
                        m =>
                            MonsterSmiteables.Contains(m.BaseSkinName) && m.IsValidTarget(Smite.Range) &&
                            Prediction.Health.GetPrediction(m, Game.Ping) <= (SmiteDamage() + Me.GetSpellDamage(m, SpellSlot.Q)) - sliderSafeDmg &&
                                    SummonerMenu.GetCheckBoxValue("monster" + m.BaseSkinName));
                if (getJungleMinion != null)
                {
                    Q2.Cast(getJungleMinion);
                    Smite.Cast(getJungleMinion);
                }

            }
            else if (!IsSpider() && W1.IsReady())
            {
                getJungleMinion = EntityManager.MinionsAndMonsters.GetJungleMonsters()
                    .FirstOrDefault(
                        m =>
                            MonsterSmiteables.Contains(m.BaseSkinName) && m.IsValidTarget(Smite.Range) &&
                            Prediction.Health.GetPrediction(m, Game.Ping) <=
                            (SmiteDamage() + Me.GetSpellDamage(m, SpellSlot.W)) - sliderSafeDmg &&
                            SummonerMenu.GetCheckBoxValue("monster" + m.BaseSkinName));
                if (getJungleMinion != null)
                {
                    W1.Cast(getJungleMinion.Position);

                    if (R.IsReady())
                    {
                        R.Cast();
                        Core.DelayAction(() => Q2.Cast(getJungleMinion), 50);
                    }

                    Smite.Cast(getJungleMinion);
                }
            }
            else
            {
                getJungleMinion = EntityManager.MinionsAndMonsters.GetJungleMonsters()
                    .FirstOrDefault(
                        m =>
                            MonsterSmiteables.Contains(m.BaseSkinName) && m.IsValidTarget(Smite.Range) &&
                            Prediction.Health.GetPrediction(m, Game.Ping) <= SmiteDamage() - sliderSafeDmg &&
                                    SummonerMenu.GetCheckBoxValue("monster" + m.BaseSkinName));
                if (getJungleMinion != null)
                {
                    Smite.Cast(getJungleMinion);
                }
            }

            if (!SummonerMenu.GetCheckBoxValue("smiteUseOnChampions")) return;

            var smiteGanker = Player.Spells.FirstOrDefault(s => s.Name.ToLower().Contains("playerganker"));

            if (smiteGanker != null)
            {
                var target =
                    EntityManager.Heroes.Enemies.FirstOrDefault(
                        e =>
                            Prediction.Health.GetPrediction(e, Game.Ping) - 5 <= SmiteKsDamage() && e.IsValidTarget(Smite.Range));

                if (target != null)
                {
                    Smite.Cast(target);
                }
            }

            var smiteDuel = Player.Spells.FirstOrDefault(s => s.Name.ToLower().Contains("duel"));

            if (smiteDuel != null)
            {
                var target = TargetSelector.GetTarget(Smite.Range, DamageType.Mixed);

                if (target != null && Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo) && target.HealthPercent <= 60 &&
                    target.IsInAutoAttackRange(Player.Instance))
                {
                    Smite.Cast(target);
                }
            }
        }

        public static Spell.Targeted Smite;
        public static bool PlayerHasSmite = false;

        public static string[] MonsterSmiteables =
        {
             "SRU_Red", "SRU_Blue", "SRU_Dragon_Water",  "SRU_Dragon_Fire", "SRU_Dragon_Earth", "SRU_Dragon_Air", "SRU_Dragon_Elder",
                "SRU_Baron", "SRU_Gromp", "SRU_Murkwolf",
                "SRU_Razorbeak", "SRU_RiftHerald",
                "SRU_Krug", "Sru_Crab", "TT_Spiderboss",
                "TT_NGolem", "TT_NWolf", "TT_NWraith"
        };

        public static string[] MonsterEpic =
        {
            "SRU_Dragon_Water",  "SRU_Dragon_Fire", "SRU_Dragon_Earth", "SRU_Dragon_Air", "SRU_Dragon_Elder",
                "SRU_Baron","TT_Spiderboss"
        };

        public static float SmiteDamage()
        {
            return
                new float[] { 390, 410, 430, 450, 480, 510, 540, 570, 600, 640, 680, 720, 760, 800, 850, 900, 950, 1000 }[
                    Player.Instance.Level];
        }

        public static float SmiteKsDamage()
        {
            return 20 + 8 * Player.Instance.Level;
        }

        #endregion Smite
    }
}
