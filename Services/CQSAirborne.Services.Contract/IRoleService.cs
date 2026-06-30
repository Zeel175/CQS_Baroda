using CQSAirborne.Model.Core;
using CQSAirborne.Model.Role;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQSAirborne.Services.Contract
{
    public interface IRoleService
    {
        IQueryable<AddEditRoleModel> GetAll();
        List<StructureRoleSelectListModel> GetRoleSelectList();
        Task<int> CreateEditAsync(AddEditRoleModel model, int userId);
        Task<AddEditRoleModel> GetByIdAsync(int id);
        //Task<LoginResult> DeleteRole(long Id, long DeletedBy);
        Task<List<AssignPermissionViewModel>> GetPermissionByType(PermissionScreenType permissionScreenType, string id, int userId);
    }
}
