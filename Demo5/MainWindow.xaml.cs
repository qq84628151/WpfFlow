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
using WpfFlow.FlowEventArgs;

namespace Demo5
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            FlowChart.DisableResize = true;
        }

        private void CloseLink_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (staticClose == sender)
            {
                FlowChart.ItemsSource.Remove(staticLink);
                test_ellipse.Fill = Brushes.LightGray;
            }
            else
            {
                var link = (sender as Grid).Tag as LinkBase;
                FlowChart.ItemsSource.Remove(link);

                if (link.Source == flowRect1.NodeId || link.Target == flowRect1.NodeId)
                {
                    test_ellipse.Fill = Brushes.LightGray;
                }

                if (link.Source == flowRect2.NodeId || link.Target == flowRect2.NodeId)
                {
                    test_rectangle.Fill = Brushes.LightGray;
                }
            }

            UpdateJoinColor();
        }

        private void FlowChart_DragAddNewLink(object sender, NewLineEventArgs e)
        {
            if (e.Source == flowRect1 || e.Target == flowRect1)
            {
                UpdateColor(test_ellipse);
            }

            if (e.Source == flowRect2 || e.Target == flowRect2)
            {
                UpdateColor(test_rectangle);
            }

            foreach (var item in FlowChart.ItemsSource)
            {
                if (item is RectLinkShape link)
                {
                    if (link != e.Link && ((link.Source == e.Link.Source && link.Target == e.Link.Target) || (link.Source == e.Link.Target && link.Target == e.Link.Source)))
                    {
                        FlowChart.ItemsSource.Remove(e.Link);
                        break;
                    }
                }
            }

            if (FlowChart.ItemsSource.Contains(e.Link))
            {
                var grid = new Grid() { Width = 15, Height = 15, Background = Brushes.Transparent, Cursor = Cursors.Arrow };
                grid.Children.Add(new Image() { Source = (ImageSource)Application.Current.Resources["DeleteNodeIcon"] });
                grid.Tag = e.Link;
                grid.MouseDown += CloseLink_MouseDown;

                e.Link.Labels.Add(new LinkLabel() { Content = grid });
            }

            UpdateJoinColor();
        }

        private void color_Checked(object sender, RoutedEventArgs e)
        {
            foreach (var item in FlowChart.ItemsSource)
            {
                if (item is RectLinkShape link)
                {
                    if (link.Source == flowRect1.NodeId || link.Target == flowRect1.NodeId)
                    {
                        UpdateColor(test_ellipse);
                    }

                    if (link.Source == flowRect2.NodeId || link.Target == flowRect2.NodeId)
                    {
                        UpdateColor(test_rectangle);
                    }
                }
            }
        }

        private void UpdateColor(Shape ui)
        {
            if ((bool)color1.IsChecked)
            {
                ui.Fill = color_rect1.Fill;
            }

            if ((bool)color2.IsChecked)
            {
                ui.Fill = color_rect2.Fill;
            }

            if ((bool)color3.IsChecked)
            {
                ui.Fill = color_rect3.Fill;
            }
        }

        private void UpdateJoinColor()
        {
            outJoin1.Fill = Brushes.LightGray;
            outJoin2.Fill = Brushes.LightGray;
            outJoin3.Fill = Brushes.LightGray;

            foreach (var item in FlowChart.ItemsSource)
            {
                if (item is RectLinkShape link)
                {
                    if (link.Source == flowRect0.NodeId || link.Target == flowRect0.NodeId)
                    {
                        outJoin1.Fill = Brushes.LightGreen;
                    }

                    if (link.Source == flowRect1.NodeId || link.Target == flowRect1.NodeId)
                    {
                        outJoin2.Fill = Brushes.LightGreen;
                    }

                    if (link.Source == flowRect2.NodeId || link.Target == flowRect2.NodeId)
                    {
                        outJoin3.Fill = Brushes.LightGreen;
                    }
                }
            }
        }
    }
}
