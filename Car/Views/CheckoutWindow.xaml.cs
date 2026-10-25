using System.Windows;
using Car.Database;
using Car.Models;

namespace Car.Views
{
    public partial class CheckoutWindow : Window
    {
        private readonly Car.Models.Car _car;
        private readonly User _user;

        public CheckoutWindow(Car.Models.Car car, User user)
        {
            InitializeComponent();
            _car = car;
            _user = user;

            TxtCarName.Text = $"Mercedes-Benz {car.Name}";
            TxtCarDetails.Text = $"{car.Model}  •  {car.Year}  •  {car.Color}";
            TxtCarPrice.Text = car.PriceFormatted;
        }

        private void BtnConfirm_Click(object sender, RoutedEventArgs e)
        {
            TxtError.Visibility = Visibility.Collapsed;
            BtnConfirm.IsEnabled = false;

            try
            {
                // 1 — Save order to database
                int orderId = CarRepository.PlaceOrder(
                    _user.UserId,
                    _car.CarId,
                    _car.Price,
                    "Cash on Delivery");

                // 2 — Load the saved order (needed for receipt data)
                Order? order = OrderRepository.GetById(orderId);

                if (order == null)
                {
                    MessageBox.Show(
                        "Order saved but receipt data could not be loaded.",
                        "Warning", MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    this.Close();
                    return;
                }

                // 3 — Generate PDF and save path to DB automatically
                string pdfPath = ReceiptGenerator.GenerateAndSave(
                    order,
                    _user.Email,
                    _user.PhoneNumber);

                // 4 — Tell the user and open the PDF
                MessageBox.Show(
                    $"Purchase confirmed!\n\n" +
                    $"Receipt No: {order.ReceiptNumber}\n" +
                    $"PDF saved to:\n{pdfPath}",
                    "Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                ReceiptGenerator.OpenReceipt(pdfPath);
                this.Close();
            }
            catch (Exception ex)
            {
                TxtError.Text = "Error: " + ex.Message;
                TxtError.Visibility = Visibility.Visible;
                BtnConfirm.IsEnabled = true;
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}

