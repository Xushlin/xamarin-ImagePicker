namespace Xamarin.Droid.ImagePicker.Helpers
{
    public class ConstantsCustomGallery
    {
        public const int PERMISSION_REQUEST_CODE = 1000;
        public const int PERMISSION_GRANTED = 1001;
        public const int PERMISSION_DENIED = 1002;

        public const  int REQUEST_CODE = 2000;

        public const  int FETCH_STARTED = 2001;
        public const  int FETCH_COMPLETED = 2002;
        public const  int ERROR = 2005;
        public const  int LIMIT = 5;

        /**
         * Request code for permission has to be < (1 << 8)
         * Otherwise throws java.lang.IllegalArgumentException: Can only use lower 8 bits for requestCode
         */
        public static readonly int PERMISSION_REQUEST_READ_EXTERNAL_STORAGE = 23;

        public static readonly string INTENT_EXTRA_ALBUM = "album";
        public static readonly string INTENT_EXTRA_IMAGES = "images";
        public static readonly string INTENT_EXTRA_LIMIT = "limit";
        public static readonly int DEFAULT_LIMIT = 10;

        //Maximum number of images that can be selected at a time
        public static int limit;
    }
}