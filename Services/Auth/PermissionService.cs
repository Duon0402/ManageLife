using ManageLife.Base;
using ManageLife.Commons;
using ManageLife.Data;
using ManageLife.Interfaces;
using ManageLife.Models;
using ManageLife.Repositories;

namespace ManageLife.Services
{
    public class PermissionService : ServiceBase, IPermissionService
    {
        private readonly PermissionRepository _repo;

        public PermissionService(AppDbContext context) : base(context)
        {
            _repo = new PermissionRepository(context);
        }

        public async Task<Result<List<PermissionModel>>> GetListPermissionsAsync()
        {
            string msg;
            try
            {
                var models = new List<PermissionModel>();

                var entities = await _repo.GetAllAsync();

                if (entities.IsNotEmpty())
                {
                    models = entities.MapToList<PermissionModel>();
                }

                return Result.Ok(models);
            }
            catch (Exception ex)
            {
                msg = TranslationKey.Common.Message.SystemError;
                return Result.Exception<List<PermissionModel>>(msg, ex);
            }
        }
    }
}
