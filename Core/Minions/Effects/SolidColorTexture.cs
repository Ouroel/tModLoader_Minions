﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace AmuletOfManyMinions.Core.Minions.Effects
{
	internal class SolidColorTexture : ModSystem
	{
		private static Dictionary<int, Texture2D> textureCache;

		public override void Load()
		{
			textureCache = new();
		}

		public override void Unload()
		{
			textureCache = null;
		}

		public static Texture2D GetSolidTexture(int projectileId)
		{
			if(textureCache.TryGetValue(projectileId, out var cachedTexture))
			{
				// don't re-process already processed textures
				return cachedTexture;
			}
			Texture2D baseTexture = Terraria.GameContent.TextureAssets.Projectile[projectileId].Value;
			Color[] color = new Color[baseTexture.Width * baseTexture.Height];
			baseTexture.GetData(color);
			for(int i = 0; i < color.Length; i++)
			{
				if(color[i].A > 0)
				{
					color[i] = Color.White with { A = color[i].A };
				}
			}
			Texture2D copyTexture = new(Main.graphics.GraphicsDevice, baseTexture.Width, baseTexture.Height);
			copyTexture.SetData(color);
			textureCache[projectileId] = copyTexture;
			return copyTexture;
		}
	}
}
