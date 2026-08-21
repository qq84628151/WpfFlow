using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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
using WpfFlow.FlowEventArgs;

namespace Demo8
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        const double ITEM_WIDTH = 100;
        const double ITEM_HEIGHT = 30;
        const double ITEM_HEIGHT_HALF = ITEM_HEIGHT / 2;

        private readonly List<RectShape> leftPortList = new List<RectShape>();
        private readonly List<RectShape> rightPortList = new List<RectShape>();
        private List<Point> leftPortOffsetList = null;
        private List<Point> rightPortOffsetList = null;
        public MainWindow()
        {
            InitializeComponent();
            LeftPort.Tag = StartRect;
            RightPort.Tag = StartRect;
        }

        private void PortLeft_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var port = sender as Port;
            var parent = port.Tag as RectShape;
            var newRect = new RectShape();
            newRect.Size = new Size(ITEM_WIDTH, ITEM_HEIGHT);
            newRect.NodeId = $"{Guid.NewGuid().ToString()}";
            newRect.Stroke = Brushes.LightSeaGreen;
            newRect.StrokeThickness = new Thickness(0, 0, 0, 1);
            newRect.Radius = new CornerRadius(0);
            newRect.Content = "子节点";
            newRect.LeftPortPanel = PanelType.StackPanel;
            newRect.RightPortPanel = PanelType.StackPanel;
            var rightPort = new Port() { DisbaleDragJoin = true };
            rightPort.RenderTransform = new TranslateTransform(0, ITEM_HEIGHT - 0.5);
            newRect.RightPort.Add(rightPort);

            RectLinkShape link = new RectLinkShape();
            link.Stroke = newRect.Stroke;
            link.Source = parent.NodeId;
            link.SourcreDirection = PortDirection.Left;
            link.Target = newRect.NodeId;
            link.TargetDirection = PortDirection.Right;
            link.LineType = RectLinkType.Bezier;

            leftPortList.Add(newRect);
            NodeAlign(parent, leftPortList, -ITEM_WIDTH - 50);

            FlowChart.ItemsSource.Add(newRect);
            FlowChart.ItemsSource.Add(link);
        }

        private void PortRight_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var port = sender as Port;
            var rect = port.Tag as RectShape;
            var newRect = new RectShape();
            newRect.Size = new Size(ITEM_WIDTH, ITEM_HEIGHT);
            newRect.NodeId = $"{Guid.NewGuid().ToString()}";
            newRect.Stroke = Brushes.LightSeaGreen;
            newRect.StrokeThickness = new Thickness(0, 0, 0, 1);
            newRect.Radius = new CornerRadius(0);
            newRect.Content = "子节点";
            newRect.LeftPortPanel = PanelType.StackPanel;
            newRect.RightPortPanel = PanelType.StackPanel;
            var leftPort = new Port() { DisbaleDragJoin = true };
            leftPort.RenderTransform = new TranslateTransform(0, ITEM_HEIGHT - 0.5);
            newRect.LeftPort.Add(leftPort);

            RectLinkShape link = new RectLinkShape();
            link.Stroke = newRect.Stroke;
            link.Source = rect.NodeId;
            link.SourcreDirection = PortDirection.Right;
            link.Target = newRect.NodeId;
            link.TargetDirection = PortDirection.Left;
            link.LineType = RectLinkType.Bezier;

            rightPortList.Add(newRect);
            NodeAlign(rect, rightPortList, ITEM_WIDTH + 50);

            FlowChart.ItemsSource.Add(newRect);
            FlowChart.ItemsSource.Add(link);
        }

        private void NodeAlign(RectShape parent, List<RectShape> childrens, double offsetX)
        {
            double centerY = parent.Position.Y + parent.Size.Height / (parent == StartRect ? 2 : 1);
            double totalHeight = childrens.Count * ITEM_HEIGHT + (childrens.Count - 1) * 10;
            double startY = centerY - totalHeight / 2;

            for (int i = 0; i < childrens.Count; ++i)
            {
                var localRect = childrens[i];
                double localX = parent.Position.X + offsetX;
                double localY = startY + i * (ITEM_HEIGHT + 10) - ITEM_HEIGHT_HALF;
                localRect.Position = new Vector(localX, localY);
            }
        }

        private bool dragFlag = false;
        private void FlowChart_ShapeMouseDragStart(object sender, ShapeMouseButtonEventArgs e)
        {
            if (!dragFlag)
            {
                leftPortOffsetList = new List<Point>(leftPortList.Count);
                rightPortOffsetList = new List<Point>(rightPortList.Count);
                for (int i = 0; i < leftPortList.Count; ++i)
                {
                    leftPortOffsetList.Add(e.Position - leftPortList[i].Position);
                }

                for (int i = 0; i < rightPortList.Count; ++i)
                {
                    rightPortOffsetList.Add(e.Position - rightPortList[i].Position);
                }
                dragFlag = true;
            }
        }

        private void FlowChart_ShapeMouseDragEnd(object sender, ShapeMouseButtonEventArgs e)
        {
            if (dragFlag)
            {
                dragFlag = false;
            }
        }

        private void FlowChart_ShapeMouseDragMove(object sender, ShapeMouseEventArgs e)
        {
            if (dragFlag)
            {
                for (int i = 0; i < leftPortOffsetList.Count; ++i)
                {
                    leftPortList[i].Position = e.Position - leftPortOffsetList[i];
                }

                for (int i = 0; i < rightPortOffsetList.Count; ++i)
                {
                    rightPortList[i].Position = e.Position - rightPortOffsetList[i];
                }
            }
        }
    }
}
