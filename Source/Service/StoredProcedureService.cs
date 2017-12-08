using System.Collections.Generic;
using System.Linq;
using Data;
using Data.Infrastructure;
using Model.Models;

using Core.Common;
using System;
using Model;
using System.Threading.Tasks;
using Data.Models;

namespace Service
{
    public interface IStoredProcedureService : IDisposable
    {
        IEnumerable<TargetAchievement> DashBoardTargetAchievement();
        IEnumerable<TargetAchievement> DashBoardTargetAchievementPercentage();
        IEnumerable<PurchaseOrderBalanceTimeLineRisk> DashBoardPurchaseOrderBalanceTimeLineRisk();
        IEnumerable<PurchaseOrderBalanceTimeLineRisk> DashBoardPurchaseOrderBalanceTimeLineRiskPercentage();
        IEnumerable<PurchaseOrderBalanceCityTimeLine> DashBoardPurchaseOrderBalanceCityTimeLineMonthly();
        IEnumerable<PurchaseOrderBalanceCityTimeLine> DashBoardPurchaseOrderBalanceCityTimeLineQarterly();
    }

    public class StoredProcedureService : IStoredProcedureService
    {
        private readonly IPlanningStoredProcedures _storedProcedures;

        public StoredProcedureService(IPlanningStoredProcedures storedProcedures)
        {
            _storedProcedures = storedProcedures;
        }

        public IEnumerable<TargetAchievement> DashBoardTargetAchievement()
        {
            return _storedProcedures.DashBoardTargetAchievement();
        }

        public IEnumerable<TargetAchievement> DashBoardTargetAchievementPercentage()
        {
            return _storedProcedures.DashBoardTargetAchievementPercentage();
        }

        public IEnumerable<PurchaseOrderBalanceTimeLineRisk> DashBoardPurchaseOrderBalanceTimeLineRisk()
        {
            return _storedProcedures.DashBoardPurchaseOrderBalanceTimeLineRisk();
        }
        public IEnumerable<PurchaseOrderBalanceTimeLineRisk> DashBoardPurchaseOrderBalanceTimeLineRiskPercentage()
        {
            return _storedProcedures.DashBoardPurchaseOrderBalanceTimeLineRiskPercentage();
        }
        public IEnumerable<PurchaseOrderBalanceCityTimeLine> DashBoardPurchaseOrderBalanceCityTimeLineMonthly()
        {
            return _storedProcedures.DashBoardPurchaseOrderBalanceCityTimeLineMonthly();
        }
        public IEnumerable<PurchaseOrderBalanceCityTimeLine> DashBoardPurchaseOrderBalanceCityTimeLineQarterly()
        {
            return _storedProcedures.DashBoardPurchaseOrderBalanceCityTimeLineQarterly();
        }
        public void Dispose()
        {
        }
    }
}
