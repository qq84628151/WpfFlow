//===========================================================================//
//qq：1018720141     qq群：1064754010                                        //
//===========================================================================//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WpfFlow.FlowEventArgs
{
    public class NewLineEventArgs : EventArgs
    {
        public LinkBase Link { get; set; }
        public ShapeBase Source { get;set; }
        public ShapeBase Target { get; set; }
        public Port SourcePort { get; set;}
        public Port TargetPort { get; set; }

        public NewLineEventArgs(LinkBase link, ShapeBase source, ShapeBase target, Port sourcePort, Port targetPort)
        {
            Link = link;
            Source = source;
            Target = target;
            SourcePort = sourcePort;
            TargetPort = targetPort;
        }
    }
}
