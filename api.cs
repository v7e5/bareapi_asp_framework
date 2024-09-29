using auth;
using static util.Util;
using static System.Configuration.ConfigurationManager;
using System;
using System.Net;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Http.Formatting;
using System.ComponentModel.DataAnnotations;
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

    app.Use(async (ctx, next) => {
      if ((await Auth.SessionUser(ctx.Request.Cookies["_id"]) != null)
        || noauth.Contains(ctx.Request.Path.ToString().Substring(1))) {
        await next();
      } else {
        ctx.Response.StatusCode = 403;
        ctx.Response.ReasonPhrase = "Not Authorized";
        ctx.Response.ContentType = "application/json";
        await ctx.Response.WriteAsync("{\"error\": \"verboten\"}");
      }
    });
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
    if (o.Value<int>("delay") is int n && n > 0) {
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
    IDictionary<string, object> database;

    using (var conn = new SqlConnection(
      ConnectionStrings["dbconn"].ConnectionString)) {
      await conn.OpenAsync();

      using (var cmd = conn.CreateCommand()) {
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
      user = await Auth.SessionUser(Request),
      server = new {
        tz = TimeZoneInfo.Local.DisplayName,
        local = ut.ToLocalTime().ToString(),
        unix_timestamp = ut.ToUnixTimeSeconds(),
        unix_timestamp_str = ut.ToString()
      },
      database
    });
  }

  [HttpPost]
  [Route("login")]
  public async Task<HttpResponseMessage> _login(ModelLogin l) {
    if (await Auth.SessionUser(Request) != null) {
      return Request.CreateResponse(HttpStatusCode.OK);
    }

    if (!ModelState.IsValid) {
      return Request.CreateResponse(
        HttpStatusCode.Forbidden,
        ModelState
          .Values.SelectMany(v => v.Errors.Select(b => b.ErrorMessage))
          .Where(e => e != "")
      );
    }

    IDictionary<string, object> user;

    using (var conn = new SqlConnection(
      ConnectionStrings["dbconn"].ConnectionString)) {
      await conn.OpenAsync();

      using (var user_cmd = conn.CreateCommand()) {
        user_cmd.CommandText
          = "select id, passwd from usuario where username=@u";
        user_cmd.Parameters.Add(
            new SqlParameter("u", SqlDbType.Text){Value = login.username});

        user = (await cmd.ExecuteReaderAsync()).ToDictArray().FirstOrDefault();
      }
    }

    if(user is null
      || (user["passwd"]?.ToString()?.Split(':') is string[] arr
        && !FixedTimeEquals(
              deriveKey(
                password: login.passwd!,
                salt: Convert.FromBase64String(arr[0])
              ),
              Convert.FromBase64String(arr[1])
            ))
      ) {
      return Request.CreateResponse(
        HttpStatusCode.Forbidden,
        new {error = "incorrect user/pass"}
      );
    }

    int userid = user["id"];
    await Auth.SessionClear(userid);
    var response = Request.CreateResponse(HttpStatusCode.OK);

    response.Headers.Add("set-cookie", "_id="
      + await Auth.SessionSet(userid)
      + ";domain=" + AppSettings["host"]
      + ";path=/;httponly;samesite=lax;max-age=604800"
    );

    return response;
  }

  [HttpPost]
  [Route("logout")]
  public async Task<HttpResponseMessage> _logout() {
    await Auth.SessionClear(await Auth.SessionUser(Request));
    var response = Request.CreateResponse(HttpStatusCode.OK);

    response.Headers.Add("set-cookie", "_id="
      + ";domain=" + AppSettings["host"]
      + ";path=/;httponly;samesite=lax;max-age=0"
    );

    return response;
  }

}

public struct ModelLogin {
  [Required(ErrorMessage = "username cannot be blank")]
  public string username {get; set;}

  [Required(ErrorMessage = "password cannot be blank")]
  public string passwd {get; set;}
}
