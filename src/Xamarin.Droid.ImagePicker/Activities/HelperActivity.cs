using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Net;
using Android.Support.Design.Widget;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Views;
using Android.Provider;
using Xamarin.Droid.ImagePicker.Helpers;

namespace Xamarin.Droid.ImagePicker.Activities
{
    [Activity(Label = "HelperActivity")]
    public class HelperActivity : Activity
    {
        protected View view;
        private int maxLines = 4;
        private readonly string[] _permissions = new string[] {Manifest.Permission.WriteExternalStorage};

        public T F<T>(int id) where T : View
        {
            return FindViewById<T>(id);
        }

        protected void CheckPermission()
        {
            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.WriteExternalStorage) == Permission.Granted)
            {
                PermissionGranted();
            }
            else
            {
                ActivityCompat.RequestPermissions(this, _permissions, ConstantsCustomGallery.PERMISSION_REQUEST_CODE);
            }
        }

        private void RequestPermission()
        {
            if (ActivityCompat.ShouldShowRequestPermissionRationale(this, Manifest.Permission.WriteExternalStorage))
            {
                ShowRequestPermissionRationale();
            }
            else
            {
                ShowAppPermissionSettings();
            }
        }

        private void ShowRequestPermissionRationale()
        {
            var snackbar = Snackbar
                .Make(view, GetString(Resource.String.permission_info), Snackbar.LengthIndefinite)
                .SetAction(GetString(Resource.String.permission_ok), (v) =>
                {
                    ActivityCompat.RequestPermissions(this, _permissions,ConstantsCustomGallery.PERMISSION_REQUEST_CODE);
                });

            snackbar.Show();
        }

        private void ShowAppPermissionSettings()
        {
            var snackbar = Snackbar.Make(view,GetString(Resource.String.permission_force),Snackbar.LengthIndefinite)
                .SetAction(GetString(Resource.String.permission_settings), (v) =>
                {
                    var uri = Uri.FromParts(GetString(Resource.String.permission_package), this.PackageName, null);
                    var intent = new Intent();
                    intent.SetAction(Settings.ActionApplicationDetailsSettings);
                    intent.AddFlags(Intent.Flags & ActivityFlags.NoHistory);
                    intent.SetData(uri);
                    StartActivityForResult(intent, ConstantsCustomGallery.PERMISSION_REQUEST_CODE);
                });
            snackbar.Show();
        }
       
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions,
            Permission[] grantResults)
        {
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            if (requestCode != ConstantsCustomGallery.PERMISSION_REQUEST_CODE
                || grantResults.Length == 0
                || grantResults[0] == Permission.Denied)
            {
                PermissionDenied();
            }
            else
            {
                PermissionGranted();
            }
        }

        protected virtual void PermissionGranted()
        {
        }

        private void PermissionDenied()
        {
            HideViews();
            RequestPermission();
        }

        protected virtual void HideViews()
        {
        }

        protected void SetView(View v)
        {
            this.view = v;
        }
    }
}