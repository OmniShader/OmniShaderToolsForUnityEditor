//  Copyright (c) 2025-present amlovey
//  

using UnityEditor;

namespace OmniShader.Common.UI
{
    public class Theme
    {
        private static ThemeData _current;
        public static ThemeData Current
        {
            get
            {
                if (_current == null)
                {
                    DetectTheme();
                }

                return _current;
            }
        }

        public static void DetectTheme()
        {
            bool isPro = EditorGUIUtility.isProSkin;
            _current = isPro ? Pro : Personal;
        }

        public static ThemeData Personal = new ThemeData() 
        {
            Background = "#efefef",
            HoverBackground = "#efefef",
            ButtonBackground = "#e0e0e0",
            LineColor = "#a7a7a7ff",
            MenuPanelBackground = "#dfdfdf",
            ListBoxHeaderBackground = "#dfdfdf",
            MenuBackground = "#efefef",
            MenuSelectedBackgroud = "#4b89ff70",
            StatusBarBackround = "#d4d4d4",
            ForegroundColor = "#303030",
        };

        public static ThemeData Pro = new ThemeData()
        {
            Background = "#333333ff",
            HoverBackground = "#6b6b6bff",
            ButtonBackground = "#5a5a5aff",
            LineColor = "#606060ff",
            MenuPanelBackground = "#404040",
            ListBoxHeaderBackground = "#505050",
            MenuBackground = "#535353ff",
            MenuSelectedBackgroud = "#4b89ff70",
            StatusBarBackround = "#4f4f4f",
            ForegroundColor = "#cdcdcdff",
        };
    }

    public class ThemeData
    {
       public string Background;
       public string LineColor;
       public string MenuPanelBackground;
       public string MenuBackground;
       public string MenuSelectedBackgroud;
       public string ListBoxHeaderBackground;
       public string ButtonBackground;
       public string HoverBackground;
       public string StatusBarBackround;
       public string ForegroundColor;
    }
}