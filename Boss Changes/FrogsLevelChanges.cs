using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DoubleBosses.Boss_Changes
{
    internal class FrogsLevelChanges
    {
        public static int shortFrogCheck;
        public static int tallFrogCheck;

        public void Init()
        {
            tallFrogCheck = 0;
            shortFrogCheck = 0;
            On.FrogsLevelTall.Awake += Awake;
            On.FrogsLevelShort.Awake += Awake;
            On.FrogsLevelShort.StartMorph += StartMorph;
            On.FrogsLevelMorphed.Enable += Enable;
        }

        protected void Awake(On.FrogsLevelTall.orig_Awake orig, FrogsLevelTall self)
        {
            orig(self);
            if (tallFrogCheck == 1)
            {
                var tallFrogs = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.name == "Frog_Tall").ToArray();
                tallFrogs[0].name = "Frog_Tall_0";
                tallFrogs[tallFrogs.Length - 1].name = "Frog_Tall_1";
                tallFrogCheck = 0;
            }
            else
            {
                tallFrogCheck++;
            }
        }

        protected void Awake(On.FrogsLevelShort.orig_Awake orig, FrogsLevelShort self)
        {
            orig(self);

            if (shortFrogCheck == 1)
            {
                var shortFrogs = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.name == "Frog_Short").ToArray();
                var morphedFrogs = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.name == "Frog_Morphed").ToArray();
                shortFrogs[0].name = "Frog_Short_0";
                shortFrogs[shortFrogs.Length - 1].name = "Frog_Short_1";
                morphedFrogs[0].name = "Frog_Morphed_0";
                morphedFrogs[morphedFrogs.Length - 1].name = "Frog_Morphed_1";
                shortFrogCheck = 0;
            }
            else
            {
                shortFrogCheck++;
            }
        }

        public void StartMorph(On.FrogsLevelShort.orig_StartMorph orig, FrogsLevelShort self)
        {
            self.StopAllCoroutines();
            self.animator.Play("Idle");
            self.state = FrogsLevelShort.State.Morphing;

            // before rolling, make sure the current frog is the correct frog associated with self short frog
            var tallFrogs = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.name.StartsWith("Frog_Tall")).ToArray();
            if (tallFrogs.Length > 1)
            {
                var morphedFrogs = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.name.StartsWith("Frog_Morphed")).ToArray();
                if (self.name.Equals("Frog_Short_0"))
                {
                    FrogsLevelTall.Current = tallFrogs[0].GetComponent<FrogsLevelTall>();
                    FrogsLevelMorphed.Current = morphedFrogs[0].GetComponent<FrogsLevelMorphed>();
                }
                else
                {
                    FrogsLevelTall.Current = tallFrogs[1].GetComponent<FrogsLevelTall>();
                    FrogsLevelMorphed.Current = morphedFrogs[1].GetComponent<FrogsLevelMorphed>();
                }
            }
            self.StartCoroutine(self.morphRoll_cr());
        }

        public void Enable(On.FrogsLevelMorphed.orig_Enable orig, FrogsLevelMorphed self, bool demonTriggered)
        {
            FrogsLevel.FINAL_FORM = false;
            orig(self, demonTriggered);
        }
    }
}
