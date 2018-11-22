using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static KinectStreams.MainWindow;

namespace KinectStreams {
    public static class Extension {
        static readonly float MAX_DEPTH_DISTANCE = 4095;
        static readonly float MIN_DEPTH_DISTANCE = 850;
        static readonly float MAX_DEPTH_DISTANCE_OFFSET = MAX_DEPTH_DISTANCE - MIN_DEPTH_DISTANCE;

        public static ImageSource ToBitmap(this ColorImageFrame frame) {
            int width, height;
            WriteableBitmap bitmap = null;
            byte[] pixels = null;
            if (bitmap == null) {
                width = frame.Width;
                height = frame.Height;
                PixelFormat format = PixelFormats.Bgr32;
                pixels = new byte[width * height * ((format.BitsPerPixel + 7) / 8)];
                bitmap = new WriteableBitmap(width, height, 96.0, 96.0, PixelFormats.Bgr32, null);
            } else {
                width = 0;
                height = 0;
                Console.Out.WriteLine("Ceci n'a aucun sens");
            }
            frame.CopyPixelDataTo(pixels);
            bitmap.Lock();
            Marshal.Copy(pixels, 0, bitmap.BackBuffer, pixels.Length);
            bitmap.AddDirtyRect(new Int32Rect(0, 0, width, height));
            bitmap.Unlock();
            return bitmap;
        }
        public static ImageSource ToBitmap(this DepthImageFrame frame) {
            int width = frame.Width;
            int height = frame.Height;
            PixelFormat format = PixelFormats.Bgr32;
            int minDepth = frame.MinDepth;
            int maxDepth = frame.MaxDepth;
            short[] pixelData = new short[frame.PixelDataLength];
            byte[] pixels = new byte[width * height * (format.BitsPerPixel + 7) / 8];
            frame.CopyPixelDataTo(pixelData);
            int colorIndex = 0;
            for (int depthIndex = 0; depthIndex < pixelData.Length; ++depthIndex) {
                short depth = pixelData[depthIndex];
                // byte intensity = (byte)(depth >= minDepth && depth <= maxDepth ? depth : 0);
                byte intensity = CalculateIntensityFromDepth(depth);
                pixels[colorIndex++] = intensity; // Blue
                pixels[colorIndex++] = intensity; // Green
                pixels[colorIndex++] = intensity; // Red 
                ++colorIndex;
            }
            int stride = width * format.BitsPerPixel / 8;
            return BitmapSource.Create(width, height, 96, 96, format, null, pixels, stride);
        }
        public static byte CalculateIntensityFromDepth(int distance) {
            // Formula for calculating monochrome intensity
            return (byte)(255 - (255 * Math.Max(distance - MIN_DEPTH_DISTANCE, 0) / (MAX_DEPTH_DISTANCE_OFFSET)));
        }
    }
}
