using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Osu.Utils
{
    /// <summary>
    /// Some icon utilities
    /// </summary>
    public static class IconUtilities
    {
        /// <summary>
        /// Delete object handle from gdi32.dll
        /// </summary>
        /// <param name="handle">the object handle</param>
        /// <returns></returns>
        [DllImport("gdi32.dll", SetLastError = true)]
        private static extern bool DeleteObject(IntPtr handle);

        /// <summary>
        /// Converts an icon to an image source
        /// </summary>
        /// <param name="icon">the icon</param>
        /// <returns>the image source</returns>
        public static ImageSource ToImageSource(this Icon icon)
        {
            Bitmap bitmap = icon.ToBitmap();
            IntPtr bitmap_handle = bitmap.GetHbitmap();

            ImageSource wpf_bitmap = Imaging.CreateBitmapSourceFromHBitmap(bitmap_handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

            if (!DeleteObject(bitmap_handle))
                throw new Win32Exception();

            return wpf_bitmap;
        }
    }
}
