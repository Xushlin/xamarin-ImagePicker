using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Views;
using Android.Widget;
using Com.Bumptech.Glide;
using Com.Bumptech.Glide.Request;
using Xamarin.Droid.ImagePicker.Models;

namespace Xamarin.Droid.ImagePicker.Adapters
{
    public class CustomAlbumSelectAdapter : CustomGenericAdapter<Album>
    {
        public CustomAlbumSelectAdapter(Activity activity, Context context, List<Album> albums): base(activity, context, albums){}

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            MyViewHolder viewHolder;

            if (convertView == null)
            {
                convertView = layoutInflater.Inflate(Resource.Layout.grid_view_item_album_select, null);

                viewHolder = new MyViewHolder
                {
                    Image = convertView.FindViewById<ImageView>(Resource.Id.image_view_album_image),
                    Name = convertView.FindViewById<TextView>(Resource.Id.text_view_album_name)
                };

                convertView.Tag = viewHolder;
            }
            else
            {
                viewHolder = (MyViewHolder) convertView.Tag;
            }

            viewHolder.Image.LayoutParameters.Width = size;
            viewHolder.Image.LayoutParameters.Height = size;

            viewHolder.Name.Text = (arrayList[position].Name);

            if (arrayList[position].Name.Equals("Take Photo"))
            {
                Glide.With(context).Load(arrayList[position].Cover).Into(viewHolder.Image); 
            }
            else
            {
                global::Android.Net.Uri uri = global::Android.Net.Uri.FromFile(new Java.IO.File(arrayList[position].Cover));
                Glide.With(context).Load(uri).Into(viewHolder.Image);
            }

            return convertView;
        }
        private class MyViewHolder : Java.Lang.Object
        {
            public ImageView Image { get; set; }
            public TextView Name { get; set; }
        }
    }
}