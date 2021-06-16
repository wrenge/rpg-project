﻿namespace StatusFX
{
	public static class StatusEffect
	{
		public static IStatusEffect GetDefault(StatusEffectType requiredEffectType)
		{
			return DefaultStatusEffectPool.Instantiate(requiredEffectType);
		}

		public static IReadOnlyStatusEffect GetEmpty(StatusEffectType requiredEffectType)
		{
			return new EmptyStatusEffect(requiredEffectType);
		}
	}
}