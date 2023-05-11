namespace HeyDoc.Web.Models
{
    /// <summary>
    ///  AI class
    /// </summary>
    public class AIModel
    {
        /// <summary>
        /// Upload image
        /// </summary>
        public string Image { get; set; }
        /// Score result came from response
        public double Score { get; set; }
        /// RiskLevel range score of risk 
        public string RiskLevel { get;  set; }
    }
}