using System;

namespace ElderBlood.Editor.Art {
    /// <summary>
    /// Pixels-per-unit is NOT uniform across the project, and that is deliberate.
    /// Tiles are authored at 16px and characters at 32px, yet both must occupy exactly
    /// one 1x1 grid cell (Grid.cellSize is 1,1 on every level prefab). The only way to
    /// get that is a per-domain PPU. UI lives in a Canvas and keeps the uGUI default.
    /// </summary>
    public readonly struct SpriteDomain {
        public readonly string PathPrefix;
        public readonly int PixelsPerUnit;
        public readonly string Name;

        /// <summary>
        /// Force Single sprite mode. Only set this for folders holding one image per file -
        /// sheets and tilesets are Multiple and their slicing must never be overwritten.
        /// </summary>
        public readonly bool ForceSingleSprite;

        public SpriteDomain(string pathPrefix, int pixelsPerUnit, string name, bool forceSingleSprite = false) {
            PathPrefix = pathPrefix;
            PixelsPerUnit = pixelsPerUnit;
            Name = name;
            ForceSingleSprite = forceSingleSprite;
        }
    }

    public static class SpriteImportRules {
        public const string PaletteAssetPath = "Assets/Art/Palette/ElderBlood.gpl";

        // First match wins - order matters, most specific first.
        private static readonly SpriteDomain[] Domains = {
            new SpriteDomain("Assets/Art/Palette/", 1, "Palette"),
            new SpriteDomain("Assets/Sprites/Enviroment/", 16, "Environment"),
            new SpriteDomain("Assets/Sprites/UI/Abilities/", 100, "Ability Icons", forceSingleSprite: true),
            new SpriteDomain("Assets/Sprites/UI/Hud/", 100, "HUD Plates", forceSingleSprite: true),
            new SpriteDomain("Assets/Sprites/UI/Menu/", 100, "Menu Plates", forceSingleSprite: true),
            new SpriteDomain("Assets/Sprites/UI/", 100, "UI"),
            new SpriteDomain("Assets/Sprites/", 32, "World"),
        };

        public static bool TryResolve(string assetPath, out SpriteDomain domain) {
            for (int i = 0; i < Domains.Length; i++) {
                if (assetPath.StartsWith(Domains[i].PathPrefix, StringComparison.Ordinal)) {
                    domain = Domains[i];
                    return true;
                }
            }

            domain = default;
            return false;
        }

        public static SpriteDomain[] All() {
            return Domains;
        }
    }
}
