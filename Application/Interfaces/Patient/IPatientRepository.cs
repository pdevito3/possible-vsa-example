namespace Application.Interfaces.Patient
{
    using System;
    using Application.Dtos.Patient;
    using Application.Wrappers;
    using System.Threading.Tasks;
    using Domain.Entities;
    using System.Threading;

    public interface IPatientRepository
    {
        Task<PagedList<Patient>> GetPatientsAsync(PatientParametersDto PatientParameters, CancellationToken cancellationToken);
        Task<Patient> GetPatientAsync(Guid PatientId);
        Patient GetPatient(Guid PatientId);
        Task AddPatient(Patient patient);
        void DeletePatient(Patient patient);
        void UpdatePatient(Patient patient);
        bool Save();
        Task<bool> SaveAsync();
    }
}