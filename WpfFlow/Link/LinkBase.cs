//===========================================================================//
//qq：1018720141     qq群：1064754010                                        //
//===========================================================================//
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Effects;
using WpfFlow.Helper;
using WpfFlow.Interface;
using WpfFlow.Link;
using WpfFlow.Other;

namespace WpfFlow
{
    public abstract class LinkBase : INotifyPropertyChanged, IShape
    {
        #region 线1
        private Brush _stroke = Brushes.Black;
        /// <summary>
        /// 线颜色
        /// </summary>
        [DefaultValue(typeof(Brush), "Black")]
        public Brush Stroke
        {
            get => this._stroke;
            set { this._stroke = value; CreateStrokePen(); NotifyPropertyChanged(); }
        }

        private double _strokeThickness = 1;
        /// <summary>
        /// 线粗细
        /// </summary>
        [DefaultValue(1.0)]
        public double StrokeThickness
        {
            get => this._strokeThickness;
            set { this._strokeThickness = value; CreateStrokePen(); NotifyPropertyChanged(); }
        }

        private DashStyle _strokeDashStyle;
        /// <summary>
        /// 线间隙样式
        /// </summary>
        public DashStyle StrokeDashStyle
        {
            get => this._strokeDashStyle;
            set { this._strokeDashStyle = value; CreateStrokePen(); NotifyPropertyChanged(); }
        }
        #endregion
        #region 线2
        private Brush _stroke2;
        /// <summary>
        /// 第二层线颜色
        /// </summary>
        public Brush Stroke2
        {
            get => this._stroke2;
            set { this._stroke2 = value; CreateStrokePen2(); NotifyPropertyChanged(); }
        }

        private double _strokeThickness2 = 1;
        /// <summary>
        /// 第二层线粗细
        /// </summary>
        [DefaultValue(1.0)]
        public double StrokeThickness2
        {
            get => this._strokeThickness2;
            set { this._strokeThickness2 = value; CreateStrokePen2(); NotifyPropertyChanged(); }
        }

        private DashStyle _strokeDashStyle2;
        /// <summary>
        /// 第二层线间隙样式
        /// </summary>
        public DashStyle StrokeDashStyle2
        {
            get => this._strokeDashStyle2;
            set { this._strokeDashStyle2 = value; CreateStrokePen2(); NotifyPropertyChanged(); }
        }
        #endregion
        #region 线3
        private Brush _stroke3;
        /// <summary>
        /// 第三层线颜色
        /// </summary>
        public Brush Stroke3
        {
            get => this._stroke3;
            set { this._stroke3 = value; CreateStrokePen3(); NotifyPropertyChanged(); }
        }

        private double _strokeThickness3 = 1;
        /// <summary>
        /// 第三层线粗细
        /// </summary>
        [DefaultValue(1.0)]
        public double StrokeThickness3
        {
            get => this._strokeThickness3;
            set { this._strokeThickness3 = value; CreateStrokePen3(); NotifyPropertyChanged(); }
        }

        private DashStyle _strokeDashStyle3;
        /// <summary>
        /// 第三层线间隙样式
        /// </summary>
        public DashStyle StrokeDashStyle3
        {
            get => this._strokeDashStyle3;
            set { this._strokeDashStyle3 = value; CreateStrokePen3(); NotifyPropertyChanged(); }
        }
        #endregion
        private string _sourcre;
        /// <summary>
        /// 源节点的NodeId
        /// </summary>
        public string Source
        {
            get => this._sourcre;
            set { this._sourcre = value; NotifyPropertyChanged(); }
        }
        private string _target;
        /// <summary>
        /// 目标节点的NodeId
        /// </summary>
        public string Target
        {
            get => this._target;
            set { this._target = value; NotifyPropertyChanged(); }
        }

        private Effect _effect;
        /// <summary>
        /// 线特效
        /// </summary>
        public Effect Effect
        {
            get => this._effect;
            set { this._effect = value; NotifyPropertyChanged(); }
        }
        /// <summary>
        /// 线标签的数据源
        /// </summary>
        public LinkCollection<LinkLabel> Labels { get; }

        internal ShapeBase SourceRect { get; set; }
        internal ShapeBase TargetRect { get; set; }
        internal Pen StrokePen { get; set; }
        internal Pen StrokePen2 { get; set; }
        internal Pen StrokePen3 { get; set; }
        internal Geometry Geometry { get; set; }
        internal DrawingVisual Visual { get; set; } = new DrawingVisual();
        internal LineVisualHost VisualHost { get; set; }

        internal abstract void Render();

        private void CreateStrokePen()
        {
            StrokePen = new Pen(Stroke, StrokeThickness);
            StrokePen.DashStyle = StrokeDashStyle;
            if (Stroke.IsFrozen && (StrokeDashStyle == null || StrokeDashStyle.IsFrozen))
            {
                StrokePen.Freeze();
            }
        }
        private void CreateStrokePen2()
        {
            if (Stroke2 != null)
            {
                StrokePen2 = new Pen(Stroke2, StrokeThickness2);
                StrokePen2.DashStyle = StrokeDashStyle2;
                if (Stroke2.IsFrozen && (StrokeDashStyle2 == null || StrokeDashStyle2.IsFrozen))
                {
                    StrokePen2.Freeze();
                }
            }
            else
            {
                StrokePen2 = null;
            }
        }
        private void CreateStrokePen3()
        {
            if (Stroke3 != null)
            {
                StrokePen3 = new Pen(Stroke3, StrokeThickness3);
                StrokePen3.DashStyle = StrokeDashStyle3;
                if (Stroke3.IsFrozen && (StrokeDashStyle3 == null || StrokeDashStyle3.IsFrozen))
                {
                    StrokePen3.Freeze();
                }
            }
            else
            {
                StrokePen3 = null;
            }
        }

        public LinkBase()
        {
            VisualHost = new LineVisualHost(this);
            Labels = new LinkCollection<LinkLabel>(this);
            CreateStrokePen();
            CreateStrokePen2();
            CreateStrokePen3();

            BindingOperations.SetBinding(VisualHost, FrameworkElement.EffectProperty, CommonHelper.CreateBinding(nameof(Effect), this));
        }
        internal void UpdateLabelsPosition()
        {
            if (Geometry == null)
            {
                foreach (var label in Labels)
                {
                    if (label.ContentCtrl.Visibility != Visibility.Collapsed)
                    {
                        label.ContentCtrl.Visibility = Visibility.Collapsed;
                    }
                }
                return;
            }

            foreach (var label in Labels)
            {
                if (label.ContentCtrl.Visibility != Visibility.Visible)
                {
                    label.ContentCtrl.Visibility = Visibility.Visible;
                }

                PathGeometry.CreateFromGeometry(Geometry).GetPointAtFractionLength(label.Percentage, out var point, out var tangent);

                Matrix matrix = Matrix.Identity;
                var centerX = label.ContentCtrl.ActualWidth * label.Origin.X;
                var centerY = label.ContentCtrl.ActualHeight * label.Origin.Y;
                if (label.FollowAngle)
                {
                    matrix.RotateAt(Math.Atan2(tangent.Y, tangent.X) * (180 / Math.PI), label.ContentCtrl.ActualWidth / 2, label.ContentCtrl.ActualHeight / 2);
                }
                matrix.Translate(point.X - centerX, point.Y - centerY);

                (label.ContentCtrl.RenderTransform as MatrixTransform).Matrix = matrix;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
