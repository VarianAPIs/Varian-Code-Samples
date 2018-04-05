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
using System.Windows.Documents;//Needed to inherit from the DocumentPaginator class
using System.Windows.Media;//Needed for the Typeface class
using System.Windows;//Needed for the Point and Size objects

using VMS.TPS.Common.Model.API;//Needed to be able to use  Eclipse API objects 
using VMS.TPS.Common.Model.Types;//Needed to be able to use  Eclipse API objects 

namespace MVVM_Demo
{
    //Most of the code for the class is taken from chapter 16 of Matthew MacDonald's book Pro WPF in C# 2010. CSMayo
    class PrintReportDocumentPaginator:DocumentPaginator
    {

        //These methods are specific to this implemenation of the class to be able to pass the Eclipse information to control what is printed. 
        private VMS.TPS.Common.Model.API.Patient curpat;
        private VMS.TPS.Common.Model.API.PlanSetup curps;

        private TPReportData rd1;
        private TPReportData rd2;

        //These members are generic and used for all printing operations
        private int pagecount; 
        private Typeface typeface;
        private double margin;
        private double fontsize;
        private int rowsperpage; 
        private int charperrow; 
        private int lastrowprinted = -1;
        private int currentrow = -1;
        private bool printedpreviousrow = false;
        private bool finishedpage = false;
        private int lastpageprinted = -1;

        private Point PrintPointCurrent;//The location gets updated after calls to the 
        private Point PrintPointCummulative;
        private Size pagesize;


        //These methods need to be modified to instantiate an instance of the class that contains the data you want to use in the report (PrintReportDocumentPaginator) and what you want
        //printed on each page of the report (GetPage).

        public PrintReportDocumentPaginator(TPReportData pass_rd1, TPReportData pass_rd2, Typeface input_typeface, double input_fontSize, double input_margin, Size input_pageSize)
        {
            //Modify the method to include the data objects that you want to use to create the report
            //curpat = input_pat;
            //curps = input_ps;


            rd1 = pass_rd1;
            rd2 = pass_rd2;
            
            //generic members for the class
            margin = input_margin;
            fontsize = input_fontSize;
            pagesize = input_pageSize;
            typeface = input_typeface;

            //First call to determine how many pages there are in the document
            PaginateData();


        }
        public override DocumentPage GetPage(int pageNumber)//This is the method that creates the printed document,
        {
            //Modify the method to display the report using the data objects that you passed to the class.



            FormattedText ft = GetFormattedText("A");// new FormattedText("A", System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, fontsize, Brushes.Black); 

            float tabspace = (float)(pagesize.Width - 2 * margin) / 5.0f;

            DrawingVisual visual = new DrawingVisual();//This is the "blank slate" that we're going to write the report on.

            PrintPointCummulative = new Point(margin, margin);
            PrintPointCurrent = PrintPointCummulative;


            using (DrawingContext dc = visual.RenderOpen())
            {
                

                //TPReportData rd1 = new TPReportData();
                //rd1.SetDemoDate(1);

                //TPReportData rd2 = new TPReportData();
                //rd2.SetDemoDate(2);

                Typeface columnHeaderTypeface = new Typeface(typeface.FontFamily, FontStyles.Normal, FontWeights.Bold, FontStretches.Normal);//Typeface for Headers
                ft = GetFormattedText("Treatment Plans Comparasion Report", columnHeaderTypeface);
                if (IsOnPage(ft.Height, pageNumber)) dc.DrawText(ft, PrintPointCurrent);

                //IsOnPage(ft.Height, pageNumber);
                if (IsOnPage(ft.Height, pageNumber)) dc.DrawLine(new Pen(Brushes.Blue, 2), new Point(margin, PrintPointCurrent.Y), new Point(pagesize.Width - margin, PrintPointCurrent.Y));

                // build general info
                string outputline = string.Empty;
                AddSubTitle("General Info:", ft, dc, pageNumber, columnHeaderTypeface);
                outputline = "Patients\tPatient Name\tPatientID\t\tCourseID\t\tPlanID";
                AddNewContent(outputline, ft, dc, pageNumber, columnHeaderTypeface);
                outputline = "Patient1";
                outputline += "\t" +rd1.CurPatientInfo.LastName + ", " + rd1.CurPatientInfo.FirstName;
                outputline += "\t\t" + rd1.CurPatientInfo.PatientID;
                outputline += "\t\t" + rd1.CurPlanInfo.CourseID + "\t\t" + rd1.CurPlanInfo.PlaneID;
                AddNewContent(outputline, ft, dc, pageNumber, columnHeaderTypeface);

                outputline = "Patient2";
                outputline += "\t" + rd2.CurPatientInfo.LastName + ", " + rd2.CurPatientInfo.FirstName;
                outputline += "\t\t" + rd2.CurPatientInfo.PatientID;
                outputline += "\t\t" + rd2.CurPlanInfo.CourseID + "\t\t" + rd2.CurPlanInfo.PlaneID;
                AddNewContent(outputline, ft, dc, pageNumber, columnHeaderTypeface);

                // build Dose Presciption info
                AddSpace(ft, dc, pageNumber, columnHeaderTypeface);
                AddSubTitle("Dose Presciption Info:", ft, dc, pageNumber, columnHeaderTypeface);
                outputline = "Patients\t\tNormalization\t\tNFractions\tDose/fx\t\tTotalDose";
                AddNewContent(outputline, ft, dc, pageNumber, columnHeaderTypeface);
                outputline = "Patient1";
                outputline += "\t\t" + rd1.GetDosePrescriptionString();
                AddNewContent(outputline, ft, dc, pageNumber, columnHeaderTypeface);

                outputline = "Patient2";
                outputline += "\t\t" + rd2.GetDosePrescriptionString();
                AddNewContent(outputline, ft, dc, pageNumber, columnHeaderTypeface);

                // build Field Info
                AddSpace(ft, dc, pageNumber, columnHeaderTypeface);
                AddSubTitle("Treatment Beams(Patient1)", ft, dc, pageNumber, columnHeaderTypeface);
                AddFiedTable(rd1.CurFieldInfoList, ft, dc, pageNumber, columnHeaderTypeface);

                AddSpace(ft, dc, pageNumber, columnHeaderTypeface);
                AddSubTitle("Treatment Beams(Patient2)", ft, dc, pageNumber, columnHeaderTypeface);
                AddFiedTable(rd2.CurFieldInfoList, ft, dc, pageNumber, columnHeaderTypeface);

                // build structure info
                AddSpace(ft, dc, pageNumber, columnHeaderTypeface);
                AddSubTitle("Structure Info (Patient1/Patient2):", ft, dc, pageNumber, columnHeaderTypeface);
                outputline = "StructureID\t\tType\t\tVolume\t\tMinDos\t\tMaxDose";
                AddNewContent(outputline, ft, dc, pageNumber, columnHeaderTypeface);


               // foreach (StructureData sd1 in rd1.CurStructureDataList)
               // {
               //     if (rd2.CurStructureDataList.Where(x => x.StructureID == sd1.StructureID).Any())
               //     {
               //         StructureData sd2 = rd2.CurStructureDataList.Where(x => x.StructureID == sd1.StructureID).First();
               //         outputline = sd1.StructureID
               //     }
               // }



                bool bHasStructureSetPatient1 = rd1.CurStructureDataList != null && rd1.CurStructureDataList.Count >0;
                 bool bHasStructureSetPatient2 = rd2.CurStructureDataList != null && rd2.CurStructureDataList.Count >0;
                int iCountP1 = rd1.CurStructureDataList != null ? rd1.CurStructureDataList.Count:0;
                 int iCountP2 = rd2.CurStructureDataList != null ? rd2.CurStructureDataList.Count:0;
                int iMinCount = Math.Min(iCountP1, iCountP2);
                if (iMinCount >0)
                {
                    for (int i = 0; i < iMinCount; i++)
                    {
                        if (rd1.CurStructureDataList[i].StructureID == rd2.CurStructureDataList[0].StructureID)
                        {
                            outputline = rd1.CurStructureDataList[i].StructureID;
                            outputline += "\t\t" + rd1.CurStructureDataList[i].Type + "/" + rd2.CurStructureDataList[i].Type;
                            string strTemp = rd1.CurStructureDataList[i].Volume + "/" + rd2.CurStructureDataList[i].Volume;
                            outputline += "\t\t" + rd1.CurStructureDataList[i].Volume + "/" + rd2.CurStructureDataList[i].Volume;
                            string strFormat = strTemp.Length > 12 ? "\t" : "\t\t";
                            outputline += strFormat + rd1.CurStructureDataList[i].MinDose + "/" + rd2.CurStructureDataList[i].MinDose;
                            outputline += "\t\t" + rd1.CurStructureDataList[i].MaxDose + "/" + rd2.CurStructureDataList[i].MaxDose;
                            AddNewContent(outputline, ft, dc, pageNumber, columnHeaderTypeface);
                        }
                        
                    
                    }
                
                }
                




            }


            return new DocumentPage(visual, pagesize, new Rect(pagesize), new Rect(pagesize));

        }

        //public override DocumentPage GetPage(int pageNumber)//This is the method that creates the printed document,
        //{
        //    //Modify the method to display the report using the data objects that you passed to the class.

        //    FormattedText ft = GetFormattedText("A");// new FormattedText("A", System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, fontsize, Brushes.Black); 

        //    float tabspace = (float)(pagesize.Width - 2 * margin) / 5.0f;

        //    DrawingVisual visual = new DrawingVisual();//This is the "blank slate" that we're going to write the report on.

        //    PrintPointCummulative = new Point(margin, margin);
        //    PrintPointCurrent = PrintPointCummulative;


        //    using (DrawingContext dc = visual.RenderOpen())
        //    {
        //        string strDoseUnit = "";

        //        Typeface columnHeaderTypeface = new Typeface(typeface.FontFamily, FontStyles.Normal, FontWeights.Bold, FontStretches.Normal);//Typeface for Headers
        //        ft = GetFormattedText("Treatment Plan Report", columnHeaderTypeface);
        //        if (IsOnPage(ft.Height, pageNumber)) dc.DrawText(ft, PrintPointCurrent);

        //        //IsOnPage(ft.Height, pageNumber);
        //        if (IsOnPage(ft.Height, pageNumber)) dc.DrawLine(new Pen(Brushes.Blue, 2), new Point(margin, PrintPointCurrent.Y), new Point(pagesize.Width - margin, PrintPointCurrent.Y));

        //        ft = GetFormattedText("Summary Info:", columnHeaderTypeface);
        //        if (IsOnPage(ft.Height, pageNumber)) dc.DrawText(ft, PrintPointCurrent);
        //        if (IsOnPage(ft.Height, pageNumber))
        //            // dc.DrawLine(new Pen(Brushes.Black, 1), new Point(1.8 * ft.Width, PrintPointCurrent.Y + ft.Height / 2), new Point(pagesize.Width - margin, PrintPointCurrent.Y + ft.Height / 2));
        //            dc.DrawLine(new Pen(Brushes.Black, 1), new Point(margin + 20, PrintPointCurrent.Y + ft.Height / 2), new Point(pagesize.Width - margin, PrintPointCurrent.Y + ft.Height / 2));

        //        string outputline = string.Empty;
        //        outputline = "\tPatient Name: " + curpat.FirstName + " " + curpat.LastName;
        //        outputline += "\t\tPatient ID: " + curpat.Id;
        //        ft = GetFormattedText(outputline);
        //        if (IsOnPage(ft.Height, pageNumber)) dc.DrawText(ft, PrintPointCurrent);


        //        outputline = "\tCourse ID: " + curps.Course.Id;
        //        outputline += "\t\tPlan ID: " + curps.Id;
        //        ft = GetFormattedText(outputline);
        //        if (IsOnPage(ft.Height, pageNumber)) dc.DrawText(ft, PrintPointCurrent);


        //        // build Dose prescription info
        //        AddSpace(ft, dc, pageNumber, columnHeaderTypeface);
        //        AddSubTitle("Dose Prescription:", ft, dc, pageNumber, columnHeaderTypeface);
        //        outputline = "\tPlan Normalization: \t" + curps.PlanNormalizationValue.ToString() + "%";
        //        AddNewContent(outputline, ft, dc, pageNumber, columnHeaderTypeface);
        //        outputline = "\tFractionation: \t\t" + curps.UniqueFractionation.Id;
        //        AddNewContent(outputline, ft, dc, pageNumber, columnHeaderTypeface);
        //        outputline = "\t\tPrescribed Dose, Fractionation, %: \t";
        //        outputline += (curps.UniqueFractionation.NumberOfFractions.Value * curps.UniqueFractionation.PrescribedDosePerFraction).ToString() + strDoseUnit;
        //        outputline += " (" + curps.UniqueFractionation.PrescribedDosePerFraction + strDoseUnit + "/" + curps.UniqueFractionation.NumberOfFractions.ToString();
        //        outputline += " Fraction) (" + curps.PlanNormalizationValue.ToString() + "%)";
        //        AddNewContent(outputline, ft, dc, pageNumber, columnHeaderTypeface);



        //        // build beam info
        //        AddSpace(ft, dc, pageNumber, columnHeaderTypeface);

        //        ft = GetFormattedText("Treatment Beams", columnHeaderTypeface);
        //        if (IsOnPage(ft.Height, pageNumber)) dc.DrawText(ft, PrintPointCurrent);
        //        if (IsOnPage(ft.Height, pageNumber))
        //        {
        //            // dc.DrawLine(new Pen(Brushes.Black, 1), new Point(1.8 * ft.Width, PrintPointCurrent.Y + ft.Height / 2), new Point(pagesize.Width - margin, PrintPointCurrent.Y + ft.Height / 2));
        //            //dc.DrawLine(new Pen(Brushes.Black, 1), new Point( margin, PrintPointCurrent.Y), new Point(pagesize.Width - margin, PrintPointCurrent.Y));
        //            dc.DrawLine(new Pen(Brushes.Black, 1), new Point(margin + 20, PrintPointCurrent.Y + ft.Height / 2), new Point(pagesize.Width - margin, PrintPointCurrent.Y + ft.Height / 2));
        //        }



        //        outputline = "\tBeam ID\tMU\tMachine\t\tEnergy\tGantry\tColl\tTable\tX1\tX2\tY1\tY2";
        //        ft = GetFormattedText(outputline);
        //        if (IsOnPage(ft.Height, pageNumber)) dc.DrawText(ft, PrintPointCurrent);

        //        foreach (Beam b in curps.Beams)
        //        {
        //            outputline = string.Empty;
        //            outputline += "\t" + b.Id + "\t" + Math.Round(b.Meterset.Value, 0).ToString() + "\t" + b.TreatmentUnit.Id + "\t" + b.EnergyModeDisplayName;
        //            outputline += "\t" + Math.Round(b.ControlPoints[0].GantryAngle, 1).ToString() + "\t" + Math.Round(b.ControlPoints[0].CollimatorAngle, 1).ToString() + "\t" + Math.Round(b.ControlPoints[0].PatientSupportAngle, 1).ToString();
        //            outputline += "\t" + Math.Round(b.ControlPoints[0].JawPositions.X1 / 10.0f, 1).ToString() + "\t" + Math.Round(b.ControlPoints[0].JawPositions.X2 / 10.0f, 1).ToString() + "\t" + Math.Round(b.ControlPoints[0].JawPositions.Y1 / 10.0f, 1).ToString() + "\t" + Math.Round(b.ControlPoints[0].JawPositions.Y2 / 10.0f, 1).ToString();
        //            ft = GetFormattedText(outputline);
        //            if (IsOnPage(ft.Height, pageNumber)) dc.DrawText(ft, PrintPointCurrent);
        //        }

        //        // build Structures info
        //        AddSpace(ft, dc, pageNumber, columnHeaderTypeface);

        //        ft = GetFormattedText("Structures", columnHeaderTypeface);
        //        if (IsOnPage(ft.Height, pageNumber)) dc.DrawText(ft, PrintPointCurrent);
        //        if (IsOnPage(ft.Height, pageNumber))
        //        {

        //            //dc.DrawLine(new Pen(Brushes.Black, 1), new Point(1.8 * ft.Width, PrintPointCurrent.Y + ft.Height / 2), new Point(pagesize.Width - margin, PrintPointCurrent.Y + ft.Height / 2));
        //            dc.DrawLine(new Pen(Brushes.Black, 1), new Point(margin + 20, PrintPointCurrent.Y + ft.Height / 2), new Point(pagesize.Width - margin, PrintPointCurrent.Y + ft.Height / 2));
        //            //dc.DrawLine(new Pen(Brushes.Black, 1), new Point( margin, PrintPointCurrent.Y), new Point(pagesize.Width - margin, PrintPointCurrent.Y ));
        //        }

        //        //outputline = "Structures";
        //        //ft = GetFormattedText(outputline);
        //        //if (IsOnPage(ft.Height, pageNumber)) dc.DrawText(ft, PrintPointCurrent);

        //        outputline = "\tStucture ID\t\tDicomType\tVolume";
        //        ft = GetFormattedText(outputline);
        //        if (IsOnPage(ft.Height, pageNumber)) dc.DrawText(ft, PrintPointCurrent);

        //        foreach (Structure s in curps.StructureSet.Structures.OrderBy(x => x.DicomType).OrderBy(x => x.Id).Where(x => !x.IsEmpty))
        //        {
        //            outputline = string.Empty;
        //            string sFormat1 = s.Id.Length < 9 ? "\t\t\t" : "\t\t";
        //            string sFormat2 = s.DicomType.Length < 7 ? "\t\t" : "\t";
        //            outputline += "\t" + s.Id + sFormat1 + "(" + s.DicomType + ")" + sFormat2 + Math.Round(s.Volume, 2).ToString() + " cc";
        //            ft = GetFormattedText(outputline);
        //            if (IsOnPage(ft.Height, pageNumber)) dc.DrawText(ft, PrintPointCurrent);
        //        }
        //    }


        //    return new DocumentPage(visual, pagesize, new Rect(pagesize), new Rect(pagesize));

        //}

        private void AddSpace(FormattedText ft, DrawingContext dc, int pageNumber, Typeface columnHeaderTypeface)
        {
            ft = GetFormattedText(" ", columnHeaderTypeface);
            if (IsOnPage(ft.Height, pageNumber)) dc.DrawText(ft, PrintPointCurrent);
            ft = GetFormattedText(" ", columnHeaderTypeface);
            if (IsOnPage(ft.Height, pageNumber)) dc.DrawText(ft, PrintPointCurrent);
        }


        private void AddNewContent(string strContent, FormattedText ft, DrawingContext dc, int pageNumber, Typeface columnHeaderTypeface)
        {
            ft = GetFormattedText(strContent);
            if (IsOnPage(ft.Height, pageNumber)) dc.DrawText(ft, PrintPointCurrent);
        }

        private void AddSubTitle(string strTitle, FormattedText ft, DrawingContext dc, int pageNumber, Typeface columnHeaderTypeface)
        {
            ft = GetFormattedText(strTitle, columnHeaderTypeface);
            if (IsOnPage(ft.Height, pageNumber)) dc.DrawText(ft, PrintPointCurrent);
            if (IsOnPage(ft.Height, pageNumber))
                 dc.DrawLine(new Pen(Brushes.Black, 1), new Point(margin + 20, PrintPointCurrent.Y + ft.Height / 2), new Point(pagesize.Width - margin, PrintPointCurrent.Y + ft.Height / 2));
               

        }

        private void AddFiedTable(List<FieldInfo> fieldInfoList, FormattedText ft, DrawingContext dc, int pageNumber, Typeface columnHeaderTypeface)
        {
            string outputline = "";
            outputline = "\tBeam ID\tMU\tMachine\t\tEnergy\tGantry\tColl\tTable\tX1\tX2\tY1\tY2";
            AddNewContent(outputline, ft, dc, pageNumber, columnHeaderTypeface);

            foreach (var b in fieldInfoList)
            {
                string strFormat1 = b.BeamID.Length < 9 ? "\t" : "\t\t";
                outputline = string.Empty;
                outputline += "\t" + b.BeamID + strFormat1 + b.MU + "\t" + b.Machine + "\t" + b.Energy;
                outputline += "\t" + b.Gantry + "\t" + b.Collimator + "\t" + b.Couch;
                outputline += "\t" + b.X1 + "\t" + b.X2 + "\t" + b.Y1 + "\t" + b.Y2;
                AddNewContent(outputline, ft, dc, pageNumber, columnHeaderTypeface);
            }



        }



        private void PaginateData()
        {
            FormattedText ft = GetFormattedText("A");// new FormattedText("A", System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, fontsize, Brushes.Black);

            rowsperpage = (int)((pagesize.Height - margin * 2) / ft.Height);

            //Leave a row for the headings
            rowsperpage -= 1;
            charperrow = (int)((pagesize.Width - margin * 2) / ft.Width);

            //Need to modify the line that determines how many pages there are in the document
            pagecount = 3;
            // pagecount = (int)Math.Ceiling((double)(rx.TargetConstraints.Constraints.Count + rx.NormalTissueConstraints.Constraints.Count) / rowsperpage);
            pagecount++;
            // Count the lines that fit on a page

        }

        

        //These methods are generic and used for all printing operations
        public override Size PageSize
        {
            get
            {
                return pagesize;
            }
            set
            {
                pagesize = value;
                PaginateData();
            }
        }
        public bool IsOnPage(double deltay, int pageNumber)
        {
            bool returnvalue = false;



            if (lastpageprinted != pageNumber)
            {
                lastpageprinted = pageNumber;
                finishedpage = false;
                currentrow = 0;
                //printedpreviousrow = false;
            }

            currentrow++;
            if (currentrow > lastrowprinted && !finishedpage)
            {
                if (!printedpreviousrow)
                {
                    PrintPointCurrent.Y = margin;
                }
                else
                {
                    PrintPointCurrent.Y += deltay;
                }


                if (PrintPointCurrent.Y >= margin && PrintPointCurrent.Y <= pagesize.Height - 2.0f * margin)
                {
                    returnvalue = true;
                    lastrowprinted = currentrow;
                    printedpreviousrow = true;
                    finishedpage = false;
                }
                else
                {
                    finishedpage = true;
                    printedpreviousrow = false;
                    returnvalue = false;

                }

            }
            return returnvalue;
        }
        private FormattedText GetFormattedText(string text)
        {
            return GetFormattedText(text, typeface);
        }
        private FormattedText GetFormattedText(string text, Typeface typeface)
        {
            return new FormattedText(text, System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, fontsize, Brushes.Black);
        }
        public override bool IsPageCountValid
        {
            get { return true; } //Alway returns true, because the page count is updated immediately and synchronosly, when the page size changes. It is never left in an indeterminant state
        }
        public override int PageCount
        {
            get { return pagecount; }
        }
        public override IDocumentPaginatorSource Source
        {
            get { return null; }
        }


    }
}
