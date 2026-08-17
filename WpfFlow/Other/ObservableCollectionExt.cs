//===========================================================================//
//qq：1018720141     qq群：1064754010                                        //
//===========================================================================//
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfFlow.Other
{
    public class ObservableCollectionExt<T> : ObservableCollection<T>   
    {
        protected override void ClearItems()
        {
            if (this.Count == 0) return;
            var oldItems = new List<T>(this);
            this.Items.Clear();
            OnPropertyChanged(new PropertyChangedEventArgs("Count"));
            OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldItems, 0));
        }
    }
}
