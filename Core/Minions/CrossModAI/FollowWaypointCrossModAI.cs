﻿using AmuletOfManyMinions.Core.Minions.AI;
using AmuletOfManyMinions.Projectiles.Minions;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace AmuletOfManyMinions.Core.Minions.CrossModAI
{
	internal class FollowWaypointCrossModAI : ICrossModSimpleMinion
	{

		public Projectile Projectile { get; set; }

		public Player Player => Main.player[Projectile.owner];
		public int BuffId { get; set; }
		public SimpleMinionBehavior Behavior { get; set; }

		private int MaxSpeed { get; set; }
		private int Inertia { get; set; }
		private int SearchRange { get; set; }

		/// <summary>
		/// Cache the projectile's velocity
		/// </summary>
		private Vector2 CachedVelocity { get; set; }
		private Vector2 CachedPosition { get; set; }

		public FollowWaypointCrossModAI(Projectile projectile, int buffId, int maxSpeed, int inertia = 8, int searchRange = 600)
		{
			Projectile = projectile;
			BuffId = buffId;
			MaxSpeed = maxSpeed;
			Inertia = inertia;
			SearchRange = searchRange;
			
			Behavior = new(this);
			Behavior.Player = Player;
		}

		public WaypointMovementStyle WaypointMovementStyle => WaypointMovementStyle.IDLE;

		public void IdleMovement(Vector2 vectorToIdlePosition)
		{
			// Only take over idle movement if pathfinding
			if(!Behavior.IsFollowingBeacon) { return;  }

			if(vectorToIdlePosition.LengthSquared() > MaxSpeed * MaxSpeed)
			{
				vectorToIdlePosition.Normalize();
				vectorToIdlePosition *= MaxSpeed;
			}

			Projectile.velocity = (Projectile.velocity * (Inertia - 1) + vectorToIdlePosition) / Inertia;
			CachedVelocity = Projectile.velocity;
			CachedPosition = Projectile.position;
		}
		
		public Vector2? FindTarget()
		{
			// If we're at the edge of the beacon, break control of the projectile
			// as soon as an enemy comes in range
			if(!Behavior.Pathfinder.InTransit)
			{
				return Behavior.AnyEnemyInRange(SearchRange);
			}
			return default;
		}

		public void PostAI()
		{
			if(!Behavior.IsFollowingBeacon) { return; }

			Projectile.velocity = CachedVelocity;
			Projectile.position = CachedPosition;
		}

		public void AfterMoving() 
		{
			// no op
		}

		public bool DoVanillaAI() => true;

		public Vector2 IdleBehavior()
		{
			// no op
			return default;
		}


		public bool MinionContactDamage() => false;

		public void TargetedMovement(Vector2 vectorToTargetPosition)
		{
			// No op
		}
	}
}
