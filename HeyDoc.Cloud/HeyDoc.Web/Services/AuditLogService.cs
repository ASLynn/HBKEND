using HeyDoc.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace HeyDoc.Web.Services
{
    public class AuditLogService
    {
        public static List<AuditLogModel> GetAuditLogList(int skip, int take, int sortParam, string sortOrder, string searchTerm, out int recordsFiltered, out int recordsTotal)
        {
            var result = new List<AuditLogModel>();
            using (var db = new Entity.db_HeyDocEntities())
            {
                db.Database.CommandTimeout = 60;

                var auditLogs = db.AuditLogs
                    .Include(e => e.UserProfile)
                    .AsQueryable();

                recordsTotal = auditLogs.Count();
               
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    auditLogs = auditLogs.Where(e => e.UserProfile.FullName.Contains(searchTerm) || e.Description.Contains(searchTerm) || e.RelatedId.Contains(searchTerm));
                    recordsFiltered = auditLogs.Count();
                }
                else
                {
                    recordsFiltered = recordsTotal;
                }

                switch (sortOrder)
                {
                    case "asc":
                        switch (sortParam)
                        {
                            case 1:
                                auditLogs = auditLogs.OrderBy(e => e.UserProfile.FullName);
                                break;
                            case 2:
                                auditLogs = auditLogs.OrderBy(e => e.LogType);
                                break;
                            case 4:
                                auditLogs = auditLogs.OrderBy(e => e.CreateDate);
                                break;

                            default:
                                auditLogs = auditLogs.OrderBy(e => e.CreateDate);
                                break;
                        }
                        break;
                    case "desc":
                        switch (sortParam)
                        {
                            case 1:
                                auditLogs = auditLogs.OrderByDescending(e => e.UserProfile.FullName);
                                break;
                            case 2:
                                auditLogs = auditLogs.OrderByDescending(e => e.LogType);
                                break;
                            case 4:
                                auditLogs = auditLogs.OrderByDescending(e => e.CreateDate);
                                break;

                            default:
                                auditLogs = auditLogs.OrderByDescending(e => e.CreateDate);
                                break;
                        }
                        break;
                }

                auditLogs = auditLogs.Skip(skip).Take(take);

                foreach (var auditLog in auditLogs)
                {
                    result.Add(new AuditLogModel(auditLog));
                }
                return result;
            }
        }
    }
}