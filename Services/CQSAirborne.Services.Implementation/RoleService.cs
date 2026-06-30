using CQSAirborne.Domain;
using CQSAirborne.Domain.StoredProcedure.TableTypes;
using CQSAirborne.Model.Core;
using CQSAirborne.Model.Plant;
using CQSAirborne.Model.Role;
using CQSAirborne.Repository.Contract;
using CQSAirborne.Services.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQSAirborne.Services.Implementation
{
    public class RoleService : IRoleService
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IPermissionRepository _permissionRepository;
        private readonly IRolePermissionRepository _rolePermissionRepository;
        private readonly IDataMapper _dataMapper;
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;

        public RoleService(IRoleRepository roleRepository,
            IPermissionRepository permissionRepository,
            IRolePermissionRepository rolePermissionRepository,
            IUnitOfWork unitOfWork, IDataMapper autoMapperGenericDataMapper,
            IUserRepository userRepository)
        {
            _roleRepository = roleRepository;
            _permissionRepository = permissionRepository;
            _rolePermissionRepository = rolePermissionRepository;
            _unitOfWork = unitOfWork;
            _dataMapper = autoMapperGenericDataMapper;
            _userRepository = userRepository;
        }

        public IQueryable<AddEditRoleModel> GetAll()
        {
            var entity = _roleRepository.GetAll().Where(m => m.IsActive == true);
            return _dataMapper.Project<RoleEntity, AddEditRoleModel>(entity);
        }

        public async Task<AddEditRoleModel> GetByIdAsync(int id)
        {
            var roleEntity = await _roleRepository.GetByIdAsync(id);
            if (roleEntity == null)
                return null;
            var result = _dataMapper.Map<RoleEntity, AddEditRoleModel>(roleEntity);
            return result;
        }

        public List<StructureRoleSelectListModel> GetRoleSelectList()
        {
            return _dataMapper.Project<RoleEntity, StructureRoleSelectListModel>
                (_roleRepository.GetAll().Where(m => m.IsActive == true)).ToList();
        }

        public async Task<int> CreateEditAsync(AddEditRoleModel model, int userId)
        {
            try
            {
                var entity = _dataMapper.Map<AddEditRoleModel, RoleEntity>(model);

                if (model.Id != 0)
                {
                    var mapEntity = await _roleRepository.GetByIdAsync(model.Id);
                    if (mapEntity != null)
                    {
                        model.RoleName = mapEntity.RoleName;
                        entity = _dataMapper.Map(model, mapEntity);

                        var permissionsToRemove = entity.RolePermissions.ToList();
                        foreach (var permission in permissionsToRemove)
                        {
                            _rolePermissionRepository.Delete(permission);
                            entity.RolePermissions.Remove(permission);
                        }
                    }
                }
                else
                {
                    entity.CreatedBy = userId;
                    entity.CreatedOn = DateTime.Now;
                }

                foreach (var permission in model.Permissions)
                {
                    // List permission
                    if (permission.IsList)
                    {
                        PermissionEntity listPermission = _permissionRepository.GetAll().Where(m => m.Code == permission.Code && m.PermissionTypeId == Constants.PermissionTypeConstant.List).FirstOrDefault();
                        if (listPermission != null)
                        {
                            entity.RolePermissions.Add(new RolePermissionEntity
                            {
                                PermissionId = listPermission.Id
                            });
                        }
                    }

                    // Add Permission
                    if (permission.IsAdd)
                    {
                        PermissionEntity addPermission = _permissionRepository.GetAll().Where(m => m.Code == permission.Code && m.PermissionTypeId == Constants.PermissionTypeConstant.Add).FirstOrDefault();
                        if (addPermission != null)
                        {
                            entity.RolePermissions.Add(new RolePermissionEntity
                            {
                                PermissionId = addPermission.Id
                            });
                        }
                    }

                    // Edit Permission
                    if (permission.IsEdit)
                    {
                        PermissionEntity editPermission = _permissionRepository.GetAll().Where(m => m.Code == permission.Code && m.PermissionTypeId == Constants.PermissionTypeConstant.Edit).FirstOrDefault();
                        if (editPermission != null)
                        {
                            entity.RolePermissions.Add(new RolePermissionEntity
                            {
                                PermissionId = editPermission.Id
                            });
                        }
                    }

                    if (permission.IsDelete)
                    {
                        PermissionEntity editPermission = _permissionRepository.GetAll().Where(m => m.Code == permission.Code && m.PermissionTypeId == Constants.PermissionTypeConstant.Delete).FirstOrDefault();
                        if (editPermission != null)
                        {
                            entity.RolePermissions.Add(new RolePermissionEntity
                            {
                                PermissionId = editPermission.Id
                            });
                        }
                    }
                }

                entity.IsActive = true;

                entity.ModifiedBy = userId;
                entity.ModifiedOn = DateTime.Now;
                if (model.Id != 0)
                {
                    _roleRepository.Update(entity);
                }
                else
                {
                    _roleRepository.Insert(entity);
                }
                 _unitOfWork.Commit();

                return await Task.Run(() => entity.Id);
            }
            catch (Exception ex)
            {
                return await Task.Run(() => 0);
            }
        }

        public async Task<List<AssignPermissionViewModel>> GetPermissionByType(PermissionScreenType permissionScreenType, string id, int userId)
        {
            int? roleId = 0;
            if (id == null || id == string.Empty || id.ToLower() == "undefined")
            {
                roleId =  _roleRepository.GetAll().Where(m => m.RoleName == Role.User).FirstOrDefault().Id;
            }
            else
            {
                try
                {
                    roleId = _roleRepository.GetAll().Where(m => m.RoleName.ToLower() == id.ToLower())?.FirstOrDefault().Id;
                }
                catch(Exception e)
                {

                }
                
            }

            List<PermissionEntity> permissions = new List<PermissionEntity>();
            switch (permissionScreenType)
            {
                case PermissionScreenType.Role:
                    permissions = _rolePermissionRepository.GetAll().Where(m => m.RoleId == roleId).Select(m => m.PermissionMaster).ToList();
                    break;
                    //case PermissionScreenType.User:
                    //    //permissions = _userRepository.GetByUserAndStructureId(id, userId).ToList();
                    //    var userPermissions = _userPermissionRepository.Get(m => m.UserId == userId).ToList();
                    //    var rolePermissions = permissions.Select(s => s).ToList();
                    //    foreach (var rolePermission in rolePermissions)
                    //    {
                    //        if (userPermissions.Any(w => w.PermissionId == rolePermission.Id && !w.IsActive))
                    //        {
                    //            permissions.Remove(rolePermission);
                    //        }
                    //    }

                    //    foreach (var userPermission in userPermissions)
                    //    {
                    //        if (userPermission.IsActive && !rolePermissions.Select(s => s.Id).Contains(userPermission.PermissionId))
                    //        {
                    //            permissions.Add(userPermission.PermissionMaster);
                    //        }
                    //    }
                    //    break;
            }

            var permissionData = _permissionRepository.GetAll().Where(m => m.IsActive == true).ToList()
                .GroupBy(s => new
                {
                    s.Code,
                    s.Name,
                    s.DisplayOrder
                }).Select(s => new AssignPermissionViewModel
                {
                    Code = s.Key.Code,
                    Name = s.Key.Name,
                    DisplayOrder = s.Key.DisplayOrder,
                    IsList = permissions.Any(w => w.Code == s.Key.Code && w.PermissionTypeId == Constants.PermissionTypeConstant.List),
                    IsAdd = permissions.Any(w => w.Code == s.Key.Code && w.PermissionTypeId == Constants.PermissionTypeConstant.Add),
                    IsEdit = permissions.Any(w => w.Code == s.Key.Code && w.PermissionTypeId == Constants.PermissionTypeConstant.Edit),
                    IsDelete = permissions.Any(w => w.Code == s.Key.Code && w.PermissionTypeId == Constants.PermissionTypeConstant.Delete),
                }).ToList();


            return await Task.Run(() => permissionData);
        }

        //public async Task<LoginResult> DeleteRole(int Id, int DeletedBy)
        //{
        //    LoginResult model = new LoginResult();
        //    model.IsSuccess = true;
        //    if (_userRepository.Get(m => m.RoleId == Id && m.IsActive == true).Count() > 0)
        //    {
        //        model.IsSuccess = false;
        //        model.Error = "Role Already Assigned.";
        //    }
        //    else
        //    {
        //        var data = _roleRepository.Get(m => m.Id == Id).FirstOrDefault();
        //        data.IsActive = false; ;
        //        data.ModifiedBy = DeletedBy;
        //        data.ModifiedDate = DateTime.Now;
        //        _roleRepository.Update(data);
        //        _unitOfWork.Save();
        //    }

        //    return await Task.Run(() => model);
        //}

    }
}
