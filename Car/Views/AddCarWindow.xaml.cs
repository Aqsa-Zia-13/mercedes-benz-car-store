using System.Windows;
using Car.Database;

namespace Car.Views
{
    public partial class AddCarWindow : Window
    {
        public AddCarWindow() => InitializeComponent();

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            ErrBorder.Visibility = Visibility.Collapsed;

            string name = TxtName.Text.Trim();
            string model = TxtModel.Text.Trim();
            string color = TxtColor.Text.Trim();
            string desc = TxtDesc.Text.Trim();
            string fuel = (CmbFuel.SelectedItem as System.Windows.Controls.ComboBoxItem)
                           ?.Content?.ToString() ?? "Petrol";

            if (string.IsNullOrWhiteSpace(name))
            { ShowErr("Car name is required."); return; }
            if (string.IsNullOrWhiteSpace(model))
            { ShowErr("Model is required."); return; }
            if (!int.TryParse(TxtYear.Text, out int year)
                || year < 2000 || year > 2030)
            { ShowErr("Enter a valid year (2000–2030)."); return; }
            if (!decimal.TryParse(TxtPrice.Text, out decimal price) || price <= 0)
            { ShowErr("Enter a valid price greater than 0."); return; }
            if (string.IsNullOrWhiteSpace(color))
            { ShowErr("Color is required."); return; }
            if (!int.TryParse(TxtMileage.Text, out int mileage) || mileage < 0)
            { ShowErr("Enter a valid mileage (0 or more)."); return; }
            if (!int.TryParse(TxtStock.Text, out int stock) || stock < 1)
            { ShowErr("Stock must be at least 1."); return; }

            CarInventoryRepository.AddCar(new Car.Models.Car
            {
                Name = name,
                Model = model,
                Year = year,
                Price = price,
                Color = color,
                FuelType = fuel,
                Mileage = mileage,
                Description = desc,
                StockCount = stock
            });

            MessageBox.Show("Car added successfully!",
                "Done", MessageBoxButton.OK, MessageBoxImage.Information);
            this.Close();
        }

        private void ShowErr(string msg)
        {
            TxtErr.Text = msg;
            ErrBorder.Visibility = Visibility.Visible;
        }
    }
}