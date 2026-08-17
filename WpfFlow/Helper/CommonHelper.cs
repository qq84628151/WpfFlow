//===========================================================================//
//qq：1018720141     qq群：1064754010                                        //
//===========================================================================//
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;

namespace WpfFlow.Helper
{
    public static class CommonHelper
    {
        public static (Point arcStart, Point arcEnd, SweepDirection sweep) GetArcBy3Point(Point prev, Point current, Point next, double radius)
        {
            Vector v1 = prev - current;
            Vector v2 = next - current;
            double len1 = v1.Length;
            double len2 = v2.Length;
            if (len1 < 0.000001 || len2 < 0.000001)
            {
                return (current, current, SweepDirection.Counterclockwise);
            }
            v1.Normalize();
            v2.Normalize();
            double angleBetween = Vector.AngleBetween(v1, v2);
            double absAngleRad = Math.Abs(angleBetween) * Math.PI / 180.0;
            if (absAngleRad < 0.000001 || Math.Abs(absAngleRad - Math.PI) < 0.000001)
            {
                return (current, current, SweepDirection.Counterclockwise);
            }
            double tangentLength = radius / Math.Tan(absAngleRad / 2.0);
            double maxTangent = Math.Min(len1 / 2.0, len2 / 2.0);
            if (tangentLength > maxTangent)
            {
                tangentLength = maxTangent;
                radius = tangentLength * Math.Tan(absAngleRad / 2.0);
            }
            Point arcStart = current + v1 * tangentLength;
            Point arcEnd = current + v2 * tangentLength;
            SweepDirection sweep = angleBetween > 0 ? SweepDirection.Counterclockwise : SweepDirection.Clockwise;
            return (arcStart, arcEnd, sweep);
        }

        public static Binding CreateBinding(string path, object source, BindingMode mode = BindingMode.OneWay)
        {
            return new Binding(path) { Source = source, Mode = mode };
        }

        public static Vector GetAngleByDir(RectShape rect, Point portCenter)
        {
            double centerX = rect.Position.X + rect.Size.Width / 2.0;
            double centerY = rect.Position.Y + rect.Size.Height / 2.0;

            double radians = rect.Angle * Math.PI / 180.0;

            double cos = Math.Abs(Math.Cos(radians));
            double sin = Math.Abs(Math.Sin(radians));

            double aabbWidth = rect.Size.Width * cos + rect.Size.Height * sin;
            double aabbHeight = rect.Size.Width * sin + rect.Size.Height * cos;

            double minX = centerX - aabbWidth / 2.0;
            double minY = centerY - aabbHeight / 2.0;


            Rect rect_test = new Rect(minX, minY, aabbWidth, aabbHeight);

            var rectCenter = new Point(rect_test.X + rect_test.Width / 2, rect_test.Y + rect_test.Height / 2);
            var portAngle = Vector.AngleBetween(new Vector(1, 0), portCenter - rectCenter);
            var topLeftAngle = Vector.AngleBetween(new Vector(1, 0), rect_test.TopLeft - rectCenter);
            var topRightAngle = Vector.AngleBetween(new Vector(1, 0), rect_test.TopRight - rectCenter);
            var bottomLeftAngle = Vector.AngleBetween(new Vector(1, 0), rect_test.BottomLeft - rectCenter);
            var bottomRightAngle = Vector.AngleBetween(new Vector(1, 0), rect_test.BottomRight - rectCenter);

            if (portAngle >= topLeftAngle && portAngle <= topRightAngle)
            {
                return new Vector(0, -1);
            }
            else if (portAngle >= topRightAngle && portAngle <= bottomRightAngle)
            {
                return new Vector(1, 0);
            }
            else if (portAngle >= bottomRightAngle && portAngle <= bottomLeftAngle)
            {
                return new Vector(0, 1);
            }
            else
            {
                return new Vector(-1, 0);
            }
        }
        public static Cursor GetCursorByPosition(Point rectCenter, Point handleCenter)
        {
            double dx = handleCenter.X - rectCenter.X;
            double dy = handleCenter.Y - rectCenter.Y;

            double angle = Math.Atan2(dy, dx) * (180 / Math.PI);
            if (angle < 0) angle += 360;

            if ((angle >= 0 && angle < 90) || (angle >= 180 && angle < 270))
            {
                return Cursors.SizeNWSE;
            }
            return Cursors.SizeNESW;
        }
    }
}
