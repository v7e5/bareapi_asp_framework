using static util.Util;
using static System.Configuration.ConfigurationManager;

using System;
using System.Linq;
using System.Web.Http;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Http.Formatting;

using Owin;
using Newtonsoft.Json.Linq;
using Microsoft.Owin.Hosting;

sealed class XXX {
  public static async Task Main(string[] args) {
    cl(String.Join("", collatz(new int[]{(new Random()).Next(1, 79)})
        .Select(e => $@"[;38;5;{e % 256};1m|")) + @"[0m");

    var url = $"http://{AppSettings["host"]}:{AppSettings["port"]}";

    using (WebApp.Start<Startup>(url)) {
      Console.WriteLine(":: listening on " + url);
      await Task.Delay(-1);
    }
  }
}

sealed class Startup {
  private static readonly string[] noauth = {
    "login", "hailstone", "echo", "env", "now"
  }; 

  public void Configuration(IAppBuilder app) {
    var config = new HttpConfiguration();
    config.MapHttpAttributeRoutes();

    config.Formatters.Clear();
    config.Formatters.Add(new JsonMediaTypeFormatter());

    //app.Use(async (ctx, next) => {
    //  if((ctx.Request.Path.ToString() == "/login")
    //    || (await Auth.SessionUser(ctx.Request.Cookies["_id"]) != null)) {
    //    await next();
    //  } else {
    //    ctx.Response.StatusCode = 403;
    //    ctx.Response.ReasonPhrase = "Not Authorized";
    //    ctx.Response.ContentType = "application/json";
    //    await ctx.Response.WriteAsync("{\"error\": \"not logged in\"}");
    //  }
    //});

    config.EnsureInitialized();
    app.UseWebApi(config);
  }
}

public sealed class XXXController: ApiController {

  [HttpPost]
  [Route("env")]
  public async Task<IHttpActionResult> _env() =>
    Ok(await Task.Run(() => env()));

  [HttpPost]
  [Route("echo")]
  public async Task<IHttpActionResult> _echo(JObject o) {
    if(o.Value<int>("delay") is int n && n > 0) {
      await Task.Delay(n);
    }

    return Ok(
      new {t = DateTimeOffset.UtcNow.ToLocalTime().ToString(), o}
    );
  }

  [HttpPost]
  [Route("hailstone")]
  public IHttpActionResult _hailstone(JObject o) =>
    Ok((o.Value<int>("n") is int n && n > 0) ? collatz(new int[]{n}) : null);


  [HttpPost]
  [Route("now")]
  public async Task<IHttpActionResult> _now() {

    IDictionary<string,object> database;
    using (var conn = new SqlConnection(
      ConnectionStrings["dbconn"].ConnectionString)) {
      await conn.OpenAsync();

      using(var cmd = conn.CreateCommand()) {
        cmd.CommandText = @"
          select
          current_timezone() as tz,
          sysdatetimeoffset() as local,
          datediff(second, '19700101', sysutcdatetime()) as unix_timestamp,
          sysutcdatetime() as unix_timestamp_str";

        database = (await cmd.ExecuteReaderAsync())
          .ToDictArray().FirstOrDefault();
      }
    }

    var ut = DateTimeOffset.UtcNow;

    return Ok(new {
      user = "",
      server = new {
        tz = TimeZoneInfo.Local.DisplayName,
        local = ut.ToLocalTime().ToString(),
        unix_timestamp = ut.ToUnixTimeSeconds(),
        unix_timestamp_str = ut.ToString()
      },
      database,
      ng = new NonGen().Cast<int>(),
      cg = new ConGen()
    });
  }

}
