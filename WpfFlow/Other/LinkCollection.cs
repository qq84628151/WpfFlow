//===========================================================================//
//qq：1018720141     qq群：1064754010                                        //
//===========================================================================//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfFlow.Other
{
    public class LinkCollection<T> : ObservableCollectionExt<T>
    {
        internal LinkBase Link { get; set; }
        internal LinkCollection(LinkBase link)
        {
            Link = link;
        }
    }
}
