namespace Nancy.Hal.Example.Model.Users
{
    using System.Linq;

    using AutoMapper;

    using Nancy;
    using Nancy.Hal.Example.Hal;
    using Nancy.Hal.Example.Model.Users.Commands;
    using Nancy.Hal.Example.Model.Users.Queries;
    using Nancy.Hal.Example.Model.Users.ViewModels;
    using Nancy.Hal.Example.Model.Users.ViewModels.Resources;
    using Nancy.ModelBinding;

    public class RolesModule : NancyModule
    {
        public RolesModule(Database db)
            : base("/roles")
        {
            this.Get["/"] = _ => 
                    {
                        var request = this.Bind<GetRoleList>();
                        var roles = db.GetAllRoles();
                        return
                            Negotiate.WithModel(roles)
                                     .WithHalModel(
                                         new RoleSummaryListResource(
                                             roles.Select(Mapper.Map<Role, RoleSummaryResource>).ToList()));
                    };

            this.Get["/{roleId:guid}"] = _ => 
                {
                    var req = this.Bind<GetRoleDetails>();
                    var role = db.GetRoleById(req.RoleId);

                    if (role == null)
                    {
                        return 404;
                    }

                    return Negotiate.WithModel(role).WithHalModel<RoleDetails, RoleDetailsResource>(role);
                };

            this.Post["/"] = _ =>
                {
                    var req = this.Bind<CreateRole>();
                    db.CreateRole(req);
                        var role = db.GetRoleById(req.Id.GetValueOrDefault());
                        return Negotiate
                            .WithHeader("Location", "/roles/" + req.Id.GetValueOrDefault())
                            .WithModel(role)
                            .WithHalModel<RoleDetails, RoleDetailsResource>(role)
                            .WithStatusCode(HttpStatusCode.Created);
                    };

            this.Put["/{roleId:guid}"] = _ =>
                {
                    var req = this.Bind<UpdateRole>();
                    db.UpdateRole(req);
                    var role = db.GetRoleById(req.RoleId);
                    
                    return
                        Negotiate.WithHeader("Location", "/roles/" + req.RoleId)
                                 .WithModel(role)
                                 .WithHalModel<RoleDetails, RoleDetailsResource>(role);
                };

            this.Delete["/{roleId:guid}"] = _ => 
                    {
                        var req = this.Bind<DeleteRole>();
                        db.DeleteRole(req);
                        return this.Negotiate.WithStatusCode(HttpStatusCode.NoContent);
                    };
        }
    }
}