#!/usr/bin/bash

### For RRRIX's cygwin script :)

GILESVERSION=`cat /c/db/Plugins/GilesTrinity/Plugin.cs | grep "new Version" | sed -E 's/ +return new Version//g' | sed -E 's/[\(\);]//g' | sed -E 's/, /./g'`

A="TrinityPlugin-"
B=$GILESVERSION
D=".zip"

ZIPFILE=$A$B$D

./CreateZip.cmd

mv Latest-GilesTrinity.zip $ZIPFILE
