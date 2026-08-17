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

namespace WpfFlow
{
    /// <summary>
    /// 标签
    /// </summary>
    public class LabelBase : INotifyPropertyChanged
    {
        /// <summary>
        /// 标签对齐原点
        /// </summary>
        private Point _origin = new Point(0.5, 0.5);
        [DefaultValue(typeof(Point), "0.5,0.5")]
        public Point Origin
        {
            get => this._origin;
            set { this._origin = value; NotifyPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
