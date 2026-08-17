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
    /// 网格
    /// </summary>
    public class QuareGrid : GridBase
    {
        private Brush _gridColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DCDCDC"));
        /// <summary>
        /// 网格颜色
        /// </summary>
        [DefaultValue(typeof(Brush), "#DCDCDC")]
        public Brush GridColor
        {
            get => this._gridColor;
            set { this._gridColor = value; NotifyPropertyChanged(); }
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
            GridViewport = new System.Windows.Rect(0, 0, this.GridLength, this.GridLength);
            var half = this.GridSize / 2;
            this.Line1StartPoint = new Point(-half, 0);
            this.Line1EndPoint = new Point(this.GridLength + half, 0);
            this.Line2StartPoint = new Point(0, -half);
            this.Line2EndPoint = new Point(0, this.GridLength + half);
        }

        internal override DrawingBrush GenerateDrawingBrush()
        {
            var gridPen = new Pen();
            BindingOperations.SetBinding(gridPen, Pen.BrushProperty, CommonHelper.CreateBinding(nameof(GridColor), this));
            BindingOperations.SetBinding(gridPen, Pen.ThicknessProperty, CommonHelper.CreateBinding(nameof(GridSize), this));
            BindingOperations.SetBinding(gridPen, Pen.DashStyleProperty, CommonHelper.CreateBinding(nameof(DashStyle), this));

            LineGeometry line1 = new LineGeometry();
            BindingOperations.SetBinding(line1, LineGeometry.StartPointProperty, CommonHelper.CreateBinding(nameof(Line1StartPoint), this));
            BindingOperations.SetBinding(line1, LineGeometry.EndPointProperty, CommonHelper.CreateBinding(nameof(Line1EndPoint), this));

            LineGeometry line2 = new LineGeometry();
            BindingOperations.SetBinding(line2, LineGeometry.StartPointProperty, CommonHelper.CreateBinding(nameof(Line2StartPoint), this));
            BindingOperations.SetBinding(line2, LineGeometry.EndPointProperty, CommonHelper.CreateBinding(nameof(Line2EndPoint), this));

            var geoGroup = new GeometryGroup();
            geoGroup.Children.Add(line1);
            geoGroup.Children.Add(line2);

            var db = new DrawingBrush(new GeometryDrawing(null, gridPen, geoGroup));
            db.TileMode = TileMode.Tile;
            db.ViewportUnits = BrushMappingMode.Absolute;
            BindingOperations.SetBinding(db, DrawingBrush.ViewportProperty, CommonHelper.CreateBinding(nameof(GridViewport), this));

            return db;
        }

        private Point _line1StartPoint = new Point(-0.5, 0);
        public Point Line1StartPoint
        {
            get => this._line1StartPoint;
            private set { this._line1StartPoint = value; NotifyPropertyChanged(); }
        }

        private Point _line1EndPoint = new Point(DEFAULT_LENGTH_Ext, 0);
        public Point Line1EndPoint
        {
            get => this._line1EndPoint;
            private set { this._line1EndPoint = value; NotifyPropertyChanged(); }
        }

        private Point _line2StartPoint = new Point(0, -0.5);
        public Point Line2StartPoint
        {
            get => this._line2StartPoint;
            private set { this._line2StartPoint = value; NotifyPropertyChanged(); }
        }

        private Point _line2EndPoint = new Point(0, DEFAULT_LENGTH_Ext);
        public Point Line2EndPoint
        {
            get => this._line2EndPoint;
            private set { this._line2EndPoint = value; NotifyPropertyChanged(); }
        }

        private System.Windows.Rect _gridViewport = new System.Windows.Rect(0, 0, DEFAULT_LENGTH, DEFAULT_LENGTH);
        public System.Windows.Rect GridViewport
        {
            get => this._gridViewport;
            private set { this._gridViewport = value; NotifyPropertyChanged(); }
        }

        private DashStyle _dashStyle;
        public DashStyle DashStyle
        {
            get => this._dashStyle;
            set { this._dashStyle = value; NotifyPropertyChanged(); }
        }

        public QuareGrid()
        {
            GridColor.Freeze();
        }
    }
}
