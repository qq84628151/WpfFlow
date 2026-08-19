using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfFlow;
using WpfFlow.Enum;

namespace Demo6
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private int index = 0;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Border panel = sender as Border;
            Image img = panel.Child as Image;
            ItemData data = null;
            if (img == RectShape || img == RectRadiusShape)
            {
                data = new ItemData(img.Source, 50, 30);
            }
            else
            {
                data = new ItemData(img.Source, 50, 50);
            }
            DragDrop.DoDragDrop(panel, new DataObject("ItemData", data), DragDropEffects.Copy);
        }

        private void FlowChart_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("ItemData"))
            {
                ItemData itemData = e.Data.GetData("ItemData") as ItemData;
                DrawingImage drawingImage = itemData.Source as DrawingImage;
                Point dropPoint = e.GetPosition(FlowChart);


                var path = new Path();
                path.Stretch = Stretch.Fill;
                path.Data = (drawingImage.Drawing as GeometryDrawing).Geometry;
                path.Stroke = Brushes.Black;
                path.StrokeThickness = 1;

                var rectShape = new RectShape();
                rectShape.NodeId = $"node_{index++}";
                rectShape.Size = new Size(itemData.Width, itemData.Height);
                rectShape.Position = new Vector(-FlowChart.ViewRealOffset.X + dropPoint.X, -FlowChart.ViewRealOffset.Y + dropPoint.Y);
                rectShape.Content = path;
                rectShape.HorizontalAlignment = HorizontalAlignment.Stretch;
                rectShape.VerticalAlignment = VerticalAlignment.Stretch;
                rectShape.Fill = Brushes.Transparent;

                var leftPort = new Port() { JoinAlignType = PortJoinAlign.Center, Content = new Rectangle() { Width = 10, Height = 10, Fill = Brushes.Transparent, RenderTransform = new TranslateTransform(5, 0) } };
                var topPort = new Port() { JoinAlignType = PortJoinAlign.Center, Content = new Rectangle() { Width = 10, Height = 10, Fill = Brushes.Transparent, RenderTransform = new TranslateTransform(0, 5) } };
                var rightPort = new Port() { JoinAlignType = PortJoinAlign.Center, Content = new Rectangle() { Width = 10, Height = 10, Fill = Brushes.Transparent, RenderTransform = new TranslateTransform(-5, 0) } };
                var bottomPort = new Port() { JoinAlignType = PortJoinAlign.Center, Content = new Rectangle() { Width = 10, Height = 10, Fill = Brushes.Transparent, RenderTransform = new TranslateTransform(0, -5) } };

                rectShape.LeftPort.Add(leftPort);
                rectShape.TopPort.Add(topPort);
                rectShape.RightPort.Add(rightPort);
                rectShape.BottomPort.Add(bottomPort);

                FlowChart.ItemsSource.Add(rectShape);
            }
        }

        public class ItemData
        {
            public ImageSource Source { get; set; }
            public double Width { get; set; }
            public double Height { get; set; }

            public ItemData(ImageSource source, double width, double height)
            {
                Source = source;
                Width = width;
                Height = height;
            }
        }
    }
}
