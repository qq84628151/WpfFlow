//===========================================================================//
//qq：1018720141     qq群：1064754010                                        //
//===========================================================================//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace WpfFlow.FlowEventArgs
{
    public class ShapeMouseEventArgs : EventArgs
    {
        public Point Position { get; set; }
        public ShapeMouseEventArgs(Point position)
        {
            Position = position;
        }
    }
}
