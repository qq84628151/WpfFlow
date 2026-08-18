//===========================================================================//
//qq：1018720141     qq群：1064754010                                        //
//===========================================================================//
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
using WpfFlow.Enum;
using WpfFlow.Helper;
using WpfFlow.Other;

namespace WpfFlow
{
    /// <summary>
    /// 矩形节点
    /// </summary>
    public class RectShape : ShapeBase
    {
        private Brush _fill;
        /// <summary>
        /// 填充颜色
        /// </summary>
        public Brush Fill
        {
            get => this._fill;
            set { this._fill = value; ContentPanel.Background = _fill; NotifyPropertyChanged(); }
        }

        private CornerRadius _radius = new CornerRadius(5);
        /// <summary>
        /// 圆角半径
        /// </summary>
        [DefaultValue(typeof(CornerRadius), "5")]
        public CornerRadius Radius
        {
            get => this._radius;
            set { this._radius = value; NotifyPropertyChanged(); }
        }
        private Thickness _portThickness = new Thickness(20);
        /// <summary>
        /// 上下左右端口距离
        /// </summary>
        [DefaultValue(typeof(Thickness), "20")]
        public Thickness PortThickness
        {
            get => this._portThickness;
            set { this._portThickness = value; NotifyPropertyChanged(); }
        }
        private Size _size;
        /// <summary>
        /// 节点大小
        /// </summary>
        public new Size Size
        {
            get => this._size;
            set
            {
                if (_size == value) return;
                if (value.Width < FlowChart.NODE_MAX_SIZE) value.Width = FlowChart.NODE_MAX_SIZE;
                if (value.Height < FlowChart.NODE_MAX_SIZE) value.Height = FlowChart.NODE_MAX_SIZE;
                this._size = value;
                UpdatePanelChildrenPositionSize();
                NotifyPropertyChanged();
            }
        }

        private Vector _position;
        /// <summary>
        /// 节点位置
        /// </summary>
        public new Vector Position
        {
            get => this._position;
            set
            {
                if (_position == value) return;
                this._position = value;
                UpdateMatri();
                NotifyPropertyChanged();
            }
        }

        private double _angle;
        /// <summary>
        /// 节点旋转角度(0~360)
        /// </summary>
        public new double Angle
        {
            get => this._angle;
            set { this._angle = value; UpdateMatri(); NotifyPropertyChanged(); }
        }

        private PanelType _leftPortPanel;
        /// <summary>
        /// 左边端口容器
        /// </summary>
        public PanelType LeftPortPanel
        {
            get => this._leftPortPanel;
            set { this._leftPortPanel = value; CreatePortLayout(true); NotifyPropertyChanged(); }
        }

        private PanelType _topPortPanel;
        /// <summary>
        /// 顶部端口容器
        /// </summary>
        public PanelType TopPortPanel
        {
            get => this._topPortPanel;
            set { this._topPortPanel = value; CreatePortLayout(true); NotifyPropertyChanged(); }
        }

        private PanelType _rightPortPanel;
        /// <summary>
        /// 右边端口容器
        /// </summary>
        public PanelType RightPortPanel
        {
            get => this._rightPortPanel;
            set { this._rightPortPanel = value; CreatePortLayout(true); NotifyPropertyChanged(); }
        }

        private PanelType _bottomPortPanel;
        /// <summary>
        /// 底部端口容器
        /// </summary>
        public PanelType BottomPortPanel
        {
            get => this._bottomPortPanel;
            set { this._bottomPortPanel = value; CreatePortLayout(true); NotifyPropertyChanged(); }
        }

        /// <summary>
        /// 左边端口数据源
        /// </summary>
        public RectShapeCollection<Port> LeftPort { get; }
        /// <summary>
        /// 顶部端口数据源
        /// </summary>
        public RectShapeCollection<Port> TopPort { get; }
        /// <summary>
        /// 右边端口数据源
        /// </summary>
        public RectShapeCollection<Port> RightPort { get; }
        /// <summary>
        /// 底部端口数据源
        /// </summary>
        public RectShapeCollection<Port> BottomPort { get; }
        internal RectShapeCollection<Port>[] Ports { get; } = new RectShapeCollection<Port>[4];
        internal Border ContentPanel { get; set; } = new Border();

        private Panel left_grid = new UniformGrid();
        private Panel top_grid = new UniformGrid();
        private Panel right_grid = new UniformGrid();
        private Panel bottom_grid = new UniformGrid();

        public RectShape()
        {
            ContentPanel.Tag = this;
            Ports[0] = LeftPort = new RectShapeCollection<Port>(this, PortDirection.Left);
            Ports[1] = TopPort = new RectShapeCollection<Port>(this, PortDirection.Top);
            Ports[2] = RightPort = new RectShapeCollection<Port>(this, PortDirection.Right);
            Ports[3] = BottomPort = new RectShapeCollection<Port>(this, PortDirection.Bottom);

            BindingOperations.SetBinding(ContentPanel, Border.BackgroundProperty, CommonHelper.CreateBinding(nameof(Fill), this));
            BindingOperations.SetBinding(ContentPanel, Border.CornerRadiusProperty, CommonHelper.CreateBinding(nameof(Radius), this));
            BindingOperations.SetBinding(ContentPanel, Border.BorderBrushProperty, CommonHelper.CreateBinding(nameof(Stroke), this));
            BindingOperations.SetBinding(ContentPanel, Border.BorderThicknessProperty, CommonHelper.CreateBinding(nameof(StrokeThickness), this));

            var content = new ContentControl();
            content.HorizontalAlignment = HorizontalAlignment.Center;
            content.VerticalAlignment = VerticalAlignment.Center;
            content.SetBinding(ContentControl.ContentProperty, CommonHelper.CreateBinding(nameof(Content), this));
            ContentPanel.Child = content;
            ContentPanel.RenderTransform = new TranslateTransform();

            CreatePortLayout();
        }
        internal void CreatePortLayout(bool flag = false)
        {
            Panel.Children.Clear();
            Panel.Children.Add(ContentPanel);

            UpdatePortPanel(ref left_grid, LeftPortPanel);
            UpdatePortPanel(ref top_grid, TopPortPanel);
            UpdatePortPanel(ref right_grid, RightPortPanel);
            UpdatePortPanel(ref bottom_grid, BottomPortPanel);

            Panel.Children.Add(left_grid);
            Panel.Children.Add(top_grid);
            Panel.Children.Add(right_grid);
            Panel.Children.Add(bottom_grid);

            left_grid.RenderTransform = new TranslateTransform();
            top_grid.RenderTransform = new TranslateTransform();
            right_grid.RenderTransform = new TranslateTransform();
            bottom_grid.RenderTransform = new TranslateTransform();

            if (flag) Panel.InvalidateMeasure();

            UpdatePanelChildrenPositionSize();
        }

        private void UpdatePortPanel(ref Panel panel, PanelType _type)
        {
            List<Port> tmpList = null;
            if (panel != null && panel.Children.Count > 0)
            {
                tmpList = new List<Port>(panel.Children.Count);
                foreach (Port port in panel.Children)
                {
                    tmpList.Add(port);
                }
                panel.Children.Clear();
            }

            switch (_type)
            {
                case PanelType.UniformGrid:
                    panel = new UniformGrid();
                    if (panel == left_grid || panel == right_grid) (panel as UniformGrid).Columns = 1;
                    if (panel == top_grid || panel == bottom_grid) (panel as UniformGrid).Rows = 1;
                    break;
                case PanelType.StackPanel:
                    panel = new StackPanel();
                    if (panel == left_grid || panel == right_grid) (panel as StackPanel).Orientation = Orientation.Vertical;
                    if (panel == top_grid || panel == bottom_grid) (panel as StackPanel).Orientation = Orientation.Horizontal;
                    break;
            }

            if (tmpList != null && tmpList.Count > 0)
            {
                foreach (Port port in tmpList)
                {
                    panel.Children.Add(port);
                }
            }
        }

        internal void UpdatePanelChildrenPositionSize()
        {
            if (Panel != null)
            {
                ContentPanel.Width = Size.Width;
                ContentPanel.Height = Size.Height;

                var x = PortThickness.Left;
                var y = PortThickness.Top;
                var right = PortThickness.Right;
                var bottom = PortThickness.Bottom;

                Panel.Width = Size.Width + x + right;
                Panel.Height = Size.Height + y + bottom;

                left_grid.Width = x;
                left_grid.Height = Size.Height;

                top_grid.Width = Size.Width;
                top_grid.Height = y;

                right_grid.Width = right;
                right_grid.Height = Size.Height;

                bottom_grid.Width = Size.Width;
                bottom_grid.Height = bottom;

                (ContentPanel.RenderTransform as TranslateTransform).X = x;
                (ContentPanel.RenderTransform as TranslateTransform).Y = y;

                (left_grid.RenderTransform as TranslateTransform).Y = y;

                (top_grid.RenderTransform as TranslateTransform).X = x;

                (right_grid.RenderTransform as TranslateTransform).X = Size.Width + x;
                (right_grid.RenderTransform as TranslateTransform).Y = y;

                (bottom_grid.RenderTransform as TranslateTransform).X = x;
                (bottom_grid.RenderTransform as TranslateTransform).Y = Size.Height + y;
            }
        }

        private void UpdateMatri()
        {
            var centerX = PortThickness.Left + Size.Width / 2;
            var centerY = PortThickness.Top + Size.Height / 2;
            var matrix = Matrix.Identity;
            matrix.RotateAt(Angle, centerX, centerY);
            matrix.Translate(Position.X - PortThickness.Left, Position.Y - PortThickness.Top);
            MatrixTransform.Matrix = matrix;
        }
    }
}
