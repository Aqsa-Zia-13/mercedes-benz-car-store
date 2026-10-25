using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Car.Database;
using Car.Models;

namespace Car.Views
{
    public partial class ManagerDashboard : Window
    {
        private readonly User _user;

        public ManagerDashboard(User user)
        {
            InitializeComponent();
            _user = user;
            TxtWelcome.Text = $"Welcome,\n{user.FullName}";
            LoadQueries();
        }

        private void NavBtn_Click(object sender, RoutedEventArgs e)
        {
            string tag = (sender as Button)?.Tag?.ToString() ?? "";
            PanelQueries.Visibility = Visibility.Collapsed;
            PanelInventory.Visibility = Visibility.Collapsed;
            if (tag == "Queries")
            {
                PanelQueries.Visibility = Visibility.Visible;
                LoadQueries();
            }
            else
            {
                PanelInventory.Visibility = Visibility.Visible;
                LoadInventory();
            }
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            new MainWindow().Show();
            this.Close();
        }

        // ── QUERIES ─────────────────────────────────────────────
        private void LoadQueries()
        {
            QueriesPanel.Children.Clear();
            var queries = ManagerRepository.GetAllQueries();

            if (queries.Count == 0)
            {
                QueriesPanel.Children.Add(new TextBlock
                {
                    Text = "No customer queries at the moment.",
                    FontSize = 14,
                    Foreground = Brushes.Gray
                });
                return;
            }

            foreach (var q in queries)
                QueriesPanel.Children.Add(BuildQueryCard(q));
        }

        private Border BuildQueryCard(QueryItem q)
        {
            var border = new Border
            {
                Background = Brushes.White,
                CornerRadius = new CornerRadius(8),
                BorderBrush = new SolidColorBrush(Color.FromRgb(216, 228, 240)),
                BorderThickness = new Thickness(1),
                Margin = new Thickness(0, 0, 0, 14),
                Padding = new Thickness(18, 14, 18, 14)
            };

            var sp = new StackPanel();

            // Header row
            var headerRow = new Grid();
            headerRow.ColumnDefinitions.Add(new ColumnDefinition
            { Width = new GridLength(1, GridUnitType.Star) });
            headerRow.ColumnDefinitions.Add(new ColumnDefinition
            { Width = GridLength.Auto });

            var titleStack = new StackPanel();
            titleStack.Children.Add(new TextBlock
            {
                Text = q.Subject,
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(15, 52, 96))
            });
            titleStack.Children.Add(new TextBlock
            {
                Text = $"From: {q.CustomerName}  •  {q.CreatedAt:dd MMM yyyy HH:mm}",
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.FromRgb(107, 140, 174)),
                Margin = new Thickness(0, 3, 0, 0)
            });

            // Status badge
            bool resolved = q.Status == "Resolved";
            var badge = new Border
            {
                Background = resolved
                    ? new SolidColorBrush(Color.FromRgb(240, 255, 244))
                    : new SolidColorBrush(Color.FromRgb(255, 248, 220)),
                BorderBrush = resolved
                    ? new SolidColorBrush(Color.FromRgb(160, 216, 176))
                    : new SolidColorBrush(Color.FromRgb(240, 200, 100)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(8, 3, 8, 3),
                VerticalAlignment = VerticalAlignment.Top
            };
            badge.Child = new TextBlock
            {
                Text = q.Status,
                FontSize = 11,
                FontWeight = FontWeights.SemiBold,
                Foreground = resolved
                    ? new SolidColorBrush(Color.FromRgb(30, 122, 60))
                    : new SolidColorBrush(Color.FromRgb(160, 120, 0))
            };

            Grid.SetColumn(titleStack, 0);
            Grid.SetColumn(badge, 1);
            headerRow.Children.Add(titleStack);
            headerRow.Children.Add(badge);
            sp.Children.Add(headerRow);

            // Message
            sp.Children.Add(new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(245, 248, 252)),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(12, 8, 12, 8),
                Margin = new Thickness(0, 10, 0, 0),
                Child = new TextBlock
                {
                    Text = q.Message,
                    FontSize = 12,
                    Foreground = new SolidColorBrush(Color.FromRgb(40, 60, 80)),
                    TextWrapping = TextWrapping.Wrap
                }
            });

            // Existing reply
            if (!string.IsNullOrEmpty(q.ManagerReply))
            {
                sp.Children.Add(new Border
                {
                    Background = new SolidColorBrush(Color.FromRgb(240, 246, 255)),
                    BorderBrush = new SolidColorBrush(Color.FromRgb(176, 200, 224)),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(6),
                    Padding = new Thickness(12, 8, 12, 8),
                    Margin = new Thickness(0, 8, 0, 0),
                    Child = new StackPanel
                    {
                        Children =
                        {
                            new TextBlock
                            {
                                Text = "Your Reply:",
                                FontSize = 11, FontWeight = FontWeights.Bold,
                                Foreground = new SolidColorBrush(
                                    Color.FromRgb(28, 90, 158))
                            },
                            new TextBlock
                            {
                                Text = q.ManagerReply, FontSize = 12,
                                Foreground = new SolidColorBrush(
                                    Color.FromRgb(28, 43, 74)),
                                TextWrapping = TextWrapping.Wrap,
                                Margin = new Thickness(0, 4, 0, 0)
                            }
                        }
                    }
                });
            }

            // Reply box (always visible so manager can update reply)
            var replyBox = new Border
            {
                BorderBrush = new SolidColorBrush(Color.FromRgb(176, 200, 224)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(6),
                Margin = new Thickness(0, 10, 0, 0)
            };
            var replyTxt = new TextBox
            {
                Background = new SolidColorBrush(Color.FromRgb(240, 246, 255)),
                Foreground = new SolidColorBrush(Color.FromRgb(28, 43, 74)),
                CaretBrush = new SolidColorBrush(Color.FromRgb(28, 43, 74)),
                BorderThickness = new Thickness(0),
                FontSize = 13,
                Padding = new Thickness(10, 8, 10, 8),
                Height = 70,
                TextWrapping = TextWrapping.Wrap,
                AcceptsReturn = true,
                MaxLength = 2000,
                Text = q.ManagerReply
            };
            replyBox.Child = replyTxt;
            sp.Children.Add(replyBox);

            // Send reply button
            var btnReply = new Button
            {
                Content = resolved ? "✏  Update Reply" : "✉  Send Reply",
                FontSize = 12,
                FontWeight = FontWeights.SemiBold,
                Foreground = Brushes.White,
                Background = new SolidColorBrush(Color.FromRgb(15, 52, 96)),
                BorderThickness = new Thickness(0),
                Height = 36,
                Width = 160,
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(0, 8, 0, 0),
                Cursor = System.Windows.Input.Cursors.Hand
            };
            btnReply.Click += (_, _) =>
            {
                string reply = replyTxt.Text.Trim();
                if (string.IsNullOrWhiteSpace(reply))
                {
                    MessageBox.Show("Please write a reply before sending.",
                        "Validation", MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }
                ManagerRepository.ReplyToQuery(q.QueryId, reply);
                MessageBox.Show("Reply sent successfully!",
                    "Done", MessageBoxButton.OK,
                    MessageBoxImage.Information);
                LoadQueries();
            };
            sp.Children.Add(btnReply);

            border.Child = sp;
            return border;
        }

        // ── INVENTORY (view only) ────────────────────────────────
        private void LoadInventory()
        {
            InventoryPanel.Children.Clear();
            var cars = CarInventoryRepository.GetAllCars();
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
                Text = $"Mercedes-Benz {car.Name} — {car.Model} ({car.Year})",
                FontSize = 13,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(15, 52, 96))
            });
            info.Children.Add(new TextBlock
            {
                Text = $"{car.Color}  •  {car.FuelType}  •  {car.PriceFormatted}  •  Stock: {car.StockCount}",
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.FromRgb(107, 140, 174)),
                Margin = new Thickness(0, 3, 0, 0)
            });
            if (!string.IsNullOrEmpty(car.StatusNote))
                info.Children.Add(new TextBlock
                {
                    Text = "Note: " + car.StatusNote,
                    FontSize = 11,
                    FontStyle = FontStyles.Italic,
                    Foreground = Brushes.OrangeRed,
                    Margin = new Thickness(0, 3, 0, 0)
                });

            // Status badge
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
                VerticalAlignment = VerticalAlignment.Center,
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
            grid.Children.Add(info);
            grid.Children.Add(badge);
            border.Child = grid;
            return border;
        }
    }
}