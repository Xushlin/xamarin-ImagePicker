using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Net;
using Android.Widget;
using Android.OS;
using Android.Support.V7.App;
using Xamarin.Droid.ImagePicker.Activities;
using Xamarin.Droid.ImagePicker.Helpers;
using Xamarin.Droid.ImagePicker.Models;

namespace Xamarin.Droid.ImagePicker.Demo
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        private GridView _selectedGridView;
        private SelectedImageAdapter _selectedImageAdapter;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Xamarin.Droid.ImagePicker.Demo.Resource.Layout.activity_main);
            _selectedGridView = FindViewById<GridView>(Resource.Id.grid_view_images_selected);
            FindViewById<Button>(Xamarin.Droid.ImagePicker.Demo.Resource.Id.btnPickerImages).Click += (s, e) =>
            {
                var imageIntent = new Intent(this, typeof(AlbumSelectActivity));
                imageIntent.PutExtra(ConstantsCustomGallery.INTENT_EXTRA_LIMIT, ConstantsCustomGallery.LIMIT);
                StartActivityForResult(imageIntent, ConstantsCustomGallery.REQUEST_CODE);
            };
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            if (requestCode == ConstantsCustomGallery.REQUEST_CODE && resultCode == Result.Ok && data != null)
            {
                var result = data.GetStringExtra(ConstantsCustomGallery.INTENT_EXTRA_IMAGES);
                if (string.IsNullOrEmpty(result)) return;
                var selectedImages = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Image>>(result);

                _selectedImageAdapter = new SelectedImageAdapter(this, this, selectedImages);
                _selectedGridView.Adapter = _selectedImageAdapter;
                _selectedGridView.SetNumColumns( 3 );
            }
        }
    }
}

