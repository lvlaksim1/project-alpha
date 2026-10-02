using System.Data;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;
using Baraban.Models;
using Baraban.Services;

namespace Baraban;

public partial class MainWindow : Window
{
    private readonly SessionStore _sessionStore = new();
    private readonly DrumRepository _drumRepository = new();
    private readonly HttpExecutor _executor = new();
    private readonly Dictionary<string, HttpRunResult> _results = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> _variables = new(StringComparer.OrdinalIgnoreCase);
    private SessionProfile _session;
    private DrumDefinition? _drum;
    private HttpRequestDefinition? _request;
    private WebViewSessionBridge? _browserBridge;

    public MainWindow()
    {
        InitializeComponent();
        var version = typeof(MainWindow).Assembly.GetName().Version;
        Title = version is null
            ? "Baraban"
            : $"Baraban v{version.Major}.{version.Minor}.{version.Build}";
        _session = _sessionStore.Load();
        Loaded += MainWindow_Loaded;
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        ReloadDrums();
        RenderSession();
        try
        {
            _browserBridge = new WebViewSessionBridge(_sessionStore);
            await _browserBridge.InitializeAsync(Browser, _session, header => Dispatcher.Invoke(() =>
            {
                AppendLog($"Browser header captured: {header}");
                RenderSession();
            }));
        }
        catch (Exception ex)
        {
            AppendLog("WebView2: " + ex.Message);
        }
    }

    private string BuiltInDrumsDirectory => Path.Combine(AppContext.BaseDirectory, "Drums");
    private string DrumsDirectory => Path.Combine(_sessionStore.RootDirectory, "Drums");

    private void ReloadDrums()
    {
        _drumRepository.SeedUserDirectory(BuiltInDrumsDirectory, DrumsDirectory);
        var drums = _drumRepository.Load(DrumsDirectory);
        DrumList.ItemsSource = drums;
        if (drums.Count > 0)
            DrumList.SelectedIndex = 0;
        AppendLog($"Loaded drums: {drums.Count}");
    }

    private void ReloadDrums_Click(object sender, RoutedEventArgs e) => ReloadDrums();

    private void DrumList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _drum = DrumList.SelectedItem as DrumDefinition;
        RequestList.ItemsSource = _drum?.Requests;
        _results.Clear();
        _variables.Clear();
        if (_drum is not null)
        {
            foreach (var pair in _drum.Variables)
                _variables[pair.Key] = pair.Value;
            BrowserUrlText.Text = _drum.LoginUrl;
            RequestList.SelectedIndex = _drum.Requests.Count > 0 ? 0 : -1;
        }
        RenderVariables();
        ResultGrid.ItemsSource = null;
        ResultEmptyText.Text = "Результат появится здесь после запуска цепочки.";
        ResultEmptyText.Visibility = Visibility.Visible;
        ResponseText.Clear();
        ResponseStatusText.Text = "";
        ResponseGrid.ItemsSource = null;
    }

    private void RequestList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _request = RequestList.SelectedItem as HttpRequestDefinition;
        RenderRequest();
    }

    private void RenderRequest()
    {
        if (_request is null)
            return;
        MethodBox.Text = _request.Method;
        UrlText.Text = _request.Url;
        HeadersText.Text = string.Join(Environment.NewLine, _request.Headers.Select(x => $"{x.Key}: {x.Value}"));
        BodyText.Text = JsonTextFormatter.PrettyOrOriginal(_request.Body);
        ResponseText.Clear();
        ResponseStatusText.Text = "";
        ResponseGrid.ItemsSource = null;
    }

    private void SaveRequest_Click(object sender, RoutedEventArgs e)
    {
        if (_request is null)
            return;
        _request.Method = MethodBox.Text.Trim().ToUpperInvariant();
        _request.Url = UrlText.Text.Trim();
        _request.Headers = ParseHeaders(HeadersText.Text);
        _request.Body = JsonTextFormatter.PrettyOrOriginal(BodyText.Text);
        BodyText.Text = _request.Body;

        if (_drum is not null)
        {
            var path = Path.Combine(DrumsDirectory, $"{_drum.Id}.json");
            var json = JsonSerializer.Serialize(_drum, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            File.WriteAllText(path, json);
        }

        AppendLog($"Request saved: {_request.Name}");
    }

    private async void SendRequest_Click(object sender, RoutedEventArgs e)
    {
        if (_request is null)
            return;
        SaveRequest_Click(sender, e);
        ApplyVariablesFromText();
        _results.Clear();
        ResultGrid.ItemsSource = null;
        ResultEmptyText.Text = "Выполняется цепочка...";
        ResultEmptyText.Visibility = Visibility.Visible;
        try
        {
            await SyncBrowserSessionIfReadyAsync();
            var result = await _executor.SendAsync(_request, _session, _variables);
            _results[_request.Id] = result;
            WorkflowRunner.Capture(_request, result.ResponseBody, _variables);
            ResponseStatusText.Text = $"HTTP {result.StatusCode} {result.ReasonPhrase}";
            ResponseText.Text = JsonTextFormatter.PrettyOrOriginal(result.ResponseBody);
            ResponseGrid.ItemsSource = JsonTableProjector.Build(result.ResponseBody).DefaultView;
            AppendLog($"{_request.Id}: HTTP {result.StatusCode}");
            RenderVariables();
            RenderResult();
        }
        catch (Exception ex)
        {
            ResponseStatusText.Text = "Ошибка";
            ResponseText.Text = ex.Message;
            ResponseGrid.ItemsSource = null;
            AppendLog($"{_request.Id}: ERROR {ex.Message}");
        }
    }

    private async void RunWorkflow_Click(object sender, RoutedEventArgs e)
    {
        if (_drum is null)
            return;
        if (_drum.Archived)
        {
            MessageBox.Show(this, "Этот барабан сохранён как архивный. Автоматический запуск цепочки отключён.", "Архивный барабан", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        ApplyVariablesFromText();
        try
        {
            await SyncBrowserSessionIfReadyAsync();
            var runner = new WorkflowRunner(_executor);
            var results = await runner.RunAsync(_drum, _session, _variables);
            foreach (var result in results)
            {
                _results[result.RequestId] = result;
                AppendLog($"{result.RequestId}: HTTP {result.StatusCode}");
            }
            RenderVariables();
            RenderResult();
        }
        catch (Exception ex)
        {
            AppendLog("Workflow ERROR: " + ex.Message);
            MessageBox.Show(this, ex.Message, "Ошибка цепочки", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void RenderResult()
    {
        if (_drum is null)
            return;

        var table = ResultProjector.Build(_drum, _results, _variables);
        ResultGrid.ItemsSource = table.DefaultView;

        if (table.Rows.Count == 0)
        {
            ResultEmptyText.Text = _results.Count == 0
                ? "Результат появится здесь после запуска цепочки."
                : "Цепочка выполнена, но строки результата не найдены.";
            ResultEmptyText.Visibility = Visibility.Visible;
        }
        else
        {
            ResultEmptyText.Visibility = Visibility.Collapsed;
        }
    }

    private void ResultGrid_LoadingRow(object sender, DataGridRowEventArgs e)
    {
        if (e.Row.Item is DataRowView row && row.Row.Table.Columns.Contains("IsWinner") && row["IsWinner"] is true)
        {
            e.Row.Background = Brushes.Honeydew;
            e.Row.FontWeight = FontWeights.Bold;
        }
    }

    private void RenderVariables() => VariablesText.Text = JsonSerializer.Serialize(_variables, new JsonSerializerOptions { WriteIndented = true });

    private void ApplyVariables_Click(object sender, RoutedEventArgs e)
    {
        ApplyVariablesFromText();
        RenderVariables();
    }

    private void ApplyVariablesFromText()
    {
        var parsed = JsonSerializer.Deserialize<Dictionary<string, string>>(VariablesText.Text) ?? [];
        _variables.Clear();
        foreach (var pair in parsed)
            _variables[pair.Key] = pair.Value ?? "";
    }

    private void RenderSession() => SessionText.Text = _sessionStore.ExportEditable(_session);

    private void ReloadSession_Click(object sender, RoutedEventArgs e)
    {
        _session = _sessionStore.Load();
        RenderSession();
    }

    private async void SaveSession_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            _session = _sessionStore.ImportEditable(SessionText.Text);
            _sessionStore.Save(_session);
            if (_browserBridge is not null)
                await _browserBridge.RestoreSessionToBrowserAsync(Browser, _session);
            RenderSession();
            AppendLog("Session saved (DPAPI/current Windows user).");
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Некорректная сессия", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void ImportCookieHeader_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new CookieImportWindow { Owner = this };
        if (dialog.ShowDialog() != true)
            return;

        var domain = dialog.Domain.Trim();
        foreach (var part in dialog.CookieHeader.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var index = part.IndexOf('=');
            if (index <= 0)
                continue;
            var name = part[..index].Trim();
            var value = part[(index + 1)..].Trim();
            _session.Cookies.RemoveAll(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && c.Domain.Equals(domain, StringComparison.OrdinalIgnoreCase));
            _session.Cookies.Add(new StoredCookie { Name = name, Value = value, Domain = domain, Path = "/" });
        }
        _sessionStore.Save(_session);
        if (_browserBridge is not null)
            await _browserBridge.RestoreSessionToBrowserAsync(Browser, _session);
        RenderSession();
        AppendLog("Cookie header imported and applied to browser session.");
    }

    private async void ImportCaptureZip_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Выберите ZIP записи браузерной сессии",
            Filter = "ZIP archive (*.zip)|*.zip|All files (*.*)|*.*"
        };
        if (dialog.ShowDialog(this) != true)
            return;

        try
        {
            var importer = new CaptureZipImporter(_sessionStore);
            var result = importer.Import(dialog.FileName, _session);
            if (_browserBridge is not null)
                await _browserBridge.RestoreSessionToBrowserAsync(Browser, _session);
            if (!string.IsNullOrWhiteSpace(result.RestoreUrl))
                BrowserUrlText.Text = result.RestoreUrl;
            RenderSession();
            AppendLog($"Capture ZIP imported: format={result.Format} v{result.FormatVersion}, extension={result.ExtensionVersion}, requests={result.RequestsObserved}, request profiles={result.RequestProfiles}, cookies={result.CookiesImported}, localStorage={result.LocalStorageKeys}, sessionStorage={result.SessionStorageKeys}");
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Не удалось импортировать capture ZIP", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ClearSession_Click(object sender, RoutedEventArgs e)
    {
        _session = new SessionProfile();
        _sessionStore.Clear();
        RenderSession();
        AppendLog("Stored HTTP session cleared. WebView2 profile is intentionally retained.");
    }

    private void OpenBrowser_Click(object sender, RoutedEventArgs e)
    {
        if (Browser.CoreWebView2 is null)
            return;
        Browser.CoreWebView2.Navigate(BrowserUrlText.Text.Trim());
    }

    private async void SyncCookies_Click(object sender, RoutedEventArgs e)
    {
        if (_browserBridge is null)
            return;
        await _browserBridge.SyncCookiesFromBrowserAsync(Browser, _session);
        RenderSession();
        AppendLog($"Cookies synchronized: {_session.Cookies.Count}");
    }

    private async Task SyncBrowserSessionIfReadyAsync()
    {
        if (_browserBridge is null || Browser.CoreWebView2 is null)
            return;

        await _browserBridge.SyncCookiesFromBrowserAsync(Browser, _session);
        RenderSession();
    }

    private static Dictionary<string, string> ParseHeaders(string raw)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var line in raw.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries))
        {
            var index = line.IndexOf(':');
            if (index <= 0)
                continue;
            result[line[..index].Trim()] = line[(index + 1)..].Trim();
        }
        return result;
    }

    private static void AppendLog(string value) =>
        System.Diagnostics.Debug.WriteLine($"[{DateTime.Now:HH:mm:ss}] {value}");
}
