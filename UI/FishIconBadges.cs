using System;
using TMPro;
using UnityEngine;

namespace BetterFeedingDetails.UI
{
    internal sealed class FishBadgeTextOverlay : MonoBehaviour
    {
        internal TreeNodeRender gender;
        internal TreeNodeRender star;
        internal TreeNodeRender fakeGender;
        internal TreeNodeRender fakeStar;
    }

    internal static class FishIconBadges
    {
        private const string MaleSymbol = "\u2642";
        private const string FemaleSymbol = "\u2640";
        private const string StarSymbol = "\u2605";
        private const float FontSize = 5f;
        private const float StarOffsetX = 5f;
        private const float NodeSize = 8f;

        private static readonly Color MaleColor = new Color32(74, 144, 255, 255);
        private static readonly Color FemaleColor = new Color32(255, 119, 183, 255);
        private static readonly Color StarColor = new Color32(255, 213, 74, 255);

        private static bool logFailed;

        internal static void Sync(GameItemElement element)
        {
            if (element == null || element.handler == null)
            {
                return;
            }

            try
            {
                FishBadgeTextOverlay overlay = EnsureOverlay(element);
                if (!AquacultureHelper.IsFish(element) || element.isDebugMenu)
                {
                    HideAll(overlay);
                    return;
                }

                string sex = element.IsTag("FISH_SEX_TAG") ? element.GetTagReadonly("FISH_SEX_TAG").GetString() : null;
                string genderSymbol = null;
                Color genderColor = MaleColor;
                if (sex == "MALE")
                {
                    genderSymbol = MaleSymbol;
                    genderColor = MaleColor;
                }
                else if (sex == "FEMALE")
                {
                    genderSymbol = FemaleSymbol;
                    genderColor = FemaleColor;
                }

                string starSymbol = AquacultureHelper.GetStage(element) >= FishStage.Adult ? StarSymbol : null;

                Vector2 origin = element.background == null ? Vector2.zero : element.background.rectTransform.anchoredPosition;
                Place(overlay.gender, origin, 0f);
                Apply(overlay.gender, genderSymbol, genderColor);
                Place(overlay.fakeGender, origin, 0f);
                Apply(overlay.fakeGender, genderSymbol, genderColor);

                Place(overlay.star, origin, StarOffsetX);
                Apply(overlay.star, starSymbol, StarColor);
                Place(overlay.fakeStar, origin, StarOffsetX);
                Apply(overlay.fakeStar, starSymbol, StarColor);
            }
            catch (Exception e)
            {
                if (!logFailed)
                {
                    logFailed = true;
                    ModEntry.LogError("fish icon text badge sync failed: " + e);
                }
                HideAll(element.handler.GetComponent<FishBadgeTextOverlay>());
            }
        }

        private static FishBadgeTextOverlay EnsureOverlay(GameItemElement element)
        {
            FishBadgeTextOverlay overlay = element.handler.GetComponent<FishBadgeTextOverlay>();
            if (overlay != null)
            {
                return overlay;
            }

            overlay = element.handler.gameObject.AddComponent<FishBadgeTextOverlay>();
            overlay.gender = CreateTextNode(element.rotator, "BFD Gender Text");
            overlay.star = CreateTextNode(element.rotator, "BFD Star Text");
            if (element.fakeRotator != null)
            {
                overlay.fakeGender = CreateTextNode(element.fakeRotator, "BFD Gender Text (Ghost)");
                overlay.fakeStar = CreateTextNode(element.fakeRotator, "BFD Star Text (Ghost)");
            }
            return overlay;
        }

        private static TreeNodeRender CreateTextNode(TreeNodeRender parent, string name)
        {
            if (parent == null)
            {
                return null;
            }

            TreeNodeRender node = TreeNodeRender.InstantiateText().SetParent(parent);
            node.gameObject.name = name;
            if (node.image != null)
            {
                node.image.enabled = false;
                node.image.raycastTarget = false;
            }
            node.text.raycastTarget = false;
            node.text.fontSize = FontSize;
            node.text.fontStyle = FontStyles.Bold;
            node.text.alignment = TextAlignmentOptions.MidlineLeft;
            node.text.enableWordWrapping = false;
            node.rectTransform.anchorMin = new Vector2(0f, 1f);
            node.rectTransform.anchorMax = new Vector2(0f, 1f);
            node.rectTransform.pivot = new Vector2(0f, 1f);
            node.rectTransform.sizeDelta = new Vector2(NodeSize, NodeSize);
            node.gameObject.SetActive(value: false);
            return node;
        }

        private static void Place(TreeNodeRender node, Vector2 origin, float offsetX)
        {
            if (node == null)
            {
                return;
            }
            node.rectTransform.anchoredPosition = origin + new Vector2(offsetX, 0f);
        }

        private static void Apply(TreeNodeRender node, string symbol, Color color)
        {
            if (node == null)
            {
                return;
            }

            bool visible = !string.IsNullOrEmpty(symbol);
            node.gameObject.SetActive(visible);
            node.text.enabled = visible;
            if (!visible)
            {
                return;
            }

            node.text.text = symbol;
            node.text.color = color;
        }

        private static void HideAll(FishBadgeTextOverlay overlay)
        {
            if (overlay == null)
            {
                return;
            }
            Apply(overlay.gender, null, MaleColor);
            Apply(overlay.star, null, StarColor);
            Apply(overlay.fakeGender, null, MaleColor);
            Apply(overlay.fakeStar, null, StarColor);
        }
    }
}
