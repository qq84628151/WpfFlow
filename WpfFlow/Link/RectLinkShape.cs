//===========================================================================//
//qq：1018720141     qq群：1064754010                                        //
//===========================================================================//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using WpfFlow.Enum;
using WpfFlow.Helper;

namespace WpfFlow
{
    /// <summary>
    /// 矩形节点连线
    /// </summary>
    public class RectLinkShape : LinkBase
    {
        internal const double BEZIER_REVER_LIMIT_LENGTH = 80;
        internal const double BEZIER_REVER_LIMIT_LENGTH_FACTOR = BEZIER_REVER_LIMIT_LENGTH / 4;

        private PortDirection _sourcre_direction;
        /// <summary>
        /// 源节点的端口方向
        /// </summary>
        public PortDirection SourcreDirection
        {
            get => this._sourcre_direction;
            set { this._sourcre_direction = value; NotifyPropertyChanged(); }
        }

        private PortDirection _target_direction;
        /// <summary>
        /// 目标节点的端口方向
        /// </summary>
        public PortDirection TargetDirection
        {
            get => this._target_direction;
            set { this._target_direction = value; NotifyPropertyChanged(); }
        }


        private int _sourcre_index;
        /// <summary>
        /// 源节点的端口索引
        /// </summary>
        public int SourcreIndex
        {
            get => this._sourcre_index;
            set { this._sourcre_index = value; NotifyPropertyChanged(); }
        }

        private int _target_index;
        /// <summary>
        /// 目标节点的端口索引
        /// </summary>
        public int TargetIndex
        {
            get => this._target_index;
            set { this._target_index = value; NotifyPropertyChanged(); }
        }

        private double _arc_radius = 3;
        /// <summary>
        /// 线的拐角半径
        /// </summary>
        [DefaultValue(3.0)]
        public double ArcRadius
        {
            get => this._arc_radius;
            set { this._arc_radius = value; NotifyPropertyChanged(); }
        }

        private RectLinkType _lineType = RectLinkType.Manhattan;
        /// <summary>
        /// 线类型
        /// </summary>
        public RectLinkType LineType
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
        internal static LinkOutterPoint GetJoinPoint(RectShape rect, Port port, double outter, out Vector dir)
        {
            var bound = VisualTreeHelper.GetDescendantBounds(port);
            if (bound.IsEmpty) bound = new Rect(0, 0, port.RenderSize.Width, port.RenderSize.Height);

            var realBound = port.TransformToAncestor(rect.Panel).TransformBounds(bound);
            var center = realBound.Location;

            if (port.Dir == PortDirection.Right)
            {
                center.X = realBound.X + realBound.Width;
            }

            if (port.Dir == PortDirection.Left || port.Dir == PortDirection.Right)
            {
                center.Y = realBound.Y + realBound.Height / 2;
            }

            if (port.Dir == PortDirection.Bottom)
            {
                center.Y = realBound.Y + realBound.Height;
            }

            if (port.Dir == PortDirection.Top || port.Dir == PortDirection.Bottom)
            {
                center.X = realBound.X + realBound.Width / 2;
            }

            center = rect.Panel.RenderTransform.Transform(center);
            dir = CommonHelper.GetAngleByDir(rect, center);
            return new LinkOutterPoint(center, center + dir * outter);
        }
        private Vector _srcDir;
        private Vector _tarDir;
        private LinkOutterPoint _SourcePoint;
        private LinkOutterPoint _TargetPoint;
        internal void DrawGeometry(double outter, Point? targetPoint = null)
        {
            var srcRect = SourceRect as RectShape;
            var tarRect = TargetRect as RectShape;
            if (SourcreDirection == PortDirection.None || srcRect == null || SourcreIndex < 0 || SourcreIndex >= srcRect.Ports[(int)SourcreDirection - 1].Count || 
                (targetPoint == null && (tarRect == null || TargetDirection == PortDirection.None || TargetIndex < 0 || TargetIndex >= tarRect.Ports[(int)TargetDirection - 1].Count)))
            {
                Geometry = null;
                return;
            }
            var srcPort = srcRect.Ports[(int)SourcreDirection - 1][SourcreIndex];
            var tarPort = targetPoint == null ? tarRect.Ports[(int)TargetDirection - 1][TargetIndex] : null;
            _SourcePoint = GetJoinPoint(srcRect, srcPort, outter, out _srcDir);
            if (targetPoint != null)
            {
                _TargetPoint = new LinkOutterPoint(targetPoint.Value, targetPoint.Value);
                _tarDir = Vector.Multiply(_srcDir, targetPoint.Value - _SourcePoint.OutterCenter) > 0 ? -_srcDir : _srcDir;
            }
            else
            {
                _TargetPoint = GetJoinPoint(tarRect, tarPort, outter, out _tarDir);
            }
            CreateGeometry(_srcDir, _tarDir, _SourcePoint, _TargetPoint);
        }
        internal void ReDrawGeometry()
        {
            CreateGeometry(_srcDir, _tarDir, _SourcePoint, _TargetPoint);
        }
        internal void CreateGeometry(Vector sourceDir, Vector targetDir, LinkOutterPoint SourcePoint, LinkOutterPoint TargetPoint)
        {
            var geo = new StreamGeometry();

            if (LineType == RectLinkType.Manhattan)
            {
                using (var ctx = geo.Open())
                {
                    ctx.BeginFigure(SourcePoint.Center, false, false);

                    var absDirX = Math.Abs(sourceDir.X);
                    var absDirY = Math.Abs(sourceDir.Y);
                    var sourceSameDirPoint = new Vector(SourcePoint.OutterCenter.X * absDirX, SourcePoint.OutterCenter.Y * absDirY);
                    var targetSameDirPoint = new Vector(TargetPoint.OutterCenter.X * absDirX, TargetPoint.OutterCenter.Y * absDirY);
                    var dir = targetSameDirPoint - sourceSameDirPoint;
                    var len = dir.Length;
                    var sourceSameDirSwapPoint = new Vector(SourcePoint.OutterCenter.X * absDirY, SourcePoint.OutterCenter.Y * absDirX);
                    var targetSameDirSwapPoint = new Vector(TargetPoint.OutterCenter.X * absDirY, TargetPoint.OutterCenter.Y * absDirX);
                    var dirSwap = targetSameDirSwapPoint - sourceSameDirSwapPoint;
                    var lenSwap = dirSwap.Length;
                    var rawDirSwap = new Vector(Math.Sign(dirSwap.X * absDirY), Math.Sign(dirSwap.Y * absDirX));

                    if (-sourceDir == targetDir)
                    {
                        var lenHalf = len / 2;
                        var lenHalfSwap = lenSwap / 2;
                        if (dir * sourceDir > 0)
                        {
                            ctx.LineTo(SourcePoint.OutterCenter, true, false);

                            Point current = SourcePoint.OutterCenter + sourceDir * lenHalf;
                            Point current2 = TargetPoint.OutterCenter + -sourceDir * lenHalf;
                            var (arcStart, arcEnd, sweep) = CommonHelper.GetArcBy3Point(SourcePoint.OutterCenter, current, current2, ArcRadius);
                            ctx.LineTo(arcStart, true, false);
                            ctx.ArcTo(arcEnd, new Size(ArcRadius, ArcRadius), 0, false, sweep, true, false);

                            var (arcStart2, arcEnd2, sweep2) = CommonHelper.GetArcBy3Point(current, current2, TargetPoint.OutterCenter, ArcRadius);
                            ctx.LineTo(arcStart2, true, false);
                            ctx.ArcTo(arcEnd2, new Size(ArcRadius, ArcRadius), 0, false, sweep2, true, false);

                            ctx.LineTo(TargetPoint.OutterCenter, true, false);
                        }
                        else
                        {
                            Point current = SourcePoint.OutterCenter + rawDirSwap * lenHalfSwap;
                            Point current2 = TargetPoint.OutterCenter + -rawDirSwap * lenHalfSwap;

                            var (arcStart_start, arcEnd_start, sweep_start) = CommonHelper.GetArcBy3Point(SourcePoint.Center, SourcePoint.OutterCenter, current, ArcRadius);
                            ctx.LineTo(arcStart_start, true, false);
                            ctx.ArcTo(arcEnd_start, new Size(ArcRadius, ArcRadius), 0, false, sweep_start, true, false);

                            var (arcStart, arcEnd, sweep) = CommonHelper.GetArcBy3Point(SourcePoint.OutterCenter, current, current2, ArcRadius);
                            ctx.LineTo(arcStart, true, false);
                            ctx.ArcTo(arcEnd, new Size(ArcRadius, ArcRadius), 0, false, sweep, true, false);

                            var (arcStart2, arcEnd2, sweep2) = CommonHelper.GetArcBy3Point(current, current2, TargetPoint.OutterCenter, ArcRadius);
                            ctx.LineTo(arcStart2, true, false);
                            ctx.ArcTo(arcEnd2, new Size(ArcRadius, ArcRadius), 0, false, sweep2, true, false);

                            var (arcStart_end, arcEnd_end, sweep_end) = CommonHelper.GetArcBy3Point(current2, TargetPoint.OutterCenter, TargetPoint.Center, ArcRadius);
                            ctx.LineTo(arcStart_end, true, false);
                            ctx.ArcTo(arcEnd_end, new Size(ArcRadius, ArcRadius), 0, false, sweep_end, true, false);
                        }
                    }
                    else
                    {
                        if (dir * sourceDir > 0)
                        {
                            if (dirSwap * targetDir > 0)
                            {
                                Point current = SourcePoint.OutterCenter + targetDir * lenSwap;

                                var (arcStart_start, arcEnd_start, sweep_start) = CommonHelper.GetArcBy3Point(SourcePoint.Center, SourcePoint.OutterCenter, current, ArcRadius);
                                ctx.LineTo(arcStart_start, true, false);
                                ctx.ArcTo(arcEnd_start, new Size(ArcRadius, ArcRadius), 0, false, sweep_start, true, false);

                                var (arcStart, arcEnd, sweep) = CommonHelper.GetArcBy3Point(SourcePoint.OutterCenter, current, TargetPoint.OutterCenter, ArcRadius);
                                ctx.LineTo(arcStart, true, false);
                                ctx.ArcTo(arcEnd, new Size(ArcRadius, ArcRadius), 0, false, sweep, true, false);

                                var (arcStart_end, arcEnd_end, sweep_end) = CommonHelper.GetArcBy3Point(current, TargetPoint.OutterCenter, TargetPoint.Center, ArcRadius);
                                ctx.LineTo(arcStart_end, true, false);
                                ctx.ArcTo(arcEnd_end, new Size(ArcRadius, ArcRadius), 0, false, sweep_end, true, false);
                            }
                            else
                            {
                                Point current = SourcePoint.OutterCenter + sourceDir * len;

                                var (arcStart_start, arcEnd_start, sweep_start) = CommonHelper.GetArcBy3Point(SourcePoint.Center, SourcePoint.OutterCenter, current, ArcRadius);
                                ctx.LineTo(arcStart_start, true, false);
                                ctx.ArcTo(arcEnd_start, new Size(ArcRadius, ArcRadius), 0, false, sweep_start, true, false);

                                var (arcStart, arcEnd, sweep) = CommonHelper.GetArcBy3Point(SourcePoint.OutterCenter, current, TargetPoint.OutterCenter, ArcRadius);
                                ctx.LineTo(arcStart, true, false);
                                ctx.ArcTo(arcEnd, new Size(ArcRadius, ArcRadius), 0, false, sweep, true, false);

                                var (arcStart_end, arcEnd_end, sweep_end) = CommonHelper.GetArcBy3Point(current, TargetPoint.OutterCenter, TargetPoint.Center, ArcRadius);
                                ctx.LineTo(arcStart_end, true, false);
                                ctx.ArcTo(arcEnd_end, new Size(ArcRadius, ArcRadius), 0, false, sweep_end, true, false);
                            }
                        }
                        else
                        {
                            Point current = SourcePoint.OutterCenter + rawDirSwap * lenSwap;

                            var (arcStart_start, arcEnd_start, sweep_start) = CommonHelper.GetArcBy3Point(SourcePoint.Center, SourcePoint.OutterCenter, current, ArcRadius);
                            ctx.LineTo(arcStart_start, true, false);
                            ctx.ArcTo(arcEnd_start, new Size(ArcRadius, ArcRadius), 0, false, sweep_start, true, false);

                            var (arcStart, arcEnd, sweep) = CommonHelper.GetArcBy3Point(SourcePoint.OutterCenter, current, TargetPoint.OutterCenter, ArcRadius);
                            ctx.LineTo(arcStart, true, false);
                            ctx.ArcTo(arcEnd, new Size(ArcRadius, ArcRadius), 0, false, sweep, true, false);

                            var (arcStart_end, arcEnd_end, sweep_end) = CommonHelper.GetArcBy3Point(current, TargetPoint.OutterCenter, TargetPoint.Center, ArcRadius);
                            ctx.LineTo(arcStart_end, true, false);
                            ctx.ArcTo(arcEnd_end, new Size(ArcRadius, ArcRadius), 0, false, sweep_end, true, false);
                        }
                    }

                    ctx.LineTo(TargetPoint.Center, true, false);
                }
            }

            if (LineType == RectLinkType.Bezier)
            {
                using (var ctx = geo.Open())
                {
                    var controlPoint = GetBezizerControlPoint(sourceDir, SourcePoint.Center, TargetPoint.Center);
                    var targtetPoint = GetBezizerControlPoint(targetDir, TargetPoint.Center, SourcePoint.Center);

                    ctx.BeginFigure(SourcePoint.Center, false, false);
                    ctx.BezierTo(controlPoint, targtetPoint, TargetPoint.Center, true, false);
                }
            }

            if (LineType == RectLinkType.Line)
            {
                using (var ctx = geo.Open())
                {
                    ctx.BeginFigure(SourcePoint.Center, false, false);
                    ctx.LineTo(TargetPoint.Center, true, false);
                }
            }
            geo.Freeze();
            base.Geometry = geo;
        }
        private Point GetBezizerControlPoint(Vector dir, Point source, Point target)
        {
            double dx = target.X - source.X;
            double dy = target.Y - source.Y;
            double dotProduct = dx * dir.X + dy * dir.Y;

            if (dotProduct > 0)
            {
                return new Point(
                    source.X + (dir.X != 0 ? dx / 2 : 0),
                    source.Y + (dir.Y != 0 ? dy / 2 : 0)
                );
            }

            double len = dir.X != 0 ? Math.Abs(dx) : Math.Abs(dy);
            if (len > BEZIER_REVER_LIMIT_LENGTH)
            {
                double offset = BEZIER_REVER_LIMIT_LENGTH + (len / BEZIER_REVER_LIMIT_LENGTH_FACTOR);
                return new Point(source.X + dir.X * offset, source.Y + dir.Y * offset);
            }
            return new Point(source.X + dir.X * len, source.Y + dir.Y * len);
        }
    }

    internal struct LinkOutterPoint
    {
        internal Point Center;
        internal Point OutterCenter;

        public LinkOutterPoint(Point center, Point outterCenter)
        {
            Center = center;
            OutterCenter = outterCenter;
        }
    }
}
