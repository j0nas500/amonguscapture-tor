using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using AmongUsCapture;

namespace AUCapture_WPF.Converters
{
    public class PlayerColorToBrush : IValueConverter
    {
        private static readonly Dictionary<PlayerColor, SolidColorBrush> BrushMapping = new() {
            { PlayerColor.Red,     new SolidColorBrush(Color.FromRgb(197, 17, 17))},
            { PlayerColor.Blue,    new SolidColorBrush(Color.FromRgb(19, 46, 209))},
            { PlayerColor.Green,   new SolidColorBrush(Color.FromRgb(17, 127, 45))},
            { PlayerColor.Pink,    new SolidColorBrush(Color.FromRgb(237, 84, 186))},
            { PlayerColor.Orange,  new SolidColorBrush(Color.FromRgb(239, 125, 13))},
            { PlayerColor.Yellow,  new SolidColorBrush(Color.FromRgb(245, 245, 87))},
            { PlayerColor.Black,   new SolidColorBrush(Color.FromRgb(63, 71, 78))},
            { PlayerColor.White,   new SolidColorBrush(Color.FromRgb(214, 224, 240))},
            { PlayerColor.Purple,  new SolidColorBrush(Color.FromRgb(107, 47, 187))},
            { PlayerColor.Brown,   new SolidColorBrush(Color.FromRgb(113, 73, 30))},
            { PlayerColor.Cyan,    new SolidColorBrush(Color.FromRgb(56, 254, 220))},
            { PlayerColor.Lime,    new SolidColorBrush(Color.FromRgb(80, 239, 57))},
            { PlayerColor.Maroon,  new SolidColorBrush(Color.FromRgb(95, 29, 46))},
            { PlayerColor.Rose,    new SolidColorBrush(Color.FromRgb(236, 192, 211))},
            { PlayerColor.Banana,  new SolidColorBrush(Color.FromRgb(240, 231, 168))},
            { PlayerColor.Gray,    new SolidColorBrush(Color.FromRgb(117, 133, 147))},
            { PlayerColor.Tan,     new SolidColorBrush(Color.FromRgb(145, 136, 119))},
            { PlayerColor.Coral,  new SolidColorBrush(Color.FromRgb(215, 100, 100))},
            { PlayerColor.Salmon, new SolidColorBrush(Color.FromRgb(239, 191, 192))},
            { PlayerColor.Bordeaux, new SolidColorBrush(Color.FromRgb(109, 7, 26)) },
            { PlayerColor.Olive, new SolidColorBrush(Color.FromRgb(154, 140, 61)) },
            { PlayerColor.Turqoise, new SolidColorBrush(Color.FromRgb(22, 132, 176)) },
            { PlayerColor.Mint, new SolidColorBrush(Color.FromRgb(111, 192, 156)) },
            { PlayerColor.Lavender, new SolidColorBrush(Color.FromRgb(173, 126, 201)) },
            { PlayerColor.Nougat, new SolidColorBrush(Color.FromRgb(160, 101, 56)) },
            { PlayerColor.Peach, new SolidColorBrush(Color.FromRgb(255, 164, 119)) },
            { PlayerColor.Wasabi, new SolidColorBrush(Color.FromRgb(112, 143, 46)) },
            { PlayerColor.HotPink, new SolidColorBrush(Color.FromRgb(255, 51, 102)) },
            { PlayerColor.Petrol, new SolidColorBrush(Color.FromRgb(0, 99, 105)) },
            { PlayerColor.Lemon, new SolidColorBrush(Color.FromRgb(0xDB, 0xFD, 0x2F)) },
			{ PlayerColor.SignalOrange, new SolidColorBrush(Color.FromRgb(0xF7, 0x44, 0x17)) },
            { PlayerColor.Teal, new SolidColorBrush(Color.FromRgb(0x25, 0xB8, 0xBF)) },
			{ PlayerColor.Blurple, new SolidColorBrush(Color.FromRgb(0x59, 0x3C, 0xD6)) },
			{ PlayerColor.Sunrise, new SolidColorBrush(Color.FromRgb(0xFF, 0xCA, 0x19)) },
			{ PlayerColor.Ice, new SolidColorBrush(Color.FromRgb(0xA8, 0xDF, 0xFF)) }  
        };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
             var color = value as PlayerColor? ?? PlayerColor.Red;
             return BrushMapping[color];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class PlayerColorToBrushShaded : IValueConverter
    {
        public static Color shadeColor(Color inColor, float percent) {
            
            float R = (inColor.R * (100 + percent)) / 100;
            float G = (inColor.G * (100 + percent)) / 100;
            float B = (inColor.B * (100 + percent)) / 100;
            R = R < 255 ? R : 255;
            G = G < 255 ? G : 255;
            B = B < 255 ? B : 255;
            return Color.FromArgb(255, (byte) R, (byte) G, (byte) B);
        }

        private static readonly Dictionary<PlayerColor, SolidColorBrush> BrushMapping = new() {
            { PlayerColor.Red,     new SolidColorBrush(Color.FromRgb(197, 17, 17))},
            { PlayerColor.Blue,    new SolidColorBrush(Color.FromRgb(19, 46, 209))},
            { PlayerColor.Green,   new SolidColorBrush(Color.FromRgb(17, 127, 45))},
            { PlayerColor.Pink,    new SolidColorBrush(Color.FromRgb(237, 84, 186))},
            { PlayerColor.Orange,  new SolidColorBrush(Color.FromRgb(239, 125, 13))},
            { PlayerColor.Yellow,  new SolidColorBrush(Color.FromRgb(245, 245, 87))},
            { PlayerColor.Black,   new SolidColorBrush(Color.FromRgb(63, 71, 78))},
            { PlayerColor.White,   new SolidColorBrush(Color.FromRgb(214, 224, 240))},
            { PlayerColor.Purple,  new SolidColorBrush(Color.FromRgb(107, 47, 187))},
            { PlayerColor.Brown,   new SolidColorBrush(Color.FromRgb(113, 73, 30))},
            { PlayerColor.Cyan,    new SolidColorBrush(Color.FromRgb(56, 254, 220))},
            { PlayerColor.Lime,    new SolidColorBrush(Color.FromRgb(80, 239, 57))},
            { PlayerColor.Maroon,  new SolidColorBrush(Color.FromRgb(95, 29, 46))},
            { PlayerColor.Rose,    new SolidColorBrush(Color.FromRgb(236, 192, 211))},
            { PlayerColor.Banana,  new SolidColorBrush(Color.FromRgb(240, 231, 168))},
            { PlayerColor.Gray,    new SolidColorBrush(Color.FromRgb(117, 133, 147))},
            { PlayerColor.Tan,     new SolidColorBrush(Color.FromRgb(145, 136, 119))},
            { PlayerColor.Coral,  new SolidColorBrush(Color.FromRgb(215, 100, 100))},
            { PlayerColor.Salmon, new SolidColorBrush(Color.FromRgb(182, 119, 114)) },
            { PlayerColor.Bordeaux, new SolidColorBrush(Color.FromRgb(54, 2, 11)) },
            { PlayerColor.Olive, new SolidColorBrush(Color.FromRgb(104, 95, 40)) },
            { PlayerColor.Turqoise, new SolidColorBrush(Color.FromRgb(15, 89, 117)) },
            { PlayerColor.Mint, new SolidColorBrush(Color.FromRgb(65, 148, 111)) },
            { PlayerColor.Lavender, new SolidColorBrush(Color.FromRgb(131, 58, 203)) },
            { PlayerColor.Nougat, new SolidColorBrush(Color.FromRgb(115, 15, 78)) },
            { PlayerColor.Peach, new SolidColorBrush(Color.FromRgb(238, 128, 100)) },
            { PlayerColor.Wasabi, new SolidColorBrush(Color.FromRgb(72, 92, 29)) },
            { PlayerColor.HotPink, new SolidColorBrush(Color.FromRgb(232, 0, 58)) },
            { PlayerColor.Petrol, new SolidColorBrush(Color.FromRgb(0, 61, 54)) },
			{ PlayerColor.Lemon, new SolidColorBrush(Color.FromRgb(0x74, 0xE5, 0x10)) },
			{ PlayerColor.SignalOrange, new SolidColorBrush(Color.FromRgb(0x9B, 0x2E, 0x0F)) },
			{ PlayerColor.Teal, new SolidColorBrush(Color.FromRgb(0x12, 0x89, 0x86)) },
			{ PlayerColor.Blurple, new SolidColorBrush(Color.FromRgb(0x29, 0x17, 0x96)) },
            { PlayerColor.Sunrise, new SolidColorBrush(Color.FromRgb(0xDB, 0x44, 0x42)) },
            { PlayerColor.Ice, new SolidColorBrush(Color.FromRgb(0x59, 0x9F, 0xC8)) }
        };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var color = value as PlayerColor? ?? PlayerColor.Red;
            var mainColor = BrushMapping[color];
            var shaded = shadeColor(mainColor.Color, -20f);
            return new SolidColorBrush(shaded);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
