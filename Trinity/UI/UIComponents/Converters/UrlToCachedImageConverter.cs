using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using Trinity.Technicals;

namespace Trinity.UIComponents
{
    /// <summary>
    ///     Attempts to use local file in the trinity images folder.
    ///     Otherwise, downloads the image and stores it locally.
    /// </summary>
    public sealed class UriToCachedImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var url = value as string;
            if (url == null)
                return null;

            var webUri = new Uri(url, UriKind.Absolute);
            var filename = Path.GetFileName(webUri.AbsolutePath);
            
            var localFilePath = Path.Combine(FileManager.TrinityImagesPath, filename);

            if (File.Exists(localFilePath))
            {
                Logger.LogVerbose("Found cached image on disk: {0}", filename);
                return BitmapFrame.Create(new Uri(localFilePath, UriKind.Absolute));
            }

            Logger.LogVerbose("Creating Bitmap from url: {0}", value.ToString());

            var image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.UriSource = webUri;
            image.EndInit();

            SaveImage(image, localFilePath);

            return image;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public void SaveImage(BitmapImage image, string localFilePath)
        {
            image.DownloadCompleted += (sender, args) =>
            {
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create((BitmapImage) sender));
                using (var filestream = new FileStream(localFilePath, FileMode.Create))
                {
                    encoder.Save(filestream);
                }
            };
        }
    }
}

