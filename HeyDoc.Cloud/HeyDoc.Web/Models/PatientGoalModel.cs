using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HeyDoc.Web.Models
{
    public class PatientGoalModel
    {
        public long GoalId { get; set; }
        public int ChatRoomId { get; set; }
        public string Description { get; set; }
        public bool IsComplete { get; set; }
        public string Duration { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public bool IsLifeTime { get; set; }
        public int? RemainderDays { get; set; }
        public int RemarkCount { get; set; }

        public PatientGoalModel()
        {

        }

        public PatientGoalModel(Entity.PatientGoal entityGoal)
        {
            GoalId = entityGoal.GoalId;
            Description = entityGoal.Description;
            IsComplete = entityGoal.IsComplete;
            Duration = entityGoal.Duration;
            CreateDate = entityGoal.CreateDate;
            StartTime = entityGoal.StartTime;
            EndTime = entityGoal.EndTime;
            IsLifeTime = entityGoal.IsLifeTime;

            if (entityGoal.StartTime.HasValue && entityGoal.EndTime.HasValue)
            {
                RemainderDays = (entityGoal.EndTime.Value.Date - entityGoal.StartTime.Value.Date).Days;
            }
           
        }
    }
}