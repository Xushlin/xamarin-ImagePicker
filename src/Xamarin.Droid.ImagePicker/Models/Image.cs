namespace Xamarin.Droid.ImagePicker.Models
{
    public class Image
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public bool IsSelected { get; set; }

        public Image(long id,string name, string path, bool isSelected)
        {
            Id = id;
            Name = name;
            Path = path;
            IsSelected = isSelected;
        }
    }
}