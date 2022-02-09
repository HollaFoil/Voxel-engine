using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Formats.Png;
using System.Reflection;
using System.IO;


namespace Voxel_engine
{

    internal static class TextureUtils
    {
        public static byte[] LoadTexture(string filePath, out int width, out int height)
        {
            Stream imgStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
    "Voxel_engine.Assets.Textures." + filePath);
            using Image<Rgba32> img = Image.Load<Rgba32>(imgStream);
            return GetPixelsFromImage(img, out width, out height);
        }
        public static byte[] LoadTexture(int id, out int width, out int height)
        {
            string filePath = "Assets/Textures/" + NameFromID[id] + ".png";
            Stream imgStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
    "Voxel_engine.Assets.Textures." + NameFromID[id] + ".png");
            using Image<Rgba32> img = Image.Load<Rgba32>(imgStream);
            return GetPixelsFromImage(img, out width, out height);
        }
        private static byte[] GetPixelsFromImage(Image<Rgba32> img, out int width, out int height)
        {
            width = img.Width;
            height = img.Height;
            byte[] pixels = new byte[img.Width*img.Height*4];
            for (int i = 0; i < img.Height; i++)
            {
                var span = img.GetPixelRowSpan(i);
                for (int j = 0; j < span.Length; j++)
                {
                    pixels[i * img.Width * 4 + j * 4] = span[j].R;
                    pixels[i * img.Width * 4 + j * 4 + 1] = span[j].G;
                    pixels[i * img.Width * 4 + j * 4 + 2] = span[j].B;
                    pixels[i * img.Width * 4 + j * 4 + 3] = span[j].A;
                }
            }
            return pixels;
        }
        public static Texture[] CreateTextures()
        {
            Texture[] textures = new Texture[NameFromID.Count];
            for (int i = 0; i < NameFromID.Count; i++)
            {
                textures[i] = new Texture(i, NameFromID[i], LoadTexture(i, out int width, out int height));
            }
            return textures;
        }


        public static Dictionary<int, string> NameFromID = new Dictionary<int, string>
        {
            {0, "map" },
            {1, "cobblestone"},
            {2, "dirt"},
            {3, "grass_block_side"},
            {4, "grass_block_top"},
            {5, "oak_planks"},
            {6, "sand"},
            {7, "stone" },
            {8, "oak_log_side" },
            {9, "oak_log_top" },
            {10, "oak_leaves" }
        };
    }

    public enum ID : int
    {
        Map = 0,
        Cobblestone = 1,
        Dirt = 2,
        GrassSide = 3,
        GrassTop = 4,
        OakPlanks = 5,
        Sand = 6,
        Stone = 7,
        OakLogSide = 8,
        OakLogTop = 9,
        OakLeaves = 10,
    }
}
