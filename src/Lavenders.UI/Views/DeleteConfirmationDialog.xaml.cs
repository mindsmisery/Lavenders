using System.Windows;

namespace Lavenders.UI;

public partial class DeleteConfirmationDialog : Window
{
    public DeleteConfirmationDialog()
    {
        InitializeComponent();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;

    private void Delete_Click(object sender, RoutedEventArgs e) => DialogResult = true;
}
