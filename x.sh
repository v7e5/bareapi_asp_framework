#!/usr/bin/zsh
set -euo pipefail

#cke='./cookie'
cke='./cookie_0'

qe='{
  "n": 11,
  "delay" : 10000,
  "k": "v",
  "x": [1, 2, "3", "x"]
}'

req() {
  [[ ! -f ${cke} ]] && touch ${cke}

  local a=(
    now
    'login'
    hailstone
    'env'
    'echo'
  )

  curl -vs -X POST \
    --cookie ${cke} \
    --cookie-jar ${cke} \
    -H 'content-type: application/json' \
    -H 'accept: application/json' \
    --data ${qe} \
    'http://172.16.1.11:902/'${a[1]} | jq
}

purge() {
  rm -rf bin obj
}

cln() {
  msbuild api.csproj \
    -v:q \
    -t:clean > /dev/null
    #-maxcpucount:4 \
}

bld() {
  msbuild api.csproj \
    -v:q \
    -t:build
    #-maxcpucount:4 \
    #-p:WarningLevel=0 \
}

xxx() {
  pushd bin/Release/
    echo -e '\e[0;96m||||||||||||||||||||||||||||||||||||||||||||||||||\e[0m\n'
    ./api.exe
  popd
}

update() {
  exit
  nuget install Microsoft.AspNet.WebApi.Owin -OutputDirectory packages
  nuget install Microsoft.Owin.Hosting -OutputDirectory packages
  nuget install Microsoft.Owin.Host.HttpListener -OutputDirectory packages
}

typeset -A opts
for k in ${(k)functions}; do
  opts+=(["$k[1]"]=$k)
done

[[ -z ${@} ]] && \
  paste -d ' ' <(print -l '\-'${(j:\n-:k)opts}) <(print -l $opts) && exit

while getopts ${(j::k)opts} opt ${@}; do
  $opts[$opt] ${@}
done
shift $(($OPTIND -1))

exit
