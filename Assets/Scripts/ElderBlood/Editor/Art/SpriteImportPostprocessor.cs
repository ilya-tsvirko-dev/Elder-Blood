using UnityEditor;
using UnityEngine;

namespace ElderBlood.Editor.Art {
    /// <summary>
    /// Enforces pixel-art import settings so they cannot drift per-asset.
    /// Before this existed the project had Bilinear filtering and compression on
    /// pixel sprites, and PPU values of 16/32/100 mixed inside the same domain.
    /// </summary>
    public class SpriteImportPostprocessor : AssetPostprocessor {
        private void OnPreprocessTexture() {
            if (!SpriteImportRules.TryResolve(assetPath, out var domain)) {
                return;
            }

            var importer = (TextureImporter)assetImporter;
            Apply(importer, domain);
        }

        public static bool Apply(TextureImporter importer, SpriteDomain domain) {
            bool changed = false;

            if (importer.textureType != TextureImporterType.Sprite) {
                importer.textureType = TextureImporterType.Sprite;
                changed = true;
            }

            if (importer.filterMode != FilterMode.Point) {
                importer.filterMode = FilterMode.Point;
                changed = true;
            }

            if (importer.textureCompression != TextureImporterCompression.Uncompressed) {
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                changed = true;
            }

            if (domain.ForceSingleSprite && importer.spriteImportMode != SpriteImportMode.Single) {
                importer.spriteImportMode = SpriteImportMode.Single;
                changed = true;
            }

            if (importer.spritePixelsPerUnit != domain.PixelsPerUnit) {
                importer.spritePixelsPerUnit = domain.PixelsPerUnit;
                changed = true;
            }

            if (importer.mipmapEnabled) {
                importer.mipmapEnabled = false;
                changed = true;
            }

            if (!importer.alphaIsTransparency) {
                importer.alphaIsTransparency = true;
                changed = true;
            }

            if (importer.wrapMode != TextureWrapMode.Clamp) {
                importer.wrapMode = TextureWrapMode.Clamp;
                changed = true;
            }

            if (importer.npotScale != TextureImporterNPOTScale.None) {
                importer.npotScale = TextureImporterNPOTScale.None;
                changed = true;
            }

            // Crunch would quantise colours away from the palette.
            if (importer.crunchedCompression) {
                importer.crunchedCompression = false;
                changed = true;
            }

            return changed;
        }
    }
}
