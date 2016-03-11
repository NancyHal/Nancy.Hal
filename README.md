Nancy.Hal [![NuGet Badge](https://buildstats.info/nuget/Nancy.Hal)](https://www.nuget.org/packages/Nancy.Hal/)
=========

Adds lightweight support for the Hal+JSON media type to Nancy

What is Hal?
===========
[Specification](http://stateless.co/hal_specification.html)

What Nancy.Hal does
============
 - Allows Nancy web services to return hal+json formatted responses
 - Allows your web services to return plain old JSON/XML representations of your POCOs
 - Does not require your models to inherit from any base classes or implement any interfaces
 - Uses a fluent declarative syntax to configure the links used to decorate your hypermedia resources
 - Works with whatever JSON Serializer you are using with Nancy

What Nancy.Hal does not do
===================
 - Handle hal+xml responses
 - Deserialize Hal representations back into POCOs (HAL is a serialization format, but says nothing about how to update documents)

Get started
=============
1) Install the Nancy.Hal package
``` 
Install-Package Nancy.Hal
```

2) Create a `HalConfiguration` instance.
```
var config = new HalConfiguration();

//simple example - creates a "self" link templated with the user's id
config.For<UserSummary>()
    .Links(model => new Link("self", "/users/{id}").CreateLink(model));

//complex example - creates paging links populated with query string search terms
config.For<PagedList<UserSummary>>()
      .Embeds("users", x => x.Data)
      .Links(
          (model, ctx) =>
          LinkTemplates.Users.GetUsersPaged.CreateLink("self", ctx.Request.Query, new { blah = "123" }))
      .Links(
          (model, ctx) =>
          LinkTemplates.Users.GetUsersPaged.CreateLink("next", ctx.Request.Query, new { page = model.PageNumber + 1 }),
          model => model.PageNumber < model.TotalPages)
      .Links(
          (model, ctx) =>
          LinkTemplates.Users.GetUsersPaged.CreateLink("prev", ctx.Request.Query, new { page = model.PageNumber - 1 }),
          model => model.PageNumber > 0);


//per request configuration
public ExampleModule()
{
    this.Get["/"] = _ => 
    {
        this.Context
            .LocalHalConfigFor<Users>()
            .Links("relation", "/link");

        return 200;
    };
}
```

3) Register it in your application container.
```
//TinyIOC
container.Register(typeof(IProvideHalTypeConfiguration), config);

//NInject
kernel.Bind<IProvideHalTypeConfiguration>().ToConstant(config);
```

4) That's it! Don't forget to set your `Accept` header to `application/hal+json`

Acknowledgements
================
This library could not exist without the work and ideas of others:
 - It started as a port of [Jake Ginnivan](http://twitter.com/jakeginnivan)'s [WebApi.Hal](https://github.com/JakeGinnivan/WebApi.Hal)
 - ..which in turn is based on the work of [Steve Michelotti](https://bitbucket.org/smichelotti/hal-media-type)'s hal-media-type
 - The fluent configuration idea was lifted from [https://github.com/kekekeks/hal-json-net/tree/master/HalJsonNet](here)
 - And ideas were borrowed from [wis3guy](https://github.com/wis3guy)
