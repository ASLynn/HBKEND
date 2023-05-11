namespace HeyDoc.Web.Models
{
    public class PrescriptionSourceModel
    {
        public int PrescriptionSourceId { get; set; }
        public string PrescriptionSourceName { get; set; }
        public PhotoModel Logo { get; set; }

        public PrescriptionSourceModel() { }

        public PrescriptionSourceModel(Entity.PrescriptionSource entityPrescriptionSource)
        {
            PrescriptionSourceId = entityPrescriptionSource.PrescriptionSourceId;
            PrescriptionSourceName = entityPrescriptionSource.PrescriptionSourceName;
            Logo = new PhotoModel(entityPrescriptionSource.Photo);
        }
    }
}