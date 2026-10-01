using System.Windows;

namespace Baraban;

public partial class CookieImportWindow : Window
{
    public CookieImportWindow() => InitializeComponent();

    public string Domain => DomainText.Text;
    public string CookieHeader => CookieText.Text;

    private void Import_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }
}
