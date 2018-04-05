///////////////////////////////////////////////////////////////////////////////////////////////
ReadMe for Eclipse Simple UI Projects, 7/30/2014
////////////////////////////////////////////////////////////////////////////////////////////////
Projects included:
ESAPISimpleUI - a generic selection box window for selection of Eclipse data objects such as PlanSetup, Beam, Structure,...etc
DemoApp - a stand-alone demo application that generates filtered patient list and shows the selected patient's plans/beams using the generic selection box window in the ESAPISimpleUI project.
==============================================================================
ESAPISimpleUI
The goal of this project is to create a generic selection box window for developers who just need a quick GUI for selection of Eclipse API data objects.  The following Eclipse API data objects are supported:
	Course
	PlanSetup
	Beam
	StructureSet
	Structure

Microsoft WPF and MVVM (model view view-model) design pattern are used for implementation. Other Eclipse API data object can be easy implemented using any of the view-model of the above supported objects as a template.
The list of Eclipse API data objects must be populated by the caller to use the selection window. Type of selection boxes, ListBox or DataGrid, is specified in the constructor by the caller. Single- or Multiple -selection mode can be also specified in the constructor.
The following example codes show how to use the selection window:
    List <VMS.TPS.Common.Model.API.PlanSetup> lstPlans;		// caller to populate the list
	ESAPISimpleUI.View.ListBoxWindow win = new ESAPISimpleUI.View.ListBoxWindow(
		"Test selection box",								// title of the window
		"Select a plan",									// label for the selection box
		System.Windows.Controls.SelectionMode.Single,		// single selection mode, the other option is Multiple
		ESAPISimpleUI.View.ViewType.DataGrid,				// type of the selection box is DataGrid, the other option is ListBox
		lstPlans);											// list of api objects,  which is IEnumerable<VMS.TPS.Common.Model.API.ApiDataObject>
	bool bOK = win.ShowDialog().Value; // show the window and wait for the result
	if (bOK) 
	{
		// cast the selection back to the type of Eclipse object
		List <VMS.TPS.Common.Model.API.PlanSetup> lstSelectedPlans = win.SelectedItems.Cast<VMS.TPS.Common.Model.API.PlanSetup>().ToList();
	}

==============================================================================
DemoApp
The project is a stand-alone Eclipse API program that uses WPF toolkit (Charting) to visualize available categories for filtering patients and demonstrates the use of the generic ESAPISimpleUI selection box window for selecting plans/beams of the selected patient.
Note that the project is only for demonstration purpose since some filtering categories are hard coded. 
WPF toolkit is required to build this project. It can be downloaded from https://wpf.codeplex.com/.
The two DLLs in the toolkit, System.Windows.Controls.DataVisualization.Toolkit.dll and WPFToolkit.dll should be placed in the DemoApp project root directory in order to build the project.


