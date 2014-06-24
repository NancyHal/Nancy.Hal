namespace Nancy.Hal.Example.Model{

    public static class LinkTemplates
    {
        public static class Users
        {
            public static Link GetUser { get { return new Link("user", "/users/{id}"); } }

            public static Link GetUsersPaged { get { return new Link("users", "/users/{?query,page,pageSize}"); } }
        }

        public static class User
        {
            public static Link Edit { get { return new Link("edit", "/users/{id}"); } }

            public static Link Deactivate { get { return new Link("deactivate", "/users/{id}/deactivate"); } }

            public static Link Reactivate { get { return new Link("reactivate", "/users/{id}/reactivate"); } }

            public static Link ChangeRole { get { return new Link("change-role", "/users/{id}/role/{roleId}"); } }

            public static Link ChangePassword { get { return new Link("change-password", "/users/{id}/password"); } }
        }

        public static class Roles
        {
            public static Link GetRole { get { return new Link("role", "/roles/{id}"); } }

            public static Link GetRolesPaged { get { return new Link("roles", "/roles/{?query,page,pageSize}"); } }
        }
    }
}