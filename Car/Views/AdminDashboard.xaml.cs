using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Car.Database;
using Car.Models;

namespace Car.Views
{
    public partial class AdminDashboard : Window
    {
        private readonly User _user;

        public AdminDashboard(User user)
        {
            InitializeComponent();
            _user = user;
            TxtWelcome.Text = $"Welcome,\n{user.FullName}";
            LoadOrders();
        }

        private void NavBtn_Click(object sender, RoutedEventArgs e)
        {
            string tag = (sender as Button)?.Tag?.ToString() ?? "";
            PanelOrders.Visibility = Visibility.Collapsed;
            PanelInventory.Visibility = Visibility.Collapsed;
            PanelReports.Visibility = Visibility.Collapsed;
            switch (tag)
            {
                case "Orders":
                    PanelOrders.Visibility = Visibility.Visible;
                    LoadOrders(); break;
                case "Inventory":
                    PanelInventory.Visibility = Visibility.Visible;
                    LoadInventory(); break;


                //case "Reports":
                //    PanelReports.Visibility = Visibility.Visible;
                //    LoadReports(null, null); break;


                case "Reports":
                    PanelReports.Visibility = Visibility.Visible;
                    ReportsPanel.Children.Clear();
                    break;
            }
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            new MainWindow().Show();
            this.Close();
        }

        // ── ORDERS ──────────────────────────────────────────────
        private void LoadOrders()
        {
            OrdersPanel.Children.Clear();
            var orders = OrderRepository.GetAllOrders();
            if (orders.Count == 0)
            {
                OrdersPanel.Children.Add(new TextBlock
                {
                    Text = "No orders placed yet.",
                    FontSize = 14,
                    Foreground = Brushes.Gray
                });
                return;
            }
            foreach (var o in orders)
                OrdersPanel.Children.Add(BuildOrderRow(o));
        }

        private Border BuildOrderRow(Order o)
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
            grid.ColumnDefinitions.Add(new ColumnDefinition
            { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition
            { Width = GridLength.Auto });

            var info = new StackPanel();
            info.Children.Add(new TextBlock
            {
                Text = $"Mercedes-Benz {o.CarName} — {o.CarModel} ({o.CarYear})",
                FontSize = 13,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(22, 33, 62))
            });
            info.Children.Add(new TextBlock
            {
                Text = $"Customer: {o.CustomerName}  •  {o.DateFormatted}",
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.FromRgb(107, 140, 174)),
                Margin = new Thickness(0, 3, 0, 0)
            });
            info.Children.Add(new TextBlock
            {
                Text = $"Receipt: {o.ReceiptNumber}",
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.FromRgb(107, 140, 174)),
                Margin = new Thickness(0, 2, 0, 0)
            });
            info.Children.Add(new TextBlock
            {
                Text = o.PriceFormatted,
                FontSize = 15,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(46, 109, 180)),
                Margin = new Thickness(0, 6, 0, 0)
            });

            var btnPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center
            };
            var btnView = MakeBtn("🧾 View Receipt", "#E8F0F8", "#1C5A9E");
            btnView.Click += (_, _) =>
            {
                if (!string.IsNullOrEmpty(o.ReceiptPath))
                    ReceiptGenerator.OpenReceipt(o.ReceiptPath);
                else
                    MessageBox.Show("Receipt not found for this order.",
                        "Not Found", MessageBoxButton.OK, MessageBoxImage.Warning);
            };
            btnPanel.Children.Add(btnView);

            Grid.SetColumn(info, 0);
            Grid.SetColumn(btnPanel, 1);
            grid.Children.Add(info);
            grid.Children.Add(btnPanel);
            border.Child = grid;
            return border;
        }

        // ── INVENTORY ────────────────────────────────────────────
        private void LoadInventory()
        {
            InventoryPanel.Children.Clear();
            var cars = CarInventoryRepository.GetAllCars();
            if (cars.Count == 0)
            {
                InventoryPanel.Children.Add(new TextBlock
                {
                    Text = "No cars in inventory yet.",
                    FontSize = 14,
                    Foreground = Brushes.Gray
                });
                return;
            }
            foreach (var car in cars)
                InventoryPanel.Children.Add(BuildInventoryRow(car));
        }

        private Border BuildInventoryRow(Car.Models.Car car)
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
                Foreground = new SolidColorBrush(Color.FromRgb(22, 33, 62))
            });
            info.Children.Add(new TextBlock
            {
                Text = $"{car.Color}  •  {car.FuelType}  •  {car.PriceFormatted}  •  Stock: {car.StockCount}",
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
                    FontSize = 12,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = new SolidColorBrush(badgeFg)
                }
            };

            Grid.SetColumn(info, 0);
            Grid.SetColumn(badge, 1);
            topGrid.Children.Add(info);
            topGrid.Children.Add(badge);
            mainStack.Children.Add(topGrid);

            // Status note (if any)
            if (!string.IsNullOrEmpty(car.StatusNote))
                mainStack.Children.Add(new TextBlock
                {
                    Text = "⚠ Note: " + car.StatusNote,
                    FontSize = 11,
                    FontStyle = FontStyles.Italic,
                    Foreground = Brushes.OrangeRed,
                    Margin = new Thickness(0, 4, 0, 0)
                });

            // Controls row
            var ctrlRow = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 10, 0, 0)
            };

            var cmbStatus = new ComboBox
            {
                Width = 160,
                FontSize = 12,
                Padding = new Thickness(8, 6, 8, 6),
                Margin = new Thickness(0, 0, 8, 0),
                SelectedIndex = car.CarStatus == "Needs Repair" ? 1
                              : car.CarStatus == "Needs Restock" ? 2 : 0
            };
            cmbStatus.Items.Add(new ComboBoxItem { Content = "Available" });
            cmbStatus.Items.Add(new ComboBoxItem { Content = "Needs Repair" });
            cmbStatus.Items.Add(new ComboBoxItem { Content = "Needs Restock" });

            var noteBorder = new Border
            {
                BorderBrush = new SolidColorBrush(Color.FromRgb(176, 200, 224)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(6),
                Margin = new Thickness(0, 0, 8, 0),
                Width = 280
            };
            var noteTxt = new TextBox
            {
                Background = new SolidColorBrush(Color.FromRgb(240, 246, 255)),
                Foreground = new SolidColorBrush(Color.FromRgb(28, 43, 74)),
                CaretBrush = new SolidColorBrush(Color.FromRgb(28, 43, 74)),
                BorderThickness = new Thickness(0),
                FontSize = 12,
                Padding = new Thickness(8, 6, 8, 6),
                Text = car.StatusNote,
                MaxLength = 500
            };
            noteBorder.Child = noteTxt;

            var btnUpdate = MakeBtn("✔ Update Status", "#2E6DB4", "#FFFFFF");
            btnUpdate.Click += (_, _) =>
            {
                string newStatus = (cmbStatus.SelectedItem as ComboBoxItem)
                    ?.Content?.ToString() ?? "Available";
                string note = noteTxt.Text.Trim();
                CarInventoryRepository.UpdateCarStatus(
                    car.CarId, newStatus, note, _user.UserId);
                MessageBox.Show("Car status updated!",
                    "Updated", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadInventory();
            };

            var btnDelete = MakeBtn("🗑 Remove", "#FFF0F0", "#C0392B");
            btnDelete.BorderBrush = new SolidColorBrush(Color.FromRgb(192, 57, 43));
            btnDelete.Margin = new Thickness(8, 0, 0, 0);
            btnDelete.Click += (_, _) =>
            {
                var res = MessageBox.Show(
                    $"Remove '{car.Name}' from inventory?",
                    "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (res == MessageBoxResult.Yes)
                {
                    CarInventoryRepository.DeleteCar(car.CarId);
                    LoadInventory();
                }
            };

            ctrlRow.Children.Add(cmbStatus);
            ctrlRow.Children.Add(noteBorder);
            ctrlRow.Children.Add(btnUpdate);
            ctrlRow.Children.Add(btnDelete);
            mainStack.Children.Add(ctrlRow);

            border.Child = mainStack;
            return border;
        }

        private void BtnAddCar_Click(object sender, RoutedEventArgs e)
        {
            var win = new AddCarWindow();
            win.ShowDialog();
            LoadInventory();
        }

        // ── REPORTS ──────────────────────────────────────────────
        //private void BtnGenReport_Click(object sender, RoutedEventArgs e)
        //{
        //    LoadReports(DpFrom.SelectedDate, DpTo.SelectedDate);
        //}

        private void BtnGenReport_Click(object sender, RoutedEventArgs e)
        {
            if (ReportsPanel == null) return;
            LoadReports(DpFrom.SelectedDate, DpTo.SelectedDate);
        }










        //private void LoadReports(DateTime? from, DateTime? to)
        //{
        //    ReportsPanel.Children.Clear();

        //    try
        //    {
        //        // ── Summary stats ──
        //        var stats = OrderRepository.GetDashboardStats();
        //        var invStats = CarInventoryRepository.GetInventoryStats();

        //        ReportsPanel.Children.Add(SectionTitle("📊 Summary"));

        //        var statsGrid = new Grid { Margin = new Thickness(0, 0, 0, 20) };
        //        statsGrid.ColumnDefinitions.Add(new ColumnDefinition());
        //        statsGrid.ColumnDefinitions.Add(new ColumnDefinition());
        //        statsGrid.ColumnDefinitions.Add(new ColumnDefinition());
        //        statsGrid.ColumnDefinitions.Add(new ColumnDefinition());

        //        var c0 = StatCard("💰 Revenue",
        //            $"PKR {stats.TotalRevenue:N0}", 0);
        //        var c1 = StatCard("📦 Orders",
        //            stats.TotalOrders.ToString(), 1);
        //        var c2 = StatCard("👥 Customers",
        //            stats.TotalCustomers.ToString(), 2);
        //        var c3 = StatCard("🚗 Total Cars",
        //            stats.TotalCars.ToString(), 3);

        //        statsGrid.Children.Add(c0);
        //        statsGrid.Children.Add(c1);
        //        statsGrid.Children.Add(c2);
        //        statsGrid.Children.Add(c3);
        //        ReportsPanel.Children.Add(statsGrid);

        //        // ── Inventory status ──
        //        ReportsPanel.Children.Add(SectionTitle("🚗 Inventory Status"));

        //        var invGrid = new Grid { Margin = new Thickness(0, 0, 0, 20) };
        //        invGrid.ColumnDefinitions.Add(new ColumnDefinition());
        //        invGrid.ColumnDefinitions.Add(new ColumnDefinition());
        //        invGrid.ColumnDefinitions.Add(new ColumnDefinition());

        //        invGrid.Children.Add(StatCard("✅ Available",
        //            invStats.Available.ToString(), 0,
        //            Color.FromRgb(240, 255, 244), Color.FromRgb(30, 122, 60)));
        //        invGrid.Children.Add(StatCard("🔧 Needs Repair",
        //            invStats.NeedsRepair.ToString(), 1,
        //            Color.FromRgb(255, 240, 240), Color.FromRgb(192, 57, 43)));
        //        invGrid.Children.Add(StatCard("📦 Needs Restock",
        //            invStats.NeedsRestock.ToString(), 2,
        //            Color.FromRgb(255, 248, 220), Color.FromRgb(160, 120, 0)));
        //        ReportsPanel.Children.Add(invGrid);

        //        // ── Sales table ──
        //        ReportsPanel.Children.Add(SectionTitle("💳 Sales Orders"));
        //        var orders = OrderRepository.GetOrdersForReport(from, to);

        //        if (orders.Count == 0)
        //        {
        //            ReportsPanel.Children.Add(new TextBlock
        //            {
        //                Text = "No orders found for selected date range.",
        //                FontSize = 13,
        //                Foreground = Brushes.Gray,
        //                Margin = new Thickness(0, 0, 0, 20)
        //            });
        //        }
        //        else
        //        {
        //            var salesRows = orders.Select(o => new[]
        //            {
        //                o.ReceiptNumber,
        //                o.CustomerName,
        //                $"MB {o.CarName} ({o.CarYear})",
        //                o.PriceFormatted,
        //                o.DateFormatted,
        //                o.Status
        //            }).ToList();

        //            ReportsPanel.Children.Add(BuildTable(
        //                new[] { "Receipt #", "Customer", "Car",
        //                        "Amount", "Date", "Status" },
        //                salesRows));
        //        }

        //        // ── Customer activity ──
        //        ReportsPanel.Children.Add(SectionTitle("👥 Customer Activity"));
        //        var activity = CarInventoryRepository.GetCustomerActivity();

        //        if (activity.Count == 0)
        //        {
        //            ReportsPanel.Children.Add(new TextBlock
        //            {
        //                Text = "No customer activity yet.",
        //                FontSize = 13,
        //                Foreground = Brushes.Gray,
        //                Margin = new Thickness(0, 0, 0, 20)
        //            });
        //        }
        //        else
        //        {
        //            var actRows = activity.Select(a => new[]
        //            {
        //                a.Name,
        //                a.Email,
        //                a.Orders.ToString(),
        //                $"PKR {a.Spent:N0}"
        //            }).ToList();

        //            ReportsPanel.Children.Add(BuildTable(
        //                new[] { "Customer", "Email", "Orders", "Total Spent" },
        //                actRows));
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ReportsPanel.Children.Add(new TextBlock
        //        {
        //            Text = $"Error loading report: {ex.Message}",
        //            FontSize = 13,
        //            Foreground = Brushes.Red,
        //            TextWrapping = TextWrapping.Wrap
        //        });
        //    }
        //}





        private void LoadReports(DateTime? from, DateTime? to)
        {
            ReportsPanel.Children.Clear();

            // ── Step 1: Summary stats ──────────────────────────────
            try
            {
                var stats = OrderRepository.GetDashboardStats();

                ReportsPanel.Children.Add(SectionTitle("📊 Summary"));

                var statsGrid = new Grid { Margin = new Thickness(0, 0, 0, 20) };
                statsGrid.ColumnDefinitions.Add(new ColumnDefinition());
                statsGrid.ColumnDefinitions.Add(new ColumnDefinition());
                statsGrid.ColumnDefinitions.Add(new ColumnDefinition());
                statsGrid.ColumnDefinitions.Add(new ColumnDefinition());

                var s0 = StatCard("💰 Revenue",
                    $"PKR {stats.TotalRevenue:N0}", 0);
                var s1 = StatCard("📦 Orders",
                    stats.TotalOrders.ToString(), 1);
                var s2 = StatCard("👥 Customers",
                    stats.TotalCustomers.ToString(), 2);
                var s3 = StatCard("🚗 Total Cars",
                    stats.TotalCars.ToString(), 3);

                statsGrid.Children.Add(s0);
                statsGrid.Children.Add(s1);
                statsGrid.Children.Add(s2);
                statsGrid.Children.Add(s3);
                ReportsPanel.Children.Add(statsGrid);
            }
            catch (Exception ex)
            {
                ReportsPanel.Children.Add(new TextBlock
                {
                    Text = "Summary error: " + ex.Message,
                    Foreground = Brushes.Red,
                    FontSize = 12,
                    Margin = new Thickness(0, 0, 0, 10),
                    TextWrapping = TextWrapping.Wrap
                });
            }

            // ── Step 2: Inventory stats ────────────────────────────
            try
            {
                var inv = CarInventoryRepository.GetInventoryStats();

                ReportsPanel.Children.Add(SectionTitle("🚗 Inventory Status"));

                var invGrid = new Grid { Margin = new Thickness(0, 0, 0, 20) };
                invGrid.ColumnDefinitions.Add(new ColumnDefinition());
                invGrid.ColumnDefinitions.Add(new ColumnDefinition());
                invGrid.ColumnDefinitions.Add(new ColumnDefinition());

                var i0 = StatCard("✅ Available",
                    inv.Available.ToString(), 0,
                    Color.FromRgb(240, 255, 244),
                    Color.FromRgb(30, 122, 60));
                var i1 = StatCard("🔧 Needs Repair",
                    inv.NeedsRepair.ToString(), 1,
                    Color.FromRgb(255, 240, 240),
                    Color.FromRgb(192, 57, 43));
                var i2 = StatCard("📦 Needs Restock",
                    inv.NeedsRestock.ToString(), 2,
                    Color.FromRgb(255, 248, 220),
                    Color.FromRgb(160, 120, 0));

                invGrid.Children.Add(i0);
                invGrid.Children.Add(i1);
                invGrid.Children.Add(i2);
                ReportsPanel.Children.Add(invGrid);
            }
            catch (Exception ex)
            {
                ReportsPanel.Children.Add(new TextBlock
                {
                    Text = "Inventory error: " + ex.Message,
                    Foreground = Brushes.Red,
                    FontSize = 12,
                    Margin = new Thickness(0, 0, 0, 10),
                    TextWrapping = TextWrapping.Wrap
                });
            }

            // ── Step 3: Sales orders table ─────────────────────────
            try
            {
                ReportsPanel.Children.Add(SectionTitle("💳 Sales Orders"));
                var orders = OrderRepository.GetOrdersForReport(from, to);

                if (orders.Count == 0)
                {
                    ReportsPanel.Children.Add(new TextBlock
                    {
                        Text = "No orders found for the selected date range.",
                        FontSize = 13,
                        Foreground = Brushes.Gray,
                        Margin = new Thickness(0, 0, 0, 20)
                    });
                }
                else
                {
                    var rows = new List<string[]>();
                    foreach (var o in orders)
                    {
                        rows.Add(new[]
                        {
                    o.ReceiptNumber,
                    o.CustomerName,
                    $"MB {o.CarName} ({o.CarYear})",
                    o.PriceFormatted,
                    o.DateFormatted,
                    o.Status
                });
                    }
                    ReportsPanel.Children.Add(BuildTable(
                        new[] { "Receipt #", "Customer", "Car",
                        "Amount", "Date", "Status" },
                        rows));
                }
            }
            catch (Exception ex)
            {
                ReportsPanel.Children.Add(new TextBlock
                {
                    Text = "Sales error: " + ex.Message,
                    Foreground = Brushes.Red,
                    FontSize = 12,
                    Margin = new Thickness(0, 0, 0, 10),
                    TextWrapping = TextWrapping.Wrap
                });
            }

            // ── Step 4: Customer activity table ───────────────────
            try
            {
                ReportsPanel.Children.Add(SectionTitle("👥 Customer Activity"));
                var activity = CarInventoryRepository.GetCustomerActivity();

                if (activity.Count == 0)
                {
                    ReportsPanel.Children.Add(new TextBlock
                    {
                        Text = "No customer activity yet.",
                        FontSize = 13,
                        Foreground = Brushes.Gray,
                        Margin = new Thickness(0, 0, 0, 20)
                    });
                }
                else
                {
                    var rows = new List<string[]>();
                    foreach (var a in activity)
                    {
                        rows.Add(new[]
                        {
                    a.Name,
                    a.Email,
                    a.Orders.ToString(),
                    $"PKR {a.Spent:N0}"
                });
                    }
                    ReportsPanel.Children.Add(BuildTable(
                        new[] { "Customer", "Email", "Orders", "Total Spent" },
                        rows));
                }
            }
            catch (Exception ex)
            {
                ReportsPanel.Children.Add(new TextBlock
                {
                    Text = "Customer activity error: " + ex.Message,
                    Foreground = Brushes.Red,
                    FontSize = 12,
                    Margin = new Thickness(0, 0, 0, 10),
                    TextWrapping = TextWrapping.Wrap
                });
            }
        }

















        // ── HELPERS ──────────────────────────────────────────────
        private static TextBlock SectionTitle(string text) => new()
        {
            Text = text,
            FontSize = 16,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromRgb(22, 33, 62)),
            Margin = new Thickness(0, 16, 0, 10)
        };

        private static Border StatCard(string label, string value, int col,
            Color? bg = null, Color? fg = null)
        {
            var card = new Border
            {
                Background = new SolidColorBrush(
                    bg ?? Color.FromRgb(240, 246, 255)),
                CornerRadius = new CornerRadius(8),
                BorderBrush = new SolidColorBrush(
                    fg ?? Color.FromRgb(176, 200, 224)),
                BorderThickness = new Thickness(1),
                Margin = new Thickness(col == 0 ? 0 : 8, 0, 0, 0),
                Padding = new Thickness(16, 14, 16, 14)
            };

            // ── THIS was the root cause of blank reports ──
            // Grid.SetColumn must be called on the element BEFORE
            // it is added to the Grid, and the card IS the element.
            Grid.SetColumn(card, col);

            var sp = new StackPanel();
            sp.Children.Add(new TextBlock
            {
                Text = label,
                FontSize = 12,
                Foreground = new SolidColorBrush(
                    fg ?? Color.FromRgb(70, 100, 140))
            });
            sp.Children.Add(new TextBlock
            {
                Text = value,
                FontSize = 20,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(
                    fg ?? Color.FromRgb(22, 33, 62)),
                Margin = new Thickness(0, 4, 0, 0)
            });
            card.Child = sp;
            return card;
        }

        private static Border BuildTable(string[] headers, List<string[]> rows)
        {
            var outer = new Border
            {
                Background = Brushes.White,
                CornerRadius = new CornerRadius(8),
                BorderBrush = new SolidColorBrush(Color.FromRgb(216, 228, 240)),
                BorderThickness = new Thickness(1),
                Margin = new Thickness(0, 0, 0, 20)
            };

            var grid = new Grid();
            foreach (var _ in headers)
                grid.ColumnDefinitions.Add(new ColumnDefinition());

            // Header row
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            for (int c = 0; c < headers.Length; c++)
            {
                var cell = new Border
                {
                    Background = new SolidColorBrush(Color.FromRgb(22, 33, 62)),
                    Padding = new Thickness(10, 9, 10, 9)
                };
                cell.Child = new TextBlock
                {
                    Text = headers[c],
                    Foreground = Brushes.White,
                    FontSize = 12,
                    FontWeight = FontWeights.Bold
                };
                Grid.SetRow(cell, 0);
                Grid.SetColumn(cell, c);
                grid.Children.Add(cell);
            }

            // Data rows
            for (int r = 0; r < rows.Count; r++)
            {
                int rowIndex = r + 1;
                grid.RowDefinitions.Add(new RowDefinition
                { Height = GridLength.Auto });
                var rowBg = rowIndex % 2 == 0
                    ? Color.FromRgb(248, 251, 255) : Colors.White;

                for (int c = 0; c < rows[r].Length && c < headers.Length; c++)
                {
                    var cell = new Border
                    {
                        Background = new SolidColorBrush(rowBg),
                        Padding = new Thickness(10, 8, 10, 8),
                        BorderBrush = new SolidColorBrush(
                            Color.FromRgb(230, 238, 248)),
                        BorderThickness = new Thickness(0, 0, 0, 1)
                    };
                    cell.Child = new TextBlock
                    {
                        Text = rows[r][c],
                        FontSize = 12,
                        Foreground = new SolidColorBrush(
                            Color.FromRgb(40, 60, 80)),
                        TextWrapping = TextWrapping.Wrap
                    };
                    Grid.SetRow(cell, rowIndex);
                    Grid.SetColumn(cell, c);
                    grid.Children.Add(cell);
                }
            }

            outer.Child = grid;
            return outer;
        }

        private static Button MakeBtn(string text, string bg, string fg)
        {
            return new Button
            {
                Content = text,
                FontSize = 12,
                FontWeight = FontWeights.SemiBold,
                Height = 34,
                Padding = new Thickness(12, 0, 12, 0),
                Cursor = System.Windows.Input.Cursors.Hand,
                Foreground = new SolidColorBrush(
                    (Color)ColorConverter.ConvertFromString(fg)!),
                Background = new SolidColorBrush(
                    (Color)ColorConverter.ConvertFromString(bg)!),
                BorderBrush = new SolidColorBrush(Color.FromRgb(46, 109, 180)),
                BorderThickness = new Thickness(1)
            };
        }
    }
}