using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ESAPIAnon.Settings
{
    public class AnonymizeSettings
    {
        //OPTIONS
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Id { get; set; }
        public bool DoAnonymizeStudyIDs { get; set; }
        public bool DoAnonymizeUIDs { get; set; }
        public DateSettings DateSettings { get; set; }
        public bool DoRemovePrivateTags { get; set; }

        public static AnonymizeSettings Default
        {
            get
            {
                return new AnonymizeSettings
                {
                    FirstName = string.Empty,
                    LastName = string.Empty,
                    Id = string.Empty,
                    DoAnonymizeStudyIDs = true,
                    DoAnonymizeUIDs = true,
                    DateSettings = DateSettings.PRESERVE_AGE,
                    DoRemovePrivateTags = true
                };
            }
        }

        public static AnonymizeSettings Generate(DICOMUI.ViewModel.MainViewModel mainViewModel)
        {
            var settings = new AnonymizeSettings();
            settings.FirstName = mainViewModel.FirstName;
            settings.LastName = mainViewModel.LastName;
            settings.Id = mainViewModel.Id;
            settings.DoRemovePrivateTags = mainViewModel.DoRemovePrivateTags;
            settings.DoAnonymizeStudyIDs = mainViewModel.DoAnonymizeStudyIDs;
            settings.DoAnonymizeUIDs = mainViewModel.DoAnonymizeUIDs;
            settings.DateSettings = mainViewModel.DateSettings;
            return settings;
        }
    }
}
