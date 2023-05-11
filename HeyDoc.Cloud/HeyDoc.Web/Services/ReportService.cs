using HeyDoc.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Globalization;
using HeyDoc.Web.Helpers;
using System.Data.Entity.SqlServer;
using System.Threading.Tasks;

namespace HeyDoc.Web.Services
{
    public class ReportService
    {
        public static IEnumerable<PropertyCountsReportModel<int>> GetMonthlyCorporatePrescriptionCounts(Entity.db_HeyDocEntities db, DateTime fromMonth, DateTime toMonth)
        {
            if (toMonth < fromMonth)
            {
                throw new Exception("Invalid date values");
            }

            SortedDictionary<string, List<int>> resultDict = new SortedDictionary<string, List<int>>();
            var totalMonths = (toMonth.Year - fromMonth.Year) * 12 + (toMonth.Month - fromMonth.Month + 1);

            var startPeriod = new DateTime(fromMonth.Year, fromMonth.Month, 1);
            var endPeriod = new DateTime(toMonth.Year + (toMonth.Month == 12 ? 1 : 0), toMonth.Month == 12 ? 1 : (toMonth.Month + 1), 1);

            var data = (from c in db.Corporates
                        join u in db.UserProfiles on c.CorporateId equals u.CorporateId
                        join p in db.Prescriptions on u.UserId equals p.PatientId
                        where !u.IsDelete && !p.IsDelete && p.CreateDate >= startPeriod && p.CreateDate < endPeriod
                        group p by new { c.BranchName, p.CreateDate.Month } into g
                        orderby g.Key.BranchName
                        select new { g.Key.BranchName, g.Key.Month, Count = g.Count() }).AsEnumerable();

            foreach (var row in data)
            {
                var monthIndex = Math.Abs(row.Month - fromMonth.Month);
                if (!resultDict.ContainsKey(row.BranchName))
                {
                    resultDict[row.BranchName] = Enumerable.Repeat(0, totalMonths).ToList();
                }
                resultDict[row.BranchName][monthIndex] += row.Count;
            }

            return resultDict.Select(e => new PropertyCountsReportModel<int>
            {
                Name = e.Key,
                Counts = e.Value
            });
        }

        public static IEnumerable<PropertyCountsReportModel<int>> GetMonthlyCorporateSignUpCounts(Entity.db_HeyDocEntities db, DateTime fromMonth, DateTime toMonth)
        {
            if (toMonth < fromMonth)
            {
                throw new Exception("Invalid date values");
            }

            List<PropertyCountsReportModel<int>> results = new List<PropertyCountsReportModel<int>>();
            var totalMonths = (toMonth.Year - fromMonth.Year) * 12 + (toMonth.Month - fromMonth.Month + 1);

            foreach (var branchName in db.Corporates.Select(e => e.BranchName).OrderBy(name => name))
            {
                results.Add(new PropertyCountsReportModel<int>
                {
                    Name = branchName,
                    Counts = Enumerable.Repeat(0, totalMonths).ToList()
                });
            }

            var startPeriod = new DateTime(fromMonth.Year, fromMonth.Month, 1);
            var endPeriod = new DateTime(toMonth.Year + (toMonth.Month == 12 ? 1 : 0), toMonth.Month == 12 ? 1 : (toMonth.Month + 1), 1);

            var data = (from c in db.Corporates
                        join u in db.UserProfiles on c.CorporateId equals u.CorporateId
                        where u.CreateDate >= startPeriod && u.CreateDate < endPeriod
                        group u by new { c.BranchName, u.CreateDate.Month } into g
                        orderby g.Key.BranchName
                        select new { g.Key.BranchName, g.Key.Month, Count = g.Count() }).AsEnumerable();

            foreach (var row in data)
            {
                var monthIndex = Math.Abs(row.Month - fromMonth.Month);
                results.FirstOrDefault(r => r.Name == row.BranchName).Counts[monthIndex] += row.Count;
            }

            return results;
        }

        public static IEnumerable<PropertyCountsReportModel<int>> GetPharmacyPrescribedMedCounts(Entity.db_HeyDocEntities db, DateTime monthYear, int prescriptionSourceId)
        {
            var startOfMonth = new DateTime(monthYear.Year, monthYear.Month, 1);
            DateTime startOfNextMonth;
            if (monthYear.Month == 12)
            {
                startOfNextMonth = new DateTime(monthYear.Year + 1, 1, 1);
            }
            else
            {
                startOfNextMonth = new DateTime(monthYear.Year, monthYear.Month + 1, 1);
            }

            var results = (from p in db.Prescriptions
                           where p.PrescribedByUser.PrescriptionSourceId == prescriptionSourceId && !p.IsDelete && p.CreateDate >= startOfMonth && p.CreateDate < startOfNextMonth
                           join d in db.Drugs on p.PrescriptionId equals d.PrescriptionId
                           join m in db.Medications on d.MedicationId equals m.MedicationId
                           group m by new { m.MedicationName } into g
                           select new { MedName = g.Key.MedicationName, PrescribeCount = g.Count() })
                           .Where(e => e.PrescribeCount > 0)
                           .OrderByDescending(e => e.PrescribeCount);

            return results.Select(r => new PropertyCountsReportModel<int>
            {
                Name = r.MedName,
                Counts = new List<int>()
                {
                    r.PrescribeCount
                }
            });
        }

        public static IEnumerable<ReportActivityCountModel> GetTpaActivityCountsSinceMonth(Entity.db_HeyDocEntities db, DateTime fromMonth, int tpaId)
        {
            List<ReportActivityCountModel> results = new List<ReportActivityCountModel>();

            var entityCorporates = db.Corporates.Where(e => e.TPAId == tpaId);

            var startOfMonth = new DateTime(fromMonth.Year, fromMonth.Month, 1);

            // user sign-ups, number of users that used chat, and last activity since startOfMonth
            var activityCounts = entityCorporates.GroupJoin(db.UserProfiles, c => c.CorporateId, u => u.CorporateId, (c, u) => new
            {
                BranchName = c.BranchName,
                NewSignUps = u.Count(e => e.CreateDate >= startOfMonth),
                ActiveUsers = u.Count(e => e.LastActivityDate >= startOfMonth),
                UsedChat = u.Count(e => e.Chats.FirstOrDefault(m => m.CreateDate >= startOfMonth) != null)
            }).OrderBy(r => r.BranchName).ToList();

            foreach (var row in activityCounts)
            {
                results.Add(new ReportActivityCountModel
                {
                    BranchName = row.BranchName,
                    NewSignUpCount = row.NewSignUps,
                    ActiveUsersCount = row.ActiveUsers,
                    ChatUseCount = row.UsedChat
                });
            }

            return results;
        }

        public static IEnumerable<ReportDoctorReviewModel> GetTpaDoctorReviews(Entity.db_HeyDocEntities db, DateTime inMonth, int tpaId)
        {
            var startOfMonth = new DateTime(inMonth.Year, inMonth.Month, 1);
            var startOfNextMonth = new DateTime(startOfMonth.Month == 12 ? startOfMonth.Year + 1 : startOfMonth.Year, startOfMonth.Month == 12 ? 1 : startOfMonth.Month + 1, 1);

            // doctor reviews
            var reviewCounts = db.DoctorUserReviews.Where(r => r.UserProfile1.Corporate.TPAId == tpaId && r.CreateDate >= startOfMonth && r.CreateDate < startOfNextMonth).ToList().Select(e => new ReportDoctorReviewModel
            {
                DoctorName = e.UserProfile.FullName,
                PatientName = e.UserProfile1.FullName,
                Rating = e.Rating,
                Comment = e.Comment,
                ReviewTime = e.CreateDate
            }).OrderByDescending(e => e.ReviewTime);

            return reviewCounts;
        }

        public static IEnumerable<PropertyCountsReportModel<int>> GetTpaChatRequestAcceptedCounts(Entity.db_HeyDocEntities db, DateTime inMonth, int tpaId)
        {
            var startOfMonth = new DateTime(inMonth.Year, inMonth.Month, 1);
            var startOfNextMonth = new DateTime(startOfMonth.Month == 12 ? startOfMonth.Year + 1 : startOfMonth.Year, startOfMonth.Month == 12 ? 1 : startOfMonth.Month + 1, 1);

            var results = new List<PropertyCountsReportModel<int>>();

            // requested chat & accepted chat counts
            var chatCounts = db.ChatResponses
                .Where(e => e.ChatRoom.Patient.UserProfile.Corporate.TPAId == tpaId && e.CreatedDate >= startOfMonth && e.CreatedDate < startOfNextMonth)
                .GroupBy(e => e.CreatedDate.Month)
                .Select(g => new
                {
                    Month = g.Key,
                    Requested = g.Count(x => x.RequestStatus == RequestStatus.Requested),
                    Accepted = g.Count(x => x.RequestStatus == RequestStatus.Accepted)
                })
                .OrderByDescending(e => e.Month);

            foreach (var row in chatCounts)
            {
                results.Add(new PropertyCountsReportModel<int>
                {
                    Name = $"{ DateTimeFormatInfo.InvariantInfo.GetMonthName(row.Month) } { inMonth.Year }",
                    Counts = new List<int>
                    {
                        row.Requested,
                        row.Accepted
                    }
                });
            }

            return results;
        }

        public static IEnumerable<ReportChatRequestModel> GetTpaChatRequests(Entity.db_HeyDocEntities db, DateTime inMonth, int tpaId)
        {
            var startOfMonth = new DateTime(inMonth.Year, inMonth.Month, 1);
            var startOfNextMonth = new DateTime(startOfMonth.Month == 12 ? startOfMonth.Year + 1 : startOfMonth.Year, startOfMonth.Month == 12 ? 1 : startOfMonth.Month + 1, 1);

            var entityChatRequestsDetailed = (
                from r1 in db.ChatResponses
                from r2 in db.ChatResponses
                    .Where(r => r.ChatRoomId == r1.ChatRoomId && r.CreatedDate > r1.CreatedDate)
                    .OrderBy(r => r.CreatedDate)
                    .Take(1)
                let r3 = db.ChatResponses.Where(r => r.CreatedDate > r1.CreatedDate && r.ChatRoomId == r1.ChatRoomId && r.RequestStatus == RequestStatus.Completed)
                    .OrderBy(r => r.CreatedDate)
                    .FirstOrDefault()
                where r1.ChatRoom.Patient.UserProfile.Corporate.TPAId == tpaId && r1.CreatedDate >= startOfMonth && r1.CreatedDate < startOfNextMonth && r1.RequestStatus == RequestStatus.Requested && (r2.RequestStatus == RequestStatus.Canceled || r2.RequestStatus == RequestStatus.Accepted || r2.RequestStatus == RequestStatus.Rejected)
                orderby r1.CreatedDate descending
                select new
                {
                    PatientName = r1.ChatRoom.Patient.UserProfile.FullName,
                    DoctorName = r1.ChatRoom.Doctor.UserProfile.FullName,
                    ChatRequestTime = r1.CreatedDate,
                    RequestRespondedTime = (DateTime?)r2.CreatedDate,
                    SessionEndTime = (DateTime?)r3.CreatedDate,
                    RequestResponse = r2.RequestStatus == RequestStatus.Canceled ? "Cancelled" : (r2.RequestStatus == RequestStatus.Accepted ? "Accepted" : "Rejected")
                }
            );

            var chatRequestsDetailed = entityChatRequestsDetailed.ToList().Select(e => new ReportChatRequestModel
            {
                PatientName = e.PatientName,
                DoctorName = e.DoctorName,
                ChatRequestTime = e.ChatRequestTime,
                RequestRespondedTime = e.RequestRespondedTime,
                SessionEndTime = e.SessionEndTime,
                ResponseTime = e.RequestRespondedTime.HasValue ? Math.Floor(e.RequestRespondedTime.Value.Subtract(e.ChatRequestTime).TotalSeconds).ToString() : "No Session",
                SessionDuration = e.SessionEndTime.HasValue ? Math.Floor(e.SessionEndTime.Value.Subtract(e.ChatRequestTime).TotalSeconds).ToString() : "No Session",
                RequestResponse = e.RequestResponse
            });

            return chatRequestsDetailed;
        }

        public static IEnumerable<ReportPrescriptionMedModel> GetTpaPrescriptionMeds(Entity.db_HeyDocEntities db, DateTime inMonth, int tpaId)
        {
            var startOfMonth = new DateTime(inMonth.Year, inMonth.Month, 1);
            var startOfNextMonth = new DateTime(startOfMonth.Month == 12 ? startOfMonth.Year + 1 : startOfMonth.Year, startOfMonth.Month == 12 ? 1 : startOfMonth.Month + 1, 1);

            var results = new List<ReportPrescriptionMedModel>();

            // prescribed medications
            var medCounts = db.Prescriptions.Where(e => e.Patient.UserProfile.Corporate.TPAId == tpaId && e.CreateDate >= startOfMonth && e.CreateDate < startOfNextMonth && !e.IsDelete).Join(db.Drugs, p => p.PrescriptionId, d => d.PrescriptionId, (p, d) => new
            {
                PatientName = p.Patient.UserProfile.FullName,
                PrescriptionId = p.PrescriptionId,
                MedicationName = d.Medication.MedicationName,
                PrescriptionCreateTime = p.CreateDate,
                MedicalSummary = p.MedicalSummary
            }).OrderByDescending(e => e.PrescriptionCreateTime);

            foreach (var row in medCounts)
            {
                results.Add(new ReportPrescriptionMedModel
                {
                    PatientName = row.PatientName,
                    PrescriptionId = row.PrescriptionId,
                    MedicationName = row.MedicationName,
                    PrescriptionCreateTime = row.PrescriptionCreateTime,
                    MedicalSummary = row.MedicalSummary
                });
            }

            return results;
        }

        public static async Task<IEnumerable<PropertyCountsReportModel<string>>> GetDoctorChatRequestSummary(Entity.db_HeyDocEntities db, DateTime fromDate, DateTime toDate, Guid? referralId = null, bool corporateOnly = false)
        {
            var startOfDate = fromDate.Date;
            var endOfDate = toDate.Date.AddDays(1);

            var query = from r1 in (db.ChatResponses
                            .ConditionalWhere(referralId.HasValue, m => m.ChatRoom.Patient.UserProfile.ReferrerId == referralId.Value)
                            .ConditionalWhere(corporateOnly, m => m.ChatRoom.Patient.UserProfile.CorporateId.HasValue))
                        where r1.CreatedDate >= startOfDate && r1.CreatedDate < endOfDate && r1.RequestStatus == RequestStatus.Requested
                        let r2 = db.ChatResponses.OrderBy(e => e.CreatedDate).FirstOrDefault(res => res.ChatRoomId == r1.ChatRoomId && res.CreatedDate > r1.CreatedDate)
                        select new
                        {
                            r1.ChatRoom.DoctorId,
                            r1.ChatRoom.Doctor.UserProfile.FullName,
                            ResponseStatus = (RequestStatus?)r2.RequestStatus,
                            RequestTime = r1.CreatedDate,
                            ResponseTime = (DateTime?)r2.CreatedDate
                        };
            var results = from temp in await query.ToListAsync()
                          group temp by new { temp.DoctorId, temp.FullName } into tempResult
                          let averageTime = tempResult.Average(r => r.ResponseTime.HasValue && (r.ResponseStatus == RequestStatus.Rejected || r.ResponseStatus == RequestStatus.Accepted) ? r.ResponseTime.Value.Subtract(r.RequestTime).TotalSeconds as double? : null)
                          select new PropertyCountsReportModel<string>()
                          {
                              Name = tempResult.Key.DoctorId.ToString(),
                              Counts = new List<string>()
                              {
                                  tempResult.Key.FullName,
                                  tempResult.Count().ToString(),
                                  tempResult.Count(e => e.ResponseStatus == RequestStatus.Accepted).ToString(),
                                  tempResult.Count(e => e.ResponseStatus == RequestStatus.Rejected).ToString(),
                                  averageTime.HasValue ? Math.Round(averageTime.Value).ToString() : "-"
                              }
                          };

            return results;
        }

        public static IEnumerable<PropertyCountsReportModel<string>> GetDoctorChatRequestDetails(Entity.db_HeyDocEntities db, DateTime fromDate, DateTime toDate)
        {
            db.Database.CommandTimeout = 60; // works fine on staging but worried about timeouts on production

            var startOfDate = fromDate.Date;
            var endOfDate = toDate.Date.AddDays(1);

            var results = new List<PropertyCountsReportModel<string>>();

            var query = from r1 in db.ChatResponses
            join c in db.ChatRooms on r1.ChatRoomId equals c.ChatRoomId
            where r1.CreatedDate >= startOfDate && r1.CreatedDate < endOfDate && r1.RequestStatus == RequestStatus.Requested
            let r2 = db.ChatResponses.OrderBy(e => e.CreatedDate).FirstOrDefault(r => r.ChatRoomId == r1.ChatRoomId && r.CreatedDate > r1.CreatedDate)
            select new
            {
                DoctorId = c.DoctorId,
                DoctorName = c.Doctor.UserProfile.FullName,
                CorporateId = c.Patient.UserProfile.CorporateId,
                CorporateName = c.Patient.UserProfile.Corporate.BranchName,
                ReferrerName = c.Patient.UserProfile.UserReferralCode,
                RequestTime = r1.CreatedDate,
                ResponseTime = (DateTime?)r2.CreatedDate,
                RequestStatus = (RequestStatus?)r2.RequestStatus
            };
            
            foreach (var row in query)
            {
                results.Add(new PropertyCountsReportModel<string>()
                {
                    Name = row.DoctorId.ToString(),
                    Counts = new List<string>()
                    {
                        row.DoctorName,
                        row.CorporateId?.ToString() ?? "",
                        row.CorporateName ?? "",
                        row.ReferrerName?.ReferrerName ?? "",
                        row.RequestTime.ToString("yyyy-MM-dd HH:mm:ss"),
                        row.ResponseTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? "-",
                        row.ResponseTime.HasValue ? Math.Round((row.ResponseTime.Value - row.RequestTime).TotalSeconds).ToString() : "-",
                        row.RequestStatus.HasValue ? row.RequestStatus.Value.ToString() : "Not responded"
                    }
                });
            }

            return results;
        }

        public static async Task<IEnumerable<PropertyCountsReportModel<string>>> GetDoctorEpsResponseStats(Entity.db_HeyDocEntities db, DateTime fromDate, DateTime toDate)
        {
            var startOfDate = fromDate.Date;
            var endOfDate = toDate.Date.AddDays(1);

            var query = from p in db.Prescriptions
                        where p.PrescribedBy.HasValue && p.CreateDate >= startOfDate && p.CreateDate < endOfDate && p.DoctorId.HasValue
                        select new
                        {
                            p.DoctorId,
                            p.Doctor.UserProfile.FullName,
                            p.ApprovedDate,
                            p.AssignedDate,
                            p.IsDelete
                        };
            var results = from p in await query.ToListAsync()
                          group p by new { p.DoctorId, p.FullName } into tempResult
                          let averageTime = tempResult.Average(p => (p.ApprovedDate.HasValue && p.AssignedDate.HasValue) ? p.ApprovedDate.Value.Subtract(p.AssignedDate.Value).TotalSeconds as double? : null)
                          select new PropertyCountsReportModel<string>()
                          {
                              Name = tempResult.Key.DoctorId.ToString(),
                              Counts = new List<string>()
                              {
                                  tempResult.Key.FullName,
                                  tempResult.Count().ToString(),
                                  tempResult.Count(p => p.IsDelete).ToString(),
                                  averageTime.HasValue ? Math.Round(averageTime.Value).ToString() : "-"
                              }
                          };

            return results;
        }

        public static async Task<IEnumerable<PropertyCountsReportModel<string>>> GetDoctorEpsResponseDetails(Entity.db_HeyDocEntities db, DateTime fromDate, DateTime toDate)
        {
            var startOfDate = fromDate.Date;
            var endOfDate = toDate.Date.AddDays(1);

            var query = from p in db.Prescriptions
            where p.PrescribedBy.HasValue && p.CreateDate >= startOfDate && p.CreateDate < endOfDate && p.DoctorId.HasValue
            select new
            {
                p.DoctorId,
                p.Doctor.UserProfile.FullName,
                p.PrescriptionId,
                p.PrescribedByUser.PrescriptionSource.PrescriptionSourceName,
                p.CreateDate,
                p.AssignedDate,
                p.ApprovedDate,
                p.PrescriptionAvailabilityStatus
            };

            return (await query.ToListAsync()).Select(p => new PropertyCountsReportModel<string>()
            {
                Name = p.DoctorId.ToString(),
                Counts = new List<string>()
                {
                    p.FullName,
                    p.PrescriptionId.ToString(),
                    p.PrescriptionSourceName,
                    p.CreateDate.ToString("yyyy-MM-dd HH:mm:ss"),
                    p.AssignedDate.HasValue? p.AssignedDate.Value.ToString("yyyy-MM-dd HH:mm:ss") : "-",
                    p.ApprovedDate.HasValue? p.ApprovedDate.Value.ToString("yyyy-MM-dd HH:mm:ss") : "-",
                    (p.ApprovedDate.HasValue && p.AssignedDate.HasValue) ? Math.Round((p.ApprovedDate.Value - p.AssignedDate.Value).TotalSeconds).ToString() : "-",
                    p.PrescriptionAvailabilityStatus.ToString()
                }
            });
        }

        public static List<PropertyCountsReportModel<string>> GetEpsOutletsPrescriptionCounts(Entity.db_HeyDocEntities db, DateTime fromDate, DateTime toDate, int prescriptionSourceId)
        {
            var res = PrescriptionService.GeneratePrescriptionStats(db, prescriptionSourceId, fromDate, toDate).ToList().Select(e => e as EpsPrescriptionStatsModel);

            return res.Select(e => new PropertyCountsReportModel<string>
            {
                Name = e.FullName,
                Counts = new List<string>
                {
                    e.Email,
                    e.PrescriptionCount.ToString(),
                    e.ApprovedCount.ToString(),
                    e.RejectedCount.ToString(),
                    e.DispensedPharmacistCount.ToString(),
                    e.DispensedDoctorCount.ToString(),
                }
            }).ToList();
        }

    }
}
