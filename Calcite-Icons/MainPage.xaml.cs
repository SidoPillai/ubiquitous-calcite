using Esri.Calcite.Maui;
using System.Collections.ObjectModel;

namespace Calcite_Icons
{
    public partial class MainPage : ContentPage
    {
        public ObservableCollection<IconItem> Icons { get; set; }
        public ObservableCollection<IconItem> FilteredIcons { get; set; }
        private CancellationTokenSource _searchCancellationTokenSource;

        public MainPage()
        {
            InitializeComponent();
            Icons = new ObservableCollection<IconItem>();
            FilteredIcons = new ObservableCollection<IconItem>();
            IconsCollectionView.ItemsSource = FilteredIcons;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await Task.Delay(1000);
            _ = LoadIconsAsync();
        }

        private async Task LoadIconsAsync()
        {
            BusyIndicator.IsRunning = true;
            BusyIndicator.IsVisible = true;

            try
            {
                var icons = Enum.GetValues(typeof(CalciteIcon));
                Color iconColor = Colors.Black;

                if (Application.Current != null && Application.Current.RequestedTheme == AppTheme.Dark)
                {
                    iconColor = Colors.White;
                }

                await Dispatcher.DispatchAsync(async () =>
                {
                    foreach (var icon in icons)
                    {
                        var calciteIcon = (CalciteIcon)icon;
                        Console.WriteLine($"Loading {calciteIcon.ToString()}");
                        var iconItem = new IconItem
                        {
                            IconName = calciteIcon.ToString(),
                            Icon16 = GetCalciteIcon(calciteIcon, CalciteIconScale.Small, 16, iconColor),
                            Icon20 = GetCalciteIcon(calciteIcon, CalciteIconScale.Medium, 20, iconColor),
                            Icon24 = GetCalciteIcon(calciteIcon, CalciteIconScale.Large, 24, iconColor)
                        };

                        Icons.Add(iconItem);
                    }
                });
            }
            catch (Exception ex)
            {
                // Log the exception (optional)
                System.Diagnostics.Debug.WriteLine($"Error loading icons: {ex.Message}");

                // Display a user-friendly message
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "Failed to load icons. Please try again later.", "OK");
                });
            }
            finally
            {
                BusyIndicator.IsRunning = false;
                BusyIndicator.IsVisible = false;
            }
        }

        private CalciteIconImageSource GetCalciteIcon(CalciteIcon Icon, CalciteIconScale Scale, int Size, Color Color)
        {
            var CheckCircleIcon = new CalciteIconImageSource
            {
                Icon = Icon,
                Size = Size,
                Color = Color,
                Scale = Scale
            };
            return CheckCircleIcon;
        }

        private void OnSearchBarTextChanged(object sender, TextChangedEventArgs e)
        {
            _searchCancellationTokenSource?.Cancel();
            _searchCancellationTokenSource?.Dispose();
            _searchCancellationTokenSource = new CancellationTokenSource();
            var token = _searchCancellationTokenSource.Token;

            Task.Delay(2000, token).ContinueWith(async t =>
            {
                if (!t.IsCanceled)
                {
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        var searchText = e.NewTextValue.ToLower();
                        FilteredIcons.Clear();
                        foreach (var icon in Icons.Where(i => i.IconName.ToLower().Contains(searchText)))
                        {
                            FilteredIcons.Add(icon);
                        }
                    });
                }
            }, token);
        }
    }

    public class IconItem
    {
        public required string IconName { get; set; }
        public required CalciteIconImageSource Icon16 { get; set; }
        public required CalciteIconImageSource Icon20 { get; set; }
        public required CalciteIconImageSource Icon24 { get; set; }
    }
}
