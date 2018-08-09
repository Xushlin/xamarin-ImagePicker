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
using Orientation = Android.Content.Res.Orientation;


namespace Xamarin.Droid.ImagePicker.Activities
{
    [Activity(Label = "AlbumSelectActivity")]
    public class AlbumSelectActivity : HelperActivity
    {
        private List<Album> _albums;
        private TextView _errorDisplay, _tvProfile;
        private LinearLayout _liFinish;
        private ProgressBar _loader;
        private GridView _gridView;
        private CustomAlbumSelectAdapter _adapter;
        private ActionBar actionBar;
        private ContentObserver _observer;
        private Handler _handler;
        private System.Threading.Thread _thread;
        private readonly string[] _projection = new string[]
        {
            MediaStore.Images.Media.InterfaceConsts.BucketId,
            MediaStore.Images.Media.InterfaceConsts.BucketDisplayName,
            MediaStore.Images.Media.InterfaceConsts.Data
        };

        
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_album_select);
            SetView(FindViewById<View>(Resource.Id.layout_album_select));
            ConstantsCustomGallery.limit = Intent.GetIntExtra(ConstantsCustomGallery.INTENT_EXTRA_LIMIT,ConstantsCustomGallery.DEFAULT_LIMIT);
            _errorDisplay = FindViewById<TextView>(Resource.Id.text_view_error);
            _errorDisplay.Visibility = ViewStates.Invisible;
            _tvProfile = F<TextView>(Resource.Id.tvProfile);
            _tvProfile.Text = GetString(Resource.String.album_view);
            _liFinish = F<LinearLayout>(Resource.Id.liFinish);
            _loader = F<ProgressBar>(Resource.Id.loader);
            _gridView = F<GridView>(Resource.Id.grid_view_album_select);
            _gridView.ItemClick += GridView_ItemClick;
            _liFinish.Click += (s, e) =>
            {
                Finish();
                OverridePendingTransition(global::Android.Resource.Animation.FadeIn, global::Android.Resource.Animation.FadeOut);
            };
        }

        private void GridView_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            if (_albums[e.Position].Name.Equals(GetString(Resource.String.capture_photo)))
            {
                //HelperClass.displayMessageOnScreen(getApplicationContext(), "HMM!", false);
            }
            else
            {
                var intent = new Intent(this, typeof(ImageSelectActivity));
                intent.PutExtra(ConstantsCustomGallery.INTENT_EXTRA_ALBUM, _albums[e.Position].Name);
                StartActivityForResult(intent, ConstantsCustomGallery.REQUEST_CODE);
            }
        }

        protected override void OnStart()
        {
            base.OnStart();
            _handler = new AlbumHander(this);
            _observer = new AlbumContentObserver(_handler, this);
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
            actionBar?.SetHomeAsUpIndicator(null);
            _albums = null;
            _adapter?.ReleaseResources();
            _gridView.ItemClick += GridView_ItemClick;
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
                var size = orientation == Orientation.Portrait ? metrics.WidthPixels / 2 : metrics.WidthPixels / 4;
                _adapter.SetLayoutParams(size);
            }
            _gridView.SetNumColumns(orientation == Orientation.Portrait ? 2 : 4);
        }

        public override void OnBackPressed()
        {
            base.OnBackPressed();
            SetResult(Result.Canceled);
            OverridePendingTransition(global::Android.Resource.Animation.FadeIn, global::Android.Resource.Animation.FadeOut);
            Finish();
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            if (requestCode == ConstantsCustomGallery.REQUEST_CODE && resultCode == Result.Ok && data != null)
            {
                SetResult(Result.Ok, data);
                Finish();
            }
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case global::Android.Resource.Id.Home:
                    OnBackPressed();
                    return true;
                default: return false;
            }
        }
        private void LoadAlbums()
        {
            StartThread(new AlbumLoaderRunnable(this));
        }
        private void StartThread(AlbumLoaderRunnable runnable)
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
                
            }
        }
        private void SendMessage(int what)
        {
            if (_handler == null)
            {
                return;
            }

            var message = _handler.ObtainMessage();
            message.What = what;
            message.SendToTarget();
        }
        protected override void PermissionGranted()
        {
            var message = _handler.ObtainMessage();
            message.What = ConstantsCustomGallery.PERMISSION_GRANTED;
            message.SendToTarget();
        }
        protected override void HideViews()
        {
            _loader.Visibility=ViewStates.Gone;
            _gridView.Visibility = ViewStates.Invisible;
        }

        private class AlbumContentObserver : ContentObserver
        {
            private readonly AlbumSelectActivity _ctx;
            public AlbumContentObserver(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference,transfer){}
            public AlbumContentObserver(Handler handler, AlbumSelectActivity ctx) : base(handler)
            {
                _ctx = ctx;
            }

            public override void OnChange(bool selfChange, global::Android.Net.Uri uri)
            {
                base.OnChange(selfChange, uri);
                _ctx.LoadAlbums();
            }
        }

        private class AlbumHander : Handler
        {
            private readonly AlbumSelectActivity _ctx;
            public AlbumHander(AlbumSelectActivity ctx)
            {
                _ctx = ctx;
            }
            public override void HandleMessage(Message msg)
            {
                switch (msg.What)
                {
                    case ConstantsCustomGallery.PERMISSION_GRANTED:
                        _ctx.LoadAlbums();
                        break;

                    case ConstantsCustomGallery.FETCH_STARTED:
                        _ctx._loader.Visibility = ViewStates.Visible;
                        _ctx._gridView.Visibility = ViewStates.Invisible;
                        break;
                    case ConstantsCustomGallery.FETCH_COMPLETED:
                        if (_ctx._adapter == null)
                        {
                            _ctx._adapter = new CustomAlbumSelectAdapter(_ctx, _ctx, _ctx._albums);
                            _ctx._gridView.Adapter = _ctx._adapter;
                            _ctx._loader.Visibility = ViewStates.Gone;
                            _ctx._gridView.Visibility = ViewStates.Visible;
                            _ctx.OrientationBasedUi(_ctx.Resources.Configuration.Orientation);
                        }
                        else
                        {
                            _ctx._adapter.NotifyDataSetChanged();
                        }

                        break;
                    case ConstantsCustomGallery.ERROR:
                        _ctx._loader.Visibility = ViewStates.Gone;
                        _ctx._errorDisplay.Visibility = ViewStates.Visible;
                        break;
                    default:
                        base.HandleMessage(msg);
                        break;
                }
            }
        }

        private class AlbumLoaderRunnable
        {
            private readonly AlbumSelectActivity _context;
            public AlbumLoaderRunnable(AlbumSelectActivity context)
            {
                _context = context;
            }
            public System.Threading.Thread GetThread()
            {
                var thread =new System.Threading.Thread((new System.Threading.ThreadStart(Run)));

                return thread;
            }
            private void Run()
            {
                global::Android.OS.Process.SetThreadPriority(ThreadPriority.Background);
                if (_context._adapter == null)
                {
                    _context.SendMessage(ConstantsCustomGallery.FETCH_STARTED);
                }
                var cursor = _context.Application.ApplicationContext.ContentResolver.Query(MediaStore.Images.Media.ExternalContentUri, _context._projection,null, null, MediaStore.Images.Media.InterfaceConsts.DateModified);
                if (cursor == null)
                {
                    _context.SendMessage(ConstantsCustomGallery.ERROR);

                    return;
                }

                var temp = new List<Album>(cursor.Count);
                var albumSet = new List<long>();
                if (cursor.MoveToLast())
                {
                    do
                    {
                        if (Thread.Interrupted())
                        {
                            return;
                        }

                        var albumId = cursor.GetLong(cursor.GetColumnIndex(_context._projection[0]));
                        var album = cursor.GetString(cursor.GetColumnIndex(_context._projection[1]));
                        var image = cursor.GetString(cursor.GetColumnIndex(_context._projection[2]));
                       
                        if (!albumSet.Contains(albumId))
                        {
                            /*
            It may happen that some image file paths are still present in cache,
            though image file does not exist. These last as long as media
            scanner is not run again. To avoid get such image file paths, check
            if image file exists.
             */
                            File file = new File(image);
                            if (file.Exists())
                            {
                                temp.Add(new Album(album, image));

                                /*if (!album.equals("Hiding particular folder")) {
                                    temp.add(new Album(album, image));
                                }*/
                                albumSet.Add(albumId);
                            }
                        }

                    } while (cursor.MoveToPrevious());
                }
                cursor.Close();

                if (_context._albums == null)
                {
                    _context._albums = new List<Album>();
                }
                _context._albums.Clear();
                // adding taking photo from camera option!
                /*albums.add(new Album(getString(R.string.capture_photo),
                        "https://image.freepik.com/free-vector/flat-white-camera_23-2147490625.jpg"));*/
                _context._albums.AddRange(temp);
                _context.SendMessage(ConstantsCustomGallery.FETCH_COMPLETED);
            }
        }
    }
}

