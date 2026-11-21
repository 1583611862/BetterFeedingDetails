using System;
using System.Collections.Generic;
using UnityEngine;

namespace BetterFeedingDetails.UI
{
    internal static class FishIconBadges
    {
        private const int BadgeSize = 12;
        private const int StarSize = 10;
        private const int GlyphSize = 8;
        private const int GlyphOffset = 2;
        private const int BadgePriority = 1000;
        private const int StarPriority = 1001;

        private static readonly string[] MaleGlyph =
        {
            "..#..#..",
            ".#....#.",
            "#......#",
            "..####..",
            ".#....#.",
            "#.....#.",
            "..####..",
            "........"
        };

        private static readonly string[] FemaleGlyph =
        {
            ".#....#.",
            ".#....#.",
            ".######.",
            ".#.##.#.",
            ".######.",
            ".#.##.#.",
            ".######.",
            "........"
        };

        private static readonly string[] StarMask =
        {
            "....#.....",
            "....#.....",
            "...###....",
            "#########.",
            ".#######..",
            "..#####...",
            "..##.##...",
            ".##...##..",
            "#.......#.",
            ".........."
        };

        private static readonly Color32 BadgeOutline = new Color32(20, 26, 45, 255);
        private static readonly Color32 MaleFill = new Color32(46, 111, 219, 255);
        private static readonly Color32 MaleLight = new Color32(91, 155, 255, 255);
        private static readonly Color32 MaleDark = new Color32(29, 79, 168, 255);
        private static readonly Color32 FemaleFill = new Color32(232, 90, 166, 255);
        private static readonly Color32 FemaleLight = new Color32(255, 138, 196, 255);
        private static readonly Color32 FemaleDark = new Color32(184, 60, 126, 255);
        private static readonly Color32 StarOutline = new Color32(58, 42, 0, 255);
        private static readonly Color32 StarFill = new Color32(255, 210, 63, 255);
        private static readonly Color32 StarLight = new Color32(255, 241, 168, 255);
        private static readonly Color32 StarDark = new Color32(200, 145, 0, 255);
        private static readonly Color32 GlyphColor = new Color32(255, 255, 255, 255);
        private static readonly Color32 Transparent = new Color32(0, 0, 0, 0);

        private static Sprite maleSprite;
        private static Sprite femaleSprite;
        private static Sprite starSprite;
        private static bool spriteInitFailed;

        internal static void Attach(GameItem item)
        {
            if (item == null)
            {
                return;
            }

            Func<GameItem, List<ItemSpriteModifier>> previous = item.getItemSpriteModifierForItemFunc;
            item.getItemSpriteModifierForItemFunc = delegate(GameItem self)
            {
                List<ItemSpriteModifier> modifiers = new List<ItemSpriteModifier>();
                if (previous != null)
                {
                    List<ItemSpriteModifier> existing = previous(self);
                    if (existing != null)
                    {
                        modifiers.AddRange(existing);
                    }
                }

                try
                {
                    AppendBadges(self, modifiers);
                }
                catch (Exception e)
                {
                    if (!spriteInitFailed)
                    {
                        spriteInitFailed = true;
                        ModEntry.LogError("fish icon badge render failed: " + e);
                    }
                }
                return modifiers;
            };
        }

        private static void AppendBadges(GameItem item, List<ItemSpriteModifier> modifiers)
        {
            if (item == null)
            {
                return;
            }

            string sex = item.IsTag("FISH_SEX_TAG") ? item.GetTagReadonly("FISH_SEX_TAG").GetString() : null;
            Sprite badge = null;
            if (sex == "MALE")
            {
                badge = GetMaleSprite();
            }
            else if (sex == "FEMALE")
            {
                badge = GetFemaleSprite();
            }

            if (badge != null)
            {
                modifiers.Add(new ItemSpriteModifier(badge, Vector2.zero, 0, false, BadgePriority, false, Color.white));
            }

            if (AquacultureHelper.GetStage(item) >= FishStage.Adult)
            {
                Sprite star = GetStarSprite();
                if (star != null)
                {
                    modifiers.Add(new ItemSpriteModifier(star, new Vector2(BadgeSize + 1f, 0f), 0, false, StarPriority, false, Color.white));
                }
            }
        }

        private static Sprite GetMaleSprite()
        {
            if (maleSprite == null)
            {
                maleSprite = CreateBadgeSprite(MaleGlyph, MaleFill, MaleLight, MaleDark);
            }
            return maleSprite;
        }

        private static Sprite GetFemaleSprite()
        {
            if (femaleSprite == null)
            {
                femaleSprite = CreateBadgeSprite(FemaleGlyph, FemaleFill, FemaleLight, FemaleDark);
            }
            return femaleSprite;
        }

        private static Sprite GetStarSprite()
        {
            if (starSprite == null)
            {
                starSprite = CreateStarSprite();
            }
            return starSprite;
        }

        private static Sprite CreateBadgeSprite(string[] glyph, Color32 fill, Color32 light, Color32 dark)
        {
            Color32[] pixels = new Color32[BadgeSize * BadgeSize];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = Transparent;
            }

            for (int y = 0; y < BadgeSize; y++)
            {
                for (int x = 0; x < BadgeSize; x++)
                {
                    bool corner = (x == 0 || x == BadgeSize - 1) && (y == 0 || y == BadgeSize - 1);
                    if (corner)
                    {
                        continue;
                    }

                    bool border = x == 0 || y == 0 || x == BadgeSize - 1 || y == BadgeSize - 1;
                    if (border)
                    {
                        SetPixel(pixels, BadgeSize, x, y, BadgeOutline);
                        continue;
                    }

                    Color32 color = fill;
                    if (y == 1 || x == 1)
                    {
                        color = light;
                    }
                    else if (y == BadgeSize - 2 || x == BadgeSize - 2)
                    {
                        color = dark;
                    }
                    SetPixel(pixels, BadgeSize, x, y, color);
                }
            }

            for (int y = 0; y < GlyphSize; y++)
            {
                string row = glyph[y];
                for (int x = 0; x < GlyphSize; x++)
                {
                    if (row[x] == '#')
                    {
                        SetPixel(pixels, BadgeSize, x + GlyphOffset, y + GlyphOffset, GlyphColor);
                    }
                }
            }

            return CreateSprite(pixels, BadgeSize);
        }

        private static Sprite CreateStarSprite()
        {
            Color32[] pixels = new Color32[StarSize * StarSize];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = Transparent;
            }

            for (int y = 0; y < StarSize; y++)
            {
                for (int x = 0; x < StarSize; x++)
                {
                    if (!IsStarPixel(x, y))
                    {
                        continue;
                    }

                    bool edge = x == 0 || y == 0 || x == StarSize - 1 || y == StarSize - 1 ||
                                !IsStarPixel(x - 1, y) || !IsStarPixel(x + 1, y) ||
                                !IsStarPixel(x, y - 1) || !IsStarPixel(x, y + 1);
                    Color32 color = StarFill;
                    if (edge)
                    {
                        color = StarOutline;
                    }
                    else if (y <= 3 && x <= 5)
                    {
                        color = StarLight;
                    }
                    else if (y >= 7)
                    {
                        color = StarDark;
                    }
                    SetPixel(pixels, StarSize, x, y, color);
                }
            }

            return CreateSprite(pixels, StarSize);
        }

        private static bool IsStarPixel(int x, int y)
        {
            if (x < 0 || y < 0 || x >= StarSize || y >= StarSize)
            {
                return false;
            }
            return StarMask[y][x] == '#';
        }

        private static void SetPixel(Color32[] pixels, int size, int x, int y, Color32 color)
        {
            pixels[(size - 1 - y) * size + x] = color;
        }

        private static Sprite CreateSprite(Color32[] pixels, int size)
        {
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.SetPixels32(pixels);
            texture.Apply(false, false);
            return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0f, 1f), 1f);
        }
    }
}
