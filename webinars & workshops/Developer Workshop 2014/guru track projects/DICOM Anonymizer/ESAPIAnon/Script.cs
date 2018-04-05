using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Collections.Generic;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using DICOMUI;
using DICOMUI.ViewModel;
using System.Windows.Controls;

namespace VMS.TPS
{
  public class Script
  {
    public Script()
    {
    }

    public void Execute(ScriptContext context , System.Windows.Window window)
    {
        new Splash().ShowDialog();
        Frame baseFrame = new Frame();
        var main = new MainWindow();
        main.DataContext = new ViewModelLocator(context).Main;
        baseFrame.Content = main;
        window.Title = "ESAPI Anonymizer";
        window.Content = baseFrame;
        window.SizeToContent = SizeToContent.WidthAndHeight;
        window.ResizeMode = ResizeMode.NoResize;
    }
  }
}
