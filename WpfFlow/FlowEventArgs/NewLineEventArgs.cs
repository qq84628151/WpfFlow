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
        public NewLineEventArgs(LinkBase link)
        {
            Link = link;
        }
    }
}
