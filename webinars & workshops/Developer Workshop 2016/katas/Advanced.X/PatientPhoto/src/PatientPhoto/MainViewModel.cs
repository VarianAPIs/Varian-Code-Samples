using System;
using System.IO;
using System.Linq;
using PatientPhoto.Entity;
using Image = System.Drawing.Image;
using Patient = VMS.TPS.Common.Model.API.Patient;

namespace VMS.TPS
{
    public class MainViewModel
    {
        private readonly Patient _patient;

        public MainViewModel(Patient patient)
        {
            _patient = patient;
        }

        public string FullName
        {
            get { return _patient.FirstName + " " + _patient.LastName; }
        }

        public string Id
        {
            get { return _patient.Id; }
        }

        public DateTime? DateOfBirth
        {
            get { return _patient.DateOfBirth; }
        }

        public string PhysicianFullName
        {
            get { return GetPhysicianFullName(); }
        }

        public Image Photo
        {
            get { return GetPatientPhoto(); }
        }

        private Image GetPatientPhoto()
        {
            return ConvertBytesToImage(GetPatientPhotoBytes(_patient.Id));
        }

        private byte[] GetPatientPhotoBytes(string patientId)
        {
            using (var ariaContext = new AriaEntityContext(GetConnectionString()))
            {
                var dbPatient = ariaContext.Patients.First(p => p.PatientId == patientId);
                return dbPatient.Photos.Last().Picture;
            }
        }

        private Image ConvertBytesToImage(byte[] bytes)
        {
            return bytes != null ? Image.FromStream(new MemoryStream(bytes)) : null;
        }

        private string GetPhysicianFullName()
        {
            using (var ariaContext = new AriaEntityContext(GetConnectionString()))
            {
                var physicianId = _patient.PrimaryOncologistId;
                var dbDoctor = ariaContext.Doctors.First(d => d.DoctorId == physicianId);
                return dbDoctor.FirstName + " " + dbDoctor.LastName;
            }
        }

        private string GetConnectionString()
        {
            return "data source=server;initial catalog=VARIAN;integrated security=True;MultipleActiveResultSets=True;App=EntityFramework";
        }
    }
}