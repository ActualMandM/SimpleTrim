using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using System;

namespace SparrowTrim
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            DirSearch(args[0]);
        }

        public static void DirSearch(string dir)
        {
            foreach (string d in Directory.GetDirectories(dir))
            {
                DirSearch(d);
            }

            foreach (string f in Directory.GetFiles(dir))
            {
                if (Path.GetExtension(f) != ".png")
                {
                    continue;
                }

                var filePath = Path.Combine(Path.GetDirectoryName(f), Path.GetFileNameWithoutExtension(f));

                if (File.Exists(filePath + ".xml"))
                {
                    try
                    {
                        ProcessImage(filePath);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"{filePath}: {e.Message}");
                    }
                }
            }
        }

        public static void ProcessImage(string file)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(Regex.Replace(File.ReadAllText(file + ".xml"), " name=\".*?\"", ""));

            int padding = Int32.MaxValue;
            int maxX = Int32.MinValue;
            int maxY = Int32.MinValue;

            foreach (XmlNode node in doc.DocumentElement.SelectNodes("SubTexture"))
            {
                padding = Math.Min(padding, (int)Math.Ceiling(float.Parse(node.Attributes["x"].Value)));
                maxX = Math.Max(maxX, (int)Math.Ceiling(float.Parse(node.Attributes["x"].Value)) + (int)Math.Ceiling(float.Parse(node.Attributes["width"].Value)));
                maxY = Math.Max(maxY, (int)Math.Ceiling(float.Parse(node.Attributes["y"].Value)) + (int)Math.Ceiling(float.Parse(node.Attributes["height"].Value)));
            }

            maxX += padding;
            maxY += padding;

            string fileName = file + ".png";

            Bitmap original = new Bitmap(fileName);

            if (maxX >= original.Width && maxY >= original.Height)
            {
                original.Dispose();
                return;
            }

            maxX = Math.Min(maxX, original.Width);
            maxY = Math.Min(maxY, original.Height);

            Bitmap clone = original.Clone(new Rectangle(0, 0, maxX, maxY), original.PixelFormat);
            original.Dispose();

            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }

            clone.Save(fileName, ImageFormat.Png);
            clone.Dispose();
        }
    }
}