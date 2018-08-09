using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.Database;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.IO;
using Java.Lang;
using Xamarin.Droid.ImagePicker.Adapters;
using Xamarin.Droid.ImagePicker.Helpers;
using Xamarin.Droid.ImagePicker.Models;
using Exception = System.Exception;
using Orientation = Android.Content.Res.Orientation;
using Process = Android.OS.Process;
using Thread = Java.Lang.Thread;
using ThreadPriority = Android.OS.ThreadPriority;

namespace Xamarin.Droid.ImagePicker.Activities
{
    [Activity(Label = "ImageSelectActivity")]
    public class ImageSelectActivity : HelperActivity
    {
        private List<Image> _images;
        private string _album;
        private TextView _errorDisplay, _tvProfile, _tvAdd, _tvSelectCount;
        private LinearLayout _liFinish;
        private ProgressBar _loader;
        private GridView _gridView;
        private CustomImageSelectAdapter _adapter;
        private int _countSelected;
        private ContentObserver _observer;
        private Handler _handler;
        private System.Threading.Thread _thread;
        private readonly string[] _projection = {
            MediaStore.Images.Media.InterfaceConsts.Id,
            MediaStore.Images.Media.InterfaceConsts.DisplayName,
            MediaStore.Images.Media.InterfaceConsts.Data
        };
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_image_select);
            SetView(FindViewById<RelativeLayout>(Resource.Id.layout_image_select));
            _tvProfile = FindViewById<TextView>(Resource.Id.tvProfile);
            _tvAdd = FindViewById<TextView>(Resource.Id.tvAdd);
            _tvSelectCount = FindViewById<TextView>(Resource.Id.tvSelectCount);
            _tvProfile.Text = GetString(Resource.String.image_view);
            _liFinish = FindViewById<LinearLayout>(Resource.Id.liFinish);
            _album = Intent.GetStringExtra(ConstantsCustomGallery.INTENT_EXTRA_ALBUM);
            _errorDisplay = FindViewById<TextView>(Resource.Id.text_view_error);
            _errorDisplay.Visibility = ViewStates.Invisible;
            _loader = FindViewById<ProgressBar>(Resource.Id.loader);
            _gridView = FindViewById<GridView>(Resource.Id.grid_view_image_select);
            _gridView.ItemClick += GridView_ItemClick;

            _liFinish.Click += (s, e) =>
            {
                if (_tvSelectCount.Visibility == ViewStates.Visible)
                {
                    DeselectAll();
                }
                else
                {
                    Finish();
                    OverridePendingTransition(global::Android.Resource.Animation.FadeIn, global::Android.Resource.Animation.FadeOut);
                }
            };
            _tvAdd.Click += (s, e) => { SendIntent(); };
        }

        protected override void OnStart()
        {
            base.OnStart();
            _handler = new MyHander(this);
            _observer = new MyContentObserver(_handler, this);
            ContentResolver.RegisterContentObserver(MediaStore.Images.Media.ExternalContentUri, false, _observer);
            CheckPermission();
        }
        protected override void OnStop()
        {
            base.OnStop();

            StopThread();

            ContentResolver.UnregisterContentObserver(_observer);
            _observer = null;

            if (_handler != null)
            {
                _handler.RemoveCallbacksAndMessages(null);
                _handler = null;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _images = null;
            _adapter?.ReleaseResources();
            _gridView.ItemClick -= GridView_ItemClick;
        }

        private void GridView_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            ToggleSelection(e.Position);
            _tvSelectCount.Text = _countSelected + " " + GetString(Resource.String.selected);
            _tvSelectCount.Visibility = ViewStates.Visible;
            _tvAdd.Visibility = ViewStates.Visible;
            _tvProfile.Visibility = ViewStates.Gone;

            if (_countSelected == 0)
            {
                _tvSelectCount.Visibility = ViewStates.Gone;
                _tvAdd.Visibility = ViewStates.Gone;
                _tvProfile.Visibility = ViewStates.Visible;
            }
        }

        public override void OnConfigurationChanged(Configuration newConfig)
        {
            base.OnConfigurationChanged(newConfig);
            OrientationBasedUi(newConfig.Orientation);
        }

        private void OrientationBasedUi(Orientation orientation)
        {
            var windowManager = this.WindowManager;
            var metrics = new DisplayMetrics();
            windowManager.DefaultDisplay.GetMetrics(metrics);

            if (_adapter != null)
            {
                var size = orientation == Orientation.Portrait? metrics.WidthPixels / 3: metrics.WidthPixels / 5;
                _adapter.SetLayoutParams(size);
            }

            _gridView.SetNumColumns(orientation == Orientation.Portrait ? 3 : 5);
        }
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case global::Android.Resource.Id.Home:
                    OnBackPressed();
                    return true;
                default:
                    return false;
            }
        }
        private void ToggleSelection(int position)
        {
            if (!_images[position].IsSelected && _countSelected >= ConstantsCustomGallery.limit)
            {
                Toast.MakeText(ApplicationContext, string.Format(GetString(Resource.String.limit_exceeded), ConstantsCustomGallery.limit),
                        ToastLength.Short).Show();
                return;
            }

            _images[position].IsSelected = !_images[position].IsSelected;
            if (_images[position].IsSelected)
            {
                _countSelected++;
            }
            else
            {
                _countSelected--;
            }

            _adapter.NotifyDataSetChanged();
        }
        private void DeselectAll()
        {
            _tvProfile.Visibility = (ViewStates.Visible);
            _tvAdd.Visibility = (ViewStates.Gone);
            _tvSelectCount.Visibility = (ViewStates.Gone);

            for (int i = 0, l = _images.Count; i < l; i++)
            {
                _images[i].IsSelected = false;
            }

            _countSelected = 0;
            _adapter.NotifyDataSetChanged();
        }
        private List<Image> GetSelected()
        {
            var selectedImages = new List<Image>();
            for (int i = 0, l = _images.Count; i < l; i++)
            {
                if (_images[i].IsSelected)
                {
                    selectedImages.Add(_images[i]);
                }
            }
            return selectedImages;
        }
        private void SendIntent()
        {
            var intent = new Intent();
            intent.PutExtra(ConstantsCustomGallery.INTENT_EXTRA_IMAGES, Newtonsoft.Json.JsonConvert.SerializeObject(GetSelected()));
            SetResult(Result.Ok, intent);
            Finish();
            OverridePendingTransition(global::Android.Resource.Animation.FadeIn, global::Android.Resource.Animation.FadeOut);
        }
        private void LoadImages(ImageSelectActivity context)
        {
            StartThread(new ImageRunnable(context));
        }
        private void StartThread(ImageRunnable runnable)
        {
            StopThread();
            _thread = runnable.GetThread();
            _thread.Start();
        }
        private void StopThread()
        {
            if (_thread == null || !_thread.IsAlive) return;

            _thread.Interrupt();
            try
            {
                _thread.Join();
            }
            catch (InterruptedException e)
            {
                e.PrintStackTrace();
            }
        }
        private void SendMessage(int what)
        {
            SendMessage(what, 0);
        }
        private void SendMessage(int what, int arg1)
        {
            if (_handler == null)
            {
                return;
            }

            var message = _handler.ObtainMessage();
            message.What = what;
            message.Arg1 = arg1;
            message.SendToTarget();
        }
        protected override void PermissionGranted()
        {
            SendMessage(ConstantsCustomGallery.PERMISSION_GRANTED);
        }
        protected override void HideViews()
        {
            _loader.Visibility = (ViewStates.Gone);
            _gridView.Visibility = (ViewStates.Invisible);
        }
        public override void OnBackPressed()
        {
            if (_tvSelectCount.Visibility == ViewStates.Visible)
            {
                DeselectAll();
            }
            else
            {
                base.OnBackPressed();
                OverridePendingTransition(global::Android.Resource.Animation.FadeIn, global::Android.Resource.Animation.FadeOut);
                Finish();
            }
        }

        private class ImageRunnable
        {
            private readonly ImageSelectActivity _context;
            public ImageRunnable(ImageSelectActivity context)
            {
                _context = context;
            }

            public System.Threading.Thread GetThread()
            {
                var newThread =new System.Threading.Thread((new System.Threading.ThreadStart(Run)));

                return newThread;
            }

            void Run()
            {
                Process.SetThreadPriority(ThreadPriority.Background);
                if (_context._adapter == null)
                {
                    _context.SendMessage(ConstantsCustomGallery.FETCH_STARTED);
                }

                File file;
                var selectedImages = new List<long>();
                if (_context._images != null)
                {
                    for (int i = 0, l = _context._images.Count; i < l; i++)
                    {
                        var image = _context._images[i];
                        file = new File(image.Path);
                        if (file.Exists() && image.IsSelected)
                        {
                            selectedImages.Add(image.Id);
                        }
                    }
                }

                var cursor = _context.Application.ApplicationContext.ContentResolver.Query(
                    MediaStore.Images.Media.ExternalContentUri, _context._projection,
                    MediaStore.Images.Media.InterfaceConsts.BucketDisplayName + " =?", new string[] { _context._album },
                    MediaStore.Images.Media.InterfaceConsts.DateAdded);
                if (cursor == null)
                {
                    _context.SendMessage(ConstantsCustomGallery.ERROR);
                    return;
                }

                /*
                In case this runnable is executed to onChange calling loadImages,
                using countSelected variable can result in a race condition. To avoid that,
                tempCountSelected keeps track of number of selected images. On handling
                FETCH_COMPLETED message, countSelected is assigned value of tempCountSelected.
                 */
                int tempCountSelected = 0;
                var temp = new List<Image>(cursor.Count);
                if (cursor.MoveToLast())
                {
                    do
                    {
                        if (Thread.Interrupted())
                        {
                            return;
                        }

                        var id = cursor.GetLong(cursor.GetColumnIndex(_context._projection[0]));
                        var name = cursor.GetString(cursor.GetColumnIndex(_context._projection[1]));
                        var path = cursor.GetString(cursor.GetColumnIndex(_context._projection[2]));
                        var isSelected = selectedImages.Contains(id);
                        if (isSelected)
                        {
                            tempCountSelected++;
                        }

                        file = null;
                        try
                        {
                            file = new File(path);
                        }
                        catch (Exception e)
                        {
                            Log.Debug("Exception : ", e.ToString());
                        }

                        if (file.Exists())
                        {
                            temp.Add(new Image(id, name, path, isSelected));
                        }
                    } while (cursor.MoveToPrevious());
                }

                cursor.Close();

                if (_context._images == null)
                {
                    _context._images = new List<Image>();
                }

                _context._images.Clear();
                _context._images.AddRange(temp);

                _context.SendMessage(ConstantsCustomGallery.FETCH_COMPLETED, tempCountSelected);
            }
        }

        private class MyHander : Handler
        {
            private readonly ImageSelectActivity _context;

            public MyHander(ImageSelectActivity context)
            {
                _context = context;
            }

            public override void HandleMessage(Message msg)
            {
                switch (msg.What)
                {
                    case ConstantsCustomGallery.PERMISSION_GRANTED:
                        _context.LoadImages(_context);
                        break;
                    case ConstantsCustomGallery.FETCH_STARTED:
                        _context._loader.Visibility = ViewStates.Visible;
                        _context._gridView.Visibility = ViewStates.Invisible;
                        break;

                    case ConstantsCustomGallery.FETCH_COMPLETED:

                        /*
                        If adapter is null, this implies that the loaded images will be shown
                        for the first time, hence send FETCH_COMPLETED message.
                        However, if adapter has been initialised, this thread was run either
                        due to the activity being restarted or content being changed.
                         */
                        if (_context._adapter == null)
                        {
                            _context._adapter = new CustomImageSelectAdapter(_context, _context, _context._images);
                            _context._gridView.Adapter = (_context._adapter);

                            _context._loader.Visibility = ViewStates.Gone;
                            _context._gridView.Visibility = ViewStates.Visible;
                            _context.OrientationBasedUi(_context.Resources.Configuration.Orientation);
                        }
                        else
                        {
                            _context._adapter.NotifyDataSetChanged();
                            /*
                            Some selected images may have been deleted
                            hence update action mode title
                             */
                            _context._countSelected = msg.Arg1;
                            //actionMode.setTitle(countSelected + " " + getString(R.string.selected));
                            _context._tvSelectCount.Text =_context._countSelected + " " + _context.GetString(Resource.String.selected);
                            _context._tvSelectCount.Visibility = ViewStates.Visible;
                            _context._tvAdd.Visibility = ViewStates.Visible;
                            _context._tvProfile.Visibility = ViewStates.Gone;
                        }

                        break;
                    case ConstantsCustomGallery.ERROR:
                        _context._loader.Visibility = ViewStates.Gone;
                        _context._errorDisplay.Visibility = ViewStates.Visible;
                        break;
                    default:
                        base.HandleMessage(msg);
                        break;
                }
            }
        }

        private class MyContentObserver : ContentObserver
        {
            public MyContentObserver(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer){}
            private readonly ImageSelectActivity _context;
            public MyContentObserver(Handler handler, ImageSelectActivity context) :base(handler)
            {
                this._context = context;
            }

            public override void OnChange(bool selfChange)
            {
                _context.LoadImages(_context);
            }
        }
    }
}