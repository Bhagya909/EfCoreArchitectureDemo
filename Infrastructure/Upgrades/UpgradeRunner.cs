using Application.Common;
using Application.Interfaces;
using Application.Interfaces.Services;
using Application.Interfaces.Upgrades;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Upgrades
{
    public class UpgradeRunner : IUpgradeRunner
    {
        private readonly IEnumerable<IDataUpgrade> _upgrades;

        private readonly IUnitOfWork _unitOfWork;

        private readonly IChangeLogService _changeLogService;

        private readonly RetailDbContext _context;

        public UpgradeRunner(
        IEnumerable<IDataUpgrade> upgrades,
        IUnitOfWork unitOfWork,
        IChangeLogService changeLogService,
        RetailDbContext context)
        {
            _upgrades = upgrades;

            _unitOfWork = unitOfWork;

            _changeLogService = changeLogService;

            _context = context;
        }

        public async Task RunUpgradesAsync()
        {
            foreach (var upgrade in _upgrades)
            {
                var alreadyExecuted =
                    await _context.ChangeLogs
                        .AnyAsync(log =>
                            log.ActionType == LogActionTypes.UpgradeExecuted
                            && log.RawData ==
                            $"Upgrade={upgrade.Name}");

                if (alreadyExecuted)
                {
                    continue;
                }

                await _unitOfWork.ExecuteInTransactionAsync(async () =>
                {
                    await upgrade.ExecuteAsync();

                    await _changeLogService.LogAsync(
                        actionType: LogActionTypes.UpgradeExecuted,
                        entityName: LogEntityNames.DatabaseUpgrade,
                        referenceId: null,
                        description:
                            $"Upgrade '{upgrade.Name}' executed successfully.",
                        rawData:
                            $"Upgrade={upgrade.Name}");

                    await _unitOfWork.SaveChangesAsync();
                });
            }
        }
    }
}
