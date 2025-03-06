using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Tools.Utils
{
    public static class IconFactory
    {
        public static void SavePngsAsIcon(Bitmap[] images, Stream outputStream)
        {
            using (var writer = new BinaryWriter(outputStream))
            {
                // 写入ICO文件头
                writer.Write((short)0);      // 保留字段，必须为0
                writer.Write((short)1);      // 图像类型，1=图标，2=光标
                writer.Write((short)images.Length);    // 图像数量

                // 计算每个图像的偏移量
                var imageOffset = 6 + (16 * images.Length);

                // 写入图像目录
                for (var i = 0; i < images.Length; i++)
                {
                    var image = images[i];
                    writer.Write((byte)image.Width);   // 宽度
                    writer.Write((byte)image.Height);  // 高度
                    writer.Write((byte)0);            // 调色板颜色数
                    writer.Write((byte)0);            // 保留字段，必须为0
                    writer.Write((short)1);           // 颜色平面数
                    writer.Write((short)32);          // 每个像素的位数
                    
                    using (var memoryStream = new MemoryStream())
                    {
                        image.Save(memoryStream, ImageFormat.Png);
                        var imageBytes = memoryStream.ToArray();
                        writer.Write((int)imageBytes.Length); // 图像数据大小
                        writer.Write(imageOffset);            // 图像数据偏移量
                        imageOffset += imageBytes.Length;
                    }
                }

                // 写入图像数据
                for (var i = 0; i < images.Length; i++)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        images[i].Save(memoryStream, ImageFormat.Png);
                        writer.Write(memoryStream.ToArray());
                    }
                }
            }
        }
    }
} 