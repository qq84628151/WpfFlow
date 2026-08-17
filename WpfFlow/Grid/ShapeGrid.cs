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
    /// 几何形状
    /// </summary>
    public class ShapeGrid : GridBase
    {
        private double _gridSize = 1.0;
        /// <summary>
        /// 几何线粗细
        /// </summary>
        [DefaultValue(1.0)]
        public double GridSize
        {
            get => this._gridSize;
            set { this._gridSize = value; NotifyPropertyChanged(); }
        }
        /// <summary>
        /// 几何形状
        /// </summary>
        private ShapeGridType _shapeType;
        public ShapeGridType ShapeType
        {
            get => this._shapeType;
            set { this._shapeType = value; UpdateLine(); NotifyPropertyChanged(); }
        }

        internal override void UpdateLine()
        {
            GridViewport = new System.Windows.Rect(0, 0, this.GridLength * 2, this.GridLength * 2);
            this.Geometry = new DrawingGroup();
            this.Geometry.Children.Add(new GeometryDrawing(Brushes.Transparent, null, new RectangleGeometry(GridViewport)));
            switch (ShapeType)
            {
                case ShapeGridType.Circle:
                    var eg = new EllipseGeometry(new Point(GridLength, GridLength), GridSize, GridSize);
                    BindingOperations.SetBinding(eg, EllipseGeometry.RadiusXProperty, CommonHelper.CreateBinding(nameof(GridSize), this));
                    BindingOperations.SetBinding(eg, EllipseGeometry.RadiusYProperty, CommonHelper.CreateBinding(nameof(GridSize), this));
                    var gd = new GeometryDrawing(GridColor, null, eg);
                    BindingOperations.SetBinding(gd, GeometryDrawing.BrushProperty, CommonHelper.CreateBinding(nameof(GridColor), this));
                    this.Geometry.Children.Add(gd);
                    break;
                case ShapeGridType.Cross:
                    var gg = new GeometryGroup();
                    double rad = GridLength * GridSize / 10;
                    gg.Children.Add(new LineGeometry(new Point(GridLength, GridLength - rad), new Point(GridLength, GridLength + rad)));
                    gg.Children.Add(new LineGeometry(new Point(GridLength - rad, GridLength), new Point(GridLength + rad, GridLength)));
                    var gd2 = new GeometryDrawing(GridColor, null, gg);
                    gd2.Pen = new Pen();
                    BindingOperations.SetBinding(gd2.Pen, Pen.BrushProperty, CommonHelper.CreateBinding(nameof(GridColor), this));
                    BindingOperations.SetBinding(gd2.Pen, Pen.ThicknessProperty, CommonHelper.CreateBinding(nameof(GridSize), this));
                    this.Geometry.Children.Add(gd2);
                    break;
            }
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

        private System.Windows.Rect _gridViewport = new System.Windows.Rect(0, 0, DEFAULT_LENGTH_DOUBLE, DEFAULT_LENGTH_DOUBLE);
        public System.Windows.Rect GridViewport
        {
            get => this._gridViewport;
            private set { this._gridViewport = value; NotifyPropertyChanged(); }
        }

        private Brush _gridColor = Brushes.Gray;
        /// <summary>
        /// 几何线颜色
        /// </summary>
        [DefaultValue(typeof(Brush), "Gray")]
        public Brush GridColor
        {
            get => this._gridColor;
            set { this._gridColor = value; NotifyPropertyChanged(); }
        }
    }

    public enum ShapeGridType
    {
        Circle,
        Cross,
    }
}
