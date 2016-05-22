using System;
using System.Collections.Generic;
using EloBuddy;
using EloBuddy.SDK;
using Smart4All.Managers;

namespace Smart4All.Core
{
    public class Brain
    {
        #region Global
        public static string GVersion = "0.0.1";
        public static string GCharname = Player.Instance.ChampionName;

        public static List<Spell.SpellBase> Spells;

        private readonly ActionQueueList comboQueue;
        private readonly ActionQueueList lastHitQueue;
        private readonly ActionQueueList harassQueue;
        private readonly ActionQueueList laneQueue;
        private readonly ActionQueueList jungleQueue;

        private readonly ActionManager actionManager;

        private Orbwalker.ActiveModes activeMode;

        private readonly bool tick;

        #endregion

        public Brain()
        {
            actionManager = new ActionManager();
            comboQueue = new ActionQueueList();
            lastHitQueue = new ActionQueueList();
            harassQueue = new ActionQueueList();
            laneQueue = new ActionQueueList();
            jungleQueue = new ActionQueueList();
        }

        public virtual void Initialize()
        {
            Drawing.OnDraw += OnDraw;
            Game.OnNotify += GameOnOnNotify;
            Orbwalker.OnPostAttack += OnAfterAuto;
            Orbwalker.OnPreAttack += OnBeforeAuto;

            if (tick)
                Game.OnTick += OnTick;
            else
                Game.OnUpdate += OnUpdate;
        }


        #region Drawing

        public virtual void OnDraw(EventArgs args)
        {
            
        }

        #endregion Drawing

        #region Auto-Attack Control

        public virtual void OnBeforeAuto(AttackableUnit target, Orbwalker.PreAttackArgs args)
        {

        }

        public virtual void OnAfterAuto(AttackableUnit target, EventArgs args)
        {
            
        }

        #endregion

        #region Ticks - Update

        public virtual void OnUpdate(EventArgs args)
        {
            PermaActive();
        }

        public virtual void OnTick(EventArgs args)
        {
            PermaActive();
        }

        public virtual void PermaActive()
        {
            if (Player.Instance.IsDead) return;

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo)) OnCombo();
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass)) OnHarass();
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear)) OnLaneClear();
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LastHit)) OnLastHit();
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Flee)) OnFlee();
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear)) OnJungleClear();

        }

        #endregion

        #region Orbwalker Active Modes

        public virtual void OnCombo()
        {
        }

        public virtual void OnHarass()
        {
        }

        public virtual void OnLaneClear()
        {
        }

        public virtual void OnJungleClear()
        {
        }

        public virtual void OnLastHit()
        {
        }

        public virtual void OnFlee()
        {
        }

        #endregion

        #region Useless

        public virtual void GameOnOnNotify(GameNotifyEventArgs args)
        {
            if (args.EventId == GameEventId.OnChampionKill)
            {
                var killer = ObjectManager.GetUnitByNetworkId<Obj_AI_Base>(args.NetworkId);

                if (killer.IsMe)
                {
                    Chat.Say("/masterybadge");
                    EloBuddy.SDK.Core.DelayAction(() => Chat.Say("/l"), 0x3e8);
                }
            }
        }

        #endregion Useless
    }
}
