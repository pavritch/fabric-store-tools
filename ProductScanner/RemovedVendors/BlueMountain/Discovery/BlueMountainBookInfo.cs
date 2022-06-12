namespace BlueMountain.Discovery
{
    public class BlueMountainBookInfo
    {
        public string Color { get; set; }
        public int ColorId { get; set; }

        public BlueMountainBookInfo(string color, int colorId)
        {
            Color = color;
            ColorId = colorId;
        }
    }
}