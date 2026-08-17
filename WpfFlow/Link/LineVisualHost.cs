//===========================================================================//
//qq：1018720141     qq群：1064754010                                        //
//===========================================================================//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace WpfFlow.Link
{
    public class LineVisualHost : FrameworkElement
    {
        private readonly VisualCollection _children;

        public LineVisualHost(LinkBase link)
        {
            _children = new VisualCollection(this);
            _children.Add(link.Visual);
            this.IsHitTestVisible = false;
        }

        protected override int VisualChildrenCount => _children.Count;

        protected override Visual GetVisualChild(int index)
        {
            return _children[index];
        }

    }
}
