using OpenCvSharp;

namespace ProcessBase.Models
{
    public class ImageStatus
    {
        public ImageStatus()
        {
            
        }

        public Rect ImageRect { get; set; }
        public bool Status { get; set; }

    }
}
