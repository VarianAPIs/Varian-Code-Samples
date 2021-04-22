using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeamDataVisualization.Models
{
    public class BeamScanModel
    {
        public string Energy { get; set; }
        public BeamScanTypeEnum BeamScanType { get; set; }
        public double FieldX { get; set; }
        public double FieldY { get; set; }
        public string DisplayTxt { get; set; }
        public double Depth { get; set; }
        public List<BeamDataPointModel> BeamDataPoints { get; set; }
        public BeamScanModel()
        {
            BeamDataPoints = new List<BeamDataPointModel>();
        }
    }
}
