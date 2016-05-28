using System;
using System.Collections.Generic;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using SharpDX;
using Smart4All.Core;
using Smart4All.Util;
using Color = System.Drawing.Color;

namespace Smart4All.Plugin.Champion
{
    public class Caitlyn : Brain
    {
        #region Spells

        public static Spell.Skillshot Q { get; private set; }
        public static Spell.Skillshot W { get; private set; }
        public static Spell.Skillshot E { get; private set; }
        public static Spell.Targeted R { get; private set; }

        static void initSpell()
        {
            // Initialize spells
            Q = new Spell.Skillshot(SpellSlot.Q, 1250, SkillShotType.Linear, 1000, 1400, 65) { AllowedCollisionCount = int.MaxValue };
            W = new Spell.Skillshot(SpellSlot.W, 800, SkillShotType.Circular, 250, 1400, 67) { AllowedCollisionCount = int.MaxValue };
            E = new Spell.Skillshot(SpellSlot.E, 950, SkillShotType.Linear, 500, 1300, 70) { AllowedCollisionCount = 0 };
            R = new Spell.Targeted(SpellSlot.R, (uint)(1500 + Player.Instance.Level * 500));
        }
        #endregion

        #region BuffRecognition

        private readonly string PassiveReady = "CaitlynHeadShotReady";
        private readonly string PassiveSpellReady = "caitlynheadshotrangecheck";

        #endregion

        private static readonly string MenuName = "Smart4All - " + Player.Instance.ChampionName;

        private static readonly Menu Menu;

        static Caitlyn()
        {
            Menu = MainMenu.AddMenu(MenuName, MenuName.ToLower());
            Menu.AddGroupLabel(Player.Instance.ChampionName + " Plugin");
            Menu.AddLabel("By MrArticuno");

            Modes.Initialize();
            initSpell();

            permaHarass = Modes.Harass.AutoHarass;

            Gapcloser.OnGapcloser += GapcloserOnOnGapcloser;

            // Disabled while event is broken
            Dash.OnDash += delegate(Obj_AI_Base sender, Dash.DashEventArgs args)
            {
                if(sender == null || !sender.IsValidTarget() || args == null) return;

                if (Modes.Misc.AutoQOnDash && Q.IsReady() && sender.IsValidTarget(Q.Range))
                {
                    Q.Cast(args.EndPos);
                }
            };
        }

        #region Drawing

        public override void OnDraw(EventArgs args)
        {

            if (Q.IsReady() && Modes.Draw.UseQ)
            {
                Circle.Draw(SharpDX.Color.Blue, Q.Range, Player.Instance);
            }

            if (W.IsReady() && Modes.Draw.UseW)
            {
                Circle.Draw(SharpDX.Color.Blue, W.Range, Player.Instance);
            }

            if (E.IsReady() && Modes.Draw.UseE)
            {
                Circle.Draw(SharpDX.Color.Blue, E.Range, Player.Instance);
            }

            if (R.IsReady() && Modes.Draw.UseR)
            {
                Circle.Draw(SharpDX.Color.Blue, R.Range, Player.Instance);
            }

        }

        #endregion

        #region Util

        private static void GapcloserOnOnGapcloser(AIHeroClient sender, Gapcloser.GapcloserEventArgs gapcloserEventArgs)
        {
            if (sender.IsAlly || !sender.IsValid()) return;

            if ((gapcloserEventArgs.Target != null && gapcloserEventArgs.Target == Player.Instance) || gapcloserEventArgs.End.Distance(Player.Instance) <= 150)
            {
                if (W.IsReady() && W.IsInRange(sender) && Modes.Misc.AntiGapW) W.Cast(Player.Instance.ServerPosition);

                if (E.IsReady() && E.IsInRange(sender) && Modes.Misc.AntiGapE)
                {
                    E.Cast(E.GetPrediction(sender).CastPosition);

                    if (Q.IsReady() && Modes.Misc.AntiGapQ) EloBuddy.SDK.Core.DelayAction(() => Q.Cast(Q.GetPrediction(sender).CastPosition), 250);
                }
            }

        }

        #endregion

        #region Orbwalker Modes

        public override void OnCombo()
        {
            base.OnCombo();

            CanCastR();

            var target = TargetSelector.GetTarget(Q.Range, DamageType.Physical);

            if (target == null || !target.IsValidTarget())
            {
                return;
            }

            if (CanCastQ(target))
            {
                Q.Cast(Q.GetPrediction(target).CastPosition);
            }

            if (CanCastE(target))
            {
                E.Cast(E.GetPrediction(target).CastPosition);
                actionManager.EnqueueAction(
                    comboQueue,
                    () => true,
                    () => Q.Cast(Q.GetPrediction(target).CastPosition),
                    () => true);
            }

            if (CanCastW(target))
            {
                W.Cast(W.GetPrediction(target).CastPosition);
            }
        }

        public override void OnHarass()
        {
            base.OnHarass();

            var target = TargetSelector.GetTarget(Q.Range, DamageType.Physical);

            if (target == null || !target.IsValidTarget())
            {
                return;
            }

            if (CanCastQ(target))
            {
                Q.Cast(Q.GetPrediction(target).CastPosition);
            }

            if (CanCastE(target))
            {
                E.Cast(E.GetPrediction(target).CastPosition);

                actionManager.EnqueueAction(
                    comboQueue,
                    () => CanCastQ(target),
                    () => Q.Cast(Q.GetPrediction(target).CastPosition),
                    () => true);
            }

            if (CanCastW(target))
            {
                W.Cast(W.GetPrediction(target).CastPosition);
            }
        }

        public override void OnJungleClear()
        {
            base.OnJungleClear();

            if (!Modes.JungleClear.Mana) return;

            var bestpos = FarmUtil.GetBestLineFarmLocation(EntityManager.MinionsAndMonsters.Monsters.Where(m => Q.IsInRange(m) && !m.IsDead).Select(o => o.ServerPosition.To2D()).ToList(), Q.Width, Q.Range);

            if (Modes.JungleClear.UseQ && Q.IsReady() && bestpos.MinionsHit >= 3)
            {
                Q.Cast(bestpos.Position.To3D());
            }

            var nearMinion = EntityManager.MinionsAndMonsters.Monsters.FirstOrDefault(t => t.Distance(Player.Instance) < 300);

            if (nearMinion == null || !nearMinion.IsValidTarget(E.Range) || E.IsReady() || Modes.JungleClear.UseE) return;

            E.Cast(nearMinion.ServerPosition);
        }

        public override void OnLaneClear()
        {
            base.OnLaneClear();

            if (!Modes.LaneClear.Mana) return;

            var bestpos = FarmUtil.GetBestLineFarmLocation(EntityManager.MinionsAndMonsters.EnemyMinions.Where(m => Q.IsInRange(m) && !m.IsDead).Select(o => o.ServerPosition.To2D()).ToList(), Q.Width, Q.Range);

            if (Modes.LaneClear.UseQ && Q.IsReady() && bestpos.MinionsHit >= 3)
            {
                Q.Cast(bestpos.Position.To3D());
            }

        }

        public override void PermaActive()
        {
            base.PermaActive();

            if (Modes.Misc.AutoWOnImmobile)
            {
                foreach (var target in EntityManager.Heroes.Enemies.Where(t => t != null && t.IsValidTarget(W.Range)))
                {
                    if (!target.CanMove && W.IsReady())
                    {
                        W.Cast(W.GetPrediction(target).CastPosition);
                    }
                }
            }

            if (Modes.Misc.AutoQImmobile)
            {
                foreach (var target in EntityManager.Heroes.Enemies.Where(t => t != null && t.IsValidTarget(Q.Range)))
                {
                    if (!target.CanMove && Q.IsReady())
                    {
                        Q.Cast(Q.GetPrediction(target).CastPosition);
                    }
                }
            }
        }

        #endregion

        #region Spell Logic Combo

        static void CanCastR()
        {
            var target = TargetSelector.GetTarget(R.Range, DamageType.Physical);

            if (Modes.Combo.UseR && R.IsReady())
            {
                if (target != null && target.IsValidTarget(R.Range) && !Player.Instance.IsInAutoAttackRange(target))
                {
                    var dmg = Player.Instance.CalculateDamageOnUnit(target, DamageType.Physical,
                        (new float[] { 0, 250, 475, 700 }[R.Level] + (2f * Player.Instance.FlatMagicDamageMod))) - target.FlatPhysicalReduction;

                    if (dmg > target.Health) R.Cast(target);
                }
            }
        }

        static bool CanCastQ(Obj_AI_Base target)
        {
            if (((Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo) && Modes.Combo.UseQ) || (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass) && Modes.Harass.UseQ)) && Q.IsReady())
            {
                if (target != null && target.IsValidTarget(Q.Range) && !Player.Instance.IsInAutoAttackRange(target))
                {
                    var predict = Q.GetPrediction(target);

                    if (predict.HitChancePercent >= 70)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        static bool CanCastW(Obj_AI_Base target)
        {
            if (((Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo) && Modes.Combo.UseW) || (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass) && Modes.Harass.UseW)) && W.IsReady())
            {
                if (target != null && target.IsValidTarget(W.Range))
                {
                    if (target.IsMelee && target.IsFacing(Player.Instance))
                    {
                        return true;
                    }
                    if (!target.CanMove)
                    {
                        return true;
                    }
                    if (target.IsFacing(Player.Instance) && target.IsInAutoAttackRange(Player.Instance))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        static bool CanCastE(Obj_AI_Base target)
        {
            if (((Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo) && Modes.Combo.UseE) || (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass) && Modes.Harass.UseE)) && E.IsReady())
            {
                if (!Modes.Misc.CheckSafePosForDash)
                {
                    if (target != null && target.IsValidTarget(E.Range))
                    {
                        if (target.IsMelee && target.IsFacing(Player.Instance) &&
                            target.IsInAutoAttackRange(Player.Instance))
                        {
                            return true;
                        }
                        if (target.IsInAutoAttackRange(Player.Instance) && Player.Instance.HealthPercent <= 30)
                        {
                            return true;
                        }
                        if (target.IsFacing(Player.Instance) && (target.IsInAutoAttackRange(Player.Instance) || target.Distance(Player.Instance) <= 250))
                        {
                            return true;
                        }
                    }
                }
                else
                {
                    if (target != null && target.IsValidTarget(E.Range))
                    {
                        if (target.IsMelee && target.IsFacing(Player.Instance) &&
                            target.IsInAutoAttackRange(Player.Instance))
                        {
                            var predict = E.GetPrediction(target);

                            if (predict.HitChancePercent >= 70)
                            {
                                var futurePos = Player.Instance.ServerPosition.Extend(predict.CastPosition, 400);

                                if (futurePos.CountEnemiesInRange(Player.Instance.GetAutoAttackRange()) <= futurePos.CountAlliesInRange(Player.Instance.GetAutoAttackRange()))
                                    return true;
                            }
                        }
                        else if (target.IsInAutoAttackRange(Player.Instance) && Player.Instance.HealthPercent <= 30)
                        {
                            var predict = E.GetPrediction(target);
                            var futurePos = Player.Instance.ServerPosition.Extend(predict.CastPosition, 400);

                            if (futurePos.CountEnemiesInRange(Player.Instance.GetAutoAttackRange()) <=
                                futurePos.CountAlliesInRange(Player.Instance.GetAutoAttackRange()))
                                return true;
                        }
                    }
                }
            }
            return false;
        }

        #endregion

        #region Menu Stuff D:
        public static class Modes
        {
            private static readonly Menu Menu;

            static Modes()
            {
                Menu = Caitlyn.Menu.AddSubMenu("Drawing");
                Draw.Initialize();

                Menu = Caitlyn.Menu.AddSubMenu("Modes");

                Combo.Initialize();
                Menu.AddSeparator();

                Harass.Initialize();
                Menu.AddSeparator();

                LaneClear.Initialize();
                Menu.AddSeparator();

                JungleClear.Initialize();
                Menu.AddSeparator();

                Menu = Caitlyn.Menu.AddSubMenu("Miscellanea");

                Misc.Initialize();
            }

            public static void Initialize()
            {
            }

            public static class Draw
            {
                private static readonly CheckBox _drawQ;
                private static readonly CheckBox _drawW;
                private static readonly CheckBox _drawE;
                private static readonly CheckBox _drawR;
                private static readonly CheckBox _drawKillableR;

                public static bool UseQ
                {
                    get { return _drawQ.CurrentValue; }
                }
                public static bool UseW
                {
                    get { return _drawW.CurrentValue; }
                }
                public static bool UseE
                {
                    get { return _drawE.CurrentValue; }
                }
                public static bool UseR
                {
                    get { return _drawR.CurrentValue; }
                }

                public static bool DrawRKillable
                {
                    get { return _drawKillableR.CurrentValue; }
                }

                static Draw()
                {
                    Menu.AddGroupLabel("Draw");
                    _drawQ = Menu.Add("draw.Q", new CheckBox("Draw Q"));
                    _drawW = Menu.Add("draw.W", new CheckBox("Draw W"));
                    _drawE = Menu.Add("draw.E", new CheckBox("Draw E"));
                    _drawR = Menu.Add("draw.R", new CheckBox("Draw R"));
                    _drawR = Menu.Add("draw.R.Killable", new CheckBox("Draw R Killable"));
                }

                public static void Initialize()
                {
                }
            }

            public static class Combo
            {
                private static readonly CheckBox _useQ;
                private static readonly CheckBox _useW;
                private static readonly CheckBox _useE;
                private static readonly CheckBox _useR;

                public static bool UseQ
                {
                    get { return _useQ.CurrentValue; }
                }
                public static bool UseW
                {
                    get { return _useW.CurrentValue; }
                }
                public static bool UseE
                {
                    get { return _useE.CurrentValue; }
                }
                public static bool UseR
                {
                    get { return _useR.CurrentValue; }
                }

                static Combo()
                {
                    Menu.AddGroupLabel("Combo");
                    _useQ = Menu.Add("comboUseQ", new CheckBox("Use Q"));
                    _useW = Menu.Add("comboUseW", new CheckBox("Use W"));
                    _useE = Menu.Add("comboUseE", new CheckBox("Use E"));
                    _useR = Menu.Add("comboUseR", new CheckBox("Use R"));
                }

                public static void Initialize()
                {
                }
            }

            public static class Harass
            {
                private static readonly CheckBox _useQ;
                private static readonly CheckBox _useW;
                private static readonly CheckBox _useE;
                private static readonly KeyBind _autoHarass;
                private static readonly Slider _manaPercent;

                public static bool UseQ
                {
                    get { return _useQ.CurrentValue; }
                }
                public static bool UseW
                {
                    get { return _useW.CurrentValue; }
                }
                public static bool UseE
                {
                    get { return _useE.CurrentValue; }
                }
                public static bool AutoHarass
                {
                    get { return _autoHarass.CurrentValue; }
                }
                public static bool Mana
                {
                    get { return Player.Instance.ManaPercent > _manaPercent.CurrentValue; }
                }

                static Harass()
                {
                    Menu.AddGroupLabel("Harass");
                    _useQ = Menu.Add("harass.q", new CheckBox("Use Q"));
                    _useW = Menu.Add("harass.w", new CheckBox("Use W"));
                    _useE = Menu.Add("harass.e", new CheckBox("Use E"));
                    _autoHarass = Menu.Add("harass.auto", new KeyBind("Auto Harass", false, KeyBind.BindTypes.PressToggle));
                    _autoHarass.OnValueChange += delegate
                    {
                        permaHarass = _autoHarass.CurrentValue;
                    };
                    _manaPercent = Menu.Add("harass.mana", new Slider("Min Mana %", 45, 0, 100));
                }

                public static void Initialize()
                {
                }
            }

            public static class LaneClear
            {
                private static readonly CheckBox _useQ;
                private static readonly Slider _manaPercent;

                public static bool UseQ
                {
                    get { return _useQ.CurrentValue; }
                }
                public static bool Mana
                {
                    get { return Player.Instance.ManaPercent > _manaPercent.CurrentValue; }
                }

                static LaneClear()
                {
                    Menu.AddGroupLabel("Lane Clear");
                    _useQ = Menu.Add("laneclear.q", new CheckBox("Use Q"));
                    _manaPercent = Menu.Add("laneclear.mana", new Slider("Min Mana %", 45, 0, 100));
                }

                public static void Initialize()
                {
                }
            }

            public static class JungleClear
            {
                private static readonly CheckBox _useQ;
                private static readonly CheckBox _useE;
                private static readonly Slider _manaPercent;

                public static bool UseQ
                {
                    get { return _useQ.CurrentValue; }
                }
                public static bool UseE
                {
                    get { return _useE.CurrentValue; }
                }
                public static bool Mana
                {
                    get { return Player.Instance.ManaPercent > _manaPercent.CurrentValue; }
                }

                static JungleClear()
                {
                    Menu.AddGroupLabel("Jungle Clear");
                    _useQ = Menu.Add("jungleclear.q", new CheckBox("Use Q"));
                    _useE = Menu.Add("jungleclear.e", new CheckBox("Use E"));
                    _manaPercent = Menu.Add("jungleclear.mana", new Slider("Min Mana %", 45, 0, 100));
                }

                public static void Initialize()
                {
                }
            }

            public static class Misc
            {
                private static readonly CheckBox _checkSafePosForDash;
                private static readonly CheckBox _autoQImmobile;
                private static readonly CheckBox _autoQOnDash;
                private static readonly CheckBox _autoWOnImmobile;
                private static readonly CheckBox _antiGapCloserQ;
                private static readonly CheckBox _antiGapCloserW;
                private static readonly CheckBox _antiGapCloserE;

                public static bool CheckSafePosForDash
                {
                    get { return _checkSafePosForDash.CurrentValue; }
                }

                public static bool AutoQImmobile
                {
                    get { return _autoQImmobile.CurrentValue; }
                }

                public static bool AutoQOnDash
                {
                    get { return _autoQOnDash.CurrentValue; }
                }

                public static bool AutoWOnImmobile
                {
                    get { return _autoWOnImmobile.CurrentValue; }
                }

                public static bool AntiGapQ
                {
                    get { return _antiGapCloserQ.CurrentValue; }
                }

                public static bool AntiGapW
                {
                    get { return _antiGapCloserW.CurrentValue; }
                }

                public static bool AntiGapE
                {
                    get { return _antiGapCloserE.CurrentValue; }
                }

                static Misc()
                {
                    Menu.AddGroupLabel("Misc");
                    _checkSafePosForDash = Menu.Add("misc.check.e", new CheckBox("Check Safe pos before E"));
                    _autoQOnDash = Menu.Add("misc.dash.q", new CheckBox("Use Q on Dash"));
                    _autoQImmobile = Menu.Add("misc.immobile.q", new CheckBox("Use Q on Immobile"));
                    _autoWOnImmobile = Menu.Add("misc.immobile.W", new CheckBox("Use W on Immobile"));
                    _antiGapCloserQ = Menu.Add("misc.antigap.q", new CheckBox("Q for anti gap closers"));
                    _antiGapCloserW = Menu.Add("misc.antigap.w", new CheckBox("W for anti gap closers", false));
                    _antiGapCloserE = Menu.Add("misc.antigap.e", new CheckBox("E for anti gap closers", false));
                }

                public static void Initialize()
                {
                }
            }
        }
        #endregion
    }
}
