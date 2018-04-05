using ESAPIAnon.Helpers;
using ESAPIAnon.Settings;
using EvilDICOM.Core;
using EvilDICOM.Core.Element;
using EvilDICOM.Core.Enums;
using EvilDICOM.Core.Helpers;
using EvilDICOM.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ESAPIAnon.Anonymizers
{
    public class DateAnonymizer : IAnonymizer
    {
        public DateAnonymizer(DateSettings settings)
        {
            Settings = settings;
            BaseDate = new System.DateTime(1900, 1, 1);
        }

        public DateSettings Settings { get; set; }

        public void Anonymize(DICOMObject d)
        {
            if (Settings == DateSettings.KEEP_ALL_DATES)
            {
                return;
            }
            else
            {
                if (Settings == DateSettings.PRESERVE_AGE)
                {
                    PreserveAndAnonymize(d);
                }
                else if (Settings == DateSettings.NULL_AGE)
                {
                    NullAndAnonymize(d);
                }
                else if (Settings == DateSettings.MAKE_89)
                {
                    Make89AndAnonymize(d);
                }
                else
                {
                    Randomize(d);
                }
            }
        }

        public System.DateTime BaseDate { get; set; }
        public DateAnonymizer(System.DateTime baseDate)
        {
            BaseDate = baseDate;
        }

        public void PreserveAndAnonymize(DICOMObject d)
        {
            List<IDICOMElement> dates = d.FindAll(VR.Date);
            if (dates.Count > 0)
            {
                Date oldest = (Date)dates.OrderBy(da => (da as Date).Data).ToList()[0];
                foreach (IDICOMElement el in dates)
                {
                    Date da = el as Date;
                    System.DateTime? date = DateHelper.DateRelativeBaseDate(da.Data, oldest.Data);
                    da.Data = DateHelper.DateRelativeBaseDate(da.Data, oldest.Data);
                }
            }
        }

        public void NullAndAnonymize(DICOMObject d)
        {
            Date dob = d.FindFirst(TagHelper.PATIENT_BIRTH_DATE) as Date;
            dob.Data = null;

            List<IDICOMElement> dates = d.FindAll(VR.Date);
            if (dates.Count > 0)
            {
                Date oldest = (Date)dates
                    .Where(da => (da as Date).Data != null)
                    .OrderBy(da => (da as Date).Data)
                    .ToList()[0];
                foreach (IDICOMElement el in dates)
                {
                    Date da = el as Date;
                    System.DateTime? date = DateHelper.DateRelativeBaseDate(da.Data, oldest.Data);
                    da.Data = DateHelper.DateRelativeBaseDate(da.Data, oldest.Data);
                }
            }
        }

        public void Make89AndAnonymize(DICOMObject d)
        {
            Date dob = d.FindFirst(TagHelper.PATIENT_BIRTH_DATE) as Date;
            List<IDICOMElement> dates = d.FindAll(VR.Date);
            if (dates.Count > 0)
            {
                Date oldest = (Date)dates
                    .Where(da => (da as Date).Data != null && da.Tag.CompleteID != TagHelper.PATIENT_BIRTH_DATE.CompleteID)
                    .OrderBy(da => (da as Date).Data)
                    .ToList()[0];
                System.DateTime oldestDate = (System.DateTime)oldest.Data;
                dob.Data = new System.DateTime(oldestDate.Year - 89, oldestDate.Month, oldestDate.Day);

                oldest = dob;
                foreach (IDICOMElement el in dates)
                {
                    Date da = el as Date;
                    System.DateTime? date = DateHelper.DateRelativeBaseDate(da.Data, oldest.Data);
                    da.Data = DateHelper.DateRelativeBaseDate(da.Data, oldest.Data);
                }
            }
        }

        public void Randomize(DICOMObject d)
        {
            List<IDICOMElement> dates = d.FindAll(VR.Date);

            foreach (IDICOMElement el in dates)
            {
                Date da = el as Date;
                da.Data = DateHelper.RandomDate;
            }
        }

        public string Name
        {
            get { return "Date Anonymizer"; }
        }
    }
}
