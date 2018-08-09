using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Views;
using Android.Widget;
using Com.Bumptech.Glide;
using Java.IO;
using Xamarin.Droid.ImagePicker.Models;

namespace Xamarin.Droid.ImagePicker.Adapters
{
    public class CustomImageSelectAdapter : CustomGenericAdapter<Image>
    {
        public CustomImageSelectAdapter(Activity activity, Context context, List<Image> images) : base(activity,context, images){}

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            MyViewHolder viewHolder;
            if (convertView == null)
            {
                convertView = layoutInflater.Inflate(Resource.Layout.grid_view_image_select, null);
                viewHolder = new MyViewHolder
                {
                    Image = convertView.FindViewById<ImageView>(Resource.Id.image_view_image_select),
                    view = convertView.FindViewById<View>(Resource.Id.view_alpha)
                };

                convertView.Tag = viewHolder;
            }
            else
            {
                viewHolder = (MyViewHolder) convertView.Tag;
            }

            viewHolder.Image.LayoutParameters.Width = size;
            viewHolder.Image.LayoutParameters.Height = size;

            viewHolder.view.LayoutParameters.Width = size;
            viewHolder.view.LayoutParameters.Height = size;

            if (arrayList[position].IsSelected)
            {
                viewHolder.view.Alpha = 0.5f;
                ((FrameLayout) convertView).Foreground = context.Resources.GetDrawable(Resource.Drawable.ic_done_white);
            }
            else
            {
                viewHolder.view.Alpha = (0.0f);
                ((FrameLayout) convertView).Foreground = null;
            }

            global::Android.Net.Uri uri = global::Android.Net.Uri.FromFile(new File(arrayList[position].Path));
            Glide.With(context).Load(uri).Into(viewHolder.Image);

            return convertView;
        }

        class MyViewHolder : Java.Lang.Object
        {
            public ImageView Image { get; set; }
            public View view { get; set; }
        }
    }
}