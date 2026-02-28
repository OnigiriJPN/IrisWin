namespace IrisWin
{
    public static class Class1
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool AdjustWindowRectEx(ref RECT lpRect, uint dwStyle, bool bMenu, uint dwExStyle);

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(nint hwnd, int attr, ref int attrValue, int attrSize);

        [DllImport("user32.dll")]
        private static extern int GetWindowLongW(nint hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLongW(nint hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        private static extern bool SetLayeredWindowAttributes(nint hwnd, uint crKey, byte bAlpha, uint dwFlags);

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left, Top, Right, Bottom;
        }

        private const int DWMWA_CAPTION_COLOR = 35;
        private const int DWMWA_TEXT_COLOR = 36;
        private const int DWMWA_SYSTEMBACKDROP_TYPE = 38;
        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_LAYERED = 0x80000;
        private const uint LWA_ALPHA = 0x2;
        private const uint WS_OVERLAPPEDWINDOW = 0x00CF0000;
#endregion

        public enum BackdropType : int
        {
            None = 1,
            Mica = 2,    // Win11標準の控えめな透け
            Acrylic = 3, // はっきりした半透明
            Tabbed = 4   // タブUI向け
        }

        /// <summary>
        /// [Lens] クライアント領域を基準に、タイトルバー等を含めた正確な外寸を計算します。
        /// </summary>
        public static (int Width, int Height) GetAdjustedSize(int clientWidth, int clientHeight)
        {
            var rect = new RECT { Left = 0, Top = 0, Right = clientWidth, Bottom = clientHeight };
            AdjustWindowRectEx(ref rect, WS_OVERLAPPEDWINDOW, false, 0);
            return (rect.Right - rect.Left, rect.Bottom - rect.Top);
        }

        /// <summary>
        /// [Chroma] ネイティブのタイトルバーに「虹色（カスタムカラー）」を適用します。
        /// </summary>
        public static void SetTheme(nint hWnd, Color bgColor, Color textColor)
        {
            int bg = (bgColor.R) | (bgColor.G << 8) | (bgColor.B << 16);
            int text = (textColor.R) | (textColor.G << 8) | (textColor.B << 16);

            DwmSetWindowAttribute(hWnd, DWMWA_CAPTION_COLOR, ref bg, sizeof(int));
            DwmSetWindowAttribute(hWnd, DWMWA_TEXT_COLOR, ref text, sizeof(int));
        }

        /// <summary>
        /// [Iris] 背景の透過エフェクト(Mica/Acrylic)と不透明度を制御します。
        /// </summary>
        public static void SetVisualEffect(nint hWnd, BackdropType backdrop, byte opacity = 255)
        {
            // 背景エフェクト
            int type = (int)backdrop;
            DwmSetWindowAttribute(hWnd, DWMWA_SYSTEMBACKDROP_TYPE, ref type, sizeof(int));

            // 不透明度 (Layered Window)
            if (opacity < 255)
            {
                int exStyle = GetWindowLongW(hWnd, GWL_EXSTYLE);
                SetWindowLongW(hWnd, GWL_EXSTYLE, exStyle | WS_EX_LAYERED);
                SetLayeredWindowAttributes(hWnd, 0, opacity, LWA_ALPHA);
            }
        }
    }
}
