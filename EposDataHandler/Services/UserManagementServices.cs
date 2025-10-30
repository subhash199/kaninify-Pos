using EntityFrameworkDatabaseLibrary.Data;
using EntityFrameworkDatabaseLibrary.Models;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using DataHandlerLibrary.Interfaces;
using DataHandlerLibrary.Models;

namespace DataHandlerLibrary.Services
{
    public class UserManagementServices
    {
        private readonly IDbContextFactory<DatabaseInitialization> _dbFactory;
        private readonly PosUserServices _userServices;
        private readonly UserSiteAccessServices _siteAccessServices;
        private readonly SiteServices _siteServices;
        
        public UserManagementServices(
            IDbContextFactory<DatabaseInitialization> dbFactory,
            PosUserServices userServices,
            UserSiteAccessServices siteAccessServices,
            SiteServices siteServices)
        {
            _dbFactory = dbFactory;
            _userServices = userServices;
            _siteAccessServices = siteAccessServices;
            _siteServices = siteServices;
        }

        #region User Management Operations

        /// <summary>
        /// Get all users with their related data (roles, sites, access)
        /// </summary>
        public async Task<List<PosUser>> GetAllUsersAsync()
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            var users = await context.PosUsers.AsNoTracking()
                    .Include(u => u.PrimarySite)
                    .Include(u => u.SiteAccesses)
                        .ThenInclude(sa => sa.Site)
                    .OrderBy(u => u.Last_Name)
                    .ThenBy(u => u.First_Name)
                    .ToListAsync();
        
                // Load Created_By and Last_Modified_By separately for each user
                foreach (var user in users)
                {
                    if (user.Created_By_Id > 0)
                    {
                        user.Created_By = await context.PosUsers.AsNoTracking()
                            .FirstOrDefaultAsync(u => u.Id == user.Created_By_Id);
                    }
        
                    if (user.Last_Modified_By_Id > 0)
                    {
                        user.Last_Modified_By = await context.PosUsers.AsNoTracking()
                            .FirstOrDefaultAsync(u => u.Id == user.Last_Modified_By_Id);
                    }
                }
        
               return users;
            
            
              
            
        }

        /// <summary>
        /// Get user with all related data (roles, sites, access)
        /// </summary>
        public async Task<PosUser> GetUserWithFullDetailsAsync(int userId)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            var user = await context.PosUsers.AsNoTracking()
                .Include(u => u.PrimarySite)
                .Include(u => u.SiteAccesses)
                    .ThenInclude(sa => sa.Site)
                .Include(u => u.Site)
                .Include(u => u.Till)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user != null)
            {
                // Load Created_By user separately if needed
                if (user.Created_By_Id > 0)
                {
                    user.Created_By = await context.PosUsers.AsNoTracking()
                        .FirstOrDefaultAsync(u => u.Id == user.Created_By_Id);
                }

                // Load Last_Modified_By user separately if needed
                if (user.Last_Modified_By_Id > 0)
                {
                    user.Last_Modified_By = await context.PosUsers.AsNoTracking()
                        .FirstOrDefaultAsync(u => u.Id == user.Last_Modified_By_Id);
                }
            }

            return user;
        }

        /// <summary>
        /// Get all users for a specific site
        /// </summary>
        public async Task<IEnumerable<PosUser>> GetUsersForSiteAsync(int siteId)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            return await context.PosUsers.AsNoTracking()
                .Include(u => u.PrimarySite)
                .Include(u => u.SiteAccesses)
                .Where(u => u.Primary_Site_Id == siteId || 
                           u.SiteAccesses.Any(sa => sa.Site_Id == siteId && sa.Is_Active))
                .ToListAsync();
        }

        /// <summary>
        /// Get all users with a specific role
        /// </summary>
        public async Task<IEnumerable<PosUser>> GetUsersByRoleAsync(PosUserType userType)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            return await context.PosUsers.AsNoTracking()
                .Include(u => u.PrimarySite)
                .Where(u => u.User_Type == userType && u.Is_Activated && !u.Is_Deleted)
                .ToListAsync();
        }

        #endregion

        #region Site Access Management

        /// <summary>
        /// Assign user to a site with specific access
        /// </summary>
        public async Task<bool> AssignUserToSiteAsync(int userId, int siteId, int createdBy)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            try
            {
                // Check if user already has access to this site
                var existingAccess = await context.UserSiteAccesses
                    .FirstOrDefaultAsync(usa => usa.User_Id == userId && usa.Site_Id == siteId);

                if (existingAccess != null)
                {
                    if (!existingAccess.Is_Active)
                    {
                        existingAccess.Is_Active = true;
                        existingAccess.Date_Granted = DateTime.UtcNow;
                        existingAccess.Last_Modified = DateTime.UtcNow;
                        existingAccess.Last_Modified_By_Id = createdBy;
                        await context.SaveChangesAsync();
                    }
                    return true;
                }

                var newAccess = new UserSiteAccess
                {
                    User_Id = userId,
                    Site_Id = siteId,
                    Date_Granted = DateTime.UtcNow,
                    Is_Active = true,
                    Date_Created = DateTime.UtcNow,
                    Last_Modified = DateTime.UtcNow,
                    Created_By_Id = createdBy,
                    Last_Modified_By_Id = createdBy
                };

                await _siteAccessServices.AddAsync(newAccess);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Remove user access from a site
        /// </summary>
        public async Task<bool> RemoveUserFromSiteAsync(int userId, int siteId, int modifiedBy)
        {
            try
            {
                using var context = _dbFactory.CreateDbContext(); // fresh DbContext

                var access = await context.UserSiteAccesses
                    .FirstOrDefaultAsync(usa => usa.User_Id == userId && usa.Site_Id == siteId);

                if (access != null)
                {
                    access.Is_Active = false;
                    access.Date_Revoked = DateTime.UtcNow;
                    access.Last_Modified = DateTime.UtcNow;
                    access.Last_Modified_By_Id = modifiedBy;
                    await context.SaveChangesAsync();
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Get all sites a user has access to
        /// </summary>
        public async Task<IEnumerable<Site>> GetUserAccessibleSitesAsync(int userId)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            return await context.Sites.AsNoTracking()
                .Where(s => s.UserAccesses.Any(ua => ua.User_Id == userId && ua.Is_Active) ||
                           s.PrimaryUsers.Any(u => u.Id == userId))
                .ToListAsync();
        }

        /// <summary>
        /// Validate if user has access to a specific site
        /// </summary>
        public async Task<bool> ValidateUserAccessAsync(int userId, int siteId)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            var user = await context.PosUsers.AsNoTracking()
                .Include(u => u.SiteAccesses)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null || !user.Is_Activated || user.Is_Deleted)
                return false;

            // Check if it's their primary site
            if (user.Primary_Site_Id == siteId)
                return true;

            // Check if they have active access to the site
            return user.SiteAccesses.Any(sa => sa.Site_Id == siteId && sa.Is_Active);
        }

        #endregion

        #region Role Management

        /// <summary>
        /// Change user's role
        /// </summary>
        public async Task<bool> ChangeUserRoleAsync(int userId, PosUserType userType, int modifiedBy)
        {
            try
            {
                using var context = _dbFactory.CreateDbContext(); // fresh DbContext

                var user = await context.PosUsers.FindAsync(userId);
                if (user == null) return false;

                user.User_Type = userType;
                user.Last_Modified = DateTime.UtcNow;
                user.Last_Modified_By_Id = modifiedBy;

                await context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Site Management

        /// <summary>
        /// Get all sites
        /// </summary>
        public async Task<List<Site>> GetAllSitesAsync()
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            return await context.Sites.AsNoTracking()
                .Where(s => !s.Is_Deleted)
                .OrderBy(s => s.Site_BusinessName)
                .ToListAsync();
        }

        /// <summary>
        /// Create a new site
        /// </summary>
        public async Task CreateSiteAsync(Site site)
        {
            site.Date_Created = DateTime.UtcNow;
            site.Last_Modified = DateTime.UtcNow;
            site.Is_Deleted = false;
            site.Is_Active = true;
            
            await _siteServices.AddAsync(site);
        }

        /// <summary>
        /// Update an existing site
        /// </summary>
        public async Task UpdateSiteAsync(Site site)
        {
            site.Last_Modified = DateTime.UtcNow;
            await _siteServices.UpdateAsync(site);
        }

        /// <summary>
        /// Activate a site
        /// </summary>
        public async Task ActivateSiteAsync(int siteId)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            var site = await context.Sites.FindAsync(siteId);
            if (site != null)
            {
                site.Is_Active = true;
                site.Last_Modified = DateTime.UtcNow;
                await context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Deactivate a site
        /// </summary>
        public async Task DeactivateSiteAsync(int siteId)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            var site = await context.Sites.FindAsync(siteId);
            if (site != null)
            {
                site.Is_Active = false;
                site.Last_Modified = DateTime.UtcNow;
                await context.SaveChangesAsync();
            }
        }

        #endregion

        #region User Creation and Management

        /// <summary>
        /// Create a new user with role and site assignment
        /// </summary>
        /// 
        
        public async Task<string> CreateUserAsync(PosUser user)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            using var transaction = await context.Database.BeginTransactionAsync();
            try
            {                
                await _userServices.AddAsync(user);                
                await transaction.CommitAsync();
                return string.Empty;
            }
            catch(Exception ex)
            {              
                await transaction.RollbackAsync();
                return $"Error creating user: {ex.InnerException}";
            }
            
        }

        /// <summary>
        /// Update an existing user
        /// </summary>
        public async Task<string> UpdateUserAsync(PosUser user)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                var existingUser = await context.PosUsers.FindAsync(user.Id);
                if (existingUser == null)
                {
                    await transaction.RollbackAsync();
                    return "User not found";
                }

                // Copy all properties from user to existingUser in one line
                context.Entry(existingUser).CurrentValues.SetValues(user);
                
                // Update audit fields
                existingUser.Last_Modified = DateTime.UtcNow;               

                await _userServices.UpdateAsync(existingUser);
                await transaction.CommitAsync();
                return string.Empty;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return $"Error updating user: {ex.InnerException}";
            }
        }
        public async Task<bool> CreateUserWithAccessAsync(
            PosUser user, 
            PosUserType userType, 
            int primarySiteId, int createdBy,
            List<int> additionalSiteIds = null)            
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                // Set user properties
                user.User_Type = userType;
                user.Primary_Site_Id = primarySiteId;
                user.Date_Created = DateTime.UtcNow;
                user.Last_Modified = DateTime.UtcNow;
                user.Created_By_Id = createdBy;
                user.Last_Modified_By_Id = createdBy;
                user.Is_Activated = true;
                user.Is_Deleted = false;

                await _userServices.AddAsync(user);

                // Add additional site access if specified
                if (additionalSiteIds != null && additionalSiteIds.Any())
                {
                    foreach (var siteId in additionalSiteIds.Where(id => id != primarySiteId))
                    {
                        await AssignUserToSiteAsync(user.Id, siteId, createdBy);
                    }
                }

                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

        /// <summary>
        /// Deactivate user and revoke all site access
        /// </summary>
        public async Task<bool> DeactivateUserAsync(int userId, int modifiedBy)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                var user = await context.PosUsers.FindAsync(userId);
                if (user == null) return false;

                // Deactivate user
                user.Is_Activated = false;
                user.Last_Modified = DateTime.UtcNow;
                user.Last_Modified_By_Id = modifiedBy;

                // Revoke all site access
                var userAccesses = await context.UserSiteAccesses
                    .Where(usa => usa.User_Id == userId && usa.Is_Active)
                    .ToListAsync();

                foreach (var access in userAccesses)
                {
                    access.Is_Active = false;
                    access.Date_Revoked = DateTime.UtcNow;
                    access.Last_Modified = DateTime.UtcNow;
                    access.Last_Modified_By_Id = modifiedBy;
                }

                await context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

        /// <summary>
        /// Reactivate user
        /// </summary>
        public async Task<bool> ReactivateUserAsync(int userId, int modifiedBy)
        {
            try
            {
                using var context = _dbFactory.CreateDbContext(); // fresh DbContext

                var user = await context.PosUsers.FindAsync(userId);
                if (user == null) return false;

                user.Is_Activated = true;
                user.Last_Modified = DateTime.UtcNow;
                user.Last_Modified_By_Id = modifiedBy;

                await context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Reporting and Analytics

        /// <summary>
        /// Get user access summary for reporting
        /// </summary>
        public async Task<object> GetUserAccessSummaryAsync(int userId)
        {
            var user = await GetUserWithFullDetailsAsync(userId);
            if (user == null) return null;

            return new
            {
                User = new
                {
                    user.Id,
                    user.First_Name,
                    user.Last_Name,
                    user.Is_Activated,
                    user.Date_Created
                },
                UserType = user.User_Type,
                PrimarySite = user.PrimarySite?.Site_AddressLine1,
                AdditionalSites = user.SiteAccesses
                    .Where(sa => sa.Is_Active && sa.Site_Id != user.Primary_Site_Id)
                    .Select(sa => new
                    {
                        sa.Site.Site_AddressLine1,
                        sa.Date_Granted,
                        sa.Is_Active
                    })
                    .ToList(),
                TotalSiteAccess = user.SiteAccesses.Count(sa => sa.Is_Active) + 1 // +1 for primary site
            };
        }

        /// <summary>
        /// Get site access statistics
        /// </summary>
        public async Task<object> GetSiteAccessStatisticsAsync(int siteId)
        {
            using var context = _dbFactory.CreateDbContext(); // fresh DbContext

            var site = await context.Sites.AsNoTracking()
                .Include(s => s.UserAccesses)
                    .ThenInclude(ua => ua.User)
                .Include(s => s.PrimaryUsers)
                .FirstOrDefaultAsync(s => s.Id == siteId);

            if (site == null) return null;

            return new
            {
                Site = new
                {
                    site.Id,
                    site.Site_BusinessName,
                },
                PrimaryUsers = site.PrimaryUsers.Count(u => u.Is_Activated && !u.Is_Deleted),
                AdditionalUsers = site.UserAccesses.Count(ua => ua.Is_Active),
                TotalActiveUsers = site.PrimaryUsers.Count(u => u.Is_Activated && !u.Is_Deleted) +
                                 site.UserAccesses.Count(ua => ua.Is_Active),
                UsersByRole = site.PrimaryUsers
                    .Where(u => u.Is_Activated && !u.Is_Deleted)
                    .GroupBy(u => u.User_Type)
                    .Select(g => new { Role = g.Key, Count = g.Count() })
                    .ToList()
            };
        }

        #endregion
    }
}