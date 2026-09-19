using DataHandlerLibrary.Models;
using DataHandlerLibrary.Services;
using EposRetail.Models;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace EposRetail.Services
{
    public class TeyaPosLinkService
    {
#if DEBUG
        private const string TeyaConfigurationSection = "TeyaSandbox";
#else
        private const string TeyaConfigurationSection = "TeyaProduction";
#endif

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly IConfiguration _configuration;
        private readonly AzureKeyVaultSecretProvider _keyVaultSecretProvider;
        private readonly PaymentTerminalSettingsServices _paymentTerminalSettingsServices;
        private readonly UserSessionService _userSessionService;

        public TeyaPosLinkService(
            IConfiguration configuration,
            AzureKeyVaultSecretProvider keyVaultSecretProvider,
            PaymentTerminalSettingsServices paymentTerminalSettingsServices,
            UserSessionService userSessionService)
        {
            _configuration = configuration;
            _keyVaultSecretProvider = keyVaultSecretProvider;
            _paymentTerminalSettingsServices = paymentTerminalSettingsServices;
            _userSessionService = userSessionService;
        }

        public async Task<TeyaDeviceAuthorizationResponse> BeginDeviceAuthorizationAsync(CancellationToken cancellationToken = default)
        {
            var credentials = await _keyVaultSecretProvider.GetTeyaPartnerCredentialsAsync();

            using var client = CreateHttpClient();
            using var response = await client.PostAsync(
                $"{GetIdentityBaseUrl()}/oauth/v2/device",
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["client_id"] = credentials.ClientId,
                    ["client_secret"] = credentials.ClientSecret
                }),
                cancellationToken);

            await EnsureSuccessAsync(response);
            return await DeserializeRequiredAsync<TeyaDeviceAuthorizationResponse>(response, cancellationToken);
        }

        public async Task<TeyaTokenResponse> PollForDeviceAuthorizationAsync(TeyaDeviceAuthorizationResponse deviceAuthorization, CancellationToken cancellationToken = default)
        {
            var credentials = await _keyVaultSecretProvider.GetTeyaPartnerCredentialsAsync();
            var intervalSeconds = Math.Max(2, deviceAuthorization.IntervalSeconds);
            var expiryUtc = DateTime.UtcNow.AddSeconds(deviceAuthorization.ExpiresInSeconds);

            using var client = CreateHttpClient();

            while (DateTime.UtcNow < expiryUtc)
            {
                cancellationToken.ThrowIfCancellationRequested();

                using var response = await client.PostAsync(
                    $"{GetIdentityBaseUrl()}/oauth/v2/oauth-token",
                    new FormUrlEncodedContent(new Dictionary<string, string>
                    {
                        ["grant_type"] = "urn:ietf:params:oauth:grant-type:device_code",
                        ["device_code"] = deviceAuthorization.DeviceCode,
                        ["client_id"] = credentials.ClientId,
                        ["client_secret"] = credentials.ClientSecret
                    }),
                    cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    return await DeserializeRequiredAsync<TeyaTokenResponse>(response, cancellationToken);
                }

                var error = await DeserializeOptionalAsync<TeyaOAuthErrorResponse>(response, cancellationToken);
                var errorCode = error?.Error?.Trim().ToLowerInvariant();

                if (response.StatusCode == HttpStatusCode.BadRequest && errorCode == "authorization_pending")
                {
                    await Task.Delay(TimeSpan.FromSeconds(intervalSeconds), cancellationToken);
                    continue;
                }

                if (response.StatusCode == HttpStatusCode.BadRequest && errorCode == "slow_down")
                {
                    intervalSeconds += 5;
                    await Task.Delay(TimeSpan.FromSeconds(intervalSeconds), cancellationToken);
                    continue;
                }

                if (response.StatusCode == HttpStatusCode.BadRequest && errorCode == "access_denied")
                {
                    throw new InvalidOperationException("Merchant declined the Teya connection request.");
                }

                if (response.StatusCode == HttpStatusCode.BadRequest && errorCode == "expired_token")
                {
                    throw new InvalidOperationException("The Teya connection request expired before the merchant approved it.");
                }

                await EnsureSuccessAsync(response);
            }

            throw new TimeoutException("Timed out waiting for merchant approval from Teya.");
        }

        public async Task<List<TeyaStoreSummary>> GetStoresAsync(string accessToken, CancellationToken cancellationToken = default)
        {
            using var client = CreateAuthorizedHttpClient(accessToken);
            using var response = await client.GetAsync($"{GetApiBaseUrl()}/poslink/v1/stores", cancellationToken);
            await EnsureSuccessAsync(response);
            return (await DeserializeRequiredAsync<TeyaStoresResponse>(response, cancellationToken)).Stores;
        }

        public async Task<List<TeyaTerminalSummary>> GetTerminalsAsync(string accessToken, string storeId, CancellationToken cancellationToken = default)
        {
            using var client = CreateAuthorizedHttpClient(accessToken);
            using var response = await client.GetAsync($"{GetApiBaseUrl()}/poslink/v1/stores/{Uri.EscapeDataString(storeId)}/terminals", cancellationToken);
            await EnsureSuccessAsync(response);
            return (await DeserializeRequiredAsync<TeyaTerminalsResponse>(response, cancellationToken)).Terminals;
        }

        public async Task<TeyaStoreConfigResponse> SetPayAtCounterAsync(string accessToken, string storeId, bool isEnabled, CancellationToken cancellationToken = default)
        {
            using var client = CreateAuthorizedHttpClient(accessToken);
            using var response = await client.PutAsync(
                $"{GetApiBaseUrl()}/poslink/v1/stores/{Uri.EscapeDataString(storeId)}/configs/PAT_ENABLED",
                CreateJsonContent(new { value = isEnabled ? "true" : "false" }),
                cancellationToken);

            await EnsureSuccessAsync(response);
            return await DeserializeRequiredAsync<TeyaStoreConfigResponse>(response, cancellationToken);
        }

        public async Task<List<TeyaStoreConfigResponse>> GetTerminalConfigsAsync(string accessToken, string storeId, string terminalId, CancellationToken cancellationToken = default)
        {
            using var client = CreateAuthorizedHttpClient(accessToken);
            using var response = await client.GetAsync(
                $"{GetApiBaseUrl()}/poslink/v1/stores/{Uri.EscapeDataString(storeId)}/terminals/{Uri.EscapeDataString(terminalId)}/configs",
                cancellationToken);

            await EnsureSuccessAsync(response);
            return (await DeserializeRequiredAsync<TeyaTerminalConfigsResponse>(response, cancellationToken)).Configs;
        }

        public async Task<TeyaPaymentProcessingResult> ProcessPaymentForCurrentSessionAsync(
            decimal amount,
            string currency,
            string basketTransactionId,
            CancellationToken cancellationToken = default)
        {
            var setting = await _paymentTerminalSettingsServices.GetEffectiveSettingAsync(
                _userSessionService.GetCurrentSiteId(),
                _userSessionService.GetCurrentTillId());

            if (setting == null || !setting.Is_Enabled)
            {
                return new TeyaPaymentProcessingResult
                {
                    IsConfigured = false,
                    IsSuccess = false,
                    Message = "Teya card terminal is not configured for this site or till."
                };
            }

            if (string.IsNullOrWhiteSpace(setting.Store_Id) ||
                string.IsNullOrWhiteSpace(setting.Terminal_Id) ||
                string.IsNullOrWhiteSpace(setting.Access_Token) ||
                string.IsNullOrWhiteSpace(setting.Refresh_Token))
            {
                return new TeyaPaymentProcessingResult
                {
                    IsConfigured = true,
                    IsSuccess = false,
                    Message = "Teya configuration is incomplete. Reconnect the merchant terminal from Business Management."
                };
            }

            try
            {
                var paymentRequest = await CreatePaymentRequestAsync(setting, amount, currency, basketTransactionId, cancellationToken);
                var finalStatus = await WaitForFinalPaymentStatusAsync(setting, paymentRequest.PaymentRequestId, cancellationToken);

                var succeeded = string.Equals(finalStatus.Status, "SUCCESSFUL", StringComparison.OrdinalIgnoreCase);
                return new TeyaPaymentProcessingResult
                {
                    IsConfigured = true,
                    IsSuccess = succeeded,
                    Message = succeeded
                        ? "Card payment approved by Teya terminal."
                        : $"Teya terminal returned {finalStatus.Status}{(string.IsNullOrWhiteSpace(finalStatus.StatusReason) ? string.Empty : $": {finalStatus.StatusReason}")}",
                    PaymentRequestId = finalStatus.PaymentRequestId,
                    GatewayPaymentId = finalStatus.GatewayPaymentId,
                    FinalStatus = finalStatus.Status,
                    StatusReason = finalStatus.StatusReason
                };
            }
            catch (Exception ex)
            {
                return new TeyaPaymentProcessingResult
                {
                    IsConfigured = true,
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<TeyaPaymentRequestResponse> CancelPaymentForCurrentSessionAsync(
            string paymentRequestId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(paymentRequestId))
            {
                throw new ArgumentException("Payment request id is required.", nameof(paymentRequestId));
            }

            var setting = await _paymentTerminalSettingsServices.GetEffectiveSettingAsync(
                _userSessionService.GetCurrentSiteId(),
                _userSessionService.GetCurrentTillId());

            if (setting == null || !setting.Is_Enabled)
            {
                throw new InvalidOperationException("Teya card terminal is not configured for this site or till.");
            }

            setting = await EnsureFreshAccessTokenAsync(setting, cancellationToken);

            async Task<HttpResponseMessage> SendAsync(string accessToken)
            {
                using var client = CreateAuthorizedHttpClient(accessToken);
                return await client.PutAsync(
                    $"{GetApiBaseUrl()}/poslink/v2/payment-requests/{Uri.EscapeDataString(paymentRequestId)}",
                    CreateJsonContent(new TeyaPaymentStatusUpdateRequest()),
                    cancellationToken);
            }

            using var response = await SendAsync(setting.Access_Token!);
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                setting = await ForceRefreshAccessTokenAsync(setting, cancellationToken);
                using var retryResponse = await SendAsync(setting.Access_Token!);
                await EnsureSuccessAsync(retryResponse);
                return await DeserializeRequiredAsync<TeyaPaymentRequestResponse>(retryResponse, cancellationToken);
            }

            await EnsureSuccessAsync(response);
            return await DeserializeRequiredAsync<TeyaPaymentRequestResponse>(response, cancellationToken);
        }

        private async Task<TeyaPaymentRequestResponse> CreatePaymentRequestAsync(
            PaymentTerminalSetting setting,
            decimal amount,
            string currency,
            string basketTransactionId,
            CancellationToken cancellationToken)
        {
            setting = await EnsureFreshAccessTokenAsync(setting, cancellationToken);

            async Task<HttpResponseMessage> SendAsync(string accessToken)
            {
                var client = CreateAuthorizedHttpClient(accessToken);
                client.DefaultRequestHeaders.Add("Idempotency-Key", Guid.NewGuid().ToString("N"));

                var requestBody = new
                {
                    store_id = setting.Store_Id,
                    terminal_id = setting.Terminal_Id,
                    requested_amount = new
                    {
                        amount = ConvertToMinorUnits(Math.Abs(amount)),
                        currency = string.IsNullOrWhiteSpace(currency) ? setting.Currency_Code : currency,
                        tip = 0
                    },
                    transaction_type = amount < 0 ? "REFUND" : "SALE",
                    merchant_reference = setting.Store_Id,
                    epos_instance_id = BuildEposInstanceId(setting),
                    basket_transaction_id = basketTransactionId
                };

                return await client.PostAsync(
                    $"{GetApiBaseUrl()}/poslink/v3/payment-requests",
                    CreateJsonContent(requestBody),
                    cancellationToken);
            }

            using var response = await SendAsync(setting.Access_Token!);
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                setting = await ForceRefreshAccessTokenAsync(setting, cancellationToken);
                using var retryResponse = await SendAsync(setting.Access_Token!);
                await EnsureSuccessAsync(retryResponse);
                return await DeserializeRequiredAsync<TeyaPaymentRequestResponse>(retryResponse, cancellationToken);
            }

            await EnsureSuccessAsync(response);
            return await DeserializeRequiredAsync<TeyaPaymentRequestResponse>(response, cancellationToken);
        }

        private async Task<TeyaPaymentRequestResponse> WaitForFinalPaymentStatusAsync(
            PaymentTerminalSetting setting,
            string paymentRequestId,
            CancellationToken cancellationToken)
        {
            var expiryUtc = DateTime.UtcNow.AddMinutes(2);

            while (DateTime.UtcNow < expiryUtc)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var requests = await GetRecentPaymentRequestsAsync(setting, cancellationToken);
                var current = requests.FirstOrDefault(x => x.PaymentRequestId == paymentRequestId);

                if (current != null && IsFinalStatus(current.Status))
                {
                    return current;
                }

                await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken);
            }

            throw new TimeoutException("Timed out waiting for the Teya terminal to complete the payment.");
        }

        private async Task<List<TeyaPaymentRequestResponse>> GetRecentPaymentRequestsAsync(PaymentTerminalSetting setting, CancellationToken cancellationToken)
        {
            setting = await EnsureFreshAccessTokenAsync(setting, cancellationToken);

            async Task<HttpResponseMessage> SendAsync(string accessToken)
            {
                var client = CreateAuthorizedHttpClient(accessToken);
                var url =
                    $"{GetApiBaseUrl()}/poslink/v2/payment-requests?store_id={Uri.EscapeDataString(setting.Store_Id!)}" +
                    $"&terminal_id={Uri.EscapeDataString(setting.Terminal_Id!)}&limit=20&sort=DESC";
                return await client.GetAsync(url, cancellationToken);
            }

            using var response = await SendAsync(setting.Access_Token!);
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                setting = await ForceRefreshAccessTokenAsync(setting, cancellationToken);
                using var retryResponse = await SendAsync(setting.Access_Token!);
                await EnsureSuccessAsync(retryResponse);
                return (await DeserializeRequiredAsync<TeyaPaymentRequestsResponse>(retryResponse, cancellationToken)).PaymentRequests;
            }

            await EnsureSuccessAsync(response);
            return (await DeserializeRequiredAsync<TeyaPaymentRequestsResponse>(response, cancellationToken)).PaymentRequests;
        }

        private async Task<PaymentTerminalSetting> EnsureFreshAccessTokenAsync(PaymentTerminalSetting setting, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrWhiteSpace(setting.Access_Token) &&
                setting.Access_Token_Expires_At.HasValue &&
                setting.Access_Token_Expires_At.Value > DateTime.UtcNow.AddMinutes(2))
            {
                return setting;
            }

            return await ForceRefreshAccessTokenAsync(setting, cancellationToken);
        }

        private async Task<PaymentTerminalSetting> ForceRefreshAccessTokenAsync(PaymentTerminalSetting setting, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(setting.Refresh_Token))
            {
                throw new InvalidOperationException("Teya refresh token is missing. Reconnect the merchant account.");
            }

            var credentials = await _keyVaultSecretProvider.GetTeyaPartnerCredentialsAsync();
            using var client = CreateHttpClient();

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                "Basic",
                Convert.ToBase64String(Encoding.UTF8.GetBytes($"{credentials.ClientId}:{credentials.ClientSecret}")));

            using var response = await client.PostAsync(
                $"{GetIdentityBaseUrl()}/oauth/v2/oauth-token",
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["grant_type"] = "refresh_token",
                    ["refresh_token"] = setting.Refresh_Token
                }),
                cancellationToken);

            await EnsureSuccessAsync(response);
            var refreshedTokens = await DeserializeRequiredAsync<TeyaTokenResponse>(response, cancellationToken);

            setting.Access_Token = refreshedTokens.AccessToken;
            setting.Refresh_Token = refreshedTokens.RefreshToken;
            setting.Access_Token_Expires_At = DateTime.UtcNow.AddSeconds(refreshedTokens.ExpiresInSeconds);
            setting.Last_Modified = DateTime.UtcNow;
            setting.Last_Known_Status = "Refreshed";
            setting.SyncStatus = SyncStatus.Pending;

            await _paymentTerminalSettingsServices.UpsertForScopeAsync(setting);
            return setting;
        }

        private HttpClient CreateHttpClient()
        {
            return new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(60)
            };
        }

        private HttpClient CreateAuthorizedHttpClient(string accessToken)
        {
            var client = CreateHttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return client;
        }

        private string GetApiBaseUrl()
        {
            return (_configuration[$"{TeyaConfigurationSection}:ApiBaseUrl"] ?? "https://api.teya.com").TrimEnd('/');
        }

        private string GetIdentityBaseUrl()
        {
            return (_configuration[$"{TeyaConfigurationSection}:OAuthBaseUrl"] ?? "https://id.teya.com").TrimEnd('/');
        }

        private static StringContent CreateJsonContent(object value)
        {
            return new StringContent(JsonSerializer.Serialize(value), Encoding.UTF8, "application/json");
        }

        private static int ConvertToMinorUnits(decimal amount)
        {
            return (int)Math.Round(amount * 100m, MidpointRounding.AwayFromZero);
        }

        private static string BuildEposInstanceId(PaymentTerminalSetting setting)
        {
            if (!string.IsNullOrWhiteSpace(setting.Epos_Instance_Id))
            {
                return setting.Epos_Instance_Id;
            }

            var machineName = Environment.MachineName.ToLowerInvariant();
            if (setting.Till_Id.HasValue)
            {
                return $"kaninify-{machineName}-till-{setting.Till_Id.Value}";
            }

            return $"kaninify-{machineName}-site-{setting.Site_Id ?? 0}";
        }

        private static bool IsFinalStatus(string? status)
        {
            return string.Equals(status, "SUCCESSFUL", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(status, "FAILED", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(status, "CANCELLED", StringComparison.OrdinalIgnoreCase);
        }

        private static async Task EnsureSuccessAsync(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                return;
            }

            var body = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(body))
            {
                throw new InvalidOperationException($"Teya request failed with status {(int)response.StatusCode}.");
            }

            throw new InvalidOperationException($"Teya request failed with status {(int)response.StatusCode}: {body}");
        }

        private static async Task<T> DeserializeRequiredAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            var model = await DeserializeOptionalAsync<T>(response, cancellationToken);
            return model ?? throw new InvalidOperationException($"Unable to deserialize {typeof(T).Name} from Teya response.");
        }

        private static async Task<T?> DeserializeOptionalAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            if (stream == null || stream.Length == 0)
            {
                return default;
            }

            return await JsonSerializer.DeserializeAsync<T>(stream, JsonOptions, cancellationToken);
        }
    }
}
