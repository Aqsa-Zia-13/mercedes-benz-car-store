using System.IO;                          // Fix #2: added for File.Exists
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Car.Database;
using Car.Models;

namespace Car.Views
{
    public partial class CustomerDashboard : Window
    {
        private readonly User _user;

        public CustomerDashboard(User user)
        {
            InitializeComponent();
            _user = user;
            TxtWelcome.Text = $"Welcome,\n{user.FullName}";
            LoadBrowseCars();
        }

        // ── NAVIGATION ──────────────────────────────────────────
        private void NavBtn_Click(object sender, RoutedEventArgs e)
        {
            string tag = (sender as Button)?.Tag?.ToString() ?? "";
            PanelBrowse.Visibility = Visibility.Collapsed;
            PanelCart.Visibility = Visibility.Collapsed;
            PanelOrders.Visibility = Visibility.Collapsed;
            PanelQueries.Visibility = Visibility.Collapsed;

            switch (tag)
            {
                case "Browse":
                    PanelBrowse.Visibility = Visibility.Visible;
                    LoadBrowseCars();
                    break;
                case "Cart":
                    PanelCart.Visibility = Visibility.Visible;
                    LoadCart();
                    break;
                case "Orders":
                    PanelOrders.Visibility = Visibility.Visible;
                    LoadOrders();
                    break;
                case "Queries":
                    PanelQueries.Visibility = Visibility.Visible;
                    LoadQueries();
                    break;
            }
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            new MainWindow().Show();
            this.Close();
        }

        // ── BROWSE CARS ─────────────────────────────────────────
        private void LoadBrowseCars()
        {
            if (CarCardsPanel == null) return;          // Fix #0: null guard

            string search = TxtSearch?.Text.Trim() ?? "";
            string fuel = (CmbFuel?.SelectedIndex ?? 0) == 0
                                ? ""
                                : (CmbFuel.SelectedItem as ComboBoxItem)
                                  ?.Content?.ToString() ?? "";
            string sort = CmbSort?.SelectedIndex switch
            {
                1 => "Price ASC",
                2 => "Price DESC",
                3 => "Year DESC",
                _ => "Name"
            };

            var cars = CarRepository.GetAvailableCars(search, fuel, sort);
            CarCardsPanel.Children.Clear();

            if (cars.Count == 0)
            {
                CarCardsPanel.Children.Add(new TextBlock
                {
                    Text = "No cars found matching your search.",
                    FontSize = 14,
                    Foreground = Brushes.Gray,
                    Margin = new Thickness(20)
                });
                return;
            }

            foreach (var car in cars)
                CarCardsPanel.Children.Add(BuildCarCard(car));
        }

        private Border BuildCarCard(Car.Models.Car car)
        {
            var card = new Border
            {
                Width = 260,
                Margin = new Thickness(10),
                Background = Brushes.White,
                CornerRadius = new CornerRadius(10),
                BorderBrush = new SolidColorBrush(Color.FromRgb(216, 228, 240)),
                BorderThickness = new Thickness(1)
            };

            var stack = new StackPanel { Margin = new Thickness(16) };

            stack.Children.Add(new TextBlock
            {
                Text = $"Mercedes-Benz {car.Name}",
                FontSize = 15,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(28, 43, 74)),
                TextWrapping = TextWrapping.Wrap
            });
            stack.Children.Add(new TextBlock
            {
                Text = car.Model,
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(107, 140, 174)),
                Margin = new Thickness(0, 2, 0, 10)
            });

            stack.Children.Add(DetailRow("📅 Year", car.Year.ToString()));
            stack.Children.Add(DetailRow("🎨 Color", car.Color));
            stack.Children.Add(DetailRow("⛽ Fuel", car.FuelType));
            stack.Children.Add(DetailRow("🛣 Mileage", car.MileageFormatted));

            if (!string.IsNullOrEmpty(car.Description))
                stack.Children.Add(new TextBlock
                {
                    Text = car.Description,
                    FontSize = 11,
                    Foreground = new SolidColorBrush(Color.FromRgb(130, 150, 170)),
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 8, 0, 0)
                });

            stack.Children.Add(new TextBlock
            {
                Text = car.PriceFormatted,
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(46, 109, 180)),
                Margin = new Thickness(0, 12, 0, 12)
            });

            var btnRow = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            // Fix #1: explicit cast so .Click is resolved on Button, not FrameworkElement
            var btnCart = (Button)MakeButton("🛒 Add to Cart", "#E8F0F8", "#1C5A9E");
            btnCart.Margin = new Thickness(0, 0, 8, 0);
            btnCart.Click += (_, _) => AddToCart(car);

            var btnBuy = (Button)MakeButton("💳 Buy Now", "#2E6DB4", "#FFFFFF");
            btnBuy.Click += (_, _) => OpenCheckout(car);

            btnRow.Children.Add(btnCart);
            btnRow.Children.Add(btnBuy);
            stack.Children.Add(btnRow);

            card.Child = stack;
            return card;
        }

        private static StackPanel DetailRow(string label, string value)
        {
            var row = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 2, 0, 2)
            };
            row.Children.Add(new TextBlock
            {
                Text = label + ": ",
                FontSize = 12,
                Width = 90,
                Foreground = new SolidColorBrush(Color.FromRgb(107, 140, 174))
            });
            row.Children.Add(new TextBlock
            {
                Text = value,
                FontSize = 12,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(28, 43, 74))
            });
            return row;
        }

        private static Button MakeButton(string text, string bg, string fg)
        {
            var btn = new Button
            {
                Content = text,
                FontSize = 12,
                FontWeight = FontWeights.SemiBold,
                Height = 36,
                Padding = new Thickness(10, 0, 10, 0),
                Cursor = System.Windows.Input.Cursors.Hand,
                // Fix #3: null-forgiving operator (!) suppresses nullable warning
                Foreground = new SolidColorBrush(
                                    (Color)ColorConverter.ConvertFromString(fg)!),
                BorderThickness = new Thickness(1),
                BorderBrush = new SolidColorBrush(Color.FromRgb(46, 109, 180)),
                Background = new SolidColorBrush(
                                    (Color)ColorConverter.ConvertFromString(bg)!)
            };
            return btn;
        }

        // Fix #4: !IsLoaded guards on all three handlers
        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        { if (!IsLoaded) return; LoadBrowseCars(); }

        private void CmbFuel_Changed(object sender, SelectionChangedEventArgs e)
        { if (!IsLoaded) return; LoadBrowseCars(); }

        private void CmbSort_Changed(object sender, SelectionChangedEventArgs e)
        { if (!IsLoaded) return; LoadBrowseCars(); }

        // ── CART ────────────────────────────────────────────────
        private void AddToCart(Car.Models.Car car)
        {
            CarRepository.AddToCart(_user.UserId, car.CarId);
            MessageBox.Show($"'{car.Name}' added to your cart!",
                "Cart Updated", MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void LoadCart()
        {
            CartItemsPanel.Children.Clear();
            var items = CarRepository.GetCartItems(_user.UserId);

            if (items.Count == 0)
            {
                TxtCartTotal.Text = "";
                CartItemsPanel.Children.Add(new TextBlock
                {
                    Text = "Your cart is empty. Browse cars to add some!",
                    FontSize = 14,
                    Foreground = Brushes.Gray
                });
                return;
            }

            decimal total = 0;
            foreach (var car in items)
            {
                total += car.Price;
                CartItemsPanel.Children.Add(BuildCartRow(car));
            }
            TxtCartTotal.Text = $"Total: PKR {total:N0}";
        }

        private Border BuildCartRow(Car.Models.Car car)
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

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var info = new StackPanel();
            info.Children.Add(new TextBlock
            {
                Text = $"Mercedes-Benz {car.Name} — {car.Model}",
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(28, 43, 74))
            });
            info.Children.Add(new TextBlock
            {
                Text = $"{car.Year}  •  {car.Color}  •  {car.FuelType}",
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(107, 140, 174)),
                Margin = new Thickness(0, 3, 0, 0)
            });
            info.Children.Add(new TextBlock
            {
                Text = car.PriceFormatted,
                FontSize = 15,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(46, 109, 180)),
                Margin = new Thickness(0, 6, 0, 0)
            });

            // Fix #1: explicit cast
            var btnBuy = (Button)MakeButton("💳 Buy Now", "#2E6DB4", "#FFFFFF");
            btnBuy.Margin = new Thickness(10, 0, 10, 0);
            btnBuy.VerticalAlignment = VerticalAlignment.Center;
            btnBuy.Click += (_, _) => OpenCheckout(car);

            var btnRemove = (Button)MakeButton("🗑 Remove", "#FFF0F0", "#C0392B");
            btnRemove.BorderBrush = new SolidColorBrush(Color.FromRgb(192, 57, 43));
            btnRemove.VerticalAlignment = VerticalAlignment.Center;
            btnRemove.Click += (_, _) =>
            {
                CarRepository.RemoveFromCart(_user.UserId, car.CarId);
                LoadCart();
            };

            Grid.SetColumn(info, 0);
            Grid.SetColumn(btnBuy, 1);
            Grid.SetColumn(btnRemove, 2);

            grid.Children.Add(info);
            grid.Children.Add(btnBuy);
            grid.Children.Add(btnRemove);

            border.Child = grid;
            return border;
        }

        // ── CHECKOUT ────────────────────────────────────────────
        private void OpenCheckout(Car.Models.Car car)
        {
            var win = new CheckoutWindow(car, _user);
            win.ShowDialog();
            LoadCart();
            LoadBrowseCars();
        }

        // ── ORDERS ──────────────────────────────────────────────
        private void LoadOrders()
        {
            OrdersPanel.Children.Clear();
            var orders = CarRepository.GetMyOrders(_user.UserId);

            if (orders.Count == 0)
            {
                OrdersPanel.Children.Add(new TextBlock
                {
                    Text = "You have not placed any orders yet.",
                    FontSize = 14,
                    Foreground = Brushes.Gray
                });
                return;
            }

            foreach (var order in orders)
                OrdersPanel.Children.Add(BuildOrderRow(order));
        }

        private Border BuildOrderRow(Order order)
        {
            var border = new Border
            {
                Background = Brushes.White,
                CornerRadius = new CornerRadius(8),
                BorderBrush = new SolidColorBrush(Color.FromRgb(216, 228, 240)),
                BorderThickness = new Thickness(1),
                Margin = new Thickness(0, 0, 0, 12),
                Padding = new Thickness(16, 14, 16, 14)
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var info = new StackPanel();
            info.Children.Add(new TextBlock
            {
                Text = $"Mercedes-Benz {order.CarName} — {order.CarModel} ({order.CarYear})",
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(28, 43, 74))
            });
            info.Children.Add(new TextBlock
            {
                Text = $"Receipt: {order.ReceiptNumber}   •   {order.DateFormatted}",
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(107, 140, 174)),
                Margin = new Thickness(0, 3, 0, 0)
            });
            info.Children.Add(new TextBlock
            {
                Text = order.PriceFormatted,
                FontSize = 15,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(46, 109, 180)),
                Margin = new Thickness(0, 6, 0, 0)
            });

            var badge = new Border
            {
                Background = order.Status == "Completed"
                    ? new SolidColorBrush(Color.FromRgb(240, 255, 244))
                    : new SolidColorBrush(Color.FromRgb(255, 248, 220)),
                BorderBrush = order.Status == "Completed"
                    ? new SolidColorBrush(Color.FromRgb(160, 216, 176))
                    : new SolidColorBrush(Color.FromRgb(240, 200, 100)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(8, 3, 8, 3),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 12, 0)
            };
            badge.Child = new TextBlock
            {
                Text = order.Status,
                FontSize = 12,
                FontWeight = FontWeights.SemiBold,
                Foreground = order.Status == "Completed"
                    ? new SolidColorBrush(Color.FromRgb(30, 122, 60))
                    : new SolidColorBrush(Color.FromRgb(160, 120, 0))
            };

            var btnPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center
            };

            // Fix #1: explicit cast
            var btnReceipt = (Button)MakeButton("🧾 Download Receipt", "#E8F0F8", "#1C5A9E");
            btnReceipt.Click += (_, _) => DownloadReceipt(order.OrderId);

            btnPanel.Children.Add(badge);
            btnPanel.Children.Add(btnReceipt);

            Grid.SetColumn(info, 0);
            Grid.SetColumn(btnPanel, 1);
            grid.Children.Add(info);
            grid.Children.Add(btnPanel);
            border.Child = grid;
            return border;
        }

        private void DownloadReceipt(int orderId)
        {
            var order = CarRepository.GetOrderById(orderId);
            if (order == null) return;

            // Fix #2: File.Exists now works because of `using System.IO;`
            if (!string.IsNullOrEmpty(order.ReceiptPath)
                && File.Exists(order.ReceiptPath))
            {
                ReceiptGenerator.OpenReceipt(order.ReceiptPath);
                return;
            }

            string path = ReceiptGenerator.GenerateAndSave(
                order, _user.Email, _user.PhoneNumber);
            ReceiptGenerator.OpenReceipt(path);
        }

        // ── QUERIES ─────────────────────────────────────────────
        private void BtnSubmitQuery_Click(object sender, RoutedEventArgs e)
        {
            QueryErrorBorder.Visibility = Visibility.Collapsed;
            string subject = TxtQuerySubject.Text.Trim();
            string message = TxtQueryMessage.Text.Trim();

            if (string.IsNullOrWhiteSpace(subject))
            {
                TxtQueryError.Text = "Please enter a subject.";
                QueryErrorBorder.Visibility = Visibility.Visible;
                TxtQuerySubject.Focus();
                return;
            }
            if (subject.Length < 5)
            {
                TxtQueryError.Text = "Subject must be at least 5 characters.";
                QueryErrorBorder.Visibility = Visibility.Visible;
                TxtQuerySubject.Focus();
                return;
            }
            if (string.IsNullOrWhiteSpace(message))
            {
                TxtQueryError.Text = "Please enter your message.";
                QueryErrorBorder.Visibility = Visibility.Visible;
                TxtQueryMessage.Focus();
                return;
            }
            if (message.Length < 10)
            {
                TxtQueryError.Text = "Message must be at least 10 characters.";
                QueryErrorBorder.Visibility = Visibility.Visible;
                TxtQueryMessage.Focus();
                return;
            }

            CarRepository.SubmitQuery(_user.UserId, subject, message);
            TxtQuerySubject.Clear();
            TxtQueryMessage.Clear();
            MessageBox.Show("Your query has been submitted successfully!\n" +
                            "A manager will reply shortly.",
                "Query Submitted", MessageBoxButton.OK,
                MessageBoxImage.Information);
            LoadQueries();
        }

        private void LoadQueries()
        {
            QueriesListPanel.Children.Clear();
            var queries = CarRepository.GetMyQueries(_user.UserId);

            if (queries.Count == 0)
            {
                QueriesListPanel.Children.Add(new TextBlock
                {
                    Text = "You have not submitted any queries yet.",
                    FontSize = 14,
                    Foreground = Brushes.Gray
                });
                return;
            }

            foreach (var q in queries)
            {
                var border = new Border
                {
                    Background = Brushes.White,
                    CornerRadius = new CornerRadius(8),
                    BorderBrush = new SolidColorBrush(Color.FromRgb(216, 228, 240)),
                    BorderThickness = new Thickness(1),
                    Margin = new Thickness(0, 0, 0, 12),
                    Padding = new Thickness(16, 14, 16, 14)
                };

                var sp = new StackPanel();
                sp.Children.Add(new TextBlock
                {
                    Text = q.Subject,
                    FontSize = 14,
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush(Color.FromRgb(28, 43, 74))
                });
                sp.Children.Add(new TextBlock
                {
                    Text = q.CreatedAt.ToString("dd MMM yyyy  HH:mm") +
                                 "   •   Status: " + q.Status,
                    FontSize = 11,
                    Foreground = new SolidColorBrush(Color.FromRgb(107, 140, 174)),
                    Margin = new Thickness(0, 3, 0, 6)
                });
                sp.Children.Add(new TextBlock
                {
                    Text = q.Message,
                    FontSize = 12,
                    Foreground = new SolidColorBrush(Color.FromRgb(60, 80, 100)),
                    TextWrapping = TextWrapping.Wrap
                });

                if (!string.IsNullOrEmpty(q.Reply))
                {
                    var replyBox = new Border
                    {
                        Background = new SolidColorBrush(Color.FromRgb(240, 246, 255)),
                        BorderBrush = new SolidColorBrush(Color.FromRgb(176, 200, 224)),
                        BorderThickness = new Thickness(1),
                        CornerRadius = new CornerRadius(6),
                        Padding = new Thickness(10, 8, 10, 8),
                        Margin = new Thickness(0, 10, 0, 0)
                    };
                    var replyStack = new StackPanel();
                    replyStack.Children.Add(new TextBlock
                    {
                        Text = "Manager Reply:",
                        FontSize = 11,
                        FontWeight = FontWeights.Bold,
                        Foreground = new SolidColorBrush(Color.FromRgb(28, 90, 158))
                    });
                    replyStack.Children.Add(new TextBlock
                    {
                        Text = q.Reply,
                        FontSize = 12,
                        Foreground = new SolidColorBrush(Color.FromRgb(28, 43, 74)),
                        TextWrapping = TextWrapping.Wrap,
                        Margin = new Thickness(0, 4, 0, 0)
                    });
                    replyBox.Child = replyStack;
                    sp.Children.Add(replyBox);
                }

                border.Child = sp;
                QueriesListPanel.Children.Add(border);
            }
        }
    }
}