//===========================================================================//
//qq：1018720141     qq群：1064754010                                        //
//===========================================================================//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Effects;
using WpfFlow.Helper;
using WpfFlow.Interface;
using WpfFlow.Shape;

namespace WpfFlow
{
    [DefaultProperty("Content")]
    [ContentProperty("Content")]
    public abstract class ShapeBase : INotifyPropertyChanged, IShape
    {
        private string _node_id;
        /// <summary>
        /// 节点Id，用于连线识别的标识
        /// </summary>
        public string NodeId
        {
            get => this._node_id;
            set { this._node_id = value; NotifyPropertyChanged(); }
        }

        private object _content;
        /// <summary>
        /// 节点内容
        /// </summary>
        public object Content
        {
            get => this._content;
            set { this._content = value; NotifyPropertyChanged(); }
        }

        private Brush _stroke;
        /// <summary>
        /// 节点线颜色
        /// </summary>
        public Brush Stroke
        {
            get => this._stroke;
            set { this._stroke = value; this._stroke?.Freeze();NotifyPropertyChanged(); }
        }

        private Thickness _strokeThickness = new Thickness(1);
        /// <summary>
        /// 节点线粗细
        /// </summary>
        [DefaultValue(typeof(Thickness), "1")]
        public Thickness StrokeThickness
        {
            get => this._strokeThickness;
            set { this._strokeThickness = value;  NotifyPropertyChanged(); }
        }

        private double _opacity = 1;
        /// <summary>
        /// 节点透明度
        /// </summary>
        [DefaultValue(1.0)]
        public double Opacity
        {
            get => this._opacity;
            set { this._opacity = value; NotifyPropertyChanged(); }
        }

        private Effect _effect;
        /// <summary>
        /// 节点特效
        /// </summary>
        public Effect Effect
        {
            get => this._effect;
            set { this._effect = value; NotifyPropertyChanged(); }
        }

        /// <summary>
        /// 节点位置
        /// </summary>
        public Vector Position { get; set; }
        /// <summary>
        /// 节点大小
        /// </summary>
        public Vector Size { get; set; }
        /// <summary>
        /// 节点旋转角度(0~360)
        /// </summary>
        public double Angle { get;set; }

        internal ShapeHost Panel { get; set; } = new ShapeHost();
        internal MatrixTransform MatrixTransform { get; set; } = new MatrixTransform();

        public ShapeBase()
        {
            Panel.Cursor = Cursors.SizeAll;
            Panel.RenderTransform = MatrixTransform;
            BindingOperations.SetBinding(Panel, Canvas.EffectProperty, CommonHelper.CreateBinding(nameof(Effect), this));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
