using static util.Util;
using static System.Configuration.ConfigurationManager;

using System.Data;
using System.Linq;
using System.Net.Http;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace auth {
  static class Auth {

    public static async Task<int?> SessionUser(string key) {
      if (key == null) {
        return null;
      }

      using (var conn = new SqlConnection(
        ConnectionStrings["dbconn"].ConnectionString)) {
        await conn.OpenAsync();

        using (var cmd = conn.CreateCommand()) {
          cmd.CommandText = "select userid from sesion where token=@key";
          cmd.Parameters.Add(
            new SqlParameter("key", SqlDbType.Char, 36){Value = key});

          return (int?) await cmd.ExecuteScalarAsync();
        }
      }
    }

    public static async Task<int?> SessionUser(HttpRequestMessage request) {
      return await SessionUser(
        request.Headers.GetCookies("_id").FirstOrDefault()?["_id"].Value);
    }

    public static async void SessionClear(int? userid) {
      using (var conn =
        new SqlConnection(ConnectionStrings["dbconn"].ConnectionString)) {
        await conn.OpenAsync();

        using (var cmd = conn.CreateCommand()) {
          cmd.CommandText = "delete from sesion where userid=@userid";
          cmd.Parameters.Add(
            new SqlParameter("userid", SqlDbType.Int){Value = userid});
          await cmd.ExecuteNonQueryAsync();
        }
      }
    }

    static IEnumerable<string> _guid() {
      while (true) {
        yield return System.Guid.NewGuid().ToString();
      }
    }

    public static async Task<string> SessionSet(int userid) {
      string guid = "";

      using (var conn = new SqlConnection(
            ConnectionStrings["dbconn"].ConnectionString)) {
        await conn.OpenAsync();

        var q = "select token from sesion where token=@key";

        foreach (var g in _guid()) {
          using (var cmd = conn.CreateCommand()) {
            cmd.CommandText = q;
            cmd.Parameters.Add(
              new SqlParameter("key", SqlDbType.Char, 36){Value = g});

            if (await cmd.ExecuteScalarAsync() == null) {
              guid = g;

              using (var sess_add = conn.CreateCommand()) {
                sess_add.CommandText =
                  "insert into sesion(token, userid) values (@key, @userid)";
                sess_add.Parameters.Add(
                  new SqlParameter("key", SqlDbType.Char, 36){Value = g});
                sess_add.Parameters.Add(
                  new SqlParameter("userid", SqlDbType.Int){Value = userid});
                await sess_add.ExecuteNonQueryAsync();
              }

              break;
            }
          }
        }
      }

      return guid;
    }
  }
}
