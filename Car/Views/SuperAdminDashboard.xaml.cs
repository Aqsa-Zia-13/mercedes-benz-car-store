using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Car.Database;
using Car.Models;

namespace Car.Views
{
    public partial class SuperAdminDashboard : Window
    {
        private readonly User _user;

        public SuperAdminDashboard(User user)
        {
            InitializeComponent();
            _user = user;
            TxtWelcome.Text = $"Welcome,\n{user.FullName}";
            LoadAllCars();
        }

        private void NavBtn_Click(object sender, RoutedEventArgs e)
        {
            string tag = (sender as Button)?.Tag?.ToString() ?? "";
            PanelAllCars.Visibility = Visibility.Collapsed;
            PanelFlagged.Visibility = Visibility.Collapsed;
            PanelUsers.Visibility = Visibility.Collapsed;
            switch (tag)
            {
                case "AllCars":
                    PanelAllCars.Visibility = Visibility.Visible;
                    LoadAllCars(); break;
                case "Flagged":
                    PanelFlagged.Visibility = Visibility.Visible;
                    LoadFlagged(); break;
                case "Users":
                    PanelUsers.Visibility = Visibility.Visible;
                    LoadUsers(); break;
            }
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            new MainWindow().Show();
            this.Close();
        }

        // ── ALL CARS ─────────────────────────────────────────────
        private void LoadAllCars()
        {
            AllCarsPanel.Children.Clear();
            var cars = CarInventoryRepository.GetAllCars();
            foreach (var car in cars)
                AllCarsPanel.Children.Add(BuildAllCarRow(car));
        }

        private Border BuildAllCarRow(Car.Models.Car car)
        {
            var border = new Border
            {
                Background = Brushes.White,
                CornerRadius = new CornerRadius(8),
                BorderBrush = new SolidColorBrush(Color.FromRgb(216, 228, 240)),
                BorderThickness = new Thickness(1),
                Margin = new Thickness(0, 0, 0, 12),
                Padding = new Thickness(16, 12, 16, 12)
            };

            var mainStack = new StackPanel();

            // Info row
            var topGrid = new Grid();
            topGrid.ColumnDefinitions.Add(new ColumnDefinition
            { Width = new GridLength(1, GridUnitType.Star) });
            topGrid.ColumnDefinitions.Add(new ColumnDefinition
            { Width = GridLength.Auto });

            var info = new StackPanel();
            info.Children.Add(new TextBlock
            {
                Text = $"Mercedes-Benz {car.Name} — {car.Model} ({car.Year})",
                FontSize = 13,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(27, 27, 47))
            });
            info.Children.Add(new TextBlock
            {
                Text = $"{car.Color}  •  {car.FuelType}  •  {car.PriceFormatted}",
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.FromRgb(107, 140, 174)),
                Margin = new Thickness(0, 3, 0, 0)
            });

            var badgeBg = car.CarStatus == "Available"
                ? Color.FromRgb(240, 255, 244)
                : car.CarStatus == "Needs Repair"
                    ? Color.FromRgb(255, 240, 240)
                    : Color.FromRgb(255, 248, 220);
            var badgeFg = car.CarStatus == "Available"
                ? Color.FromRgb(30, 122, 60)
                : car.CarStatus == "Needs Repair"
                    ? Color.FromRgb(192, 57, 43)
                    : Color.FromRgb(160, 120, 0);

            var badge = new Border
            {
                Background = new SolidColorBrush(badgeBg),
                BorderBrush = new SolidColorBrush(badgeFg),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(10, 4, 10, 4),
                VerticalAlignment = VerticalAlignment.Top,
                Child = new TextBlock
                {
                    Text = car.StatusBadge,
                    FontSize = 11,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = new SolidColorBrush(badgeFg)
                }
            };

            Grid.SetColumn(info, 0);
            Grid.SetColumn(badge, 1);
            topGrid.Children.Add(info);
            topGrid.Children.Add(badge);
            mainStack.Children.Add(topGrid);

            // Stock editor row
            var stockRow = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 10, 0, 0)
            };

            stockRow.Children.Add(new TextBlock
            {
                Text = "Current Stock:",
                FontSize = 12,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(60, 80, 100)),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 0)
            });

            var stockBorder = new Border
            {
                BorderBrush = new SolidColorBrush(Color.FromRgb(176, 200, 224)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(6),
                Margin = new Thickness(0, 0, 8, 0)
            };
            var stockTxt = new TextBox
            {
                Text = car.StockCount.ToString(),
                Background = new SolidColorBrush(Color.FromRgb(240, 246, 255)),
                Foreground = new SolidColorBrush(Color.FromRgb(28, 43, 74)),
                CaretBrush = new SolidColorBrush(Color.FromRgb(28, 43, 74)),
                BorderThickness = new Thickness(0),
                FontSize = 13,
                Padding = new Thickness(10, 6, 10, 6),
                Width = 80
            };
            stockBorder.Child = stockTxt;
            stockRow.Children.Add(stockBorder);

            var btnUpdateStock = new Button
            {
                Content = "✔ Update Stock",
                FontSize = 12,
                FontWeight = FontWeights.SemiBold,
                Foreground = Brushes.White,
                Background = new SolidColorBrush(Color.FromRgb(27, 27, 47)),
                BorderThickness = new Thickness(0),
                Height = 34,
                Padding = new Thickness(14, 0, 14, 0),
                Cursor = System.Windows.Input.Cursors.Hand,
                Margin = new Thickness(0, 0, 10, 0)
            };
            btnUpdateStock.Click += (_, _) =>
            {
                if (!int.TryParse(stockTxt.Text, out int newStock) || newStock < 0)
                {
                    MessageBox.Show("Enter a valid stock number (0 or more).",
                        "Validation", MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }
                CarInventoryRepository.UpdateStock(car.CarId, newStock, _user.UserId);
                MessageBox.Show($"Stock updated to {newStock}.",
                    "Updated", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadAllCars();
            };
            stockRow.Children.Add(btnUpdateStock);

            mainStack.Children.Add(stockRow);

            if (!string.IsNullOrEmpty(car.StatusNote))
                mainStack.Children.Add(new TextBlock
                {
                    Text = "⚠ Admin Note: " + car.StatusNote,
                    FontSize = 11,
                    FontStyle = FontStyles.Italic,
                    Foreground = Brushes.OrangeRed,
                    Margin = new Thickness(0, 6, 0, 0)
                });

            border.Child = mainStack;
            return border;
        }

        // ── FLAGGED CARS ─────────────────────────────────────────
        private void LoadFlagged()
        {
            FlaggedPanel.Children.Clear();
            var cars = CarInventoryRepository.GetFlaggedCars();

            if (cars.Count == 0)
            {
                FlaggedPanel.Children.Add(new TextBlock
                {
                    Text = "✅ No cars are currently flagged. All good!",
                    FontSize = 14,
                    Foreground = Brushes.SeaGreen
                });
                return;
            }

            foreach (var car in cars)
                FlaggedPanel.Children.Add(BuildFlaggedRow(car));
        }

        private Border BuildFlaggedRow(Car.Models.Car car)
        {
            bool isRepair = car.CarStatus == "Needs Repair";

            var border = new Border
            {
                Background = new SolidColorBrush(
                    isRepair ? Color.FromRgb(255, 247, 247)
                             : Color.FromRgb(255, 252, 235)),
                CornerRadius = new CornerRadius(8),
                BorderBrush = new SolidColorBrush(
                    isRepair ? Color.FromRgb(255, 180, 180)
                             : Color.FromRgb(240, 210, 100)),
                BorderThickness = new Thickness(1),
                Margin = new Thickness(0, 0, 0, 12),
                Padding = new Thickness(16, 14, 16, 14)
            };

            var sp = new StackPanel();
            sp.Children.Add(new TextBlock
            {
                Text = $"{car.StatusBadge} — Mercedes-Benz {car.Name} {car.Model} ({car.Year})",
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(
                    isRepair ? Color.FromRgb(192, 57, 43)
                             : Color.FromRgb(150, 100, 0))
            });
            sp.Children.Add(new TextBlock
            {
                Text = $"{car.Color}  •  {car.FuelType}  •  Stock: {car.StockCount}",
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.FromRgb(107, 140, 174)),
                Margin = new Thickness(0, 3, 0, 0)
            });
            if (!string.IsNullOrEmpty(car.StatusNote))
                sp.Children.Add(new TextBlock
                {
                    Text = "Admin Note: " + car.StatusNote,
                    FontSize = 12,
                    FontStyle = FontStyles.Italic,
                    Foreground = new SolidColorBrush(Color.FromRgb(100, 60, 0)),
                    Margin = new Thickness(0, 6, 0, 0)
                });

            var btnRow = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 10, 0, 0)
            };

            // Stock update
            var stockBorder = new Border
            {
                BorderBrush = new SolidColorBrush(Color.FromRgb(176, 200, 224)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(6),
                Margin = new Thickness(0, 0, 8, 0)
            };
            var stockTxt = new TextBox
            {
                Text = car.StockCount.ToString(),
                Background = new SolidColorBrush(Color.FromRgb(240, 246, 255)),
                Foreground = new SolidColorBrush(Color.FromRgb(28, 43, 74)),
                CaretBrush = new SolidColorBrush(Color.FromRgb(28, 43, 74)),
                BorderThickness = new Thickness(0),
                FontSize = 13,
                Padding = new Thickness(10, 6, 10, 6),
                Width = 80
            };
            stockBorder.Child = stockTxt;
            btnRow.Children.Add(stockBorder);

            var btnRestock = new Button
            {
                Content = "📦 Update Stock",
                FontSize = 12,
                FontWeight = FontWeights.SemiBold,
                Foreground = Brushes.White,
                Background = new SolidColorBrush(Color.FromRgb(46, 109, 180)),
                BorderThickness = new Thickness(0),
                Height = 34,
                Padding = new Thickness(14, 0, 14, 0),
                Cursor = System.Windows.Input.Cursors.Hand,
                Margin = new Thickness(0, 0, 10, 0)
            };
            btnRestock.Click += (_, _) =>
            {
                if (!int.TryParse(stockTxt.Text, out int s) || s < 0)
                {
                    MessageBox.Show("Enter a valid stock number.",
                        "Validation", MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }
                CarInventoryRepository.UpdateStock(car.CarId, s, _user.UserId);
                MessageBox.Show("Stock updated.", "Done",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                LoadFlagged();
            };

            var btnResolve = new Button
            {
                Content = "✅ Mark as Resolved",
                FontSize = 12,
                FontWeight = FontWeights.SemiBold,
                Foreground = Brushes.White,
                Background = new SolidColorBrush(Color.FromRgb(30, 122, 60)),
                BorderThickness = new Thickness(0),
                Height = 34,
                Padding = new Thickness(14, 0, 14, 0),
                Cursor = System.Windows.Input.Cursors.Hand
            };
            btnResolve.Click += (_, _) =>
            {
                CarInventoryRepository.ResolveFlaggedCar(car.CarId, _user.UserId);
                MessageBox.Show("Car marked as resolved and available.",
                    "Resolved", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadFlagged();
            };

            btnRow.Children.Add(btnRestock);
            btnRow.Children.Add(btnResolve);
            sp.Children.Add(btnRow);
            border.Child = sp;
            return border;
        }

        // ── USERS ─────────────────────────────────────────────────
        private void LoadUsers()
        {
            UsersPanel.Children.Clear();
            var users = SuperAdminRepository.GetAllUsers();
            foreach (var u in users)
                UsersPanel.Children.Add(BuildUserRow(u));
        }

        private static Border BuildUserRow(User u)
        {
            var roleColor = u.UserRole switch
            {
                "SuperAdmin" => Color.FromRgb(100, 50, 150),
                "Admin" => Color.FromRgb(22, 33, 62),
                "Manager" => Color.FromRgb(15, 52, 96),
                _ => Color.FromRgb(46, 109, 180)
            };

            var border = new Border
            {
                Background = Brushes.White,
                CornerRadius = new CornerRadius(8),
                BorderBrush = new SolidColorBrush(Color.FromRgb(216, 228, 240)),
                BorderThickness = new Thickness(1),
                Margin = new Thickness(0, 0, 0, 10),
                Padding = new Thickness(16, 12, 16, 12)
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition
            { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition
            { Width = GridLength.Auto });

            var info = new StackPanel();
            info.Children.Add(new TextBlock
            {
                Text = u.FullName,
                FontSize = 13,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(27, 27, 47))
            });
            info.Children.Add(new TextBlock
            {
                Text = $"@{u.Username}  •  {u.Email}  •  {u.PhoneNumber}",
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.FromRgb(107, 140, 174)),
                Margin = new Thickness(0, 3, 0, 0)
            });

            var roleBadge = new Border
            {
                Background = new SolidColorBrush(roleColor),
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(10, 4, 10, 4),
                VerticalAlignment = VerticalAlignment.Center,
                Child = new TextBlock
                {
                    Text = u.UserRole,
                    FontSize = 11,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.White
                }
            };

            Grid.SetColumn(info, 0);
            Grid.SetColumn(roleBadge, 1);
            grid.Children.Add(info);
            grid.Children.Add(roleBadge);
            border.Child = grid;
            return border;
        }
    }
}