using Application.Contracts;
using Domain.Entities.Production;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using NpgsqlTypes;
using System.Data;

namespace Infrastructure.Persistance.Repositories.Production
{
    public class WorkMasterRepository : Repository<WorkMaster, Guid>, IWorkMasterRepository
    {
        public IWorkMasterPhaseRepository Phases { get; }

        public WorkMasterRepository(ApplicationDbContext context) : base(context)
        {
            Phases = new WorkMasterPhaseRepository(context);
        }

        public override async Task<IEnumerable<WorkMaster>> GetAll()
        {
            return await dbSet
                        .Include(d => d.Reference)
                        .AsNoTracking()
                        .ToListAsync();
        }

        public override async Task<WorkMaster?> Get(Guid id)
        {
            return await dbSet
                        .Include(d => d.Reference)
                        .Include(d => d.Phases)
                            .ThenInclude(d => d.Details)
                        .AsNoTracking()
                        .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<bool>Copy(WorkMasterCopy workMasterCopy)
        {
            
            var referenceCodeParam = new NpgsqlParameter("@referenceCode", NpgsqlDbType.Varchar)
            {
                Value = workMasterCopy.ReferenceCode,
                Size = 250
            };

            var workmasterIdParam = new NpgsqlParameter("@workmasterId", NpgsqlDbType.Uuid)
            {
                Value = workMasterCopy.WorkmasterId
            };

            var referenceIdParam = new NpgsqlParameter("@referenceId", NpgsqlDbType.Uuid)
            {
                Value = workMasterCopy.ReferenceId != null ? workMasterCopy.ReferenceId : DBNull.Value,
                Direction = ParameterDirection.InputOutput
            };

            var modeParam = new NpgsqlParameter("@modeId", NpgsqlDbType.Integer)
            {
                Value = workMasterCopy.Mode
            };



            await context.Database.ExecuteSqlInterpolatedAsync(
            $"CALL public.SP_Production_CopyWorkMaster({referenceCodeParam}, {workmasterIdParam}, {referenceIdParam}, {modeParam})");

            return true;
        }

        public async Task<WorkMaster?> GetFullById(Guid id)
        {
            return await dbSet
                        .Include(d => d.Reference)
                        .Include("Phases.Details")
                        .Include("Phases.BillOfMaterials")
                        .AsNoTracking()
                        .FirstOrDefaultAsync(e => e.Id == id);
        }
    }
}
