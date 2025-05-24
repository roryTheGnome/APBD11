namespace APBD11.DTO;

public class PrescriptionResponse
{
    public int IdPrescription { get; set; }
    public DateTime Date { get; set; }
    public DateTime DueDate { get; set; }

    public List<MedicamentDetail> Medicaments { get; set; }

    public DoctorDetail Doctor { get; set; }
}