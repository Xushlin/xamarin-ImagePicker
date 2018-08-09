using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Util;
using Android.Views;
using Android.Widget;
using Com.Bumptech.Glide;
using Java.IO;
using Xamarin.Droid.ImagePicker.Adapters;
using Xamarin.Droid.ImagePicker.Models;

namespace Xamarin.Droid.ImagePicker.Demo
{
    public class SelectedImageAdapter : CustomGenericAdapter<Image>
    {
        public SelectedImageAdapter(Activity activity, Context context, List<Image> images) : base(activity,context, images){}

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            ViewHolder viewHolder;
            if (convertView == null)
            {
                convertView = layoutInflater.Inflate(ImagePicker.Resource.Layout.grid_view_image_select, null);
                viewHolder = new ViewHolder
                {
                    Image = convertView.FindViewById<ImageView>(ImagePicker.Resource.Id.image_view_image_select),
                   
                };

                convertView.Tag = viewHolder;
            }
            else
            {
                viewHolder = convertView.Tag as ViewHolder;
            }
            var windowManager = activity.WindowManager;
            var metrics = new DisplayMetrics();
            windowManager.DefaultDisplay.GetMetrics(metrics);


            var size = metrics.WidthPixels / 3;
              SetLayoutParams(size);
           

           
            viewHolder.Image.LayoutParameters.Width = size;
            viewHolder.Image.LayoutParameters.Height = size;
            

            global::Android.Net.Uri uri = global::Android.Net.Uri.FromFile(new File(arrayList[position].Path));
            Glide.With(context).Load(uri).Into(viewHolder.Image);

            return convertView;
        }

        class ViewHolder : Java.Lang.Object
        {
            public ImageView Image { get; set; }
        }
    }
}