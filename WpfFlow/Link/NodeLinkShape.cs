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
using WpfFlow.Enum;

namespace WpfFlow
{
    /// <summary>
    /// 节点线
    /// </summary>
    public class NodeLinkShape : LinkBase
    {
        /// <summary>
        /// 线类型
        /// </summary>
        private NodeLinkType _lineType = NodeLinkType.Line;
        public NodeLinkType LineType
        {
            get => this._lineType;
            set { this._lineType = value; NotifyPropertyChanged(); }
        }


        internal override void Render()
        {
            if (Geometry == null)
            {
                Visual.RenderOpen().Close();
                return;
            }

            UpdateLabelsPosition();

            using (DrawingContext ctx = Visual.RenderOpen())
            {
                if (StrokePen3 != null) ctx.DrawGeometry(null, StrokePen3, Geometry);
                if (StrokePen2 != null) ctx.DrawGeometry(null, StrokePen2, Geometry);
                if (StrokePen != null) ctx.DrawGeometry(null, StrokePen, Geometry);
            }
        }

        private Point? sourceCenter = null;
        private Point? targetCenter = null;
        internal void DrawGeometry(Visual container)
        {
            sourceCenter = null;
            targetCenter = null;

            if (SourceRect == null || TargetRect == null)
            {
                Geometry = null;
                return;
            }

            if (SourceRect is RectShape sourceRect)
            {
                sourceCenter = sourceRect.Position + new Point(sourceRect.Size.Width / 2, sourceRect.Size.Height / 2);
            }

            if (TargetRect is RectShape targetRect)
            {
                targetCenter = targetRect.Position + new Point(targetRect.Size.Width / 2, targetRect.Size.Height / 2);
            }

            ReGeometry();
        }
        internal void ReGeometry()
        {
            if (sourceCenter == null || targetCenter == null) return;

            var geo = new StreamGeometry();

            using (var ctx = geo.Open())
            {
                ctx.BeginFigure(sourceCenter.Value, false, false);
                if (LineType == NodeLinkType.Line)
                {
                    ctx.LineTo(targetCenter.Value, true, false);
                }

                if (LineType == NodeLinkType.Bezier)
                {
                    ctx.QuadraticBezierTo(new Point(sourceCenter.Value.X, targetCenter.Value.Y), targetCenter.Value, true, false);
                }
            }

            geo.Freeze();
            base.Geometry = geo;
        }
    }
}
