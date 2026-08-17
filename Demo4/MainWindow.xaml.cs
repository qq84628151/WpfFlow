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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfFlow;

namespace Demo4
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private Random random = new Random();
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            FlowChart.DisableResize = true;

            Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(1500);

                    await this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        DoubleAnimation animation = new DoubleAnimation
                        {
                            To = random.Next(20, 80),
                            Duration = TimeSpan.FromSeconds(0.5),
                            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                        };

                        TankProcess.BeginAnimation(ProgressBar.ValueProperty, animation);
                    }));
                }
            });

            Fan1Rotate.BeginAnimation(RotateTransform.AngleProperty, new DoubleAnimation
            {
                To = 360,
                Duration = TimeSpan.FromSeconds(1),
                RepeatBehavior = RepeatBehavior.Forever
            });

            Fan1Rotate2.BeginAnimation(RotateTransform.AngleProperty, new DoubleAnimation
            {
                To = 360,
                Duration = TimeSpan.FromSeconds(1),
                RepeatBehavior = RepeatBehavior.Forever
            });

            foreach (var shape in FlowChart.ItemsSource)
            {
                if (shape is RectLinkShape rectLink)
                {
                    DoubleAnimation flowAnim = new DoubleAnimation
                    {
                        From = rectLink.StrokeDashStyle.Dashes.Aggregate(0.0, (current, next) => current + next),
                        To = 0,
                        Duration = TimeSpan.FromSeconds(1),
                        RepeatBehavior = RepeatBehavior.Forever
                    };
                    rectLink.StrokeDashStyle.BeginAnimation(DashStyle.OffsetProperty, flowAnim);
                }
            }
        }
    }
}
