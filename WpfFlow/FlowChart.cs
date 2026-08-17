//===========================================================================//
//qq：1018720141     qq群：1064754010                                        //
//===========================================================================//
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.Remoting.Lifetime;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using WpfFlow.Enum;
using WpfFlow.FlowEventArgs;
using WpfFlow.Helper;
using WpfFlow.Interface;
using WpfFlow.Other;
using WpfFlow.Shape;

namespace WpfFlow
{
    /// <summary>
    /// 流程节点控件
    /// </summary>
    [DefaultProperty("ItemsSource")]
    [ContentProperty("ItemsSource")]
    public class FlowChart : FrameworkElement
    {
        /// <summary>
        /// 鼠标进入节点事件
        /// </summary>
        public event EventHandler<ShapeMouseEventArgs> ShapeMouseEnter;
        /// <summary>
        /// 鼠标离开节点事件
        /// </summary>
        public event EventHandler<ShapeMouseEventArgs> ShapeMouseLeave;
        /// <summary>
        /// 鼠标开始拖拽节点事件
        /// </summary>
        public event EventHandler<ShapeMouseButtonEventArgs> ShapeMouseDragStart;
        /// <summary>
        /// 鼠标结束拖拽节点事件
        /// </summary>
        public event EventHandler<ShapeMouseButtonEventArgs> ShapeMouseDragEnd;
        /// <summary>
        /// 鼠标拖拽节点移动事件
        /// </summary>
        public event EventHandler<ShapeMouseEventArgs> ShapeMouseDragMove;
        /// <summary>
        /// 拖拽端口添加新的线事件
        /// </summary>
        public event EventHandler<NewLineEventArgs> DragAddNewLink;

        public static readonly DependencyProperty BackgroundProperty = DependencyProperty.Register("Background", typeof(Brush), typeof(FlowChart), new PropertyMetadata(Brushes.White, OnBackgroundChanged));
        /// <summary>
        /// 背景颜色
        /// </summary>
        public Brush Background
        {
            get => (Brush)GetValue(BackgroundProperty);
            set => SetValue(BackgroundProperty, value);
        }
        private static void OnBackgroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((FlowChart)d).RenderBackground();
        }

        public static readonly DependencyPropertyKey ItemsSourcePropertyKey = DependencyProperty.RegisterReadOnly(nameof(ItemsSource), typeof(ObservableCollectionExt<IShape>), typeof(FlowChart), new FrameworkPropertyMetadata());
        public static readonly DependencyProperty ItemsSourceProperty = ItemsSourcePropertyKey.DependencyProperty;
        /// <summary>
        /// 节点数据源
        /// </summary>
        public ObservableCollectionExt<IShape> ItemsSource
        {
            get => (ObservableCollectionExt<IShape>)GetValue(ItemsSourceProperty);
        }

        public static readonly DependencyProperty GridStyleProperty = DependencyProperty.Register(nameof(GridStyle), typeof(IGrid), typeof(FlowChart), new PropertyMetadata(new BigSmallGrid(), OnGridStyleChanged));
        /// <summary>
        /// 网格样色
        /// </summary>
        public IGrid GridStyle
        {
            get => (IGrid)GetValue(GridStyleProperty);
            set => SetValue(GridStyleProperty, value);
        }
        private static void OnGridStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((FlowChart)d).RenderBackground();
        }

        public static readonly DependencyProperty AdsorptionRadiusProperty = DependencyProperty.Register(nameof(AdsorptionRadius), typeof(double), typeof(FlowChart), new PropertyMetadata(10.0));
        /// <summary>
        /// 端口吸附半径
        /// </summary>
        public double AdsorptionRadius
        {
            get => (double)GetValue(AdsorptionRadiusProperty);
            set => SetValue(AdsorptionRadiusProperty, value);
        }

        public static readonly DependencyProperty ScaleProperty = DependencyProperty.Register(nameof(Scale), typeof(double), typeof(FlowChart), new PropertyMetadata(1.0, OnScaleChanged));
        /// <summary>
        /// 节点缩放值,最小0.5，最大2.0
        /// </summary>
        public double Scale
        {
            get => (double)GetValue(ScaleProperty);
            set
            {
                if (value < SCALE_MIN) value = SCALE_MIN;
                if (value > SCALE_MAX) value = SCALE_MAX;
                SetValue(ScaleProperty, value);
            }
        }
        private static void OnScaleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((FlowChart)d).ScaleChange();
        }

        public static readonly DependencyProperty OutterProperty = DependencyProperty.Register(nameof(Outter), typeof(double), typeof(FlowChart), new PropertyMetadata(20.0, OnOutterChanged));
        /// <summary>
        /// 端口外边距，防止曼哈顿连线过于接近端口导致不美观
        /// </summary>
        public double Outter
        {
            get => (double)GetValue(OutterProperty);
            set => SetValue(OutterProperty, value);
        }
        private static void OnOutterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((FlowChart)d).RefreshAllRectLink();
        }

        public static readonly DependencyProperty PreviewSizeProperty = DependencyProperty.Register(nameof(PreviewSize), typeof(Size), typeof(FlowChart), new PropertyMetadata(new Size(210, 160), OnPreviewSizeChanged));
        /// <summary>
        /// 右下角的预览视图大小
        /// </summary>
        public Size PreviewSize
        {
            get => (Size)GetValue(PreviewSizeProperty);
            set => SetValue(PreviewSizeProperty, value);
        }
        private static void OnPreviewSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var fc = ((FlowChart)d);
            if (fc.DisablePreview)
            {
                fc.ClearAndHidePreview();
                return;
            }
            fc.ShowPreview();
            fc.UpdatePreviewOffset();
            fc.UpdatePreviewRect();
        }

        public static readonly DependencyProperty DisablePreviewProperty = DependencyProperty.Register(nameof(DisablePreview), typeof(bool), typeof(FlowChart), new PropertyMetadata(OnPreviewSizeChanged));
        /// <summary>
        /// 禁用预览视图
        /// </summary>
        public bool DisablePreview
        {
            get => (bool)GetValue(DisablePreviewProperty);
            set => SetValue(DisablePreviewProperty, value);
        }

        public static readonly DependencyProperty DisableDragBackProperty = DependencyProperty.Register(nameof(DisableDragBack), typeof(bool), typeof(FlowChart), new PropertyMetadata(OnDisableMouseDragChanged));
        /// <summary>
        /// 禁用拖拽背景整体平移
        /// </summary>
        public bool DisableDragBack
        {
            get => (bool)GetValue(DisableDragBackProperty);
            set => SetValue(DisableDragBackProperty, value);
        }
        private static void OnDisableMouseDragChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var fc = ((FlowChart)d);
            fc.Cursor = fc.DisableDragBack ? Cursors.Arrow : pointerGrabCursor;
        }

        public static readonly DependencyProperty DisableScaleProperty = DependencyProperty.Register(nameof(DisableScale), typeof(bool), typeof(FlowChart), new PropertyMetadata());
        /// <summary>
        /// 禁用缩放
        /// </summary>
        public bool DisableScale
        {
            get => (bool)GetValue(DisableScaleProperty);
            set => SetValue(DisableScaleProperty, value);
        }

        public static readonly DependencyProperty DisableResizeProperty = DependencyProperty.Register(nameof(DisableResize), typeof(bool), typeof(FlowChart), new PropertyMetadata(OnDisableResizeChanged));
        /// <summary>
        /// 禁用拖拽设置节点大小
        /// </summary>
        public bool DisableResize
        {
            get => (bool)GetValue(DisableResizeProperty);
            set => SetValue(DisableResizeProperty, value);
        }
        private static void OnDisableResizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var fc = ((FlowChart)d);
            fc._resizeCanvas.Visibility = Visibility.Collapsed;
            fc._nodeResizeShape = null;
        }

        public static readonly DependencyProperty DisableDragMoveProperty = DependencyProperty.Register(nameof(DisableDragMove), typeof(bool), typeof(FlowChart), new PropertyMetadata(OnDisableDragMoveChanged));
        /// <summary>
        /// 禁用拖拽移动节点
        /// </summary>
        public bool DisableDragMove
        {
            get => (bool)GetValue(DisableDragMoveProperty);
            set => SetValue(DisableDragMoveProperty, value);
        }
        private static void OnDisableDragMoveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((FlowChart)d).UpdateShapeCursos();
        }

        public static readonly DependencyProperty DisableDragAddLineProperty = DependencyProperty.Register(nameof(DisableDragAddLine), typeof(bool), typeof(FlowChart), new PropertyMetadata(OnDisableDragAddLineChanged));
        /// <summary>
        /// 禁用拖拽端口添加新的线
        /// </summary>
        public bool DisableDragAddLine
        {
            get => (bool)GetValue(DisableDragAddLineProperty);
            set => SetValue(DisableDragAddLineProperty, value);
        }
        private static void OnDisableDragAddLineChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((FlowChart)d).UpdatePortCursos();
        }

        public static readonly DependencyProperty DefaultNewLineProperty = DependencyProperty.Register(nameof(DefaultNewLine), typeof(RectLinkShape), typeof(FlowChart), new PropertyMetadata());
        /// <summary>
        /// 添加新的线默认样式
        /// </summary>
        public RectLinkShape DefaultNewLine
        {
            get => (RectLinkShape)GetValue(DefaultNewLineProperty);
            set => SetValue(DefaultNewLineProperty, value);
        }
        public static readonly DependencyProperty ResizeHandleColorProperty = DependencyProperty.Register(nameof(ResizeHandleColor), typeof(Brush), typeof(FlowChart), new PropertyMetadata(OnResizeHandleColorChanged));
        /// <summary>
        /// 修改节点大小手柄颜色
        /// </summary>
        public Brush ResizeHandleColor
        {
            get => (Brush)GetValue(ResizeHandleColorProperty);
            set => SetValue(ResizeHandleColorProperty, value);
        }
        private static void OnResizeHandleColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((FlowChart)d).UpdateResizeHandleColor((Brush)e.NewValue);
        }
        public static readonly DependencyProperty ResizeStrokeColorProperty = DependencyProperty.Register(nameof(ResizeStroke), typeof(Brush), typeof(FlowChart), new PropertyMetadata(OnResizeStrokeChanged));
        /// <summary>
        /// 修改节点大小线框颜色
        /// </summary>
        public Brush ResizeStroke
        {
            get => (Brush)GetValue(ResizeStrokeColorProperty);
            set => SetValue(ResizeStrokeColorProperty, value);
        }
        private static void OnResizeStrokeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((FlowChart)d).UpdateResizeStrokeColor((Brush)e.NewValue);
        }
        /// <summary>
        /// 节点与视图实际偏移
        /// </summary>
        public Point ViewRealOffset => new Point(_mouseDragTransform.X, _mouseDragTransform.Y);

        internal const double PREVIEW_MARGIN = 10;
        internal const double PREVIEW_MARGIN_DOUBLE = PREVIEW_MARGIN * 2;
        internal const double PREVIEW_RADIUS = 5;
        internal const double SCALE_MIN = 0.5;
        internal const double SCALE_MAX = 2.0;
        internal const double RESIZE_MARGIN = 15;
        internal const double RESIZE_MARGIN_DOUBLE = RESIZE_MARGIN * 2;
        internal const double RESIZE_DRAG_SIZE = 10;
        internal const double RESIZE_DRAG_SIZE_HALVE = RESIZE_DRAG_SIZE / 2;
        internal const double NODE_MAX_SIZE = 20;
        internal const double DRAG_ANGLE_HANDLE_LENGTH = 50;
        internal const double RESIZE_HANDLE_LINE_WIDTH = 2;

        private readonly VisualCollection _children;
        private readonly ContainerVisual _childrenRoot = new ContainerVisual();
        private readonly DrawingVisual _background = new DrawingVisual();
        private readonly DrawingVisual _renderBackground = new DrawingVisual();
        private readonly ContainerVisual _renderContents = new ContainerVisual();
        private readonly VisualBrush _preview;
        private readonly ContainerVisual _previewRoot = new ContainerVisual();
        private readonly DrawingVisual _previewMask = new DrawingVisual();
        private readonly DrawingVisual _previewVisual = new DrawingVisual();
        private readonly Brush _previewColor = new SolidColorBrush(Color.FromArgb(0xE5, 0xFF, 0xFF, 0xFF));
        private readonly Brush _previewMaskColor = new SolidColorBrush(Color.FromArgb(0x59, 0xD2, 0xD2, 0xD2));
        private RectangleGeometry _previewMaskRect;
        private readonly TransformGroup _mouseDragTran = new TransformGroup();
        private readonly TranslateTransform _mouseDragTransform = new TranslateTransform();
        private readonly ScaleTransform _mouseDragScale = new ScaleTransform();
        private readonly ContainerVisual __resizePanel1 = new ContainerVisual();
        private readonly ShapeHost _resizeCanvas = new ShapeHost();
        private readonly Rectangle _resizeContent = new Rectangle();
        private readonly Rectangle _resizeTopLeft = new Rectangle();
        private readonly Rectangle _resizeTopRight = new Rectangle();
        private readonly Rectangle _resizeBottomLeft = new Rectangle();
        private readonly Rectangle _resizeBottomRight = new Rectangle();
        private readonly Line _resizeAngleLine = new Line();
        private readonly Ellipse _resizeAngleHandle = new Ellipse();

        private readonly static HashSet<IShape> _shapeExistx = new HashSet<IShape>();
        private readonly static HashSet<Port> _portExistx = new HashSet<Port>();

        internal readonly static Cursor pointerGrabCursor;
        internal readonly static Cursor pointerGrabbingCursor;
        internal readonly static Cursor angleRotateCursor;
        static FlowChart()
        {
            using (var ms = new MemoryStream(Properties.Resources.pointer_grab_large))
            {
                pointerGrabCursor = new Cursor(ms, true);
            }
            using (var ms = new MemoryStream(Properties.Resources.pointer_grabbing_large))
            {
                pointerGrabbingCursor = new Cursor(ms, true);
            }
            using (var ms = new MemoryStream(Properties.Resources.rotate))
            {
                angleRotateCursor = new Cursor(ms, true);
            }
        }

        public FlowChart()
        {
            SetValue(ItemsSourcePropertyKey, new ObservableCollectionExt<IShape>());

            _previewColor.Freeze();
            _previewMaskColor.Freeze();
            _resizeCanvas.Visibility = Visibility.Collapsed;
            _resizeCanvas.Children.Add(_resizeContent);
            _resizeCanvas.Children.Add(_resizeTopLeft);
            _resizeCanvas.Children.Add(_resizeTopRight);
            _resizeCanvas.Children.Add(_resizeBottomLeft);
            _resizeCanvas.Children.Add(_resizeBottomRight);
            _resizeCanvas.Children.Add(_resizeAngleLine);
            _resizeCanvas.Children.Add(_resizeAngleHandle);
            __resizePanel1.Children.Add(_resizeCanvas);

            _children = new VisualCollection(this);
            _children.Add(_background);
            _children.Add(_childrenRoot);
            _children.Add(_previewRoot);

            MouseMove += FlowChart_MouseMove;
            MouseLeftButtonDown += FlowChart_MouseDown;
            MouseLeftButtonUp += FlowChart_MouseUp;
            MouseWheel += FlowChart_MouseWheel;

            _resizeContent.RenderTransform = new TranslateTransform();
            _resizeTopLeft.RenderTransform = new TranslateTransform();
            _resizeTopRight.RenderTransform = new TranslateTransform();
            _resizeBottomLeft.RenderTransform = new TranslateTransform();
            _resizeBottomRight.RenderTransform = new TranslateTransform();
            _resizeAngleLine.RenderTransform = new TranslateTransform();
            _resizeAngleHandle.RenderTransform = new TranslateTransform();

            _resizeTopLeft.MouseLeftButtonDown += _resizeDrag_MouseDown;
            _resizeTopRight.MouseLeftButtonDown += _resizeDrag_MouseDown;
            _resizeBottomLeft.MouseLeftButtonDown += _resizeDrag_MouseDown;
            _resizeBottomRight.MouseLeftButtonDown += _resizeDrag_MouseDown;

            _resizeTopLeft.MouseEnter += _resize_MouseEnter;
            _resizeTopRight.MouseEnter += _resize_MouseEnter;
            _resizeBottomLeft.MouseEnter += _resize_MouseEnter;
            _resizeBottomRight.MouseEnter += _resize_MouseEnter;

            _resizeTopLeft.MouseLeftButtonUp += _resizeDrag_MouseUp;
            _resizeTopRight.MouseLeftButtonUp += _resizeDrag_MouseUp;
            _resizeBottomLeft.MouseLeftButtonUp += _resizeDrag_MouseUp;
            _resizeBottomRight.MouseLeftButtonUp += _resizeDrag_MouseUp;

            _resizeAngleHandle.MouseLeftButtonDown += _resizeAngleHandle_MouseLeftButtonDown;
            _resizeAngleHandle.MouseLeftButtonUp += _resizeAngleHandle_MouseLeftButtonUp;

            UpdateResizeHandleColor(Brushes.LightSeaGreen);
            UpdateResizeStrokeColor(Brushes.LightBlue);
            _resizeAngleLine.Stroke = Brushes.Gray;
            _resizeAngleLine.IsHitTestVisible = false;

            _resizeAngleHandle.Cursor = angleRotateCursor;

            _preview = new VisualBrush(_childrenRoot) { Stretch = Stretch.Uniform };
            _childrenRoot.Children.Add(_renderBackground);
            _childrenRoot.Children.Add(_renderContents);
            _childrenRoot.Children.Add(__resizePanel1);
            _previewRoot.Children.Add(_previewVisual);
            _previewRoot.Children.Add(_previewMask);

            _mouseDragTran.Children.Add(_mouseDragScale);
            _mouseDragTran.Children.Add(_mouseDragTransform);
            _renderContents.Transform = __resizePanel1.Transform = _mouseDragTran;

            this.Cursor = pointerGrabCursor;

            ItemsSource.CollectionChanged += ItemsSource_CollectionChanged;
            Loaded += FlowChart_Loaded;
        }
        private void UpdateResizeHandleColor(Brush color)
        {
            _resizeTopLeft.Fill = color;
            _resizeTopRight.Fill = color;
            _resizeBottomLeft.Fill = color;
            _resizeBottomRight.Fill = color;
            _resizeAngleHandle.Fill = color;
        }
        private void UpdateResizeStrokeColor(Brush color)
        {
            _resizeContent.Stroke = color;
        }
        private void UpdateShapeCursos()
        {
            var cur = DisableDragMove ? Cursors.Arrow : Cursors.SizeAll;
            foreach (var item in ItemsSource)
            {
                if (item is ShapeBase shape)
                {
                    shape.Panel.Cursor = cur;
                }
            }
        }
        private void UpdatePortCursos()
        {
            var cur = DisableDragAddLine ? Cursors.Arrow : Cursors.Cross;
            foreach (var item in ItemsSource)
            {
                if (item is RectShape rect)
                {
                    foreach (var portList in rect.Ports)
                    {
                        foreach (var port in portList)
                        {
                            port.Cursor = cur;
                        }
                    }
                }
            }
        }
        private void RefreshAllRectLink()
        {
            foreach (var item in ItemsSource)
            {
                if (item is RectLinkShape rectLink)
                {
                    rectLink.DrawGeometry(Outter);
                    rectLink.Render();
                }
            }
        }
        /// <summary>
        /// 视图居中到所有节点的中心
        /// </summary>
        /// <param name="scaleFlag">视图无法显示所有节点的情况，是否进行缩放显示全部节点，缩放值有最大和最小值</param>
        public void ViewCenter(bool scaleFlag = true)
        {
            if (_shapeExistx.Count == 0) return;

            double left = double.MaxValue;
            double top = double.MaxValue;
            double right = double.MinValue;
            double bottom = double.MinValue;

            foreach (var item in _shapeExistx)
            {
                if (item is RectShape rect)
                {
                    if (rect.Position.X < left) left = rect.Position.X;
                    if (rect.Position.Y < top) top = rect.Position.Y;

                    var _right = rect.Position.X + rect.Size.Width;
                    if (_right > right) right = _right;

                    var _bottom = rect.Position.Y + rect.Size.Height;
                    if (_bottom > bottom) bottom = _bottom;
                }
            }

            double windowWidth = this.ActualWidth;
            double windowHeight = this.ActualHeight;

            if (windowWidth <= 0 || windowHeight <= 0) return;

            double boxWidth = right - left;
            double boxHeight = bottom - top;

            double paddingFactor = 0.9;
            double availableWidth = windowWidth * paddingFactor;
            double availableHeight = windowHeight * paddingFactor;

            double targetScale = Scale;

            if (scaleFlag)
            {
                double scaleX = availableWidth / boxWidth;
                double scaleY = availableHeight / boxHeight;

                targetScale = Math.Min(scaleX, scaleY);
                targetScale = Math.Min(Scale, targetScale);

                if (targetScale < SCALE_MIN) targetScale = SCALE_MIN;
                if (targetScale > SCALE_MAX) targetScale = SCALE_MAX;
            }

            double boxCenterX = ((left + right) / 2.0) * targetScale;
            double boxCenterY = ((top + bottom) / 2.0) * targetScale;

            double windowCenterX = windowWidth / 2.0;
            double windowCenterY = windowHeight / 2.0;

            _mouseDragTransform.X = windowCenterX - boxCenterX;
            _mouseDragTransform.Y = windowCenterY - boxCenterY;

            if (scaleFlag)
            {
                Scale = targetScale;
            }
            UpdatePreviewRect();
            UpdateResizeDragSize(_nodeResizeShape);
        }

        private void ScaleChange()
        {
            _mouseDragScale.ScaleX = _mouseDragScale.ScaleY = Scale;

            UpdatePreviewRect();
            UpdateResizeDragSize(_nodeResizeShape);
        }

        private void _resize_MouseEnter(object sender, MouseEventArgs e)
        {
            Point p1 = new Point(_nodeResizeShape.Position.X + _nodeResizeShape.Size.Width / 2, _nodeResizeShape.Position.Y + _nodeResizeShape.Size.Height / 2);

            var ele = (sender as FrameworkElement);
            var p2 = ele.TransformToAncestor(__resizePanel1).Transform(new Point(ele.ActualWidth / 2, ele.ActualHeight / 2));
            ele.Cursor = CommonHelper.GetCursorByPosition(p1, p2);
        }

        private void FlowChart_Loaded(object sender, RoutedEventArgs e)
        {
            ShowPreview();
        }

        private void ShowPreview()
        {
            if (DisablePreview) return;

            _previewMaskRect = new RectangleGeometry(new Rect(0, 0, PreviewSize.Width + PREVIEW_MARGIN_DOUBLE, PreviewSize.Height + PREVIEW_MARGIN_DOUBLE), PREVIEW_RADIUS, PREVIEW_RADIUS);
            _previewMaskRect.Freeze();

            using (var ctx = _previewVisual.RenderOpen())
            {
                ctx.DrawRoundedRectangle(_previewColor, null, new Rect(0, 0, PreviewSize.Width + PREVIEW_MARGIN_DOUBLE, PreviewSize.Height + PREVIEW_MARGIN_DOUBLE), PREVIEW_RADIUS, PREVIEW_RADIUS);
                ctx.DrawRectangle(_preview, null, new Rect(PREVIEW_MARGIN, PREVIEW_MARGIN, PreviewSize.Width, PreviewSize.Height));
            }
        }

        private void ItemsSource_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewItems != null)
                        foreach (var item in e.NewItems) CreateAndAddChildElement(item as IShape);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (e.OldItems != null)
                        foreach (var item in e.OldItems) RemoveChildElement(item as IShape);
                    break;
            }
        }
        private void RemoveChildElement(IShape shape)
        {
            _shapeExistx.Remove(shape);

            if (shape is ShapeBase shapeBase)
            {
                _renderContents.Children.Remove(shapeBase.Panel);
                shapeBase.PropertyChanged -= OnShapeBaseDataItemPropertyChanged;
            }

            if (shape is RectShape rect)
            {
                rect.ContentPanel.MouseLeftButtonDown -= ContentPanel_MouseDown;
                rect.ContentPanel.MouseLeftButtonUp -= ContentPanel_MouseUp;
                rect.ContentPanel.MouseEnter -= ContentPanel_MouseEnter;
                rect.ContentPanel.MouseLeave -= ContentPanel_MouseLeave;

                foreach (var portList in rect.Ports)
                {
                    foreach (var item in portList) RemoveChildPortElement(portList, item);
                    portList.CollectionChanged -= Ports_CollectionChanged;
                }

                RefreshLinkByRect(rect);
            }

            if (shape is LinkBase linkBase)
            {
                _renderContents.Children.Remove(linkBase.VisualHost);
                linkBase.PropertyChanged -= OnShapeBaseDataItemPropertyChanged;
                LinkMatchSourceAngTarget(linkBase);
                linkBase.Labels.CollectionChanged -= Labels_CollectionChanged;
            }
        }
        private void CreateAndAddChildElement(IShape shape)
        {
            if (_shapeExistx.Contains(shape))
            {
                throw new Exception("图形已存在其他Flow控件内");
            }

            if (shape is ShapeBase _shapeBase)
            {
                foreach (var item in ItemsSource)
                {
                    if (_shapeBase != item && item is ShapeBase local_shapeBase && local_shapeBase.NodeId == _shapeBase.NodeId)
                    {
                        throw new Exception($"NodeId: {_shapeBase.NodeId}，已存在");
                    }
                }
            }

            _shapeExistx.Add(shape);

            if (shape is ShapeBase shapeBase)
            {
                _renderContents.Children.Add(shapeBase.Panel);
                shapeBase.Panel.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                shapeBase.Panel.Arrange(new Rect(new Point(), shapeBase.Panel.DesiredSize));
                shapeBase.Panel.UpdateLayout();
                shapeBase.PropertyChanged += OnShapeBaseDataItemPropertyChanged;
            }

            if (shape is RectShape rect)
            {
                rect.ContentPanel.MouseLeftButtonDown += ContentPanel_MouseDown;
                rect.ContentPanel.MouseLeftButtonUp += ContentPanel_MouseUp;
                rect.ContentPanel.MouseEnter += ContentPanel_MouseEnter;
                rect.ContentPanel.MouseLeave += ContentPanel_MouseLeave;

                foreach (var portList in rect.Ports)
                {
                    foreach (var item in portList) CreateAndAddPortElement(portList, item);
                    portList.CollectionChanged += Ports_CollectionChanged;
                }

                RefreshLinkByRect(rect);
                rect.Panel.Opacity = rect.Opacity;
            }

            if (shape is LinkBase linkBase)
            {
                _renderContents.Children.Add(linkBase.VisualHost);
                linkBase.PropertyChanged += OnShapeBaseDataItemPropertyChanged;
                LinkMatchSourceAngTarget(linkBase);
                foreach (var label in linkBase.Labels) CreateLinkLabel(label, linkBase);
                linkBase.Labels.CollectionChanged += Labels_CollectionChanged;
            }

            if (shape is RectLinkShape link)
            {
                if (link.SourceRect != null && link.TargetRect != null && link.SourcreDirection != PortDirection.None && link.TargetDirection != PortDirection.None)
                {
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        link.DrawGeometry(Outter);
                        link.Render();
                    }), DispatcherPriority.Render);
                }
            }

            if (shape is NodeLinkShape nodeLink)
            {
                if (nodeLink.SourceRect != null && nodeLink.TargetRect != null)
                {
                    nodeLink.DrawGeometry(_renderContents);
                    nodeLink.Render();
                }
            }
        }

        private void RefreshLinkByRect(RectShape rect)
        {
            foreach (var item in ItemsSource)
            {
                if (item is LinkBase _linkBase && (_linkBase.SourceRect == rect || _linkBase.TargetRect == rect || _linkBase.Source == rect.NodeId || _linkBase.Target == rect.NodeId))
                {
                    LinkMatchSourceAngTarget(_linkBase);
                    if (_linkBase is RectLinkShape rectLink) rectLink.DrawGeometry(Outter);
                    if (_linkBase is NodeLinkShape _nodeLink) _nodeLink.DrawGeometry(_renderContents);
                    _linkBase.Render();
                    UpdatePreviewRect();
                }
            }
        }

        private Point _nodeMoveOffset;
        private RectShape _nodeMoveShape;
        private RectShape _nodeResizeShape;
        private object _dragHandle;

        private void _resizeDrag_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_dragHandle != null)
            {
                Mouse.Capture(null);
                _dragHandle = null;
                e.Handled = true;
            }
        }

        private void _resizeDrag_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_dragHandle == null)
            {
                Mouse.Capture(sender as FrameworkElement);
                _dragHandle = sender;
                e.Handled = true;
            }
        }

        private void ContentPanel_MouseEnter(object sender, MouseEventArgs e)
        {
            var rect = (sender as FrameworkElement).Tag as RectShape;
            var pos = TransformToVisual(_renderContents).Transform(e.GetPosition(this));
            ShapeMouseEnter?.Invoke(rect, new ShapeMouseEventArgs(pos));
        }

        private void ContentPanel_MouseLeave(object sender, MouseEventArgs e)
        {
            var rect = (sender as FrameworkElement).Tag as RectShape;
            var pos = TransformToVisual(_renderContents).Transform(e.GetPosition(this));
            ShapeMouseLeave?.Invoke(rect, new ShapeMouseEventArgs(pos));
        }

        private void ContentPanel_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_nodeMoveShape != null)
            {
                var rect = (sender as FrameworkElement).Tag as RectShape;
                var pos = TransformToVisual(_renderContents).Transform(e.GetPosition(this));
                ShapeMouseDragEnd?.Invoke(rect, new ShapeMouseButtonEventArgs(e.ButtonState, pos, e.LeftButton, e.RightButton, e.MiddleButton));
                _nodeMoveShape = null;
                e.Handled = true;
                Mouse.Capture(null);
            }
        }
        private void UpdateResizeDragSize(RectShape rect)
        {
            if (rect != null)
            {
                var size = RESIZE_DRAG_SIZE / Scale;
                var sizeHalve = RESIZE_DRAG_SIZE_HALVE / Scale;

                _resizeTopLeft.Width = _resizeTopLeft.Height = size;
                _resizeTopRight.Width = _resizeTopRight.Height = size;
                _resizeBottomLeft.Width = _resizeBottomLeft.Height = size;
                _resizeBottomRight.Width = _resizeBottomRight.Height = size;
                _resizeAngleHandle.Width = _resizeAngleHandle.Height = size;

                _resizeContent.StrokeThickness = RESIZE_HANDLE_LINE_WIDTH / Scale;
                _resizeContent.Width = rect.Size.Width + RESIZE_MARGIN_DOUBLE;
                _resizeContent.Height = rect.Size.Height + RESIZE_MARGIN_DOUBLE;

                var x = rect.PortThickness.Left;
                var y = rect.PortThickness.Top;

                (_resizeContent.RenderTransform as TranslateTransform).X = x - RESIZE_MARGIN;
                (_resizeContent.RenderTransform as TranslateTransform).Y = y - RESIZE_MARGIN;

                (_resizeTopLeft.RenderTransform as TranslateTransform).X = x - RESIZE_MARGIN - sizeHalve;
                (_resizeTopLeft.RenderTransform as TranslateTransform).Y = y - RESIZE_MARGIN - sizeHalve;

                (_resizeTopRight.RenderTransform as TranslateTransform).X = x + rect.Size.Width + RESIZE_MARGIN - sizeHalve;
                (_resizeTopRight.RenderTransform as TranslateTransform).Y = y - RESIZE_MARGIN - sizeHalve;

                (_resizeBottomLeft.RenderTransform as TranslateTransform).X = x - RESIZE_MARGIN - sizeHalve;
                (_resizeBottomLeft.RenderTransform as TranslateTransform).Y = y + rect.Size.Height + RESIZE_MARGIN - sizeHalve;

                (_resizeBottomRight.RenderTransform as TranslateTransform).X = x + rect.Size.Width + RESIZE_MARGIN - sizeHalve;
                (_resizeBottomRight.RenderTransform as TranslateTransform).Y = y + rect.Size.Height + RESIZE_MARGIN - sizeHalve;

                (_resizeAngleLine.RenderTransform as TranslateTransform).X = x + rect.Size.Width / 2;
                (_resizeAngleLine.RenderTransform as TranslateTransform).Y = y;


                (_resizeAngleHandle.RenderTransform as TranslateTransform).X = x + rect.Size.Width / 2 - sizeHalve;
                (_resizeAngleHandle.RenderTransform as TranslateTransform).Y = y - DRAG_ANGLE_HANDLE_LENGTH - sizeHalve;

                _resizeAngleLine.Y2 = -DRAG_ANGLE_HANDLE_LENGTH;
            }
        }
        private void ContentPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var rect = (sender as FrameworkElement).Tag as RectShape;

            if (_nodeMoveShape == null && !DisableDragMove)
            {
                var pos = TransformToVisual(_renderContents).Transform(e.GetPosition(this));
                ShapeMouseDragStart?.Invoke(rect, new ShapeMouseButtonEventArgs(e.ButtonState, pos, e.LeftButton, e.RightButton, e.MiddleButton));
                _nodeMoveShape = rect;
                _nodeMoveOffset = pos - _nodeMoveShape.Position;
                e.Handled = true;
                Mouse.Capture(sender as FrameworkElement);
            }

            if (!DisableResize)
            {
                _nodeResizeShape = rect;
                _resizeCanvas.Visibility = Visibility.Visible;
                _resizeCanvas.RenderTransform = _nodeResizeShape.MatrixTransform;
                UpdateResizeDragSize(_nodeResizeShape);
                e.Handled = true;
            }
        }
        private bool _resizeAngleFlag;
        private void _resizeAngleHandle_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_resizeAngleFlag)
            {
                Mouse.Capture(null);
                _resizeAngleFlag = false;
                e.Handled = true;
            }
        }

        private void _resizeAngleHandle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!_resizeAngleFlag && _nodeResizeShape != null)
            {
                Mouse.Capture(sender as FrameworkElement);
                _resizeAngleFlag = true;
                e.Handled = true;
            }
        }

        private void Labels_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewItems != null)
                    {
                        foreach (LinkLabel label in e.NewItems) CreateLinkLabel(label, (sender as LinkCollection<LinkLabel>).Link);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (e.OldItems != null)
                        foreach (LinkLabel label in e.OldItems)
                        {
                            RemoveLinkLabel(label);
                            label.PropertyChanged -= Label_PropertyChanged;
                        }
                    break;
            }
        }

        private void Label_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            LinkLabel label = sender as LinkLabel;
            switch (e.PropertyName)
            {
                case nameof(LinkLabel.Origin):
                case nameof(LinkLabel.Percentage):
                case nameof(LinkLabel.FollowAngle):
                    label.Link?.UpdateLabelsPosition();
                    break;
            }
        }

        private void CreateLinkLabel(LinkLabel linkLabel, LinkBase link)
        {
            linkLabel.ContentCtrl.RenderTransform = new MatrixTransform();
            _renderContents.Children.Add(linkLabel.ContentCtrl);
            linkLabel.ContentCtrl.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            linkLabel.ContentCtrl.Arrange(new Rect(new Point(), linkLabel.ContentCtrl.DesiredSize));
            linkLabel.ContentCtrl.UpdateLayout();

            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                linkLabel.Link = link;
                linkLabel.Link.UpdateLabelsPosition();
            }), DispatcherPriority.Render);
            linkLabel.PropertyChanged += Label_PropertyChanged;
        }

        private void RemoveLinkLabel(LinkLabel linkLabel)
        {
            _renderContents.Children.Remove(linkLabel.ContentCtrl);
        }

        private void LinkMatchSourceAngTarget(LinkBase link)
        {
            link.SourceRect = null;
            link.TargetRect = null;

            foreach (var item in ItemsSource)
            {
                if (item is RectShape rectShape)
                {
                    if (rectShape.NodeId == link.Source) link.SourceRect = rectShape;
                    if (rectShape.NodeId == link.Target) link.TargetRect = rectShape;

                    if (link.SourceRect != null && link.TargetRect != null) break;
                }
            }
        }

        private void Ports_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewItems != null)
                        foreach (var item in e.NewItems) CreateAndAddPortElement(sender, item as Port);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (e.OldItems != null)
                        foreach (var item in e.OldItems) RemoveChildPortElement(sender, item as Port);
                    break;
            }
        }
        private void CreateAndAddPortElement(object ports, Port port)
        {
            if (_portExistx.Contains(port))
            {
                throw new Exception("端口已存在其他Flow控件内");
            }
            _portExistx.Add(port);

            var shapeColl = ports as RectShapeCollection<Port>;

            if (shapeColl.Shape is RectShape rect)
            {
                switch (shapeColl.Dir)
                {
                    case PortDirection.Left:
                        port.HorizontalAlignment = HorizontalAlignment.Right;
                        port.VerticalAlignment = VerticalAlignment.Center;
                        break;
                    case PortDirection.Top:
                        port.HorizontalAlignment = HorizontalAlignment.Center;
                        port.VerticalAlignment = VerticalAlignment.Bottom;
                        break;
                    case PortDirection.Right:
                        port.HorizontalAlignment = HorizontalAlignment.Left;
                        port.VerticalAlignment = VerticalAlignment.Center;
                        break;
                    case PortDirection.Bottom:
                        port.HorizontalAlignment = HorizontalAlignment.Center;
                        port.VerticalAlignment = VerticalAlignment.Top;
                        break;
                }
                port.Dir = shapeColl.Dir;
                port.Shape = rect;
                port.MouseLeftButtonDown += Port_MouseDown;
                (rect.Panel.Children[(int)shapeColl.Dir] as Panel).Children.Add(port);
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    RefreshRectLink(rect);
                    UpdatePreviewRect();
                }), DispatcherPriority.Render);
            }
        }

        private RectLinkShape _newLink = null;
        private Port _newLinkPort = null;
        private Dictionary<Port, (Rect, Point)> _portBoundCheck = new Dictionary<Port, (Rect, Point)>();
        private (Rect, Point)? portCheck = null;
        private void Port_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var port = (Port)sender;
            if (_newLink == null && !port.DisbaleDragJoin)
            {
                Mouse.Capture(sender as UIElement);
                _newLinkPort = port;
                RectShape rect = _newLinkPort.Shape as RectShape;
                _newLink = new RectLinkShape();
                _newLink.Source = rect.NodeId;
                _newLink.SourcreDirection = _newLinkPort.Dir;
                _newLink.SourcreIndex = rect.Ports[(int)_newLinkPort.Dir - 1].IndexOf(_newLinkPort);

                if (DefaultNewLine != null)
                {
                    _newLink.Stroke = DefaultNewLine.Stroke;
                    _newLink.Stroke2 = DefaultNewLine.Stroke2;
                    _newLink.Stroke3 = DefaultNewLine.Stroke3;
                    _newLink.StrokeThickness = DefaultNewLine.StrokeThickness;
                    _newLink.StrokeThickness2 = DefaultNewLine.StrokeThickness2;
                    _newLink.StrokeThickness3 = DefaultNewLine.StrokeThickness3;
                    _newLink.StrokeDashStyle = DefaultNewLine.StrokeDashStyle;
                    _newLink.StrokeDashStyle2 = DefaultNewLine.StrokeDashStyle2;
                    _newLink.StrokeDashStyle3 = DefaultNewLine.StrokeDashStyle3;
                    _newLink.ArcRadius = DefaultNewLine.ArcRadius;
                    _newLink.Effect = DefaultNewLine.Effect;
                }

                ItemsSource.Add(_newLink);

                _portBoundCheck.Clear();
                for (int i = ItemsSource.Count - 1; i >= 0; --i)
                {
                    if (ItemsSource[i] is RectShape local_rect)
                    {
                        foreach (var ports in local_rect.Ports)
                        {
                            foreach (var localPort in ports)
                            {
                                if (localPort == _newLinkPort) continue;

                                var point = RectLinkShape.GetJoinPoint(localPort.Shape as RectShape, localPort, Outter, out _);
                                var bound = localPort.TransformToVisual(_renderContents).TransformBounds(VisualTreeHelper.GetDescendantBounds(localPort));
                                _portBoundCheck[localPort] = (bound, point.Center);
                            }
                        }
                    }
                }
            }
            e.Handled = true;
        }

        private void RemoveChildPortElement(object ports, Port port)
        {
            _portExistx.Remove(port);
            var shapeColl = ports as RectShapeCollection<Port>;

            if (shapeColl.Shape is RectShape rect)
            {
                port.MouseLeftButtonDown -= Port_MouseDown;
                (rect.Panel.Children[(int)shapeColl.Dir] as Panel).Children.Remove(port);
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    RefreshRectLink(rect);
                    UpdatePreviewRect();
                }), DispatcherPriority.Render);
            }
        }
        public void UpdateLinkByLink(LinkBase link)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (link is RectLinkShape local_link)
                {
                    local_link.DrawGeometry(Outter);
                    local_link.Render();
                }

                if (link is NodeLinkShape local_node_link)
                {
                    local_node_link.DrawGeometry(_renderContents);
                    local_node_link.Render();
                }
            }), DispatcherPriority.Render);
        }
        public void UpdateLinkByRectShape(RectShape rect) => this.Dispatcher.BeginInvoke(new Action(() => RefreshRectLink(rect)), DispatcherPriority.Render);
        private void RefreshRectLink(RectShape rect)
        {
            foreach (var item in ItemsSource)
            {
                if (item is RectLinkShape local_link && (local_link.SourceRect == rect || local_link.TargetRect == rect))
                {
                    local_link.DrawGeometry(Outter);
                    local_link.Render();
                }

                if (item is NodeLinkShape local_node_link && (local_node_link.SourceRect == rect || local_node_link.TargetRect == rect))
                {
                    local_node_link.DrawGeometry(_renderContents);
                    local_node_link.Render();
                }
            }
        }
        private void OnShapeBaseDataItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is RectShape rect)
            {
                if (e.PropertyName == nameof(RectShape.Size))
                {
                    rect.Panel.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                    rect.Panel.Arrange(new Rect(new Point(), rect.Panel.DesiredSize));
                    rect.Panel.UpdateLayout();
                }

                if (e.PropertyName == nameof(RectShape.NodeId))
                {
                    RefreshLinkByRect(rect);
                }

                switch (e.PropertyName)
                {
                    case nameof(RectShape.LeftPortPanel):
                    case nameof(RectShape.TopPortPanel):
                    case nameof(RectShape.RightPortPanel):
                    case nameof(RectShape.BottomPortPanel):
                        this.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            RefreshRectLink(rect);
                            UpdatePreviewRect();
                        }), DispatcherPriority.Render);
                        break;
                    case nameof(RectShape.Angle):
                    case nameof(RectShape.Size):
                    case nameof(RectShape.Radius):
                    case nameof(RectShape.Position):
                        RefreshRectLink(rect);
                        UpdatePreviewRect();
                        break;
                    case nameof(RectShape.Opacity):
                        rect.Panel.Opacity = rect.Opacity;
                        break;
                }
            }

            if (sender is LinkBase linkBase)
            {
                switch (e.PropertyName)
                {
                    case nameof(RectLinkShape.Source):
                    case nameof(RectLinkShape.Target):
                        LinkMatchSourceAngTarget(linkBase);
                        break;
                    case nameof(LinkBase.Stroke):
                    case nameof(LinkBase.Stroke2):
                    case nameof(LinkBase.Stroke3):
                    case nameof(LinkBase.StrokeThickness):
                    case nameof(LinkBase.StrokeThickness2):
                    case nameof(LinkBase.StrokeThickness3):
                    case nameof(LinkBase.StrokeDashStyle):
                    case nameof(LinkBase.StrokeDashStyle2):
                    case nameof(LinkBase.StrokeDashStyle3):
                        linkBase.Render();
                        break;
                }
            }

            if (sender is RectLinkShape link)
            {
                switch (e.PropertyName)
                {
                    case nameof(RectLinkShape.ArcRadius):
                    case nameof(RectLinkShape.LineType):
                        link.ReDrawGeometry();
                        link.Render();
                        break;
                    case nameof(RectLinkShape.Source):
                    case nameof(RectLinkShape.Target):
                        link.DrawGeometry(Outter);
                        link.Render();
                        break;
                    case nameof(RectLinkShape.SourcreDirection):
                    case nameof(RectLinkShape.SourcreIndex):
                    case nameof(RectLinkShape.TargetDirection):
                    case nameof(RectLinkShape.TargetIndex):
                        link.DrawGeometry(Outter);
                        link.Render();
                        break;
                }
            }

            if (sender is NodeLinkShape nodeLink)
            {
                switch (e.PropertyName)
                {
                    case nameof(RectLinkShape.Source):
                    case nameof(RectLinkShape.Target):
                        nodeLink.DrawGeometry(_renderContents);
                        nodeLink.Render();
                        break;
                    case nameof(NodeLinkShape.LineType):
                        nodeLink.ReGeometry();
                        nodeLink.Render();
                        break;
                }
            }
        }

        private Point? backDragMoveOffset;
        private Point _mouseDownPoint;

        private void FlowChart_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (DisableScale) return;

            const double zoomFactor = 1.1;
            double newScale = e.Delta > 0 ? Scale * zoomFactor : Scale / zoomFactor;

            if (newScale < SCALE_MIN) newScale = SCALE_MIN;
            if (newScale > SCALE_MAX) newScale = SCALE_MAX;

            Point position = this.TransformToVisual(_renderContents).Transform(e.GetPosition(this));
            double dx = position.X * (newScale - Scale);
            double dy = position.Y * (newScale - Scale);

            _mouseDragScale.ScaleX = _mouseDragScale.ScaleY = Scale = newScale;
            _mouseDragTransform.X -= dx;
            _mouseDragTransform.Y -= dy;

            UpdatePreviewRect();
            UpdateResizeDragSize(_nodeResizeShape);
        }

        private Point mouseMovePoint;
        private void FlowChart_MouseMove(object sender, MouseEventArgs e)
        {
            mouseMovePoint = e.GetPosition(this);

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (_nodeMoveShape != null)
                {
                    var rect = (sender as FrameworkElement).Tag as RectShape;
                    var pos = TransformToVisual(_renderContents).Transform(e.GetPosition(this));
                    _nodeMoveShape.Position = pos - _nodeMoveOffset;
                    ShapeMouseDragMove?.Invoke(rect, new ShapeMouseEventArgs(pos));
                }

                if (backDragMoveOffset != null && !DisableDragBack)
                {
                    var pos = mouseMovePoint - backDragMoveOffset.Value;
                    _mouseDragTransform.X = pos.X;
                    _mouseDragTransform.Y = pos.Y;
                    UpdatePreviewRect();
                }

                if (_newLink != null)
                {
                    var targetPoint = TransformToVisual(_renderContents).Transform(mouseMovePoint);

                    if (portCheck != null)
                    {
                        if (!portCheck.Value.Item1.Contains(targetPoint) || (portCheck.Value.Item2 - targetPoint).Length >= AdsorptionRadius)
                        {
                            _newLink.Target = null;
                            _newLink.TargetRect = null;
                            portCheck = null;
                        }
                    }

                    if (portCheck == null)
                    {
                        KeyValuePair<Port, (Rect, Point)>? tmpKV = null;
                        double minDis = double.MaxValue;

                        foreach (var kv in _portBoundCheck)
                        {
                            var inOutflag = !kv.Key.DisbaleDragJoin && (_newLinkPort.Type == PortType.None || (((_newLinkPort.Type == PortType.In && kv.Key.Type == PortType.Out) || (_newLinkPort.Type == PortType.Out && kv.Key.Type == PortType.In)) && _newLinkPort.ValueType == kv.Key.ValueType));
                            
                            if (portCheck == null && kv.Value.Item1.Contains(targetPoint) && inOutflag)
                            {
                                var rect = kv.Key.Shape as RectShape;
                                _newLink.TargetDirection = kv.Key.Dir;
                                _newLink.TargetIndex = rect.Ports[(int)kv.Key.Dir - 1].IndexOf(kv.Key);
                                _newLink.TargetRect = rect;
                                _newLink.Target = rect.NodeId;
                                portCheck = kv.Value;
                            }

                            var dis = (kv.Value.Item2 - targetPoint).LengthSquared;
                            if (dis < minDis && inOutflag)
                            {
                                minDis = dis;
                                tmpKV = kv;
                            }
                        }

                        if (portCheck == null && Math.Sqrt(minDis) < AdsorptionRadius)
                        {
                            var rect = tmpKV.Value.Key.Shape as RectShape;
                            _newLink.TargetDirection = tmpKV.Value.Key.Dir;
                            _newLink.TargetIndex = rect.Ports[(int)tmpKV.Value.Key.Dir - 1].IndexOf(tmpKV.Value.Key);
                            _newLink.TargetRect = rect;
                            _newLink.Target = rect.NodeId;
                            portCheck = tmpKV.Value.Value;
                        }
                    }

                    if (portCheck == null) _newLink.DrawGeometry(Outter, targetPoint);
                    else _newLink.DrawGeometry(Outter);

                    _newLink.Render();
                    UpdatePreviewRect();
                }


                if (_dragHandle != null && _nodeResizeShape != null)
                {
                    double oldWidth = _nodeResizeShape.Size.Width;
                    double oldHeight = _nodeResizeShape.Size.Height;
                    double angle = _nodeResizeShape.Angle;

                    Matrix currentMat = Matrix.Identity;
                    currentMat.RotateAt(angle, oldWidth / 2, oldHeight / 2);
                    currentMat.Translate(_nodeResizeShape.Position.X, _nodeResizeShape.Position.Y);
                    Point globalMousePoint = TransformToVisual(_renderContents).Transform(e.GetPosition(this));

                    double localFixedX = (_dragHandle == _resizeTopLeft || _dragHandle == _resizeBottomLeft) ? (oldWidth + RESIZE_MARGIN) : -RESIZE_MARGIN;
                    double localFixedY = (_dragHandle == _resizeTopLeft || _dragHandle == _resizeTopRight) ? (oldHeight + RESIZE_MARGIN) : -RESIZE_MARGIN;

                    Point globalFixedPoint = currentMat.Transform(new Point(localFixedX, localFixedY));

                    Matrix inverseMat = currentMat;
                    inverseMat.Invert();
                    Point localMouse = inverseMat.Transform(globalMousePoint);
                    Point localFixed = inverseMat.Transform(globalFixedPoint);

                    double newWidth = oldWidth;
                    double newHeight = oldHeight;

                    if (_dragHandle == _resizeTopLeft || _dragHandle == _resizeBottomLeft)
                        newWidth = Math.Max(NODE_MAX_SIZE, (localFixed.X - localMouse.X) - 2 * RESIZE_MARGIN);
                    else if (_dragHandle == _resizeTopRight || _dragHandle == _resizeBottomRight)
                        newWidth = Math.Max(NODE_MAX_SIZE, (localMouse.X - localFixed.X) - 2 * RESIZE_MARGIN);

                    if (_dragHandle == _resizeTopLeft || _dragHandle == _resizeTopRight)
                        newHeight = Math.Max(NODE_MAX_SIZE, (localFixed.Y - localMouse.Y) - 2 * RESIZE_MARGIN);
                    else if (_dragHandle == _resizeBottomLeft || _dragHandle == _resizeBottomRight)
                        newHeight = Math.Max(NODE_MAX_SIZE, (localMouse.Y - localFixed.Y) - 2 * RESIZE_MARGIN);

                    Matrix newLocalRotateMat = Matrix.Identity;
                    newLocalRotateMat.RotateAt(angle, newWidth / 2, newHeight / 2);

                    double newLocalFixedX = (_dragHandle == _resizeTopLeft || _dragHandle == _resizeBottomLeft) ? (newWidth + RESIZE_MARGIN) : -RESIZE_MARGIN;
                    double newLocalFixedY = (_dragHandle == _resizeTopLeft || _dragHandle == _resizeTopRight) ? (newHeight + RESIZE_MARGIN) : -RESIZE_MARGIN;
                    Point newTempRotatedFixedPoint = newLocalRotateMat.Transform(new Point(newLocalFixedX, newLocalFixedY));

                    double newTranslateX = globalFixedPoint.X - newTempRotatedFixedPoint.X;
                    double newTranslateY = globalFixedPoint.Y - newTempRotatedFixedPoint.Y;

                    _nodeResizeShape.Size = new Size(newWidth, newHeight);
                    _nodeResizeShape.Position = new Vector(newTranslateX, newTranslateY);

                    UpdateResizeDragSize(_nodeResizeShape);
                }

                if (_resizeAngleFlag && _nodeResizeShape != null)
                {
                    var targetPoint = TransformToVisual(_renderContents).Transform(mouseMovePoint);
                    var handleX = _nodeResizeShape.Position.X + _nodeResizeShape.Size.Width / 2;
                    var handleY = _nodeResizeShape.Position.Y + _nodeResizeShape.Size.Height / 2;
                    Vector dir = targetPoint - new Point(handleX, handleY);
                    var angle = Vector.AngleBetween(new Vector(0, -1), dir);
                    _nodeResizeShape.Angle = angle < 0 ? angle + 360 : angle;
                    RefreshRectLink(_nodeResizeShape);
                }
            }
        }

        private void FlowChart_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (backDragMoveOffset != null)
            {
                Mouse.Capture(null);
                backDragMoveOffset = null;
                this.Cursor = this.DisableDragBack ? Cursors.Arrow : pointerGrabCursor;
            }

            if (_newLink != null)
            {
                if (_newLink.Target == null)
                {
                    ItemsSource.Remove(_newLink);
                }
                DragAddNewLink?.Invoke(_newLink, new NewLineEventArgs(_newLink));
                _newLink = null;
                _newLinkPort = null;
                Mouse.Capture(null);
            }
        }
        private void FlowChart_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _mouseDownPoint = e.GetPosition(this);
            VisualTreeHelper.HitTest(this, FlowChartHitTestFilter, FlowChartMouseDownHitTestResult, new PointHitTestParameters(_mouseDownPoint));
        }

        private HitTestFilterBehavior FlowChartHitTestFilter(DependencyObject o)
        {
            if (o != this && o is UIElement) return HitTestFilterBehavior.ContinueSkipSelfAndChildren;
            return HitTestFilterBehavior.Continue;
        }

        private HitTestResultBehavior FlowChartMouseDownHitTestResult(HitTestResult result)
        {
            if (result is HitTestResult hitResult)
            {
                if (backDragMoveOffset == null && !DisableDragBack && hitResult.VisualHit == _renderBackground)
                {
                    if (_nodeResizeShape != null)
                    {
                        _resizeCanvas.Visibility = Visibility.Collapsed;
                        _nodeResizeShape = null;
                    }
                    Mouse.Capture(this);
                    backDragMoveOffset = _mouseDownPoint - new Vector(_mouseDragTransform.X, _mouseDragTransform.Y);
                    this.Cursor = pointerGrabbingCursor;
                    return HitTestResultBehavior.Stop;
                }
            }
            return HitTestResultBehavior.Continue;
        }
        private void ClearAndHidePreview()
        {
            _previewVisual.RenderOpen().Close();
            _previewMask.RenderOpen().Close();
        }
        private void UpdatePreviewRect()
        {
            if (DisablePreview) return;

            var contentBounds = VisualTreeHelper.GetDescendantBounds(this);
            double scale = Math.Min(PreviewSize.Width / contentBounds.Width, PreviewSize.Height / contentBounds.Height);
            double scaledWidth = contentBounds.Width * scale;
            double scaledHeight = contentBounds.Height * scale;
            double offsetX = (PreviewSize.Width - scaledWidth) / 2 - (contentBounds.Left * scale);
            double offsetY = (PreviewSize.Height - scaledHeight) / 2 - (contentBounds.Top * scale);
            var rect = new Rect(offsetX + PREVIEW_MARGIN, offsetY + PREVIEW_MARGIN, ActualWidth * scale, ActualHeight * scale);
            using (var ctx = _previewMask.RenderOpen())
            {
                var pvRectGeo2 = new RectangleGeometry(rect, PREVIEW_RADIUS, PREVIEW_RADIUS);
                pvRectGeo2.Freeze();
                var geo = new CombinedGeometry(GeometryCombineMode.Exclude, _previewMaskRect, pvRectGeo2);
                geo.Freeze();
                ctx.DrawGeometry(_previewMaskColor, null, geo);
            }
        }
        private void RenderBackground()
        {

            using (var ctx = _background.RenderOpen())
            {
                ctx.DrawRectangle(Background, null, new Rect(0, 0, ActualWidth, ActualHeight));
                if (GridStyle != null)
                {
                    var brush = (GridStyle as GridBase).GenerateDrawingBrush();
                    brush.Transform = _mouseDragTran;
                    ctx.DrawRectangle(brush, null, new Rect(0, 0, ActualWidth, ActualHeight));
                }
            }
        }
        private void UpdatePreviewOffset()
        {
            if (DisablePreview) return;
            _previewRoot.Offset = new Vector(ActualWidth - (PreviewSize.Width + PREVIEW_MARGIN_DOUBLE) - 10, ActualHeight - (PreviewSize.Height + PREVIEW_MARGIN_DOUBLE) - 10);
        }
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            RenderBackground();
            UpdatePreviewRect();
            using (var ctx = _renderBackground.RenderOpen())
            {
                ctx.DrawRectangle(Brushes.Transparent, null, new Rect(0, 0, ActualWidth, ActualHeight));
            }
            UpdatePreviewOffset();
        }
        protected override Size MeasureOverride(Size availableSize)
        {
            foreach (var element in EnumerateUIElements(_childrenRoot))
            {
                element.Measure(availableSize);
            }
            return base.MeasureOverride(availableSize);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            foreach (var element in EnumerateUIElements(_childrenRoot))
            {
                element.Arrange(new Rect(new Point(0, 0), element.DesiredSize));
            }
            return finalSize;
        }
        private static IEnumerable<UIElement> EnumerateUIElements(Visual visual)
        {
            if (visual == null)
                yield break;

            if (visual is UIElement uiElement)
                yield return uiElement;

            if (visual is ContainerVisual container)
            {
                foreach (var child in container.Children)
                {
                    foreach (var result in EnumerateUIElements(child))
                    {
                        yield return result;
                    }
                }
            }
        }
        protected override int VisualChildrenCount => _children.Count;
        protected override Visual GetVisualChild(int index) => _children[index];
    }
}
