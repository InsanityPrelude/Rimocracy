﻿using RimWorld;
using System.Collections.Generic;
using Verse.AI;

namespace Rimocracy
{
    class JobDriver_Govern : JobDriver
    {
        bool isSitting = false;

        public override bool TryMakePreToilReservations(bool errorOnFailed) => pawn.Reserve(TargetThingA, job);

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
            if (TargetThingA.def.building != null && TargetThingA.def.building.isSittable)
            {
                isSitting = true;
                yield return Toils_General.Do(delegate
                {
                    job.SetTarget(TargetIndex.B, TargetThingA.InteractionCell + TargetThingA.Rotation.FacingCell);
                });
            }
            Toil governToil = new Toil();
            governToil.tickAction = Govern_TickAction;
            governToil.FailOn(() => Utility.RimocracyComp.Governance >= 1);
            governToil.FailOn(() => !pawn.IsLeader());
            governToil.FailOnCannotTouch(TargetIndex.A, PathEndMode.InteractionCell);
            governToil.defaultCompleteMode = ToilCompleteMode.Delay;
            governToil.defaultDuration = GenDate.TicksPerHour * 2;
            governToil.activeSkill = () => SkillDefOf.Social;
            yield return governToil;
            yield return Toils_General.Wait(2, TargetIndex.None);
            yield break;
        }

        void Govern_TickAction()
        {
            if (isSitting)
                rotateToFace = TargetIndex.B;
            Utility.RimocracyComp.ImproveGovernance(
                pawn.GetStatValue(RimocracyDefOf.GovernEfficiency)
                * TargetA.Thing.GetStatValue(RimocracyDefOf.GovernEfficiencyFactor)
                / GenDate.TicksPerHour);
            pawn.skills.Learn(SkillDefOf.Intellectual, 0.05f);
            pawn.skills.Learn(SkillDefOf.Social, 0.05f);
            pawn.GainComfortFromCellIfPossible(true);
        }
    }
}
