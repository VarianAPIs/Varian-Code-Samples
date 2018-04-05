#region copyright
////////////////////////////////////////////////////////////////////////////////
//
// Copyright (c) 2014 Varian Medical Systems, Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy 
// of this software and associated documentation files (the "Software"), to deal 
// in the Software without restriction, including without limitation the rights 
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
// copies of the Software, and to permit persons to whom the Software is 
// furnished to do so, subject to the following conditions:
//
//  The above copyright notice and this permission notice shall be included in 
//  all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN 
// THE SOFTWARE.
//////////////////////////////////////////////////////////////////////////////////
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MVVM_Demo
{
    public class TPReportData
    {
        public PatientInfo CurPatientInfo = new PatientInfo();
        public PlanInfo CurPlanInfo = new PlanInfo();
        public DosePrescription CurDosePrescription = new DosePrescription();

        public List<FieldInfo> CurFieldInfoList = new List<FieldInfo>();
        public List<StructureData> CurStructureDataList = new List<StructureData>();

        public void SetDemoDate(int iDateSet =0 )
        { 
            //patient info
            CurPatientInfo.FirstName = (iDateSet == 0) ? "" : "Patient"+ iDateSet.ToString()+ "FN"; 
            CurPatientInfo.LastName = (iDateSet == 0) ? "" : "Patient"+ iDateSet.ToString()+ "LN"; 
            CurPatientInfo.PatientID = (iDateSet == 0) ? "" :"Patient"+ iDateSet.ToString()+ "ID"; 

            //plan info
            CurPlanInfo.CourseID = (iDateSet == 0) ? "" : "Patient" + iDateSet.ToString() + "_CID";
            CurPlanInfo.PlaneID = (iDateSet == 0) ? "" : "Patient" + iDateSet.ToString() + "_PID"; 

            // Dose CDosePrescription
            CurDosePrescription.Fractionation = (iDateSet == 0) ? "" : "Patient" + iDateSet.ToString() + "_F1";
            CurDosePrescription.PlanNormalization = (iDateSet == 0) ? "" :(100 - iDateSet).ToString() + "%";
            CurDosePrescription.PrescribedDoseFractination = (iDateSet == 0) ? "" : (1.6 +iDateSet).ToString() + " Gy";
            CurDosePrescription.NumberOfFraction = (iDateSet == 0) ? "" : (30 + iDateSet).ToString() ;
            CurDosePrescription.TotalPrescribedDose = (iDateSet == 0) ? "" : ((1.6 + iDateSet) * (30 + iDateSet)).ToString();

            // FieldInfor
            FieldInfo fieldInfo1 = new FieldInfo();
            fieldInfo1.BeamID = (iDateSet == 0) ? "" : "P" + iDateSet.ToString() + "_Feild1";
            fieldInfo1.Collimator = (iDateSet == 0) ? "" : iDateSet.ToString() ;
            fieldInfo1.Gantry = (iDateSet == 0) ? "" : iDateSet.ToString();
            fieldInfo1.Couch = (iDateSet == 0) ? "" : iDateSet.ToString(); 
            fieldInfo1.Machine = (iDateSet == 0) ? "" : "Machine" + iDateSet.ToString();
            fieldInfo1.Energy = (iDateSet == 0) ? "" : "Energy" + iDateSet.ToString();
            fieldInfo1.MU = (iDateSet == 0) ? "" : "MU" + iDateSet.ToString();
            fieldInfo1.DoseRate = (iDateSet == 0) ? "" : "DoseRate" + iDateSet.ToString();
            fieldInfo1.X1 = (iDateSet == 0) ? "" : "0.5" + iDateSet.ToString();
            fieldInfo1.X2 = (iDateSet == 0) ? "" : "11." + iDateSet.ToString();
            fieldInfo1.Y1 = (iDateSet == 0) ? "" : "7." + iDateSet.ToString();
            fieldInfo1.Y2 = (iDateSet == 0) ? "" : "9." + iDateSet.ToString();

            CurFieldInfoList.Add(fieldInfo1);


            // SturctureData
            StructureData structureData1 = new StructureData();
            structureData1.StructureID = (iDateSet == 0) ? "" : "PTV" ;
            structureData1.Type = (iDateSet == 0) ? "" : "Body" + iDateSet.ToString();
            structureData1.Volume = (iDateSet == 0) ? "" : "CTVal" + iDateSet.ToString();
            structureData1.MinDose = (iDateSet == 0) ? "" : "20." + iDateSet.ToString();
            structureData1.MaxDose = (iDateSet == 0) ? "" : "70." + iDateSet.ToString();
            CurStructureDataList.Add(structureData1);
        }

        public string GetGneralInfoString()
        {

            string outputline = null;
            outputline += CurPatientInfo.LastName + ", " + CurPatientInfo.FirstName;
            outputline += "\t" + CurPatientInfo.PatientID;
            outputline += "\t" + CurPlanInfo.CourseID;
            outputline += "\t" + CurPlanInfo.PlaneID;
             return outputline;

        }


        public string GetDosePrescriptionString()
        {

            string outputline = null;
            outputline +=  CurDosePrescription.PlanNormalization;
            outputline += "\t\t" + CurDosePrescription.Fractionation;
            outputline += "\t" + CurDosePrescription.NumberOfFraction;
            outputline += "\t\t" + CurDosePrescription.PrescribedDoseFractination;
            outputline += "\t\t" + CurDosePrescription.TotalPrescribedDose;
            return outputline;
         
        }


        
    }

    public class PatientInfo
    {
        // member vPatientInforiables
        public string FirstName;
        public string LastName;
        public string PatientID;
        
        public PatientInfo()
         { }
    }


    public class PlanInfo
    {

        public string CourseID;
        public string PlaneID;
        public PlanInfo()
         { }
    }


    public class DosePrescription
    {
        public string PlanNormalization;   // percentage
        public string Fractionation;
        public string NumberOfFraction;
        public string PrescribedDoseFractination;
        public string TotalPrescribedDose;
       

        public DosePrescription()
            { }
    }

      public class FieldInfo
        {
            
            public string BeamID;
            public string Machine;
            public string Energy;
            public string DoseRate;
            public string Gantry;
            public string Collimator;
            public string Couch;
            public string X1;
            public string X2;
            public string Y1;
            public string Y2;
             public string MU;

            public FieldInfo()
            { }
        }

     public class StructureData
     {
            public string StructureID;
            public string Type;
            public string Volume;
            public string MaxDose;
            public string MinDose;

            public StructureData()
            { }
        }

      
}
    





