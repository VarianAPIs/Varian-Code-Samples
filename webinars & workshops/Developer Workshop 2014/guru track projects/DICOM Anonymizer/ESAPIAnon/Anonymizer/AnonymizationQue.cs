using EvilDICOM.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ESAPIAnon.Anonymizers
{
    /// <summary>
    /// This class holds all of the anonymization objects that will transform the DICOM objects
    /// </summary>
    public class AnonymizationQue : IAnonymizer
    {
        public AnonymizationQue()
        {
            Que = new List<IAnonymizer>();
        }

        public void Anonymize(DICOMObject d)
        {
            foreach (var q in Que)
            {
                q.Anonymize(d);
            }
        }

        public List<IAnonymizer> Que { get; set; }


        public static AnonymizationQue Build(Settings.AnonymizeSettings settings, List<DICOMObject> objects)
        {
            var anonQue = new AnonymizationQue();
            if (settings.DoAnonymizeStudyIDs || settings.DoAnonymizeUIDs)
            {
                StudyIdAnonymizer studyAnon = settings.DoAnonymizeStudyIDs? new StudyIdAnonymizer(): null;
                UIDAnonymizer uidAnon = settings.DoAnonymizeUIDs ? new UIDAnonymizer() : null;
                foreach (var ob in objects)
                {
                    
                    if (uidAnon != null) uidAnon.AddDICOMObject(ob);
                    if (studyAnon != null) studyAnon.AddDICOMObject(ob);
                }
                if (studyAnon != null) { studyAnon.FinalizeDictionary(); anonQue.Que.Add(studyAnon); }
                if (uidAnon != null) { anonQue.Que.Add(uidAnon); }
            }

            if (settings.DoRemovePrivateTags) anonQue.Que.Add(new PrivateTagAnonymizer());
            anonQue.Que.Add(new PatientIdAnonymizer(settings.FirstName, settings.LastName, settings.Id));
            anonQue.Que.Add(new DateAnonymizer(settings.DateSettings));
            anonQue.Que.Add(new ProfileAnonymizer());
            return anonQue;
        }

        public string Name
        {
            get { return "Anonymization Que"; }
        }
    }
}
