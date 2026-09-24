using DataHandlerLibrary.Models;
using DataHandlerLibrary.Models.SupabaseModels;
using DataHandlerLibrary.Services;
using EposRetail.Models;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace EposRetail.Services
{
    public class TeyaPosLinkService
    {
#if DEBUG
        private const string TeyaEnvironment = "Sandbox";
#else
        private const string TeyaEnvironment = "Production";
#endif

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private PaymentIntegrationPartner? _teyaPartner;
        private readonly SupabaseSyncService _supabaseSyncService;
        private readonly PaymentTerminalSettingsServices _paymentTerminalSettingsServices;
        private readonly CardTransactionServices _cardTransactionServices;
        private readonly UserSessionService _userSessionService;

        public TeyaPosLinkService(
            SupabaseSyncService supabaseSyncService,
            PaymentTerminalSettingsServices paymentTerminalSettingsServices,
            UserSessionService userSessionService,
            CardTransactionServices cardTransactionServices)
        {
            _supabaseSyncService = supabaseSyncService;
            _paymentTerminalSettingsServices = paymentTerminalSettingsServices;
            _userSessionService = userSessionService;
            _cardTransactionServices = cardTransactionServices;
        }

        public async Task<TeyaDeviceAuthorizationResponse> BeginDeviceAuthorizationAsync(CancellationToken cancellationToken = default)
        {
            var partner = await GetTeyaPartnerAsync();

            using var client = CreateHttpClient();
            using var response = await client.PostAsync(
                $"{GetIdentityBaseUrl(partner)}/oauth/v2/device",
                CreateFormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["client_id"] = partner.ClientId.ToString(),
                    ["client_secret"] = partner.ClientSecret
                }),
                cancellationToken);

            await EnsureSuccessAsync(response);
            return await DeserializeRequiredAsync<TeyaDeviceAuthorizationResponse>(response, cancellationToken);
        }

        public async Task<TeyaTokenResponse> PollForDeviceAuthorizationAsync(TeyaDeviceAuthorizationResponse deviceAuthorization, CancellationToken cancellationToken = default)
        {
            var partner = await GetTeyaPartnerAsync();
            var intervalSeconds = Math.Max(2, deviceAuthorization.IntervalSeconds);
            var expiryUtc = DateTime.UtcNow.AddSeconds(deviceAuthorization.ExpiresInSeconds);

            using var client = CreateHttpClient();

            while (DateTime.UtcNow < expiryUtc)
            {
                cancellationToken.ThrowIfCancellationRequested();

                using var response = await client.PostAsync(
                    $"{GetIdentityBaseUrl(partner)}/oauth/v2/oauth-token",
                    CreateFormUrlEncodedContent(new Dictionary<string, string>
                    {
                        ["grant_type"] = "urn:ietf:params:oauth:grant-type:device_code",
                        ["device_code"] = deviceAuthorization.DeviceCode,
                        ["client_id"] = partner.ClientId.ToString(),
                        ["client_secret"] = partner.ClientSecret
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
            using var response = await client.GetAsync($"{GetApiBaseUrl(await GetTeyaPartnerAsync())}/poslink/v1/stores", cancellationToken);
            await EnsureSuccessAsync(response);
            return (await DeserializeRequiredAsync<TeyaStoresResponse>(response, cancellationToken)).Stores;
        }

        public async Task<List<TeyaTerminalSummary>> GetTerminalsAsync(string accessToken, string storeId, CancellationToken cancellationToken = default)
        {
            using var client = CreateAuthorizedHttpClient(accessToken);
            using var response = await client.GetAsync($"{GetApiBaseUrl(await GetTeyaPartnerAsync())}/poslink/v1/stores/{Uri.EscapeDataString(storeId)}/terminals", cancellationToken);
            await EnsureSuccessAsync(response);
            return (await DeserializeRequiredAsync<TeyaTerminalsResponse>(response, cancellationToken)).Terminals;
        }

        public async Task<TeyaStoreConfigResponse> SetPayAtCounterAsync(string accessToken, string storeId, bool isEnabled, CancellationToken cancellationToken = default)
        {
            using var client = CreateAuthorizedHttpClient(accessToken);
            using var response = await client.PutAsync(
                $"{GetApiBaseUrl(await GetTeyaPartnerAsync())}/poslink/v1/stores/{Uri.EscapeDataString(storeId)}/configs/PAT_ENABLED",
                CreateJsonContent(new { value = isEnabled ? "true" : "false" }),
                cancellationToken);

            await EnsureSuccessAsync(response);
            return await DeserializeRequiredAsync<TeyaStoreConfigResponse>(response, cancellationToken);
        }

        public async Task<List<TeyaStoreConfigResponse>> GetTerminalConfigsAsync(string accessToken, string storeId, string terminalId, CancellationToken cancellationToken = default)
        {
            using var client = CreateAuthorizedHttpClient(accessToken);
            using var response = await client.GetAsync(
                $"{GetApiBaseUrl(await GetTeyaPartnerAsync())}/poslink/v1/stores/{Uri.EscapeDataString(storeId)}/terminals/{Uri.EscapeDataString(terminalId)}/configs",
                cancellationToken);

            await EnsureSuccessAsync(response);
            return (await DeserializeRequiredAsync<TeyaTerminalConfigsResponse>(response, cancellationToken)).Configs;
        }

        public async Task<PaymentTerminalSetting?> LoadEnabledTerminalForCurrentSessionAsync()
        {
            var siteId = _userSessionService.GetCurrentSiteId();
            var tillId = _userSessionService.GetCurrentTillId();
            _userSessionService.SetPaymentTerminalSetting(null);

            if (!siteId.HasValue)
            {
                throw new InvalidOperationException("A current site is required to load card terminal settings.");
            }

            var setting = await _paymentTerminalSettingsServices.GetEffectiveSettingAsync(siteId, tillId);
            if (siteId != _userSessionService.GetCurrentSiteId() ||
                tillId != _userSessionService.GetCurrentTillId())
            {
                throw new InvalidOperationException("The current site or till changed. Please start the payment again.");
            }

            _userSessionService.SetPaymentTerminalSetting(setting);
            return _userSessionService.CurrentPaymentTerminalSetting;
        }

        public async Task<TeyaPaymentProcessingResult> ProcessPaymentForCurrentSessionAsync(
            decimal amount,
            string currency,
            string basketTransactionId,
            CancellationToken cancellationToken = default,
            Func<string, Task>? onStatusChanged = null,
            Func<bool>? isCancellationRequested = null,
            Func<string, Task>? onCancellationFailed = null)
        {
            var setting = await LoadEnabledTerminalForCurrentSessionAsync();

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

            CardTransaction? cardTransaction = null;
            try
            {
                if (onStatusChanged != null)
                {
                    await onStatusChanged("Connecting to Teya and preparing the terminal...");
                }
                cardTransaction = new CardTransaction
                {
                    Transaction_Reference = basketTransactionId,
                    Environment = TeyaEnvironment,
                    Idempotency_Key = Guid.NewGuid().ToString("N"),
                    Amount = ConvertToMinorUnits(Math.Abs(amount)) / 100m,
                    Currency_Code = (string.IsNullOrWhiteSpace(currency) ? setting.Currency_Code : currency).Trim().ToUpperInvariant(),
                    Transaction_Type = amount < 0 ? "REFUND" : "SALE",
                    Store_Id = setting.Store_Id,
                    Terminal_Id = setting.Terminal_Id,
                    Epos_Instance_Id = BuildEposInstanceId(setting),
                    Merchant_Reference = setting.Store_Id,
                    Site_Id = setting.Site_Id,
                    Till_Id = _userSessionService.CurrentTill?.Id,
                    Created_By_Id = _userSessionService.CurrentUser?.Id
                };
                // Persist the intent before contacting the terminal, including its retry key.
                await _cardTransactionServices.AddAsync(cardTransaction, cancellationToken);
                var paymentRequest = await CreatePaymentRequestAsync(setting, amount, currency, cardTransaction.Idempotency_Key, cancellationToken);
                if (string.IsNullOrWhiteSpace(paymentRequest.PaymentRequestId))
                {
                    throw new InvalidOperationException("Teya did not return a payment request reference. Check the terminal before retrying.");
                }
                await SaveCardPaymentStatusAsync(cardTransaction, paymentRequest);
                if (onStatusChanged != null)
                {
                    await onStatusChanged("Payment sent. Ask the customer to follow the instructions on the terminal.");
                }
                var finalStatus = await WaitForFinalPaymentStatusAsync(setting, paymentRequest.PaymentRequestId,
                    cancellationToken, onStatusChanged, cardTransaction, isCancellationRequested, onCancellationFailed);

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
                    Message = ex.Message,
                    PaymentRequestId = cardTransaction?.Payment_Request_Id,
                    GatewayPaymentId = cardTransaction?.Gateway_Payment_Id
                };
            }
        }

        private async Task SaveCardPaymentStatusAsync(CardTransaction transaction, TeyaPaymentRequestResponse response)
        {
            transaction.Payment_Request_Id = response.PaymentRequestId;
            transaction.Gateway_Payment_Id = response.GatewayPaymentId ?? transaction.Gateway_Payment_Id;
            transaction.Status = string.IsNullOrWhiteSpace(response.Status) ? "PENDING" : response.Status;
            transaction.Status_Reason = response.StatusReason;
            transaction.Progress_Status = response.ProgressStatus;
            if (response.RequestedAmount != null)
            {
                transaction.Requested_Amount_Minor_Units = response.RequestedAmount.Amount;
                transaction.Requested_Tip_Minor_Units = response.RequestedAmount.Tip;
                transaction.Amount = response.RequestedAmount.Amount / 100m;
                if (!string.IsNullOrWhiteSpace(response.RequestedAmount.Currency))
                {
                    transaction.Currency_Code = response.RequestedAmount.Currency;
                }
            }
            transaction.Transaction_Type = response.TransactionType ?? transaction.Transaction_Type;
            transaction.Merchant_Reference = response.MerchantReference ?? transaction.Merchant_Reference;
            transaction.Epos_Instance_Id = response.EposInstanceId ?? transaction.Epos_Instance_Id;
            transaction.Store_Id = response.StoreId ?? transaction.Store_Id;
            transaction.Terminal_Id = response.TerminalId ?? transaction.Terminal_Id;
            transaction.Provider_Created_At = response.CreatedAt?.ToUniversalTime() ?? transaction.Provider_Created_At;
            transaction.Provider_Updated_At = response.UpdatedAt?.ToUniversalTime() ?? transaction.Provider_Updated_At;
            if (response.MetaData is { ValueKind: not JsonValueKind.Null and not JsonValueKind.Undefined } metadata)
            {
                // Keep the nested provider details intact, including nulls and leading zeros.
                transaction.Metadata = metadata.Deserialize<CardTransactionMetadata>();
            }
            transaction.Transaction_Timestamp = response.TransactionTimeStamp?.ToUniversalTime() ?? transaction.Transaction_Timestamp;
            // Once Teya replies, retain the outcome even if the caller has cancelled.
            await _cardTransactionServices.UpdateAsync(transaction);
        }

        public async Task<TeyaPaymentRequestResponse> CancelPaymentForCurrentSessionAsync(
            string paymentRequestId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(paymentRequestId))
            {
                throw new ArgumentException("Payment request id is required.", nameof(paymentRequestId));
            }

            var setting = await LoadEnabledTerminalForCurrentSessionAsync();

            if (setting == null || !setting.Is_Enabled)
            {
                throw new InvalidOperationException("Teya card terminal is not configured for this site or till.");
            }

            return await CancelPaymentAsync(setting, paymentRequestId, cancellationToken);
        }

        private async Task<TeyaPaymentRequestResponse> CancelPaymentAsync(
            PaymentTerminalSetting setting,
            string paymentRequestId,
            CancellationToken cancellationToken)
        {
            setting = await EnsureFreshAccessTokenAsync(setting, cancellationToken);

            async Task<HttpResponseMessage> SendAsync(string accessToken)
            {
                using var client = CreateAuthorizedHttpClient(accessToken);
                return await client.PutAsync(
                    $"{GetApiBaseUrl(await GetTeyaPartnerAsync())}/poslink/v2/payment-requests/{Uri.EscapeDataString(paymentRequestId)}",
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
            string idempotencyKey,
            CancellationToken cancellationToken)
        {
            setting = await EnsureFreshAccessTokenAsync(setting, cancellationToken);

            // Standalone payments do not have a Basket Data Service correlation ID.
            var requestBody = new
            {
                store_id = setting.Store_Id,
                terminal_id = setting.Terminal_Id,
                requested_amount = new
                {
                    amount = ConvertToMinorUnits(Math.Abs(amount)),
                    currency = (string.IsNullOrWhiteSpace(currency) ? setting.Currency_Code : currency).Trim().ToUpperInvariant()
                },
                transaction_type = amount < 0 ? "REFUND" : "SALE",
                merchant_reference = setting.Store_Id,
                epos_instance_id = BuildEposInstanceId(setting)
            };

            async Task<HttpResponseMessage> SendAsync(string accessToken)
            {
                using var client = CreateAuthorizedHttpClient(accessToken);
                client.DefaultRequestHeaders.Add("Idempotency-Key", idempotencyKey);
                using var content = CreateJsonContent(requestBody);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                return await client.PostAsync(
                    $"{GetApiBaseUrl(await GetTeyaPartnerAsync())}/poslink/v3/payment-requests",
                    content,
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
            CancellationToken cancellationToken,
            Func<string, Task>? onStatusChanged,
            CardTransaction cardTransaction,
            Func<bool>? isCancellationRequested,
            Func<string, Task>? onCancellationFailed)
        {
            var cancellationSent = false;
            // A pending terminal payment has no local time limit.
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var current = await GetPaymentRequestByIdOrDefaultAsync(setting, paymentRequestId, cancellationToken);

                if (current != null)
                {
                    await SaveCardPaymentStatusAsync(cardTransaction, current);
                }

                if (current != null && onStatusChanged != null)
                {
                    var status = string.IsNullOrWhiteSpace(current.ProgressStatus)
                        ? current.Status
                        : $"{current.Status}: {current.ProgressStatus}";
                    if (!string.IsNullOrWhiteSpace(current.StatusReason))
                    {
                        status += $" ({current.StatusReason})";
                    }
                    await onStatusChanged(status.Replace('_', ' '));
                }

                if (current != null && IsFinalStatus(current.Status))
                {
                    return current;
                }

                if (!cancellationSent && isCancellationRequested?.Invoke() == true)
                {
                    try
                    {
                        // Use this payment's terminal settings and serialize cancellation with polling.
                        var cancellation = await CancelPaymentAsync(setting, paymentRequestId, cancellationToken);
                        cancellationSent = true;
                        if (cancellation.PaymentRequestId == paymentRequestId)
                        {
                            await SaveCardPaymentStatusAsync(cardTransaction, cancellation);
                            if (IsFinalStatus(cancellation.Status))
                            {
                                return cancellation;
                            }
                        }
                        if (onStatusChanged != null)
                        {
                            await onStatusChanged("Cancellation sent. Waiting for the terminal to confirm the payment outcome...");
                        }
                    }
                    catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
                    {
                        var message = $"Cancellation could not be confirmed: {ex.Message} Payment status checking will continue.";
                        if (onCancellationFailed != null)
                        {
                            cancellationSent = false;
                            await onCancellationFailed(message);
                        }
                        else
                        {
                            // Without a UI retry callback, avoid repeatedly sending a failed request.
                            cancellationSent = true;
                            if (onStatusChanged != null)
                            {
                                await onStatusChanged(message);
                            }
                        }
                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken);
            }
        }

        private async Task<List<TeyaPaymentRequestResponse>> GetRecentPaymentRequestsAsync(PaymentTerminalSetting setting, CancellationToken cancellationToken)
        {
            setting = await EnsureFreshAccessTokenAsync(setting, cancellationToken);

            async Task<HttpResponseMessage> SendAsync(string accessToken)
            {
                var client = CreateAuthorizedHttpClient(accessToken);
                var url =
                    $"{GetApiBaseUrl(await GetTeyaPartnerAsync())}/poslink/v2/payment-requests?store_id={Uri.EscapeDataString(setting.Store_Id!)}" +
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

        private async Task<TeyaPaymentRequestResponse?> GetPaymentRequestByIdOrDefaultAsync(
            PaymentTerminalSetting setting, string paymentRequestId, CancellationToken cancellationToken)
        {
            setting = await EnsureFreshAccessTokenAsync(setting, cancellationToken);

            async Task<HttpResponseMessage> SendAsync(string accessToken)
            {
                var client = CreateAuthorizedHttpClient(accessToken);
                var url = $"{GetApiBaseUrl(await GetTeyaPartnerAsync())}/poslink/v2/payment-requests/{Uri.EscapeDataString(paymentRequestId)}";
                return await client.GetAsync(url, cancellationToken);
            }

            using var response = await SendAsync(setting.Access_Token!);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                setting = await ForceRefreshAccessTokenAsync(setting, cancellationToken);
                using var retryResponse = await SendAsync(setting.Access_Token!);
                if (retryResponse.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }
                await EnsureSuccessAsync(retryResponse);
                return await DeserializeRequiredAsync<TeyaPaymentRequestResponse>(retryResponse, cancellationToken);
            }

            await EnsureSuccessAsync(response);
            return await DeserializeRequiredAsync<TeyaPaymentRequestResponse>(response, cancellationToken);
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

            var partner = await GetTeyaPartnerAsync();
            using var client = CreateHttpClient();

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                "Basic",
                Convert.ToBase64String(Encoding.UTF8.GetBytes($"{partner.ClientId}:{partner.ClientSecret}")));

            using var response = await client.PostAsync(
                $"{GetIdentityBaseUrl(partner)}/oauth/v2/oauth-token",
                CreateFormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["grant_type"] = "refresh_token",
                    ["refresh_token"] = setting.Refresh_Token
                }),
                cancellationToken);

            await EnsureSuccessAsync(response);
            var refreshedTokens = await DeserializeRequiredAsync<TeyaTokenResponse>(response, cancellationToken);

            if (string.IsNullOrWhiteSpace(refreshedTokens.AccessToken) || refreshedTokens.ExpiresInSeconds <= 0)
            {
                throw new InvalidOperationException("Teya returned an invalid token refresh response. Reconnect the merchant account.");
            }

            // Do not expose refreshed credentials in the session until they have been persisted.
            if (ReferenceEquals(_userSessionService.CurrentPaymentTerminalSetting, setting))
            {
                _userSessionService.SetPaymentTerminalSetting(null);
            }

            var verifiedAt = DateTime.UtcNow;
            setting.Access_Token = refreshedTokens.AccessToken;
            // OAuth refresh responses may omit this field when the token is not rotated.
            if (!string.IsNullOrWhiteSpace(refreshedTokens.RefreshToken))
            {
                setting.Refresh_Token = refreshedTokens.RefreshToken;
            }
            setting.Access_Token_Expires_At = verifiedAt.AddSeconds(refreshedTokens.ExpiresInSeconds);
            setting.Last_Verified_At = verifiedAt;
            setting.Last_Modified = verifiedAt;
            setting.Last_Modified_By_Id = _userSessionService.GetCurrentUserId();
            setting.Last_Known_Status = "Refreshed";
            setting.SyncStatus = SyncStatus.Pending;

            await _paymentTerminalSettingsServices.UpsertForScopeAsync(setting);
            _userSessionService.SetPaymentTerminalSetting(setting);
            return setting;
        }

        private HttpClient CreateHttpClient()
        {
            var client = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(60)
            };

            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("KaninifyRetailEpos", "1.0"));
            return client;
        }

        private HttpClient CreateAuthorizedHttpClient(string accessToken)
        {
            var client = CreateHttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return client;
        }

        private async Task<PaymentIntegrationPartner> GetTeyaPartnerAsync()
        {
            if (_teyaPartner != null)
            {
                return _teyaPartner;
            }

            var retailer = await _userSessionService.EnsureRetailerAsync()
                ?? throw new InvalidOperationException("A retailer session is required to load the Teya payment integration.");
            var result = await _supabaseSyncService.GetAsync<PaymentIntegrationPartner>(
                retailer,
                "PaymentIntegrationPartner",
                whereClause: $"PaymentProvider=eq.Teya&Environment=eq.{TeyaEnvironment}");

            if (!result.IsSuccess)
            {
                throw new InvalidOperationException($"Unable to load the Teya payment integration from Supabase: {result.Error ?? result.Message}");
            }

            _teyaPartner = result.Data?.SingleOrDefault()
                ?? throw new InvalidOperationException($"No Teya payment integration is configured in Supabase for the {TeyaEnvironment} environment.");

            if (_teyaPartner.ClientId == Guid.Empty || string.IsNullOrWhiteSpace(_teyaPartner.ClientSecret))
            {
                throw new InvalidOperationException("The Teya payment integration in Supabase is missing its client ID or client secret.");
            }

            return _teyaPartner;
        }

        private static string GetApiBaseUrl(PaymentIntegrationPartner partner)
        {
            return (partner.ApiBaseUrl ?? "https://api.teya.com").TrimEnd('/');
        }

        private static string GetIdentityBaseUrl(PaymentIntegrationPartner partner)
        {
            return (partner.OAuthBaseUrl ?? "https://id.teya.com").TrimEnd('/');
        }

        private static StringContent CreateJsonContent(object value)
        {
            return new StringContent(JsonSerializer.Serialize(value), Encoding.UTF8, "application/json");
        }

        private static FormUrlEncodedContent CreateFormUrlEncodedContent(
            Dictionary<string, string> values)
        {
            var content = new FormUrlEncodedContent(values);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
            return content;
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
