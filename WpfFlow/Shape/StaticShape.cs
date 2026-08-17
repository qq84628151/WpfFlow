//===========================================================================//
//qq：1018720141     qq群：1064754010                                        //
//===========================================================================//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using WpfFlow.Helper;

namespace WpfFlow
{
    /// <summary>
    /// 静态节点，用于显示自定义内容
    /// </summary>
    public class StaticShape : ShapeBase
    {
        private Size _size;
        /// <summary>
        /// 节点大小
        /// </summary>
        public new Size Size
        {
            get => this._size;
            set
            {
                this._size = value;
                ContentControl.Width = Panel.Width = value.Width;
                ContentControl.Height = Panel.Height = value.Height;
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
            set { this._position = value; UpdateMatri(); NotifyPropertyChanged(); }
        }

        private object _content;
        /// <summary>
        /// 节点内容
        /// </summary>
        public new object Content
        {
            get => this._content;
            set { this._content = value; NotifyPropertyChanged(); }
        }

        internal ContentControl ContentControl { get; set; } = new ContentControl();

        public StaticShape()
        {
            Panel.Cursor = Cursors.Arrow;
            BindingOperations.SetBinding(ContentControl, ContentControl.ContentProperty, CommonHelper.CreateBinding(nameof(Content), this));
            Panel.Children.Add(ContentControl);
        }
        private void UpdateMatri()
        {
            var matrix = Matrix.Identity;
            matrix.Translate(Position.X, Position.Y);
            MatrixTransform.Matrix = matrix;
        }
    }
}
