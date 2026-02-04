using DataHandlerLibrary.Models.PaymentsModels.Card;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DataHandlerLibrary.Services.Payment.Card
{
    public class TeyaPaymentServices
    {
        private Process _process;
        private readonly Dictionary<string, TaskCompletionSource<JsonElement>> _pendingRequests = new Dictionary<string, TaskCompletionSource<JsonElement>>();
        private readonly TaskCompletionSource setupCompletionSource = new TaskCompletionSource();

        private TeyaInfo _teyaInfo;
        public TeyaPaymentServices(TeyaInfo pTeyaInfo)
        {
            _teyaInfo = pTeyaInfo;
        }

        public bool MatchSignatureCertificate(string exepath, string cerFilePath)
        {
            if (string.IsNullOrEmpty(exepath) || string.IsNullOrEmpty(cerFilePath))
            {
                return false;
            }
            try
            {
                X509Certificate2 expectedCert = new X509Certificate2(cerFilePath);
                X509Certificate2 execert = new X509Certificate2(X509Certificate.CreateFromSignedFile(exepath));

                return string.Equals(
                    expectedCert.Thumbprint, execert.Thumbprint, StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void StartProcess()
        {
            if (_teyaInfo == null)
            {
                throw new ArgumentNullException(nameof(_teyaInfo), "TeyaInfo cannot be null.");
            }

            if (!MatchSignatureCertificate(_teyaInfo.ExePath, _teyaInfo.CerFilePath))
            {
                throw new InvalidOperationException("The executable's signature does not match the expected certificate.");
            }
            var processStartInfo = new ProcessStartInfo
            {
                FileName = _teyaInfo.ExePath,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false
            };

            _process = Process.Start(processStartInfo);
            Task.Run(ReadResponsesAsync);
        }

        public async Task<JsonElement> Initialize()
        {
            var parameter = new
            {
                requesterId = _teyaInfo.AppId,
                requesterVersion = _teyaInfo.Appversion,
                isProductionEnv = _teyaInfo.IsProduction,
                clientId = _teyaInfo.ClientId,
                clientSecret = _teyaInfo.ClientSecret
            };
            return await SendRequestAsync("Initialize", parameter);
        }
        
        public async Task<JsonElement> Setup()
        {
            var response = await SendRequestAsync("Setup");
            if(response.TryGetProperty("response", out JsonElement responseValue) &&
                responseValue.GetString() == "Success")
            {
               setupCompletionSource.TrySetResult();
            }
            return response;
        }

        public async Task<MakePaymentStateChange> Makepayment(string id, int totalAmount, int? tipAmount)
        {
            var paymentDetails = new
            {
                transactionId = id,
                amount = totalAmount,
                tip = tipAmount ?? 0,
                currency = _teyaInfo.AppCurrency,
                useTeyaDefaultUi = true,
                
            };
            var response = await SendRequestAsync("makePaymentAndSubscribe", paymentDetails);
            return JsonSerializer.Deserialize<MakePaymentStateChange>(response);
        }
        public async Task<JsonElement> CancelPayment(string id)
        {
            var parameters = new
            {
                transactionId = id,
            };

            return await SendRequestAsync("cancelPayment", parameters);
        }

        public async Task<JsonElement> HideTeyaUi(string id)
        {
            var parameters = new
            {
                transactionId = id,
            };

            return await SendRequestAsync("unsubscribeAll", parameters);
        }

        public async Task<JsonElement> PrepareLogs()
        {
            return await SendRequestAsync("zipLogs");
        }

        public async Task<JsonElement> ClearUserAuth()
        {
            return await SendRequestAsync("clearUserAuth");
        }

        public async Task<JsonElement> ClearDeviceLink()
        {
            return await SendRequestAsync("clearDeviceLink");
        }

        public async Task<JsonElement> RefundPayment(string id, int refundAmount)
        {
            var refundParams = new
            {
                gatewayPaymentId = id,
                amount = refundAmount,
                currency = _teyaInfo.AppCurrency,
            };
            return await SendRequestAsync("refundPayment", refundParams);
        }

        private async Task<JsonElement> SendRequestAsync(string methodName, object parameters = null)
        {
            // for methods that need setup, let's wait for setup to be completed
            if (methodName != "initialize" && methodName != "setup" && methodName != "clearUserAuth" && methodName != "clearDeviceLink")
            {
                await setupCompletionSource.Task;
            }

            if (_process == null || _process.HasExited)
            {
                throw new InvalidOperationException("SDK process is not running");
            }

            string reqId = Guid.NewGuid().ToString();

            object request;
            if (parameters == null)
            {
                request = new
                {
                    jsonrpc = "2.0",
                    id = reqId,
                    method = methodName
                };
            }
            else
            {
                request = new
                {
                    jsonrpc = "2.0",
                    id = reqId,
                    method = methodName,
                    @params = parameters
                };
            }

            string requestJson = JsonSerializer.Serialize(request);

            var tcs = new TaskCompletionSource<JsonElement>();
            lock (_pendingRequests)
            {
                _pendingRequests[reqId] = tcs;
            }

            await _process.StandardInput.WriteLineAsync(requestJson);
            await _process.StandardInput.FlushAsync();

            return await tcs.Task;
        }

        private async Task ReadResponsesAsync()
        {
            using (StreamReader reader = _process.StandardOutput)
            {
                while (!reader.EndOfStream)
                {
                    string? line = await reader.ReadLineAsync();

                    if (string.IsNullOrEmpty(line))
                        continue;

                    ProcessResponse(line);
                }
            }
        }

        private void ProcessResponse(string responseJson)
        {
            JsonElement root = JsonDocument.Parse(responseJson).RootElement;

            if (!root.TryGetProperty("id", out JsonElement idElement))
            {
                // This is a notification, not a response to a request
                root.TryGetProperty("result", out JsonElement resultElement);

                var state = JsonSerializer.Deserialize<MakePaymentStateChange>(resultElement.ToString());

                Debug.WriteLine("State change = " + state);
                return;
            }

            string id = idElement.GetString();

            TaskCompletionSource<JsonElement> tcs = null;
            lock (_pendingRequests)
            {
                if (_pendingRequests.TryGetValue(id, out tcs))
                {
                    _pendingRequests.Remove(id);
                }
            }

            if (tcs == null)
            {
                Console.WriteLine($"Received response for unknown request ID: {id}");
                return;
            }

            // Check if the response contains an error
            if (root.TryGetProperty("error", out JsonElement errorElement) && !errorElement.ValueKind.Equals(JsonValueKind.Null))
            {
                tcs.SetException(new Exception($"JSON-RPC error: {errorElement}"));
            }
            else if (root.TryGetProperty("result", out JsonElement resultElement))
            {
                tcs.SetResult(resultElement);
            }
            else
            {
                tcs.SetException(new Exception("Invalid JSON-RPC response: missing result"));
            }
        }
    }

}
