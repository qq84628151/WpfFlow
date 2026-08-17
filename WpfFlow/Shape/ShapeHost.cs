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

namespace WpfFlow.Shape
{
    public class ShapeHost : FrameworkElement
    {
        private readonly VisualCollection _children;
        internal VisualCollection Children { get => _children; }

        public ShapeHost()
        {
            _children = new VisualCollection(this);
        }
        protected override Size MeasureOverride(Size availableSize)
        {
            foreach (UIElement element in _children)
            {
                element.Measure(availableSize);
            }
            return base.MeasureOverride(availableSize);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            foreach (UIElement element in _children)
            {
                element.Arrange(new Rect(new Point(0, 0), element.DesiredSize));
            }
            return finalSize;
        }

        protected override int VisualChildrenCount => _children.Count;

        protected override Visual GetVisualChild(int index)
        {
            return _children[index];
        }
    }
}
