using System;
using System.Data;
using System.Linq;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Runtime.CompilerServices;

namespace util {
  static class Util {
    public static void cl<T>(T x) {
      Console.WriteLine(x);
    }

    public static void z<T>(this T t) {
      Console.WriteLine(t);
    }

    public static IEnumerable<int> collatz(IEnumerable<int> a) =>
      (a.Last() is int n && n == 1) ?
        a : collatz(a.Append(n % 2 == 0 ? n/2 : (3*n)+1));

    public static IDictionary<string, string> env() =>
      Environment
        .GetEnvironmentVariables()
        .Cast<System.Collections.DictionaryEntry>()
        .ToDictionary(kv => kv.Key.ToString(), kv => kv.Value.ToString());

    static IEnumerable<T> _reverse<T>(
      this IEnumerable<T> source, bool reverse = true
    ) => reverse ? source.Reverse() : source;

    public static IDictionary<string, object>[] ToDictArray(
      this SqlDataReader reader, bool reverse = false
    ) => reader
      .Cast<IDataRecord>()
      .Select(e => Enumerable.Range(0, e.FieldCount)
        .ToDictionary(e.GetName,  (int i) =>
          (e.GetValue(i) is var x && x == DBNull.Value) ? null : x))
      ._reverse(reverse)
      .ToArray();

    [MethodImpl(
      MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization
    )]
    public static bool FixedTimeEquals(
      ReadOnlySpan<byte> left,
      ReadOnlySpan<byte> right
    ) {
      if (left.Length != right.Length) {
        return false;
      }

      int length = left.Length;
      int accum = 0;
      for (int i = 0; i < length; i++) {
        accum |= left[i] - right[i];
      }

      return accum == 0;
    }

    public static byte[] deriveKey(string password, byte[] salt) =>
      (new Rfc2898DeriveBytes(
        password: password,
        salt: salt,
        iterations: 100000,
        hashAlgorithm: HashAlgorithmName.SHA512
      )).GetBytes(32);

  }
}
