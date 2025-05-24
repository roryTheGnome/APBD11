namespace APBD11.DTO;

public class PatientResponse
{
    public int IdPatient { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime Birthdate { get; set; }

    public List<PrescriptionResponse> Prescriptions { get; set; }
}