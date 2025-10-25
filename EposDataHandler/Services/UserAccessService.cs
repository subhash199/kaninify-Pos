using EntityFrameworkDatabaseLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EntityFrameworkDatabaseLibrary.Services
{
    public class UserAccessService
    {
        public static List<Site> GetUserAccessibleSites(PosUser user)
        {
            var accessibleSites = new List<Site>();

            // Director has access to all sites
            //if (user.UserRole.Can_Access_All_Sites)
            //{
            //    // Return all sites from database
            //    // This would be implemented with your DbContext
            //    return GetAllSites();
            //}

            //// Staff and Manager - get specific site access
            //accessibleSites.AddRange(user.
            //    .Where(sa => sa.Is_Active)
            //    .Select(sa => sa.Site)
            //    .ToList());

            // Include primary site if not already included
            if (user.PrimarySite != null && !accessibleSites.Any(s => s.Id == user.Primary_Site_Id))
            {
                accessibleSites.Add(user.PrimarySite);
            }

            return accessibleSites;
        }

        //public static bool CanUserAccessSite(PosUser user, int siteId)
        //{
        //    // Director can access all sites
        //    if (user.UserRole.Can_Access_All_Sites)
        //        return true;

        //    // Check if user has specific access to this site
        //    //return user.SiteAccesses.Any(sa => sa.Site_ID == siteId && sa.Is_Active) ||
        //    //       user.Primary_Site_Id == siteId;
        //}

        public static void GrantSiteAccess(int userId, int siteId, bool canManage = false)
        {
            // Implementation to add UserSiteAccess record
            // This would use your DbContext to add the relationship
        }

        public static void RevokeSiteAccess(int userId, int siteId)
        {
            // Implementation to remove or deactivate UserSiteAccess record
        }

        private static List<Site> GetAllSites()
        {
            // This would be implemented with your DbContext
            // Return all sites from database
            return new List<Site>();
        }
    }
}