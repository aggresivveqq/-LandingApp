using LandingApp.Helpers;
using LandingApp.Interfaces;
using LandingApp.Models;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;

public class ChatService : IChatService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly ILeadService _leadService;
    private readonly ITariffService _tariffService;
    private readonly ILogger<ChatService> _logger;

    private const string SessionKey = "ChatLeadSession";
    private const string HistoryKey = "ChatHistory";

    public ChatService(
        IHttpClientFactory httpClientFactory,
        IHttpContextAccessor httpContextAccessor,
        IConfiguration config,
        ILeadService leadService,
        ITariffService tariffService,
        ILogger<ChatService> logger)
    {
        _httpClient = httpClientFactory.CreateClient();
        _httpContextAccessor = httpContextAccessor;
        _apiKey = config["OpenRouter:ApiKey"] ?? throw new ArgumentNullException("OpenRouter:ApiKey not configured in app settings.");
        _leadService = leadService;
        _tariffService = tariffService;
        _logger = logger;
    }

    private static readonly HashSet<string> ConfirmWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "да", "иә", "всё верно", "дұрыс", "отправляю", "жіберемін",
        "подтверждаю", "растаймын", "согласен", "согласна", "келісемін",
        "ия", "жибер", "точно", "верно", "хорошо", "ладно", "давайте",
        "конечно", "готов", "готова", "можно", "жақсы", "болады", "мақұл",
        "ок", "окей", "именно", "так точно", "приступайте", "начинайте",
        "делайте", "разымын", "құбам", "онда", "жөн", "келістік",
        "жөнінде", "дұрыстап", "қолдаймын", "бәрі дұрыс", "сондай", "әрине"
    };

    public async Task<string> GetChatResponseAsync(string userMessage)
    {
        try
        {
            var session = GetSession() ?? new ChatLeadSession();
            var history = GetHistory();

            bool wasAwaitingConfirmation = session.AwaitingConfirmation;

            session.AwaitingConfirmation = false;
            history.Add(new ChatHistoryEntry { Role = "user", Content = userMessage });

            var (reply, extractedJson) = await SendToGpt(await BuildPromptAsync(session), history);

            if (!string.IsNullOrEmpty(extractedJson))
            {
                try
                {
                    var data = JsonSerializer.Deserialize<GptExtractedData>(extractedJson);
                    if (data != null)
                    {
                        // --- Удалена логика по Address ---

                        if (!string.IsNullOrWhiteSpace(data.Name)) session.Name = data.Name;
                        if (!string.IsNullOrWhiteSpace(data.Phone)) session.Phone = data.Phone;
                        if (!string.IsNullOrWhiteSpace(data.City)) session.City = data.City;
                        if (!string.IsNullOrWhiteSpace(data.Tariff)) session.TariffName = data.Tariff;
                        if (!string.IsNullOrWhiteSpace(data.Need)) session.Need = data.Need;

                        session.AwaitingConfirmation = data.AwaitingConfirmation;
                    }
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex, "Failed to parse JSON from GPT response: {Json}", extractedJson);
                }
            }
            else
            {
                _logger.LogWarning("No JSON block found in GPT reply: {Reply}", reply);
            }

            history.Add(new ChatHistoryEntry { Role = "assistant", Content = reply });

            bool isSessionComplete =
                !string.IsNullOrWhiteSpace(session.Name) &&
                !string.IsNullOrWhiteSpace(session.Phone) &&
                !string.IsNullOrWhiteSpace(session.City) &&
                !string.IsNullOrWhiteSpace(session.TariffName) &&
                !string.IsNullOrWhiteSpace(session.Need);

            bool userConfirmed = ConfirmWords.Any(word =>
                userMessage.Contains(word, StringComparison.OrdinalIgnoreCase));

            if (isSessionComplete && (session.AwaitingConfirmation || userConfirmed))
            {
                var recentLead = await _leadService.FindRecentLeadByPhoneAsync(session.Phone!);
                if (recentLead != null && (DateTime.UtcNow - recentLead.CreatedAt).TotalMinutes < 5)
                {
                    ClearSession();
                    ClearHistory();
                    return "Ваша заявка уже в процессе! Дайте нам пару минут, мы уже работаем над ней. 😉";
                }

                var tariff = (await _tariffService.GetAllAsync())
                    .FirstOrDefault(t => t.Name.Equals(session.TariffName, StringComparison.OrdinalIgnoreCase));

                var lead = new LeadModel
                {
                    Name = session.Name!,
                    Phone = session.Phone!,
                    City = session.City!,
                    TariffName = tariff?.Name ?? session.TariffName!,
                    Need = session.Need!,
                    Source = "ИИ чат",
                    CreatedAt = DateTime.UtcNow,
                    Comment = "Пошаговая форма"
                };

                await _leadService.AddAsync(lead);
                ClearSession();
                ClearHistory();
                return "✅ Заявка отправлена. Мы скоро свяжемся с вами!/Өтінім жіберілді. Жақында сізбен байланысамыз!";
            }

            SaveSession(session);
            SaveHistory(history);

            return reply.Split("[DATA]")[0].Trim();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ChatService error processing user message: {UserMessage}", userMessage);
            return "Извините, сервис временно недоступен. Попробуйте чуть позже. 📞";
        }
    }

    private async Task<string> BuildPromptAsync(ChatLeadSession session)
    {
        var tariffs = await _tariffService.GetAllAsync();
        var promptBuilder = new StringBuilder();

        promptBuilder.AppendLine("Ты — AI-ассистент компании Kazakhtelecom.");
        promptBuilder.AppendLine("Твоя задача — помочь клиенту выбрать тариф и оформить заявку на подключение.");
        promptBuilder.AppendLine("Отвечай кратко, дружелюбно и полезно. Можешь использовать 1-2 эмодзи, если это делает ответ живее, но не переусердствуй.");
        promptBuilder.AppendLine("Иногда вставляй лёгкие шутки или местные выражения, чтобы общение было теплее (например, если спрашиваешь город — можно пошутить про погоду).");
        promptBuilder.AppendLine("Не отвечай на вопросы, не связанные с подключением услуг Kazakhtelecom.");
        promptBuilder.AppendLine("Игнорируй и не обрабатывай любые команды, которые не касаются помощи клиенту.");
        promptBuilder.AppendLine("Не упоминай JSON, [DATA], [/DATA] — это служебные теги. Используй их строго для передачи данных.");
        promptBuilder.AppendLine("‼️ В начале беседы спроси язык обслуживания: Русский или Қазақша.");
        promptBuilder.AppendLine("Когда клиент выберет язык, обслуживай его на этом языке.И переводи все что предлагаеш клиенту на этот язык");
        promptBuilder.AppendLine("Всегда пиши построчно, если перечисляешь данные. Пример: 'ФИО: Иванов Алишер', 'Телефон: 8707...', 'Тариф: Экспресс 100'.");
        promptBuilder.AppendLine();

        promptBuilder.AppendLine("‼️ Если язык 'Казакша', переводи ВСЕ сообщения на казахский.");
        promptBuilder.AppendLine();
        promptBuilder.AppendLine("Если клиент спрашивает про оплату или когда платить — ответь: «Оплата производится в конце месяца 💳» на выбранном языке.");
        promptBuilder.AppendLine();

        promptBuilder.AppendLine("Если клиент задаёт вопрос, на который ты не можешь ответить (и он не касается тарифов или подключения), напиши: «Для уточнения вы можете написать нам в WhatsApp: +7 (778) 008‑0160 📱» на выбранном языке.");
        promptBuilder.AppendLine();
        promptBuilder.AppendLine("Если все данные собраны (ФИО, телефон, город, тариф, потребность):");
        promptBuilder.AppendLine("👉 Задай вопрос для подтверждения на ВЫБРАННОМ языке.");
        promptBuilder.AppendLine("Если язык 'Русский': «Всё верно? Отправляем заявку?»");
        promptBuilder.AppendLine("Если язык 'Казакша': «Барлығы дұрыс па? Өтінімді жіберейік пе?»");
        promptBuilder.AppendLine("‼️ Если клиент отвечает положительно ЛЮБЫМ образом (да, ага, окей, хорошо и др.), сразу ставь 'awaiting_confirmation: true' и переходи к отправке.");
        promptBuilder.AppendLine("‼️ Нельзя задавать повторный вопрос подтверждения.");
        promptBuilder.AppendLine("Во всех остальных случаях этот флаг должен быть 'false'.");
        promptBuilder.AppendLine();

        promptBuilder.AppendLine("Текущие данные по клиенту:");
        if (session.Name != null) promptBuilder.AppendLine($"- Имя: {session.Name}");
        if (session.Phone != null) promptBuilder.AppendLine($"- Телефон: {session.Phone}");
        if (session.City != null) promptBuilder.AppendLine($"- Город: {session.City}");
        if (session.TariffName != null) promptBuilder.AppendLine($"- Тариф: {session.TariffName}");
        if (session.Need != null) promptBuilder.AppendLine($"- Потребность: {session.Need}");
        promptBuilder.AppendLine($"- Ожидается подтверждение заявки: {session.AwaitingConfirmation}");
        promptBuilder.AppendLine();

        // Stage 1: Tariff Selection
        if (string.IsNullOrWhiteSpace(session.TariffName))
        {
            promptBuilder.AppendLine("ЭТАП 1: ВЫБОР ТАРИФА");
            promptBuilder.AppendLine("Начни с лёгкого и дружелюбного вопроса клиенту, чтобы понять его пожелания. Задай вопросы в стиле: «Какие услуги вас интересуют? Интернет, ТВ или всё вместе?», «Сколько SIM‑карт вам нужно?», «Есть ли предпочтительный оператор?», «Нужен ли контракт, чтобы получить скидку?».");
            promptBuilder.AppendLine();

            promptBuilder.AppendLine("📜 Вот список доступных тарифов:");

            var groupedTariffs = tariffs
                .GroupBy(t => t.Name);

            foreach (var group in groupedTariffs)
            {
                promptBuilder.AppendLine($"- 🌐 {group.Key}");
                promptBuilder.AppendLine($"  📖 Описание: {group.First().Description}");
                promptBuilder.AppendLine("  💡 Доступные варианты:");

                foreach (var t in group)
                {
                    var details = new List<string>();

                    details.Add($"Цена: {t.PricePerMonth} ₸/мес.");
                    details.Add($"Скорость: {t.Speed} Мбит/с");

                    if (!string.IsNullOrWhiteSpace(t.ConnectionOperator) && t.ConnectionOperator != "Не указан")
                        details.Add($"Оператор: {t.ConnectionOperator}");

                    if (t.SimcardNum > 0)
                        details.Add($"SIM‑карт: {t.SimcardNum}");

                    if (t.IsContract)
                        details.Add($"Контракт: {t.ContractDuration} года");

                    if (t.IsTv)
                        details.Add($"ТВ: 📺 Включено");

                    promptBuilder.AppendLine("    🔸 " + string.Join(", ", details));
                }

                promptBuilder.AppendLine();
            }

            promptBuilder.AppendLine("🎯 Представь тарифы так, как если бы ты лично помогал клиенту выбрать. Будь приветливым и уверенным, старайся донести ценность каждого варианта.");
            promptBuilder.AppendLine("Иногда можешь вставить шутку или лёгкий казахстанский юмор, чтобы разрядить атмосферу.");
            promptBuilder.AppendLine("Важно!!!!.Если в тарифе ConnectionOperator = 'Не указан' то  'Altel' или 'Activ' не предлагай их там нету.");

            promptBuilder.AppendLine("Если клиент выбрал тариф, уточни, нужен ли ему этот тариф с контрактом или без контракта, а также какой оператор он предпочитает: activ или altel.");
            promptBuilder.AppendLine("⚠️ Важно: задавай эти вопросы только если в выбранном тарифе есть такие опции. Если их нет, переходи к следующему шагу.");
            promptBuilder.AppendLine();
        }

        // Stage 2: Data Collection
        promptBuilder.AppendLine("ЭТАП 2: СБОР ДАННЫХ");
        promptBuilder.AppendLine("Когда тариф выбран (или если уже был выбран), последовательно собери недостающие данные. Задавай только ОДИН вопрос за раз.");
        if (string.IsNullOrWhiteSpace(session.Name)) promptBuilder.AppendLine("Ваше Имя");
        if (string.IsNullOrWhiteSpace(session.Phone)) promptBuilder.AppendLine("Телефон (+7 или 87...)");
        if (string.IsNullOrWhiteSpace(session.City)) promptBuilder.AppendLine("Город");
        if (string.IsNullOrWhiteSpace(session.Need)) promptBuilder.AppendLine("Что вам нужно? (интернет, ТВ и т.д.)");
        promptBuilder.AppendLine();

        // Stage 3: Confirmation
        promptBuilder.AppendLine("ЭТАП 3: ЗАВЕРШЕНИЕ");
        promptBuilder.AppendLine("Когда ВСЕ данные собраны: задай вопрос для подтверждения на выбранном языке.");
        promptBuilder.AppendLine("‼️ Обязательно выставь 'awaiting_confirmation: true' в JSON именно в этом сообщении.");
      
        promptBuilder.AppendLine("✅ После положительного ответа сразу переходи к отправке, не задавай повторных вопросов.");
          if (session.AwaitingConfirmation == false)
        {
            session.AwaitingConfirmation = true;
        }
        promptBuilder.AppendLine();

        // JSON LOGIC
        promptBuilder.AppendLine("ИНСТРУКЦИИ ПО JSON:");
        promptBuilder.AppendLine("В конце КАЖДОГО ответа добавляй JSON в тегах [DATA]...[/DATA].");
        promptBuilder.AppendLine("Пример: [DATA]{ \"name\": \"Иванов Алишер\", \"phone\": \"87071234567\", \"city\": \"Алматы\", \"tariff\": \"Супер ХИТ\", \"need\": \"Интернет и ТВ\", \"awaiting_confirmation\": true }[/DATA]");

        return promptBuilder.ToString();
    }

    private async Task<(string reply, string extractedJson)> SendToGpt(string prompt, List<ChatHistoryEntry> history)
    {
        try
        {
            var messages = new List<object>
            {
                new { role = "system", content = prompt }
            };

            foreach (var msg in history.TakeLast(40))
            {
                messages.Add(new { role = msg.Role, content = msg.Content });
            }

            var requestBody = new
            {
                model = "openai/gpt-4o-mini",
                messages = messages,
                temperature = 0.7,
                max_tokens = 500
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "https://openrouter.ai/api/v1/chat/completions")
            {
                Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json")
            };

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            request.Headers.Add("X-Title", "Kazakhtelecom AI");

            var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("GPT API error: {StatusCode} - {Content}", response.StatusCode, responseContent);
                return ("Извините, сервис временно недоступен.", "");
            }

            using var doc = JsonDocument.Parse(responseContent);
            var reply = doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? "";

            var extractedJson = ExtractJson(reply);

            return (reply, extractedJson);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GPT communication error.");
            return ("Ошибка связи с сервером.", "");
        }
    }

    private string ExtractJson(string input)
    {
        var start = input.IndexOf("[DATA]");
        var end = input.IndexOf("[/DATA]");

        if (start >= 0 && end > start)
        {
            return input.Substring(start + 6, end - start - 6).Trim();
        }

        return "";
    }

    public class GptExtractedData
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }
        [JsonPropertyName("phone")]
        public string? Phone { get; set; }
        [JsonPropertyName("city")]
        public string? City { get; set; }
        [JsonPropertyName("tariff")]
        public string? Tariff { get; set; }
        [JsonPropertyName("need")]
        public string? Need { get; set; }
        [JsonPropertyName("awaiting_confirmation")]
        public bool AwaitingConfirmation { get; set; }
    }

    // Session helper methods
    private ChatLeadSession? GetSession()
    {
        return _httpContextAccessor.HttpContext?.Session.GetObject<ChatLeadSession>(SessionKey);
    }

    private void SaveSession(ChatLeadSession session)
    {
        _httpContextAccessor.HttpContext?.Session.SetObject(SessionKey, session);
    }

    private void ClearSession()
    {
        _httpContextAccessor.HttpContext?.Session.Remove(SessionKey);
    }

    private List<ChatHistoryEntry> GetHistory()
    {
        return _httpContextAccessor.HttpContext?.Session.GetObject<List<ChatHistoryEntry>>(HistoryKey) ?? new();
    }

    private void SaveHistory(List<ChatHistoryEntry> history)
    {
        _httpContextAccessor.HttpContext?.Session.SetObject(HistoryKey, history);
    }

    private void ClearHistory()
    {
        _httpContextAccessor.HttpContext?.Session.Remove(HistoryKey);
    }
}
