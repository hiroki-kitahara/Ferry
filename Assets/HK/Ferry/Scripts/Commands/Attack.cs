﻿using System;
using HK.Ferry.BattleSystems;
using HK.Ferry.Extensions;
using I2.Loc;
using UniRx;
using UnityEngine;
using UnityEngine.Assertions;
using System.Linq;
using static HK.Ferry.BattleSystems.BattleEvent;
using System.Collections.Generic;

namespace HK.Ferry
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public sealed class Attack : ICommand
    {
        [SerializeField]
        private float rate = 1.0f;

        public IObservable<Unit> Invoke(BattleSystem battleSystem, BattleCharacter attacker, BattleCharacter target)
        {
            return Observable.Defer(() =>
            {
                var damageResult = attacker.GiveDamage(target);
                battleSystem.AddLog(ScriptLocalization.UI.Sentence_Attack.Format(attacker.CurrentSpec.Name, target.CurrentSpec.Name, damageResult.damage));

                var otherStreams = new List<IObservable<Unit>>();

                otherStreams.Add(Observable.Timer(TimeSpan.FromSeconds(1.0f)).AsUnitObservable());

                foreach (var s in attacker.Skills.OfType<IOnGiveDamage>())
                {
                    otherStreams.Add(s.OnGiveDamage(battleSystem, attacker, target));
                }

                foreach (var s in target.Skills.OfType<IOnTakeDamage>())
                {
                    otherStreams.Add(s.OnTakeDamage(battleSystem, attacker, target));
                }

                return otherStreams.Concat().AsSingleUnitObservable();
            });
        }
    }
}
