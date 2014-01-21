﻿using IRemember.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Windows.Devices.Geolocation;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;


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
        private BitmapImage bitmapimage;
        private WriteableBitmap wBitmap;
        private CancellationTokenSource _cts = null;
        private string newCollectionString = "Add new collection...";
        StorageItemAccessList m_futureAccess = StorageApplicationPermissions.FutureAccessList;
        StorageFile file;

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
            loadCollectionComboBox();
        }

        private async void loadCollectionComboBox()
        {
            
            IEnumerable<Data.SampleDataGroup> group = await Data.SampleDataSource.GetGroupsAsync();
            foreach(Data.SampleDataGroup g in group)
            {
                collectionComboBox.Items.Add(g.Title);
            }
            collectionComboBox.Items.Add(newCollectionString);
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            file = (StorageFile)e.Parameter;
            if (file != null)
            {
                bitmapimage = new BitmapImage();
                using (IRandomAccessStream fileStream = await file.OpenAsync(FileAccessMode.Read))
                {
                    bitmapimage.SetSource(fileStream);
                }
            }
            Photo.Source = bitmapimage;

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
             if (newCollectionNameTextBox.Text != "")
                {
                    collectionComboBox.Items.RemoveAt(collectionComboBox.Items.Count - 1);
                    collectionComboBox.Items.Add(newCollectionNameTextBox.Text);
                    collectionComboBox.SelectedItem = newCollectionNameTextBox.Text;
                }
            if (Title.Text == "")
            {
                var msgBox = new MessageDialog("Please enter a title");
                await msgBox.ShowAsync();
            }
            else if (Story.Text == "")
            {
                var msgBox = new MessageDialog("Please enter a description");
                await msgBox.ShowAsync();
            }
             else if (collectionComboBox.SelectedItem == newCollectionString || collectionComboBox.SelectedIndex == -1)
             {
                 var msgBox = new MessageDialog("Please enter a collection name");
                 await msgBox.ShowAsync();
             }

            else if (Title.Text != "" && Story.Text != "") //all included check.
            {
                if (file != null)
                {
                    using (var streamCamera = await file.OpenAsync(FileAccessMode.Read))//openin storage file
                    {

                        BitmapImage bitmapCamera = new BitmapImage();
                        bitmapCamera.SetSource(streamCamera); //setting as bitmap
                        //set file sizes.
                        int width = bitmapCamera.PixelWidth;
                        int height = bitmapCamera.PixelHeight;

                        wBitmap = new WriteableBitmap(width, height);

                        using (var stream = await file.OpenAsync(FileAccessMode.Read))
                        {
                            wBitmap.SetSource(stream);
                        }

                        SaveImageAsJpeg(); //Save picker and save function call {TODO} TURN BACK ON!

                        //get location lol no
                        //getLocation();

                        if(collectionComboBox.SelectedIndex == collectionComboBox.Items.Count-1)
                        {
                                collectionComboBox.Items.RemoveAt(collectionComboBox.Items.Count - 1);
                                collectionComboBox.Items.Add(newCollectionNameTextBox.Text);
                                collectionComboBox.SelectedItem = newCollectionNameTextBox.Text;
                                Data.SampleDataSource.addGroup(new Data.SampleDataGroup(newCollectionNameTextBox.Text, newCollectionNameTextBox.Text, newCollectionNameTextBox.Text, "Assets/LightGray.png", newCollectionNameTextBox.Text), new Data.SampleDataItem(Title.Text, Title.Text, Story.Text, "Assets/LightGray.png", Story.Text, Story.Text));
                        }
                        else
                        {
                            Data.SampleDataSource.addItem(new Data.SampleDataItem(Title.Text, Title.Text, Story.Text, "Assets/LightGray.png", Story.Text, Story.Text), collectionComboBox.SelectedValue.ToString());
                        }

                    }
                }
            
            }

        }

        private async void getLocation()
        {
            _cts = new CancellationTokenSource();
            CancellationToken token = _cts.Token;
            Geolocator locator = new Geolocator();
            Geoposition position = await locator.GetGeopositionAsync().AsTask(token);

            System.Diagnostics.Debug.WriteLine("Test output" + position.Coordinate.Longitude.ToString());
        }

        private async void SaveImageAsJpeg()  //Save picker and save function
        {
            FileSavePicker picker = new FileSavePicker();
            picker.FileTypeChoices.Add("JPG File", new List<string>() { ".jpg" });
            StorageFile file = await picker.PickSaveFileAsync();
            System.Diagnostics.Debug.WriteLine(file.Path);

            if (file != null)
            {
                using (IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.ReadWrite))
                {
                    BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, stream);
                    Stream pixelStream = wBitmap.PixelBuffer.AsStream();
                    byte[] pixels = new byte[pixelStream.Length];
                    await pixelStream.ReadAsync(pixels, 0, pixels.Length);
                    encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore, (uint)wBitmap.PixelWidth, (uint)wBitmap.PixelHeight, 96.0, 96.0, pixels);
                    await encoder.FlushAsync();
                }
            }
        }

        private void newGroup(string uniqueIdNewGroup, string titleNewGroup, string subtitleNewGroup, string imagePathNewGroup, string descriptionNewGroup)
        {
               //Data.SampleDataGroup group = await new Data.SampleDataGroup(uniqueIdNewGroup, titleNewGroup, subtitleNewGroup, imagePathNewGroup, descriptionNewGroup);       
        }

        private void collectionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (collectionComboBox.SelectedItem == newCollectionString)
            {
                newCollectionNameTextBox.Visibility = Visibility.Visible;
            }
        }

        private void newCollectionNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            //collectionComboBox.Item. newCollectionNameTextBox.Text;
        }

        private void newCollectionNameTextBox_KeyUp_1(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
           
        }

    }
}
