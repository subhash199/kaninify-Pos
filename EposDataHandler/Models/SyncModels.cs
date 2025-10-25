using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EposDataHandler.Models
{
    /// <summary>
    /// Generic result wrapper for sync operations
    /// </summary>
    public class SyncResult<T>
    {
        public bool IsSuccess { get; set; }
        public T? Data { get; set; }
        public string? Error { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public int? StatusCode { get; set; }
        public Dictionary<string, object>? Metadata { get; set; }
    }
    
    /// <summary>
    /// Conflict resolution strategies
    /// </summary>
    public enum ConflictResolutionStrategy
    {
        LocalWins,
        RemoteWins,
        MostRecent,
        Manual,
        Merge
    }
    
    /// <summary>
    /// Represents a data conflict during sync
    /// </summary>
    public class ConflictItem
    {
        public string TableName { get; set; } = string.Empty;
        public string RecordId { get; set; } = string.Empty;
        public object? LocalData { get; set; }
        public object? RemoteData { get; set; }
        public DateTime LocalLastModified { get; set; }
        public DateTime RemoteLastModified { get; set; }
        public ConflictResolutionStrategy RecommendedStrategy { get; set; }
        public string ConflictReason { get; set; } = string.Empty;
    }
    
    /// <summary>
    /// Sync operation status
    /// </summary>
    public enum SyncOperationType
    {
        Insert,
        Update,
        Delete,
        Upsert,
        BatchInsert,
        BatchUpdate
    }
    
    /// <summary>
    /// Detailed sync operation result
    /// </summary>
    public class SyncOperationResult
    {
        public Guid OperationId { get; set; } = Guid.NewGuid();
        public SyncOperationType OperationType { get; set; }
        public string TableName { get; set; } = string.Empty;
        public Guid RetailerId { get; set; }
        public bool IsSuccess { get; set; }
        public int RecordsProcessed { get; set; }
        public int RecordsSucceeded { get; set; }
        public int RecordsFailed { get; set; }
        public List<ConflictItem> Conflicts { get; set; } = new();
        public List<string> Errors { get; set; } = new();
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan Duration => EndTime - StartTime;
        public Dictionary<string, object>? Metadata { get; set; }
    }
    
    /// <summary>
    /// Sync configuration settings
    /// </summary>
    public class SyncConfiguration
    {
        public int BatchSize { get; set; } = 100;
        public int MaxRetryAttempts { get; set; } = 3;
        public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(5);
        public ConflictResolutionStrategy DefaultConflictResolution { get; set; } = ConflictResolutionStrategy.MostRecent;
        public bool EnableBatchOperations { get; set; } = true;
        public bool EnableConflictDetection { get; set; } = true;
        public List<string> SyncTables { get; set; } = new();
        public Dictionary<string, string> TableMappings { get; set; } = new();
    }
    
    /// <summary>
    /// Sync session information
    /// </summary>
    public class SyncSession
    {
        public Guid SessionId { get; set; } = Guid.NewGuid();
        public Guid RetailerId { get; set; }
        public DateTime StartTime { get; set; } = DateTime.UtcNow;
        public DateTime? EndTime { get; set; }
        public bool IsActive { get; set; } = true;
        public List<SyncOperationResult> Operations { get; set; } = new();
        public SyncConfiguration Configuration { get; set; } = new();
        
        public TimeSpan? Duration => EndTime.HasValue ? EndTime.Value - StartTime : null;
        public int TotalRecordsProcessed => Operations.Sum(o => o.RecordsProcessed);
        public int TotalRecordsSucceeded => Operations.Sum(o => o.RecordsSucceeded);
        public int TotalRecordsFailed => Operations.Sum(o => o.RecordsFailed);
        public bool HasConflicts => Operations.Any(o => o.Conflicts.Count > 0);
    }
    
    /// <summary>
    /// Change tracking for sync operations
    /// </summary>
    public class ChangeTrackingInfo
    {
        public string TableName { get; set; } = string.Empty;
        public string RecordId { get; set; } = string.Empty;
        public SyncOperationType ChangeType { get; set; }
        public DateTime ChangeTimestamp { get; set; }
        public string? ChangeData { get; set; }
        public Guid RetailerId { get; set; }
        public bool IsSynced { get; set; }
        public DateTime? LastSyncAttempt { get; set; }
        public int SyncAttempts { get; set; }
        public string? LastSyncError { get; set; }
    }
    
    /// <summary>
    /// Sync statistics and metrics
    /// </summary>
    public class SyncStatistics
    {
        public Guid RetailerId { get; set; }
        public DateTime LastFullSync { get; set; }
        public DateTime LastIncrementalSync { get; set; }
        public int TotalSyncSessions { get; set; }
        public int SuccessfulSyncSessions { get; set; }
        public int FailedSyncSessions { get; set; }
        public long TotalRecordsSynced { get; set; }
        public long TotalConflictsResolved { get; set; }
        public TimeSpan AverageSyncDuration { get; set; }
        public Dictionary<string, int> TableSyncCounts { get; set; } = new();
        public List<string> RecentErrors { get; set; } = new();
    }
    
    /// <summary>
    /// Authentication token response from Supabase
    /// </summary>
    public class AuthTokenResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public string TokenType { get; set; } = "bearer";
        public int ExpiresIn { get; set; }
        public DateTime ExpiresAt { get; set; }
        public UserInfo? User { get; set; }
    }
    
    /// <summary>
    /// User information from authentication response
    /// </summary>
    public class UserInfo
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public Dictionary<string, object>? UserMetadata { get; set; }
        public Dictionary<string, object>? AppMetadata { get; set; }
    }
}