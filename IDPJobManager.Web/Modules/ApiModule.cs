namespace IDPJobManager.Web.Modules
{
    using IDPJobManager.Core.Domain;
    using Nancy;
    using Nancy.Metadata.Modules;
    using Nancy.ModelBinding;
    using Nancy.Swagger;
    using Nancy.TinyIoc;
    using Swagger.ObjectModel;
    using System.Collections.Generic;

    public class ApiModule : NancyModule
    {
        public ApiModule(TinyIoCContainer container) : base("/Api")
        {
            Get["/"] = _ =>
            {
                var url = $"{Request.Url.BasePath}/api-docs";
                return View["Doc", url];
            };

            Get["/List"] = _ =>
            {
                return "list";
            };

            Get["GetUsers", "/users"] = _ => new[] { new User { Name = "Vincent Vega", Age = 45 } };

            Post["PostUsers", "/users"] = _ =>
            {
                var result = this.BindAndValidate<User>();

                if (!ModelValidationResult.IsValid)
                {
                    return Negotiate.WithModel(new { Message = "Oops" })
                        .WithStatusCode(HttpStatusCode.UnprocessableEntity);
                }

                return Negotiate.WithModel(result).WithStatusCode(HttpStatusCode.Created);
            };
        }
    }

    public class ApiMetadataModule: MetadataModule<PathItem>
    {
        public ApiMetadataModule(ISwaggerModelCatalog modelCatalog)
        {
            modelCatalog.AddModels(typeof(User), typeof(Address), typeof(Role));
            Describe["GetUsers"] = description => description.AsSwagger(
                with => with.Operation(
                    op => op.OperationId("GetUsers")
                            .Tag("Users")
                            .Summary("The list of users")
                            .Description("This returns a list of users from our awesome app")
                            .Response(r => r.Schema<User>().Description("The list of users"))));


            Describe["PostUsers"] =
                description =>
                description.AsSwagger(
                    with =>
                    with.Operation(
                        op =>
                        op.OperationId("PostUsers")
                          .Tag("Users")
                          .Summary("Create a User")
                          .Description("Creates a user with the shown schema for our awesome app")
                          .Response(201, r => r.Description("Created a User"))
                          .Response(422, r => r.Description("Invalid input"))
                          .BodyParameter(p => p.Description("A User object").Name("user").Schema<User>())));
        }
    }
}
