namespace APBD11.DTO;

public class AddPrescriptionRequest
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime Birthdate { get; set; }

    public int DoctorId { get; set; }
    public DateTime Date { get; set; }
    public DateTime DueDate { get; set; }

    public List<MedicamentRequest> Medicaments { get; set; }
}