using EvilDICOM.Core;
using EvilDICOM.Core.Element;
using EvilDICOM.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ESAPIAnon.Anonymizers
{
    public class PatientIdAnonymizer : IAnonymizer
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Id { get; set; }
        public string FormattedName
        {
            get
            {
                return string.Format("{0}^{1}", LastName, FirstName);
            }
        }

        public PatientIdAnonymizer(string firstName, string lastName, string id)
        {
            FirstName = firstName;
            LastName = lastName;
            Id = id;
        }

        public void Anonymize(DICOMObject d)
        {
            //PATIENTS NAME
            PersonName name = new PersonName();
            name.Tag = TagHelper.PATIENT_NAME;
            name.Data = FormattedName;
            d.Replace(name);

            //PATIENT ID
            LongString id = new LongString();
            id.Tag = TagHelper.PATIENT_ID;
            id.Data = Id;
            d.Replace(id);
        }

        public string Name
        {
            get { return "Id Anonymizer"; }
        }
    }
}
