//===========================================================================//
//qq：1018720141     qq群：1064754010                                        //
//===========================================================================//
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfFlow.Enum;

namespace WpfFlow.Other
{
    public class RectShapeCollection<T> : ObservableCollectionExt<T>
    {
        internal ShapeBase Shape { get;set; }
        internal PortDirection Dir { get; set; }
        internal RectShapeCollection(ShapeBase shape, PortDirection dir)
        {
            Shape = shape;
            Dir = dir;
        }
    }
}
