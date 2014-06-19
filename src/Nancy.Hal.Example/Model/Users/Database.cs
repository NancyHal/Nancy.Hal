namespace Nancy.Hal.Example.Model.Users
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Nancy.Hal.Example.Model.Users.Commands;
    using Nancy.Hal.Example.Model.Users.Queries;
    using Nancy.Hal.Example.Model.Users.ViewModels;

    public class Database
    {
        private static IList<UserDetails> userDatabase = new List<UserDetails>();

        private static IList<RoleDetails> roleDatabase = new List<RoleDetails>(); 

        public void CreateUser(CreateUser command)
        {
            var user = new UserDetails()
                           {
                               Id = Guid.NewGuid(),
                               Created = DateTimeOffset.Now,
                               Active = true,
                               UserName = command.UserName,
                               FullName = command.FullName,
                               Email = command.Email,
                               Role = AutoMapper.Mapper.Map<RoleDetails, Role>(roleDatabase.SingleOrDefault(r => r.Id == command.RoleId)),
                               WindowsUsername = command.WindowsUserName
                           };
            userDatabase.Add(user);
            command.Id = user.Id;
        }

        public void UpdateUser(UpdateUserDetails command)
        {
            var user = userDatabase.Single(x => x.Id == command.UserId);
            user.UserName = command.UserName;
            user.FullName = command.FullName;
            user.Email = command.Email;
            user.WindowsUsername = command.WindowsUserName;
            user.Modified = DateTimeOffset.Now;
        }

        public void ChangeRole(ChangeUserRole command)
        {
            var user = userDatabase.Single(x => x.Id == command.UserId);
            user.Role =
                AutoMapper.Mapper.Map<RoleDetails, Role>(roleDatabase.SingleOrDefault(r => r.Id == command.RoleId));

            user.Modified = DateTimeOffset.Now;
        }

        public void Deactivate(DeactivateUser command)
        {
            var user = userDatabase.Single(x => x.Id == command.UserId);
            user.Active = false;
            user.Modified = DateTimeOffset.Now;
        }

        public void Reactivate(ReactivateUser command)
        {
            var user = userDatabase.Single(x => x.Id == command.UserId);
            user.Active = true;
            user.Modified = DateTimeOffset.Now;
        }

        public void CreateRole(CreateRole command)
        {
            var role = new RoleDetails()
                           {
                               Id = Guid.NewGuid(),
                               Name = command.Name,
                               Permissions = command.Permissions,
                               Created = DateTimeOffset.Now
                           };
            roleDatabase.Add(role);
            command.Id = role.Id;
        }

        public void UpdateRole(UpdateRole command)
        {
            var role = roleDatabase.Single(x => x.Id == command.RoleId);
            role.Name = command.Name;
            role.Permissions = command.Permissions;
            role.Modified = DateTimeOffset.Now;
        }

        public void DeleteRole(DeleteRole command)
        {
            var role = roleDatabase.Single(x => x.Id == command.RoleId);
            roleDatabase.Remove(role);
        }

        public UserDetails GetUserById(Guid id)
        {
            return userDatabase.Single(x => x.Id == id);
        }

        public IPagedList<UserSummary> GetAllUsersPaged(GetUserList request)
        {
            var users = userDatabase.ToList();
            if (!string.IsNullOrEmpty(request.Query))
            {
                var query = request.Query.ToLower();
                users =
                    users.Where(x => x.UserName.ToLower().Contains(query) || x.FullName.ToLower().Contains(query))
                         .ToList();
            }

            var totalResults = users.Count;

            if (request.Page.HasValue)
            {
                users = users.Skip(request.Page.Value * request.PageSize.GetValueOrDefault()).ToList();
            }

            if (request.PageSize.HasValue)
            {
                users = users.Take(request.PageSize.Value).ToList();
            }

            var response = new PagedList<UserSummary>(users.Select(AutoMapper.Mapper.Map<UserDetails, UserSummary>))
                               {
                                   TotalResults = totalResults,
                                   PageNumber = request.Page ?? 0,
                                   PageSize = request.PageSize ?? totalResults
                               };
            return response;
        }

        public RoleDetails GetRoleById(Guid id)
        {
            return roleDatabase.Single(x => x.Id == id);
        }

        public IEnumerable<Role> GetAllRoles()
        {
            return roleDatabase.Select(AutoMapper.Mapper.Map<RoleDetails, Role>);
        }
    }
}