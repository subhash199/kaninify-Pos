using DataHandlerLibrary.Models;
using DataHandlerLibrary.Models.SupabaseModels;
using DataHandlerLibrary.Services;
using EFCore.BulkExtensions;
using EntityFrameworkDatabaseLibrary.Data;
using EntityFrameworkDatabaseLibrary.Models;
using EposDataHandler.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using UnknownProduct = DataHandlerLibrary.Models.UnknownProduct;

namespace DataHandlerLibrary.Services
{
    public class SupabaseSyncService
    {
        private HttpClient _httpClient;
        private readonly ILogger<SupabaseSyncService> _logger;
        private readonly IDbContextFactory<DatabaseInitialization> _dbFactory;
        private readonly ErrorLogServices _errorLogServices;
        private readonly ProductServices _productServices;
        private readonly GlobalErrorLogService _globalErrorLogService;
        UserSessionService _userSessionService;

        private string _supabaseUrl;
        private string _supabaseKey;
        private string _supabaseServiceKey;
        private Guid? _currentRetailerId;
        private List<Department> _cachedDepartments;
        private List<Vat> _cachedVats;
        public SupabaseSyncService(

           IDbContextFactory<DatabaseInitialization> dbFactory, ILogger<SupabaseSyncService> logger, ErrorLogServices errorLogServices, UserSessionService UserSessionService, ProductServices productServices, GlobalErrorLogService globalErrorLogService)
        {
            _httpClient = new HttpClient();
            _dbFactory = dbFactory;
            _errorLogServices = errorLogServices;
            _logger = logger;
            _userSessionService = UserSessionService;
            _productServices = productServices;
            _globalErrorLogService = globalErrorLogService;
        }

        /// <summary>
        /// Initialize the Supabase connection for a specific retailer
        /// </summary>
        public async Task InitializeForRetailerAsync(Retailer pRetailer)
        {
            try
            {
                if (pRetailer == null)
                {
                    throw new ArgumentNullException("No retailer found in the database.");
                }

                // Get Supabase credentials from the retailer
                _supabaseUrl = pRetailer.ApiUrl ?? throw new ArgumentNullException("Retailer.ApiUrl");
                _supabaseKey = pRetailer.ApiKey ?? throw new ArgumentNullException("Retailer.ApiKey");
                _supabaseServiceKey = pRetailer.AccessToken ?? throw new ArgumentNullException("Retailer.AccessToken");
                _currentRetailerId = pRetailer.RetailerId;

                ConfigureHttpClient();


            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to initialize Supabase connection for retailer {pRetailer?.RetailerId}");
                if (_globalErrorLogService != null)
                {
                    await _globalErrorLogService.LogErrorAsync(
                        ex,
                        nameof(InitializeForRetailerAsync),
                        $"Failed to initialize Supabase connection for retailer {pRetailer?.RetailerId}");
                }
                throw;
            }
        }

        private void ConfigureHttpClient()
        {
            // Create a new HttpClient instance instead of modifying the existing one
            _httpClient?.Dispose();
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri($"{_supabaseUrl}/rest/v1/");
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("apikey", _supabaseKey);
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_supabaseServiceKey}");
            // Remove this line - Content-Type should be set on individual requests, not as default header
            // _httpClient.DefaultRequestHeaders.Add("Content-Type", "application/json");
            _httpClient.DefaultRequestHeaders.Add("Prefer", "return=representation");
        }

        #region Generic CRUD Operations

        /// <summary>
        /// Generic method to insert data into Supabase table
        /// </summary>
        public async Task<SyncResult<T>> InsertAsync<T>(Retailer retailer, string tableName, T data, Guid retailerId) where T : class
        {
            retailer = await EnsureInitializedAsync(retailer);

            try
            {
                var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = {
                        new JsonStringEnumConverter(),
                        new NullableDateTimeConverter()
                    }
                });

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Add RLS (Row Level Security) header for retailer isolation
                var request = new HttpRequestMessage(HttpMethod.Post, tableName)
                {
                    Content = content
                };
                request.Headers.Add("X-Retailer-Id", retailerId.ToString());

                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<T[]>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        Converters = {
                            new JsonStringEnumConverter(),
                            new NullableDateTimeConverter()
                        }
                    });

                    return new SyncResult<T>
                    {
                        IsSuccess = true,
                        Data = result?.Length > 0 ? result[0] : data,
                        Message = "Data inserted successfully"
                    };
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Insert failed for table {tableName}: {errorContent}");

                    return new SyncResult<T>
                    {
                        IsSuccess = false,
                        Error = errorContent,
                        Message = $"Insert failed: {response.StatusCode}"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception during insert to table {tableName}");
                if (_globalErrorLogService != null)
                {
                    await _globalErrorLogService.LogErrorAsync(
                        ex,
                        nameof(InsertAsync),
                        $"Insert failed for table {tableName}");
                }
                return new SyncResult<T>
                {
                    IsSuccess = false,
                    Error = ex.Message,
                    Message = "Insert operation failed with exception"
                };
            }
        }

        /// <summary>
        /// Generic method to update data in Supabase table
        /// </summary>
        public async Task<SyncResult<T>> UpdateAsync<T>(Retailer retailer, string tableName, T data, string whereClause, Guid retailerId) where T : class
        {
            retailer = await EnsureInitializedAsync(retailer);

            try
            {
                var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = {
                        new JsonStringEnumConverter(),
                        new NullableDateTimeConverter()
                    }
                });

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var request = new HttpRequestMessage(HttpMethod.Patch, $"{tableName}?{whereClause}")
                {
                    Content = content
                };
                request.Headers.Add("X-Retailer-Id", retailerId.ToString());

                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<T[]>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        Converters = {
                            new JsonStringEnumConverter(),
                            new NullableDateTimeConverter()
                        }
                    });

                    return new SyncResult<T>
                    {
                        IsSuccess = true,
                        Data = result?.Length > 0 ? result[0] : data,
                        Message = "Data updated successfully"
                    };
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Update failed for table {tableName}: {errorContent}");

                    return new SyncResult<T>
                    {
                        IsSuccess = false,
                        Error = errorContent,
                        Message = $"Update failed: {response.StatusCode}"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception during update to table {tableName}");
                if (_globalErrorLogService != null)
                {
                    await _globalErrorLogService.LogErrorAsync(
                        ex,
                        nameof(UpdateAsync),
                        $"Update failed for table {tableName}");
                }
                return new SyncResult<T>
                {
                    IsSuccess = false,
                    Error = ex.Message,
                    Message = "Update operation failed with exception"
                };
            }
        }

        /// <summary>
        /// Generic method to upsert (insert or update) data in Supabase table
        /// </summary>
        public async Task<SyncResult<T>> UpsertAsync<T>(Retailer retailer, string tableName, T data, string conflictColumns, Guid retailerId) where T : class
        {
            retailer = await EnsureInitializedAsync(retailer);

            try
            {
                var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = {
                        new JsonStringEnumConverter(),
                        new NullableDateTimeConverter()
                    }
                });

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var endpoint = string.IsNullOrEmpty(conflictColumns)
                    ? tableName
                    : $"{tableName}?on_conflict={Uri.EscapeDataString(conflictColumns)}";
                var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
                {
                    Content = content
                };
                request.Headers.Add("X-Retailer-Id", retailerId.ToString());
                request.Headers.Add("Prefer", $"resolution=merge-duplicates,return=representation");

                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    if (!string.Equals(tableName, "SyncedLogs"))
                    {
                        var result = JsonSerializer.Deserialize<T[]>(responseContent, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true,
                            Converters = {
                            new JsonStringEnumConverter(),
                            new NullableDateTimeConverter(),
                            new SafeDateTimeConverter()
                        }
                        });
                        return new SyncResult<T>
                        {
                            IsSuccess = true,
                            Data = result?.Length > 0 ? result[0] : data,
                            Message = "Data upserted successfully"
                        };
                    }
                    else
                    {
                        return new SyncResult<T>
                        {
                            IsSuccess = true,
                            Data = data,
                            Message = "Data upserted successfully"
                        };
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Upsert failed for table {tableName}: {errorContent}");

                    return new SyncResult<T>
                    {
                        IsSuccess = false,
                        Error = errorContent,
                        Message = $"Upsert failed: {response.StatusCode}"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception during upsert to table {tableName}");
                return new SyncResult<T>
                {
                    IsSuccess = false,
                    Error = ex.Message,
                    Message = "Upsert operation failed with exception"
                };
            }
        }

        /// <summary>
        /// Generic method to fetch data from Supabase table
        /// </summary>
        public async Task<SyncResult<List<T>>> GetAsync<T>(Retailer retailer, string tableName, string selectColumns = "*", string whereClause = "", Guid? retailerId = null) where T : class
        {
            retailer = await EnsureInitializedAsync(retailer);

            try
            {
                var url = _httpClient.BaseAddress + $"{tableName}?select={selectColumns}";
                if (!string.IsNullOrEmpty(whereClause))
                {
                    url += $"&{whereClause}";
                }

                var request = new HttpRequestMessage(HttpMethod.Get, url);
                if (retailerId.HasValue)
                {
                    request.Headers.Add("X-Retailer-Id", retailerId.Value.ToString());
                }
                else if (retailer != null)
                {
                    request.Headers.Add("X-Retailer-Id", retailer.RetailerId.ToString());
                }

                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<List<T>>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        Converters = {
                             new JsonStringEnumConverter(),
                            new NullableDateTimeConverter(),
                            new SafeDateTimeConverter()
                        }
                    });
                    return new SyncResult<List<T>>
                    {
                        IsSuccess = true,
                        Data = result ?? new List<T>(),
                        Message = "Data retrieved successfully"
                    };
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning("Received 401 Unauthorized during select operation, attempting token refresh and retry");

                    bool retryTokenValid = false;
                    if (retailerId.HasValue)
                    {
                        retryTokenValid = await RefreshTokenIfNeededAsync(retailer);
                    }
                    else if (retailer != null)
                    {
                        retryTokenValid = await RefreshTokenIfNeededAsync(retailer);
                    }

                    if (retryTokenValid)
                    {
                        // Retry the request with refreshed token
                        retailer = await EnsureInitializedAsync(retailer);
                        var retryRequest = new HttpRequestMessage(HttpMethod.Get, url);
                        if (retailerId.HasValue)
                        {
                            retryRequest.Headers.Add("X-Retailer-Id", retailerId.Value.ToString());
                        }
                        else if (retailer != null)
                        {
                            retryRequest.Headers.Add("X-Retailer-Id", retailer.RetailerId.ToString());
                        }

                        var retryResponse = await _httpClient.SendAsync(retryRequest);

                        if (retryResponse.IsSuccessStatusCode)
                        {
                            var retryResponseContent = await retryResponse.Content.ReadAsStringAsync();
                            var retryResult = JsonSerializer.Deserialize<List<T>>(retryResponseContent, new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true,
                                Converters = {
                                    new JsonStringEnumConverter(),
                                    new NullableDateTimeConverter(),
                                    new SafeDateTimeConverter()
                                }
                            });

                            return new SyncResult<List<T>>
                            {
                                IsSuccess = true,
                                Data = retryResult ?? new List<T>(),
                                Message = "Data retrieved successfully after token refresh"
                            };
                        }
                        else
                        {
                            var retryErrorContent = await retryResponse.Content.ReadAsStringAsync();
                            _logger.LogError($"Retry get failed for table {tableName} after token refresh: {retryErrorContent}");

                            return new SyncResult<List<T>>
                            {
                                IsSuccess = false,
                                Error = retryErrorContent,
                                Message = $"Retry get failed after token refresh: {retryResponse.StatusCode}"
                            };
                        }
                    }
                    else
                    {
                        _logger.LogError($"Unable to refresh token for select operation on table {tableName}");
                        return new SyncResult<List<T>>
                        {
                            IsSuccess = false,
                            Error = "Authentication failed",
                            Message = "Unable to authenticate after token refresh attempt"
                        };
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Get failed for table {tableName}: {errorContent}");

                    return new SyncResult<List<T>>
                    {
                        IsSuccess = false,
                        Error = errorContent,
                        Message = $"Get failed: {response.StatusCode}"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception during get from table {tableName}");
                if (_globalErrorLogService != null)
                {
                    await _globalErrorLogService.LogErrorAsync(
                        ex,
                        nameof(GetAsync),
                        $"Get failed for {tableName} select={selectColumns} where={(string.IsNullOrEmpty(whereClause) ? "null" : whereClause)}");
                }
                return new SyncResult<List<T>>
                {
                    IsSuccess = false,
                    Error = ex.Message,
                    Message = "Get operation failed with exception"
                };
            }
        }

        /// <summary>
        /// Generic method to delete data from Supabase table
        /// </summary>
        public async Task<SyncResult<bool>> DeleteAsync(Retailer retailer, string tableName, string whereClause, Guid retailerId)
        {
            retailer = await EnsureInitializedAsync(retailer);

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Delete, $"{tableName}?{whereClause}");
                request.Headers.Add("X-Retailer-Id", retailerId.ToString());

                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    return new SyncResult<bool>
                    {
                        IsSuccess = true,
                        Data = true,
                        Message = "Data deleted successfully"
                    };
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Delete failed for table {tableName}: {errorContent}");

                    return new SyncResult<bool>
                    {
                        IsSuccess = false,
                        Data = false,
                        Error = errorContent,
                        Message = $"Delete failed: {response.StatusCode}"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception during delete from table {tableName}");
                if (_globalErrorLogService != null)
                {
                    await _globalErrorLogService.LogErrorAsync(
                        ex,
                        nameof(DeleteAsync),
                        $"Delete failed for {tableName} where={whereClause}");
                }
                return new SyncResult<bool>
                {
                    IsSuccess = false,
                    Data = false,
                    Error = ex.Message,
                    Message = "Delete operation failed with exception"
                };
            }
        }

        #region Unsynced Data Management

        /// <summary>
        /// Gets unsynced records from Supabase, groups them by table name, fetches the actual records, 
        /// syncs them to local database, and updates sync status
        /// </summary>
        /// <param name="retailer">The retailer for authentication</param>
        /// <returns>SyncResult containing sync operation results</returns>
        public async Task<SyncResult<Dictionary<string, int>>> SyncUnsyncedDataFromCloudAsync(Retailer retailer)
        {
            retailer = await EnsureInitializedAsync(retailer);

            var syncResults = new Dictionary<string, int>();
            var totalSynced = 0;
            var totalFailed = 0;

            try
            {
                // Step 1: Get all unsynced records for this retailer
                var unsyncedLogsResult = await GetAsync<SupaUnSyncedLogs>(
                   retailer,
                    "UnSyncedLogs",
                    "*",
                    $"RetailerId.eq.{retailer.RetailerId}&SyncStatus.eq.Pending"
                );

                if (!unsyncedLogsResult.IsSuccess || unsyncedLogsResult.Data == null || !unsyncedLogsResult.Data.Any())
                {
                    return new SyncResult<Dictionary<string, int>>
                    {
                        IsSuccess = true,
                        Data = syncResults,
                        Message = "No unsynced records found"
                    };
                }

                _logger.LogInformation($"Found {unsyncedLogsResult.Data.Count} unsynced records for retailer {retailer.RetailerId}");

                // Step 2: Group unsynced records by table name
                var groupedByTable = unsyncedLogsResult.Data
                    .GroupBy(log => log.TableName)
                    .ToDictionary(g => g.Key, g => g.ToList());

                // Batch collections for efficient processing
                var allSyncLogs = new List<SupaSyncedLogs>();
                var allSuccessfulRecords = new List<SupaUnSyncedLogs>();
                var allFailedRecords = new List<SupaUnSyncedLogs>();

                // Step 3: Process each table
                foreach (var tableGroup in groupedByTable)
                {
                    var tableName = tableGroup.Key;
                    var unsyncedRecords = tableGroup.Value;

                    _logger.LogInformation($"Processing {unsyncedRecords.Count} unsynced records for table {tableName}");

                    try
                    {
                        // Step 4: Sync records for this table
                        var tableSyncResult = await SyncTableRecordsAsync(tableName, unsyncedRecords, retailer);

                        if (tableSyncResult.IsSuccess)
                        {
                            syncResults[tableName] = tableSyncResult.Data;
                            totalSynced += tableSyncResult.Data;

                            // Collect sync logs and successful records for batch processing
                            var syncLogs = CreateSyncLogEntry(tableName, unsyncedRecords.Select(dr => dr.RecordId).ToList(), SyncStatus.Synced,
                                 SyncLocation.Local, unsyncedRecords.FirstOrDefault()?.Operation ?? Operation.DELETE, retailer.RetailerId);

                            allSyncLogs.Add(syncLogs);
                            allSuccessfulRecords.AddRange(unsyncedRecords);
                        }
                        else
                        {
                            syncResults[tableName] = 0;
                            totalFailed += unsyncedRecords.Count;

                            // Collect failed records for batch processing
                            allFailedRecords.AddRange(unsyncedRecords);

                            _logger.LogError($"Failed to sync table {tableName}: {tableSyncResult.Error}");
                        }
                    }
                    catch (Exception ex)
                    {
                        syncResults[tableName] = 0;
                        totalFailed += unsyncedRecords.Count;

                        // Collect failed records for batch processing
                        allFailedRecords.AddRange(unsyncedRecords);

                        _logger.LogError(ex, $"Exception while syncing table {tableName}");
                    }
                }

                // Step 5: Execute batch operations outside the loop for efficiency
                try
                {
                    // Batch log sync operations
                    if (allSyncLogs.Any())
                    {
                        // Group sync logs by table name for batch logging
                        var syncLogsByTable = allSyncLogs.GroupBy(sl => sl.TableName);
                        foreach (var tableLogGroup in syncLogsByTable)
                        {
                            await LogSyncOperationAsync(tableLogGroup.Key, tableLogGroup.ToList(), retailer);
                        }
                    }

                    // Batch update successful records
                    if (allSuccessfulRecords.Any())
                    {
                        await UpdateUnsyncedLogsStatusAsync(allSuccessfulRecords, SyncStatus.Synced, retailer);
                    }

                    // Batch update failed records
                    if (allFailedRecords.Any())
                    {
                        await UpdateUnsyncedLogsStatusAsync(allFailedRecords, SyncStatus.Failed, retailer);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Exception during batch operations for sync status updates");
                }

                return new SyncResult<Dictionary<string, int>>
                {
                    IsSuccess = totalFailed == 0,
                    Data = syncResults,
                    Message = $"Sync completed. Total synced: {totalSynced}, Total failed: {totalFailed}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during unsynced data sync operation");
                return new SyncResult<Dictionary<string, int>>
                {
                    IsSuccess = false,
                    Error = ex.Message,
                    Message = "Unsynced data sync operation failed with exception"
                };
            }
        }

        /// <summary>
        /// Syncs records for a specific table based on unsynced log entries
        /// </summary>
        /// <param name="tableName">Name of the table to sync</param>
        /// <param name="unsyncedRecords">List of unsynced log entries for this table</param>
        /// <param name="retailer">The retailer for authentication</param>
        /// <returns>SyncResult containing the number of successfully synced records</returns>
        private async Task<SyncResult<int>> SyncTableRecordsAsync(string tableName, List<SupaUnSyncedLogs> unsyncedRecords, Retailer retailer)
        {
            try
            {
                using var context = _dbFactory.CreateDbContext(); // fresh DbContext

                var deleteRecords = unsyncedRecords.Where(r => r.Operation == Operation.DELETE).ToList();
                var upsertRecords = unsyncedRecords.Where(r => r.Operation != Operation.DELETE).ToList();
                // Get record IDs to fetch
                var recordIds = upsertRecords.Select(r => r.RecordId).ToList();
                var recordIdsString = string.Join(",", recordIds);

                // Build where clause to fetch specific records
                var whereClause = $"Supa_Id=in.({recordIdsString})";

                // Determine the type and sync method based on table name
                switch (tableName.ToLower())
                {
                    case "retailers":
                        if (deleteRecords.Any())
                        {
                            context.Retailers.Remove(retailer);
                            await context.SaveChangesAsync();
                        }
                        if (upsertRecords.Any())
                        {
                            return await SyncRetailerRecordAsync(retailer);
                        }
                        // If neither, return success with 0
                        return new SyncResult<int>
                        {
                            IsSuccess = true,
                            Data = 0,
                            Message = "No retailer records to sync"
                        };

                    case "daylogs":
                        if (deleteRecords.Any())
                        {
                            var recordIdsToDelete = deleteRecords.Select(d => d.RecordId).Distinct().ToList();
                            var recordsToDelete = context.DayLogs
                                .Where(p => p.Supa_Id.HasValue && recordIdsToDelete.Contains(p.Supa_Id.Value))
                                .ToList();
                            context.RemoveRange(recordsToDelete);
                            await context.SaveChangesAsync();
                        }
                        if (upsertRecords.Any())
                        {
                            return await SyncDayLogRecordsAsync(whereClause, retailer);
                        }
                        // If neither, return success with 0
                        return new SyncResult<int>
                        {
                            IsSuccess = true,
                            Data = 0,
                            Message = "No day log records to sync"
                        };

                    case "drawerlogs":
                        if (deleteRecords.Any())
                        {
                            var recordIdsToDelete = deleteRecords.Select(d => d.RecordId).Distinct().ToList();
                            var recordsToDelete = context.DrawerLogs
                                .Where(p => p.Supa_Id.HasValue && recordIdsToDelete.Contains(p.Supa_Id.Value))
                                .ToList();
                            context.RemoveRange(recordsToDelete);
                            await context.SaveChangesAsync();
                        }
                        if (upsertRecords.Any())
                        {
                            return await SyncDrawerLogRecordsAsync(whereClause, retailer);
                        }
                        // If neither, return success with 0
                        return new SyncResult<int>
                        {
                            IsSuccess = true,
                            Data = 0,
                            Message = "No drawer log records to sync"
                        };
                    case "errorlogs":
                        if (deleteRecords.Any())
                        {
                            var recordIdsToDelete = deleteRecords.Select(d => d.RecordId).Distinct().ToList();
                            var recordsToDelete = context.ErrorLogs
                                .Where(p => p.Supa_Id.HasValue && recordIdsToDelete.Contains(p.Supa_Id.Value))
                                .ToList();
                            context.RemoveRange(recordsToDelete);
                            await context.SaveChangesAsync();
                        }
                        if (upsertRecords.Any())
                        {
                            return await SyncErrorLogRecordsAsync(whereClause, retailer);
                        }
                        // If neither, return success with 0
                        return new SyncResult<int>
                        {
                            IsSuccess = true,
                            Data = 0,
                            Message = "No error log records to sync"
                        };
                    case "payouts":
                        if (deleteRecords.Any())
                        {
                            var recordIdsToDelete = deleteRecords.Select(d => d.RecordId).Distinct().ToList();
                            var recordsToDelete = context.Payouts
                                .Where(p => p.Supa_Id.HasValue && recordIdsToDelete.Contains(p.Supa_Id.Value))
                                .ToList();
                            context.RemoveRange(recordsToDelete);
                            await context.SaveChangesAsync();
                        }
                        if (upsertRecords.Any())
                        {
                            return await SyncPayoutRecordsAsync(whereClause, retailer);
                        }
                        // If neither, return success with 0
                        return new SyncResult<int>
                        {
                            IsSuccess = true,
                            Data = 0,
                            Message = "No payout records to sync"
                        };
                    case "posusers":
                        if (deleteRecords.Any())
                        {
                            var recordIdsToDelete = deleteRecords.Select(d => d.RecordId).Distinct().ToList();
                            var recordsToDelete = context.PosUsers
                                .Where(p => p.Supa_Id.HasValue && recordIdsToDelete.Contains(p.Supa_Id.Value))
                                .ToList();
                            context.RemoveRange(recordsToDelete);
                            await context.SaveChangesAsync();
                        }
                        if (upsertRecords.Any())
                        {
                            return await SyncPosUserRecordsAsync(whereClause, retailer);
                        }
                        return new SyncResult<int>
                        {
                            IsSuccess = true,
                            Data = 0,
                            Message = "No POS user records to sync"
                        };

                    case "products":
                        if (deleteRecords.Any())
                        {
                            var recordIdsToDelete = deleteRecords.Select(d => d.RecordId).Distinct().ToList();
                            var recordsToDelete = context.Products
                                .Where(p => p.Supa_Id.HasValue && recordIdsToDelete.Contains(p.Supa_Id.Value))
                                .ToList();
                            context.RemoveRange(recordsToDelete);
                            await context.SaveChangesAsync();
                        }
                        if (upsertRecords.Any())
                        {
                            return await SyncProductRecordsAsync(whereClause, retailer);
                        }
                        // If neither, return success with 0
                        return new SyncResult<int>
                        {
                            IsSuccess = true,
                            Data = 0,
                            Message = "No product records to sync"
                        };

                    case "promotions":
                        if (deleteRecords.Any())
                        {
                            var recordIdsToDelete = deleteRecords.Select(d => d.RecordId).Distinct().ToList();
                            var recordsToDelete = context.Promotions
                                .Where(p => p.Supa_Id.HasValue && recordIdsToDelete.Contains(p.Supa_Id.Value))
                                .ToList();
                            context.RemoveRange(recordsToDelete);
                            await context.SaveChangesAsync();
                        }
                        if (upsertRecords.Any())
                        {
                            return await SyncPromotionRecordsAsync(whereClause, retailer);
                        }
                        return new SyncResult<int>
                        {
                            IsSuccess = true,
                            Data = 0,
                            Message = "No promotion records to sync"
                        };

                    case "receiptprinters":
                        if (deleteRecords.Any())
                        {
                            var recordIdsToDelete = deleteRecords.Select(d => d.RecordId).Distinct().ToList();
                            var recordsToDelete = context.ReceiptPrinters
                                .Where(p => p.Supa_Id.HasValue && recordIdsToDelete.Contains(p.Supa_Id.Value))
                                .ToList();
                            context.RemoveRange(recordsToDelete);
                            await context.SaveChangesAsync();
                        }
                        if (upsertRecords.Any())
                        {
                            return await SyncReceiptPrinterRecordsAsync(whereClause, retailer);
                        }
                        return new SyncResult<int>
                        {
                            IsSuccess = true,
                            Data = 0,
                            Message = "No receipt printer records to sync"
                        };
                    //case "departments":
                    //    return await SyncDepartmentRecordsAsync(whereClause, retailer);
                    case "salesitemtransactions":
                        if (deleteRecords.Any())
                        {
                            var recordIdsToDelete = deleteRecords.Select(d => d.RecordId).Distinct().ToList();
                            var recordsToDelete = context.SalesItemTransactions
                                .Where(p => p.Supa_Id.HasValue && recordIdsToDelete.Contains(p.Supa_Id.Value))
                                .ToList();
                            context.RemoveRange(recordsToDelete);
                            await context.SaveChangesAsync();
                        }
                        if (upsertRecords.Any())
                        {
                            return await SyncSalesItemTransactionRecordsAsync(whereClause, retailer);
                        }
                        return new SyncResult<int>
                        {
                            IsSuccess = true,
                            Data = 0,
                            Message = "No sales item transaction records to sync"
                        };

                    case "salestransactions":
                        if (deleteRecords.Any())
                        {
                            var recordIdsToDelete = deleteRecords.Select(d => d.RecordId).Distinct().ToList();
                            var recordsToDelete = context.SalesTransactions
                                .Where(p => p.Supa_Id.HasValue && recordIdsToDelete.Contains(p.Supa_Id.Value))
                                .ToList();
                            context.RemoveRange(recordsToDelete);
                            await context.SaveChangesAsync();
                        }
                        if (upsertRecords.Any())
                        {
                            return await SyncSalesTransactionRecordsAsync(whereClause, retailer);
                        }
                        return new SyncResult<int>
                        {
                            IsSuccess = true,
                            Data = 0,
                            Message = "No sales transaction records to sync"
                        };



                    case "shifts":
                        if (deleteRecords.Any())
                        {
                            var recordIdsToDelete = deleteRecords.Select(d => d.RecordId).Distinct().ToList();
                            var recordsToDelete = context.Shifts
                                .Where(p => p.Supa_Id.HasValue && recordIdsToDelete.Contains(p.Supa_Id.Value))
                                .ToList();
                            context.RemoveRange(recordsToDelete);
                            await context.SaveChangesAsync();
                        }
                        if (upsertRecords.Any())
                        {
                            return await SyncShiftRecordsAsync(whereClause, retailer);
                        }
                        return new SyncResult<int>
                        {
                            IsSuccess = true,
                            Data = 0,
                            Message = "No shift records to sync"
                        };

                    case "sites":
                        if (deleteRecords.Any())
                        {
                            var recordIdsToDelete = deleteRecords.Select(d => d.RecordId).Distinct().ToList();
                            var recordsToDelete = context.Sites
                                .Where(p => p.Supa_Id.HasValue && recordIdsToDelete.Contains(p.Supa_Id.Value))
                                .ToList();
                            context.RemoveRange(recordsToDelete);
                            await context.SaveChangesAsync();
                        }
                        if (upsertRecords.Any())
                        {
                            return await SyncSiteRecordsAsync(whereClause, retailer);
                        }
                        return new SyncResult<int>
                        {
                            IsSuccess = true,
                            Data = 0,
                            Message = "No site records to sync"
                        };
                    case "stockrefills":
                        if (deleteRecords.Any())
                        {
                            var recordIdsToDelete = deleteRecords.Select(d => d.RecordId).Distinct().ToList();
                            var recordsToDelete = context.StockRefills
                                .Where(p => p.Supa_Id.HasValue && recordIdsToDelete.Contains(p.Supa_Id.Value))
                                .ToList();
                            context.RemoveRange(recordsToDelete);
                            await context.SaveChangesAsync();
                        }
                        if (upsertRecords.Any())
                        {
                            return await SyncStockRefillRecordsAsync(whereClause, retailer);
                        }
                        return new SyncResult<int>
                        {
                            IsSuccess = true,
                            Data = 0,
                            Message = "No stock refill records to sync"
                        };

                    case "stocktransactions":
                        if (deleteRecords.Any())
                        {
                            var recordIdsToDelete = deleteRecords.Select(d => d.RecordId).Distinct().ToList();
                            var recordsToDelete = context.StockTransactions
                                .Where(p => p.Supa_Id.HasValue && recordIdsToDelete.Contains(p.Supa_Id.Value))
                                .ToList();
                            context.RemoveRange(recordsToDelete);
                            await context.SaveChangesAsync();
                        }
                        if (upsertRecords.Any())
                        {
                            return await SyncStockTransactionRecordsAsync(whereClause, retailer);
                        }
                        return new SyncResult<int>
                        {
                            IsSuccess = true,
                            Data = 0,
                            Message = "No stock transaction records to sync"
                        };
                    case "supplieritems":
                        if (deleteRecords.Any())
                        {
                            var recordIdsToDelete = deleteRecords.Select(d => d.RecordId).Distinct().ToList();
                            var recordsToDelete = context.SupplierItems
                                .Where(p => p.Supa_Id.HasValue && recordIdsToDelete.Contains(p.Supa_Id.Value))
                                .ToList();
                            context.RemoveRange(recordsToDelete);
                            await context.SaveChangesAsync();
                        }
                        if (upsertRecords.Any())
                        {
                            return await SyncSupplierItemRecordsAsync(whereClause, retailer);
                        }
                        return new SyncResult<int>
                        {
                            IsSuccess = true,
                            Data = 0,
                            Message = "No supplier item records to sync"
                        };

                    case "suppliers":
                        if (deleteRecords.Any())
                        {
                            var recordIdsToDelete = deleteRecords.Select(d => d.RecordId).Distinct().ToList();
                            var recordsToDelete = context.Suppliers
                                .Where(p => p.Supa_Id.HasValue && recordIdsToDelete.Contains(p.Supa_Id.Value))
                                .ToList();
                            context.RemoveRange(recordsToDelete);
                            await context.SaveChangesAsync();
                        }
                        if (upsertRecords.Any())
                        {
                            return await SyncSupplierRecordsAsync(whereClause, retailer);
                        }
                        return new SyncResult<int>
                        {
                            IsSuccess = true,
                            Data = 0,
                            Message = "No supplier records to sync"
                        };

                    case "tills":
                        if (deleteRecords.Any())
                        {
                            var recordIdsToDelete = deleteRecords.Select(d => d.RecordId).Distinct().ToList();
                            var recordsToDelete = context.Tills
                                .Where(p => p.Supa_Id.HasValue && recordIdsToDelete.Contains(p.Supa_Id.Value))
                                .ToList();
                            context.RemoveRange(recordsToDelete);
                            await context.SaveChangesAsync();
                        }
                        if (upsertRecords.Any())
                        {
                            return await SyncTillRecordsAsync(whereClause, retailer);
                        }
                        return new SyncResult<int>
                        {
                            IsSuccess = true,
                            Data = 0,
                            Message = "No till records to sync"
                        };

                    case "unknownproducts":
                        if (deleteRecords.Any())
                        {
                            var recordIdsToDelete = deleteRecords.Select(d => d.RecordId).Distinct().ToList();
                            var recordsToDelete = context.UnknownProducts
                                .Where(p => p.Supa_Id.HasValue && recordIdsToDelete.Contains(p.Supa_Id.Value))
                                .ToList();
                            context.RemoveRange(recordsToDelete);
                            await context.SaveChangesAsync();
                        }
                        if (upsertRecords.Any())
                        {
                            return await SyncUnknownProductRecordsAsync(whereClause, retailer);
                        }
                        return new SyncResult<int>
                        {
                            IsSuccess = true,
                            Data = 0,
                            Message = "No unknown product records to sync"
                        };

                    case "usersiteaccesses":
                        if (deleteRecords.Any())
                        {
                            var recordIdsToDelete = deleteRecords.Select(d => d.RecordId).Distinct().ToList();
                            var recordsToDelete = context.UserSiteAccesses
                                .Where(p => p.Supa_Id.HasValue && recordIdsToDelete.Contains(p.Supa_Id.Value))
                                .ToList();
                            context.RemoveRange(recordsToDelete);
                            await context.SaveChangesAsync();
                        }
                        if (upsertRecords.Any())
                        {
                            return await SyncUserSiteAccessRecordsAsync(whereClause, retailer);
                        }
                        return new SyncResult<int>
                        {
                            IsSuccess = true,
                            Data = 0,
                            Message = "No user site access records to sync"
                        };

                    case "voidedproducts":
                        if (deleteRecords.Any())
                        {
                            var recordIdsToDelete = deleteRecords.Select(d => d.RecordId).Distinct().ToList();
                            var recordsToDelete = context.VoidedProducts
                                .Where(p => p.Supa_Id.HasValue && recordIdsToDelete.Contains(p.Supa_Id.Value))
                                .ToList();
                            context.RemoveRange(recordsToDelete);
                            await context.SaveChangesAsync();
                        }
                        if (upsertRecords.Any())
                        {
                            return await SyncVoidedProductRecordsAsync(whereClause, retailer);
                        }
                        return new SyncResult<int>
                        {
                            IsSuccess = true,
                            Data = 0,
                            Message = "No voided product records to sync"
                        };

                    //case "vats":
                    //    return await SyncVatRecordsAsync(whereClause, retailer);


                    default:
                        _logger.LogWarning($"Unknown table name for sync: {tableName}");
                        return new SyncResult<int>
                        {
                            IsSuccess = false,
                            Error = $"Unknown table name: {tableName}",
                            Message = "Table not supported for sync"
                        };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception while syncing records for table {tableName}");
                return new SyncResult<int>
                {
                    IsSuccess = false,
                    Error = ex.InnerException?.ToString(),
                    Message = $"Failed to sync records for table {tableName}"
                };
            }
        }

        /// <summary>
        /// Updates the sync status of unsynced log entries
        /// </summary>
        /// <param name="unsyncedRecords">List of unsynced records to update</param>
        /// <param name="newStatus">New sync status to set</param>
        /// <param name="retailer">The retailer for authentication</param>
        /// <returns>Task representing the async operation</returns>
        private async Task UpdateUnsyncedLogsStatusAsync(List<SupaUnSyncedLogs> unsyncedRecords, SyncStatus newStatus, Retailer retailer)
        {
            try
            {
                // Update each record's status
                foreach (var record in unsyncedRecords)
                {
                    record.SyncStatus = newStatus;
                    record.LastModified = DateTime.UtcNow;
                }

                // Bulk update the unsynced logs
                var updateResult = await UpsertListAsync("UnSyncedLogs", unsyncedRecords, "Supa_Id", retailer);

                if (!updateResult.IsSuccess)
                {
                    _logger.LogError($"Failed to update unsynced logs status: {updateResult.Error}");
                }
                else
                {
                    _logger.LogInformation($"Successfully updated {unsyncedRecords.Count} unsynced log entries to status {newStatus}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while updating unsynced logs status");
            }
        }

        #region Table-Specific Sync Methods

        private async Task<SyncResult<int>> SyncProductRecordsAsync(string whereClause, Retailer retailer)
        {
            var productsResult = await GetAsync<Models.SupabaseModels.SupaRetailerProducts>(retailer, "Products", "*", whereClause);
            if (!productsResult.IsSuccess || productsResult.Data == null)
            {
                return new SyncResult<int> { IsSuccess = false, Error = productsResult.Error, Message = "Failed to fetch products" };
            }

            // Convert Supabase models to local models and save to local database
            var localProducts = new List<Product>();
            foreach (var supaProduct in productsResult.Data)
            {
                var localProduct = await MapSupaProductToLocal(supaProduct);
                localProducts.Add(localProduct);
            }

            // Save to local database
            await SaveProductsToLocalDatabaseAsync(localProducts);

            return new SyncResult<int> { IsSuccess = true, Data = localProducts.Count, Message = $"Synced {localProducts.Count} products" };
        }

        //private async Task<SyncResult<int>> SyncDepartmentRecordsAsync(string whereClause, Retailer retailer)
        //{
        //    var departmentsResult = await GetAsync<Models.SupabaseModels.SupaDepartments>("Departments", "*",  whereClause);
        //    if (!departmentsResult.IsSuccess || departmentsResult.Data == null)
        //    {
        //        return new SyncResult<int> { IsSuccess = false, Error = departmentsResult.Error, Message = "Failed to fetch departments" };
        //    }

        //    var localDepartments = new List<Department>();
        //    foreach (var supaDepartment in departmentsResult.Data)
        //    {
        //        var localDepartment = MapSupaDepartmentToLocal(supaDepartment);
        //        localDepartments.Add(localDepartment);
        //    }

        //    await SaveDepartmentsToLocalDatabaseAsync(localDepartments);

        //    return new SyncResult<int> { IsSuccess = true, Data = localDepartments.Count, Message = $"Synced {localDepartments.Count} departments" };
        //}

        private async Task<SyncResult<int>> SyncSalesTransactionRecordsAsync(string whereClause, Retailer retailer)
        {
            var salesTransactionsResult = await GetAsync<Models.SupabaseModels.SupaSalesTransactions>(retailer, "SalesTransactions", "*", whereClause);
            if (!salesTransactionsResult.IsSuccess || salesTransactionsResult.Data == null)
            {
                return new SyncResult<int> { IsSuccess = false, Error = salesTransactionsResult.Error, Message = "Failed to fetch sales transactions" };
            }

            var localSalesTransactions = new List<SalesTransaction>();
            foreach (var supaSalesTransaction in salesTransactionsResult.Data)
            {
                var localSalesTransaction = MapSupaSalesTransactionToLocal(supaSalesTransaction);
                localSalesTransactions.Add(localSalesTransaction);
            }

            await SaveSalesTransactionsToLocalDatabaseAsync(localSalesTransactions);

            return new SyncResult<int> { IsSuccess = true, Data = localSalesTransactions.Count, Message = $"Synced {localSalesTransactions.Count} sales transactions" };
        }

        private async Task<SyncResult<int>> SyncSalesItemTransactionRecordsAsync(string whereClause, Retailer retailer)
        {
            var salesItemTransactionsResult = await GetAsync<Models.SupabaseModels.SupaSalesItemsTransactions>(retailer, "SalesItemTransactions", "*", whereClause);
            if (!salesItemTransactionsResult.IsSuccess || salesItemTransactionsResult.Data == null)
            {
                return new SyncResult<int> { IsSuccess = false, Error = salesItemTransactionsResult.Error, Message = "Failed to fetch sales item transactions" };
            }

            var localSalesItemTransactions = new List<SalesItemTransaction>();
            foreach (var supaSalesItemTransaction in salesItemTransactionsResult.Data)
            {
                var localSalesItemTransaction = MapSupaSalesItemTransactionToLocal(supaSalesItemTransaction);
                localSalesItemTransactions.Add(localSalesItemTransaction);
            }

            await SaveSalesItemTransactionsToLocalDatabaseAsync(localSalesItemTransactions);

            return new SyncResult<int> { IsSuccess = true, Data = localSalesItemTransactions.Count, Message = $"Synced {localSalesItemTransactions.Count} sales item transactions" };
        }

        private async Task<SyncResult<int>> SyncShiftRecordsAsync(string whereClause, Retailer retailer)
        {
            var shiftsResult = await GetAsync<Models.SupabaseModels.SupaShifts>(retailer, "Shifts", "*", whereClause);
            if (!shiftsResult.IsSuccess || shiftsResult.Data == null)
            {
                return new SyncResult<int> { IsSuccess = false, Error = shiftsResult.Error, Message = "Failed to fetch shifts" };
            }

            var localShifts = new List<Shift>();
            foreach (var supaShift in shiftsResult.Data)
            {
                var localShift = MapSupaShiftToLocal(supaShift);
                localShifts.Add(localShift);
            }

            await SaveShiftsToLocalDatabaseAsync(localShifts);

            return new SyncResult<int> { IsSuccess = true, Data = localShifts.Count, Message = $"Synced {localShifts.Count} shifts" };
        }

        private async Task<SyncResult<int>> SyncSiteRecordsAsync(string whereClause, Retailer retailer)
        {
            var sitesResult = await GetAsync<Models.SupabaseModels.SupaSites>(retailer, "Sites", "*", whereClause);
            if (!sitesResult.IsSuccess || sitesResult.Data == null)
            {
                return new SyncResult<int> { IsSuccess = false, Error = sitesResult.Error, Message = "Failed to fetch sites" };
            }

            var localSites = new List<Site>();
            foreach (var supaSite in sitesResult.Data)
            {
                var localSite = MapSupaSiteToLocal(supaSite);
                localSites.Add(localSite);
            }

            await SaveSitesToLocalDatabaseAsync(localSites);

            return new SyncResult<int> { IsSuccess = true, Data = localSites.Count, Message = $"Synced {localSites.Count} sites" };
        }

        private async Task<SyncResult<int>> SyncStockTransactionRecordsAsync(string whereClause, Retailer retailer)
        {
            var stockTransactionsResult = await GetAsync<Models.SupabaseModels.SupaStockTransactions>(retailer, "StockTransactions", "*", whereClause);
            if (!stockTransactionsResult.IsSuccess || stockTransactionsResult.Data == null)
            {
                return new SyncResult<int> { IsSuccess = false, Error = stockTransactionsResult.Error, Message = "Failed to fetch stock transactions" };
            }

            var localStockTransactions = new List<StockTransaction>();
            foreach (var supaStockTransaction in stockTransactionsResult.Data)
            {
                var localStockTransaction = MapSupaStockTransactionToLocal(supaStockTransaction);
                localStockTransactions.Add(localStockTransaction);
            }

            await SaveStockTransactionsToLocalDatabaseAsync(localStockTransactions);

            return new SyncResult<int> { IsSuccess = true, Data = localStockTransactions.Count, Message = $"Synced {localStockTransactions.Count} stock transactions" };
        }

        private async Task<SyncResult<int>> SyncSupplierRecordsAsync(string whereClause, Retailer retailer)
        {
            var suppliersResult = await GetAsync<Models.SupabaseModels.SupaSuppliers>(retailer, "Suppliers", "*", whereClause);
            if (!suppliersResult.IsSuccess || suppliersResult.Data == null)
            {
                return new SyncResult<int> { IsSuccess = false, Error = suppliersResult.Error, Message = "Failed to fetch suppliers" };
            }

            var localSuppliers = new List<Supplier>();
            foreach (var supaSupplier in suppliersResult.Data)
            {
                var localSupplier = MapSupaSupplierToLocal(supaSupplier);
                localSuppliers.Add(localSupplier);
            }

            await SaveSuppliersToLocalDatabaseAsync(localSuppliers);

            return new SyncResult<int> { IsSuccess = true, Data = localSuppliers.Count, Message = $"Synced {localSuppliers.Count} suppliers" };
        }

        private async Task<SyncResult<int>> SyncTillRecordsAsync(string whereClause, Retailer retailer)
        {
            var tillsResult = await GetAsync<Models.SupabaseModels.SupaTills>(retailer, "Tills", "*", whereClause);
            if (!tillsResult.IsSuccess || tillsResult.Data == null)
            {
                return new SyncResult<int> { IsSuccess = false, Error = tillsResult.Error, Message = "Failed to fetch tills" };
            }

            var localTills = new List<Till>();
            foreach (var supaTill in tillsResult.Data)
            {
                var localTill = MapSupaTillToLocal(supaTill);
                localTills.Add(localTill);
            }

            await SaveTillsToLocalDatabaseAsync(localTills);

            return new SyncResult<int> { IsSuccess = true, Data = localTills.Count, Message = $"Synced {localTills.Count} tills" };
        }

        //private async Task<SyncResult<int>> SyncVatRecordsAsync(string whereClause, Retailer retailer)
        //{
        //    var vatsResult = await GetAsync<Models.SupabaseModels.SupaVats>("Vats", "*",  whereClause);
        //    if (!vatsResult.IsSuccess || vatsResult.Data == null)
        //    {
        //        return new SyncResult<int> { IsSuccess = false, Error = vatsResult.Error, Message = "Failed to fetch vats" };
        //    }

        //    var localVats = new List<Vat>();
        //    foreach (var supaVat in vatsResult.Data)
        //    {
        //        var localVat = MapSupaVatToLocal(supaVat);
        //        localVats.Add(localVat);
        //    }

        //    await SaveVatsToLocalDatabaseAsync(localVats);

        //    return new SyncResult<int> { IsSuccess = true, Data = localVats.Count, Message = $"Synced {localVats.Count} vats" };
        //}

        private async Task<SyncResult<int>> SyncPosUserRecordsAsync(string whereClause, Retailer retailer)
        {
            var posUsersResult = await GetAsync<Models.SupabaseModels.SupaPosUsers>(retailer, "PosUsers", "*", whereClause);
            if (!posUsersResult.IsSuccess || posUsersResult.Data == null)
            {
                return new SyncResult<int> { IsSuccess = false, Error = posUsersResult.Error, Message = "Failed to fetch pos users" };
            }

            var localPosUsers = new List<PosUser>();
            foreach (var supaPosUser in posUsersResult.Data)
            {
                var localPosUser = MapSupaPosUserToLocal(supaPosUser);
                localPosUsers.Add(localPosUser);
            }

            await SavePosUsersToLocalDatabaseAsync(localPosUsers);

            return new SyncResult<int> { IsSuccess = true, Data = localPosUsers.Count, Message = $"Synced {localPosUsers.Count} pos users" };
        }

        private async Task<SyncResult<int>> SyncPromotionRecordsAsync(string whereClause, Retailer retailer)
        {
            var promotionsResult = await GetAsync<Models.SupabaseModels.SupaPromotions>(retailer, "Promotions", "*", whereClause);
            if (!promotionsResult.IsSuccess || promotionsResult.Data == null)
            {
                return new SyncResult<int> { IsSuccess = false, Error = promotionsResult.Error, Message = "Failed to fetch promotions" };
            }

            var localPromotions = new List<Promotion>();
            foreach (var supaPromotion in promotionsResult.Data)
            {
                var localPromotion = MapSupaPromotionToLocal(supaPromotion);
                localPromotions.Add(localPromotion);
            }

            await SavePromotionsToLocalDatabaseAsync(localPromotions);

            return new SyncResult<int> { IsSuccess = true, Data = localPromotions.Count, Message = $"Synced {localPromotions.Count} promotions" };
        }

        private async Task<SyncResult<int>> SyncReceiptPrinterRecordsAsync(string whereClause, Retailer retailer)
        {
            var receiptPrintersResult = await GetAsync<Models.SupabaseModels.SupaReceiptPrinters>(retailer, "ReceiptPrinters", "*", whereClause);
            if (!receiptPrintersResult.IsSuccess || receiptPrintersResult.Data == null)
            {
                return new SyncResult<int> { IsSuccess = false, Error = receiptPrintersResult.Error, Message = "Failed to fetch receipt printers" };
            }

            var localReceiptPrinters = new List<ReceiptPrinter>();
            foreach (var supaReceiptPrinter in receiptPrintersResult.Data)
            {
                var localReceiptPrinter = MapSupaReceiptPrinterToLocal(supaReceiptPrinter);
                localReceiptPrinters.Add(localReceiptPrinter);
            }

            await SaveReceiptPrintersToLocalDatabaseAsync(localReceiptPrinters);

            return new SyncResult<int> { IsSuccess = true, Data = localReceiptPrinters.Count, Message = $"Synced {localReceiptPrinters.Count} receipt printers" };
        }

        private async Task<SyncResult<int>> SyncDayLogRecordsAsync(string whereClause, Retailer retailer)
        {
            var dayLogsResult = await GetAsync<Models.SupabaseModels.SupaDayLogs>(retailer, "DayLogs", "*", whereClause);
            if (!dayLogsResult.IsSuccess || dayLogsResult.Data == null)
            {
                return new SyncResult<int> { IsSuccess = false, Error = dayLogsResult.Error, Message = "Failed to fetch day logs" };
            }

            // Convert Supabase models to local models and save to local database
            var localDayLogs = new List<DayLog>();
            foreach (var supaDayLog in dayLogsResult.Data)
            {
                var localDayLog = MapSupaDayLogToLocal(supaDayLog);
                localDayLogs.Add(localDayLog);
            }

            // Save to local database
            await SaveDayLogsToLocalDatabaseAsync(localDayLogs);

            return new SyncResult<int> { IsSuccess = true, Data = localDayLogs.Count, Message = $"Synced {localDayLogs.Count} day logs" };
        }

        private async Task<SyncResult<int>> SyncRetailerRecordAsync(Retailer retailer)
        {
            var where = $"RetailerId=eq.{retailer.RetailerId}";
            var supaRetailerResult = await GetAsync<Models.SupabaseModels.SupaRetailers>(retailer, "Retailers", "*", where);
            if (!supaRetailerResult.IsSuccess || supaRetailerResult.Data == null || supaRetailerResult.Data.Count == 0)
            {
                return new SyncResult<int> { IsSuccess = false, Error = supaRetailerResult.Error, Message = "Failed to fetch retailer record" };
            }

            var supaRetailer = supaRetailerResult.Data.First();
            var localRetailer = MapSupaRetailerToLocal(supaRetailer);
            await SaveRetailerToLocalDatabaseAsync(localRetailer);

            return new SyncResult<int> { IsSuccess = true, Data = 1, Message = "Synced retailer record" };
        }

        private async Task<SyncResult<int>> SyncDrawerLogRecordsAsync(string whereClause, Retailer retailer)
        {
            var drawerLogsResult = await GetAsync<Models.SupabaseModels.SupaDrawerLogs>(retailer, "DrawerLogs", "*", whereClause);
            if (!drawerLogsResult.IsSuccess || drawerLogsResult.Data == null)
            {
                return new SyncResult<int> { IsSuccess = false, Error = drawerLogsResult.Error, Message = "Failed to fetch drawer logs" };
            }

            // Convert Supabase models to local models and save to local database
            var localDrawerLogs = new List<DrawerLog>();
            foreach (var supaDrawerLog in drawerLogsResult.Data)
            {
                var localDrawerLog = MapSupaDrawerLogToLocal(supaDrawerLog);
                localDrawerLogs.Add(localDrawerLog);
            }

            // Save to local database
            await SaveDrawerLogsToLocalDatabaseAsync(localDrawerLogs);

            return new SyncResult<int> { IsSuccess = true, Data = localDrawerLogs.Count, Message = $"Synced {localDrawerLogs.Count} drawer logs" };
        }

        private async Task<SyncResult<int>> SyncErrorLogRecordsAsync(string whereClause, Retailer retailer)
        {
            var errorLogsResult = await GetAsync<Models.SupabaseModels.SupaErrorLogs>(retailer, "ErrorLogs", "*", whereClause);
            if (!errorLogsResult.IsSuccess || errorLogsResult.Data == null)
            {
                return new SyncResult<int> { IsSuccess = false, Error = errorLogsResult.Error, Message = "Failed to fetch error logs" };
            }

            // Convert Supabase models to local models and save to local database
            var localErrorLogs = new List<ErrorLog>();
            foreach (var supaErrorLog in errorLogsResult.Data)
            {
                var localErrorLog = MapSupaErrorLogToLocal(supaErrorLog);
                localErrorLogs.Add(localErrorLog);
            }

            // Save to local database
            await SaveErrorLogsToLocalDatabaseAsync(localErrorLogs);

            return new SyncResult<int> { IsSuccess = true, Data = localErrorLogs.Count, Message = $"Synced {localErrorLogs.Count} error logs" };
        }

        private async Task<SyncResult<int>> SyncPayoutRecordsAsync(string whereClause, Retailer retailer)
        {
            var payoutsResult = await GetAsync<Models.SupabaseModels.SupaPayouts>(retailer, "Payouts", "*", whereClause);
            if (!payoutsResult.IsSuccess || payoutsResult.Data == null)
            {
                return new SyncResult<int> { IsSuccess = false, Error = payoutsResult.Error, Message = "Failed to fetch payouts" };
            }

            // Convert Supabase models to local models and save to local database
            var localPayouts = new List<Payout>();
            foreach (var supaPayout in payoutsResult.Data)
            {
                var localPayout = MapSupaPayoutToLocal(supaPayout);
                localPayouts.Add(localPayout);
            }

            // Save to local database
            await SavePayoutsToLocalDatabaseAsync(localPayouts);

            return new SyncResult<int> { IsSuccess = true, Data = localPayouts.Count, Message = $"Synced {localPayouts.Count} payouts" };
        }

        private async Task<SyncResult<int>> SyncStockRefillRecordsAsync(string whereClause, Retailer retailer)
        {
            var stockRefillsResult = await GetAsync<Models.SupabaseModels.SupaStockRefills>(retailer, "StockRefills", "*", whereClause);
            if (!stockRefillsResult.IsSuccess || stockRefillsResult.Data == null)
            {
                return new SyncResult<int> { IsSuccess = false, Error = stockRefillsResult.Error, Message = "Failed to fetch stock refills" };
            }

            // Convert Supabase models to local models and save to local database
            var localStockRefills = new List<StockRefill>();
            foreach (var supaStockRefill in stockRefillsResult.Data)
            {
                var localStockRefill = MapSupaStockRefillToLocal(supaStockRefill);
                localStockRefills.Add(localStockRefill);
            }

            // Save to local database
            await SaveStockRefillsToLocalDatabaseAsync(localStockRefills);

            return new SyncResult<int> { IsSuccess = true, Data = localStockRefills.Count, Message = $"Synced {localStockRefills.Count} stock refills" };
        }

        private async Task<SyncResult<int>> SyncSupplierItemRecordsAsync(string whereClause, Retailer retailer)
        {
            var supplierItemsResult = await GetAsync<Models.SupabaseModels.SupaSupplierItems>(retailer, "SupplierItems", "*", whereClause);
            if (!supplierItemsResult.IsSuccess || supplierItemsResult.Data == null)
            {
                return new SyncResult<int> { IsSuccess = false, Error = supplierItemsResult.Error, Message = "Failed to fetch supplier items" };
            }

            // Convert Supabase models to local models and save to local database
            var localSupplierItems = new List<SupplierItem>();
            foreach (var supaSupplierItem in supplierItemsResult.Data)
            {
                var localSupplierItem = MapSupaSupplierItemToLocal(supaSupplierItem);
                localSupplierItems.Add(localSupplierItem);
            }

            // Save to local database
            await SaveSupplierItemsToLocalDatabaseAsync(localSupplierItems);

            return new SyncResult<int> { IsSuccess = true, Data = localSupplierItems.Count, Message = $"Synced {localSupplierItems.Count} supplier items" };
        }

        private async Task<SyncResult<int>> SyncUnknownProductRecordsAsync(string whereClause, Retailer retailer)
        {
            var unknownProductsResult = await GetAsync<Models.SupabaseModels.SupaUnknownProduct>(retailer, "UnknownProducts", "*", whereClause);
            if (!unknownProductsResult.IsSuccess || unknownProductsResult.Data == null)
            {
                return new SyncResult<int> { IsSuccess = false, Error = unknownProductsResult.Error, Message = "Failed to fetch unknown products" };
            }

            // Convert Supabase models to local models and save to local database
            var localUnknownProducts = new List<Models.UnknownProduct>();
            foreach (var supaUnknownProduct in unknownProductsResult.Data)
            {
                var localUnknownProduct = MapSupaUnknownProductToLocal(supaUnknownProduct);
                localUnknownProducts.Add(localUnknownProduct);
            }
            string whereStatement = $"ProductBarcode=in.({string.Join(",", localUnknownProducts.Select(p => $"{p.ProductBarcode}"))})";
            if (localUnknownProducts.Count == 1)
            {
                whereStatement = $"ProductBarcode=eq.{localUnknownProducts[0].ProductBarcode}";
            }
            var productResult = await GetGlobalProducts(
                retailer, whereStatement);

            if (productResult.Error != null)
            {
                throw new Exception($"Products fetch error: {productResult.Error}");
            }

            var productsToAdd = new List<Product>();

            foreach (var sp in productResult.Data ?? new List<SupaGlobalProducts>())
            {
                var barcode = sp.ProductBarcode ?? string.Empty;
                if (string.IsNullOrWhiteSpace(barcode))
                {
                    continue;
                }

                // map department by name; fallback to default
                var deptName = string.IsNullOrWhiteSpace(sp.ProductDepartmentName) ? "Default" : sp.ProductDepartmentName;
                var department = GetDepartmentID;

                var unitsPerCase = (int)(sp.ProductUnitsPerCase ?? 0);
                var costPerCase = (decimal)(sp.ProductCostPerCase ?? 0.0);
                var unitCost = unitsPerCase > 0 ? Math.Round(costPerCase / unitsPerCase, 2) : 0m;
                var product = new EntityFrameworkDatabaseLibrary.Models.Product
                {
                    GlobalId = sp.Id,
                    Product_Name = sp.ProductName ?? barcode,
                    Product_Description = null,
                    Product_Barcode = barcode,
                    Product_Case_Barcode = null,
                    ShelfQuantity = 0,
                    StockroomQuantity = 0,
                    Department_ID = await GetDepartmentID(sp.ProductDepartmentName ?? ""),
                    VAT_ID = await GetVATID((decimal)(sp.ProductVatValue ?? 0.0)),
                    Product_Cost = unitCost,
                    Product_Selling_Price = (decimal)(sp.ProductRetailPrice ?? 0.0),
                    Profit_On_Return_Percentage = 0,
                    Product_Size = sp.ProductMeasurement,
                    Product_Measurement = sp.ProductMeasurementType,
                    Promotion_Id = null,
                    Brand_Name = null,
                    Product_Min_Order = 0,
                    Product_Low_Stock_Alert_QTY = 0,
                    Product_Min_Stock_Level = 0,
                    Product_Unit_Per_Case = unitsPerCase,
                    Product_Cost_Per_Case = costPerCase,
                    Expiry_Date = DateTime.UtcNow.AddYears(100),
                    Is_Activated = sp.Active ?? true,
                    Is_Deleted = sp.Deleted ?? false,
                    Priced_Changed_On = DateTime.UtcNow,
                    Is_Price_Changed = false,
                    Date_Created = sp.CreatedAt.ToUniversalTime(),
                    Last_Modified = sp.LastModified_At.ToUniversalTime(),
                    Allow_Discount = true,
                    Created_By_Id = await _userSessionService.GetValidUserIdAsync(),
                    Last_Modified_By_Id = await _userSessionService.GetValidUserIdAsync(),
                    Site_Id = await _userSessionService.GetValidSiteIdAsync(),
                    Till_Id = await _userSessionService.GetValidTillIdAsync(),
                    SyncStatus = SyncStatus.Synced
                };

                productsToAdd.Add(product);
            }

            if (productsToAdd.Any())
            {
                productsToAdd.ForEach(p => p.Is_Activated = false);

                await _productServices.BulkUpsert(productsToAdd);
            }

            // Save to local database
            await SaveUnknownProductsToLocalDatabaseAsync(localUnknownProducts);

            await SyncUnknownProductsAsync(localUnknownProducts, retailer);

            return new SyncResult<int> { IsSuccess = true, Data = localUnknownProducts.Count, Message = $"Synced {localUnknownProducts.Count} unknown products" };
        }

        private async Task<SyncResult<int>> SyncUserSiteAccessRecordsAsync(string whereClause, Retailer retailer)
        {
            var userSiteAccessResult = await GetAsync<Models.SupabaseModels.SupaUserSiteAccesses>(retailer, "UserSiteAccesses", "*", whereClause);
            if (!userSiteAccessResult.IsSuccess || userSiteAccessResult.Data == null)
            {
                return new SyncResult<int> { IsSuccess = false, Error = userSiteAccessResult.Error, Message = "Failed to fetch user site access records" };
            }

            // Convert Supabase models to local models and save to local database
            var localUserSiteAccesses = new List<UserSiteAccess>();
            foreach (var supaUserSiteAccess in userSiteAccessResult.Data)
            {
                var localUserSiteAccess = MapSupaUserSiteAccessToLocal(supaUserSiteAccess);
                localUserSiteAccesses.Add(localUserSiteAccess);
            }

            // Save to local database
            await SaveUserSiteAccessesToLocalDatabaseAsync(localUserSiteAccesses);

            return new SyncResult<int> { IsSuccess = true, Data = localUserSiteAccesses.Count, Message = $"Synced {localUserSiteAccesses.Count} user site access records" };
        }

        private async Task<SyncResult<int>> SyncVoidedProductRecordsAsync(string whereClause, Retailer retailer)
        {
            var voidedProductResult = await GetAsync<Models.SupabaseModels.SupaVoidedProducts>(retailer, "VoidedProducts", "*", whereClause);
            if (!voidedProductResult.IsSuccess || voidedProductResult.Data == null)
            {
                return new SyncResult<int> { IsSuccess = false, Error = voidedProductResult.Error, Message = "Failed to fetch voided product records" };
            }

            // Convert Supabase models to local models and save to local database
            var localVoidedProducts = new List<VoidedProduct>();
            foreach (var supaVoidedProduct in voidedProductResult.Data)
            {
                var localVoidedProduct = MapSupaVoidedProductToLocal(supaVoidedProduct);
                localVoidedProducts.Add(localVoidedProduct);
            }

            // Save to local database
            await SaveVoidedProductsToLocalDatabaseAsync(localVoidedProducts);

            return new SyncResult<int> { IsSuccess = true, Data = localVoidedProducts.Count, Message = $"Synced {localVoidedProducts.Count} voided product records" };
        }

        private async Task<Product> MapSupaProductToLocal(Models.SupabaseModels.SupaRetailerProducts supaProduct)
        {
            return new Product
            {
                Supa_Id = supaProduct.Supa_Id,
                Id = supaProduct.Product_Id,
                ShelfQuantity = supaProduct.ShelfQuantity,
                StockroomQuantity = supaProduct.StockroomQuantity,
                Product_Cost = supaProduct.Product_Cost,
                Product_Selling_Price = supaProduct.Product_Selling_Price,
                Profit_On_Return_Percentage = supaProduct.Profit_On_Return_Percentage,
                Promotion_Id = supaProduct.Promotion_Id,
                Product_Unit_Per_Case = supaProduct.Product_Unit_Per_Case,
                Product_Cost_Per_Case = (decimal)supaProduct.Product_Cost_Per_Case,
                Expiry_Date = supaProduct.Expiry_Date.ToUniversalTime(),
                Is_Activated = supaProduct.Is_Activated,
                Is_Deleted = supaProduct.Is_Deleted,
                Priced_Changed_On = supaProduct.Priced_Changed_On.ToUniversalTime(),
                Is_Price_Changed = supaProduct.Is_Price_Changed,
                Date_Created = supaProduct.Date_Created.ToUniversalTime(),
                Last_Modified = supaProduct.Last_Modified.ToUniversalTime(),
                Allow_Discount = supaProduct.Allow_Discount,
                Created_By_Id = supaProduct.Created_By_Id,
                Last_Modified_By_Id = supaProduct.Last_Modified_By_Id,
                Site_Id = supaProduct.Site_Id,
                Till_Id = supaProduct.Till_Id,
                Department_ID = await GetDepartmentID(supaProduct.DepartmentName),
                VAT_ID = await GetVATID(supaProduct.VatValue),
                Product_Name = supaProduct.ProductName,
                Product_Barcode = supaProduct.ProductBarcode,
                SyncStatus = SyncStatus.Synced,
            };


        }

        //private Department MapSupaDepartmentToLocal(Models.SupabaseModels.SupaDepartments supaDepartment)
        //{
        //    return new Department
        //    {
        //        Supa_Id = supaDepartment.Supa_Id,
        //        Department_ID = supaDepartment.Department_ID,
        //        Department_Name = supaDepartment.Department_Name,
        //        Department_Description = supaDepartment.Department_Description,
        //        Department_IsActive = supaDepartment.Department_IsActive,
        //        Date_Created = supaDepartment.Date_Created.ToUniversalTime(),
        //        Last_Modified = supaDepartment.Last_Modified.ToUniversalTime(),
        //        Created_By_Id = supaDepartment.Created_By_Id,
        //        Last_Modified_By_Id = supaDepartment.Last_Modified_By_Id,
        //        
        //        SyncStatus = SyncStatus.Synced,
        //        
        //       
        //        SyncVersion = supaDepartment.SyncVersion
        //    };
        //}

        private SalesTransaction MapSupaSalesTransactionToLocal(Models.SupabaseModels.SupaSalesTransactions supaSalesTransaction)
        {
            return new SalesTransaction
            {
                Supa_Id = supaSalesTransaction.Supa_Id,
                Id = supaSalesTransaction.SaleTransaction_ID,
                SaleTransaction_Total_QTY = supaSalesTransaction.SaleTransaction_Total_QTY,
                SaleTransaction_Total_Amount = supaSalesTransaction.SaleTransaction_Total_Amount,
                SaleTransaction_Total_Paid = supaSalesTransaction.SaleTransaction_Total_Paid,
                SaleTransaction_Cash = supaSalesTransaction.SaleTransaction_Cash,
                SaleTransaction_Card = supaSalesTransaction.SaleTransaction_Card,
                SaleTransaction_Refund = supaSalesTransaction.SaleTransaction_Refund,
                SaleTransaction_Discount = supaSalesTransaction.SaleTransaction_Discount,
                SaleTransaction_Promotion = supaSalesTransaction.SaleTransaction_Promotion,
                SaleTransaction_Change = supaSalesTransaction.SaleTransaction_Change,
                SaleTransaction_Payout = supaSalesTransaction.SaleTransaction_Payout,
                SaleTransaction_CashBack = supaSalesTransaction.SaleTransaction_CashBack,
                SaleTransaction_Card_Charges = supaSalesTransaction.SaleTransaction_Card_Charges,
                DayLog_Id = supaSalesTransaction.DayLog_Id,
                Is_Printed = supaSalesTransaction.Is_Printed,
                Shift_Id = supaSalesTransaction.Shift_Id,
                Date_Created = supaSalesTransaction.Date_Created.ToUniversalTime(),
                Last_Modified = supaSalesTransaction.Last_Modified.ToUniversalTime(),
                Sale_Start_Date = supaSalesTransaction.Sale_Start_Date.ToUniversalTime(),
                Created_By_Id = supaSalesTransaction.Created_By_Id,
                Last_Modified_By_Id = supaSalesTransaction.Last_Modified_By_Id,
                Site_Id = supaSalesTransaction.Site_Id,
                Till_Id = supaSalesTransaction.Till_Id,
                SyncStatus = SyncStatus.Synced,
            };
        }

        private SalesItemTransaction MapSupaSalesItemTransactionToLocal(Models.SupabaseModels.SupaSalesItemsTransactions supaSalesItemTransaction)
        {
            return new SalesItemTransaction
            {
                Supa_Id = supaSalesItemTransaction.Supa_Id,
                Id = supaSalesItemTransaction.SaleTransaction_Item_ID,
                SaleTransaction_ID = supaSalesItemTransaction.SaleTransaction_ID,
                Product_ID = supaSalesItemTransaction.Product_ID,
                Product_QTY = supaSalesItemTransaction.Product_QTY,
                Product_Amount = supaSalesItemTransaction.Product_Amount,
                Product_Total_Amount = supaSalesItemTransaction.Product_Total_Amount,
                SalesItemTransactionType = supaSalesItemTransaction.SalesItemTransactionType,
                SalesPayout_ID = supaSalesItemTransaction.SalesPayout_ID,
                Promotion_ID = supaSalesItemTransaction.Promotion_ID,
                Product_Total_Amount_Before_Discount = supaSalesItemTransaction.Product_Total_Amount_Before_Discount,
                Discount_Percent = supaSalesItemTransaction.Discount_Percent,
                Discount_Amount = supaSalesItemTransaction.Discount_Amount,
                Is_Manual_Weight_Entry = supaSalesItemTransaction.Is_Manual_Weight_Entry,
                Date_Created = supaSalesItemTransaction.Date_Created.ToUniversalTime(),
                Last_Modified = supaSalesItemTransaction.Last_Modified.ToUniversalTime(),
                Created_By_Id = supaSalesItemTransaction.Created_By_Id,
                Last_Modified_By_Id = supaSalesItemTransaction.Last_Modified_By_Id,
                SyncStatus = SyncStatus.Synced,
            };
        }

        private Shift MapSupaShiftToLocal(Models.SupabaseModels.SupaShifts supaShift)
        {
            return new Shift
            {
                Supa_Id = supaShift.Supa_Id,
                Id = supaShift.Shift_Id,
                DayLog_Id = supaShift.DayLog_Id,
                PosUser_Id = supaShift.PosUser_Id,
                Shift_Start_DateTime = supaShift.Shift_Start_DateTime.ToUniversalTime(),
                Shift_End_DateTime = supaShift.Shift_End_DateTime != null ? supaShift.Shift_End_DateTime.Value.ToUniversalTime() : null,
                Opening_Cash_Amount = supaShift.Opening_Cash_Amount,
                Closing_Cash_Amount = supaShift.Closing_Cash_Amount,
                Expected_Cash_Amount = supaShift.Expected_Cash_Amount,
                Cash_Variance = supaShift.Cash_Variance,
                Is_Active = supaShift.Is_Active,
                Shift_Notes = supaShift.Shift_Notes,
                Closing_Notes = supaShift.Closing_Notes,
                Date_Created = supaShift.Date_Created.ToUniversalTime(),
                Last_Modified = supaShift.Last_Modified.ToUniversalTime(),
                Created_By_Id = supaShift.Created_By_Id,
                Last_Modified_By_Id = supaShift.Last_Modified_By_Id,
                Site_Id = supaShift.Site_Id,
                Till_Id = supaShift.Till_Id,
                SyncStatus = SyncStatus.Synced,
            };
        }

        private Site MapSupaSiteToLocal(Models.SupabaseModels.SupaSites supaSite)
        {
            return new Site
            {
                Supa_Id = supaSite.Supa_Id,
                Id = supaSite.Site_Id,
                Site_BusinessName = supaSite.Site_BusinessName,
                Site_AddressLine1 = supaSite.Site_AddressLine1,
                Site_AddressLine2 = supaSite.Site_AddressLine2,
                Site_City = supaSite.Site_City,
                Site_County = supaSite.Site_County,
                Site_Postcode = supaSite.Site_Postcode,
                Site_Country = supaSite.Site_Country,
                Site_ContactNumber = supaSite.Site_ContactNumber,
                Site_Email = supaSite.Site_Email,
                Site_VatNumber = supaSite.Site_VatNumber,
                Is_Active = supaSite.Is_Active,
                Is_Deleted = supaSite.Is_Deleted,
                Is_Primary = supaSite.Is_Primary,
                Date_Created = supaSite.Date_Created.ToUniversalTime(),
                Last_Modified = supaSite.Last_Modified.ToUniversalTime(),
                Created_By_Id = supaSite.Created_By_Id,
                Last_Modified_By_Id = supaSite.Last_Modified_By_Id,
                SyncStatus = SyncStatus.Synced,
            };
        }

        private StockTransaction MapSupaStockTransactionToLocal(Models.SupabaseModels.SupaStockTransactions supaStockTransaction)
        {
            return new StockTransaction
            {
                Supa_Id = supaStockTransaction.Supa_Id,
                Id = supaStockTransaction.StockTransactionId,
                ProductId = supaStockTransaction.ProductId,
                Quantity = supaStockTransaction.Quantity,
                TotalAmount = supaStockTransaction.TotalAmount,
                DayLogId = supaStockTransaction.DayLogId,
                TransactionDate = supaStockTransaction.TransactionDate.ToUniversalTime(),
                DateCreated = supaStockTransaction.DateCreated.ToUniversalTime(),
                LastModified = supaStockTransaction.LastModified.ToUniversalTime(),
                From_Site_Id = supaStockTransaction.From_Site_Id,
                To_Site_Id = supaStockTransaction.To_Site_Id,
                Till_Id = supaStockTransaction.Till_Id,
                Created_By_Id = supaStockTransaction.Created_By_Id,
                Last_Modified_By_Id = supaStockTransaction.Last_Modified_By_Id,
                SyncStatus = SyncStatus.Synced,
            };
        }

        private Supplier MapSupaSupplierToLocal(Models.SupabaseModels.SupaSuppliers supaSupplier)
        {
            return new Supplier
            {
                Supa_Id = supaSupplier.Supa_Id,
                Id = supaSupplier.Supplier_ID,
                Supplier_Name = supaSupplier.Supplier_Name,
                Supplier_Description = supaSupplier.Supplier_Description,
                Supplier_Address = supaSupplier.Supplier_Address,
                Supplier_Phone = supaSupplier.Supplier_Phone,
                Supplier_Mobile = supaSupplier.Supplier_Mobile,
                Supplier_Email = supaSupplier.Supplier_Email,
                Supplier_Website = supaSupplier.Supplier_Website,
                Supplier_Credit_Limit = supaSupplier.Supplier_Credit_Limit,
                Is_Activated = supaSupplier.Is_Activated,
                Date_Created = supaSupplier.Date_Created.ToUniversalTime(),
                Last_Modified = supaSupplier.Last_Modified.ToUniversalTime(),
                Created_By_Id = supaSupplier.Created_By_Id,
                Last_Modified_By_Id = supaSupplier.Last_Modified_By_Id,
                Site_Id = supaSupplier.Site_Id,
                Till_Id = supaSupplier.Till_Id,
                SyncStatus = SyncStatus.Synced,
            };
        }

        private Till MapSupaTillToLocal(Models.SupabaseModels.SupaTills supaTill)
        {
            return new Till
            {
                Supa_Id = supaTill.Supa_Id,
                Id = supaTill.Till_Id,
                Till_Name = supaTill.Till_Name,
                Till_IP_Address = supaTill.Till_IP_Address,
                Till_Port_Number = supaTill.Till_Port_Number,
                Till_Password = supaTill.Till_Password,
                Is_Primary = supaTill.Is_Primary,
                Is_Active = supaTill.Is_Active,
                Is_Deleted = supaTill.Is_Deleted,
                Site_Id = supaTill.Site_Id,
                Date_Created = supaTill.Date_Created.ToUniversalTime(),
                Last_Modified = supaTill.Last_Modified.ToUniversalTime(),
                Created_By_Id = supaTill.Created_By_Id,
                Last_Modified_By_Id = supaTill.Last_Modified_By_Id,
                SyncStatus = SyncStatus.Synced,
            };
        }

        //private Vat MapSupaVatToLocal(Models.SupabaseModels.SupaVats supaVat)
        //{
        //    return new Vat
        //    {
        //        Supa_Id = supaVat.Supa_Id,
        //        VAT_ID = supaVat.VAT_ID,
        //        VAT_Name = supaVat.VAT_Name,
        //        VAT_Value = supaVat.VAT_Value,
        //        Vat_IsActive = supaVat.Vat_IsActive,
        //        Date_Created = supaVat.Date_Created.ToUniversalTime(),
        //        Last_Modified = supaVat.Last_Modified.ToUniversalTime(),
        //        Created_By_Id = supaVat.Created_By_Id,
        //        Last_Modified_By_Id = supaVat.Last_Modified_By_Id,
        //        
        //        SyncStatus = SyncStatus.Synced,
        //        
        //       
        //        SyncVersion = supaVat.SyncVersion
        //    };
        //}

        private PosUser MapSupaPosUserToLocal(Models.SupabaseModels.SupaPosUsers supaPosUser)
        {
            return new PosUser
            {
                Supa_Id = supaPosUser.Supa_Id,
                Id = supaPosUser.User_ID,
                First_Name = supaPosUser.First_Name,
                Last_Name = supaPosUser.Last_Name,
                Passcode = supaPosUser.Passcode,
                Primary_Site_Id = supaPosUser.Primary_Site_Id,
                Allowed_Void_Line = supaPosUser.Allowed_Void_Line,
                Allowed_Void_Sale = supaPosUser.Allowed_Void_Sale,
                Allowed_No_Sale = supaPosUser.Allowed_No_Sale,
                Allowed_Returns = supaPosUser.Allowed_Returns,
                Allowed_Payout = supaPosUser.Allowed_Payout,
                Allowed_Refund = supaPosUser.Allowed_Refund,
                Allowed_Change_Price = supaPosUser.Allowed_Change_Price,
                Allowed_Discount = supaPosUser.Allowed_Discount,
                Allowed_Override_Price = supaPosUser.Allowed_Override_Price,
                Allowed_Manage_Users = supaPosUser.Allowed_Manage_Users,
                Allowed_Manage_Sites = supaPosUser.Allowed_Manage_Sites,
                Allowed_Manage_Tills = supaPosUser.Allowed_Manage_Tills,
                Allowed_Manage_Products = supaPosUser.Allowed_Manage_Products,
                Allowed_Manage_Suppliers = supaPosUser.Allowed_Manage_Suppliers,
                Allowed_Manage_StockTransfer = supaPosUser.Allowed_Manage_StockTransfer,
                Allowed_Manage_Vat = supaPosUser.Allowed_Manage_Vat,
                Allowed_Manage_Departments = supaPosUser.Allowed_Manage_Departments,
                Allowed_Manage_Orders = supaPosUser.Allowed_Manage_Orders,
                Allowed_Manage_Reports = supaPosUser.Allowed_Manage_Reports,
                Allowed_Manage_Settings = supaPosUser.Allowed_Manage_Settings,
                Allowed_Manage_Tax_Rates = supaPosUser.Allowed_Manage_Tax_Rates,
                Allowed_Manage_Promotions = supaPosUser.Allowed_Manage_Promotions,
                Allowed_Manage_VoidedProducts = supaPosUser.Allowed_Manage_VoidedProducts,
                Allowed_Day_End = supaPosUser.Allowed_Day_End,
                Is_Activated = supaPosUser.Is_Activated,
                Is_Deleted = supaPosUser.Is_Deleted,
                Date_Created = supaPosUser.Date_Created.ToUniversalTime(),
                Last_Modified = supaPosUser.Last_Modified.ToUniversalTime(),
                Created_By_Id = supaPosUser.Created_By_Id,
                Last_Modified_By_Id = supaPosUser.Last_Modified_By_Id,
                Site_Id = supaPosUser.Site_Id,
                Till_Id = supaPosUser.Till_Id,
                SyncStatus = SyncStatus.Synced,
                User_Type = supaPosUser.User_Type

            };
        }

        private Promotion MapSupaPromotionToLocal(Models.SupabaseModels.SupaPromotions supaPromotion)
        {
            return new Promotion
            {
                Supa_Id = supaPromotion.Supa_Id,
                Id = supaPromotion.Promotion_ID,
                Promotion_Name = supaPromotion.Promotion_Name,
                Promotion_Description = supaPromotion.Promotion_Description,
                Buy_Quantity = supaPromotion.Buy_Quantity,
                Free_Quantity = supaPromotion.Free_Quantity,
                Discount_Percentage = supaPromotion.Discount_Percentage,
                Discount_Amount = supaPromotion.Discount_Amount,
                Minimum_Spend_Amount = supaPromotion.Minimum_Spend_Amount,
                Start_Date = supaPromotion.Start_Date.ToUniversalTime(),
                End_Date = supaPromotion.End_Date.ToUniversalTime(),
                Promotion_Type = supaPromotion.Promotion_Type,
                Is_Deleted = supaPromotion.Is_Deleted,
                Created_Date = supaPromotion.Created_Date.ToUniversalTime(),
                Last_Modified = supaPromotion.Last_Modified.ToUniversalTime(),
                Created_By_Id = supaPromotion.Created_By_Id,
                Last_Modified_By_Id = supaPromotion.Last_Modified_By_Id,
                Site_Id = supaPromotion.Site_Id,
                Till_Id = supaPromotion.Till_Id,
                SyncStatus = SyncStatus.Synced,
            };
        }

        private ReceiptPrinter MapSupaReceiptPrinterToLocal(Models.SupabaseModels.SupaReceiptPrinters supaReceiptPrinter)
        {
            return new ReceiptPrinter
            {
                Supa_Id = supaReceiptPrinter.Supa_Id,
                Id = supaReceiptPrinter.Printer_Id,
                Printer_Name = supaReceiptPrinter.Printer_Name,
                Printer_IP_Address = supaReceiptPrinter.Printer_IP_Address,
                Printer_Port_Number = supaReceiptPrinter.Printer_Port_Number,
                Printer_Password = supaReceiptPrinter.Printer_Password,
                Paper_Width = supaReceiptPrinter.Paper_Width,
                Site_Id = supaReceiptPrinter.Site_Id,
                Till_Id = supaReceiptPrinter.Till_Id,
                Is_Primary = supaReceiptPrinter.Is_Primary,
                Is_Active = supaReceiptPrinter.Is_Active,
                Is_Deleted = supaReceiptPrinter.Is_Deleted,
                Print_Receipt = supaReceiptPrinter.Print_Receipt,
                Print_Label = supaReceiptPrinter.Print_Label,
                Date_Created = supaReceiptPrinter.Date_Created.ToUniversalTime(),
                Last_Modified = supaReceiptPrinter.Last_Modified.ToUniversalTime(),
                Created_By_Id = supaReceiptPrinter.Created_By_Id,
                Last_Modified_By_Id = supaReceiptPrinter.Last_Modified_By_Id,
                SyncStatus = SyncStatus.Synced,
            };
        }

        private DayLog MapSupaDayLogToLocal(Models.SupabaseModels.SupaDayLogs supaDayLog)
        {
            return new DayLog
            {
                Supa_Id = supaDayLog.Supa_Id,
                Id = supaDayLog.DayLog_Id,
                DayLog_Start_DateTime = supaDayLog.DayLog_Start_DateTime.ToUniversalTime(),
                DayLog_End_DateTime = supaDayLog.DayLog_End_DateTime?.ToUniversalTime(),
                Opening_Cash_Amount = supaDayLog.Opening_Cash_Amount,
                Closing_Cash_Amount = supaDayLog.Closing_Cash_Amount,
                Cash_Variance = supaDayLog.Cash_Variance,
                Date_Created = supaDayLog.Date_Created.ToUniversalTime(),
                Last_Modified = supaDayLog.Last_Modified.ToUniversalTime(),
                Created_By_Id = supaDayLog.Created_By_Id,
                Last_Modified_By_Id = supaDayLog.Last_Modified_By_Id,
                Site_Id = supaDayLog.Site_Id,
                Till_Id = supaDayLog.Till_Id,
                SyncStatus = SyncStatus.Synced,
            };
        }

        private DrawerLog MapSupaDrawerLogToLocal(Models.SupabaseModels.SupaDrawerLogs supaDrawerLog)
        {
            return new DrawerLog
            {
                Supa_Id = supaDrawerLog.Supa_Id,
                Id = supaDrawerLog.DrawerLogId,
                OpenedById = supaDrawerLog.OpenedById,
                DrawerOpenDateTime = supaDrawerLog.DrawerOpenDateTime.ToUniversalTime(),
                Date_Created = supaDrawerLog.Date_Created.ToUniversalTime(),
                Last_Modified = supaDrawerLog.Last_Modified.ToUniversalTime(),
                Created_By_Id = supaDrawerLog.Created_By_Id,
                Last_Modified_By_Id = supaDrawerLog.Last_Modified_By_Id,
                Site_Id = supaDrawerLog.Site_Id,
                Till_Id = supaDrawerLog.Till_Id,
                Shift_Id = supaDrawerLog.Shift_Id,
                DayLog_Id = supaDrawerLog.DayLog_Id,
                DrawerLogType = (DrawerLogType)supaDrawerLog.DrawerLogType,
                SyncStatus = SyncStatus.Synced,
            };
        }

        private ErrorLog MapSupaErrorLogToLocal(Models.SupabaseModels.SupaErrorLogs supaErrorLog)
        {
            return new ErrorLog
            {
                Supa_Id = supaErrorLog.Supa_Id,
                Id = supaErrorLog.ErrorLog_Id,
                Error_Message = supaErrorLog.Error_Message ?? string.Empty,
                Error_Type = supaErrorLog.Error_Type,
                Source_Method = supaErrorLog.Source_Method,
                Source_Class = supaErrorLog.Source_Class,
                Stack_Trace = supaErrorLog.Stack_Trace,
                Severity_Level = (ErrorLogSeverity?)supaErrorLog.Severity_Level,
                User_Action = supaErrorLog.User_Action,
                Error_DateTime = supaErrorLog.Error_DateTime.ToUniversalTime(),
                Date_Created = supaErrorLog.Date_Created.ToUniversalTime(),
                User_Id = supaErrorLog.User_Id,
                Site_Id = supaErrorLog.Site_Id,
                Till_Id = supaErrorLog.Till_Id,
                Application_Version = supaErrorLog.Application_Version,
                Is_Resolved = supaErrorLog.Is_Resolved,
                Resolved_DateTime = supaErrorLog.Resolved_DateTime?.ToUniversalTime(),
                Resolved_By_Id = supaErrorLog.Resolved_By_Id,
                Resolution_Notes = supaErrorLog.Resolution_Notes,
                SyncStatus = SyncStatus.Synced,
            };
        }

        private Retailer MapSupaRetailerToLocal(Models.SupabaseModels.SupaRetailers supaRetailer)
        {
            return new Retailer
            {
                RetailerId = supaRetailer.RetailerId,
                RetailName = supaRetailer.RetailName,
                AccessToken = supaRetailer.AccessToken,
                RefreshToken = supaRetailer.RefreshToken,
                ApiKey = supaRetailer.ApiKey,
                ApiUrl = supaRetailer.ApiUrl,
                Password = supaRetailer.Password,
                FirstLine_Address = supaRetailer.FirstLine_Address,
                SecondLine_Address = supaRetailer.SecondLine_Address,
                City = supaRetailer.City,
                County = supaRetailer.County,
                Country = supaRetailer.Country,
                Postcode = supaRetailer.Postcode,
                Vat_Number = supaRetailer.Vat_Number,
                Business_Registration_Number = supaRetailer.Business_Registration_Number,
                Business_Type = supaRetailer.Business_Type,
                Currency = supaRetailer.Currency,
                TimeZone = supaRetailer.TimeZone,
                Contact_Name = supaRetailer.Contact_Name,
                Contact_Position = supaRetailer.Contact_Position,
                Email = supaRetailer.Email,
                Contact_Number = supaRetailer.Contact_Number,
                Website_Url = supaRetailer.Website_Url,
                Logo_Path = supaRetailer.Logo_Path,
                Business_Hours = supaRetailer.Business_Hours,
                IsActive = supaRetailer.IsActive,
                LicenseKey = supaRetailer.LicenseKey,
                LicenseType = supaRetailer.LicenseType,
                LicenseIssueDate = supaRetailer.LicenseIssueDate.ToUniversalTime(),
                LicenseExpiryDate = supaRetailer.LicenseExpiryDate.ToUniversalTime(),
                MaxUsers = supaRetailer.MaxUsers,
                MaxTills = supaRetailer.MaxTills,
                IsLicenseValid = supaRetailer.IsLicenseValid,
                LastLicenseCheck = supaRetailer.LastLicenseCheck?.ToUniversalTime(),
                SecretKey = supaRetailer.SecretKey,
                Last_Sign_In_At = supaRetailer.Last_Sign_In_At?.ToUniversalTime(),
                Date_Created = supaRetailer.Date_Created.ToUniversalTime(),
                Last_Modified = supaRetailer.Last_Modified.ToUniversalTime(),
                LastSyncedAt = supaRetailer.LastSyncedAt?.ToUniversalTime(),
                SyncVersion = supaRetailer.SyncVersion,
                IsSynced = supaRetailer.IsSynced,
                SyncStatus = SyncStatus.Synced,
                TokenExpiryAt = supaRetailer.TokenExpiryAt.HasValue ? (int?)Convert.ToInt32(supaRetailer.TokenExpiryAt.Value) : null
            };
        }

        private Payout MapSupaPayoutToLocal(Models.SupabaseModels.SupaPayouts supaPayout)
        {
            return new Payout
            {
                Supa_Id = supaPayout.Supa_Id,
                Id = supaPayout.Payout_Id,
                Payout_Description = supaPayout.Payout_Description ?? string.Empty,
                Is_Active = supaPayout.Is_Active,
                Is_Deleted = supaPayout.Is_Deleted,
                Created_Date = supaPayout.Created_Date.ToUniversalTime(),
                Last_Modified = supaPayout.Last_Modified.ToUniversalTime(),
                Created_By_Id = supaPayout.Created_By_Id,
                Last_Modified_By_Id = supaPayout.Last_Modified_By_Id,
                Site_Id = supaPayout.Site_Id,
                Till_Id = supaPayout.Till_Id,
                SyncStatus = SyncStatus.Synced,
            };
        }

        private StockRefill MapSupaStockRefillToLocal(Models.SupabaseModels.SupaStockRefills supaStockRefill)
        {
            return new StockRefill
            {
                Supa_Id = supaStockRefill.Supa_Id,
                Id = supaStockRefill.StockRefill_ID,
                SaleTransaction_Item_ID = supaStockRefill.SaleTransaction_Item_ID,
                Refilled_By = supaStockRefill.Refilled_By,
                Refilled_Date = supaStockRefill.Refilled_Date?.ToUniversalTime() ?? null,
                Confirmed_By_Scanner = supaStockRefill.Confirmed_By_Scanner,
                Refill_Quantity = supaStockRefill.Refill_Quantity,
                Quantity_Refilled = supaStockRefill.Quantity_Refilled,
                Stock_Refilled = supaStockRefill.Stock_Refilled,
                Shift_ID = supaStockRefill.Shift_ID,
                DayLog_ID = supaStockRefill.DayLog_ID,
                Created_By_ID = supaStockRefill.Created_By_ID,
                Last_Modified_By_ID = supaStockRefill.Last_Modified_By_ID,
                Notes = supaStockRefill.Notes,
                Date_Created = supaStockRefill.Date_Created.ToUniversalTime(),
                Last_Modified = supaStockRefill.Last_Modified.ToUniversalTime(),
                SyncStatus = SyncStatus.Synced,
            };
        }

        private SupplierItem MapSupaSupplierItemToLocal(Models.SupabaseModels.SupaSupplierItems supaSupplierItem)
        {
            return new SupplierItem
            {
                Supa_Id = supaSupplierItem.Supa_Id,
                Id = supaSupplierItem.SupplierItemId,
                SupplierId = supaSupplierItem.SupplierId,
                ProductId = supaSupplierItem.ProductId,
                Supplier_Product_Code = supaSupplierItem.Supplier_Product_Code,
                Product_OuterCaseBarcode = supaSupplierItem.Product_OuterCaseBarcode,
                Cost_Per_Case = supaSupplierItem.Cost_Per_Case,
                Cost_Per_Unit = supaSupplierItem.Cost_Per_Unit,
                Unit_Per_Case = supaSupplierItem.Unit_Per_Case,
                Profit_On_Return = supaSupplierItem.Profit_On_Return,
                Is_Active = supaSupplierItem.Is_Active,
                Is_Deleted = supaSupplierItem.Is_Deleted,
                Date_Created = supaSupplierItem.Date_Created.ToUniversalTime(),
                Last_Modified = supaSupplierItem.Last_Modified.ToUniversalTime(),
                Created_By_Id = supaSupplierItem.Created_By_Id,
                Last_Modified_By_Id = supaSupplierItem.Last_Modified_By_Id,
                Site_Id = supaSupplierItem.Site_Id,
                Till_Id = supaSupplierItem.Till_Id,
                SyncStatus = SyncStatus.Synced,
            };
        }

        private UnknownProduct MapSupaUnknownProductToLocal(Models.SupabaseModels.SupaUnknownProduct supaUnknownProduct)
        {
            return new UnknownProduct
            {
                Supa_Id = supaUnknownProduct.Supa_Id,
                Id = supaUnknownProduct.Id,
                ProductBarcode = supaUnknownProduct.ProductBarcode,
                IsResolved = supaUnknownProduct.IsResolved,
                DateCreated = supaUnknownProduct.DateCreated.ToUniversalTime(),
                LastModified = supaUnknownProduct.LastModified.ToUniversalTime(),
                DaylogId = supaUnknownProduct.DaylogId,
                ShiftId = supaUnknownProduct.ShiftId,
                SiteId = supaUnknownProduct.SiteId,
                TillId = supaUnknownProduct.TillId,
                CreatedById = supaUnknownProduct.CreatedById,
                SyncStatus = SyncStatus.Synced,
            };
        }

        private UserSiteAccess MapSupaUserSiteAccessToLocal(Models.SupabaseModels.SupaUserSiteAccesses supaUserSiteAccess)
        {
            return new UserSiteAccess
            {
                Supa_Id = supaUserSiteAccess.Supa_Id,
                Id = supaUserSiteAccess.UserSiteAccess_ID,
                User_Id = supaUserSiteAccess.User_Id,
                Site_Id = supaUserSiteAccess.Site_Id,
                Is_Active = supaUserSiteAccess.Is_Active,
                Is_Deleted = supaUserSiteAccess.Is_Deleted,
                Date_Granted = supaUserSiteAccess.Date_Granted.ToUniversalTime(),
                Date_Revoked = supaUserSiteAccess.Date_Revoked?.ToUniversalTime(),
                Date_Created = supaUserSiteAccess.Date_Created.ToUniversalTime(),
                Last_Modified = supaUserSiteAccess.Last_Modified.ToUniversalTime(),
                Created_By_Id = supaUserSiteAccess.Created_By_Id,
                Last_Modified_By_Id = supaUserSiteAccess.Last_Modified_By_Id,
                Till_Id = supaUserSiteAccess.Till_Id,
                SyncStatus = SyncStatus.Synced,
            };
        }

        private VoidedProduct MapSupaVoidedProductToLocal(Models.SupabaseModels.SupaVoidedProducts supaVoidedProduct)
        {
            return new VoidedProduct
            {
                Supa_Id = supaVoidedProduct.Supa_Id,
                Id = supaVoidedProduct.VoidedProduct_ID,
                Product_ID = supaVoidedProduct.Product_ID,
                Voided_Quantity = supaVoidedProduct.Voided_Quantity,
                Voided_Amount = supaVoidedProduct.Voided_Amount,
                Voided_By_User_ID = supaVoidedProduct.Voided_By_User_ID,
                Void_Date = supaVoidedProduct.Void_Date.ToUniversalTime(),
                Additional_Notes = supaVoidedProduct.Additional_Notes,
                Date_Created = supaVoidedProduct.Date_Created.ToUniversalTime(),
                Last_Modified = supaVoidedProduct.Last_Modified.ToUniversalTime(),
                Created_By_Id = supaVoidedProduct.Created_By_Id,
                Last_Modified_By_Id = supaVoidedProduct.Last_Modified_By_Id,
                Site_Id = supaVoidedProduct.Site_Id,
                Till_Id = supaVoidedProduct.Till_Id,
                Shift_Id = supaVoidedProduct.Shift_Id,
                Daylog_Id = supaVoidedProduct.Daylog_Id,
                SyncStatus = SyncStatus.Synced,
            };
        }

        #endregion

        #region Database Save Methods

        private async Task SaveProductsToLocalDatabaseAsync(List<Product> products)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            foreach (var product in products)
            {
                var existingProduct = await context.Set<Product>()
                    .FirstOrDefaultAsync(p => p.Id == product.Id);

                if (existingProduct != null)
                {
                    // Update existing product
                    existingProduct.Supa_Id = product.Supa_Id;
                    existingProduct.ShelfQuantity = product.ShelfQuantity;
                    existingProduct.StockroomQuantity = product.StockroomQuantity;
                    existingProduct.Product_Cost = product.Product_Cost;
                    existingProduct.Product_Selling_Price = product.Product_Selling_Price;
                    existingProduct.Profit_On_Return_Percentage = product.Profit_On_Return_Percentage;
                    existingProduct.Promotion_Id = product.Promotion_Id;
                    existingProduct.Product_Unit_Per_Case = product.Product_Unit_Per_Case;
                    existingProduct.Product_Cost_Per_Case = product.Product_Cost_Per_Case;
                    existingProduct.Expiry_Date = product.Expiry_Date.ToUniversalTime();
                    existingProduct.Is_Activated = product.Is_Activated;
                    existingProduct.Is_Deleted = product.Is_Deleted;
                    existingProduct.Priced_Changed_On = product.Priced_Changed_On;
                    existingProduct.Is_Price_Changed = product.Is_Price_Changed;
                    existingProduct.Date_Created = product.Date_Created.ToUniversalTime();
                    existingProduct.Last_Modified = product.Last_Modified.ToUniversalTime();
                    existingProduct.Allow_Discount = product.Allow_Discount;
                    existingProduct.Created_By_Id = product.Created_By_Id;
                    existingProduct.Last_Modified_By_Id = product.Last_Modified_By_Id;
                    existingProduct.Site_Id = product.Site_Id;
                    existingProduct.Till_Id = product.Till_Id;
                    existingProduct.SyncStatus = SyncStatus.Synced;
                    existingProduct.Product_Barcode = product.Product_Barcode;
                    existingProduct.Product_Name = product.Product_Name;
                }
                else
                {
                    // Add new product
                    await context.Set<Product>().AddAsync(product);
                }
            }
            await context.SaveChangesAsync();
        }

        private async Task SaveUserSiteAccessesToLocalDatabaseAsync(List<UserSiteAccess> userSiteAccesses)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            foreach (var userSiteAccess in userSiteAccesses)
            {
                var existingUserSiteAccess = await context.Set<UserSiteAccess>()
                    .FirstOrDefaultAsync(usa => usa.Id == userSiteAccess.Id);

                if (existingUserSiteAccess != null)
                {
                    // Update existing user site access - manually set properties to avoid mismatches
                    existingUserSiteAccess.Supa_Id = userSiteAccess.Supa_Id;
                    existingUserSiteAccess.User_Id = userSiteAccess.User_Id;
                    existingUserSiteAccess.Site_Id = userSiteAccess.Site_Id;
                    existingUserSiteAccess.Is_Active = userSiteAccess.Is_Active;
                    existingUserSiteAccess.Is_Deleted = userSiteAccess.Is_Deleted;
                    existingUserSiteAccess.Date_Granted = userSiteAccess.Date_Granted;
                    existingUserSiteAccess.Date_Revoked = userSiteAccess.Date_Revoked;
                    existingUserSiteAccess.Date_Created = userSiteAccess.Date_Created.ToUniversalTime();
                    existingUserSiteAccess.Last_Modified = userSiteAccess.Last_Modified.ToUniversalTime();
                    existingUserSiteAccess.Created_By_Id = userSiteAccess.Created_By_Id;
                    existingUserSiteAccess.Last_Modified_By_Id = userSiteAccess.Last_Modified_By_Id;
                    existingUserSiteAccess.Till_Id = userSiteAccess.Till_Id;
                    existingUserSiteAccess.SyncStatus = SyncStatus.Synced;
                }
                else
                {
                    // Add new user site access - set sync properties for new records
                    userSiteAccess.SyncStatus = SyncStatus.Synced;
                    await context.Set<UserSiteAccess>().AddAsync(userSiteAccess);
                }
            }
            await context.SaveChangesAsync();
        }

        private async Task SaveVoidedProductsToLocalDatabaseAsync(List<VoidedProduct> voidedProducts)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            foreach (var voidedProduct in voidedProducts)
            {
                var existingVoidedProduct = await context.Set<VoidedProduct>()
                    .FirstOrDefaultAsync(vp => vp.Id == voidedProduct.Id);

                if (existingVoidedProduct != null)
                {
                    // Update existing voided product - manually set properties to avoid mismatches
                    existingVoidedProduct.Supa_Id = voidedProduct.Supa_Id;
                    existingVoidedProduct.Product_ID = voidedProduct.Product_ID;
                    existingVoidedProduct.Voided_Quantity = voidedProduct.Voided_Quantity;
                    existingVoidedProduct.Voided_Amount = voidedProduct.Voided_Amount;
                    existingVoidedProduct.Voided_By_User_ID = voidedProduct.Voided_By_User_ID;
                    existingVoidedProduct.Void_Date = voidedProduct.Void_Date;
                    existingVoidedProduct.Additional_Notes = voidedProduct.Additional_Notes;
                    existingVoidedProduct.Date_Created = voidedProduct.Date_Created.ToUniversalTime();
                    existingVoidedProduct.Last_Modified = voidedProduct.Last_Modified.ToUniversalTime();
                    existingVoidedProduct.Created_By_Id = voidedProduct.Created_By_Id;
                    existingVoidedProduct.Last_Modified_By_Id = voidedProduct.Last_Modified_By_Id;
                    existingVoidedProduct.Site_Id = voidedProduct.Site_Id;
                    existingVoidedProduct.Till_Id = voidedProduct.Till_Id;
                    existingVoidedProduct.Shift_Id = voidedProduct.Shift_Id;
                    existingVoidedProduct.Daylog_Id = voidedProduct.Daylog_Id;
                    existingVoidedProduct.SyncStatus = SyncStatus.Synced;
                }
                else
                {
                    // Add new voided product - set sync properties for new records
                    voidedProduct.SyncStatus = SyncStatus.Synced;
                    await context.Set<VoidedProduct>().AddAsync(voidedProduct);
                }
            }
            await context.SaveChangesAsync();
        }

        private async Task SaveUnknownProductsToLocalDatabaseAsync(List<UnknownProduct> unknownProducts)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            foreach (var unknownProduct in unknownProducts)
            {
                var existingUnknownProduct = await context.Set<UnknownProduct>()
                    .FirstOrDefaultAsync(up => up.Id == unknownProduct.Id);

                if (existingUnknownProduct != null)
                {
                    // Update existing unknown product - manually set properties to avoid mismatches
                    existingUnknownProduct.Supa_Id = unknownProduct.Supa_Id;
                    existingUnknownProduct.ProductBarcode = unknownProduct.ProductBarcode;
                    existingUnknownProduct.IsResolved = unknownProduct.IsResolved;
                    existingUnknownProduct.DateCreated = unknownProduct.DateCreated.ToUniversalTime();
                    existingUnknownProduct.LastModified = unknownProduct.LastModified.ToUniversalTime();
                    existingUnknownProduct.DaylogId = unknownProduct.DaylogId;
                    existingUnknownProduct.ShiftId = unknownProduct.ShiftId;
                    existingUnknownProduct.SiteId = unknownProduct.SiteId;
                    existingUnknownProduct.TillId = unknownProduct.TillId;
                    existingUnknownProduct.CreatedById = unknownProduct.CreatedById;
                    existingUnknownProduct.SyncStatus = SyncStatus.Synced;
                }
            }
            await context.SaveChangesAsync();
        }

        private async Task SaveSupplierItemsToLocalDatabaseAsync(List<SupplierItem> supplierItems)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            foreach (var supplierItem in supplierItems)
            {
                var existingSupplierItem = await context.Set<SupplierItem>()
                    .FirstOrDefaultAsync(si => si.Id == supplierItem.Id);

                if (existingSupplierItem != null)
                {
                    // Update existing supplier item - manually set properties to avoid mismatches
                    existingSupplierItem.Supa_Id = supplierItem.Supa_Id;
                    existingSupplierItem.SupplierId = supplierItem.SupplierId;
                    existingSupplierItem.ProductId = supplierItem.ProductId;
                    existingSupplierItem.Supplier_Product_Code = supplierItem.Supplier_Product_Code;
                    existingSupplierItem.Product_OuterCaseBarcode = supplierItem.Product_OuterCaseBarcode;
                    existingSupplierItem.Cost_Per_Case = supplierItem.Cost_Per_Case;
                    existingSupplierItem.Cost_Per_Unit = supplierItem.Cost_Per_Unit;
                    existingSupplierItem.Unit_Per_Case = supplierItem.Unit_Per_Case;
                    existingSupplierItem.Profit_On_Return = supplierItem.Profit_On_Return;
                    existingSupplierItem.Is_Active = supplierItem.Is_Active;
                    existingSupplierItem.Is_Deleted = supplierItem.Is_Deleted;
                    existingSupplierItem.Date_Created = supplierItem.Date_Created.ToUniversalTime();
                    existingSupplierItem.Last_Modified = supplierItem.Last_Modified.ToUniversalTime();
                    existingSupplierItem.Created_By_Id = supplierItem.Created_By_Id;
                    existingSupplierItem.Last_Modified_By_Id = supplierItem.Last_Modified_By_Id;
                    existingSupplierItem.Site_Id = supplierItem.Site_Id;
                    existingSupplierItem.Till_Id = supplierItem.Till_Id;
                    existingSupplierItem.SyncStatus = SyncStatus.Synced;
                }
                else
                {
                    // Add new supplier item - set sync properties for new records
                    supplierItem.SyncStatus = SyncStatus.Synced;
                    await context.Set<SupplierItem>().AddAsync(supplierItem);
                }
            }
            await context.SaveChangesAsync();
        }

        private async Task SaveDepartmentsToLocalDatabaseAsync(List<Department> departments)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            foreach (var department in departments)
            {
                var existingDepartment = await context.Set<Department>()
                    .FirstOrDefaultAsync(d => d.Id == department.Id);

                if (existingDepartment != null)
                {
                    context.Entry(existingDepartment).CurrentValues.SetValues(department);
                }
                else
                {
                    await context.Set<Department>().AddAsync(department);
                }
            }
            await context.SaveChangesAsync();
        }

        private async Task SaveSalesTransactionsToLocalDatabaseAsync(List<SalesTransaction> salesTransactions)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            foreach (var salesTransaction in salesTransactions)
            {
                var existingSalesTransaction = await context.Set<SalesTransaction>()
                    .FirstOrDefaultAsync(st => st.Id == salesTransaction.Id);

                if (existingSalesTransaction != null)
                {
                    context.Entry(existingSalesTransaction).CurrentValues.SetValues(salesTransaction);
                }
                else
                {
                    await context.Set<SalesTransaction>().AddAsync(salesTransaction);
                }
            }
            await context.SaveChangesAsync();
        }

        private async Task SaveSalesItemTransactionsToLocalDatabaseAsync(List<SalesItemTransaction> salesItemTransactions)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            foreach (var salesItemTransaction in salesItemTransactions)
            {
                var existingSalesItemTransaction = await context.Set<SalesItemTransaction>()
                    .FirstOrDefaultAsync(sit => sit.Id == salesItemTransaction.Id);

                if (existingSalesItemTransaction != null)
                {
                    context.Entry(existingSalesItemTransaction).CurrentValues.SetValues(salesItemTransaction);
                }
                else
                {
                    await context.Set<SalesItemTransaction>().AddAsync(salesItemTransaction);
                }
            }
            await context.SaveChangesAsync();
        }

        private async Task SaveShiftsToLocalDatabaseAsync(List<Shift> shifts)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            foreach (var shift in shifts)
            {
                var existingShift = await context.Set<Shift>()
                    .FirstOrDefaultAsync(s => s.Id == shift.Id);

                if (existingShift != null)
                {
                    context.Entry(existingShift).CurrentValues.SetValues(shift);
                }
                else
                {
                    await context.Set<Shift>().AddAsync(shift);
                }
            }
            await context.SaveChangesAsync();
        }

        private async Task SaveSitesToLocalDatabaseAsync(List<Site> sites)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            foreach (var site in sites)
            {
                var existingSite = await context.Set<Site>()
                    .FirstOrDefaultAsync(s => s.Id == site.Id);

                if (existingSite != null)
                {
                    existingSite.Supa_Id = site.Supa_Id;
                    existingSite.Id = site.Id;
                    existingSite.Site_BusinessName = site.Site_BusinessName;
                    existingSite.Site_AddressLine1 = site.Site_AddressLine1;
                    existingSite.Site_AddressLine2 = site.Site_AddressLine2;
                    existingSite.Site_City = site.Site_City;
                    existingSite.Site_County = site.Site_County;
                    existingSite.Site_Country = site.Site_Country;
                    existingSite.Site_Postcode = site.Site_Postcode;
                    existingSite.Site_ContactNumber = site.Site_ContactNumber;
                    existingSite.Site_Email = site.Site_Email;
                    existingSite.Site_VatNumber = site.Site_VatNumber;
                    existingSite.Is_Active = site.Is_Active;
                    existingSite.Is_Deleted = site.Is_Deleted;
                    existingSite.Is_Primary = site.Is_Primary;
                    existingSite.Date_Created = site.Date_Created.ToUniversalTime();
                    existingSite.Last_Modified = site.Last_Modified.ToUniversalTime();
                    existingSite.Created_By_Id = site.Created_By_Id == 0 ? existingSite.Created_By_Id : site.Created_By_Id;
                    existingSite.Last_Modified_By_Id = site.Last_Modified_By_Id == 0 ? existingSite.Last_Modified_By_Id : site.Last_Modified_By_Id;
                    existingSite.SyncStatus = SyncStatus.Synced;
                }
                else
                {
                    await context.Set<Site>().AddAsync(site);
                }
            }
            await context.SaveChangesAsync();
        }

        private async Task SaveStockTransactionsToLocalDatabaseAsync(List<StockTransaction> stockTransactions)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            foreach (var stockTransaction in stockTransactions)
            {
                var existingStockTransaction = await context.Set<StockTransaction>()
                    .FirstOrDefaultAsync(st => st.Id == stockTransaction.Id);

                if (existingStockTransaction != null)
                {
                    context.Entry(existingStockTransaction).CurrentValues.SetValues(stockTransaction);
                }
                else
                {
                    await context.Set<StockTransaction>().AddAsync(stockTransaction);
                }
            }
            await context.SaveChangesAsync();
        }

        private async Task SaveSuppliersToLocalDatabaseAsync(List<Supplier> suppliers)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            foreach (var supplier in suppliers)
            {
                var existingSupplier = await context.Set<Supplier>()
                    .FirstOrDefaultAsync(s => s.Id == supplier.Id);

                if (existingSupplier != null)
                {
                    context.Entry(existingSupplier).CurrentValues.SetValues(supplier);
                }
                else
                {
                    await context.Set<Supplier>().AddAsync(supplier);
                }
            }
            await context.SaveChangesAsync();
        }

        private async Task SaveTillsToLocalDatabaseAsync(List<Till> tills)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            foreach (var till in tills)
            {
                var existingTill = await context.Set<Till>()
                    .FirstOrDefaultAsync(t => t.Id == till.Id);

                if (existingTill != null)
                {
                    context.Entry(existingTill).CurrentValues.SetValues(till);
                }
                else
                {
                    await context.Set<Till>().AddAsync(till);
                }
            }
            await context.SaveChangesAsync();
        }

        //private async Task SaveVatsToLocalDatabaseAsync(List<Vat> vats)
        //{
        //    foreach (var vat in vats)
        //    {
        //        var existingVat = await context.Set<Vat>()
        //            .FirstOrDefaultAsync(v => v.Vat_ID == vat.Vat_ID);

        //        if (existingVat != null)
        //        {
        //            context.Entry(existingVat).CurrentValues.SetValues(vat);
        //        }
        //        else
        //        {
        //            await context.Set<Vat>().AddAsync(vat);
        //        }
        //    }
        //    await context.SaveChangesAsync();
        //}

        private async Task SavePosUsersToLocalDatabaseAsync(List<PosUser> posUsers)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            foreach (var posUser in posUsers)
            {
                var existingPosUser = await context.Set<PosUser>()
                    .FirstOrDefaultAsync(pu => pu.Id == posUser.Id);

                if (existingPosUser != null)
                {
                    context.Entry(existingPosUser).CurrentValues.SetValues(posUser);
                }
                else
                {
                    await context.Set<PosUser>().AddAsync(posUser);
                }
            }
            await context.SaveChangesAsync();
        }

        private async Task SavePromotionsToLocalDatabaseAsync(List<Promotion> promotions)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            foreach (var promotion in promotions)
            {
                var existingPromotion = await context.Set<Promotion>()
                    .FirstOrDefaultAsync(p => p.Id == promotion.Id);

                if (existingPromotion != null)
                {
                    context.Entry(existingPromotion).CurrentValues.SetValues(promotion);
                }
                else
                {
                    await context.Set<Promotion>().AddAsync(promotion);
                }
            }
            await context.SaveChangesAsync();
        }

        private async Task SaveReceiptPrintersToLocalDatabaseAsync(List<ReceiptPrinter> receiptPrinters)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            foreach (var receiptPrinter in receiptPrinters)
            {
                var existingReceiptPrinter = await context.Set<ReceiptPrinter>()
                    .FirstOrDefaultAsync(rp => rp.Id == receiptPrinter.Id);

                if (existingReceiptPrinter != null)
                {
                    context.Entry(existingReceiptPrinter).CurrentValues.SetValues(receiptPrinter);
                }
                else
                {
                    await context.Set<ReceiptPrinter>().AddAsync(receiptPrinter);
                }
            }
            await context.SaveChangesAsync();
        }

        private async Task SaveDayLogsToLocalDatabaseAsync(List<DayLog> dayLogs)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            foreach (var dayLog in dayLogs)
            {
                var existingDayLog = await context.Set<DayLog>()
                    .FirstOrDefaultAsync(d => d.Id == dayLog.Id);

                if (existingDayLog != null)
                {
                    // Update existing day log - manually set properties to avoid mismatches
                    existingDayLog.Supa_Id = dayLog.Supa_Id;
                    existingDayLog.DayLog_Start_DateTime = dayLog.DayLog_Start_DateTime.ToUniversalTime();
                    existingDayLog.DayLog_End_DateTime = dayLog.DayLog_End_DateTime?.ToUniversalTime();
                    existingDayLog.Opening_Cash_Amount = dayLog.Opening_Cash_Amount;
                    existingDayLog.Closing_Cash_Amount = dayLog.Closing_Cash_Amount;
                    existingDayLog.Cash_Variance = dayLog.Cash_Variance;
                    existingDayLog.Date_Created = dayLog.Date_Created.ToUniversalTime();
                    existingDayLog.Last_Modified = dayLog.Last_Modified.ToUniversalTime();
                    existingDayLog.Created_By_Id = dayLog.Created_By_Id;
                    existingDayLog.Last_Modified_By_Id = dayLog.Last_Modified_By_Id;
                    existingDayLog.Site_Id = dayLog.Site_Id;
                    existingDayLog.Till_Id = dayLog.Till_Id;
                    existingDayLog.SyncStatus = SyncStatus.Synced;
                }
                else
                {
                    dayLog.SyncStatus = SyncStatus.Synced;
                    await context.Set<DayLog>().AddAsync(dayLog);
                }
            }
            await context.SaveChangesAsync();
        }

        private async Task SaveDrawerLogsToLocalDatabaseAsync(List<DrawerLog> drawerLogs)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            foreach (var drawerLog in drawerLogs)
            {
                var existingDrawerLog = await context.Set<DrawerLog>()
                    .FirstOrDefaultAsync(d => d.Id == drawerLog.Id);

                if (existingDrawerLog != null)
                {
                    // Update existing drawer log - manually set properties to avoid mismatches
                    existingDrawerLog.Supa_Id = drawerLog.Supa_Id;
                    existingDrawerLog.OpenedById = drawerLog.OpenedById;
                    existingDrawerLog.DrawerOpenDateTime = drawerLog.DrawerOpenDateTime.ToUniversalTime();
                    existingDrawerLog.Date_Created = drawerLog.Date_Created.ToUniversalTime();
                    existingDrawerLog.Last_Modified = drawerLog.Last_Modified.ToUniversalTime();
                    existingDrawerLog.Created_By_Id = drawerLog.Created_By_Id;
                    existingDrawerLog.Last_Modified_By_Id = drawerLog.Last_Modified_By_Id;
                    existingDrawerLog.Site_Id = drawerLog.Site_Id;
                    existingDrawerLog.Till_Id = drawerLog.Till_Id;
                    existingDrawerLog.Shift_Id = drawerLog.Shift_Id;
                    existingDrawerLog.DayLog_Id = drawerLog.DayLog_Id;
                    existingDrawerLog.SyncStatus = SyncStatus.Synced;
                }
                else
                {
                    // Add new drawer log - set sync properties for new records
                    drawerLog.SyncStatus = SyncStatus.Synced;
                    await context.Set<DrawerLog>().AddAsync(drawerLog);
                }
            }
            await context.SaveChangesAsync();
        }

        private async Task SaveErrorLogsToLocalDatabaseAsync(List<ErrorLog> errorLogs)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            foreach (var errorLog in errorLogs)
            {
                var existingErrorLog = await context.Set<ErrorLog>()
                    .FirstOrDefaultAsync(e => e.Id == errorLog.Id);

                if (existingErrorLog != null)
                {
                    // Update existing error log - manually set properties to avoid mismatches
                    existingErrorLog.Supa_Id = errorLog.Supa_Id;
                    existingErrorLog.Error_Message = errorLog.Error_Message;
                    existingErrorLog.Error_Type = errorLog.Error_Type;
                    existingErrorLog.Source_Method = errorLog.Source_Method;
                    existingErrorLog.Source_Class = errorLog.Source_Class;
                    existingErrorLog.Stack_Trace = errorLog.Stack_Trace;
                    existingErrorLog.Severity_Level = errorLog.Severity_Level;
                    existingErrorLog.User_Action = errorLog.User_Action;
                    existingErrorLog.Error_DateTime = errorLog.Error_DateTime.ToUniversalTime();
                    existingErrorLog.Date_Created = errorLog.Date_Created.ToUniversalTime();
                    existingErrorLog.User_Id = errorLog.User_Id;
                    existingErrorLog.Site_Id = errorLog.Site_Id;
                    existingErrorLog.Till_Id = errorLog.Till_Id;
                    existingErrorLog.Application_Version = errorLog.Application_Version;
                    existingErrorLog.Is_Resolved = errorLog.Is_Resolved;
                    existingErrorLog.Resolved_DateTime = errorLog.Resolved_DateTime?.ToUniversalTime();
                    existingErrorLog.Resolved_By_Id = errorLog.Resolved_By_Id;
                    existingErrorLog.Resolution_Notes = errorLog.Resolution_Notes;
                    existingErrorLog.SyncStatus = SyncStatus.Synced;
                }
                else
                {
                    // Add new error log - set sync properties for new records
                    errorLog.SyncStatus = SyncStatus.Synced;
                    await context.Set<ErrorLog>().AddAsync(errorLog);
                }
            }
            await context.SaveChangesAsync();
        }

        private async Task SaveRetailerToLocalDatabaseAsync(Retailer retailer)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext
            var existingRetailer = await context.Retailers.FindAsync(retailer.RetailerId);
            if (existingRetailer != null)
            {
                context.Entry(existingRetailer).CurrentValues.SetValues(retailer);
            }
            else
            {
                await context.Retailers.AddAsync(retailer);
            }
            await context.SaveChangesAsync();
        }

        private async Task SavePayoutsToLocalDatabaseAsync(List<Payout> payouts)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            foreach (var payout in payouts)
            {
                var existingPayout = await context.Set<Payout>()
                    .FirstOrDefaultAsync(p => p.Id == payout.Id);

                if (existingPayout != null)
                {
                    // Update existing payout - manually set properties to avoid mismatches
                    existingPayout.Supa_Id = payout.Supa_Id;
                    existingPayout.Payout_Description = payout.Payout_Description;
                    existingPayout.Is_Active = payout.Is_Active;
                    existingPayout.Is_Deleted = payout.Is_Deleted;
                    existingPayout.Created_Date = payout.Created_Date;
                    existingPayout.Last_Modified = payout.Last_Modified;
                    existingPayout.Created_By_Id = payout.Created_By_Id;
                    existingPayout.Last_Modified_By_Id = payout.Last_Modified_By_Id;
                    existingPayout.Site_Id = payout.Site_Id;
                    existingPayout.Till_Id = payout.Till_Id;
                    existingPayout.SyncStatus = SyncStatus.Synced;
                }
                else
                {
                    // Add new payout - set sync properties for new records
                    payout.SyncStatus = SyncStatus.Synced;
                    await context.Set<Payout>().AddAsync(payout);
                }
            }
            await context.SaveChangesAsync();
        }

        private async Task SaveStockRefillsToLocalDatabaseAsync(List<StockRefill> stockRefills)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            foreach (var stockRefill in stockRefills)
            {
                var existingStockRefill = await context.Set<StockRefill>()
                    .FirstOrDefaultAsync(sr => sr.Id == stockRefill.Id);

                if (existingStockRefill != null)
                {
                    // Update existing stock refill - manually set properties to avoid mismatches
                    existingStockRefill.Supa_Id = stockRefill.Supa_Id;
                    existingStockRefill.SaleTransaction_Item_ID = stockRefill.SaleTransaction_Item_ID;
                    existingStockRefill.Refilled_By = stockRefill.Refilled_By;
                    existingStockRefill.Refilled_Date = stockRefill.Refilled_Date?.ToUniversalTime();
                    existingStockRefill.Confirmed_By_Scanner = stockRefill.Confirmed_By_Scanner;
                    existingStockRefill.Refill_Quantity = stockRefill.Refill_Quantity;
                    existingStockRefill.Quantity_Refilled = stockRefill.Quantity_Refilled;
                    existingStockRefill.Stock_Refilled = stockRefill.Stock_Refilled;
                    existingStockRefill.Shift_ID = stockRefill.Shift_ID;
                    existingStockRefill.DayLog_ID = stockRefill.DayLog_ID;
                    existingStockRefill.Created_By_ID = stockRefill.Created_By_ID;
                    existingStockRefill.Last_Modified_By_ID = stockRefill.Last_Modified_By_ID;
                    existingStockRefill.Notes = stockRefill.Notes;
                    existingStockRefill.Date_Created = stockRefill.Date_Created.ToUniversalTime();
                    existingStockRefill.Last_Modified = stockRefill.Last_Modified.ToUniversalTime();
                    existingStockRefill.SyncStatus = SyncStatus.Synced;
                }
                else
                {
                    // Add new stock refill - set sync properties for new records
                    stockRefill.SyncStatus = SyncStatus.Synced;
                    await context.Set<StockRefill>().AddAsync(stockRefill);
                }
            }
            await context.SaveChangesAsync();
        }

        #endregion

        #endregion

        /// <summary>
        /// Generic method to upsert a list of data in Supabase table
        /// </summary>
        /// <typeparam name="T">The type of data to upsert</typeparam>
        /// <param name="tableName">The name of the table</param>
        /// <param name="dataList">List of data to upsert</param>
        /// <param name="conflictColumns">Columns to check for conflicts (comma-separated)</param>
        /// <param name="retailerId">The retailer ID for authentication</param>
        /// <returns>SyncResult containing the upserted data list</returns>
        public async Task<SyncResult<List<T>>> UpsertListAsync<T>(string tableName, List<T> dataList, string conflictColumns, Retailer pRetailer) where T : class
        {
            pRetailer = await EnsureInitializedAsync(pRetailer);

            if (dataList == null || !dataList.Any())
            {
                return new SyncResult<List<T>>
                {
                    IsSuccess = true,
                    Data = new List<T>(),
                    Message = "No data to upsert"
                };
            }

            try
            {
                var json = JsonSerializer.Serialize<List<T>>(dataList, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = {
                        new JsonStringEnumConverter(),
                        new NullableDateTimeConverter(),
                        new SafeDateTimeConverter()
                    }
                });

                // Build request with headers and conflict settings
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var endpoint = string.IsNullOrEmpty(conflictColumns)
                    ? tableName
                    : $"{tableName}?on_conflict={Uri.EscapeDataString(conflictColumns)}";
                var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
                {
                    Content = content
                };
                request.Headers.Add("X-Retailer-Id", pRetailer.RetailerId.ToString());
                request.Headers.Add("Prefer", "resolution=merge-duplicates,return=representation");
                // Prefer HTTP/2, but allow fallback to lower version to reduce connection issues
                request.Version = HttpVersion.Version20;
                request.VersionPolicy = HttpVersionPolicy.RequestVersionOrLower;

                HttpResponseMessage response;
                try
                {
                    // Read headers first to avoid body streaming issues on flaky connections
                    response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                }
                catch (Exception ex) when (
                    ex is System.IO.IOException ||
                    ex is System.Net.Sockets.SocketException ||
                    (ex is HttpRequestException hre && hre.InnerException is System.IO.IOException)
                )
                {
                    _logger.LogWarning(ex, $"Network error during bulk upsert to {tableName}, retrying once");
                    if (_globalErrorLogService != null)
                    {
                        await _globalErrorLogService.LogErrorAsync(
                            ex,
                            nameof(UpsertListAsync),
                            $"Bulk upsert failed for {tableName} with {dataList?.Count ?? 0} records, data: " + json);
                    }

                    // Build a fresh request for retry (content streams can't be reused)
                    var retryContentInit = new StringContent(json, Encoding.UTF8, "application/json");
                    endpoint = string.IsNullOrEmpty(conflictColumns)
                        ? tableName
                        : $"{tableName}?on_conflict={Uri.EscapeDataString(conflictColumns)}";
                    var retryRequestInit = new HttpRequestMessage(HttpMethod.Post, endpoint)
                    {
                        Content = retryContentInit
                    };
                    retryRequestInit.Headers.Add("X-Retailer-Id", pRetailer.RetailerId.ToString());
                    retryRequestInit.Headers.Add("Prefer", "resolution=merge-duplicates,return=representation");
                    retryRequestInit.Version = HttpVersion.Version20;
                    retryRequestInit.VersionPolicy = HttpVersionPolicy.RequestVersionOrLower;

                    response = await _httpClient.SendAsync(retryRequestInit, HttpCompletionOption.ResponseHeadersRead);
                }

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = string.Empty;
                    try
                    {
                        responseContent = await response.Content.ReadAsStringAsync();
                    }
                    catch (Exception readEx) when (readEx is System.IO.IOException || readEx is HttpRequestException)
                    {
                        _logger.LogWarning(readEx, $"Succeeded upsert but failed reading response body for {tableName}. Returning empty result.");
                        if (_globalErrorLogService != null)
                        {
                            await _globalErrorLogService.LogErrorAsync(
                                readEx,
                                nameof(UpsertListAsync),
                                $"Bulk upsert failed for {tableName} with {dataList?.Count ?? 0} records, data: " + json);
                        }
                    }

                    var result = !string.IsNullOrEmpty(responseContent)
                        ? JsonSerializer.Deserialize<List<T>>(responseContent, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true,
                            Converters = {
                                new JsonStringEnumConverter(),
                                new NullableDateTimeConverter(),
                                new SafeDateTimeConverter()
                            }
                        })
                        : new List<T>();

                    return new SyncResult<List<T>>
                    {
                        IsSuccess = true,
                        Data = result ?? new List<T>(),
                        Message = $"Successfully upserted {result?.Count ?? 0} items"
                    };
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning("Received 401 Unauthorized during bulk upsert, attempting token refresh and retry");

                    bool retryTokenValid = await RefreshTokenIfNeededAsync(pRetailer);
                    if (retryTokenValid)
                    {
                        var retryContent = new StringContent(json, Encoding.UTF8, "application/json");
                        endpoint = string.IsNullOrEmpty(conflictColumns)
                            ? tableName
                            : $"{tableName}?on_conflict={Uri.EscapeDataString(conflictColumns)}";
                        var retryRequest = new HttpRequestMessage(HttpMethod.Post, endpoint)
                        {
                            Content = retryContent
                        };
                        retryRequest.Headers.Add("X-Retailer-Id", pRetailer.RetailerId.ToString());
                        retryRequest.Headers.Add("Prefer", "resolution=merge-duplicates,return=representation");
                        retryRequest.Version = HttpVersion.Version20;
                        retryRequest.VersionPolicy = HttpVersionPolicy.RequestVersionOrLower;

                        var retryResponse = await _httpClient.SendAsync(retryRequest, HttpCompletionOption.ResponseHeadersRead);
                        if (retryResponse.IsSuccessStatusCode)
                        {
                            string retryResponseContent = string.Empty;
                            try
                            {
                                retryResponseContent = await retryResponse.Content.ReadAsStringAsync();
                            }
                            catch (Exception readEx) when (readEx is System.IO.IOException || readEx is HttpRequestException)
                            {
                                _logger.LogWarning(readEx, $"Succeeded upsert after refresh but failed reading response body for {tableName}.");
                                if (_globalErrorLogService != null)
                                {
                                    await _globalErrorLogService.LogErrorAsync(
                                        readEx,
                                        nameof(UpsertListAsync),
                                        $"Bulk upsert failed for {tableName} with {dataList?.Count ?? 0} records" + retryResponse.Content);
                                }
                            }

                            var retryResult = !string.IsNullOrEmpty(retryResponseContent)
                                ? JsonSerializer.Deserialize<List<T>>(retryResponseContent, new JsonSerializerOptions
                                {
                                    PropertyNameCaseInsensitive = true,
                                    Converters = {
                                        new JsonStringEnumConverter(),
                                        new NullableDateTimeConverter(),
                                        new SafeDateTimeConverter()
                                    }
                                })
                                : new List<T>();

                            return new SyncResult<List<T>>
                            {
                                IsSuccess = true,
                                Data = retryResult ?? new List<T>(),
                                Message = $"Successfully upserted {retryResult?.Count ?? 0} items after token refresh"
                            };
                        }
                        else
                        {
                            string retryErrorContent = string.Empty;
                            try
                            {
                                retryErrorContent = await retryResponse.Content.ReadAsStringAsync();
                                if (_globalErrorLogService != null)
                                {
                                    await _globalErrorLogService.LogErrorAsync(
                                        new Exception("Unknown Status Code: " + retryResponse.StatusCode + " " + retryErrorContent),
                                        nameof(UpsertListAsync),
                                        $"Bulk upsert failed for {tableName} with {dataList?.Count ?? 0} records, data: " + json);
                                }
                            }
                            catch (Exception readEx) when (readEx is System.IO.IOException || readEx is HttpRequestException)
                            {
                                _logger.LogWarning(readEx, $"Retry failed for {tableName} and response body could not be read.");
                                if (_globalErrorLogService != null)
                                {
                                    await _globalErrorLogService.LogErrorAsync(
                                        readEx,
                                        nameof(UpsertListAsync),
                                        $"Bulk upsert failed for {tableName} with {dataList?.Count ?? 0} records"+ retryErrorContent);
                                }
                            }

                            _logger.LogError($"Bulk upsert retry failed for table {tableName}: {retryErrorContent}");
                            return new SyncResult<List<T>>
                            {
                                IsSuccess = false,
                                Error = retryErrorContent,
                                Message = $"Bulk upsert retry failed: {retryResponse.StatusCode}"
                            };
                        }
                    }

                    return new SyncResult<List<T>>
                    {
                        IsSuccess = false,
                        Error = "Authentication failed",
                        Message = "Unable to authenticate after token refresh attempt"
                    };
                }
                else
                {
                    string errorContent = string.Empty;
                    try
                    {
                        errorContent = await response.Content.ReadAsStringAsync();
                        if (_globalErrorLogService != null)
                        {
                            await _globalErrorLogService.LogErrorAsync(
                                new Exception("Unknown status code " + response.StatusCode),
                                nameof(UpsertListAsync),
                                $"Bulk upsert failed for {tableName} with {dataList?.Count ?? 0} records, data: " + json);
                        }
                    }
                    catch (Exception readEx) when (readEx is System.IO.IOException || readEx is HttpRequestException)
                    {
                        _logger.LogWarning(readEx, $"Failed reading error response for {tableName}: {response.StatusCode}");
                        if (_globalErrorLogService != null)
                        {
                            await _globalErrorLogService.LogErrorAsync(
                                readEx,
                                nameof(UpsertListAsync),
                                $"Bulk upsert failed for {tableName} with {dataList?.Count ?? 0} records"+ json);
                        }
                    }

                    _logger.LogError($"Bulk upsert failed for table {tableName}: {errorContent}");
                    return new SyncResult<List<T>>
                    {
                        IsSuccess = false,
                        Error = errorContent,
                        Message = $"Bulk upsert failed: {response.StatusCode}"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception during bulk upsert to table {tableName}");
                if (_globalErrorLogService != null)
                {
                    var json = JsonSerializer.Serialize<List<T>>(dataList, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        Converters = {
                        new JsonStringEnumConverter(),
                        new NullableDateTimeConverter()
                    }
                    });

                    await _globalErrorLogService.LogErrorAsync(
                        ex,
                        nameof(UpsertListAsync),
                        $"Bulk upsert failed for {tableName} with {dataList?.Count ?? 0} records" + json);
                }
                return new SyncResult<List<T>>
                {
                    IsSuccess = false,
                    Error = ex.Message,
                    Message = "Bulk upsert operation failed with exception"
                };
            }
        }

        /// <summary>
        /// Ensures the service is initialized for the specified retailer
        /// </summary>
        private async Task<Retailer> EnsureInitializedAsync(Retailer retailer)
        {
            retailer = await CheckIfRetailerAccessTokenNeedsRefreshedAsync(retailer);
            await InitializeForRetailerAsync(retailer);
            return retailer;

        }

        #endregion

        /// Checks if the access token is expired and refreshes it if needed
        /// </summary>
        public async Task<bool> RefreshTokenIfNeededAsync(Retailer retailer)
        {

            try
            {
                // Check if we need to refresh the token using TokenExpiryAt property
                bool needsRefresh = IsTokenExpired(retailer);

                if (needsRefresh)
                {
                    _logger.LogInformation($"Access token expired for retailer {retailer.RetailerId}, attempting to refresh");

                    // Try to refresh the token using the refresh token
                    if (!string.IsNullOrEmpty(retailer.RefreshToken))
                    {
                        bool refreshSuccess = await RefreshAccessTokenAsync(retailer);

                        if (refreshSuccess)
                        {
                            _logger.LogInformation($"Successfully refreshed access token for retailer {retailer.RetailerId}");
                            return true;
                        }
                    }

                    // If refresh token is missing or expired, re-authenticate
                    _logger.LogWarning($"Refresh token invalid for retailer {retailer.RetailerId}, attempting re-authentication");
                    bool authSuccess = await ReauthenticateAsync(retailer);

                    if (authSuccess)
                    {
                        _logger.LogInformation($"Successfully re-authenticated retailer {retailer.RetailerId}");
                        return true;
                    }
                    else
                    {
                        _logger.LogError($"Failed to re-authenticate retailer {retailer.RetailerId}");
                        return false;
                    }
                }

                return false; // Token is still valid
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error during token refresh for retailer {retailer.RetailerId}");
                return false;
            }
        }

        /// <summary>
        /// Gets a new access token using the refresh token
        /// </summary>
        /// <param name="refreshToken">The refresh token to use for getting new access token</param>
        /// <param name="apiUrl">The Supabase API URL</param>
        /// <param name="apiKey">The Supabase API key</param>
        /// <returns>TokenResponse containing the new access token and related information, or null if failed</returns>
        public async Task<TokenResponse> GetAccessTokenAsync(string refreshToken, string apiUrl, string apiKey)
        {
            if (string.IsNullOrEmpty(refreshToken))
            {
                _logger.LogError("Refresh token is null or empty");
                return null;
            }

            if (string.IsNullOrEmpty(apiUrl))
            {
                _logger.LogError("API URL is null or empty");
                return null;
            }

            if (string.IsNullOrEmpty(apiKey))
            {
                _logger.LogError("API key is null or empty");
                return null;
            }

            try
            {
                // Create a temporary HTTP client for the token refresh
                using var client = new HttpClient();
                client.BaseAddress = new Uri(apiUrl);
                client.DefaultRequestHeaders.Add("apikey", apiKey);

                // Prepare the refresh token request
                var refreshRequest = new
                {
                    refresh_token = refreshToken
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(refreshRequest),
                    Encoding.UTF8,
                    "application/json");

                _logger.LogInformation("Attempting to get new access token using refresh token");

                // Send the refresh request to Supabase auth endpoint
                var response = await client.PostAsync("/auth/v1/token?grant_type=refresh_token", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(responseContent);

                    if (tokenResponse != null && !string.IsNullOrEmpty(tokenResponse.AccessToken))
                    {
                        _logger.LogInformation("Successfully obtained new access token");
                        return tokenResponse;
                    }
                    else
                    {
                        _logger.LogError("Token response is null or access token is empty");
                        return null;
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Failed to get access token. Status: {response.StatusCode}, Error: {errorContent}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while getting access token using refresh token");
                return null;
            }
        }

        /// <summary>
        /// Gets a new access token using the refresh token for a specific retailer
        /// </summary>
        /// <param name="retailer">The retailer containing the refresh token and API configuration</param>
        /// <returns>TokenResponse containing the new access token and related information, or null if failed</returns>
        public async Task<TokenResponse> GetAccessTokenAsync(Retailer retailer)
        {
            if (retailer == null)
            {
                _logger.LogError("Retailer is null");
                return null;
            }

            return await GetAccessTokenAsync(retailer.RefreshToken, retailer.ApiUrl, retailer.ApiKey);
        }



        /// <summary>
        /// Checks if a token is expired based on TokenExpiryAt property
        /// </summary>
        private bool IsTokenExpired(Retailer retailer)
        {
            try
            {
                if (string.IsNullOrEmpty(retailer.AccessToken))
                    return true;

                // Get current Unix timestamp (seconds since epoch)
                var currentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                // Add a small buffer (e.g., 5 minutes = 300 seconds) to refresh before actual expiration
                return (retailer.TokenExpiryAt - 300) <= currentTimestamp;
            }
            catch
            {
                return true; // If any error occurs, assume expired to be safe
            }
        }

        /// <summary>
        /// Refreshes the access token using the refresh token
        /// </summary>
        private async Task<bool> RefreshAccessTokenAsync(Retailer retailer)
        {
            try
            {
                using var context = _dbFactory.CreateDbContext(); // fresh DbContext

                // Create a temporary HTTP client for the token refresh
                using var client = new HttpClient();
                client.BaseAddress = new Uri(retailer.ApiUrl);
                client.DefaultRequestHeaders.Add("apikey", retailer.ApiKey);

                // Prepare the refresh token request
                var refreshRequest = new
                {
                    refresh_token = retailer.RefreshToken
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(refreshRequest),
                    Encoding.UTF8,
                    "application/json");

                // Send the refresh request to Supabase auth endpoint
                var response = await client.PostAsync("/auth/v1/token?grant_type=refresh_token", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(responseContent);

                    // Update the retailer with new tokens
                    retailer.AccessToken = tokenResponse.AccessToken;
                    retailer.RefreshToken = tokenResponse.RefreshToken;

                    // Use the expires_at value directly if available, otherwise calculate from expires_in
                    if (tokenResponse.ExpiresAt > 0)
                    {
                        retailer.TokenExpiryAt = tokenResponse.ExpiresAt;
                    }
                    else
                    {
                        // Fallback to calculating from expires_in
                        int expiresInSeconds = tokenResponse.ExpiresIn > 0 ? tokenResponse.ExpiresIn : 3600;
                        retailer.TokenExpiryAt = (int)(DateTimeOffset.UtcNow.ToUnixTimeSeconds() + expiresInSeconds);
                    }

                    // Update last sign in time if available
                    if (tokenResponse.User != null && !string.IsNullOrEmpty(tokenResponse.User.LastSignInAt))
                    {
                        if (DateTime.TryParse(tokenResponse.User.LastSignInAt, out DateTime lastSignIn))
                        {
                            retailer.Last_Sign_In_At = lastSignIn.ToUniversalTime();
                        }
                    }

                    retailer.Last_Modified = DateTime.UtcNow;

                    // Save changes to the database
                    context.Update(retailer);
                    await context.SaveChangesAsync();

                    // Update the current service instance with new token
                    if (_currentRetailerId == retailer.RetailerId)
                    {
                        _supabaseKey = retailer.AccessToken;
                        await InitializeForRetailerAsync(retailer);
                    }

                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Failed to refresh token: {errorContent}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token refresh");
                return false;
            }
        }

        /// <summary>
        /// Re-authenticates the retailer to get new access and refresh tokens
        /// </summary>
        private async Task<bool> ReauthenticateAsync(Retailer retailer)
        {
            try
            {
                using var context = _dbFactory.CreateDbContext(); // fresh DbContext

                // Create a temporary HTTP client for authentication
                using var client = new HttpClient();
                client.BaseAddress = new Uri(retailer.ApiUrl);
                client.DefaultRequestHeaders.Add("apikey", _supabaseKey);

                // For re-authentication, we need credentials
                // This is a placeholder - you'll need to implement the actual authentication logic
                // based on your application's authentication flow
                var authRequest = new
                {
                    // This could be email/password, API key, or other authentication method
                    // Example for password authentication:
                    email = retailer.Email,
                    password = retailer.Password // Note: This assumes password is stored or can be retrieved securely
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(authRequest),
                    Encoding.UTF8,
                    "application/json");

                // Send the authentication request
                var response = await client.PostAsync("/auth/v1/token?grant_type=password", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(responseContent);

                    // Update the retailer with new tokens
                    retailer.AccessToken = tokenResponse.AccessToken;
                    retailer.RefreshToken = tokenResponse.RefreshToken;
                    retailer.Last_Modified = DateTime.UtcNow;

                    // Save changes to the database
                    await context.SaveChangesAsync();

                    // Update the current service instance with new token
                    if (_currentRetailerId == retailer.RetailerId)
                    {
                        _supabaseKey = retailer.AccessToken;
                        ConfigureHttpClient();
                    }

                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Failed to re-authenticate: {errorContent}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during re-authentication");
                return false;
            }
        }

        // Make the TokenResponse class public to ensure consistent accessibility
        public class TokenResponse
        {
            [JsonPropertyName("access_token")]
            public string AccessToken { get; set; }

            [JsonPropertyName("refresh_token")]
            public string RefreshToken { get; set; }

            [JsonPropertyName("expires_in")]
            public int ExpiresIn { get; set; }

            [JsonPropertyName("expires_at")]
            public int ExpiresAt { get; set; }

            [JsonPropertyName("token_type")]
            public string TokenType { get; set; }

            [JsonPropertyName("user")]
            public UserInfo User { get; set; }
        }

        public class UserInfo
        {
            [JsonPropertyName("id")]
            public string Id { get; set; }

            [JsonPropertyName("email")]
            public string Email { get; set; }

            [JsonPropertyName("last_sign_in_at")]
            public string LastSignInAt { get; set; }
        }


        /// <summary>
        /// Syncs a list of products with Supabase and updates local products with global IDs
        /// </summary>
        public async Task<SyncResult<List<Models.SupabaseModels.SupaRetailerProducts>>> SyncProductsAsync(
            List<EntityFrameworkDatabaseLibrary.Models.Product> localProducts,
            Retailer retailer)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            retailer = await EnsureInitializedAsync(retailer);

            var results = new List<Models.SupabaseModels.SupaRetailerProducts>();
            var failedProducts = new List<(int ProductId, string Error)>();

            // Process products in bulk instead of individually
            try
            {
                // Map all local products to Supabase models
                var supaProducts = new List<Models.SupabaseModels.SupaRetailerProducts>();

                foreach (var localProduct in localProducts)
                {
                    var supaProduct = new Models.SupabaseModels.SupaRetailerProducts
                    {
                        RetailerId = retailer.RetailerId,
                        Product_Id = localProduct.Id,
                        ProductBarcode = localProduct.Product_Barcode,
                        ProductName = localProduct.Product_Name,
                        DepartmentName = localProduct.Department?.Department_Name ?? "Default",
                        ProductMeasurement = localProduct.Product_Size,
                        ProductMeasurementType = localProduct.Product_Measurement,
                        VatValue = localProduct.VAT?.VAT_Value ?? 0.00m,
                        ShelfQuantity = localProduct.ShelfQuantity,
                        StockroomQuantity = localProduct.StockroomQuantity,
                        Product_Cost = localProduct.Product_Cost,
                        Product_Selling_Price = localProduct.Product_Selling_Price,
                        Profit_On_Return_Percentage = localProduct.Profit_On_Return_Percentage,
                        Promotion_Id = localProduct.Promotion_Id,
                        Product_Unit_Per_Case = localProduct.Product_Unit_Per_Case,
                        Product_Cost_Per_Case = (double)localProduct.Product_Cost_Per_Case,
                        Expiry_Date = localProduct.Expiry_Date == DateTime.MinValue ? DateTime.UtcNow : localProduct.Expiry_Date.ToUniversalTime(),
                        Is_Activated = localProduct.Is_Activated,
                        Is_Deleted = localProduct.Is_Deleted,
                        Priced_Changed_On = localProduct.Priced_Changed_On,
                        Is_Price_Changed = localProduct.Is_Price_Changed,
                        Date_Created = localProduct.Date_Created == DateTime.MinValue ? DateTime.UtcNow : localProduct.Date_Created.ToUniversalTime(),
                        Last_Modified = localProduct.Last_Modified == DateTime.MinValue ? DateTime.UtcNow : localProduct.Last_Modified.ToUniversalTime(),
                        Allow_Discount = localProduct.Allow_Discount,
                        Created_By_Id = localProduct.Created_By_Id,
                        Last_Modified_By_Id = localProduct.Last_Modified_By_Id,
                        Site_Id = localProduct.Site_Id,
                        Till_Id = localProduct.Till_Id,
                        SyncStatus = SyncStatus.Synced,
                        // If the product already has a GlobalId, use it, otherwise a new one will be generated
                        Product_Global_Id = localProduct.GlobalId ?? Guid.NewGuid()
                    };

                    supaProducts.Add(supaProduct);
                }

                // Use bulk upsert for all products at once
                var bulkResult = await UpsertListAsync("Products", supaProducts, "Product_Id,RetailerId", retailer);

                if (bulkResult.IsSuccess && bulkResult.Data != null)
                {
                    // Create a dictionary for fast lookup of synced results by Product_Global_Id
                    var syncedResultsDict = bulkResult.Data.ToDictionary(
                        sr => sr.Product_Id,
                        sr => sr
                    );

                    // Update local products with the results from bulk operation
                    foreach (var localProduct in localProducts)
                    {
                        if (syncedResultsDict.TryGetValue(localProduct.Id, out var syncedProduct))
                        {
                            // Update the local product with the global ID
                            if (localProduct.GlobalId != syncedProduct.Product_Global_Id)
                            {
                                localProduct.GlobalId = syncedProduct.Product_Global_Id;
                                localProduct.SyncStatus = SyncStatus.Synced;
                                localProduct.Last_Modified = DateTime.UtcNow;

                                _logger.LogInformation($"Updated local product ID {localProduct.Id} with global ID {syncedProduct.Product_Global_Id}");
                            }
                            else
                            {
                                // Update sync status even if GlobalId didn't change
                                localProduct.SyncStatus = SyncStatus.Synced;
                            }
                            localProduct.Supa_Id = syncedProduct.Supa_Id;
                        }
                        else
                        {
                            _logger.LogWarning($"Could not find synced result for Product ID {localProduct.Id}");
                            // Mark as failed if not found in results
                            localProduct.SyncStatus = SyncStatus.Failed;
                        }
                    }

                    results.AddRange(bulkResult.Data);

                    // Save all changes to the database in one transaction
                    await context.SaveChangesAsync();

                    // Return overall result
                    return new SyncResult<List<Models.SupabaseModels.SupaRetailerProducts>>
                    {
                        IsSuccess = true,
                        Data = results,
                        Message = $"Successfully bulk synced {results.Count} products"
                    };
                }
                else
                {
                    // Handle bulk operation failure - mark all products as failed
                    foreach (var localProduct in localProducts)
                    {
                        localProduct.SyncStatus = SyncStatus.Failed;
                        failedProducts.Add((localProduct.Id, bulkResult.Error));
                    }

                    await context.SaveChangesAsync();

                    return new SyncResult<List<Models.SupabaseModels.SupaRetailerProducts>>
                    {
                        IsSuccess = false,
                        Error = bulkResult.Error,
                        Message = $"Bulk sync failed for all {localProducts.Count} products: {bulkResult.Message}"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during bulk product sync");

                // Mark all products as failed
                foreach (var product in localProducts)
                {
                    if (product.SyncStatus != SyncStatus.Failed)
                    {
                        product.SyncStatus = SyncStatus.Failed;
                    }
                }

                await context.SaveChangesAsync();

                return new SyncResult<List<Models.SupabaseModels.SupaRetailerProducts>>
                {
                    IsSuccess = false,
                    Error = ex.Message,
                    Message = "Bulk product sync operation failed with exception"
                };
            }
        }
        /// <summary>
        /// Syncs a list of DayLogs with Supabase using bulk operations
        /// </summary>
        /// <param name="localDayLogs">List of local DayLogs to sync</param>
        /// <param name="retailer">The retailer for authentication</param>
        /// <returns>SyncResult containing the synced DayLogs</returns>
        public async Task<SyncResult<List<Models.SupabaseModels.SupaDayLogs>>> SyncDayLogsAsync(
            List<DayLog> localDayLogs,
            Retailer retailer)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            retailer = await EnsureInitializedAsync(retailer);

            var results = new List<Models.SupabaseModels.SupaDayLogs>();
            var failedDayLogs = new List<(int DayLogId, string Error)>();

            // Process DayLogs in bulk instead of individually
            try
            {
                // Map all local DayLogs to Supabase models
                var supaDayLogs = new List<Models.SupabaseModels.SupaDayLogs>();

                foreach (var localDayLog in localDayLogs)
                {
                    var supaDayLog = new Models.SupabaseModels.SupaDayLogs
                    {
                        RetailerId = retailer.RetailerId,
                        DayLog_Id = localDayLog.Id,
                        DayLog_Start_DateTime = localDayLog.DayLog_Start_DateTime.ToUniversalTime(),
                        DayLog_End_DateTime = localDayLog.DayLog_End_DateTime?.ToUniversalTime(),
                        Opening_Cash_Amount = localDayLog.Opening_Cash_Amount,
                        Closing_Cash_Amount = localDayLog.Closing_Cash_Amount,
                        Cash_Variance = localDayLog.Cash_Variance,
                        Date_Created = localDayLog.Date_Created.ToUniversalTime(),
                        Last_Modified = localDayLog.Last_Modified.ToUniversalTime(),
                        Created_By_Id = localDayLog.Created_By_Id,
                        Last_Modified_By_Id = localDayLog.Last_Modified_By_Id,
                        Site_Id = localDayLog.Site_Id,
                        Till_Id = localDayLog.Till_Id,
                        SyncStatus = SyncStatus.Synced
                    };

                    supaDayLogs.Add(supaDayLog);
                }

                // Use bulk upsert for all DayLogs at once
                var bulkResult = await UpsertListAsync("DayLogs", supaDayLogs, "DayLog_Id,RetailerId", retailer);

                if (bulkResult.IsSuccess && bulkResult.Data != null)
                {
                    // Create a dictionary for fast lookup of synced results by DayLog_Id
                    var syncedResultsDict = bulkResult.Data.ToDictionary(
                        sr => sr.DayLog_Id,
                        sr => sr
                    );

                    // Update local DayLogs with the results from bulk operation
                    foreach (var localDayLog in localDayLogs)
                    {
                        if (syncedResultsDict.TryGetValue(localDayLog.Id, out var syncedDayLog))
                        {
                            // Update sync status and timestamp
                            localDayLog.Supa_Id = syncedDayLog.Supa_Id;
                            localDayLog.SyncStatus = SyncStatus.Synced;
                            localDayLog.Last_Modified = DateTime.UtcNow;

                            _logger.LogInformation($"Successfully synced DayLog ID {localDayLog.Id}");
                        }
                        else
                        {
                            _logger.LogWarning($"Could not find synced result for DayLog ID {localDayLog.Id}");
                            localDayLog.SyncStatus = SyncStatus.Failed;
                        }
                    }

                    results.AddRange(bulkResult.Data);

                    // Save all changes to the database in one transaction
                    await context.SaveChangesAsync();

                    // Return overall result
                    return new SyncResult<List<Models.SupabaseModels.SupaDayLogs>>
                    {
                        IsSuccess = true,
                        Data = results,
                        Message = $"Successfully bulk synced {results.Count} DayLogs"
                    };
                }
                else
                {
                    // Handle bulk operation failure - mark all DayLogs as failed
                    foreach (var localDayLog in localDayLogs)
                    {
                        localDayLog.SyncStatus = SyncStatus.Failed;
                        failedDayLogs.Add((localDayLog.Id, bulkResult.Error));
                    }

                    await context.SaveChangesAsync();

                    return new SyncResult<List<Models.SupabaseModels.SupaDayLogs>>
                    {
                        IsSuccess = false,
                        Error = bulkResult.Error,
                        Message = $"Bulk sync failed for all {localDayLogs.Count} DayLogs: {bulkResult.Message}"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during bulk DayLog sync");

                // Mark all DayLogs as failed
                foreach (var dayLog in localDayLogs)
                {
                    if (dayLog.SyncStatus != SyncStatus.Failed)
                    {
                        dayLog.SyncStatus = SyncStatus.Failed;
                    }
                }

                await context.SaveChangesAsync();

                return new SyncResult<List<Models.SupabaseModels.SupaDayLogs>>
                {
                    IsSuccess = false,
                    Error = ex.Message,
                    Message = "Bulk DayLog sync operation failed with exception"
                };
            }
        }

        /// <summary>
        /// Syncs a list of DrawerLogs with Supabase using bulk operations
        /// </summary>
        /// <param name="localDrawerLogs">List of local DrawerLogs to sync</param>
        /// <param name="retailer">The retailer for authentication</param>
        /// <returns>SyncResult containing the synced DrawerLogs</returns>
        public async Task<SyncResult<List<Models.SupabaseModels.SupaDrawerLogs>>> SyncDrawerLogsAsync(
            List<DrawerLog> localDrawerLogs,
            Retailer retailer)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            retailer = await EnsureInitializedAsync(retailer);

            var results = new List<Models.SupabaseModels.SupaDrawerLogs>();
            var failedDrawerLogs = new List<(int DrawerLogId, string Error)>();

            // Process DrawerLogs in bulk instead of individually
            try
            {
                // Map all local DrawerLogs to Supabase models
                var supaDrawerLogs = new List<Models.SupabaseModels.SupaDrawerLogs>();

                foreach (var localDrawerLog in localDrawerLogs)
                {
                    var supaDrawerLog = new Models.SupabaseModels.SupaDrawerLogs
                    {
                        RetailerId = retailer.RetailerId,
                        DrawerLogId = localDrawerLog.Id,
                        OpenedById = localDrawerLog.OpenedById,
                        DrawerOpenDateTime = localDrawerLog.DrawerOpenDateTime.ToUniversalTime(),
                        Date_Created = localDrawerLog.Date_Created.ToUniversalTime(),
                        Last_Modified = localDrawerLog.Last_Modified.ToUniversalTime(),
                        Created_By_Id = localDrawerLog.Created_By_Id,
                        Last_Modified_By_Id = localDrawerLog.Last_Modified_By_Id,
                        Site_Id = localDrawerLog.Site_Id,
                        Till_Id = localDrawerLog.Till_Id,
                        DayLog_Id = localDrawerLog.DayLog_Id,
                        Shift_Id = localDrawerLog.Shift_Id,
                        DrawerLogType = localDrawerLog.DrawerLogType,
                        SyncStatus = SyncStatus.Synced
                    };

                    supaDrawerLogs.Add(supaDrawerLog);
                }

                // Use bulk upsert for all DrawerLogs at once
                var bulkResult = await UpsertListAsync("DrawerLogs", supaDrawerLogs, "DrawerLogId,RetailerId", retailer);

                if (bulkResult.IsSuccess && bulkResult.Data != null)
                {
                    // Create a dictionary for fast lookup of synced results by DrawerLogId
                    var syncedResultsDict = bulkResult.Data.ToDictionary(
                        sr => sr.DrawerLogId,
                        sr => sr
                    );

                    // Update local DrawerLogs with the results from bulk operation
                    foreach (var localDrawerLog in localDrawerLogs)
                    {
                        if (syncedResultsDict.TryGetValue(localDrawerLog.Id, out var syncedDrawerLog))
                        {
                            // Update sync status and timestamp
                            localDrawerLog.Supa_Id = syncedDrawerLog.Supa_Id;
                            localDrawerLog.SyncStatus = SyncStatus.Synced;
                            localDrawerLog.Last_Modified = DateTime.UtcNow;

                            _logger.LogInformation($"Successfully synced DrawerLog ID {localDrawerLog.Id}");
                        }
                        else
                        {
                            _logger.LogWarning($"Could not find synced result for DrawerLog ID {localDrawerLog.Id}");
                            localDrawerLog.SyncStatus = SyncStatus.Failed;
                        }
                    }

                    results.AddRange(bulkResult.Data);

                    // Save all changes to the database in one transaction
                    await context.SaveChangesAsync();

                    // Return overall result
                    return new SyncResult<List<Models.SupabaseModels.SupaDrawerLogs>>
                    {
                        IsSuccess = true,
                        Data = results,
                        Message = $"Successfully bulk synced {results.Count} DrawerLogs"
                    };
                }
                else
                {
                    // Handle bulk operation failure - mark all DrawerLogs as failed
                    foreach (var localDrawerLog in localDrawerLogs)
                    {
                        localDrawerLog.SyncStatus = SyncStatus.Failed;
                        failedDrawerLogs.Add((localDrawerLog.Id, bulkResult.Error));
                    }

                    await context.SaveChangesAsync();

                    return new SyncResult<List<Models.SupabaseModels.SupaDrawerLogs>>
                    {
                        IsSuccess = false,
                        Error = bulkResult.Error,
                        Message = $"Bulk sync failed for all {localDrawerLogs.Count} DrawerLogs: {bulkResult.Message}"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during bulk DrawerLog sync");

                // Mark all DrawerLogs as failed
                foreach (var drawerLog in localDrawerLogs)
                {
                    if (drawerLog.SyncStatus != SyncStatus.Failed)
                    {
                        drawerLog.SyncStatus = SyncStatus.Failed;
                    }
                }

                await context.SaveChangesAsync();

                return new SyncResult<List<Models.SupabaseModels.SupaDrawerLogs>>
                {
                    IsSuccess = false,
                    Error = ex.Message,
                    Message = "Bulk DrawerLog sync operation failed with exception"
                };
            }
        }

        /// <summary>
        /// Syncs a list of ErrorLogs with Supabase using bulk operations
        /// </summary>
        /// <param name="localErrorLogs">List of local ErrorLogs to sync</param>
        /// <param name="retailer">The retailer for authentication</param>
        /// <returns>SyncResult containing the synced ErrorLogs</returns>
        public async Task<SyncResult<List<Models.SupabaseModels.SupaErrorLogs>>> SyncErrorLogsAsync(
            List<ErrorLog> localErrorLogs,
            Retailer retailer)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            retailer = await EnsureInitializedAsync(retailer);

            var results = new List<Models.SupabaseModels.SupaErrorLogs>();
            var failedErrorLogs = new List<(int ErrorLogId, string Error)>();

            // Process ErrorLogs in bulk instead of individually
            try
            {
                // Map all local ErrorLogs to Supabase models
                var supaErrorLogs = new List<Models.SupabaseModels.SupaErrorLogs>();

                foreach (var localErrorLog in localErrorLogs)
                {
                    var supaErrorLog = new Models.SupabaseModels.SupaErrorLogs
                    {
                        RetailerId = retailer.RetailerId,
                        ErrorLog_Id = localErrorLog.Id,
                        Error_Message = localErrorLog.Error_Message,
                        Error_Type = localErrorLog.Error_Type,
                        Source_Method = localErrorLog.Source_Method,
                        Source_Class = localErrorLog.Source_Class,
                        Stack_Trace = localErrorLog.Stack_Trace,
                        Severity_Level = (int)(localErrorLog.Severity_Level ?? ErrorLogSeverity.Critical),
                        User_Action = localErrorLog.User_Action,
                        Error_DateTime = localErrorLog.Error_DateTime.ToUniversalTime(),
                        Date_Created = localErrorLog.Date_Created.ToUniversalTime(),
                        User_Id = localErrorLog.User_Id,
                        Site_Id = localErrorLog.Site_Id,
                        Till_Id = localErrorLog.Till_Id,
                        Application_Version = localErrorLog.Application_Version,
                        Is_Resolved = localErrorLog.Is_Resolved,
                        Resolved_DateTime = localErrorLog.Resolved_DateTime == null ? null : localErrorLog.Resolved_DateTime.Value.ToUniversalTime(),
                        Resolved_By_Id = localErrorLog.Resolved_By_Id,
                        Resolution_Notes = localErrorLog.Resolution_Notes,
                        SyncStatus = SyncStatus.Synced
                    };

                    supaErrorLogs.Add(supaErrorLog);
                }

                // Use bulk upsert for all ErrorLogs at once
                var bulkResult = await UpsertListAsync("ErrorLogs", supaErrorLogs, "ErrorLog_Id,RetailerId", retailer);

                if (bulkResult.IsSuccess && bulkResult.Data != null)
                {
                    // Create a dictionary for fast lookup of synced results by ErrorLog_Id
                    var syncedResultsDict = bulkResult.Data.ToDictionary(
                        sr => sr.ErrorLog_Id,
                        sr => sr
                    );

                    // Update local ErrorLogs with the results from bulk operation
                    foreach (var localErrorLog in localErrorLogs)
                    {
                        if (syncedResultsDict.TryGetValue(localErrorLog.Id, out var syncedErrorLog))
                        {
                            // Update sync status and timestamp
                            localErrorLog.Supa_Id = syncedErrorLog.Supa_Id;
                            localErrorLog.SyncStatus = SyncStatus.Synced;
                            _logger.LogInformation($"Successfully synced ErrorLog ID {localErrorLog.Id}");
                        }
                        else
                        {
                            _logger.LogWarning($"Could not find synced result for ErrorLog ID {localErrorLog.Id}");
                            localErrorLog.SyncStatus = SyncStatus.Failed;
                        }
                    }

                    results.AddRange(bulkResult.Data);

                    // Save all changes to the database in one transaction
                    await context.SaveChangesAsync();

                    // Return overall result
                    return new SyncResult<List<Models.SupabaseModels.SupaErrorLogs>>
                    {
                        IsSuccess = true,
                        Data = results,
                        Message = $"Successfully bulk synced {results.Count} ErrorLogs"
                    };
                }
                else
                {
                    // Handle bulk operation failure - mark all ErrorLogs as failed
                    foreach (var localErrorLog in localErrorLogs)
                    {
                        localErrorLog.SyncStatus = SyncStatus.Failed;
                        failedErrorLogs.Add((localErrorLog.Id, bulkResult.Error));
                    }

                    await context.SaveChangesAsync();

                    return new SyncResult<List<Models.SupabaseModels.SupaErrorLogs>>
                    {
                        IsSuccess = false,
                        Error = bulkResult.Error,
                        Message = $"Bulk sync failed for all {localErrorLogs.Count} ErrorLogs: {bulkResult.Message}"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during bulk ErrorLog sync");

                // Mark all ErrorLogs as failed
                foreach (var errorLog in localErrorLogs)
                {
                    if (errorLog.SyncStatus != SyncStatus.Failed)
                    {
                        errorLog.SyncStatus = SyncStatus.Failed;
                    }
                }

                await context.SaveChangesAsync();

                return new SyncResult<List<Models.SupabaseModels.SupaErrorLogs>>
                {
                    IsSuccess = false,
                    Error = ex.Message,
                    Message = "Bulk ErrorLog sync operation failed with exception"
                };
            }
        }

        /// <summary>
        /// Authenticates a user with Supabase and returns the authentication result
        /// </summary>
        /// <param name="email">User email</param>
        /// <param name="password">User password</param>
        /// <param name="apiUrl">Supabase API URL</param>
        /// <param name="apiKey">Supabase API Key</param>
        /// <returns>Authentication result with tokens and user information</returns>
        public async Task<AuthResult> AuthenticateAsync(string email, string password, string apiUrl, string apiKey)
        {
            try
            {
                // Create a temporary HTTP client for authentication
                using var client = new HttpClient();
                client.BaseAddress = new Uri(apiUrl);
                client.DefaultRequestHeaders.Add("apikey", apiKey);

                var authRequest = new
                {
                    email = email,
                    password = password
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(authRequest),
                    Encoding.UTF8,
                    "application/json");

                // Send the authentication request
                var response = await client.PostAsync("/auth/v1/token?grant_type=password", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(responseContent);

                    return new AuthResult
                    {
                        IsSuccess = true,
                        AccessToken = tokenResponse.AccessToken,
                        RefreshToken = tokenResponse.RefreshToken,
                        ExpiresIn = tokenResponse.ExpiresIn,
                        ExpiresAt = tokenResponse.ExpiresAt,
                        UserId = tokenResponse.User?.Id,
                        Email = tokenResponse.User?.Email
                    };
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Failed to authenticate: {errorContent}");
                    return new AuthResult
                    {
                        IsSuccess = false,
                        ErrorMessage = $"Authentication failed: {errorContent}"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during authentication");
                return new AuthResult
                {
                    IsSuccess = false,
                    ErrorMessage = $"Authentication error: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Alternative method to get server datetime using a simple SQL query
        /// </summary>
        /// <param name="pRetailer">The retailer for authentication</param>
        /// <returns>SyncResult containing the server datetime</returns>
        public async Task<SyncResult<DateTime>> GetServerDateTimeAlternativeAsync(Retailer retailer)
        {
            retailer = await EnsureInitializedAsync(retailer);

            // Check if access token is valid, refresh if needed
            bool tokenValid = await RefreshTokenIfNeededAsync(retailer);
            if (!tokenValid)
            {
                return new SyncResult<DateTime>
                {
                    IsSuccess = false,
                    Error = "Authentication failed",
                    Message = "Unable to validate or refresh access token"
                };
            }

            try
            {
                // Use a simple query to get current timestamp
                var url = "rpc/GetUTCDateTime";

                var requestBody = new
                {
                    // Empty body for the RPC call
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var request = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = content
                };
                request.Headers.Add("X-Retailer-Id", _userSessionService.CurrentRetailer.RetailerId.ToString());

                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();

                    // The response should be a JSON string with the timestamp
                    var cleanResponse = responseContent.Trim().Trim('"');

                    if (DateTime.TryParse(cleanResponse, out DateTime serverDateTime))
                    {
                        return new SyncResult<DateTime>
                        {
                            IsSuccess = true,
                            Data = serverDateTime,
                            Message = "Server datetime retrieved successfully"
                        };
                    }
                    else
                    {
                        _logger.LogError($"Failed to parse server datetime: {responseContent}");
                        return new SyncResult<DateTime>
                        {
                            IsSuccess = false,
                            Error = "Failed to parse server datetime",
                            Message = "Invalid datetime format received from server"
                        };
                    }
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    // Token might have expired during the request, try one more time
                    _logger.LogWarning("Received 401 Unauthorized, attempting token refresh and retry");

                    bool retryTokenValid = await RefreshTokenIfNeededAsync(retailer);
                    if (retryTokenValid)
                    {
                        // Retry the request with refreshed token
                        var retryResponse = await _httpClient.SendAsync(request);
                        if (retryResponse.IsSuccessStatusCode)
                        {
                            var retryResponseContent = await retryResponse.Content.ReadAsStringAsync();
                            var retryCleanResponse = retryResponseContent.Trim().Trim('"');

                            if (DateTime.TryParse(retryCleanResponse, out DateTime retryServerDateTime))
                            {
                                return new SyncResult<DateTime>
                                {
                                    IsSuccess = true,
                                    Data = retryServerDateTime,
                                    Message = "Server datetime retrieved successfully after token refresh"
                                };
                            }
                        }
                    }

                    return new SyncResult<DateTime>
                    {
                        IsSuccess = false,
                        Error = "Authentication failed",
                        Message = "Unable to authenticate after token refresh attempt"
                    };
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Get server datetime failed: {errorContent}");

                    return new SyncResult<DateTime>
                    {
                        IsSuccess = false,
                        Error = errorContent,
                        Message = $"Get server datetime failed: {response.StatusCode}"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during get server datetime alternative");
                return new SyncResult<DateTime>
                {
                    IsSuccess = false,
                    Error = ex.Message,
                    Message = "Get server datetime operation failed with exception"
                };
            }
        }

        /// <summary>
        /// Syncs a list of Payouts with Supabase using bulk operations
        /// </summary>
        /// <param name="localPayouts">List of local Payouts to sync</param>
        /// <param name="retailer">The retailer for authentication</param>
        /// <returns>SyncResult containing the synced Payouts</returns>
        public async Task<SyncResult<List<Models.SupabaseModels.SupaPayouts>>> SyncPayoutsAsync(
            List<Payout> localPayouts,
            Retailer retailer)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            retailer = await EnsureInitializedAsync(retailer);

            var results = new List<Models.SupabaseModels.SupaPayouts>();
            var failedPayouts = new List<(int PayoutId, string Error)>();

            // Process Payouts in bulk instead of individually
            try
            {
                // Map all local Payouts to Supabase models
                var supaPayouts = new List<Models.SupabaseModels.SupaPayouts>();

                foreach (var localPayout in localPayouts)
                {
                    var supaPayout = new Models.SupabaseModels.SupaPayouts
                    {
                        RetailerId = retailer.RetailerId,
                        Payout_Id = localPayout.Id,
                        Payout_Description = localPayout.Payout_Description,
                        Is_Active = localPayout.Is_Active,
                        Is_Deleted = localPayout.Is_Deleted,
                        Created_Date = localPayout.Created_Date,
                        Last_Modified = localPayout.Last_Modified,
                        Created_By_Id = localPayout.Created_By_Id,
                        Last_Modified_By_Id = localPayout.Last_Modified_By_Id,
                        Site_Id = localPayout.Site_Id,
                        Till_Id = localPayout.Till_Id,
                        SyncStatus = SyncStatus.Synced
                    };

                    supaPayouts.Add(supaPayout);
                }

                // Use bulk upsert for all Payouts at once
                var bulkResult = await UpsertListAsync("Payouts", supaPayouts, "Payout_Id,RetailerId", retailer);

                if (bulkResult.IsSuccess && bulkResult.Data != null)
                {
                    // Create a dictionary for fast lookup of synced results by Payout_Id
                    var syncedResultsDict = bulkResult.Data.ToDictionary(
                        sr => sr.Payout_Id,
                        sr => sr
                    );

                    // Update local Payouts with the results from bulk operation
                    foreach (var localPayout in localPayouts)
                    {
                        if (syncedResultsDict.TryGetValue(localPayout.Id, out var syncedPayout))
                        {
                            // Update sync status and timestamp
                            localPayout.Supa_Id = syncedPayout.Supa_Id;
                            localPayout.SyncStatus = SyncStatus.Synced;
                            localPayout.Last_Modified = DateTime.UtcNow;

                            _logger.LogInformation($"Successfully synced Payout ID {localPayout.Id}");
                        }
                        else
                        {
                            _logger.LogWarning($"Could not find synced result for Payout ID {localPayout.Id}");
                            localPayout.SyncStatus = SyncStatus.Failed;
                        }
                    }

                    results.AddRange(bulkResult.Data);

                    // Save all changes to the database in one transaction
                    await context.SaveChangesAsync();

                    // Return overall result
                    return new SyncResult<List<Models.SupabaseModels.SupaPayouts>>
                    {
                        IsSuccess = true,
                        Data = results,
                        Message = $"Successfully bulk synced {results.Count} Payouts"
                    };
                }
                else
                {
                    // Handle bulk operation failure - mark all Payouts as failed
                    foreach (var localPayout in localPayouts)
                    {
                        localPayout.SyncStatus = SyncStatus.Failed;
                        failedPayouts.Add((localPayout.Id, bulkResult.Error));
                    }

                    await context.SaveChangesAsync();

                    return new SyncResult<List<Models.SupabaseModels.SupaPayouts>>
                    {
                        IsSuccess = false,
                        Error = bulkResult.Error,
                        Message = $"Bulk sync failed for all {localPayouts.Count} Payouts: {bulkResult.Message}"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during bulk Payout sync");

                // Mark all Payouts as failed
                foreach (var payout in localPayouts)
                {
                    if (payout.SyncStatus != SyncStatus.Failed)
                    {
                        payout.SyncStatus = SyncStatus.Failed;
                    }
                }

                await context.SaveChangesAsync();

                return new SyncResult<List<Models.SupabaseModels.SupaPayouts>>
                {
                    IsSuccess = false,
                    Error = ex.Message,
                    Message = "Bulk Payout sync operation failed with exception"
                };
            }
        }

        /// <summary>
        /// Syncs a list of PosUsers with Supabase using bulk operations
        /// </summary>
        /// <param name="localPosUsers">List of local PosUsers to sync</param>
        /// <param name="retailer">The retailer for authentication</param>
        /// <returns>SyncResult containing the synced PosUsers</returns>
        public async Task<SyncResult<List<Models.SupabaseModels.SupaPosUsers>>> SyncPosUsersAsync(
            List<PosUser> localPosUsers,
            Retailer retailer)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            retailer = await EnsureInitializedAsync(retailer);

            var results = new List<Models.SupabaseModels.SupaPosUsers>();
            var failedPosUsers = new List<(int PosUserId, string Error)>();

            // Process PosUsers in bulk instead of individually
            try
            {
                // Map all local PosUsers to Supabase models
                var supaPosUsers = new List<Models.SupabaseModels.SupaPosUsers>();

                foreach (var localPosUser in localPosUsers)
                {
                    var supaPosUser = new Models.SupabaseModels.SupaPosUsers
                    {
                        RetailerId = retailer.RetailerId,
                        User_ID = localPosUser.Id,
                        First_Name = localPosUser.First_Name,
                        Last_Name = localPosUser.Last_Name,
                        Passcode = localPosUser.Passcode,
                        User_Type = localPosUser.User_Type,
                        Primary_Site_Id = localPosUser.Primary_Site_Id,
                        Allowed_Void_Line = localPosUser.Allowed_Void_Line,
                        Allowed_Void_Sale = localPosUser.Allowed_Void_Sale,
                        Allowed_No_Sale = localPosUser.Allowed_No_Sale,
                        Allowed_Returns = localPosUser.Allowed_Returns,
                        Allowed_Payout = localPosUser.Allowed_Payout,
                        Allowed_Refund = localPosUser.Allowed_Refund,
                        Allowed_Change_Price = localPosUser.Allowed_Change_Price,
                        Allowed_Discount = localPosUser.Allowed_Discount,
                        Allowed_Override_Price = localPosUser.Allowed_Override_Price,
                        Allowed_Manage_Users = localPosUser.Allowed_Manage_Users,
                        Allowed_Manage_Sites = localPosUser.Allowed_Manage_Sites,
                        Allowed_Manage_Tills = localPosUser.Allowed_Manage_Tills,
                        Allowed_Manage_Products = localPosUser.Allowed_Manage_Products,
                        Allowed_Manage_Suppliers = localPosUser.Allowed_Manage_Suppliers,
                        Allowed_Manage_StockTransfer = localPosUser.Allowed_Manage_StockTransfer,
                        Allowed_Manage_Vat = localPosUser.Allowed_Manage_Vat,
                        Allowed_Manage_Departments = localPosUser.Allowed_Manage_Departments,
                        Allowed_Manage_Orders = localPosUser.Allowed_Manage_Orders,
                        Allowed_Manage_Reports = localPosUser.Allowed_Manage_Reports,
                        Allowed_Manage_Settings = localPosUser.Allowed_Manage_Settings,
                        Allowed_Manage_Tax_Rates = localPosUser.Allowed_Manage_Tax_Rates,
                        Allowed_Manage_Promotions = localPosUser.Allowed_Manage_Promotions,
                        Allowed_Manage_VoidedProducts = localPosUser.Allowed_Manage_VoidedProducts,
                        Allowed_Day_End = localPosUser.Allowed_Day_End,
                        Is_Activated = localPosUser.Is_Activated,
                        Is_Deleted = localPosUser.Is_Deleted,
                        Date_Created = localPosUser.Date_Created.ToUniversalTime(),
                        Last_Modified = localPosUser.Last_Modified.ToUniversalTime(),
                        Created_By_Id = localPosUser.Created_By_Id,
                        Last_Modified_By_Id = localPosUser.Last_Modified_By_Id,
                        Site_Id = localPosUser.Site_Id,
                        Till_Id = localPosUser.Till_Id,
                        SyncStatus = SyncStatus.Synced
                    };

                    supaPosUsers.Add(supaPosUser);
                }

                // Use bulk upsert for all PosUsers at once
                var bulkResult = await UpsertListAsync("PosUsers", supaPosUsers, "User_ID,RetailerId", retailer);

                if (bulkResult.IsSuccess && bulkResult.Data != null)
                {
                    // Create a dictionary for fast lookup of synced results by User_ID
                    var syncedResultsDict = bulkResult.Data.ToDictionary(
                        sr => sr.User_ID,
                        sr => sr
                    );

                    // Update local PosUsers with the results from bulk operation
                    foreach (var localPosUser in localPosUsers)
                    {
                        if (syncedResultsDict.TryGetValue(localPosUser.Id, out var syncedPosUser))
                        {
                            // Update sync status and timestamp
                            localPosUser.Supa_Id = syncedPosUser.Supa_Id;
                            localPosUser.SyncStatus = SyncStatus.Synced;
                            localPosUser.Last_Modified = DateTime.UtcNow;

                            _logger.LogInformation($"Successfully synced PosUser ID {localPosUser.Id}");
                        }
                        else
                        {
                            _logger.LogWarning($"Could not find synced result for PosUser ID {localPosUser.Id}");
                            localPosUser.SyncStatus = SyncStatus.Failed;
                        }
                    }

                    results.AddRange(bulkResult.Data);

                    // Save all changes to the database in one transaction
                    await context.SaveChangesAsync();

                    // Return overall result
                    return new SyncResult<List<Models.SupabaseModels.SupaPosUsers>>
                    {
                        IsSuccess = true,
                        Data = results,
                        Message = $"Successfully bulk synced {results.Count} PosUsers"
                    };
                }
                else
                {
                    // Handle bulk operation failure - mark all PosUsers as failed
                    foreach (var localPosUser in localPosUsers)
                    {
                        localPosUser.SyncStatus = SyncStatus.Failed;
                        failedPosUsers.Add((localPosUser.Id, bulkResult.Error));
                    }

                    await context.SaveChangesAsync();

                    return new SyncResult<List<Models.SupabaseModels.SupaPosUsers>>
                    {
                        IsSuccess = false,
                        Error = bulkResult.Error,
                        Message = $"Bulk sync failed for all {localPosUsers.Count} PosUsers: {bulkResult.Message}"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during bulk PosUser sync");

                // Mark all PosUsers as failed
                foreach (var posUser in localPosUsers)
                {
                    if (posUser.SyncStatus != SyncStatus.Failed)
                    {
                        posUser.SyncStatus = SyncStatus.Failed;
                    }
                }

                await context.SaveChangesAsync();

                return new SyncResult<List<Models.SupabaseModels.SupaPosUsers>>
                {
                    IsSuccess = false,
                    Error = ex.Message,
                    Message = "Bulk PosUser sync operation failed with exception"
                };
            }
        }

        /// <summary>
        /// Syncs a list of Promotions with Supabase using bulk operations
        /// </summary>
        /// <param name="localPromotions">List of local Promotions to sync</param>
        /// <param name="retailer">The retailer for authentication</param>
        /// <returns>SyncResult containing the synced Promotions</returns>
        public async Task<SyncResult<List<Models.SupabaseModels.SupaPromotions>>> SyncPromotionsAsync(
            List<Promotion> localPromotions,
            Retailer retailer)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            retailer = await EnsureInitializedAsync(retailer);

            var results = new List<Models.SupabaseModels.SupaPromotions>();
            var failedPromotions = new List<(int PromotionId, string Error)>();

            // Process Promotions in bulk instead of individually
            try
            {
                // Map all local Promotions to Supabase models
                var supaPromotions = new List<Models.SupabaseModels.SupaPromotions>();

                foreach (var localPromotion in localPromotions)
                {
                    var supaPromotion = new Models.SupabaseModels.SupaPromotions
                    {
                        RetailerId = retailer.RetailerId,
                        Promotion_ID = localPromotion.Id,
                        Promotion_Name = localPromotion.Promotion_Name,
                        Promotion_Description = localPromotion.Promotion_Description,
                        Buy_Quantity = localPromotion.Buy_Quantity,
                        Free_Quantity = localPromotion.Free_Quantity,
                        Discount_Percentage = localPromotion.Discount_Percentage ?? 0.00m,
                        Discount_Amount = localPromotion.Discount_Amount ?? 0.00m,
                        Minimum_Spend_Amount = localPromotion.Minimum_Spend_Amount ?? 0.00m,
                        Start_Date = localPromotion.Start_Date,
                        End_Date = localPromotion.End_Date,
                        Promotion_Type = localPromotion.Promotion_Type,
                        Is_Deleted = localPromotion.Is_Deleted,
                        Created_Date = localPromotion.Created_Date,
                        Last_Modified = localPromotion.Last_Modified,
                        Created_By_Id = localPromotion.Created_By_Id,
                        Last_Modified_By_Id = localPromotion.Last_Modified_By_Id,
                        Site_Id = localPromotion.Site_Id,
                        Till_Id = localPromotion.Till_Id,
                        SyncStatus = SyncStatus.Synced
                    };

                    supaPromotions.Add(supaPromotion);
                }

                // Use bulk upsert for all Promotions at once
                var bulkResult = await UpsertListAsync("Promotions", supaPromotions, "Promotion_ID,RetailerId", retailer);

                if (bulkResult.IsSuccess && bulkResult.Data != null)
                {
                    // Create a dictionary for fast lookup of synced results by Promotion_ID
                    var syncedResultsDict = bulkResult.Data.ToDictionary(
                        sr => sr.Promotion_ID,
                        sr => sr
                    );

                    // Update local Promotions with the results from bulk operation
                    foreach (var localPromotion in localPromotions)
                    {
                        if (syncedResultsDict.TryGetValue(localPromotion.Id, out var syncedPromotion))
                        {
                            // Update sync status and timestamp
                            localPromotion.Supa_Id = syncedPromotion.Supa_Id;
                            localPromotion.SyncStatus = SyncStatus.Synced;
                            localPromotion.Last_Modified = DateTime.UtcNow;

                            _logger.LogInformation($"Successfully synced Promotion ID {localPromotion.Id}");
                        }
                        else
                        {
                            _logger.LogWarning($"Could not find synced result for Promotion ID {localPromotion.Id}");
                            localPromotion.SyncStatus = SyncStatus.Failed;
                        }
                    }

                    results.AddRange(bulkResult.Data);

                    // Save all changes to the database in one transaction
                    await context.SaveChangesAsync();

                    // Return overall result
                    return new SyncResult<List<Models.SupabaseModels.SupaPromotions>>
                    {
                        IsSuccess = true,
                        Data = results,
                        Message = $"Successfully bulk synced {results.Count} Promotions"
                    };
                }
                else
                {
                    // Handle bulk operation failure - mark all Promotions as failed
                    foreach (var localPromotion in localPromotions)
                    {
                        localPromotion.SyncStatus = SyncStatus.Failed;
                        failedPromotions.Add((localPromotion.Id, bulkResult.Error));
                    }

                    await context.SaveChangesAsync();

                    return new SyncResult<List<Models.SupabaseModels.SupaPromotions>>
                    {
                        IsSuccess = false,
                        Error = bulkResult.Error,
                        Message = $"Bulk sync failed for all {localPromotions.Count} Promotions: {bulkResult.Message}"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during bulk Promotion sync");

                // Mark all Promotions as failed
                foreach (var promotion in localPromotions)
                {
                    if (promotion.SyncStatus != SyncStatus.Failed)
                    {
                        promotion.SyncStatus = SyncStatus.Failed;
                    }
                }

                await context.SaveChangesAsync();

                return new SyncResult<List<Models.SupabaseModels.SupaPromotions>>
                {
                    IsSuccess = false,
                    Error = ex.Message,
                    Message = "Bulk Promotion sync operation failed with exception"
                };
            }
        }

        /// <summary>
        /// Syncs retailers to Supabase with bulk operations
        /// </summary>
        /// <param name="retailers">List of retailers to sync</param>
        /// <returns>Sync result with success/failure counts</returns>
        public async Task<SyncResult<List<Models.SupabaseModels.SupaRetailers>>> SyncRetailersAsync(List<Retailer> retailers)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            if (retailers == null || !retailers.Any())
            {
                return new SyncResult<List<Models.SupabaseModels.SupaRetailers>>
                {
                    IsSuccess = true,
                    Data = new List<Models.SupabaseModels.SupaRetailers>(),
                    Message = "No retailers to sync"
                };
            }

            var results = new List<Models.SupabaseModels.SupaRetailers>();
            var failedRetailers = new List<(Guid RetailerId, string Error)>();

            try
            {
                // Map local retailers to Supabase model
                var supaRetailers = new List<Models.SupabaseModels.SupaRetailers>();

                foreach (var retailer in retailers)
                {
                    var supaRetailer = new Models.SupabaseModels.SupaRetailers
                    {
                        RetailerId = retailer.RetailerId,
                        RetailName = retailer.RetailName,
                        AccessToken = retailer.AccessToken,
                        RefreshToken = retailer.RefreshToken,
                        Password = retailer.Password,
                        FirstLine_Address = retailer.FirstLine_Address,
                        SecondLine_Address = retailer.SecondLine_Address,
                        City = retailer.City,
                        County = retailer.County,
                        Country = retailer.Country,
                        Postcode = retailer.Postcode,
                        Vat_Number = retailer.Vat_Number,
                        Business_Registration_Number = retailer.Business_Registration_Number,
                        Business_Type = retailer.Business_Type,
                        Currency = retailer.Currency,
                        TimeZone = retailer.TimeZone,
                        Contact_Name = retailer.Contact_Name,
                        Contact_Position = retailer.Contact_Position,
                        Email = retailer.Email, // Note: mapping Email to Contact_Email
                        Contact_Number = retailer.Contact_Number,
                        Website_Url = retailer.Website_Url,
                        Logo_Path = retailer.Logo_Path,
                        Business_Hours = retailer.Business_Hours,
                        IsActive = retailer.IsActive,
                        LicenseKey = retailer.LicenseKey,
                        LicenseType = retailer.LicenseType,
                        LicenseIssueDate = retailer.LicenseIssueDate.ToUniversalTime(),
                        LicenseExpiryDate = retailer.LicenseExpiryDate.ToUniversalTime(),
                        MaxUsers = retailer.MaxUsers,
                        MaxTills = retailer.MaxTills,
                        IsLicenseValid = retailer.IsLicenseValid,
                        LastLicenseCheck = retailer.LastLicenseCheck?.ToUniversalTime(),
                        SecretKey = retailer.SecretKey,
                        Date_Created = retailer.Date_Created.ToUniversalTime(),
                        Last_Modified = retailer.Last_Modified.ToUniversalTime(),

                        SyncVersion = retailer.SyncVersion,
                        IsSynced = retailer.IsSynced,
                        SyncStatus = retailer.SyncStatus
                    };

                    supaRetailers.Add(supaRetailer);
                }

                // Use bulk upsert for all retailers at once
                var bulkResult = await UpsertListAsync("Retailers", supaRetailers, "RetailerId", retailers.First());

                if (bulkResult.IsSuccess && bulkResult.Data != null)
                {
                    // Create a dictionary for fast lookup of synced results by RetailerId
                    var syncedResultsDict = bulkResult.Data.ToDictionary(
                        sr => sr.RetailerId,
                        sr => sr
                    );

                    // Update local retailers with the results from bulk operation
                    foreach (var localRetailer in retailers)
                    {
                        if (syncedResultsDict.TryGetValue(localRetailer.RetailerId, out var syncedRetailer))
                        {
                            // Update sync status and timestamp
                            localRetailer.SyncStatus = SyncStatus.Synced;
                            localRetailer.LastSyncedAt = DateTime.UtcNow;
                            localRetailer.IsSynced = true;
                            localRetailer.Last_Modified = DateTime.UtcNow;

                            _logger.LogInformation($"Successfully synced Retailer ID {localRetailer.RetailerId}");
                        }
                        else
                        {
                            _logger.LogWarning($"Could not find synced result for Retailer ID {localRetailer.RetailerId}");
                            localRetailer.SyncStatus = SyncStatus.Failed;
                        }
                    }

                    results.AddRange(bulkResult.Data);

                    // Save all changes to the database in one transaction
                    await context.SaveChangesAsync();

                    // Return overall result
                    return new SyncResult<List<Models.SupabaseModels.SupaRetailers>>
                    {
                        IsSuccess = true,
                        Data = results,
                        Message = $"Successfully bulk synced {results.Count} retailers"
                    };
                }
                else
                {
                    // Handle bulk operation failure - mark all retailers as failed
                    foreach (var retailer in retailers)
                    {
                        retailer.SyncStatus = SyncStatus.Failed;

                        failedRetailers.Add((retailer.RetailerId, bulkResult.Error));
                    }

                    await context.SaveChangesAsync();

                    return new SyncResult<List<Models.SupabaseModels.SupaRetailers>>
                    {
                        IsSuccess = false,
                        Error = bulkResult.Error,
                        Message = $"Bulk sync failed for all {retailers.Count} retailers: {bulkResult.Message}"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during bulk retailer sync");

                // Mark all retailers as failed
                foreach (var retailer in retailers)
                {
                    if (retailer.SyncStatus != SyncStatus.Failed)
                    {
                        retailer.SyncStatus = SyncStatus.Failed;
                    }
                }

                await context.SaveChangesAsync();

                return new SyncResult<List<Models.SupabaseModels.SupaRetailers>>
                {
                    IsSuccess = false,
                    Error = ex.Message,
                    Message = "Bulk retailer sync operation failed with exception"
                };
            }
        }

        /// <summary>
        /// Syncs a list of SalesItemTransactions with Supabase using bulk operations
        /// </summary>
        /// <param name="localSalesItemTransactions">List of local SalesItemTransactions to sync</param>
        /// <param name="retailer">The retailer for authentication</param>
        /// <returns>SyncResult containing the synced SalesItemTransactions</returns>
        public async Task<SyncResult<List<Models.SupabaseModels.SupaSalesItemsTransactions>>> SyncSalesItemTransactionsAsync(
            List<SalesItemTransaction> localSalesItemTransactions,
            Retailer retailer)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            retailer = await EnsureInitializedAsync(retailer);

            var results = new List<Models.SupabaseModels.SupaSalesItemsTransactions>();
            var failedSalesItemTransactions = new List<(int SalesItemTransactionId, string Error)>();

            // Process SalesItemTransactions in bulk instead of individually
            try
            {
                // Map all local SalesItemTransactions to Supabase models
                var supaSalesItemTransactions = new List<Models.SupabaseModels.SupaSalesItemsTransactions>();

                foreach (var localSalesItemTransaction in localSalesItemTransactions)
                {
                    var supaSalesItemTransaction = new Models.SupabaseModels.SupaSalesItemsTransactions
                    {
                        RetailerId = retailer.RetailerId,
                        SaleTransaction_Item_ID = localSalesItemTransaction.Id,
                        SaleTransaction_ID = localSalesItemTransaction.SaleTransaction_ID,
                        Product_ID = localSalesItemTransaction.Product_ID,
                        Product_QTY = localSalesItemTransaction.Product_QTY,
                        Product_Amount = localSalesItemTransaction.Product_Amount,
                        Product_Total_Amount = localSalesItemTransaction.Product_Total_Amount,
                        SalesPayout_ID = localSalesItemTransaction.SalesPayout_ID,
                        Promotion_ID = localSalesItemTransaction.Promotion_ID,
                        Discount_Percent = localSalesItemTransaction.Discount_Percent,
                        Product_Total_Amount_Before_Discount = localSalesItemTransaction.Product_Total_Amount_Before_Discount,
                        Discount_Amount = localSalesItemTransaction.Discount_Amount,
                        Is_Manual_Weight_Entry = localSalesItemTransaction.Is_Manual_Weight_Entry ?? false,
                        Date_Created = localSalesItemTransaction.Date_Created.ToUniversalTime(),
                        Last_Modified = localSalesItemTransaction.Last_Modified.ToUniversalTime(),
                        SalesItemTransactionType = (localSalesItemTransaction.SalesItemTransactionType ?? SalesItemTransactionType.Sale),
                        Created_By_Id = localSalesItemTransaction.Created_By_Id,
                        Last_Modified_By_Id = localSalesItemTransaction.Last_Modified_By_Id,
                        SyncStatus = SyncStatus.Synced
                    };

                    supaSalesItemTransactions.Add(supaSalesItemTransaction);
                }

                // Use bulk upsert for all SalesItemTransactions at once
                var bulkResult = await UpsertListAsync("SalesItemTransactions", supaSalesItemTransactions, "SaleTransaction_Item_ID,RetailerId", retailer);

                if (bulkResult.IsSuccess && bulkResult.Data != null)
                {
                    // Create a dictionary for fast lookup of synced results by SaleTransaction_Item_ID
                    var syncedResultsDict = bulkResult.Data.ToDictionary(
                        sr => sr.SaleTransaction_Item_ID,
                        sr => sr
                    );

                    // Update local SalesItemTransactions with the results from bulk operation
                    foreach (var localSalesItemTransaction in localSalesItemTransactions)
                    {
                        if (syncedResultsDict.TryGetValue(localSalesItemTransaction.Id, out var syncedSalesItemTransaction))
                        {
                            // Update sync status and timestamp
                            localSalesItemTransaction.Supa_Id = syncedSalesItemTransaction.Supa_Id;
                            localSalesItemTransaction.SyncStatus = SyncStatus.Synced;
                            localSalesItemTransaction.Last_Modified = DateTime.UtcNow;

                            _logger.LogInformation($"Successfully synced SalesItemTransaction ID {localSalesItemTransaction.Id}");
                        }
                        else
                        {
                            _logger.LogWarning($"Could not find synced result for SalesItemTransaction ID {localSalesItemTransaction.Id}");
                            localSalesItemTransaction.SyncStatus = SyncStatus.Failed;
                        }
                    }

                    results.AddRange(bulkResult.Data);

                    // Save all changes to the database in one transaction
                    await context.SaveChangesAsync();

                    // Return overall result
                    return new SyncResult<List<Models.SupabaseModels.SupaSalesItemsTransactions>>
                    {
                        IsSuccess = true,
                        Data = results,
                        Message = $"Successfully bulk synced {results.Count} SalesItemTransactions"
                    };
                }
                else
                {
                    // Handle bulk operation failure - mark all SalesItemTransactions as failed
                    foreach (var localSalesItemTransaction in localSalesItemTransactions)
                    {
                        localSalesItemTransaction.SyncStatus = SyncStatus.Failed;
                        failedSalesItemTransactions.Add((localSalesItemTransaction.Id, bulkResult.Error));
                    }

                    await context.SaveChangesAsync();

                    return new SyncResult<List<Models.SupabaseModels.SupaSalesItemsTransactions>>
                    {
                        IsSuccess = false,
                        Error = bulkResult.Error,
                        Message = $"Bulk sync failed for all {localSalesItemTransactions.Count} SalesItemTransactions: {bulkResult.Message}"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during bulk SalesItemTransaction sync");

                // Mark all SalesItemTransactions as failed
                foreach (var salesItemTransaction in localSalesItemTransactions)
                {
                    if (salesItemTransaction.SyncStatus != SyncStatus.Failed)
                    {
                        salesItemTransaction.SyncStatus = SyncStatus.Failed;
                    }
                }

                await context.SaveChangesAsync();

                return new SyncResult<List<Models.SupabaseModels.SupaSalesItemsTransactions>>
                {
                    IsSuccess = false,
                    Error = ex.Message,
                    Message = "Bulk SalesItemTransaction sync operation failed with exception"
                };
            }
        }

        /// <summary>
        /// Syncs a list of SalesTransactions with Supabase using bulk operations
        /// </summary>
        /// <param name="localSalesTransactions">List of local SalesTransactions to sync</param>
        /// <param name="retailer">The retailer for authentication</param>
        /// <returns>SyncResult containing the synced SalesTransactions</returns>
        public async Task<SyncResult<List<Models.SupabaseModels.SupaSalesTransactions>>> SyncSalesTransactionsAsync(
            List<SalesTransaction> localSalesTransactions,
            Retailer retailer)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            retailer = await EnsureInitializedAsync(retailer);

            var results = new List<Models.SupabaseModels.SupaSalesTransactions>();
            var failedSalesTransactions = new List<(int SalesTransactionId, string Error)>();

            // Process SalesTransactions in bulk instead of individually
            try
            {
                // Map all local SalesTransactions to Supabase models
                var supaSalesTransactions = new List<Models.SupabaseModels.SupaSalesTransactions>();

                foreach (var localSalesTransaction in localSalesTransactions)
                {
                    var supaSalesTransaction = new Models.SupabaseModels.SupaSalesTransactions
                    {
                        RetailerId = retailer.RetailerId,
                        SaleTransaction_ID = localSalesTransaction.Id,
                        SaleTransaction_Total_QTY = localSalesTransaction.SaleTransaction_Total_QTY,
                        SaleTransaction_Total_Amount = localSalesTransaction.SaleTransaction_Total_Amount,
                        SaleTransaction_Total_Paid = localSalesTransaction.SaleTransaction_Total_Paid,
                        SaleTransaction_Cash = localSalesTransaction.SaleTransaction_Cash,
                        SaleTransaction_Card = localSalesTransaction.SaleTransaction_Card,
                        SaleTransaction_Refund = localSalesTransaction.SaleTransaction_Refund,
                        SaleTransaction_Discount = localSalesTransaction.SaleTransaction_Discount,
                        SaleTransaction_Promotion = localSalesTransaction.SaleTransaction_Promotion,
                        SaleTransaction_Change = localSalesTransaction.SaleTransaction_Change,
                        SaleTransaction_Payout = localSalesTransaction.SaleTransaction_Payout,
                        SaleTransaction_CashBack = localSalesTransaction.SaleTransaction_CashBack,
                        SaleTransaction_Card_Charges = localSalesTransaction.SaleTransaction_Card_Charges,
                        DayLog_Id = localSalesTransaction.DayLog_Id,
                        Shift_Id = localSalesTransaction.Shift_Id,
                        Sale_Date = localSalesTransaction.Sale_Date,
                        Is_Printed = localSalesTransaction.Is_Printed,
                        Date_Created = localSalesTransaction.Date_Created.ToUniversalTime(),
                        Last_Modified = localSalesTransaction.Last_Modified.ToUniversalTime(),
                        Sale_Start_Date = localSalesTransaction.Sale_Start_Date.ToUniversalTime(),
                        Created_By_Id = localSalesTransaction.Created_By_Id,
                        Last_Modified_By_Id = localSalesTransaction.Last_Modified_By_Id,
                        Site_Id = localSalesTransaction.Site_Id,
                        Till_Id = localSalesTransaction.Till_Id,
                        SyncStatus = SyncStatus.Synced
                    };

                    supaSalesTransactions.Add(supaSalesTransaction);
                }

                // Use bulk upsert for all SalesTransactions at once
                var bulkResult = await UpsertListAsync("SalesTransactions", supaSalesTransactions, "SaleTransaction_ID,RetailerId", retailer);

                if (bulkResult.IsSuccess && bulkResult.Data != null)
                {
                    // Create a dictionary for fast lookup of synced results by SaleTransaction_ID
                    var syncedResultsDict = bulkResult.Data.ToDictionary(
                        sr => sr.SaleTransaction_ID,
                        sr => sr
                    );

                    // Update local SalesTransactions with the results from bulk operation
                    foreach (var localSalesTransaction in localSalesTransactions)
                    {
                        if (syncedResultsDict.TryGetValue(localSalesTransaction.Id, out var syncedSalesTransaction))
                        {
                            // Update sync status and timestamp
                            localSalesTransaction.Supa_Id = syncedSalesTransaction.Supa_Id;
                            localSalesTransaction.SyncStatus = SyncStatus.Synced;
                            localSalesTransaction.Last_Modified = DateTime.UtcNow;

                            _logger.LogInformation($"Successfully synced SalesTransaction ID {localSalesTransaction.Id}");
                        }
                        else
                        {
                            _logger.LogWarning($"Could not find synced result for SalesTransaction ID {localSalesTransaction.Id}");

                            // Mark as failed if not found in results
                            localSalesTransaction.SyncStatus = SyncStatus.Failed;
                        }
                    }

                    results.AddRange(bulkResult.Data);

                    // Save all changes to the database in one transaction
                    await context.SaveChangesAsync();

                    // Return overall result
                    return new SyncResult<List<Models.SupabaseModels.SupaSalesTransactions>>
                    {
                        IsSuccess = true,
                        Data = results,
                        Message = $"Successfully bulk synced {results.Count} SalesTransactions"
                    };
                }
                else
                {
                    // Handle bulk operation failure - mark all SalesTransactions as failed
                    foreach (var localSalesTransaction in localSalesTransactions)
                    {
                        localSalesTransaction.SyncStatus = SyncStatus.Failed;
                        failedSalesTransactions.Add((localSalesTransaction.Id, bulkResult.Error));
                    }

                    await context.SaveChangesAsync();

                    return new SyncResult<List<Models.SupabaseModels.SupaSalesTransactions>>
                    {
                        IsSuccess = false,
                        Error = bulkResult.Error,
                        Message = $"Bulk sync failed for all {localSalesTransactions.Count} SalesTransactions: {bulkResult.Message}"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during bulk SalesTransaction sync");

                // Mark all SalesTransactions as failed
                foreach (var salesTransaction in localSalesTransactions)
                {
                    if (salesTransaction.SyncStatus != SyncStatus.Failed)
                    {
                        salesTransaction.SyncStatus = SyncStatus.Failed;
                    }
                }

                await context.SaveChangesAsync();

                return new SyncResult<List<Models.SupabaseModels.SupaSalesTransactions>>
                {
                    IsSuccess = false,
                    Error = ex.Message,
                    Message = "Bulk SalesTransaction sync operation failed with exception"
                };
            }
        }

        /// <summary>
        /// Syncs a list of Shifts with Supabase using bulk operations
        /// </summary>
        /// <param name="localShifts">List of local Shifts to sync</param>
        /// <param name="retailer">The retailer for authentication</param>
        /// <returns>SyncResult containing the synced Shifts</returns>
        public async Task<SyncResult<List<Models.SupabaseModels.SupaShifts>>> SyncShiftsAsync(
            List<Shift> localShifts,
            Retailer retailer)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            retailer = await EnsureInitializedAsync(retailer);

            var results = new List<Models.SupabaseModels.SupaShifts>();
            var failedShifts = new List<(int ShiftId, string Error)>();

            // Process Shifts in bulk instead of individually
            try
            {
                // Map all local Shifts to Supabase models
                var supaShifts = new List<Models.SupabaseModels.SupaShifts>();

                foreach (var localShift in localShifts)
                {
                    var supaShift = new Models.SupabaseModels.SupaShifts
                    {
                        RetailerId = retailer.RetailerId,
                        Shift_Id = localShift.Id,
                        DayLog_Id = localShift.DayLog_Id,
                        PosUser_Id = localShift.PosUser_Id,
                        Shift_Start_DateTime = localShift.Shift_Start_DateTime.ToUniversalTime(),
                        Shift_End_DateTime = localShift.Shift_End_DateTime?.ToUniversalTime(),
                        Opening_Cash_Amount = localShift.Opening_Cash_Amount ?? 0.00m,
                        Closing_Cash_Amount = localShift.Closing_Cash_Amount ?? 0.00m,
                        Expected_Cash_Amount = localShift.Expected_Cash_Amount ?? 0.00m,
                        Cash_Variance = localShift.Cash_Variance ?? 0.00m,
                        Is_Active = localShift.Is_Active,
                        Shift_Notes = localShift.Shift_Notes,
                        Closing_Notes = localShift.Closing_Notes,
                        Date_Created = localShift.Date_Created.ToUniversalTime(),
                        Last_Modified = localShift.Last_Modified.ToUniversalTime(),
                        Created_By_Id = localShift.Created_By_Id,
                        Last_Modified_By_Id = localShift.Last_Modified_By_Id,
                        Site_Id = localShift.Site_Id,
                        Till_Id = localShift.Till_Id,
                        SyncStatus = SyncStatus.Synced
                    };

                    supaShifts.Add(supaShift);
                }

                // Use bulk upsert for all Shifts at once
                var bulkResult = await UpsertListAsync("Shifts", supaShifts, "Shift_Id,RetailerId", retailer);

                if (bulkResult.IsSuccess && bulkResult.Data != null)
                {
                    // Create a dictionary for fast lookup of synced results by Shift_Id
                    var syncedResultsDict = bulkResult.Data.ToDictionary(
                        sr => sr.Shift_Id,
                        sr => sr
                    );

                    // Update local Shifts with the results from bulk operation
                    foreach (var localShift in localShifts)
                    {
                        if (syncedResultsDict.TryGetValue(localShift.Id, out var syncedShift))
                        {
                            // Update sync status and timestamp
                            localShift.Supa_Id = syncedShift.Supa_Id;
                            localShift.SyncStatus = SyncStatus.Synced;
                            localShift.Last_Modified = DateTime.UtcNow;

                            _logger.LogInformation($"Successfully synced Shift ID {localShift.Id}");
                        }
                        else
                        {
                            _logger.LogWarning($"Could not find synced result for Shift ID {localShift.Id}");
                            localShift.SyncStatus = SyncStatus.Failed;
                        }
                    }

                    results.AddRange(bulkResult.Data);

                    // Save all changes to the database in one transaction
                    await context.SaveChangesAsync();

                    // Return overall result
                    return new SyncResult<List<Models.SupabaseModels.SupaShifts>>
                    {
                        IsSuccess = true,
                        Data = results,
                        Message = $"Successfully bulk synced {results.Count} Shifts"
                    };
                }
                else
                {
                    // Handle bulk operation failure - mark all Shifts as failed
                    foreach (var localShift in localShifts)
                    {
                        localShift.SyncStatus = SyncStatus.Failed;
                        failedShifts.Add((localShift.Id, bulkResult.Error));
                    }

                    await context.SaveChangesAsync();

                    return new SyncResult<List<Models.SupabaseModels.SupaShifts>>
                    {
                        IsSuccess = false,
                        Error = bulkResult.Error,
                        Message = $"Bulk sync failed for all {localShifts.Count} Shifts: {bulkResult.Message}"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during bulk Shift sync");

                // Mark all Shifts as failed
                foreach (var shift in localShifts)
                {
                    if (shift.SyncStatus != SyncStatus.Failed)
                    {
                        shift.SyncStatus = SyncStatus.Failed;
                    }
                }

                await context.SaveChangesAsync();

                return new SyncResult<List<Models.SupabaseModels.SupaShifts>>
                {
                    IsSuccess = false,
                    Error = ex.Message,
                    Message = "Bulk Shift sync operation failed with exception"
                };
            }
        }

        /// <summary>
        /// Syncs a list of Sites with Supabase using bulk operations
        /// </summary>
        /// <param name="localSites">List of local Sites to sync</param>
        /// <param name="retailer">The retailer for authentication</param>
        /// <returns>SyncResult containing the synced Sites</returns>
        public async Task<SyncResult<List<Models.SupabaseModels.SupaSites>>> SyncSitesAsync(
            List<Site> localSites,
            Retailer retailer)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            retailer = await EnsureInitializedAsync(retailer);

            var results = new List<Models.SupabaseModels.SupaSites>();
            var failedSites = new List<(int SiteId, string Error)>();

            // Process Sites in bulk instead of individually
            try
            {
                // Map all local Sites to Supabase models
                var supaSites = new List<Models.SupabaseModels.SupaSites>();

                foreach (var localSite in localSites)
                {
                    var supaSite = new Models.SupabaseModels.SupaSites
                    {
                        RetailerId = retailer.RetailerId,
                        Site_Id = localSite.Id,
                        Site_BusinessName = localSite.Site_BusinessName,
                        Site_AddressLine1 = localSite.Site_AddressLine1,
                        Site_AddressLine2 = localSite.Site_AddressLine2,
                        Site_City = localSite.Site_City,
                        Site_County = localSite.Site_County,
                        Site_Country = localSite.Site_Country,
                        Site_Postcode = localSite.Site_Postcode,
                        Site_ContactNumber = localSite.Site_ContactNumber,
                        Site_Email = localSite.Site_Email,
                        Site_VatNumber = localSite.Site_VatNumber,
                        Is_Primary = localSite.Is_Primary,
                        Is_Active = localSite.Is_Active,
                        Is_Deleted = localSite.Is_Deleted,
                        Date_Created = localSite.Date_Created.ToUniversalTime(),
                        Last_Modified = localSite.Last_Modified.ToUniversalTime(),
                        Created_By_Id = localSite.Created_By_Id,
                        Last_Modified_By_Id = localSite.Last_Modified_By_Id,
                        SyncStatus = SyncStatus.Synced
                    };

                    supaSites.Add(supaSite);
                }

                // Use bulk upsert for all Sites at once
                var bulkResult = await UpsertListAsync("Sites", supaSites, "Site_Id,RetailerId", retailer);

                if (bulkResult.IsSuccess && bulkResult.Data != null)
                {
                    // Create a dictionary for fast lookup of synced results by Site_Id
                    var syncedResultsDict = bulkResult.Data.ToDictionary(
                        sr => sr.Site_Id,
                        sr => sr
                    );

                    // Update local Sites with the results from bulk operation
                    foreach (var localSite in localSites)
                    {
                        if (syncedResultsDict.TryGetValue(localSite.Id, out var syncedSite))
                        {
                            // Update sync status and timestamp
                            localSite.Supa_Id = syncedSite.Supa_Id;
                            localSite.SyncStatus = SyncStatus.Synced;
                            localSite.Last_Modified = DateTime.UtcNow;

                            _logger.LogInformation($"Successfully synced Site ID {localSite.Id}");
                        }
                        else
                        {
                            _logger.LogWarning($"Could not find synced result for Site ID {localSite.Id}");
                            localSite.SyncStatus = SyncStatus.Failed;
                        }
                    }

                    results.AddRange(bulkResult.Data);

                    // Save all changes to the database in one transaction
                    await context.SaveChangesAsync();

                    // Return overall result
                    return new SyncResult<List<Models.SupabaseModels.SupaSites>>
                    {
                        IsSuccess = true,
                        Data = results,
                        Message = $"Successfully bulk synced {results.Count} Sites"
                    };
                }
                else
                {
                    // Handle bulk operation failure - mark all Sites as failed
                    foreach (var localSite in localSites)
                    {
                        localSite.SyncStatus = SyncStatus.Failed;
                        failedSites.Add((localSite.Id, bulkResult.Error));
                    }

                    await context.SaveChangesAsync();

                    return new SyncResult<List<Models.SupabaseModels.SupaSites>>
                    {
                        IsSuccess = false,
                        Error = bulkResult.Error,
                        Message = $"Bulk sync failed for all {localSites.Count} Sites: {bulkResult.Message}"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during bulk Site sync");

                // Mark all Sites as failed
                foreach (var site in localSites)
                {
                    if (site.SyncStatus != SyncStatus.Failed)
                    {
                        site.SyncStatus = SyncStatus.Failed;
                    }
                }

                await context.SaveChangesAsync();

                return new SyncResult<List<Models.SupabaseModels.SupaSites>>
                {
                    IsSuccess = false,
                    Error = ex.Message,
                    Message = "Bulk Site sync operation failed with exception"
                };
            }
        }

        /// <summary>
        /// Syncs a list of StockRefills with Supabase using bulk operations
        /// </summary>
        /// <param name="localStockRefills">List of local StockRefills to sync</param>
        /// <param name="retailer">The retailer for authentication</param>
        /// <returns>SyncResult containing the synced StockRefills</returns>
        public async Task<SyncResult<List<Models.SupabaseModels.SupaStockRefills>>> SyncStockRefillsAsync(
            List<StockRefill> localStockRefills,
            Retailer retailer)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            retailer = await EnsureInitializedAsync(retailer);

            var results = new List<Models.SupabaseModels.SupaStockRefills>();
            var failedStockRefills = new List<(int StockRefillId, string Error)>();

            // Process StockRefills in bulk instead of individually
            try
            {
                // Map all local StockRefills to Supabase models
                var supaStockRefills = new List<Models.SupabaseModels.SupaStockRefills>();

                foreach (var localStockRefill in localStockRefills)
                {
                    var supaStockRefill = new Models.SupabaseModels.SupaStockRefills
                    {
                        RetailerId = retailer.RetailerId,
                        StockRefill_ID = localStockRefill.Id,
                        SaleTransaction_Item_ID = localStockRefill.SaleTransaction_Item_ID,
                        Refilled_By = localStockRefill.Refilled_By,
                        Refilled_Date = localStockRefill.Refilled_Date?.ToUniversalTime() ?? DateTime.UtcNow,
                        Confirmed_By_Scanner = localStockRefill.Confirmed_By_Scanner,
                        Refill_Quantity = localStockRefill.Refill_Quantity,
                        Quantity_Refilled = localStockRefill.Quantity_Refilled,
                        Stock_Refilled = localStockRefill.Stock_Refilled,
                        Shift_ID = localStockRefill.Shift_ID,
                        DayLog_ID = localStockRefill.DayLog_ID,
                        Created_By_ID = localStockRefill.Created_By_ID,
                        Last_Modified_By_ID = localStockRefill.Last_Modified_By_ID,
                        Notes = localStockRefill.Notes,
                        Date_Created = localStockRefill.Date_Created.ToUniversalTime(),
                        Last_Modified = localStockRefill.Last_Modified.ToUniversalTime(),
                        SyncStatus = localStockRefill.SyncStatus
                    };

                    supaStockRefills.Add(supaStockRefill);
                }

                // Use bulk upsert for all StockRefills at once
                var bulkResult = await UpsertListAsync("StockRefills", supaStockRefills, "StockRefill_ID,RetailerId", retailer);

                if (bulkResult.IsSuccess && bulkResult.Data != null)
                {
                    // Create a dictionary for fast lookup of synced results by StockRefill_ID
                    var syncedResultsDict = bulkResult.Data.ToDictionary(
                        sr => sr.StockRefill_ID,
                        sr => sr
                    );

                    // Update local StockRefills with the results from bulk operation
                    foreach (var localStockRefill in localStockRefills)
                    {
                        if (syncedResultsDict.TryGetValue(localStockRefill.Id, out var syncedStockRefill))
                        {
                            // Update sync status and timestamp
                            localStockRefill.Supa_Id = syncedStockRefill.Supa_Id;
                            localStockRefill.SyncStatus = SyncStatus.Synced;
                            localStockRefill.Last_Modified = DateTime.UtcNow;

                            _logger.LogInformation($"Successfully synced StockRefill ID {localStockRefill.Id}");
                        }
                        else
                        {
                            _logger.LogWarning($"Could not find synced result for StockRefill ID {localStockRefill.Id}");
                            localStockRefill.SyncStatus = SyncStatus.Failed;
                        }
                    }

                    results.AddRange(bulkResult.Data);

                    // Save all changes to the database in one transaction
                    await context.SaveChangesAsync();

                    // Return overall result
                    return new SyncResult<List<Models.SupabaseModels.SupaStockRefills>>
                    {
                        IsSuccess = true,
                        Data = results,
                        Message = $"Successfully bulk synced {results.Count} StockRefills"
                    };
                }
                else
                {
                    // Handle bulk operation failure - mark all StockRefills as failed
                    foreach (var localStockRefill in localStockRefills)
                    {
                        localStockRefill.SyncStatus = SyncStatus.Failed;
                        failedStockRefills.Add((localStockRefill.Id, bulkResult.Error));
                    }

                    await context.SaveChangesAsync();

                    return new SyncResult<List<Models.SupabaseModels.SupaStockRefills>>
                    {
                        IsSuccess = false,
                        Error = bulkResult.Error,
                        Message = $"Bulk sync failed for all {localStockRefills.Count} StockRefills: {bulkResult.Message}"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during bulk StockRefill sync");

                // Mark all StockRefills as failed
                foreach (var stockRefill in localStockRefills)
                {
                    if (stockRefill.SyncStatus != SyncStatus.Failed)
                    {
                        stockRefill.SyncStatus = SyncStatus.Failed;
                    }
                }

                await context.SaveChangesAsync();

                return new SyncResult<List<Models.SupabaseModels.SupaStockRefills>>
                {
                    IsSuccess = false,
                    Error = ex.Message,
                    Message = "Bulk StockRefill sync operation failed with exception"
                };
            }
        }

        /// <summary>
        /// Syncs a list of StockTransactions with Supabase using bulk operations
        /// </summary>
        /// <param name="localStockTransactions">List of local StockTransactions to sync</param>
        /// <param name="retailer">The retailer for authentication</param>
        /// <returns>SyncResult containing the synced StockTransactions</returns>
        public async Task<SyncResult<List<Models.SupabaseModels.SupaStockTransactions>>> SyncStockTransactionsAsync(
            List<StockTransaction> localStockTransactions,
            Retailer retailer)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            retailer = await EnsureInitializedAsync(retailer);

            var results = new List<Models.SupabaseModels.SupaStockTransactions>();
            var failedStockTransactions = new List<(int StockTransactionId, string Error)>();

            // Process StockTransactions in bulk instead of individually
            try
            {
                // Map all local StockTransactions to Supabase models
                var supaStockTransactions = new List<Models.SupabaseModels.SupaStockTransactions>();

                foreach (var localStockTransaction in localStockTransactions)
                {
                    var supaStockTransaction = new Models.SupabaseModels.SupaStockTransactions
                    {
                        RetailerId = retailer.RetailerId,
                        StockTransactionId = localStockTransaction.Id,
                        StockTransactionType = localStockTransaction.StockTransactionType,
                        ProductId = localStockTransaction.ProductId,
                        Quantity = localStockTransaction.Quantity,
                        TotalAmount = localStockTransaction.TotalAmount,
                        DayLogId = localStockTransaction.DayLogId,
                        TransactionDate = localStockTransaction.TransactionDate,
                        DateCreated = localStockTransaction.DateCreated.ToUniversalTime(),
                        LastModified = localStockTransaction.LastModified.ToUniversalTime(),
                        Shift_Id = localStockTransaction.Shift_Id,
                        Created_By_Id = localStockTransaction.Created_By_Id,
                        Last_Modified_By_Id = localStockTransaction.Last_Modified_By_Id,
                        From_Site_Id = localStockTransaction.From_Site_Id,
                        To_Site_Id = localStockTransaction.To_Site_Id,
                        Till_Id = localStockTransaction.Till_Id,
                        SyncStatus = SyncStatus.Synced
                    };

                    supaStockTransactions.Add(supaStockTransaction);
                }

                // Use bulk upsert for all StockTransactions at once
                var bulkResult = await UpsertListAsync("StockTransactions", supaStockTransactions, "StockTransactionId,RetailerId", retailer);

                if (bulkResult.IsSuccess && bulkResult.Data != null)
                {
                    // Create a dictionary for fast lookup of synced results by StockTransactionId
                    var syncedResultsDict = bulkResult.Data.ToDictionary(
                        sr => sr.StockTransactionId,
                        sr => sr
                    );

                    // Update local StockTransactions with the results from bulk operation
                    foreach (var localStockTransaction in localStockTransactions)
                    {
                        if (syncedResultsDict.TryGetValue(localStockTransaction.Id, out var syncedStockTransaction))
                        {
                            // Update sync status and timestamp
                            localStockTransaction.Supa_Id = syncedStockTransaction.Supa_Id;
                            localStockTransaction.SyncStatus = SyncStatus.Synced;
                            localStockTransaction.LastModified = DateTime.UtcNow;

                            _logger.LogInformation($"Successfully synced StockTransaction ID {localStockTransaction.Id}");
                        }
                        else
                        {
                            _logger.LogWarning($"Could not find synced result for StockTransaction ID {localStockTransaction.Id}");
                            localStockTransaction.SyncStatus = SyncStatus.Failed;
                        }
                    }

                    results.AddRange(bulkResult.Data);

                    // Save all changes to the database in one transaction
                    await context.SaveChangesAsync();

                    // Return overall result
                    return new SyncResult<List<Models.SupabaseModels.SupaStockTransactions>>
                    {
                        IsSuccess = true,
                        Data = results,
                        Message = $"Successfully bulk synced {results.Count} StockTransactions"
                    };
                }
                else
                {
                    // Handle bulk operation failure - mark all StockTransactions as failed
                    foreach (var localStockTransaction in localStockTransactions)
                    {
                        localStockTransaction.SyncStatus = SyncStatus.Failed;
                        failedStockTransactions.Add((localStockTransaction.Id, bulkResult.Error));
                    }

                    await context.SaveChangesAsync();

                    return new SyncResult<List<Models.SupabaseModels.SupaStockTransactions>>
                    {
                        IsSuccess = false,
                        Error = bulkResult.Error,
                        Message = $"Bulk sync failed for all {localStockTransactions.Count} StockTransactions: {bulkResult.Message}"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during bulk StockTransaction sync");

                // Mark all StockTransactions as failed
                foreach (var stockTransaction in localStockTransactions)
                {
                    if (stockTransaction.SyncStatus != SyncStatus.Failed)
                    {
                        stockTransaction.SyncStatus = SyncStatus.Failed;
                    }
                }

                await context.SaveChangesAsync();

                return new SyncResult<List<Models.SupabaseModels.SupaStockTransactions>>
                {
                    IsSuccess = false,
                    Error = ex.Message,
                    Message = "Bulk StockTransaction sync operation failed with exception"
                };
            }
        }

        /// <summary>
        /// Syncs a list of SupplierItems with Supabase using bulk operations
        /// </summary>
        /// <param name="localSupplierItems">List of local SupplierItems to sync</param>
        /// <param name="retailer">The retailer for authentication</param>
        /// <returns>SyncResult containing the synced SupplierItems</returns>
        public async Task<SyncResult<List<Models.SupabaseModels.SupaSupplierItems>>> SyncSupplierItemsAsync(
            List<SupplierItem> localSupplierItems,
            Retailer retailer)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            retailer = await EnsureInitializedAsync(retailer);

            var results = new List<Models.SupabaseModels.SupaSupplierItems>();
            var failedSupplierItems = new List<(int SupplierItemId, string Error)>();

            // Process SupplierItems in bulk instead of individually
            try
            {
                // Map all local SupplierItems to Supabase models
                var supaSupplierItems = new List<Models.SupabaseModels.SupaSupplierItems>();

                foreach (var localSupplierItem in localSupplierItems)
                {
                    var supaSupplierItem = new Models.SupabaseModels.SupaSupplierItems
                    {
                        RetailerId = retailer.RetailerId,
                        SupplierItemId = localSupplierItem.Id,
                        SupplierId = localSupplierItem.SupplierId,
                        ProductId = localSupplierItem.ProductId,
                        Supplier_Product_Code = localSupplierItem.Supplier_Product_Code,
                        Product_OuterCaseBarcode = localSupplierItem.Product_OuterCaseBarcode,
                        Cost_Per_Case = localSupplierItem.Cost_Per_Case,
                        Cost_Per_Unit = localSupplierItem.Cost_Per_Unit,
                        Unit_Per_Case = localSupplierItem.Unit_Per_Case,
                        Profit_On_Return = localSupplierItem.Profit_On_Return,
                        Is_Active = localSupplierItem.Is_Active,
                        Is_Deleted = localSupplierItem.Is_Deleted,
                        Date_Created = localSupplierItem.Date_Created.ToUniversalTime(),
                        Last_Modified = localSupplierItem.Last_Modified.ToUniversalTime(),
                        Created_By_Id = localSupplierItem.Created_By_Id,
                        Last_Modified_By_Id = localSupplierItem.Last_Modified_By_Id,
                        Site_Id = localSupplierItem.Site_Id,
                        Till_Id = localSupplierItem.Till_Id,
                        SyncStatus = SyncStatus.Synced
                    };

                    supaSupplierItems.Add(supaSupplierItem);
                }

                // Use bulk upsert for all SupplierItems at once
                var bulkResult = await UpsertListAsync("SupplierItems", supaSupplierItems, "SupplierItemId,RetailerId", retailer);

                if (bulkResult.IsSuccess && bulkResult.Data != null)
                {
                    // Create a dictionary for fast lookup of synced results by SupplierItemId
                    var syncedResultsDict = bulkResult.Data.ToDictionary(
                        sr => sr.SupplierItemId,
                        sr => sr
                    );

                    // Update local SupplierItems with the results from bulk operation
                    foreach (var localSupplierItem in localSupplierItems)
                    {
                        if (syncedResultsDict.TryGetValue(localSupplierItem.Id, out var syncedSupplierItem))
                        {
                            // Update sync status and timestamp
                            localSupplierItem.Supa_Id = syncedSupplierItem.Supa_Id;
                            localSupplierItem.SyncStatus = SyncStatus.Synced;
                            localSupplierItem.Last_Modified = DateTime.UtcNow;

                            _logger.LogInformation($"Successfully synced SupplierItem ID {localSupplierItem.Id}");
                        }
                        else
                        {
                            _logger.LogWarning($"Could not find synced result for SupplierItem ID {localSupplierItem.Id}");
                            localSupplierItem.SyncStatus = SyncStatus.Failed;
                        }
                    }

                    results.AddRange(bulkResult.Data);

                    // Save all changes to the database in one transaction
                    await context.SaveChangesAsync();

                    // Return overall result
                    return new SyncResult<List<Models.SupabaseModels.SupaSupplierItems>>
                    {
                        IsSuccess = true,
                        Data = results,
                        Message = $"Successfully bulk synced {results.Count} SupplierItems"
                    };
                }
                else
                {
                    // Handle bulk operation failure - mark all SupplierItems as failed
                    foreach (var localSupplierItem in localSupplierItems)
                    {
                        localSupplierItem.SyncStatus = SyncStatus.Failed;
                        failedSupplierItems.Add((localSupplierItem.Id, bulkResult.Error));
                    }

                    await context.SaveChangesAsync();

                    return new SyncResult<List<Models.SupabaseModels.SupaSupplierItems>>
                    {
                        IsSuccess = false,
                        Error = bulkResult.Error,
                        Message = $"Bulk sync failed for all {localSupplierItems.Count} SupplierItems: {bulkResult.Message}"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during bulk SupplierItem sync");

                // Mark all SupplierItems as failed
                foreach (var supplierItem in localSupplierItems)
                {
                    if (supplierItem.SyncStatus != SyncStatus.Failed)
                    {
                        supplierItem.SyncStatus = SyncStatus.Failed;
                    }
                }

                await context.SaveChangesAsync();

                return new SyncResult<List<Models.SupabaseModels.SupaSupplierItems>>
                {
                    IsSuccess = false,
                    Error = ex.Message,
                    Message = "Bulk SupplierItem sync operation failed with exception"
                };
            }
        }

        /// <summary>
        /// Syncs a list of Suppliers with Supabase using bulk operations
        /// </summary>
        /// <param name="localSuppliers">List of local Suppliers to sync</param>
        /// <param name="retailer">The retailer for authentication</param>
        /// <returns>SyncResult containing the synced Suppliers</returns>
        public async Task<SyncResult<List<Models.SupabaseModels.SupaSuppliers>>> SyncSuppliersAsync(
            List<Supplier> localSuppliers,
            Retailer retailer)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            retailer = await EnsureInitializedAsync(retailer);

            var results = new List<Models.SupabaseModels.SupaSuppliers>();
            var failedSuppliers = new List<(int SupplierId, string Error)>();

            // Process Suppliers in bulk instead of individually
            try
            {
                // Map all local Suppliers to Supabase models
                var supaSuppliers = new List<Models.SupabaseModels.SupaSuppliers>();

                foreach (var localSupplier in localSuppliers)
                {
                    var supaSupplier = new Models.SupabaseModels.SupaSuppliers
                    {
                        RetailerId = retailer.RetailerId,
                        Supplier_ID = localSupplier.Id,
                        Supplier_Name = localSupplier.Supplier_Name,
                        Supplier_Description = localSupplier.Supplier_Description,
                        Supplier_Address = localSupplier.Supplier_Address,
                        Supplier_Phone = localSupplier.Supplier_Phone,
                        Supplier_Mobile = localSupplier.Supplier_Mobile,
                        Supplier_Email = localSupplier.Supplier_Email,
                        Supplier_Website = localSupplier.Supplier_Website,
                        Supplier_Credit_Limit = localSupplier.Supplier_Credit_Limit,
                        Is_Activated = localSupplier.Is_Activated,
                        Is_Deleted = localSupplier.Is_Deleted,
                        Date_Created = localSupplier.Date_Created.ToUniversalTime(),
                        Last_Modified = localSupplier.Last_Modified.ToUniversalTime(),
                        Created_By_Id = localSupplier.Created_By_Id,
                        Last_Modified_By_Id = localSupplier.Last_Modified_By_Id,
                        Site_Id = localSupplier.Site_Id,
                        Till_Id = localSupplier.Till_Id,
                        SyncStatus = SyncStatus.Synced
                    };

                    supaSuppliers.Add(supaSupplier);
                }

                // Use bulk upsert for all Suppliers at once
                var bulkResult = await UpsertListAsync("Suppliers", supaSuppliers, "Supplier_ID,RetailerId", retailer);

                if (bulkResult.IsSuccess && bulkResult.Data != null)
                {
                    // Create a dictionary for fast lookup of synced results by Supplier_ID
                    var syncedResultsDict = bulkResult.Data.ToDictionary(
                        sr => sr.Supplier_ID,
                        sr => sr
                    );

                    // Update local Suppliers with the results from bulk operation
                    foreach (var localSupplier in localSuppliers)
                    {
                        if (syncedResultsDict.TryGetValue(localSupplier.Id, out var syncedSupplier))
                        {
                            // Update sync status and timestamp
                            localSupplier.Supa_Id = syncedSupplier.Supa_Id;
                            localSupplier.SyncStatus = SyncStatus.Synced;
                            localSupplier.Last_Modified = DateTime.UtcNow;

                            _logger.LogInformation($"Successfully synced Supplier ID {localSupplier.Id}");
                        }
                        else
                        {
                            _logger.LogWarning($"Could not find synced result for Supplier ID {localSupplier.Id}");
                            localSupplier.SyncStatus = SyncStatus.Failed;
                        }
                    }

                    results.AddRange(bulkResult.Data);

                    // Save all changes to the database in one transaction
                    await context.SaveChangesAsync();

                    // Return overall result
                    return new SyncResult<List<Models.SupabaseModels.SupaSuppliers>>
                    {
                        IsSuccess = true,
                        Data = results,
                        Message = $"Successfully bulk synced {results.Count} Suppliers"
                    };
                }
                else
                {
                    // Handle bulk operation failure - mark all Suppliers as failed
                    foreach (var localSupplier in localSuppliers)
                    {
                        localSupplier.SyncStatus = SyncStatus.Failed;
                        failedSuppliers.Add((localSupplier.Id, bulkResult.Error));
                    }

                    await context.SaveChangesAsync();

                    return new SyncResult<List<Models.SupabaseModels.SupaSuppliers>>
                    {
                        IsSuccess = false,
                        Error = bulkResult.Error,
                        Message = $"Bulk sync failed for all {localSuppliers.Count} Suppliers: {bulkResult.Message}"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during bulk Supplier sync");

                // Mark all Suppliers as failed
                foreach (var supplier in localSuppliers)
                {
                    if (supplier.SyncStatus != SyncStatus.Failed)
                    {
                        supplier.SyncStatus = SyncStatus.Failed;
                    }
                }

                await context.SaveChangesAsync();

                return new SyncResult<List<Models.SupabaseModels.SupaSuppliers>>
                {
                    IsSuccess = false,
                    Error = ex.Message,
                    Message = "Bulk Supplier sync operation failed with exception"
                };
            }
        }

        /// <summary>
        /// Syncs a list of Tills with Supabase using bulk operations
        /// </summary>
        /// <param name="localTills">List of local Tills to sync</param>
        /// <param name="retailer">The retailer for authentication</param>
        /// <returns>SyncResult containing the synced Tills</returns>
        public async Task<SyncResult<List<Models.SupabaseModels.SupaTills>>> SyncTillsAsync(
            List<Till> localTills,
            Retailer retailer)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            retailer = await EnsureInitializedAsync(retailer);

            var results = new List<Models.SupabaseModels.SupaTills>();
            var failedTills = new List<(int TillId, string Error)>();

            // Process Tills in bulk instead of individually
            try
            {
                // Map all local Tills to Supabase models
                var supaTills = new List<Models.SupabaseModels.SupaTills>();

                foreach (var localTill in localTills)
                {
                    var supaTill = new Models.SupabaseModels.SupaTills
                    {
                        RetailerId = retailer.RetailerId,
                        Till_Id = localTill.Id,
                        Till_Name = localTill.Till_Name,
                        Till_IP_Address = localTill.Till_IP_Address,
                        Till_Port_Number = localTill.Till_Port_Number,
                        Till_Password = localTill.Till_Password,
                        Is_Primary = localTill.Is_Primary,
                        Is_Active = localTill.Is_Active,
                        Is_Deleted = localTill.Is_Deleted,
                        Date_Created = localTill.Date_Created.ToUniversalTime(),
                        Last_Modified = localTill.Last_Modified.ToUniversalTime(),
                        Created_By_Id = localTill.Created_By_Id,
                        Last_Modified_By_Id = localTill.Last_Modified_By_Id,
                        Site_Id = localTill.Site_Id,
                        SyncStatus = localTill.SyncStatus
                    };

                    supaTills.Add(supaTill);
                }

                // Use bulk upsert for all Tills at once
                var bulkResult = await UpsertListAsync("Tills", supaTills, "Till_Id,RetailerId", retailer);

                if (bulkResult.IsSuccess && bulkResult.Data != null)
                {
                    // Create a dictionary for fast lookup of synced results by Till_Id
                    var syncedResultsDict = bulkResult.Data.ToDictionary(
                        sr => sr.Till_Id,
                        sr => sr
                    );

                    // Update local Tills with the results from bulk operation
                    foreach (var localTill in localTills)
                    {
                        if (syncedResultsDict.TryGetValue(localTill.Id, out var syncedTill))
                        {
                            // Update sync status and timestamp
                            localTill.Supa_Id = syncedTill.Supa_Id;
                            localTill.SyncStatus = SyncStatus.Synced;
                            localTill.Last_Modified = DateTime.UtcNow;

                            _logger.LogInformation($"Successfully synced Till ID {localTill.Id}");
                        }
                        else
                        {
                            _logger.LogWarning($"Could not find synced result for Till ID {localTill.Id}");
                            localTill.SyncStatus = SyncStatus.Failed;
                        }
                    }

                    results.AddRange(bulkResult.Data);

                    // Save all changes to the database in one transaction
                    await context.SaveChangesAsync();

                    // Return overall result
                    return new SyncResult<List<Models.SupabaseModels.SupaTills>>
                    {
                        IsSuccess = true,
                        Data = results,
                        Message = $"Successfully bulk synced {results.Count} Tills"
                    };
                }
                else
                {
                    // Handle bulk operation failure - mark all Tills as failed
                    foreach (var localTill in localTills)
                    {
                        localTill.SyncStatus = SyncStatus.Failed;
                        failedTills.Add((localTill.Id, bulkResult.Error));
                    }

                    await context.SaveChangesAsync();

                    return new SyncResult<List<Models.SupabaseModels.SupaTills>>
                    {
                        IsSuccess = false,
                        Error = bulkResult.Error,
                        Message = $"Bulk sync failed for all {localTills.Count} Tills: {bulkResult.Message}"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during bulk Till sync");

                // Mark all Tills as failed
                foreach (var till in localTills)
                {
                    if (till.SyncStatus != SyncStatus.Failed)
                    {
                        till.SyncStatus = SyncStatus.Failed;
                    }
                }

                await context.SaveChangesAsync();

                return new SyncResult<List<Models.SupabaseModels.SupaTills>>
                {
                    IsSuccess = false,
                    Error = ex.Message,
                    Message = "Bulk Till sync operation failed with exception"
                };
            }
        }

        /// <summary>
        /// Syncs a list of UserSiteAccesses with Supabase using bulk operations
        /// </summary>
        /// <param name="localUserSiteAccesses">List of local UserSiteAccesses to sync</param>
        /// <param name="retailer">The retailer for authentication</param>
        /// <returns>SyncResult containing the synced UserSiteAccesses</returns>
        public async Task<SyncResult<List<Models.SupabaseModels.SupaUserSiteAccesses>>> SyncUserSiteAccessesAsync(
            List<UserSiteAccess> localUserSiteAccesses,
            Retailer retailer)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            retailer = await EnsureInitializedAsync(retailer);

            var results = new List<Models.SupabaseModels.SupaUserSiteAccesses>();
            var failedUserSiteAccesses = new List<(int UserSiteAccessId, string Error)>();

            // Process UserSiteAccesses in bulk instead of individually
            try
            {
                // Map all local UserSiteAccesses to Supabase models
                var supaUserSiteAccesses = new List<Models.SupabaseModels.SupaUserSiteAccesses>();

                foreach (var localUserSiteAccess in localUserSiteAccesses)
                {
                    var supaUserSiteAccess = new Models.SupabaseModels.SupaUserSiteAccesses
                    {
                        RetailerId = retailer.RetailerId,
                        UserSiteAccess_ID = localUserSiteAccess.Id,
                        User_Id = localUserSiteAccess.User_Id,
                        Site_Id = localUserSiteAccess.Site_Id,
                        Is_Active = localUserSiteAccess.Is_Active,
                        Is_Deleted = localUserSiteAccess.Is_Deleted,
                        Date_Granted = localUserSiteAccess.Date_Granted,
                        Date_Revoked = localUserSiteAccess.Date_Revoked?.ToUniversalTime(),
                        Date_Created = localUserSiteAccess.Date_Created.ToUniversalTime(),
                        Last_Modified = localUserSiteAccess.Last_Modified.ToUniversalTime(),
                        Created_By_Id = localUserSiteAccess.Created_By_Id,
                        Last_Modified_By_Id = localUserSiteAccess.Last_Modified_By_Id,
                        Till_Id = localUserSiteAccess.Till_Id,
                        SyncStatus = SyncStatus.Synced
                    };

                    supaUserSiteAccesses.Add(supaUserSiteAccess);
                }

                // Use bulk upsert for all UserSiteAccesses at once
                var bulkResult = await UpsertListAsync("UserSiteAccesses", supaUserSiteAccesses, "UserSiteAccess_ID,RetailerId", retailer);

                if (bulkResult.IsSuccess && bulkResult.Data != null)
                {
                    // Create a dictionary for fast lookup of synced results by UserSiteAccess_ID
                    var syncedResultsDict = bulkResult.Data.ToDictionary(
                        sr => sr.UserSiteAccess_ID,
                        sr => sr
                    );

                    // Update local UserSiteAccesses with the results from bulk operation
                    foreach (var localUserSiteAccess in localUserSiteAccesses)
                    {
                        if (syncedResultsDict.TryGetValue(localUserSiteAccess.Id, out var syncedUserSiteAccess))
                        {
                            // Update sync status and timestamp
                            localUserSiteAccess.Supa_Id = syncedUserSiteAccess.Supa_Id;
                            localUserSiteAccess.SyncStatus = SyncStatus.Synced;
                            localUserSiteAccess.Last_Modified = DateTime.UtcNow;

                            _logger.LogInformation($"Successfully synced UserSiteAccess ID {localUserSiteAccess.Id}");
                        }
                        else
                        {
                            _logger.LogWarning($"Could not find synced result for UserSiteAccess ID {localUserSiteAccess.Id}");
                            localUserSiteAccess.SyncStatus = SyncStatus.Failed;
                        }
                    }

                    results.AddRange(bulkResult.Data);

                    // Save all changes to the database in one transaction
                    await context.SaveChangesAsync();

                    // Return overall result
                    return new SyncResult<List<Models.SupabaseModels.SupaUserSiteAccesses>>
                    {
                        IsSuccess = true,
                        Data = results,
                        Message = $"Successfully bulk synced {results.Count} UserSiteAccesses"
                    };
                }
                else
                {
                    // Handle bulk operation failure - mark all UserSiteAccesses as failed
                    foreach (var localUserSiteAccess in localUserSiteAccesses)
                    {
                        localUserSiteAccess.SyncStatus = SyncStatus.Failed;
                        failedUserSiteAccesses.Add((localUserSiteAccess.Id, bulkResult.Error));
                    }

                    await context.SaveChangesAsync();

                    return new SyncResult<List<Models.SupabaseModels.SupaUserSiteAccesses>>
                    {
                        IsSuccess = false,
                        Error = bulkResult.Error,
                        Message = $"Bulk sync failed for all {localUserSiteAccesses.Count} UserSiteAccesses: {bulkResult.Message}"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during bulk UserSiteAccess sync");

                // Mark all UserSiteAccesses as failed
                foreach (var userSiteAccess in localUserSiteAccesses)
                {
                    if (userSiteAccess.SyncStatus != SyncStatus.Failed)
                    {
                        userSiteAccess.SyncStatus = SyncStatus.Failed;
                    }
                }

                await context.SaveChangesAsync();

                return new SyncResult<List<Models.SupabaseModels.SupaUserSiteAccesses>>
                {
                    IsSuccess = false,
                    Error = ex.Message,
                    Message = "Bulk UserSiteAccess sync operation failed with exception"
                };
            }
        }

        /// <summary>
        /// Syncs a list of VoidedProducts with Supabase using bulk operations
        /// </summary>
        /// <param name="localVoidedProducts">List of local VoidedProducts to sync</param>
        /// <param name="retailer">The retailer for authentication</param>
        /// <returns>SyncResult containing the synced VoidedProducts</returns>
        public async Task<SyncResult<List<Models.SupabaseModels.SupaVoidedProducts>>> SyncVoidedProductsAsync(
            List<VoidedProduct> localVoidedProducts,
            Retailer retailer)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            retailer = await EnsureInitializedAsync(retailer);

            var results = new List<Models.SupabaseModels.SupaVoidedProducts>();
            var failedVoidedProducts = new List<(int VoidedProductId, string Error)>();

            // Process VoidedProducts in bulk instead of individually
            try
            {
                // Map all local VoidedProducts to Supabase models
                var supaVoidedProducts = new List<Models.SupabaseModels.SupaVoidedProducts>();

                foreach (var localVoidedProduct in localVoidedProducts)
                {
                    var supaVoidedProduct = new Models.SupabaseModels.SupaVoidedProducts
                    {
                        RetailerId = retailer.RetailerId,
                        VoidedProduct_ID = localVoidedProduct.Id,
                        Product_ID = localVoidedProduct.Product_ID,
                        Voided_Quantity = localVoidedProduct.Voided_Quantity,
                        Voided_Amount = localVoidedProduct.Voided_Amount,
                        Voided_By_User_ID = localVoidedProduct.Voided_By_User_ID,
                        Void_Date = localVoidedProduct.Void_Date.ToUniversalTime(),
                        Additional_Notes = localVoidedProduct.Additional_Notes,
                        Date_Created = localVoidedProduct.Date_Created.ToUniversalTime(),
                        Last_Modified = localVoidedProduct.Last_Modified.ToUniversalTime(),
                        Created_By_Id = localVoidedProduct.Created_By_Id,
                        Last_Modified_By_Id = localVoidedProduct.Last_Modified_By_Id,
                        Site_Id = localVoidedProduct.Site_Id,
                        Till_Id = localVoidedProduct.Till_Id,
                        Daylog_Id = localVoidedProduct.Daylog_Id,
                        Shift_Id = localVoidedProduct.Shift_Id,
                        SyncStatus = localVoidedProduct.SyncStatus
                    };

                    supaVoidedProducts.Add(supaVoidedProduct);
                }

                // Use bulk upsert for all VoidedProducts at once
                var bulkResult = await UpsertListAsync("VoidedProducts", supaVoidedProducts, "VoidedProduct_ID,RetailerId", retailer);

                if (bulkResult.IsSuccess && bulkResult.Data != null)
                {
                    // Create a dictionary for fast lookup of synced results by VoidedProduct_ID
                    var syncedResultsDict = bulkResult.Data.ToDictionary(
                        sr => sr.VoidedProduct_ID,
                        sr => sr
                    );

                    // Update local VoidedProducts with the results from bulk operation
                    foreach (var localVoidedProduct in localVoidedProducts)
                    {
                        if (syncedResultsDict.TryGetValue(localVoidedProduct.Id, out var syncedVoidedProduct))
                        {
                            // Update sync status and timestamp
                            localVoidedProduct.Supa_Id = syncedVoidedProduct.Supa_Id;
                            localVoidedProduct.SyncStatus = SyncStatus.Synced;
                            localVoidedProduct.Last_Modified = DateTime.UtcNow;

                            _logger.LogInformation($"Successfully synced VoidedProduct ID {localVoidedProduct.Id}");
                        }
                        else
                        {
                            _logger.LogWarning($"Could not find synced result for VoidedProduct ID {localVoidedProduct.Id}");
                            localVoidedProduct.SyncStatus = SyncStatus.Failed;
                        }
                    }

                    results.AddRange(bulkResult.Data);

                    // Save all changes to the database in one transaction
                    await context.SaveChangesAsync();

                    // Return overall result
                    return new SyncResult<List<Models.SupabaseModels.SupaVoidedProducts>>
                    {
                        IsSuccess = true,
                        Data = results,
                        Message = $"Successfully bulk synced {results.Count} VoidedProducts"
                    };
                }
                else
                {
                    // Handle bulk operation failure - mark all VoidedProducts as failed
                    foreach (var localVoidedProduct in localVoidedProducts)
                    {
                        localVoidedProduct.SyncStatus = SyncStatus.Failed;
                        failedVoidedProducts.Add((localVoidedProduct.Id, bulkResult.Error));
                    }

                    await context.SaveChangesAsync();

                    return new SyncResult<List<Models.SupabaseModels.SupaVoidedProducts>>
                    {
                        IsSuccess = false,
                        Error = bulkResult.Error,
                        Message = $"Bulk sync failed for all {localVoidedProducts.Count} VoidedProducts: {bulkResult.Message}"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during bulk VoidedProduct sync");

                // Mark all VoidedProducts as failed
                foreach (var voidedProduct in localVoidedProducts)
                {
                    if (voidedProduct.SyncStatus != SyncStatus.Failed)
                    {
                        voidedProduct.SyncStatus = SyncStatus.Failed;
                    }
                }

                await context.SaveChangesAsync();

                return new SyncResult<List<Models.SupabaseModels.SupaVoidedProducts>>
                {
                    IsSuccess = false,
                    Error = ex.Message,
                    Message = "Bulk VoidedProduct sync operation failed with exception"
                };
            }
        }
        /// <summary>
        /// Syncs a list of ReceiptPrinters with Supabase using bulk operations
        /// </summary>
        /// <param name="localReceiptPrinters">List of local ReceiptPrinters to sync</param>
        /// <param name="retailer">The retailer for authentication</param>
        /// <returns>SyncResult containing the synced ReceiptPrinters</returns>
        public async Task<SyncResult<List<Models.SupabaseModels.SupaReceiptPrinters>>> SyncReceiptPrintersAsync(
            List<ReceiptPrinter> localReceiptPrinters,
            Retailer retailer)
        {
            retailer = await EnsureInitializedAsync(retailer);
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext


            var results = new List<Models.SupabaseModels.SupaReceiptPrinters>();
            var failedReceiptPrinters = new List<(int PrinterId, string Error)>();

            // Process ReceiptPrinters in bulk instead of individually
            try
            {
                // Map all local ReceiptPrinters to Supabase models
                var supaReceiptPrinters = new List<Models.SupabaseModels.SupaReceiptPrinters>();

                foreach (var localReceiptPrinter in localReceiptPrinters)
                {
                    var supaReceiptPrinter = new Models.SupabaseModels.SupaReceiptPrinters
                    {
                        RetailerId = retailer.RetailerId,
                        Printer_Id = localReceiptPrinter.Id,
                        Printer_Name = localReceiptPrinter.Printer_Name,
                        Printer_IP_Address = localReceiptPrinter.Printer_IP_Address,
                        Printer_Port_Number = localReceiptPrinter.Printer_Port_Number,
                        Printer_Password = localReceiptPrinter.Printer_Password,
                        Paper_Width = localReceiptPrinter.Paper_Width,
                        Is_Active = localReceiptPrinter.Is_Active,
                        Is_Deleted = localReceiptPrinter.Is_Deleted,
                        Date_Created = localReceiptPrinter.Date_Created.ToUniversalTime(),
                        Last_Modified = localReceiptPrinter.Last_Modified.ToUniversalTime(),
                        Created_By_Id = localReceiptPrinter.Created_By_Id,
                        Last_Modified_By_Id = localReceiptPrinter.Last_Modified_By_Id,
                        Site_Id = localReceiptPrinter.Site_Id,
                        Till_Id = localReceiptPrinter.Till_Id,
                        SyncStatus = SyncStatus.Synced,
                        Print_Receipt = localReceiptPrinter.Print_Receipt,
                        Print_Label = localReceiptPrinter.Print_Label
                    };

                    supaReceiptPrinters.Add(supaReceiptPrinter);
                }

                // Use bulk upsert for all ReceiptPrinters at once
                var bulkResult = await UpsertListAsync("ReceiptPrinters", supaReceiptPrinters, "Printer_Id,RetailerId", retailer);

                if (bulkResult.IsSuccess && bulkResult.Data != null)
                {
                    // Create a dictionary for fast lookup of synced results by Printer_Id
                    var syncedResultsDict = bulkResult.Data.ToDictionary(
                        sr => sr.Printer_Id,
                        sr => sr
                    );

                    // Update local ReceiptPrinters with the results from bulk operation
                    foreach (var localReceiptPrinter in localReceiptPrinters)
                    {
                        if (syncedResultsDict.TryGetValue(localReceiptPrinter.Id, out var syncedReceiptPrinter))
                        {
                            // Update sync status and timestamp
                            localReceiptPrinter.Supa_Id = syncedReceiptPrinter.Supa_Id;
                            localReceiptPrinter.SyncStatus = SyncStatus.Synced;
                            localReceiptPrinter.Last_Modified = DateTime.UtcNow;

                            _logger.LogInformation($"Successfully synced ReceiptPrinter ID {localReceiptPrinter.Id}");
                        }
                        else
                        {
                            _logger.LogWarning($"Could not find synced result for ReceiptPrinter ID {localReceiptPrinter.Id}");
                            localReceiptPrinter.SyncStatus = SyncStatus.Failed;
                        }
                    }

                    results.AddRange(bulkResult.Data);

                    // Save all changes to the database in one transaction
                    await context.SaveChangesAsync();

                    // Return overall result
                    return new SyncResult<List<Models.SupabaseModels.SupaReceiptPrinters>>
                    {
                        IsSuccess = true,
                        Data = results,
                        Message = $"Successfully bulk synced {results.Count} ReceiptPrinters"
                    };
                }
                else
                {
                    // Handle bulk operation failure - mark all ReceiptPrinters as failed
                    foreach (var localReceiptPrinter in localReceiptPrinters)
                    {
                        localReceiptPrinter.SyncStatus = SyncStatus.Failed;
                        failedReceiptPrinters.Add((localReceiptPrinter.Id, bulkResult.Error));
                    }

                    await context.SaveChangesAsync();

                    return new SyncResult<List<Models.SupabaseModels.SupaReceiptPrinters>>
                    {
                        IsSuccess = false,
                        Error = bulkResult.Error,
                        Message = $"Bulk sync failed for all {localReceiptPrinters.Count} ReceiptPrinters: {bulkResult.Message}"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during bulk ReceiptPrinter sync");

                // Mark all ReceiptPrinters as failed
                foreach (var receiptPrinter in localReceiptPrinters)
                {
                    if (receiptPrinter.SyncStatus != SyncStatus.Failed)
                    {
                        receiptPrinter.SyncStatus = SyncStatus.Failed;
                    }
                }

                await context.SaveChangesAsync();

                return new SyncResult<List<Models.SupabaseModels.SupaReceiptPrinters>>
                {
                    IsSuccess = false,
                    Error = ex.Message,
                    Message = "Bulk ReceiptPrinter sync operation failed with exception"
                };
            }
        }

        /// <summary>
        /// Syncs a list of UnknownProducts with Supabase using bulk operations
        /// </summary>
        /// <param name="localUnknownProducts">List of local UnknownProducts to sync</param>
        /// <param name="retailer">The retailer for authentication</param>
        /// <returns>SyncResult containing the synced UnknownProducts</returns>
        public async Task<SyncResult<List<Models.SupabaseModels.SupaUnknownProduct>>> SyncUnknownProductsAsync(
            List<DataHandlerLibrary.Models.UnknownProduct> localUnknownProducts,
            Retailer retailer)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            retailer = await EnsureInitializedAsync(retailer);


            var results = new List<Models.SupabaseModels.SupaUnknownProduct>();
            var failedUnknownProducts = new List<(int UnknownProductId, string Error)>();

            // Process UnknownProducts in bulk instead of individually
            try
            {
                // Map all local UnknownProducts to Supabase models
                var supaUnknownProducts = new List<Models.SupabaseModels.SupaUnknownProduct>();

                foreach (var localUnknownProduct in localUnknownProducts)
                {
                    var supaUnknownProduct = new Models.SupabaseModels.SupaUnknownProduct
                    {
                        Id = localUnknownProduct.Id,
                        ProductBarcode = localUnknownProduct.ProductBarcode,
                        IsResolved = localUnknownProduct.IsResolved,
                        DateCreated = localUnknownProduct.DateCreated.ToUniversalTime(),
                        LastModified = localUnknownProduct.LastModified.ToUniversalTime(),
                        RetailerId = retailer.RetailerId,
                        DaylogId = localUnknownProduct.DaylogId,
                        ShiftId = localUnknownProduct.ShiftId,
                        SiteId = localUnknownProduct.SiteId,
                        TillId = localUnknownProduct.TillId,
                        CreatedById = localUnknownProduct.CreatedById,
                        SyncStatus = SyncStatus.Synced,
                    };

                    supaUnknownProducts.Add(supaUnknownProduct);
                }

                // Perform bulk upsert
                var bulkResult = await UpsertListAsync("UnknownProducts", supaUnknownProducts, "Id,RetailerId", retailer);

                if (bulkResult.IsSuccess && bulkResult.Data != null)
                {
                    // Create a dictionary for fast lookup using Id as the key
                    var syncedUnknownProductsDict = bulkResult.Data.ToDictionary(sup => sup.Id, sup => sup);

                    // Update local UnknownProducts with synced data using dictionary lookup
                    foreach (var localUnknownProduct in localUnknownProducts)
                    {
                        if (syncedUnknownProductsDict.TryGetValue(localUnknownProduct.Id, out var syncedUnknownProduct))
                        {
                            // Update local record with synced data
                            localUnknownProduct.Supa_Id = syncedUnknownProduct.Supa_Id;
                            localUnknownProduct.SyncStatus = SyncStatus.Synced;
                            results.Add(syncedUnknownProduct);
                        }
                        else
                        {
                            // Handle unmatched records
                            var errorMessage = $"UnknownProduct with Id {localUnknownProduct.Id} was not found in sync result";
                            _logger.LogWarning(errorMessage);
                            localUnknownProduct.SyncStatus = SyncStatus.Failed;
                            failedUnknownProducts.Add((localUnknownProduct.Id, errorMessage));
                        }
                    }

                    // Save changes to local database
                    await context.SaveChangesAsync();

                    var successMessage = $"Successfully synced {results.Count} UnknownProducts";
                    if (failedUnknownProducts.Any())
                    {
                        successMessage += $", {failedUnknownProducts.Count} failed";
                    }

                    return new SyncResult<List<Models.SupabaseModels.SupaUnknownProduct>>
                    {
                        Data = results,
                        IsSuccess = true,
                        Message = successMessage
                    };
                }
                else
                {
                    // Handle bulk operation failure
                    foreach (var localUnknownProduct in localUnknownProducts)
                    {
                        localUnknownProduct.SyncStatus = SyncStatus.Failed;
                    }

                    await context.SaveChangesAsync();

                    return new SyncResult<List<Models.SupabaseModels.SupaUnknownProduct>>
                    {
                        Data = new List<Models.SupabaseModels.SupaUnknownProduct>(),
                        IsSuccess = false,
                        Error = bulkResult.Error ?? "Unknown error occurred during bulk sync",
                        Message = "Bulk UnknownProduct sync operation failed"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred during bulk UnknownProduct sync");

                // Update all local UnknownProducts with error status
                foreach (var localUnknownProduct in localUnknownProducts)
                {
                    localUnknownProduct.SyncStatus = SyncStatus.Failed;
                }

                await context.SaveChangesAsync();

                return new SyncResult<List<Models.SupabaseModels.SupaUnknownProduct>>
                {
                    Data = new List<Models.SupabaseModels.SupaUnknownProduct>(),
                    IsSuccess = false,
                    Error = ex.Message,
                    Message = "Bulk UnknownProduct sync operation failed with exception"
                };
            }
        }

        /// <summary>
        /// Master sync method that synchronizes all local database entities to Supabase cloud
        /// </summary>
        /// <param name="retailer">The retailer for authentication</param>
        /// <param name="batchSize">Number of records to sync in each batch (default: 100)</param>
        /// <returns>Comprehensive sync result with details for all entities</returns>
        public async Task<ComprehensiveSyncResult> SyncDatabaseToCloudAsync(Retailer retailer, int batchSize = 100)
        {
            using var context = _dbFactory.CreateDbContext();
            retailer = await EnsureInitializedAsync(retailer);

            if (retailer == null)
            {
                throw new InvalidOperationException("No retailer found in user session.");
            }
            var comprehensiveResult = new ComprehensiveSyncResult
            {
                StartTime = DateTime.UtcNow,
                EntityResults = new Dictionary<string, EntitySyncResult>()
            };

            _logger.LogInformation($"Starting comprehensive database sync for retailer {retailer?.RetailerId}");

            try
            {
                // 1. First fetch all UnSyncedLogs from local database
                var unSyncedLogs = await context.Set<UnSyncedLog>()
                    .Where(u => u.SyncStatus == SyncStatus.Pending || u.SyncStatus == SyncStatus.Failed)
                    .ToListAsync();

                _logger.LogInformation($"Found {unSyncedLogs.Count} unsynced records to process");

                // 2. Group by TableName
                var groupedLogs = unSyncedLogs.GroupBy(u => u.TableName).ToList();

                foreach (var tableGroup in groupedLogs)
                {
                    var tableName = tableGroup.Key;
                    var recordIds = tableGroup.Select(g => g.RecordId).ToList();

                    _logger.LogInformation($"Processing {recordIds.Count} records for table: {tableName}");

                    // 3. Switch statement to fetch relevant data based on table name
                    switch (tableName.ToLower())
                    {
                        case "products":
                            await SyncEntityBatch("Products", async () =>
                            {
                                var pendingProducts = await context.Set<Product>()
                                    .Include(p => p.Department)
                                    .Include(p => p.VAT)
                                    .Where(p => recordIds.Contains(p.Id))
                                    .ToListAsync();

                                if (pendingProducts.Any())
                                {
                                    var result = await SyncProductsAsync(pendingProducts, retailer);

                                    // 4. Update UnSyncedLogs to Synced on success
                                    if (result.IsSuccess)
                                    {
                                        var logsToUpdate = tableGroup.Where(g => pendingProducts.Any(p => p.Id == g.RecordId));
                                        foreach (var log in logsToUpdate)
                                        {
                                            log.SyncStatus = SyncStatus.Synced;
                                            log.LastSyncedAt = DateTime.UtcNow;
                                        }
                                        await context.SaveChangesAsync();
                                    }

                                    return new EntitySyncResult
                                    {
                                        EntityName = "Products",
                                        TotalRecords = pendingProducts.Count,
                                        SuccessCount = result.IsSuccess ? pendingProducts.Count : 0,
                                        FailureCount = result.IsSuccess ? 0 : pendingProducts.Count,
                                        IsSuccess = result.IsSuccess,
                                        ErrorMessage = result.Error ?? ""
                                    };
                                }
                                return new EntitySyncResult { EntityName = "Products", TotalRecords = 0, IsSuccess = true };
                            }, comprehensiveResult);
                            break;

                        case "daylogs":
                            await SyncEntityBatch("DayLogs", async () =>
                            {
                                var pendingDayLogs = await context.Set<DayLog>()
                                    .Where(d => recordIds.Contains(d.Id))
                                    .ToListAsync();

                                if (pendingDayLogs.Any())
                                {
                                    var result = await SyncDayLogsAsync(pendingDayLogs, retailer);

                                    if (result.IsSuccess)
                                    {
                                        var logsToUpdate = tableGroup.Where(g => pendingDayLogs.Any(d => d.Id == g.RecordId));
                                        foreach (var log in logsToUpdate)
                                        {
                                            log.SyncStatus = SyncStatus.Synced;
                                            log.LastSyncedAt = DateTime.UtcNow;
                                        }
                                        await context.SaveChangesAsync();
                                    }

                                    return new EntitySyncResult
                                    {
                                        EntityName = "DayLogs",
                                        TotalRecords = pendingDayLogs.Count,
                                        SuccessCount = result.IsSuccess ? pendingDayLogs.Count : 0,
                                        FailureCount = result.IsSuccess ? 0 : pendingDayLogs.Count,
                                        IsSuccess = result.IsSuccess,
                                        ErrorMessage = result.Error
                                    };
                                }
                                return new EntitySyncResult { EntityName = "DayLogs", TotalRecords = 0, IsSuccess = true };
                            }, comprehensiveResult);
                            break;

                        case "drawerlogs":
                            await SyncEntityBatch("DrawerLogs", async () =>
                            {
                                var pendingDrawerLogs = await context.Set<DrawerLog>()
                                    .Where(d => recordIds.Contains(d.Id))
                                    .ToListAsync();

                                if (pendingDrawerLogs.Any())
                                {
                                    var result = await SyncDrawerLogsAsync(pendingDrawerLogs, retailer);

                                    if (result.IsSuccess)
                                    {
                                        var logsToUpdate = tableGroup.Where(g => pendingDrawerLogs.Any(d => d.Id == g.RecordId));
                                        foreach (var log in logsToUpdate)
                                        {
                                            log.SyncStatus = SyncStatus.Synced;
                                            log.LastSyncedAt = DateTime.UtcNow;
                                        }
                                        await context.SaveChangesAsync();
                                    }

                                    return new EntitySyncResult
                                    {
                                        EntityName = "DrawerLogs",
                                        TotalRecords = pendingDrawerLogs.Count,
                                        SuccessCount = result.IsSuccess ? pendingDrawerLogs.Count : 0,
                                        FailureCount = result.IsSuccess ? 0 : pendingDrawerLogs.Count,
                                        IsSuccess = result.IsSuccess,
                                        ErrorMessage = result.Error
                                    };
                                }
                                return new EntitySyncResult { EntityName = "DrawerLogs", TotalRecords = 0, IsSuccess = true };
                            }, comprehensiveResult);
                            break;

                        case "supplieritems":
                            await SyncEntityBatch("SupplierItems", async () =>
                            {
                                var pendingSupplierItems = await context.Set<SupplierItem>()
                                    .Where(s => recordIds.Contains(s.Id))
                                    .ToListAsync();

                                if (pendingSupplierItems.Any())
                                {
                                    var result = await SyncSupplierItemsAsync(pendingSupplierItems, retailer);

                                    if (result.IsSuccess)
                                    {
                                        var logsToUpdate = tableGroup.Where(g => pendingSupplierItems.Any(s => s.Id == g.RecordId));
                                        foreach (var log in logsToUpdate)
                                        {
                                            log.SyncStatus = SyncStatus.Synced;
                                            log.LastSyncedAt = DateTime.UtcNow;
                                        }
                                        await context.SaveChangesAsync();
                                    }

                                    return new EntitySyncResult
                                    {
                                        EntityName = "SupplierItems",
                                        TotalRecords = pendingSupplierItems.Count,
                                        SuccessCount = result.IsSuccess ? pendingSupplierItems.Count : 0,
                                        FailureCount = result.IsSuccess ? 0 : pendingSupplierItems.Count,
                                        IsSuccess = result.IsSuccess,
                                        ErrorMessage = result.Error
                                    };
                                }
                                return new EntitySyncResult { EntityName = "SupplierItems", TotalRecords = 0, IsSuccess = true };
                            }, comprehensiveResult);
                            break;

                        case "voidedproducts":
                            await SyncEntityBatch("VoidedProducts", async () =>
                            {
                                var pendingVoidedProducts = await context.Set<VoidedProduct>()
                                    .Where(v => recordIds.Contains(v.Id))
                                    .ToListAsync();

                                if (pendingVoidedProducts.Any())
                                {
                                    var result = await SyncVoidedProductsAsync(pendingVoidedProducts, retailer);

                                    if (result.IsSuccess)
                                    {
                                        var logsToUpdate = tableGroup.Where(g => pendingVoidedProducts.Any(v => v.Id == g.RecordId));
                                        foreach (var log in logsToUpdate)
                                        {
                                            log.SyncStatus = SyncStatus.Synced;
                                            log.LastSyncedAt = DateTime.UtcNow;
                                        }
                                        await context.SaveChangesAsync();
                                    }

                                    return new EntitySyncResult
                                    {
                                        EntityName = "VoidedProducts",
                                        TotalRecords = pendingVoidedProducts.Count,
                                        SuccessCount = result.IsSuccess ? pendingVoidedProducts.Count : 0,
                                        FailureCount = result.IsSuccess ? 0 : pendingVoidedProducts.Count,
                                        IsSuccess = result.IsSuccess,
                                        ErrorMessage = result.Error
                                    };
                                }
                                return new EntitySyncResult { EntityName = "VoidedProducts", TotalRecords = 0, IsSuccess = true };
                            }, comprehensiveResult);
                            break;

                        case "salestransactions":
                            await SyncEntityBatch("SalesTransactions", async () =>
                            {
                                var pendingSalesTransactions = await context.Set<SalesTransaction>()
                                    .Where(s => recordIds.Contains(s.Id))
                                    .ToListAsync();

                                if (pendingSalesTransactions.Any())
                                {
                                    var result = await SyncSalesTransactionsAsync(pendingSalesTransactions, retailer);

                                    if (result.IsSuccess)
                                    {
                                        var logsToUpdate = tableGroup.Where(g => pendingSalesTransactions.Any(s => s.Id == g.RecordId));
                                        foreach (var log in logsToUpdate)
                                        {
                                            log.SyncStatus = SyncStatus.Synced;
                                            log.LastSyncedAt = DateTime.UtcNow;
                                        }
                                        await context.SaveChangesAsync();
                                    }

                                    return new EntitySyncResult
                                    {
                                        EntityName = "SalesTransactions",
                                        TotalRecords = pendingSalesTransactions.Count,
                                        SuccessCount = result.IsSuccess ? pendingSalesTransactions.Count : 0,
                                        FailureCount = result.IsSuccess ? 0 : pendingSalesTransactions.Count,
                                        IsSuccess = result.IsSuccess,
                                        ErrorMessage = result.Error
                                    };
                                }
                                return new EntitySyncResult { EntityName = "SalesTransactions", TotalRecords = 0, IsSuccess = true };
                            }, comprehensiveResult);
                            break;

                        case "salesitemtransactions":
                            await SyncEntityBatch("SalesItemTransactions", async () =>
                            {
                                var pendingSalesItemTransactions = await context.Set<SalesItemTransaction>()
                                    .Where(s => recordIds.Contains(s.Id))
                                    .ToListAsync();

                                if (pendingSalesItemTransactions.Any())
                                {
                                    var result = await SyncSalesItemTransactionsAsync(pendingSalesItemTransactions, retailer);

                                    // Update UnSyncedLogs to Synced on success
                                    if (result.IsSuccess)
                                    {
                                        var logsToUpdate = tableGroup.Where(g => pendingSalesItemTransactions.Any(s => s.Id == g.RecordId));
                                        foreach (var log in logsToUpdate)
                                        {
                                            log.SyncStatus = SyncStatus.Synced;
                                            log.LastSyncedAt = DateTime.UtcNow;
                                        }
                                        await context.SaveChangesAsync();
                                    }

                                    return new EntitySyncResult
                                    {
                                        EntityName = "SalesItemTransactions",
                                        TotalRecords = pendingSalesItemTransactions.Count,
                                        SuccessCount = result.IsSuccess ? pendingSalesItemTransactions.Count : 0,
                                        FailureCount = result.IsSuccess ? 0 : pendingSalesItemTransactions.Count,
                                        IsSuccess = result.IsSuccess,
                                        ErrorMessage = result.Error
                                    };
                                }
                                return new EntitySyncResult { EntityName = "SalesItemTransactions", TotalRecords = 0, IsSuccess = true };
                            }, comprehensiveResult);
                            break;

                        case "errorlogs":
                            await SyncEntityBatch("ErrorLogs", async () =>
                            {
                                var pendingErrorLogs = await context.Set<ErrorLog>()
                                    .Where(e => recordIds.Contains(e.Id))
                                    .ToListAsync();

                                if (pendingErrorLogs.Any())
                                {
                                    var result = await SyncErrorLogsAsync(pendingErrorLogs, retailer);

                                    if (result.IsSuccess)
                                    {
                                        var logsToUpdate = tableGroup.Where(g => pendingErrorLogs.Any(e => e.Id == g.RecordId));
                                        foreach (var log in logsToUpdate)
                                        {
                                            log.SyncStatus = SyncStatus.Synced;
                                            log.LastSyncedAt = DateTime.UtcNow;
                                        }
                                        await context.SaveChangesAsync();
                                    }

                                    return new EntitySyncResult
                                    {
                                        EntityName = "ErrorLogs",
                                        TotalRecords = pendingErrorLogs.Count,
                                        SuccessCount = result.IsSuccess ? pendingErrorLogs.Count : 0,
                                        FailureCount = result.IsSuccess ? 0 : pendingErrorLogs.Count,
                                        IsSuccess = result.IsSuccess,
                                        ErrorMessage = result.Error
                                    };
                                }
                                return new EntitySyncResult { EntityName = "ErrorLogs", TotalRecords = 0, IsSuccess = true };
                            }, comprehensiveResult);
                            break;
                        case "payouts":
                            await SyncEntityBatch("Payouts", async () =>
                            {
                                var pendingPayouts = await context.Set<Payout>()
                                    .Where(p => recordIds.Contains(p.Id))
                                    .ToListAsync();

                                if (pendingPayouts.Any())
                                {
                                    var result = await SyncPayoutsAsync(pendingPayouts, retailer);

                                    if (result.IsSuccess)
                                    {
                                        var logsToUpdate = tableGroup.Where(g => pendingPayouts.Any(p => p.Id == g.RecordId));
                                        foreach (var log in logsToUpdate)
                                        {
                                            log.SyncStatus = SyncStatus.Synced;
                                            log.LastSyncedAt = DateTime.UtcNow;
                                        }
                                        await context.SaveChangesAsync();
                                    }

                                    return new EntitySyncResult
                                    {
                                        EntityName = "Payouts",
                                        TotalRecords = pendingPayouts.Count,
                                        SuccessCount = result.IsSuccess ? pendingPayouts.Count : 0,
                                        FailureCount = result.IsSuccess ? 0 : pendingPayouts.Count,
                                        IsSuccess = result.IsSuccess,
                                        ErrorMessage = result.Error
                                    };
                                }
                                return new EntitySyncResult { EntityName = "Payouts", TotalRecords = 0, IsSuccess = true };
                            }, comprehensiveResult);
                            break;
                        case "posusers":
                            await SyncEntityBatch("PosUsers", async () =>
                            {
                                var pendingPosUsers = await context.Set<PosUser>()
                                    .Where(p => recordIds.Contains(p.Id))
                                    .ToListAsync();

                                if (pendingPosUsers.Any())
                                {
                                    var result = await SyncPosUsersAsync(pendingPosUsers, retailer);

                                    if (result.IsSuccess)
                                    {
                                        var logsToUpdate = tableGroup.Where(g => pendingPosUsers.Any(p => p.Id == g.RecordId));
                                        foreach (var log in logsToUpdate)
                                        {
                                            log.SyncStatus = SyncStatus.Synced;
                                            log.LastSyncedAt = DateTime.UtcNow;
                                        }
                                        await context.SaveChangesAsync();
                                    }

                                    return new EntitySyncResult
                                    {
                                        EntityName = "PosUsers",
                                        TotalRecords = pendingPosUsers.Count,
                                        SuccessCount = result.IsSuccess ? pendingPosUsers.Count : 0,
                                        FailureCount = result.IsSuccess ? 0 : pendingPosUsers.Count,
                                        IsSuccess = result.IsSuccess,
                                        ErrorMessage = result.Error
                                    };
                                }
                                return new EntitySyncResult { EntityName = "PosUsers", TotalRecords = 0, IsSuccess = true };
                            }, comprehensiveResult);
                            break;
                        case "promotions":
                            await SyncEntityBatch("Promotions", async () =>
                            {
                                var pendingPromotions = await context.Set<Promotion>()
                                    .Where(p => recordIds.Contains(p.Id))
                                    .ToListAsync();

                                if (pendingPromotions.Any())
                                {
                                    var result = await SyncPromotionsAsync(pendingPromotions, retailer);

                                    if (result.IsSuccess)
                                    {
                                        var logsToUpdate = tableGroup.Where(g => pendingPromotions.Any(p => p.Id == g.RecordId));
                                        foreach (var log in logsToUpdate)
                                        {
                                            log.SyncStatus = SyncStatus.Synced;
                                            log.LastSyncedAt = DateTime.UtcNow;
                                        }
                                        await context.SaveChangesAsync();
                                    }

                                    return new EntitySyncResult
                                    {
                                        EntityName = "Promotions",
                                        TotalRecords = pendingPromotions.Count,
                                        SuccessCount = result.IsSuccess ? pendingPromotions.Count : 0,
                                        FailureCount = result.IsSuccess ? 0 : pendingPromotions.Count,
                                        IsSuccess = result.IsSuccess,
                                        ErrorMessage = result.Error
                                    };
                                }
                                return new EntitySyncResult { EntityName = "Promotions", TotalRecords = 0, IsSuccess = true };
                            }, comprehensiveResult);
                            break;
                        case "receiptprinters":
                            await SyncEntityBatch("ReceiptPrinters", async () =>
                            {
                                var pendingReceiptPrinters = await context.Set<Models.ReceiptPrinter>()
                                    .Where(rp => recordIds.Contains(rp.Id))
                                    .ToListAsync();

                                if (pendingReceiptPrinters.Any())
                                {
                                    var result = await SyncReceiptPrintersAsync(pendingReceiptPrinters, retailer);

                                    if (result.IsSuccess)
                                    {
                                        var logsToUpdate = tableGroup.Where(g => pendingReceiptPrinters.Any(rp => rp.Id == g.RecordId));
                                        foreach (var log in logsToUpdate)
                                        {
                                            log.SyncStatus = SyncStatus.Synced;
                                            log.LastSyncedAt = DateTime.UtcNow;
                                        }
                                        await context.SaveChangesAsync();
                                    }

                                    return new EntitySyncResult
                                    {
                                        EntityName = "ReceiptPrinters",
                                        TotalRecords = pendingReceiptPrinters.Count,
                                        SuccessCount = result.IsSuccess ? pendingReceiptPrinters.Count : 0,
                                        FailureCount = result.IsSuccess ? 0 : pendingReceiptPrinters.Count,
                                        IsSuccess = result.IsSuccess,
                                        ErrorMessage = result.Error
                                    };
                                }
                                return new EntitySyncResult { EntityName = "ReceiptPrinters", TotalRecords = 0, IsSuccess = true };
                            }, comprehensiveResult);
                            break;
                        case "shifts":
                            await SyncEntityBatch("Shifts", async () =>
                            {
                                var pendingShifts = await context.Set<Shift>()
                                    .Where(s => recordIds.Contains(s.Id))
                                    .ToListAsync();

                                if (pendingShifts.Any())
                                {
                                    var result = await SyncShiftsAsync(pendingShifts, retailer);

                                    if (result.IsSuccess)
                                    {
                                        var logsToUpdate = tableGroup.Where(g => pendingShifts.Any(s => s.Id == g.RecordId));
                                        foreach (var log in logsToUpdate)
                                        {
                                            log.SyncStatus = SyncStatus.Synced;
                                            log.LastSyncedAt = DateTime.UtcNow;
                                        }
                                        await context.SaveChangesAsync();
                                    }

                                    return new EntitySyncResult
                                    {
                                        EntityName = "Shifts",
                                        TotalRecords = pendingShifts.Count,
                                        SuccessCount = result.IsSuccess ? pendingShifts.Count : 0,
                                        FailureCount = result.IsSuccess ? 0 : pendingShifts.Count,
                                        IsSuccess = result.IsSuccess,
                                        ErrorMessage = result.Error
                                    };
                                }
                                return new EntitySyncResult { EntityName = "Shifts", TotalRecords = 0, IsSuccess = true };
                            }, comprehensiveResult);
                            break;
                        case "sites":
                            await SyncEntityBatch("Sites", async () =>
                            {
                                var pendingSites = await context.Set<Site>()
                                    .Where(s => recordIds.Contains(s.Id))
                                    .ToListAsync();

                                if (pendingSites.Any())
                                {
                                    var result = await SyncSitesAsync(pendingSites, retailer);

                                    if (result.IsSuccess)
                                    {
                                        var logsToUpdate = tableGroup.Where(g => pendingSites.Any(s => s.Id == g.RecordId));
                                        foreach (var log in logsToUpdate)
                                        {
                                            log.SyncStatus = SyncStatus.Synced;
                                            log.LastSyncedAt = DateTime.UtcNow;
                                        }
                                        await context.SaveChangesAsync();
                                    }

                                    return new EntitySyncResult
                                    {
                                        EntityName = "Sites",
                                        TotalRecords = pendingSites.Count,
                                        SuccessCount = result.IsSuccess ? pendingSites.Count : 0,
                                        FailureCount = result.IsSuccess ? 0 : pendingSites.Count,
                                        IsSuccess = result.IsSuccess,
                                        ErrorMessage = result.Error
                                    };
                                }
                                return new EntitySyncResult { EntityName = "Sites", TotalRecords = 0, IsSuccess = true };
                            }, comprehensiveResult);
                            break;
                        case "stockrefills":
                            await SyncEntityBatch("StockRefills", async () =>
                            {
                                var pendingStockRefills = await context.Set<StockRefill>()
                                    .Where(s => recordIds.Contains(s.Id))
                                    .ToListAsync();

                                if (pendingStockRefills.Any())
                                {
                                    var result = await SyncStockRefillsAsync(pendingStockRefills, retailer);

                                    if (result.IsSuccess)
                                    {
                                        var logsToUpdate = tableGroup.Where(g => pendingStockRefills.Any(s => s.Id == g.RecordId));
                                        foreach (var log in logsToUpdate)
                                        {
                                            log.SyncStatus = SyncStatus.Synced;
                                            log.LastSyncedAt = DateTime.UtcNow;
                                        }
                                        await context.SaveChangesAsync();
                                    }

                                    return new EntitySyncResult
                                    {
                                        EntityName = "StockRefills",
                                        TotalRecords = pendingStockRefills.Count,
                                        SuccessCount = result.IsSuccess ? pendingStockRefills.Count : 0,
                                        FailureCount = result.IsSuccess ? 0 : pendingStockRefills.Count,
                                        IsSuccess = result.IsSuccess,
                                        ErrorMessage = result.Error
                                    };
                                }
                                return new EntitySyncResult { EntityName = "StockRefills", TotalRecords = 0, IsSuccess = true };
                            }, comprehensiveResult);
                            break;
                        case "stocktransactions":

                            await SyncEntityBatch("StockTransactions", async () =>
                            {
                                var pendingStockTransactions = await context.Set<StockTransaction>()
                                    .Where(s => recordIds.Contains(s.Id))
                                    .ToListAsync();

                                if (pendingStockTransactions.Any())
                                {
                                    var result = await SyncStockTransactionsAsync(pendingStockTransactions, retailer);

                                    if (result.IsSuccess)
                                    {
                                        var logsToUpdate = tableGroup.Where(g => pendingStockTransactions.Any(s => s.Id == g.RecordId));
                                        foreach (var log in logsToUpdate)
                                        {
                                            log.SyncStatus = SyncStatus.Synced;
                                            log.LastSyncedAt = DateTime.UtcNow;
                                        }
                                        await context.SaveChangesAsync();
                                    }

                                    return new EntitySyncResult
                                    {
                                        EntityName = "StockTransactions",
                                        TotalRecords = pendingStockTransactions.Count,
                                        SuccessCount = result.IsSuccess ? pendingStockTransactions.Count : 0,
                                        FailureCount = result.IsSuccess ? 0 : pendingStockTransactions.Count,
                                        IsSuccess = result.IsSuccess,
                                        ErrorMessage = result.Error
                                    };
                                }
                                return new EntitySyncResult { EntityName = "StockTransactions", TotalRecords = 0, IsSuccess = true };
                            }, comprehensiveResult);
                            break;
                        case "suppliers":
                            // fresh DbContext

                            await SyncEntityBatch("Suppliers", async () =>
                            {
                                var pendingSuppliers = await context.Set<Supplier>()
                                    .Where(s => recordIds.Contains(s.Id))
                                    .ToListAsync();

                                if (pendingSuppliers.Any())
                                {
                                    var result = await SyncSuppliersAsync(pendingSuppliers, retailer);

                                    if (result.IsSuccess)
                                    {
                                        var logsToUpdate = tableGroup.Where(g => pendingSuppliers.Any(s => s.Id == g.RecordId));
                                        foreach (var log in logsToUpdate)
                                        {
                                            log.SyncStatus = SyncStatus.Synced;
                                            log.LastSyncedAt = DateTime.UtcNow;
                                        }
                                        await context.SaveChangesAsync();
                                    }

                                    return new EntitySyncResult
                                    {
                                        EntityName = "Suppliers",
                                        TotalRecords = pendingSuppliers.Count,
                                        SuccessCount = result.IsSuccess ? pendingSuppliers.Count : 0,
                                        FailureCount = result.IsSuccess ? 0 : pendingSuppliers.Count,
                                        IsSuccess = result.IsSuccess,
                                        ErrorMessage = result.Error
                                    };
                                }
                                return new EntitySyncResult { EntityName = "Suppliers", TotalRecords = 0, IsSuccess = true };
                            }, comprehensiveResult);
                            break;
                        case "tills":

                            await SyncEntityBatch("Tills", async () =>
                            {
                                var pendingTills = await context.Set<Till>()
                                    .Where(t => recordIds.Contains(t.Id))
                                    .ToListAsync();

                                if (pendingTills.Any())
                                {
                                    var result = await SyncTillsAsync(pendingTills, retailer);

                                    if (result.IsSuccess)
                                    {
                                        var logsToUpdate = tableGroup.Where(g => pendingTills.Any(t => t.Id == g.RecordId));
                                        foreach (var log in logsToUpdate)
                                        {
                                            log.SyncStatus = SyncStatus.Synced;
                                            log.LastSyncedAt = DateTime.UtcNow;
                                        }
                                        await context.SaveChangesAsync();
                                    }

                                    return new EntitySyncResult
                                    {
                                        EntityName = "Tills",
                                        TotalRecords = pendingTills.Count,
                                        SuccessCount = result.IsSuccess ? pendingTills.Count : 0,
                                        FailureCount = result.IsSuccess ? 0 : pendingTills.Count,
                                        IsSuccess = result.IsSuccess,
                                        ErrorMessage = result.Error
                                    };
                                }
                                return new EntitySyncResult { EntityName = "Tills", TotalRecords = 0, IsSuccess = true };
                            }, comprehensiveResult);
                            break;
                        case "unknownproducts":
                            await SyncEntityBatch("UnknownProducts", async () =>
                            {
                                var pendingUnknownProducts = await context.Set<Models.UnknownProduct>()
                                    .Where(up => recordIds.Contains(up.Id))
                                    .ToListAsync();

                                if (pendingUnknownProducts.Any())
                                {
                                    var result = await SyncUnknownProductsAsync(pendingUnknownProducts, retailer);

                                    if (result.IsSuccess)
                                    {
                                        var logsToUpdate = tableGroup.Where(g => pendingUnknownProducts.Any(up => up.Id == g.RecordId));
                                        foreach (var log in logsToUpdate)
                                        {
                                            log.SyncStatus = SyncStatus.Synced;
                                            log.LastSyncedAt = DateTime.UtcNow;
                                        }
                                        await context.SaveChangesAsync();
                                    }

                                    return new EntitySyncResult
                                    {
                                        EntityName = "UnknownProducts",
                                        TotalRecords = pendingUnknownProducts.Count,
                                        SuccessCount = result.IsSuccess ? pendingUnknownProducts.Count : 0,
                                        FailureCount = result.IsSuccess ? 0 : pendingUnknownProducts.Count,
                                        IsSuccess = result.IsSuccess,
                                        ErrorMessage = result.Error
                                    };
                                }
                                return new EntitySyncResult { EntityName = "UnknownProducts", TotalRecords = 0, IsSuccess = true };
                            }, comprehensiveResult);
                            break;
                        case "usersiteaccesses":
                            await SyncEntityBatch("UserSiteAccesses", async () =>
                            {
                                var pendingUserSiteAccesses = await context.Set<UserSiteAccess>()
                                    .Where(u => recordIds.Contains(u.Id))
                                    .ToListAsync();

                                if (pendingUserSiteAccesses.Any())
                                {
                                    var result = await SyncUserSiteAccessesAsync(pendingUserSiteAccesses, retailer);

                                    if (result.IsSuccess)
                                    {
                                        var logsToUpdate = tableGroup.Where(g => pendingUserSiteAccesses.Any(u => u.Id == g.RecordId));
                                        foreach (var log in logsToUpdate)
                                        {
                                            log.SyncStatus = SyncStatus.Synced;
                                            log.LastSyncedAt = DateTime.UtcNow;
                                        }
                                        await context.SaveChangesAsync();
                                    }

                                    return new EntitySyncResult
                                    {
                                        EntityName = "UserSiteAccesses",
                                        TotalRecords = pendingUserSiteAccesses.Count,
                                        SuccessCount = result.IsSuccess ? pendingUserSiteAccesses.Count : 0,
                                        FailureCount = result.IsSuccess ? 0 : pendingUserSiteAccesses.Count,
                                        IsSuccess = result.IsSuccess,
                                        ErrorMessage = result.Error
                                    };
                                }
                                return new EntitySyncResult { EntityName = "UserSiteAccesses", TotalRecords = 0, IsSuccess = true };
                            }, comprehensiveResult);
                            break;
                    }
                }



                // Calculate overall results
                comprehensiveResult.EndTime = DateTime.UtcNow;
                comprehensiveResult.Duration = comprehensiveResult.EndTime - comprehensiveResult.StartTime;
                comprehensiveResult.TotalRecordsSynced = comprehensiveResult.EntityResults.Values.Sum(r => r.SuccessCount);
                comprehensiveResult.TotalRecordsFailed = comprehensiveResult.EntityResults.Values.Sum(r => r.FailureCount);
                comprehensiveResult.IsSuccess = comprehensiveResult.EntityResults.Values.All(r => r.IsSuccess);

                _logger.LogInformation($"Comprehensive sync completed. Success: {comprehensiveResult.IsSuccess}, " +
                                     $"Synced: {comprehensiveResult.TotalRecordsSynced}, " +
                                     $"Failed: {comprehensiveResult.TotalRecordsFailed}, " +
                                     $"Duration: {comprehensiveResult.Duration.TotalSeconds:F2}s");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during comprehensive database sync");

                // Log error to database
                await _errorLogServices.LogErrorAsync(
                    ex.Message,
                    "Comprehensive Database Sync",
                    ErrorLogSeverity.Critical,
                    nameof(SyncDatabaseToCloudAsync),
                    nameof(SupabaseSyncService),
                    ex.StackTrace,
                    "Comprehensive database sync operation"
                );

                comprehensiveResult.IsSuccess = false;
                comprehensiveResult.ErrorMessage = ex.Message;
                comprehensiveResult.EndTime = DateTime.UtcNow;
                comprehensiveResult.Duration = comprehensiveResult.EndTime - comprehensiveResult.StartTime;
            }

            return comprehensiveResult;
        }

        /// <summary>
        /// Helper method to execute entity sync operations with error handling
        /// </summary>
        private async Task SyncEntityBatch(string entityName, Func<Task<EntitySyncResult>> syncOperation, ComprehensiveSyncResult comprehensiveResult)
        {
            try
            {
                _logger.LogInformation($"Starting sync for {entityName}");
                var entityResult = await syncOperation();
                comprehensiveResult.EntityResults[entityName] = entityResult;

                _logger.LogInformation($"Completed sync for {entityName}: Success={entityResult.IsSuccess}, " +
                                     $"Records={entityResult.TotalRecords}, " +
                                     $"Synced={entityResult.SuccessCount}, " +
                                     $"Failed={entityResult.FailureCount}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception during {entityName} sync");
                comprehensiveResult.EntityResults[entityName] = new EntitySyncResult
                {
                    EntityName = entityName,
                    IsSuccess = false,
                    ErrorMessage = ex.Message,
                    TotalRecords = 0,
                    SuccessCount = 0,
                    FailureCount = 0
                };
            }
        }

        private async Task<int> GetDepartmentID(string DepartmentName)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            if (_cachedDepartments == null || !_cachedDepartments.Any())
            {
                _cachedDepartments = await context.Departments.ToListAsync();
            }

            if (_cachedDepartments == null || !_cachedDepartments.Any())
            {
                _logger.LogWarning("No departments found in local database.");
                return 0;
            }
            var department = _cachedDepartments.FirstOrDefault(d => d.Department_Name.Equals(DepartmentName, StringComparison.OrdinalIgnoreCase));
            if (department == null)
            {
                department = _cachedDepartments.FirstOrDefault(d => d.Department_Name.Equals("Default", StringComparison.OrdinalIgnoreCase));
            }
            return department?.Id ?? 0;
        }

        private async Task<int> GetVATID(decimal VATRate)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            if (_cachedVats == null || !_cachedVats.Any())
            {
                _cachedVats = await context.Vats.ToListAsync();
            }
            if (_cachedVats == null || !_cachedVats.Any())
            {
                _logger.LogWarning("No VAT records found in local database.");
                return 0;
            }
            var vat = _cachedVats.FirstOrDefault(v => v.VAT_Value == VATRate);
            if (vat == null)
            {
                vat = _cachedVats.FirstOrDefault(v => v.VAT_Value == 0);
            }
            return vat?.Id ?? 0;
        }

        /// <summary>
        /// Logs sync operations to SupaSyncedLogs table for audit and tracking purposes
        /// </summary>
        /// <param name="tableName">Name of the table that was synced</param>
        /// <param name="recordIds">List of record IDs that were synced</param>
        /// <param name="syncStatus">Status of the sync operation</param>
        /// <param name="syncLocation">Location where sync was performed (Central/Local)</param>
        /// <param name="retailer">The retailer performing the sync</param>
        /// <param name="modifiedBy">User ID who performed the sync operation</param>
        /// <returns>SyncResult indicating success or failure of logging operation</returns>
        public async Task<SyncResult<bool>> LogSyncOperationAsync(
            string tableName,
            List<SupaSyncedLogs> syncLogs,
            Retailer retailer
            )
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tableName))
                {
                    return new SyncResult<bool>
                    {
                        IsSuccess = false,
                        Error = "Table name cannot be null or empty",
                        Message = "Invalid table name provided"
                    };
                }

                if (retailer == null)
                {
                    return new SyncResult<bool>
                    {
                        IsSuccess = false,
                        Error = "Retailer cannot be null",
                        Message = "Valid retailer required for sync logging"
                    };
                }
                if (syncLogs.Any() == false)
                {
                    return new SyncResult<bool>
                    {
                        IsSuccess = true,
                        Data = true,
                        Message = "No sync logs to process"
                    };
                }
                var logResult = await UpsertAsync(_userSessionService.CurrentRetailer, "SyncedLogs", syncLogs, "Supa_Id", retailer.RetailerId);

                if (logResult.IsSuccess)
                {
                    _logger.LogInformation($"Successfully logged sync operation for table {tableName} with {syncLogs.Count} records");
                    return new SyncResult<bool>
                    {
                        IsSuccess = true,
                        Data = true,
                        Message = $"Sync operation logged successfully for table {tableName}"
                    };
                }
                else
                {
                    _logger.LogError($"Failed to log sync operation for table {tableName}: {logResult.Error}");
                    return new SyncResult<bool>
                    {
                        IsSuccess = false,
                        Error = logResult.Error,
                        Message = $"Failed to log sync operation for table {tableName}"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception while logging sync operation for table {tableName}");
                return new SyncResult<bool>
                {
                    IsSuccess = false,
                    Error = ex.Message,
                    Message = $"Exception occurred while logging sync operation for table {tableName}"
                };
            }
        }

        /// <summary>
        /// Logs multiple sync operations in batch for better performance
        /// </summary>
        /// <param name="syncLogs">List of sync log entries to insert</param>
        /// <param name="retailer">The retailer performing the sync</param>
        /// <returns>SyncResult indicating success or failure of batch logging operation</returns>
        public async Task<SyncResult<int>> LogSyncOperationsBatchAsync(
            List<SupaSyncedLogs> syncLogs,
            Retailer retailer)
        {
            try
            {
                if (syncLogs == null || !syncLogs.Any())
                {
                    return new SyncResult<int>
                    {
                        IsSuccess = true,
                        Data = 0,
                        Message = "No sync logs to process"
                    };
                }

                if (retailer == null)
                {
                    return new SyncResult<int>
                    {
                        IsSuccess = false,
                        Error = "Retailer cannot be null",
                        Message = "Valid retailer required for batch sync logging"
                    };
                }

                // Validate and set default values for sync logs
                foreach (var syncLog in syncLogs)
                {
                    if (syncLog.DateCreated == default)
                        syncLog.DateCreated = DateTime.UtcNow;

                    if (syncLog.LastModified == default)
                        syncLog.LastModified = DateTime.UtcNow;

                    if (syncLog.RetailerId == default)
                        syncLog.RetailerId = retailer.RetailerId;

                    if (syncLog.JsonRecordIds == null)
                        syncLog.JsonRecordIds = JsonSerializer.Serialize(new RecordsIdModel { RecordIds = new List<long>() });
                }

                var batchResult = await UpsertListAsync("SyncedLogs", syncLogs, "Supa_Id", retailer);

                if (batchResult.IsSuccess)
                {
                    _logger.LogInformation($"Successfully logged {syncLogs.Count} sync operations in batch");
                    return new SyncResult<int>
                    {
                        IsSuccess = true,
                        Data = syncLogs.Count,
                        Message = $"Successfully logged {syncLogs.Count} sync operations"
                    };
                }
                else
                {
                    _logger.LogError($"Failed to log batch sync operations: {batchResult.Error}");
                    return new SyncResult<int>
                    {
                        IsSuccess = false,
                        Error = batchResult.Error,
                        Message = "Failed to log batch sync operations"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while logging batch sync operations");
                return new SyncResult<int>
                {
                    IsSuccess = false,
                    Error = ex.Message,
                    Message = "Exception occurred while logging batch sync operations"
                };
            }
        }

        /// <summary>
        /// Helper method to create a sync log entry
        /// </summary>
        /// <param name="tableName">Name of the table</param>
        /// <param name="recordIds">List of record IDs</param>
        /// <param name="syncStatus">Sync status</param>
        /// <param name="syncLocation">Sync location</param>
        /// <param name="retailerId">Retailer ID</param>
        /// <param name="modifiedBy">User who performed the operation</param>
        /// <returns>SupaSyncedLogs object</returns>
        public SupaSyncedLogs CreateSyncLogEntry(
            string tableName,
            List<long> recordIds,
            SyncStatus syncStatus,
            SyncLocation syncLocation,
            Operation operation,
            Guid retailerId
           )
        {
            return new SupaSyncedLogs
            {
                DateCreated = DateTime.UtcNow,
                RetailerId = retailerId,
                TableName = tableName,
                SyncStatus = syncStatus,
                LastModified = DateTime.UtcNow,
                ModifiedBy = retailerId,
                Operation = operation,
                JsonRecordIds = Newtonsoft.Json.JsonConvert.SerializeObject(new RecordsIdModel
                {
                    RecordIds = recordIds ?? new List<long>()
                }),
                SyncLocation = syncLocation
            };
        }

        public async Task<Retailer> CheckIfRetailerAccessTokenNeedsRefreshedAsync(Retailer pRetailer)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            // Refresh token if needed
            Retailer retailer = pRetailer ?? throw new ArgumentNullException(nameof(pRetailer));
            bool tokenRefreshed = await RefreshTokenIfNeededAsync(retailer);

            if (tokenRefreshed)
            {
                // Get the updated retailer information after token refresh
                retailer = await context.Set<Retailer>()
                    .FirstOrDefaultAsync(r => r.RetailerId == retailer.RetailerId)
                    ?? throw new ArgumentException($"Retailer with ID {retailer.RetailerId} not found");
                _userSessionService.SetRetailer(retailer);
            }
            return retailer;
        }

        // Convenience fetch methods for cloud data
        public async Task<SyncResult<List<Models.SupabaseModels.SupaGlobalProducts>>> GetGlobalProducts(Retailer retailer, string whereClause = "", string selectColumns = "*")
        {
            // Table: SupaProducts (global catalog)
            return await GetAsync<Models.SupabaseModels.SupaGlobalProducts>(retailer, "GlobalProducts", selectColumns, whereClause);
        }

        public async Task<SyncResult<List<Models.SupabaseModels.SupaGlobalProducts>>> GetGlobalProducts(Guid retailerId, string whereClause = "", string selectColumns = "*")
        {
            // Table: SupaProducts (global catalog)
            return await GetAsync<Models.SupabaseModels.SupaGlobalProducts>(retailer: null, tableName: "GlobalProducts", selectColumns: selectColumns, whereClause: whereClause, retailerId: retailerId);
        }

        public async Task<SyncResult<List<Models.SupabaseModels.SupaDepartments>>> GetDepartments(Retailer retailer, string whereClause = "", string selectColumns = "*")
        {
            // Table: SupaDepartments (retailer-scoped)
            return await GetAsync<Models.SupabaseModels.SupaDepartments>(retailer, "Departments", selectColumns, whereClause);
        }

        public async Task<SyncResult<List<Models.SupabaseModels.SupaDepartments>>> GetDepartments(Guid retailerId, string whereClause = "", string selectColumns = "*")
        {
            // Table: SupaDepartments (retailer-scoped)
            return await GetAsync<Models.SupabaseModels.SupaDepartments>(retailer: null, tableName: "Departments", selectColumns: selectColumns, whereClause: whereClause, retailerId: retailerId);
        }

        public async Task<SyncResult<List<Models.SupabaseModels.SupaVats>>> GetVats(Retailer retailer, string whereClause = "", string selectColumns = "*")
        {
            // Table: SupaVats (global VATs)
            return await GetAsync<Models.SupabaseModels.SupaVats>(retailer, "Vats", selectColumns, whereClause);
        }

        public async Task<SyncResult<List<Models.SupabaseModels.SupaVats>>> GetVats(Guid retailerId, string whereClause = "", string selectColumns = "*")
        {
            // Table: SupaVats (global VATs)
            return await GetAsync<Models.SupabaseModels.SupaVats>(retailer: null, tableName: "Vats", selectColumns: selectColumns, whereClause: whereClause, retailerId: retailerId);
        }
    }
}

public class AuthResult
{
    public bool IsSuccess { get; set; }
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public int ExpiresIn { get; set; }
    public int ExpiresAt { get; set; }
    public string UserId { get; set; }
    public string Email { get; set; }
    public string ErrorMessage { get; set; }
}

public class ComprehensiveSyncResult
{
    public bool IsSuccess { get; set; }
    public Guid RetailerId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public TimeSpan Duration { get; set; }
    public int TotalRecordsSynced { get; set; }
    public int TotalRecordsFailed { get; set; }
    public string ErrorMessage { get; set; }
    public Dictionary<string, EntitySyncResult> EntityResults { get; set; }
}

public class EntitySyncResult
{
    public string EntityName { get; set; }
    public bool IsSuccess { get; set; }
    public int TotalRecords { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public string ErrorMessage { get; set; }
}

public class NullableDateTimeConverter : JsonConverter<DateTime?>
{
    public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        if (reader.TokenType == JsonTokenType.String)
        {
            var s = reader.GetString();
            if (string.IsNullOrEmpty(s)) return null;
            if (DateTime.TryParse(s, out var parsed)) return parsed;
            var fmts = new[]
            {
                "yyyy-MM-dd'T'HH:mm:ss.FFFFFF",
                "yyyy-MM-dd'T'HH:mm:ss",
                "yyyy-MM-dd'T'HH:mm:ss.FFFFFF'Z'",
                "yyyy-MM-dd'T'HH:mm:ss'Z'",
                "yyyy-MM-dd HH:mm:ss.FFFFFF",
                "yyyy-MM-dd HH:mm:ss"
            };
            if (DateTime.TryParseExact(s, fmts, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AssumeUniversal, out var parsedExact)) return parsedExact;
            return null;
        }

        try { return JsonSerializer.Deserialize<DateTime?>(ref reader, options); } catch { return null; }
    }

    public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
        {
            writer.WriteStringValue(value.Value.ToUniversalTime().ToString("O"));
        }
        else
        {
            writer.WriteNullValue();
        }
    }
}

public class SafeDateTimeConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return default;
        }

        if (reader.TokenType == JsonTokenType.String)
        {
            var s = reader.GetString();
            if (string.IsNullOrEmpty(s)) return default;
            if (string.Equals(s, "infinity", StringComparison.OrdinalIgnoreCase)) return DateTime.MaxValue;
            if (string.Equals(s, "-infinity", StringComparison.OrdinalIgnoreCase)) return DateTime.MinValue;

            if (DateTimeOffset.TryParse(s, out var dto))
            {
                var dt = dto.UtcDateTime;
                if (dt.Year > 9999) return DateTime.MaxValue;
                if (dt.Year < 1) return DateTime.MinValue;
                return dt;
            }

            if (DateTime.TryParse(s, out var parsed))
            {
                if (parsed.Year > 9999) return DateTime.MaxValue;
                if (parsed.Year < 1) return DateTime.MinValue;
                return DateTime.SpecifyKind(parsed, DateTimeKind.Utc);
            }

            return default;
        }

        try
        {
            var value = JsonSerializer.Deserialize<DateTime>(ref reader, options);
            if (value.Year > 9999) return DateTime.MaxValue;
            if (value.Year < 1) return DateTime.MinValue;
            return value;
        }
        catch
        {
            return default;
        }
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToUniversalTime().ToString("O"));
    }
}





