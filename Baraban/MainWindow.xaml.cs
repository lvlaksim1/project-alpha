using System.Data;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
    private readonly List<Action> _findMatches = [];

    private SessionProfile _session;
    private DrumDefinition? _drum;
    private HttpRequestDefinition? _request;
    private WebViewSessionBridge? _browserBridge;
    private DataTable? _prizeTable;
    private HttpRunResult? _lastStateResult;
    private int _findMatchIndex = -1;
    private bool _showWinnerState;

    public MainWindow()
    {
        InitializeComponent();

        var version = typeof(MainWindow).Assembly.GetName().Version;
        Title = version is null
            ? "Baraban"
            : $"Baraban v{version.Major}.{version.Minor}.{version.Build}";

        _session = _sessionStore.Load();
        Loaded += MainWindow_Loaded;
        PreviewKeyDown += MainWindow_PreviewKeyDown;
    }

    private string BuiltInDrumsDirectory => Path.Combine(AppContext.BaseDirectory, "Drums");
    private string DrumsDirectory => Path.Combine(_sessionStore.RootDirectory, "Drums");

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        ReloadDrums();
        RenderSession();

        try
        {
            _browserBridge = new WebViewSessionBridge(_sessionStore);
            await _browserBridge.InitializeAsync(Browser, _session, _ =>
                Dispatcher.Invoke(RenderSession));
        }
        catch (Exception ex)
        {
            AppendLog("WebView2: " + ex.Message);
        }
    }

    private void ReloadDrums()
    {
        _drumRepository.SeedUserDirectory(BuiltInDrumsDirectory, DrumsDirectory);
        var drums = _drumRepository.Load(DrumsDirectory);
        DrumList.ItemsSource = drums;

        if (drums.Count > 0)
            DrumList.SelectedIndex = 0;
    }

    private void ReloadDrums_Click(object sender, RoutedEventArgs e) => ReloadDrums();

    private void DrumList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _drum = DrumList.SelectedItem as DrumDefinition;
        _request = null;
        _results.Clear();
        _variables.Clear();
        _prizeTable = null;
        _lastStateResult = null;
        _showWinnerState = false;

        if (_drum is not null)
        {
            foreach (var pair in _drum.Variables)
                _variables[pair.Key] = pair.Value;

            BrowserUrlText.Text = _drum.LoginUrl;
        }

        ClearRequestEditor();
        RenderVariables();
        ClearResultViews();
        UpdateActionAvailability();
    }

    private void UpdateActionAvailability()
    {
        var active = _drum is not null && !_drum.Archived;
        var actions = GetEffectiveActions();

        GetPrizeOptionsButton.IsEnabled =
            active && actions.PrizeOptions.Count > 0 && HasAvailablePipeline(actions.PrizeOptions);
        GetDrumStateButton.IsEnabled =
            active && actions.State.Count > 0 && HasAvailablePipeline(actions.State);
        GetPrizeButton.IsEnabled =
            active && actions.Claim.Count > 0 && HasAvailablePipeline(actions.Claim);
    }

    private HttpRequestDefinition? FindRequest(string id) =>
        _drum?.Requests.FirstOrDefault(x => x.Id.Equals(id, StringComparison.OrdinalIgnoreCase));

    private DrumActionMapping GetEffectiveActions()
    {
        if (_drum?.Actions is not null)
            return _drum.Actions;

        return new DrumActionMapping
        {
            PrizeOptions =
            [
                new() { RequestId = "getOfferById" },
                new() { RequestId = "getCustomerOffersDrum" },
                new() { RequestId = "getOfferDrums" }
            ],
            State =
            [
                new() { RequestId = "getOfferById" },
                new() { RequestId = "getCustomerOffersDrum" }
            ],
            Claim =
            [
                new() { RequestId = "confirmDrumOffer" }
            ],
            PrizeResultRequestId = "getOfferDrums",
            StateResultRequestId = "getCustomerOffersDrum",
            WinnerVariable = _drum?.Result?.WinnerVariable ?? "offerWinId",
            ClaimRequiresWinner = true
        };
    }

    private bool HasAvailablePipeline(IEnumerable<DrumActionStep> steps) =>
        steps.All(step => step.Optional || FindRequest(step.RequestId) is not null);

    private async Task<HttpRunResult?> RunActionPipelineAsync(
        IEnumerable<DrumActionStep> steps,
        CancellationToken cancellationToken = default)
    {
        HttpRunResult? last = null;

        foreach (var step in steps)
        {
            if (!string.IsNullOrWhiteSpace(step.WhenVariable))
            {
                _variables.TryGetValue(step.WhenVariable, out var actual);
                if (!string.Equals(actual ?? "", step.WhenEquals ?? "", StringComparison.OrdinalIgnoreCase))
                    continue;
            }

            if (FindRequest(step.RequestId) is null)
            {
                if (step.Optional)
                    continue;

                throw new InvalidOperationException(
                    $"Запрос '{step.RequestId}' не найден в выбранном модуле.");
            }

            try
            {
                last = await ExecuteRequestAsync(step.RequestId, false, cancellationToken);
            }
            catch when (step.Optional)
            {
            }
        }

        return last;
    }

    private HttpRunResult? GetStoredResult(string requestId)
    {
        if (string.IsNullOrWhiteSpace(requestId))
            return null;

        return _results.TryGetValue(requestId, out var result) ? result : null;
    }

    private void SelectRequestForEditor(HttpRequestDefinition request)
    {
        _request = request;
        RequestNameText.Text = request.Name;
        MethodBox.Text = request.Method;
        UrlText.Text = request.Url;
        HeadersText.Text = string.Join(
            Environment.NewLine,
            request.Headers.Select(x => $"{x.Key}: {x.Value}"));
        BodyText.Text = JsonTextFormatter.PrettyOrOriginal(request.Body);
    }

    private void ClearRequestEditor()
    {
        RequestNameText.Text = "";
        MethodBox.SelectedIndex = -1;
        UrlText.Clear();
        HeadersText.Clear();
        BodyText.Clear();
        ResponseStatusText.Text = "";
        ResponseText.Clear();
        ResponseGrid.ItemsSource = null;
        OperationStatusText.Text = "";
    }

    private void SaveRequest_Click(object sender, RoutedEventArgs e)
    {
        if (_request is null || _drum is null)
            return;

        _request.Method = MethodBox.Text.Trim().ToUpperInvariant();
        _request.Url = UrlText.Text.Trim();
        _request.Headers = ParseHeaders(HeadersText.Text);
        _request.Body = JsonTextFormatter.PrettyOrOriginal(BodyText.Text);
        BodyText.Text = _request.Body;

        var path = Path.Combine(DrumsDirectory, $"{_drum.Id}.json");
        var options = new JsonSerializerOptions(JsonTextFormatter.PrettyOptions)
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        File.WriteAllText(path, JsonSerializer.Serialize(_drum, options));
        OperationStatusText.Text = "Параметры запроса сохранены.";
    }

    private async Task<HttpRunResult> ExecuteRequestAsync(
        string requestId,
        bool renderResponse,
        CancellationToken cancellationToken = default)
    {
        var request = FindRequest(requestId)
            ?? throw new InvalidOperationException($"Запрос '{requestId}' не найден в выбранном барабане.");

        SelectRequestForEditor(request);
        var result = await _executor.SendAsync(request, _session, _variables, cancellationToken);
        _results[request.Id] = result;
        WorkflowRunner.Capture(request, result.ResponseBody, _variables);

        if (renderResponse)
            RenderHttpResponse(result);

        RenderVariables();
        return result;
    }

    private async void GetPrizeOptions_Click(object sender, RoutedEventArgs e)
    {
        if (!CanUseActiveDrum())
            return;

        try
        {
            OperationStatusText.Text = "Получаю варианты призов...";
            _showWinnerState = false;
            ApplyVariablesFromText();
            await SyncBrowserSessionIfReadyAsync();

            var actions = GetEffectiveActions();
            var last = await RunActionPipelineAsync(actions.PrizeOptions);
            var response = GetStoredResult(actions.PrizeResultRequestId) ?? last;

            RenderPrizeOptions();
            if (response is not null)
                RenderHttpResponse(response);

            OperationStatusText.Text = _prizeTable is { Rows.Count: > 0 }
                ? $"Получено вариантов: {_prizeTable.Rows.Count}."
                : "Ответ получен, но варианты призов не распознаны.";
        }
        catch (Exception ex)
        {
            ShowOperationError(ex);
        }
    }

    private async void GetDrumState_Click(object sender, RoutedEventArgs e)
    {
        if (!CanUseActiveDrum())
            return;

        try
        {
            OperationStatusText.Text = "Получаю состояние барабана...";
            ApplyVariablesFromText();
            await SyncBrowserSessionIfReadyAsync();

            var actions = GetEffectiveActions();

            if (_prizeTable is null || _prizeTable.Rows.Count == 0)
                await RunActionPipelineAsync(actions.PrizeOptions);

            var last = await RunActionPipelineAsync(actions.State);
            var state = GetStoredResult(actions.StateResultRequestId) ?? last;

            _showWinnerState = true;
            RenderPrizeOptions();

            if (state is not null)
            {
                RenderDrumState(state);
                RenderHttpResponse(state);
            }
            else
            {
                CurrentPrizeText.Text = "Состояние барабана не вернуло данных.";
                StateGrid.ItemsSource = null;
            }

            OperationStatusText.Text = "Состояние барабана получено.";
        }
        catch (Exception ex)
        {
            ShowOperationError(ex);
        }
    }

    private async void GetPrize_Click(object sender, RoutedEventArgs e)
    {
        if (!CanUseActiveDrum())
            return;

        try
        {
            ApplyVariablesFromText();
            await SyncBrowserSessionIfReadyAsync();

            var actions = GetEffectiveActions();

            if (_prizeTable is null || _prizeTable.Rows.Count == 0)
            {
                await RunActionPipelineAsync(actions.PrizeOptions);
                RenderPrizeOptions();
            }

            _variables.TryGetValue(actions.WinnerVariable, out var winnerId);

            if (actions.ClaimRequiresWinner && string.IsNullOrWhiteSpace(winnerId))
            {
                var stateLast = await RunActionPipelineAsync(actions.State);
                var state = GetStoredResult(actions.StateResultRequestId) ?? stateLast;

                _showWinnerState = true;
                RenderPrizeOptions();

                if (state is not null)
                    RenderDrumState(state);

                _variables.TryGetValue(actions.WinnerVariable, out winnerId);
            }

            if (actions.ClaimRequiresWinner && string.IsNullOrWhiteSpace(winnerId))
                throw new InvalidOperationException("Сервер не вернул идентификатор текущего приза.");

            var title = ResultProjector.ResolvePrizeTitle(_prizeTable, winnerId);
            var display = string.IsNullOrWhiteSpace(winnerId)
                ? "Приз будет выбран сервером после подтверждения."
                : string.IsNullOrWhiteSpace(title)
                    ? $"ID сектора {winnerId}"
                    : $"{title} (ID сектора {winnerId})";

            var confirmation = MessageBox.Show(
                this,
                $"Отправить запрос на получение приза?\n\n{display}",
                "Получить приз",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirmation != MessageBoxResult.Yes)
                return;

            OperationStatusText.Text = "Отправляю запрос на получение приза...";
            var claim = await RunActionPipelineAsync(actions.Claim);

            _showWinnerState = true;
            RenderVariables();
            RenderPrizeOptions();

            if (claim is not null)
            {
                RenderDrumState(claim);
                RenderHttpResponse(claim);
                OperationStatusText.Text =
                    $"Запрос получения приза отправлен: HTTP {claim.StatusCode}.";
            }
            else
            {
                OperationStatusText.Text = "Запрос получения приза выполнен.";
            }
        }
        catch (Exception ex)
        {
            ShowOperationError(ex);
        }
    }

    private bool CanUseActiveDrum()
    {
        if (_drum is null)
            return false;

        if (!_drum.Archived)
            return true;

        MessageBox.Show(
            this,
            "Этот барабан сохранён как архивный. Рабочие запросы для него отключены.",
            "Архивный барабан",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
        return false;
    }

    private void ShowOperationError(Exception ex)
    {
        OperationStatusText.Text = "Ошибка.";
        ResponseStatusText.Text = "Ошибка";
        ResponseText.Text = ex.Message;
        ResponseGrid.ItemsSource = null;
        MessageBox.Show(this, ex.Message, "Ошибка запроса", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    private void RenderHttpResponse(HttpRunResult result)
    {
        ResponseStatusText.Text = $"HTTP {result.StatusCode} {result.ReasonPhrase}";
        ResponseText.Text = JsonTextFormatter.PrettyOrOriginal(result.ResponseBody);
        ResponseGrid.ItemsSource = JsonTableProjector.Build(result.ResponseBody).DefaultView;
    }

    private void RenderPrizeOptions()
    {
        if (_drum is null)
            return;

        IReadOnlyDictionary<string, string> projectionVariables = _variables;
        Dictionary<string, string>? withoutWinner = null;

        if (!_showWinnerState && _drum.Result is not null)
        {
            withoutWinner = new Dictionary<string, string>(_variables, StringComparer.OrdinalIgnoreCase)
            {
                [_drum.Result.WinnerVariable] = ""
            };
            projectionVariables = withoutWinner;
        }

        _prizeTable = ResultProjector.Build(_drum, _results, projectionVariables);
        PrizeOptionsGrid.ItemsSource = _prizeTable.DefaultView;
        ResultGrid.ItemsSource = _prizeTable.DefaultView;

        if (_prizeTable.Rows.Count == 0)
        {
            ResultEmptyText.Text = _results.ContainsKey(_drum.Result?.SourceRequestId ?? "")
                ? "Ответ получен, но список призов не удалось распознать."
                : "Результат появится здесь после запуска цепочки.";
            ResultEmptyText.Visibility = Visibility.Visible;
        }
        else
        {
            ResultEmptyText.Visibility = Visibility.Collapsed;
        }
    }

    private void RenderDrumState(HttpRunResult state)
    {
        StateGrid.ItemsSource = JsonTableProjector.Build(state.ResponseBody).DefaultView;

        var actions = GetEffectiveActions();
        _variables.TryGetValue(actions.WinnerVariable, out var winnerId);
        var title = ResultProjector.ResolvePrizeTitle(_prizeTable, winnerId);

        CurrentPrizeText.Text = string.IsNullOrWhiteSpace(winnerId)
            ? "Текущий приз не определён."
            : string.IsNullOrWhiteSpace(title)
                ? $"Текущий приз: ID сектора {winnerId}"
                : $"Текущий приз: {title}   •   ID сектора {winnerId}";
    }

    private async void RunWorkflow_Click(object sender, RoutedEventArgs e)
    {
        if (!CanUseActiveDrum() || _drum is null)
            return;

        ApplyVariablesFromText();
        _results.Clear();
        _prizeTable = null;
        _showWinnerState = true;
        ResultGrid.ItemsSource = null;
        PrizeOptionsGrid.ItemsSource = null;
        ResultEmptyText.Text = "Выполняется цепочка...";
        ResultEmptyText.Visibility = Visibility.Visible;

        try
        {
            await SyncBrowserSessionIfReadyAsync();

            var runner = new WorkflowRunner(_executor);
            var results = await runner.RunAsync(_drum, _session, _variables);

            foreach (var result in results)
                _results[result.RequestId] = result;

            RenderVariables();
            RenderPrizeOptions();

            var actions = GetEffectiveActions();
            var state = GetStoredResult(actions.StateResultRequestId);
            if (state is not null)
            {
                _lastStateResult = state;
                RenderDrumState(state);
            }

            var prizes = GetStoredResult(actions.PrizeResultRequestId);
            if (prizes is not null)
                RenderHttpResponse(prizes);
        }
        catch (Exception ex)
        {
            ResultEmptyText.Text = "Не удалось получить результат.";
            ResultEmptyText.Visibility = Visibility.Visible;
            MessageBox.Show(this, ex.Message, "Ошибка цепочки", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ClearResultViews()
    {
        ResultGrid.ItemsSource = null;
        PrizeOptionsGrid.ItemsSource = null;
        StateGrid.ItemsSource = null;
        CurrentPrizeText.Text = "Текущий приз ещё не запрошен.";
        ResultEmptyText.Text = "Результат появится здесь после запуска цепочки.";
        ResultEmptyText.Visibility = Visibility.Visible;
    }

    private void ResultGrid_LoadingRow(object sender, DataGridRowEventArgs e)
    {
        if (e.Row.Item is DataRowView row &&
            row.Row.Table.Columns.Contains("IsWinner") &&
            row["IsWinner"] is true)
        {
            e.Row.Background = Brushes.Honeydew;
            e.Row.FontWeight = FontWeights.Bold;
        }
    }

    private void RenderVariables() =>
        VariablesText.Text = JsonSerializer.Serialize(_variables, JsonTextFormatter.PrettyOptions);

    private void ApplyVariables_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            ApplyVariablesFromText();
            RenderVariables();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Некорректный JSON переменных", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ApplyVariablesFromText()
    {
        var parsed = JsonSerializer.Deserialize<Dictionary<string, string>>(VariablesText.Text) ?? [];
        _variables.Clear();

        foreach (var pair in parsed)
            _variables[pair.Key] = pair.Value ?? "";
    }

    private void RenderSession() =>
        SessionText.Text = _sessionStore.ExportEditable(_session);

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
        foreach (var part in dialog.CookieHeader.Split(
                     ';',
                     StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var index = part.IndexOf('=');
            if (index <= 0)
                continue;

            var name = part[..index].Trim();
            var value = part[(index + 1)..].Trim();

            _session.Cookies.RemoveAll(c =>
                c.Name.Equals(name, StringComparison.OrdinalIgnoreCase) &&
                c.Domain.Equals(domain, StringComparison.OrdinalIgnoreCase));

            _session.Cookies.Add(new StoredCookie
            {
                Name = name,
                Value = value,
                Domain = domain,
                Path = "/"
            });
        }

        _sessionStore.Save(_session);

        if (_browserBridge is not null)
            await _browserBridge.RestoreSessionToBrowserAsync(Browser, _session);

        RenderSession();
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
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                this,
                ex.Message,
                "Не удалось импортировать capture ZIP",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void ClearSession_Click(object sender, RoutedEventArgs e)
    {
        _session = new SessionProfile();
        _sessionStore.Clear();
        RenderSession();
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
    }

    private async Task SyncBrowserSessionIfReadyAsync()
    {
        if (_browserBridge is null || Browser.CoreWebView2 is null)
            return;

        await _browserBridge.SyncCookiesFromBrowserAsync(Browser, _session);
        RenderSession();
    }

    private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control) && e.Key == Key.F)
        {
            OpenFind();
            e.Handled = true;
            return;
        }

        if (e.Key == Key.F3)
        {
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
                MoveFind(-1);
            else
                MoveFind(1);

            e.Handled = true;
        }
    }

    private void OpenFind()
    {
        FindPanel.Visibility = Visibility.Visible;
        FindText.Focus();
        FindText.SelectAll();
        RebuildFindMatches();
    }

    private void CloseFind_Click(object sender, RoutedEventArgs e)
    {
        FindPanel.Visibility = Visibility.Collapsed;
        _findMatches.Clear();
        _findMatchIndex = -1;
        FindStatusText.Text = "";
    }

    private void FindText_TextChanged(object sender, TextChangedEventArgs e)
    {
        RebuildFindMatches();
        if (_findMatches.Count > 0)
            ActivateFindMatch(0);
    }

    private void FindText_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            MoveFind(Keyboard.Modifiers.HasFlag(ModifierKeys.Shift) ? -1 : 1);
            e.Handled = true;
        }
        else if (e.Key == Key.Escape)
        {
            CloseFind_Click(sender, e);
            e.Handled = true;
        }
    }

    private void FindNext_Click(object sender, RoutedEventArgs e) => MoveFind(1);
    private void FindPrevious_Click(object sender, RoutedEventArgs e) => MoveFind(-1);

    private void MoveFind(int delta)
    {
        if (_findMatches.Count == 0)
        {
            RebuildFindMatches();
            if (_findMatches.Count == 0)
                return;
        }

        var next = _findMatchIndex + delta;
        if (next < 0)
            next = _findMatches.Count - 1;
        if (next >= _findMatches.Count)
            next = 0;

        ActivateFindMatch(next);
    }

    private void RebuildFindMatches()
    {
        _findMatches.Clear();
        _findMatchIndex = -1;

        var query = FindText.Text;
        if (string.IsNullOrWhiteSpace(query))
        {
            FindStatusText.Text = "";
            return;
        }

        foreach (var element in EnumerateVisualTree(MainContentGrid))
        {
            if (element is not FrameworkElement frameworkElement || !frameworkElement.IsVisible)
                continue;

            switch (element)
            {
                case TextBox textBox:
                    AddTextBoxMatches(textBox, query);
                    break;

                case DataGrid dataGrid:
                    AddDataGridMatches(dataGrid, query);
                    break;

                case ListBox listBox:
                    AddListBoxMatches(listBox, query);
                    break;

                case TextBlock textBlock when
                    textBlock.Text.Contains(query, StringComparison.OrdinalIgnoreCase):
                    _findMatches.Add(() => textBlock.BringIntoView());
                    break;
            }
        }

        FindStatusText.Text = _findMatches.Count == 0
            ? "Ничего не найдено"
            : $"Найдено: {_findMatches.Count}";
    }

    private void AddTextBoxMatches(TextBox textBox, string query)
    {
        var start = 0;
        while (start < textBox.Text.Length)
        {
            var index = textBox.Text.IndexOf(query, start, StringComparison.OrdinalIgnoreCase);
            if (index < 0)
                break;

            var capturedIndex = index;
            _findMatches.Add(() =>
            {
                textBox.Focus();
                textBox.Select(capturedIndex, query.Length);
                textBox.BringIntoView();
            });

            start = index + Math.Max(1, query.Length);
        }
    }

    private void AddDataGridMatches(DataGrid dataGrid, string query)
    {
        foreach (var item in dataGrid.Items)
        {
            var text = ItemSearchText(item);
            if (!text.Contains(query, StringComparison.OrdinalIgnoreCase))
                continue;

            var captured = item;
            _findMatches.Add(() =>
            {
                dataGrid.SelectedItem = captured;
                dataGrid.ScrollIntoView(captured);
                dataGrid.Focus();
            });
        }
    }

    private void AddListBoxMatches(ListBox listBox, string query)
    {
        foreach (var item in listBox.Items)
        {
            if (!(item?.ToString() ?? "").Contains(query, StringComparison.OrdinalIgnoreCase))
                continue;

            var captured = item;
            _findMatches.Add(() =>
            {
                listBox.SelectedItem = captured;
                listBox.ScrollIntoView(captured);
                listBox.Focus();
            });
        }
    }

    private static string ItemSearchText(object item)
    {
        if (item is DataRowView row)
            return string.Join(" ", row.Row.ItemArray.Select(Convert.ToString));

        return item?.ToString() ?? "";
    }

    private static IEnumerable<DependencyObject> EnumerateVisualTree(DependencyObject root)
    {
        yield return root;

        var count = VisualTreeHelper.GetChildrenCount(root);
        for (var i = 0; i < count; i++)
        {
            foreach (var child in EnumerateVisualTree(VisualTreeHelper.GetChild(root, i)))
                yield return child;
        }
    }

    private void ActivateFindMatch(int index)
    {
        if (index < 0 || index >= _findMatches.Count)
            return;

        _findMatchIndex = index;
        _findMatches[index]();
        FindStatusText.Text = $"{index + 1} из {_findMatches.Count}";
    }

    private static Dictionary<string, string> ParseHeaders(string raw)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var line in raw.Split(
                     ["\r\n", "\n"],
                     StringSplitOptions.RemoveEmptyEntries))
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
