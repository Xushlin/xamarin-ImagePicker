namespace Xamarin.Droid.ImagePicker.Models
{
    public class Album 
    {
        public string Name { get; set; }
        public string Cover { get; set; }

        public Album(string name,string cover)
        {
            this.Name = name;
            this.Cover = cover;
        }
    }
}