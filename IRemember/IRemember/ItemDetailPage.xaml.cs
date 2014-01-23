using Bing.Maps;
using IRemember.Common;
using IRemember.Data;
using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.Devices.Geolocation;
using System.Threading;
using System.Threading.Tasks;

// The Item Detail Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234232 lol like we give a crap yolo!

namespace IRemember
{
    /// <summary>
    /// A page that displays details for a single item within a group.
    /// </summary>
    public sealed partial class ItemDetailPage : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        private SampleDataItem item;

        //getlocation
        private Geolocator _geolocator = null;
        private CancellationTokenSource _cts = null;
        private Location location;
        /// <summary>   
        /// NavigationHelper is used on each page to aid in navigation and 
        /// process lifetime management
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        /// <summary>
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        public ItemDetailPage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            _geolocator = new Geolocator();
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session.  The state will be null the first time a page is visited.</param>
        private async void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            // TODO: Create an appropriate data model for your problem domain to replace the sample data
            item = await SampleDataSource.GetItemAsync((String)e.NavigationParameter);
            this.DefaultViewModel["Item"] = item;
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


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {

            navigationHelper.OnNavigatedTo(e);
        
            //getlocation!
            getLocation();

        }

        private async void getLocation()
        {
            _cts = new CancellationTokenSource();
            CancellationToken token = _cts.Token;
            Geolocator locator = new Geolocator();
            Geoposition pos = await locator.GetGeopositionAsync().AsTask(token);
            location = new Location(Double.Parse(item.Longitude) ,Double.Parse(item.Latitude));
            System.Diagnostics.Debug.WriteLine(location.Latitude.ToString() + " " + location.Longitude.ToString());
            double zoomLevel = 13.0f;

            // if we have GPS level accuracy
            if (pos.Coordinate.Accuracy <= 10)
            {
                // Add the 10m icon and zoom closer.
                zoomLevel = 15.0f;
            }
            // Else if we have Wi-Fi level accuracy.
            else if (pos.Coordinate.Accuracy <= 100)
            {
                // Add the 100m icon and zoom a little closer.
                zoomLevel = 14.0f;
            }
            map.SetView(location, zoomLevel);
            Pushpin pushpin = new Pushpin();
            pushpin.Text = "1";
            MapLayer.SetPosition(pushpin, location);
            map.Children.Add(pushpin);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }

        #endregion
    }
}