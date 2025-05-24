using APBD11.DB;
using APBD11.DTO;
using APBD11.Models;

namespace APBD11.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]

public class PrescriptionsController : ControllerBase
{
    private readonly AppDbContext _context;

    public PrescriptionsController(AppDbContext context)
    {
        _context = context;
    }

    
    [HttpPost]
    public async Task<IActionResult> AddPrescription([FromBody] AddPrescriptionRequest request)
    {
        if (request.Medicaments == null || request.Medicaments.Count == 0 || request.Medicaments.Count > 10)
        {
            return BadRequest("Prescription limit 1-10 ");
        }

        if (request.DueDate < request.Date)
        {
            return BadRequest("unless u have a timemachine, NO");
        }

        var medicamentIds = request.Medicaments.Select(m => m.IdMedicament).ToList();
        
        var existingMedicamentIds = await _context.Medicaments
            .Where(m => medicamentIds.Contains(m.IdMedicament))
            .Select(m => m.IdMedicament).ToListAsync();

        if (existingMedicamentIds.Count != medicamentIds.Count)
        {
            return NotFound("medicaments do not exist");
        }

        var patient = await _context.Patients
            .FirstOrDefaultAsync(p =>
                p.FirstName == request.FirstName &&
                p.LastName == request.LastName &&
                p.Birthdate.Date == request.Birthdate.Date);

        if (patient == null)
        {
            patient = new Patient
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Birthdate = request.Birthdate
            };
            _context.Patients.Add(patient);
            await _context.SaveChangesAsync(); 
        }

        var prescription = new Prescription
        {
            Date = request.Date,
            DueDate = request.DueDate,
            IdPatient = patient.IdPatient,
            IdDoctor = request.DoctorId,
            PrescriptionMedicaments = request.Medicaments.Select(m => new PrescriptionMedicament
            {
                IdMedicament = m.IdMedicament,
                Dose = m.Dose,
                Description = m.Description
            }).ToList()
        };

        _context.Prescriptions.Add(prescription);
        await _context.SaveChangesAsync();

        return Ok("done");
    }

    [HttpGet("/api/patients/{id}")]
    public async Task<IActionResult> GetPatient(int id)
    {
        var patient = await _context.Patients
            .Where(p => p.IdPatient == id)
            .Include(p => p.Prescriptions)
                .ThenInclude(pr => pr.Doctor)
            .Include(p => p.Prescriptions)
                .ThenInclude(pr => pr.PrescriptionMedicaments)
                    .ThenInclude(pm => pm.Medicament)
            .FirstOrDefaultAsync();

        if (patient == null)
        {
            return NotFound("Patient not found.");
        }

        var response = new PatientResponse
        {
            IdPatient = patient.IdPatient,
            FirstName = patient.FirstName,
            LastName = patient.LastName,
            Birthdate = patient.Birthdate,
            Prescriptions = patient.Prescriptions
                .OrderBy(p => p.DueDate)
                .Select(p => new PrescriptionResponse
                {
                    IdPrescription = p.IdPrescription,
                    Date = p.Date,
                    DueDate = p.DueDate,
                    Doctor = new DoctorDetail
                    {
                        IdDoctor = p.Doctor.IdDoctor,
                        FirstName = p.Doctor.FirstName
                    },
                    Medicaments = p.PrescriptionMedicaments.Select(pm => new MedicamentDetail
                    {
                        IdMedicament = pm.Medicament.IdMedicament,
                        Name = pm.Medicament.Name,
                        Dose = pm.Dose,
                        Description = pm.Description
                    }).ToList()
                }).ToList()
        };

        return Ok(response);
    }
}
