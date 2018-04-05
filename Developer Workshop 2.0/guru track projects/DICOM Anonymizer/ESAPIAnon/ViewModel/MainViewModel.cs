
using ESAPIAnon.Settings;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.IO;
using System.Reflection;
using System.Windows;
using VMS.TPS.Common.Model.API;
using System.Linq;
using ESAPIAnon;
using FetchDicom;
using System;
using EvilDICOM.Core.IO.Reading;
using ESAPIAnon.Anonymizers;
using EvilDICOM.Core.IO.Writing;
using System.Windows.Forms;

namespace DICOMUI.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        ScriptContext sc;
        public MainViewModel(ScriptContext context)
        {
            sc = context;

            DoAnonymizeStudyIDs = true;
            DoAnonymizeUIDs = true;
            DateSettings = ESAPIAnon.Settings.DateSettings.NULL_AGE;
            DoRemovePrivateTags = true;
            DoPlans = DoStructures = DoDoses = DoImages = true;
            DoResendToDaemon = false;
            FirstName = "";
            LastName = "";
            Id = "";

            AnonymizeCommand = new RelayCommand(() =>
            {
                //Get current patient id
                var id = GetPatientId();
                //Get current directory // Make patient folder if not exists
                var path = Helpers.AssemblyDirectory;
                var dir = Path.Combine(path, id);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                //Read settings file
                var ip = "100.75.16.108";
                var scpAE = "Db Daemon";
                var scpPort = 104;

                UpdateStatus("Getting DICOM Files from the Daemon...");
                CMove.GenerateDicomFiles(sc.Patient.Id, ip, scpPort.ToString(), scpAE, dir);
                UpdateStatus("Anonymizing...");
                var files = Directory.GetFiles(dir);
                var dcms = files.ToList().Select(f => DICOMFileReader.Read(f)).ToList();
                var settings = AnonymizeSettings.Generate(this);
                var que = AnonymizationQue.Build(settings,dcms);
                dcms.ForEach(d => que.Anonymize(d));

                //Let user choose output
                FolderBrowserDialog fbd = new FolderBrowserDialog();
                fbd.Description = "Select DICOM File destination folder";
                fbd.RootFolder = Environment.SpecialFolder.Desktop;
                DialogResult result = fbd.ShowDialog();
                if (result == DialogResult.OK)
                {
                    var output = fbd.SelectedPath;
                    int i = 0;
                    foreach (var dcm in dcms)
                    {
                        var fileOut = Path.Combine(output, string.Format("{0}.dcm", i));
                        i++;
                        DICOMFileWriter.WriteLittleEndian(fileOut, dcm);
                    }

                    //Clean up directory
                    Directory.Delete(dir, true);
                    UpdateStatus("Complete!");
                }
                else
                {
                    System.Windows.MessageBox.Show("You must select a file location!");
                }
                
            });

        }

        private void UpdateStatus(string status)
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                this.Status = status;
                RaisePropertyChanged(() => Status);
            }), null);
        }

        private string GetPatientId()
        {
            return sc.Patient.Id;
        }

        public string Status { get; set; }
        public bool DoAnonymizeStudyIDs { get; set; }
        public bool DoAnonymizeUIDs { get; set; }
        public DateSettings DateSettings { get; set; }
        public bool DoRemovePrivateTags { get; set; }
        public bool DoPlans { get; set; }
        public bool DoStructures { get; set; }
        public bool DoImages { get; set; }
        public bool DoDoses { get; set; }
        public bool DoResendToDaemon { get; set; }
        public RelayCommand AnonymizeCommand { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Id { get; set; }
    }
}