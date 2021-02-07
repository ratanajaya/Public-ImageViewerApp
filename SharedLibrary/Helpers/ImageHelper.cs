using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace SharedLibrary
{
    public static class ImageHelper
    {
        public static Bitmap ResizeToResolutionLimit(this Bitmap source, int maxSize) {
            int width, height;

            if(source.Width > source.Height) {
                width = maxSize;
                height = Convert.ToInt32(maxSize * ((float)source.Height / (float)source.Width));
            }
            else {
                width = Convert.ToInt32(maxSize * ((float)source.Width / (float)source.Height));
                height = maxSize;
            }

            Rectangle destRect = new Rectangle(0, 0, width, height);
            Bitmap result = new Bitmap(width, height);

            //WARNING Mutation
            result.SetResolution(source.HorizontalResolution, source.VerticalResolution);
            using(var graphics = Graphics.FromImage(result)) {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using(var wrapMode = new ImageAttributes()) {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(source, destRect, 0, 0, source.Width, source.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return result;
        }

        public static byte[] ToByteArray(this Bitmap source) {
            byte[] result;
            using(var stream = new MemoryStream()) {
                source.Save(stream, ImageFormat.Jpeg);
                result = stream.ToArray();
            }
            return result;
        }
    }
}
