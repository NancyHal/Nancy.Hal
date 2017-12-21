using Nancy;
using Nancy.Hal.Example.Model.Users.Commands;
using Nancy.Hal.Example.Model.Users.Queries;
using Nancy.ModelBinding;
namespace Nancy.Hal.Example.Model.Users {

    public class UsersModule : NancyModule {
        public UsersModule (Database db) : base ("/users") {
            this.Get ("/", _ => {
                var request = this.Bind<GetUserList> ();
                var users = db.GetAllUsersPaged (request);

                return Negotiate.WithModel (users);
            });

            this.Get ("/{userId:guid}", _ => {
                var request = this.Bind<GetUserDetails> ();
                var user = db.GetUserById (request.UserId);
                if (user == null) {
                    return 404;
                }

                return Negotiate.WithModel (user);
            });

            this.Post ("/", _ => {
                var request = this.Bind<CreateUser> ();
                db.CreateUser (request);
                var user = db.GetUserById (request.Id.GetValueOrDefault ());

                return
                this.Negotiate.WithHeader ("Location", LinkTemplates.Users.GetUser.CreateLink (new { Id = user.Id }).ToString ())
                    .WithModel (user)
                    .WithStatusCode (HttpStatusCode.Created);
            });

            this.Put ("/{userId:guid}", _ => {
                var request = this.Bind<UpdateUserDetails> ();
                db.UpdateUser (request);
                var user = db.GetUserById (request.UserId);

                return this.Negotiate.WithModel (user)
                    .WithStatusCode (HttpStatusCode.OK);
            });

            this.Put ("/{userId:guid}/deactivate", _ => {
                var request = this.Bind<DeactivateUser> ();
                db.Deactivate (request);
                var user = db.GetUserById (request.UserId);

                return this.Negotiate.WithModel (user)
                    .WithStatusCode (HttpStatusCode.OK);
            });

            this.Put ("/{userId:guid}/reactivate", _ => {
                var request = this.Bind<ReactivateUser> ();
                db.Reactivate (request);
                var user = db.GetUserById (request.UserId);

                return this.Negotiate.WithModel (user)
                    .WithStatusCode (HttpStatusCode.OK);
            });

            this.Put ("/{userId:guid}/role/{roleId:guid}", _ => {
                var request = this.Bind<ChangeUserRole> ();
                db.ChangeRole (request);
                var user = db.GetUserById (request.UserId);

                return this.Negotiate.WithModel (user)
                    .WithStatusCode (HttpStatusCode.OK);

            });
        }
    }
}