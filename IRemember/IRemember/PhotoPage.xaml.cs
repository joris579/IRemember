using IRemember.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Storage;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Windows.Storage.AccessCache;


// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace IRemember
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class PhotoPage : Page
    {

        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        private BitmapImage textPassed;
        StorageItemAccessList m_futureAccess = StorageApplicationPermissions.FutureAccessList;
        string m_fileToken;

        /// <summary>
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        /// <summary>
        /// NavigationHelper is used on each page to aid in navigation and 
        /// process lifetime management
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }


        public PhotoPage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            this.navigationHelper.SaveState += navigationHelper_SaveState;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            BitmapImage textPassed = (BitmapImage) e.Parameter;
            Photo.Source = textPassed;

        }
        /// <summary>
        /// Populates the page with content passed during navigation. Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session. The state will be null the first time a page is visited.</param>
        private void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/></param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        #region NavigationHelper registration

        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// 
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="GridCS.Common.NavigationHelper.LoadState"/>
        /// and <see cref="GridCS.Common.NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (Title.Text == "")
            {
                var msgBox = new MessageDialog("Please enter a Title");
                await msgBox.ShowAsync();
            }
            else if (Story.Text == "")
            {
                var msgBox = new MessageDialog("Please enter a Description");
                await msgBox.ShowAsync();
            }
            else if (Title.Text != "" && Story.Text != "")
            {
                try
                {
                    StorageFile inputFile = await m_futureAccess.GetFileAsync(m_fileToken);
                    StorageFile outputFile = await Helpers.GetFileFromSavePickerAsync();
                    Guid encoderId;

                    switch (outputFile.FileType)
                    {
                        case ".png":
                            encoderId = BitmapEncoder.PngEncoderId;
                            break;
                        case ".bmp":
                            encoderId = BitmapEncoder.BmpEncoderId;
                            break;
                        case ".jpg":
                        default:
                            encoderId = BitmapEncoder.JpegEncoderId;
                            break;
                    }

                    using (IRandomAccessStream inputStream = await inputFile.OpenAsync(FileAccessMode.Read),
                               outputStream = await outputFile.OpenAsync(FileAccessMode.ReadWrite))
                    {
                        // BitmapEncoder expects an empty output stream; the user may have selected a
                        // pre-existing file.
                        outputStream.Size = 0;

                        // Get pixel data from the decoder. We apply the user-requested transforms on the
                        // decoded pixels to take advantage of potential optimizations in the decoder.
                        BitmapDecoder decoder = await BitmapDecoder.CreateAsync(inputStream);
                        BitmapTransform transform = new BitmapTransform();

                        // Scaling occurs before flip/rotation, therefore use the original dimensions
                        // (no orientation applied) as parameters for scaling.
                        transform.ScaledHeight = (uint)(decoder.PixelHeight * m_scaleFactor);
                        transform.ScaledWidth = (uint)(decoder.PixelWidth * m_scaleFactor);
                        transform.Rotation = Helpers.ConvertToBitmapRotation(m_userRotation);

                        // Fant is a relatively high quality interpolation mode.
                        transform.InterpolationMode = BitmapInterpolationMode.Fant;

                        // The BitmapDecoder indicates what pixel format and alpha mode best match the
                        // natively stored image data. This can provide a performance and/or quality gain.
                        BitmapPixelFormat format = decoder.BitmapPixelFormat;
                        BitmapAlphaMode alpha = decoder.BitmapAlphaMode;

                        PixelDataProvider pixelProvider = await decoder.GetPixelDataAsync(
                            format,
                            alpha,
                            transform,
                            ExifOrientationMode.RespectExifOrientation,
                            ColorManagementMode.ColorManageToSRgb
                            );

                        byte[] pixels = pixelProvider.DetachPixelData();

                        // Write the pixel data onto the encoder. Note that we can't simply use the
                        // BitmapTransform.ScaledWidth and ScaledHeight members as the user may have
                        // requested a rotation (which is applied after scaling).
                        BitmapEncoder encoder = await BitmapEncoder.CreateAsync(encoderId, outputStream);
                        encoder.SetPixelData(
                            format,
                            alpha,
                            (uint)((double)m_displayWidthNonScaled * m_scaleFactor),
                            (uint)((double)m_displayHeightNonScaled * m_scaleFactor),
                            decoder.DpiX,
                            decoder.DpiY,
                            pixels
                            );

                        await encoder.FlushAsync();

                        rootPage.NotifyUser("Successfully saved a copy: " + outputFile.Name, NotifyType.StatusMessage);
                    }
                }
                catch (Exception err)
                {
                    rootPage.NotifyUser("Error: " + err.Message, NotifyType.ErrorMessage);
                    ResetPersistedState();
                    ResetSessionState();
                }
            }
        }

    }
}
