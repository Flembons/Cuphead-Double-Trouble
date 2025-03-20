using IL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DoubleBosses
{
    public class PlayerStatsManagerChanges
    {
        public void Init()
        {
            On.PlayerStatsManager.CalculateHealthMax += CalculateHealthMax;
            On.PlayerStatsManager.TakeDamage += TakeDamage;
        }

        private void CalculateHealthMax(On.PlayerStatsManager.orig_CalculateHealthMax orig, PlayerStatsManager self)
        {
            self.HealthMax = DoubleBossesManager.db_startingHealth + 1;
            if (self.Loadout.charm == Charm.charm_health_up_1 && !Level.IsChessBoss)
            {
                self.HealthMax += WeaponProperties.CharmHealthUpOne.healthIncrease;
            }
            else if (self.Loadout.charm == Charm.charm_health_up_2 && !Level.IsChessBoss)
            {
                self.HealthMax += WeaponProperties.CharmHealthUpTwo.healthIncrease;
            }
            else if (self.Loadout.charm == Charm.charm_healer && !Level.IsChessBoss)
            {
                self.HealthMax += self.HealerHP;
            }
            else if (self.Loadout.charm == Charm.charm_curse && self.CurseCharmLevel >= 0 && !Level.IsChessBoss)
            {
                self.HealthMax += self.HealerHP;
                self.HealthMax += CharmCurse.GetHealthModifier(self.CurseCharmLevel);
            }
            else if (self.isChalice)
            {
                int healthMax = self.HealthMax;
                self.HealthMax = healthMax + 1;
            }
            if (self.DjimmiInUse())
            {
                self.HealthMax *= 2;
            }
            if (Level.IsInBossesHub)
            {
                PlayersStatsBossesHub playerStats = Level.GetPlayerStats(self.basePlayer.id);
                if (playerStats != null)
                {
                    self.HealthMax += playerStats.BonusHP;
                }
            }
            if (self.HealthMax > 9)
            {
                self.HealthMax = 9;
            }
        }
        private void TakeDamage(On.PlayerStatsManager.orig_TakeDamage orig, PlayerStatsManager self)
        {
            if (self.SuperInvincible)
            {
                return;
            }
            if (self.hardInvincibility)
            {
                return;
            }
            if (Level.Current.Ending)
            {
                return;
            }
            if (self.State != PlayerStatsManager.PlayerState.Ready && (!self.isChalice || self.Loadout.super != Super.level_super_ghost))
            {
                return;
            }
            if (self.StoneTime > 0f)
            {
                self.StoneTime = 0f;
            }
            if (PlayerStatsManager.GlobalInvincibility || PlayerStatsManager.DebugInvincible)
            {
                return;
            }
            if (!DoubleBossesManager.db_infiniteHealth)
            {
                self.Health--;
                PlayersStatsBossesHub playerStats = Level.GetPlayerStats(self.basePlayer.id);
                if (Level.IsInBossesHub && playerStats != null)
                {
                    if (playerStats.BonusHP > 0)
                    {
                        playerStats.LoseBonusHP();
                    }
                    else if (playerStats.healerHP > 0)
                    {
                        playerStats.LoseHealerHP();
                    }
                    self.CalculateHealthMax();
                }
                self.OnHealthChanged();
            }
            if (self.Health < 3)
            {
                Level.ScoringData.numTimesHit++;
            }
            Vibrator.Vibrate(1f, 0.2f, self.basePlayer.id);
            if (self.Health <= 0)
            {
                self.OnStatsDeath();
            }
            else
            {
                self.StartCoroutine(self.hit_cr());
            }
        }
    }
}
