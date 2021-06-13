﻿using System.Linq;
using UnityEngine;

namespace StatusFX
{
	[DefaultStatusFX(EnumStatusType.FIRE)]
	public sealed class FireDebuff : BaseGaugeStatusFX
	{
		private const float EXPLOSION_RADIUS = 5;
		private const float STATUS_SPREAD_MULT = 0.4f;
		private const float EXPLOSION_DAMAGE_MULT = 0.4f;
		private const float EXPLOSION_STRENGTH_MULT = 0.4f;
		private const float EXPLOSION_POISE_DAMAGE = 40;
		private const float HYDRO_STRENGTH_MULT = 0.5f;

		public override EnumStatusType statusType => EnumStatusType.FIRE;
		public override bool isDebuff => true;

		public FireDebuff(Character target) : base(target)
		{
		}

		protected override void OnStart()
		{
			if (!TryExplode())
				target.onGaugeTriggered += OnGaugeTriggered;
		}

		protected override void OnUpdate()
		{
			if (!started)
				return;

			target.TakeDamage(new DamageInfo(EnumDamageType.ELEMENTAL, damage * baseDecayRate), Time.deltaTime);
		}

		protected override void OnStop()
		{
			target.onGaugeTriggered -= OnGaugeTriggered;
		}

		private void OnGaugeTriggered(BaseGaugeStatusFX obj)
		{
			TryExplode();
		}

		private bool TryExplode()
		{
			if (!started)
				return false;

			var gauges = target.GetGauges();
			var count = gauges.Count;
			
			var electroStrength = 0f;
			var cryoStrength = 0f;
			var hydroStrength = 0f;
			var totalAmount = amount;
			var totalStrength = strength;
			var statusCount = 1;

			var totalDamage = 0f;
			for (int i = 0; i < count; i++)
			{
				var status = gauges[i];
				if (status.started && status.statusType != statusType)
				{
					totalDamage += status.damage * status.amount * strength;
					totalStrength += status.strength;
					totalAmount += status.amount;
					statusCount++;
					
					switch (status.statusType)
					{
						case EnumStatusType.ELECTRO:
							electroStrength = status.strength;
							break;
						case EnumStatusType.HYDRO:
							hydroStrength = status.strength;
							break;
						case EnumStatusType.CRYO:
							cryoStrength = status.strength;
							break;
					}
				}
			}

			var poiseDamage = EXPLOSION_POISE_DAMAGE * cryoStrength;

			if (totalDamage > 0)
			{
				totalDamage += damage * amount * strength; // Прибавляем сам огонь
				totalDamage += totalDamage * hydroStrength * HYDRO_STRENGTH_MULT; // Прибавляем бонус от воды
				
				for (int i = 0; i < count; i++)
					gauges[i].Clear();
				
				target.TakeDamage(new DamageInfo(EnumDamageType.ELEMENTAL, totalDamage, poiseDamage));

				if (electroStrength > 0)
				{
					ExplodeAOE(totalDamage, totalAmount, totalStrength, statusCount, poiseDamage);
				}

				return true;
			}

			return false;
		}

		private void ExplodeAOE(float totalDamage, float totalAmount, float totalStrength, int statusCount, float poiseDamage)
		{
			var colliders = Physics.OverlapSphere(target.transform.position, EXPLOSION_RADIUS);
			var explosionDamage = totalDamage * EXPLOSION_DAMAGE_MULT;
			var statusAmount = Mathf.Min(totalAmount * STATUS_SPREAD_MULT, 1);
			var explosionStength = totalStrength / statusCount * EXPLOSION_STRENGTH_MULT;

			if (colliders.Length > 0)
			{
				foreach (var collider in colliders)
				{
					var victim = collider.GetComponent<Character>();
					if (victim == null || victim == target)
						continue;

					victim.TakeDamage(new DamageInfo(EnumDamageType.ELEMENTAL, explosionDamage,
						poiseDamage));

					victim.ApplyStatus(new AddStatusInfo(EnumStatusType.FIRE, statusAmount, explosionDamage, explosionStength));
				}
			}
		}
	}
}