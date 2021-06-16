﻿using System.Reflection;

namespace StatusFX.Elemental
{
	[DefaultStatusEffect(StatusEffectType.Cryo, true)]
	internal sealed class CryoDebuff : ElementalDebuff
	{
		private const float PoiseDebuff = 10;
		private float _appliedAmount;

		protected override void OnStart()
		{
			_appliedAmount = PoiseDebuff * Strength;
			Target.PoiseDamageDebuff += _appliedAmount;
		}

		protected override void OnStop()
		{
			Target.PoiseDamageDebuff -= _appliedAmount;
			_appliedAmount = 0;
		}
	}
}