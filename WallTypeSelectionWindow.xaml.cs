using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace BIMoQ
{
    public partial class WallTypeSelectionWindow : Window
    {
        public string SelectedWallType { get; private set; }

        public WallTypeSelectionWindow(IEnumerable<string> wallTypes)
        {
            InitializeComponent();
            WallTypeComboBox.ItemsSource = wallTypes;
            if (wallTypes.Any())
                WallTypeComboBox.SelectedIndex = 0;
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedWallType = WallTypeComboBox.SelectedItem as string;
            DialogResult = true;
            Close();
        }
    }
}
