using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Views;
using Android.Widget;

namespace Xamarin.Droid.ImagePicker.Adapters
{
    public abstract class CustomGenericAdapter<T> : BaseAdapter<T>
    {
        protected List<T> arrayList;
        protected Context context;
        protected Activity activity;
        protected LayoutInflater layoutInflater;
        protected int size;

        protected CustomGenericAdapter(Activity activity, Context context, List<T> arrayList)
        {
            this.arrayList = arrayList;
            this.context = context;
            this.activity = activity;
            this.layoutInflater = LayoutInflater.From(this.context);
        }

        public override int Count => arrayList.Count;

        public override T this[int position] => arrayList[position];

        public override long GetItemId(int position)
        {
            return position;
        }

        public void SetLayoutParams(int size)
        {
            this.size = size;
        }

        public void ReleaseResources()
        {
            arrayList = null;
            context = null;
            activity = null;
        }
    }
}