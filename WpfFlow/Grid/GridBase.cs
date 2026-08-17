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
using System.Windows.Controls;
using System.Windows.Media;
using WpfFlow.Interface;

namespace WpfFlow
{
    public abstract class GridBase : IGrid, INotifyPropertyChanged
    {
        internal const double DEFAULT_LENGTH = 20.0;
        internal const double DEFAULT_LENGTH_Ext = 20.5;
        internal const double DEFAULT_LENGTH_Ext2 = 80.5;
        internal const double DEFAULT_LENGTH_DOUBLE = 20;
        internal const double DEFAULT_LENGTH_FOUR = 80;

        private double _gridLength = QuareGrid.DEFAULT_LENGTH;
        /// <summary>
        /// 网格大小
        /// </summary>
        [DefaultValue(DEFAULT_LENGTH)]
        public double GridLength
        {
            get => this._gridLength;
            set { this._gridLength = value; UpdateLine(); NotifyPropertyChanged(); }
        }

        internal abstract void UpdateLine();
        internal abstract DrawingBrush GenerateDrawingBrush();


        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
