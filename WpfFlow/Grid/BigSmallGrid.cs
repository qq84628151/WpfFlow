//===========================================================================//
//qq：1018720141     qq群：1064754010                                        //
//===========================================================================//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using WpfFlow.Helper;

namespace WpfFlow
{
    /// <summary>
    /// 大小网格
    /// </summary>
    public class BigSmallGrid : GridBase
    {
        private Brush _gridInColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f0f0f0"));
        /// <summary>
        /// 小网格颜色
        /// </summary>
        [DefaultValue(typeof(Brush), "#EEEEEE")]
        public Brush GridInColor
        {
            get => this._gridInColor;
            set { this._gridInColor = value; NotifyPropertyChanged(); }
        }

        private Brush _gridOutColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#CCCCCC"));
        /// <summary>
        /// 大网格颜色
        /// </summary>
        [DefaultValue(typeof(Brush), "#CCCCCC")]
        public Brush GridOutColor
        {
            get => this._gridOutColor;
            set { this._gridOutColor = value; NotifyPropertyChanged(); }
        }

        private double _gridSize = 1.0;
        /// <summary>
        /// 网格线粗细
        /// </summary>
        [DefaultValue(1.0)]
        public double GridSize
        {
            get => this._gridSize;
            set { this._gridSize = value; UpdateLine(); NotifyPropertyChanged(); }
        }

        internal override void UpdateLine()
        {
            double size = this.GridLength * 4;
            var half = this.GridSize / 2;
            var gdInside = new GeometryGroup();
            var gdOutside = new GeometryGroup();
            var penInside = new Pen();
            var penOutside = new Pen();

            GridViewport = new System.Windows.Rect(0, 0, size, size);
            this.Geometry = new DrawingGroup();

            BindingOperations.SetBinding(penInside, Pen.BrushProperty, CommonHelper.CreateBinding(nameof(GridInColor), this));
            BindingOperations.SetBinding(penInside, Pen.ThicknessProperty, CommonHelper.CreateBinding(nameof(GridSize), this));
            BindingOperations.SetBinding(penInside, Pen.DashStyleProperty, CommonHelper.CreateBinding(nameof(DashStyle), this));

            BindingOperations.SetBinding(penOutside, Pen.BrushProperty, CommonHelper.CreateBinding(nameof(GridOutColor), this));
            BindingOperations.SetBinding(penOutside, Pen.ThicknessProperty, CommonHelper.CreateBinding(nameof(GridSize), this));
            BindingOperations.SetBinding(penOutside, Pen.DashStyleProperty, CommonHelper.CreateBinding(nameof(DashStyle), this));

            for (int i = 3; i >= 0; --i)
            {
                var line1 = new LineGeometry(new Point(-half, i * this.GridLength), new Point(size + half, i * this.GridLength));
                var line2 = new LineGeometry(new Point(i * this.GridLength, -half), new Point(i * this.GridLength, size + half));
                if (i == 0)
                {
                    gdOutside.Children.Add(line1);
                    gdOutside.Children.Add(line2);
                }
                else
                {
                    gdInside.Children.Add(line1);
                    gdInside.Children.Add(line2);
                }
            }
            this.Geometry.Children.Add(new GeometryDrawing(null, penInside, gdInside));
            this.Geometry.Children.Add(new GeometryDrawing(null, penOutside, gdOutside));
        }

        internal override DrawingBrush GenerateDrawingBrush()
        {
            UpdateLine();

            var db = new DrawingBrush();
            db.TileMode = TileMode.Tile;
            db.ViewportUnits = BrushMappingMode.Absolute;
            BindingOperations.SetBinding(db, DrawingBrush.DrawingProperty, CommonHelper.CreateBinding(nameof(Geometry), this));
            BindingOperations.SetBinding(db, DrawingBrush.ViewportProperty, CommonHelper.CreateBinding(nameof(GridViewport), this));

            return db;
        }

        private DrawingGroup _geometry;
        public DrawingGroup Geometry
        {
            get => this._geometry;
            private set { this._geometry = value; NotifyPropertyChanged(); }
        }

        private System.Windows.Rect _gridViewport = new System.Windows.Rect(0, 0, DEFAULT_LENGTH_FOUR, DEFAULT_LENGTH_FOUR);
        public System.Windows.Rect GridViewport
        {
            get => this._gridViewport;
            private set { this._gridViewport = value; NotifyPropertyChanged(); }
        }

        private DashStyle _dashStyle;
        /// <summary>
        /// 网格线条间隙样式
        /// </summary>
        public DashStyle DashStyle
        {
            get => this._dashStyle;
            set { this._dashStyle = value; NotifyPropertyChanged(); }
        }

        public BigSmallGrid()
        {
            GridInColor.Freeze();
            GridOutColor.Freeze();
        }
    }
}
