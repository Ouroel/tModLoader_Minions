﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace AmuletOfManyMinions.Core.Minions.Effects
{
	public struct ChainDrawer
	{
		public delegate Rectangle FrameSelector(int idx, bool isLast);
		// not customizable enough to warrent an abstract class
		internal Rectangle EvenFrame;
		internal Rectangle OddFrame;
		public FrameSelector frameSelector;

		public ChainDrawer(Rectangle evenFrame, Rectangle oddFrame)
		{
			EvenFrame = evenFrame;
			OddFrame = oddFrame;
			frameSelector = null;
		}
		public ChainDrawer(Rectangle evenFrame)
		{
			EvenFrame = evenFrame;
			OddFrame = evenFrame;
			frameSelector = null;
		}

		public ChainDrawer(FrameSelector selector)
		{
			EvenFrame = default;
			OddFrame = default;
			frameSelector = selector;
		}

		public void DrawChain(SpriteBatch spriteBatch, Texture2D texture, Vector2 startPos, Vector2 endPos, Color lightColor = default)
		{
			Vector2 chainVector = endPos - startPos;
			Rectangle bounds = EvenFrame;
			float drawLength = chainVector.Length();
			Vector2 origin = new Vector2(bounds.Width / 2, bounds.Height / 2);
			Vector2 pos;
			float r;
			if (drawLength > 16)
			{
				Vector2 unitToIdle = chainVector;
				unitToIdle.Normalize();
				r = (float)Math.PI / 2 + chainVector.ToRotation();
				int idx = 0;
				for (int i = bounds.Height / 2; i < drawLength; i += bounds.Height)
				{

					bool isLast = i + bounds.Height >= chainVector.Length();
					if (drawLength - i < bounds.Height / 2)
					{
						i = (int)(drawLength - bounds.Height / 2);
					}
					bounds = frameSelector?.Invoke(idx, isLast) ?? (bounds == EvenFrame ? OddFrame : EvenFrame);
					pos = startPos + unitToIdle * i;
					lightColor = lightColor == default ? Lighting.GetColor((int)pos.X / 16, (int)pos.Y / 16) : lightColor;
					spriteBatch.Draw(texture, pos - Main.screenPosition,
						bounds, lightColor, r,
						origin, 1, SpriteEffects.None, 0);
					idx++;
				}
			}

		}
	}
}
