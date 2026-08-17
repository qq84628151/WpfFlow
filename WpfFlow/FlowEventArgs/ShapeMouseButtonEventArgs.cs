//===========================================================================//
//qq：1018720141     qq群：1064754010                                        //
//===========================================================================//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace WpfFlow.FlowEventArgs
{
    public class ShapeMouseButtonEventArgs : EventArgs
    {
        public MouseButtonState ButtonState { get; set; }
        public Point Position { get; set; }

        public MouseButtonState LeftButton {  get; set; }

        public MouseButtonState RightButton { get; set; }

        public MouseButtonState MiddleButton { get; set; }

        public ShapeMouseButtonEventArgs(MouseButtonState buttonState, Point position, MouseButtonState leftButton, MouseButtonState rightButton, MouseButtonState middleButton)
        {
            ButtonState = buttonState;
            Position = position;
            LeftButton = leftButton;
            RightButton = rightButton;
            MiddleButton = middleButton;
        }
    }
}
