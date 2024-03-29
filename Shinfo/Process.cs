using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using MaterialDesignThemes.Wpf;

namespace Shinfo
{
    internal static class Process
    {
        internal static PackIconKind IndexOfKindFromString(string kind)
        {
            PackIconKind result;
            if (Enum.TryParse(kind, out result))
                return result;
            else
                return PackIconKind.QuestionMark;
        }
        internal static void ShowErrorMessageBox(string errorMessage, string message)
        {
            MessageBox.Show(errorMessage + "\n" + message, "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
        }

    }
}
