//===========================================================================//
//qq：1018720141     qq群：1064754010                                        //
//===========================================================================//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Markup;

namespace WpfFlow
{
    /// <summary>
    /// 线标签
    /// </summary>
    [ContentProperty("Content")]
    public class LinkLabel : LabelBase
    {
        private double _percentage = 0.5;
        /// <summary>
        /// 标签显示在线的指定位置，百分比0.0~1.0
        /// </summary>
        [DefaultValue(0.5)]
        public double Percentage
        {
            get => this._percentage;
            set { this._percentage = value; NotifyPropertyChanged(); }
        }

        private bool _followAngle = false;
        /// <summary>
        /// 标签是否跟随线的方向
        /// </summary>
        [DefaultValue(false)]
        public bool FollowAngle
        {
            get => this._followAngle;
            set { this._followAngle = value; NotifyPropertyChanged(); }
        }

        private object _content;
        /// <summary>
        /// 标签内容
        /// </summary>
        public object Content
        {
            get => this._content;
            set { this._content = value; ContentCtrl.Content = value; NotifyPropertyChanged(); }
        }

        internal LinkBase Link { get;set; }

        internal ContentControl ContentCtrl { get; set; } = new ContentControl();
    }
}
