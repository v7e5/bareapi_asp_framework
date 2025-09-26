# Barebones ASP.NET Framework Web API 2 with OWIN

A minimal, self-hosted OWIN-based Web API for .NET Framework 4.8, built from
scratch on top of a Console application, without relying on Visual Studio or
IIS. This project aims to demonstrate how the different ASP.NET components fit
together and integrate with the OWIN/Katana pipeline.

## Features
+ Builds on the bare minimum .NET Framework 4.8 Console application.
+ Self-hosted OWIN-based Web API with all startup logic contained in a single
file (api.cs).
+ Implements a simple cookie based user authentication / session using raw http
headers, backed by a session table in the database.
+ Avoids the complexity and ceremony of Dapper/EFCore/ORMs in favor of raw sql
queries/ADO.NET.

### Notes
+ Developed on MSYS2 using the standalone
[JetBrains redistributable of
MSBuild](https://blog.jetbrains.com/dotnet/2018/04/13/introducing-jetbrains-redistributable-msbuild/).
You can also use the MSBuild executable that is included with Visual Studio if
you have it installed. Just include the full path to the MSBuild exe in a
script, something like this.


```shell
#!/usr/bin/zsh
set -euo pipefail

/c/Program\ Files\ \(x86\)/Microsoft\ Visual\ Studio/2019/Professional/MSBuild/\
Current/Bin/amd64/MSBuild.exe ${@}

# or

# /path to JetBrains redistributable MSBuild/MSBuild.exe ${@}

```
+ Tested with `Microsoft SQL Server 2019 (RTM) - 15.0.2000.5 (X64)`.
+ Needs MultipleActiveResultSets to be enabled, so include
`MultipleActiveResultSets=true` in your connection string.
+ Listening host and port need to be defined in a `app_settings.config` file in
root. See `sample_app_settings.config`.
+ Database connection string to be defined in a `connection_strings.config`
file in root. See `sample_connection_strings.config`.
+  x.sh is a simple shell script is intended to be run in a zsh shell on
MSYS2/Cygwin.
Reading it should help you understand how to use MSBuild to build and run this
project from any cli such as Powershell or cmd.

## Endpoints

### Unauthenticated endpoints

#### hailstone
POST: `/hailstone`

Returns the Collatz (Hailstone) sequence for a given positive integer.


Parameters:
```shell
{
    "n": int (required)
}
```

#### env
POST: `/env`

List of environment variables as key-value pairs

#### now
POST: `/now`

Returns current timestamps sourced from the database (local time, Unix epoch,
formatted timestamp) and the server's UTC time.


#### ecco
POST: `/ecco`

Echoes posted content with an optional delay parameter.

Parameters:
```shell
{
    "delay": int (optional),
    ...
}

```

#### login
POST: `/login`

Example request:
```shell
curl -vs -X POST \
  --cookie ${COOKIE_FILE_PATH} \
  --cookie-jar ${COOKIE_FILE_PATH} \
  -H 'content-type: application/json' \
  -H 'accept: application/json' \
  --data-binary "$(cat <<EOL
{
  "username": string (required),
  "passwd": string (required)
}
EOL
)" \
'http://0.0.0.0:8000/login'

```

### Authenticated endpoints

#### logout
POST: `/logout`

Example request:
```shell
curl -vs -X POST \
  --cookie ${COOKIE_FILE_PATH} \
  --cookie-jar ${COOKIE_FILE_PATH} \
  -H 'content-type: application/json' \
  -H 'accept: application/json' \
'http://0.0.0.0:8000/logout'

```

#### userid
POST: `/userid`

Returns the ID of the current session user.
