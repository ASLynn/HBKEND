namespace HeyDoc.Web.Models
{
    /// <summary>
    ///  AI Image Request class
    /// </summary>
    public class AIImageRequest
    {
        /// Upload image
        public string Image { get; set; }
        /// Score result came from response
        public double Score { get; set; }
    }
}