using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Storage;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

// The data model defined by this file serves as a representative example of a strongly-typed
// model.  The property names chosen coincide with data bindings in the standard item templates.
//
// Applications may use this model as a starting point and build on it, or discard it entirely and
// replace it with something appropriate to their needs. If using this model, you might improve app 
// responsiveness by initiating the data loading task in the code behind for App.xaml when the app 
// is first launched.

namespace IRemember.Data
{
    /// <summary>
    /// Generic item data model.
    /// </summary>
    public class SampleDataItem
    {
        public SampleDataItem(String uniqueId, String title, String subtitle, String imagePath, String description, String content, String longitude, String latidude)
        {
            this.UniqueId = uniqueId;
            this.Title = title;
            this.Subtitle = subtitle;
            this.Description = description;
            this.ImagePath = imagePath;
            this.Content = content;
            this.Longitude = longitude;
            this.Latitude = latidude;
        }

        public string UniqueId { get; private set; }
        public string Title { get; private set; }
        public string Subtitle { get; private set; }
        public string Description { get; private set; }
        public string ImagePath { get; private set; }
        public string Content { get; private set; }
        public string Longitude { get; private set; }
        public string Latitude { get; private set; }

        public override string ToString()
        {
            return this.Title;
        }
    }

    /// <summary>
    /// Generic group data model.
    /// </summary>
    public class SampleDataGroup
    {
        public SampleDataGroup(String uniqueId, String title, String subtitle, String imagePath, String description)
        {
            this.UniqueId = uniqueId;
            this.Title = title;
            this.Subtitle = subtitle;
            this.Description = description;
            this.ImagePath = imagePath;
            this.Items = new ObservableCollection<SampleDataItem>();
        }

        public string UniqueId { get; private set; }
        public string Title { get; private set; }
        public string Subtitle { get; private set; }
        public string Description { get; private set; }
        public string ImagePath { get; private set; }
        public ObservableCollection<SampleDataItem> Items { get; private set; }

        public override string ToString()
        {
            return this.Title;
        }
    }

    /// <summary>
    /// Creates a collection of groups and items with content read from a static json file.
    /// 
    /// SampleDataSource initializes with data read from a static json file included in the 
    /// project.  This provides sample data at both design-time and run-time.
    /// </summary>
    public sealed class SampleDataSource
    {
        private static SampleDataSource _sampleDataSource = new SampleDataSource();

        private ObservableCollection<SampleDataGroup> _groups = new ObservableCollection<SampleDataGroup>();
        public ObservableCollection<SampleDataGroup> Groups
        {
            get { return this._groups; }
        }

        public static async Task<IEnumerable<SampleDataGroup>> GetGroupsAsync()
        {
            
            await _sampleDataSource.GetSampleDataAsync();

            return _sampleDataSource.Groups;
        }

        public static async Task<SampleDataGroup> GetGroupAsync(string uniqueId)
        {
            await _sampleDataSource.GetSampleDataAsync();
            // Simple linear search is acceptable for small data sets
            var matches = _sampleDataSource.Groups.Where((group) => group.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        public static async Task<SampleDataItem> GetItemAsync(string uniqueId)
        {
            await _sampleDataSource.GetSampleDataAsync();
            // Simple linear search is acceptable for small data sets
            var matches = _sampleDataSource.Groups.SelectMany(group => group.Items).Where((item) => item.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        private async Task GetSampleDataAsync()
        {
            if (this._groups.Count != 0)
            {
                _groups.Clear();
            }

            Uri dataUri = new Uri("ms-appdata:///local/Data.json");

            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(dataUri);
            file = await StorageFile.GetFileFromApplicationUriAsync(dataUri);
            string jsonText = await FileIO.ReadTextAsync(file);
            JsonObject jsonObject = JsonObject.Parse(jsonText);
            JsonArray jsonArray = jsonObject["Groups"].GetArray();

            foreach (JsonValue groupValue in jsonArray)
            {
                JsonObject groupObject = groupValue.GetObject();
                SampleDataGroup group = new SampleDataGroup(groupObject["UniqueId"].GetString(),
                                                            groupObject["Title"].GetString(),
                                                            groupObject["Subtitle"].GetString(),
                                                            groupObject["ImagePath"].GetString(),
                                                            groupObject["Description"].GetString());

                foreach (JsonValue itemValue in groupObject["Items"].GetArray())
                {
                    JsonObject itemObject = itemValue.GetObject();
                    group.Items.Add(new SampleDataItem(itemObject["UniqueId"].GetString(),
                                                       itemObject["Title"].GetString(),
                                                       itemObject["Subtitle"].GetString(),
                                                       itemObject["ImagePath"].GetString(),
                                                       itemObject["Description"].GetString(),
                                                       itemObject["Content"].GetString(),
                                                       itemObject["Longitude"].GetString(),
                                                       itemObject["Latitude"].GetString()));
                }
                this.Groups.Add(group);            
            }
        }
        public static async void addGroup(Data.SampleDataGroup group, Data.SampleDataItem item)
        {
            Uri dataUri = new Uri("ms-appdata:///local/Data.json");

            JsonObject groupObject = new JsonObject();
            groupObject.Add("UniqueId",JsonValue.CreateStringValue(group.UniqueId));
            groupObject.Add("Title",JsonValue.CreateStringValue(group.Title));
            groupObject.Add("Subtitle",JsonValue.CreateStringValue(group.Subtitle));
            groupObject.Add("ImagePath",JsonValue.CreateStringValue(group.ImagePath));
            groupObject.Add("Description",JsonValue.CreateStringValue(group.Description));
            JsonArray jsonArray = new JsonArray();
            JsonObject itemObject = new JsonObject();
            itemObject.Add("UniqueId", JsonValue.CreateStringValue(item.UniqueId));
            itemObject.Add("Title", JsonValue.CreateStringValue(item.Title));
            itemObject.Add("Subtitle", JsonValue.CreateStringValue(item.Subtitle));
            itemObject.Add("ImagePath", JsonValue.CreateStringValue(item.ImagePath));
            itemObject.Add("Description", JsonValue.CreateStringValue(item.Description));
            itemObject.Add("Content", JsonValue.CreateStringValue(item.Content));
            itemObject.Add("Longitude", JsonValue.CreateStringValue(item.Longitude));
            itemObject.Add("Latitude", JsonValue.CreateStringValue(item.Latitude));
            jsonArray.Add(itemObject);
            groupObject.Add("Items", jsonArray);
            string jsonGroupString = groupObject.Stringify();

            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(dataUri);
            string jsonText = await FileIO.ReadTextAsync(file);
            JsonObject jsonObject = JsonObject.Parse(jsonText);

            jsonObject["Groups"].GetArray().Add(groupObject);
       
            await Windows.Storage.FileIO.WriteTextAsync(file, jsonObject.Stringify()); 
            //TODO Stom van me let me, jaja. ik zie het al. dat bestnad heropenen terwijl hij er al is, gekke shit man
            //Fix Unauthorized Exeption so it will save the new group
        }
        public static async void addItem(Data.SampleDataItem item, string groupName)
        {
            Uri dataUri = new Uri("ms-appdata:///local/Data.json");

            JsonObject itemObject = new JsonObject();
            itemObject.Add("UniqueId", JsonValue.CreateStringValue(item.UniqueId));
            itemObject.Add("Title", JsonValue.CreateStringValue(item.Title));
            itemObject.Add("Subtitle", JsonValue.CreateStringValue(item.Subtitle));
            itemObject.Add("ImagePath", JsonValue.CreateStringValue(item.ImagePath));
            itemObject.Add("Description", JsonValue.CreateStringValue(item.Description));
            itemObject.Add("Content", JsonValue.CreateStringValue(item.Content));
            itemObject.Add("Longitude", JsonValue.CreateStringValue(item.Longitude));
            itemObject.Add("Latitude", JsonValue.CreateStringValue(item.Latitude));
            

            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(dataUri);
            file = await StorageFile.GetFileFromApplicationUriAsync(dataUri);
            string jsonText = await FileIO.ReadTextAsync(file);
            JsonObject jsonObject = JsonObject.Parse(jsonText);
            JsonArray jsonArray = jsonObject["Groups"].GetArray();

            
            foreach (JsonValue groupValue in jsonArray)
            {
                JsonObject jObj = groupValue.GetObject();
                if (groupName == jObj["Title"].GetString())
                {
                    jObj["Items"].GetArray().Add(itemObject);
                    break;
                }

            }
            jsonObject["Groups"] = jsonArray;
            await Windows.Storage.FileIO.WriteTextAsync(file, jsonObject.Stringify()); 

        }
    }
}