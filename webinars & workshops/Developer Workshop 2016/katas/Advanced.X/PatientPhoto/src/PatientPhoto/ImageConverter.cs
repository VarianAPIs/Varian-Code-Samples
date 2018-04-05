using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace PatientPhoto
{
    public class ImageConverter : IValueConverter
    {
        // Source: http://www.codeproject.com/Tips/517457/Simple-Way-to-Bind-an-Image-Class-as-Source-to-Ima
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Image image = value as Image;

            if (image != null)
            {
                MemoryStream ms = new MemoryStream();
                image.Save(ms, image.RawFormat);
                ms.Seek(0, SeekOrigin.Begin);
                BitmapImage bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.StreamSource = ms;
                bmp.EndInit();
                return bmp;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
