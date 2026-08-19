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
using System.Windows.Input;
using System.Windows.Markup;
using WpfFlow.Enum;

namespace WpfFlow
{
    /// <summary>
    /// 节点端口
    /// </summary>
    public class Port : ContentControl
    {
        private PortType _type;
        /// <summary>
        /// 端口类型
        /// </summary>
        public PortType Type
        {
            get => this._type;
            set { this._type = value; NotifyPropertyChanged(); }
        }
        /// <summary>
        /// 类型对应的匹配值
        /// </summary>
        private string _valueType;
        public string ValueType
        {
            get => this._valueType;
            set { this._valueType = value; NotifyPropertyChanged(); }
        }

        private bool _disbaleDragJoin;
        /// <summary>
        /// 禁用被拖拽连接
        /// </summary>
        public bool DisbaleDragJoin
        {
            get => this._disbaleDragJoin;
            set
            {
                this._disbaleDragJoin = value;
                this.Cursor = value ? Cursors.Arrow : Cursors.Cross;
                NotifyPropertyChanged();
            }
        }

        private PortJoinAlign _joinAlignType;
        /// <summary>
        /// 连线点对齐方式
        /// </summary>
        public PortJoinAlign JoinAlignType
        {
            get => this._joinAlignType;
            set { this._joinAlignType = value; NotifyPropertyChanged(); }
        }


        internal PortDirection Dir { get; set; }
        internal ShapeBase Shape { get; set; }

        public Port()
        {
            this.Cursor = Cursors.Cross;
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
