#!/usr/bin/bash

### For RRRIX's cygwin script :)

VERSION=`cat /c/db/Plugins/Trinity/Plugin.cs | grep "new Version" | sed -E 's/ +return new Version//g' | sed -E 's/[\(\);]//g' | sed -E 's/, /./g' | sed -E 's/ //g'`

A="TrinityPlugin-"
B=$VERSION
D=".zip"

ZIPFILE=$A$B$D

./CreateZip.cmd


echo Moving ZipFile to ../Builds/$ZIPFILE 

mv -v Latest-Trinity.zip ../Builds/$ZIPFILE
