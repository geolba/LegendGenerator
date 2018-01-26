using System;
using System.Windows.Data;

namespace LegendGenerator.App.Utils
{
    public class CustomStringToColorConverter : IMultiValueConverter
    {
        /// <summary>
        /// Converts to a RGB Color
        /// </summary>
        /// <param name="values">arg[0] = cyan, arg[1] = magenta, arg[2] = yellow, arg[3] = schwarzanteil</param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            //string entry = (string)value;

            string cyanString = values[0] as string;
            string magentaString = values[1] as string;
            string yellowString = values[2] as string;
            string blackString = values[3] as string;

            double cyan, magenta, yellow, black;
            bool isCyanNum = double.TryParse(cyanString, out cyan);
            bool isMagentaNum = double.TryParse(magentaString, out magenta);
            bool isYellowNum = double.TryParse(yellowString, out yellow);
            bool isBlackNum = double.TryParse(blackString, out black);

            if (isCyanNum && IsWithin(cyan, 0, 100) && isMagentaNum && IsWithin(magenta, 0, 100) &&
                isYellowNum && IsWithin(yellow, 0, 100) && isBlackNum && IsWithin(black, 0, 100))
            {
                System.Windows.Media.Color rgbColor = CMYK2RGB(cyan, magenta, yellow, black);
                return rgbColor;
            }

            //return Binding.DoNothing;
            return System.Windows.Media.Colors.White;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException("Cannot convert back");
        }

        private bool IsWithin(double value, int minimum, int maximum)
        {
            return value >= minimum && value <= maximum;
        }
        
        private System.Windows.Media.Color CMYK2RGB(double c, double m, double y, double k)
        {
            byte r, g, b;

            double R, G, B;
            double C, M, Y, K;

            C = c;
            M = m;
            Y = y;
            K = k;

            C = C / 255.0;
            M = M / 255.0;
            Y = Y / 255.0;
            K = K / 255.0;

            R = C * (1.0 - K) + K;
            G = M * (1.0 - K) + K;
            B = Y * (1.0 - K) + K;

            R = (1.0 - R) * 255.0 + 0.5;
            G = (1.0 - G) * 255.0 + 0.5;
            B = (1.0 - B) * 255.0 + 0.5;

            r = (byte)R;
            g = (byte)G;
            b = (byte)B;

            System.Windows.Media.Color rgbColor = System.Windows.Media.Color.FromRgb(r, g, b);
            return rgbColor;

        }
    }
}
