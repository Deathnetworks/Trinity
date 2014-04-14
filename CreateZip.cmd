
REM ***********************************************
REM **     You have change version number ?      **
REM **  Otherwise change before press Enter key  **
REM ***********************************************

PAUSE

rd /S /Q "C:\db\Projects\Builds\Trinity\"

REM Clean Directory
rd /S /Q "Trinity\bin"
rd /S /Q "Trinity\obj"
rd /S /Q "Trinity\Logs"
del /F /S /Q "Trinity\ItemRules\Log\*"

REM Clean Old Zip file
del Latest-Trinity.zip

REM Create Temp Directory and pull source inside
xcopy /E /Y "Trinity\*.cs"   "C:\db\Projects\Builds\Trinity\"
xcopy /E /Y "Trinity\*.dis"  "C:\db\Projects\Builds\Trinity\"
xcopy /E /Y "Trinity\*.xaml" "C:\db\Projects\Builds\Trinity\"
xcopy /E /Y "Trinity\*.xml"  "C:\db\Projects\Builds\Trinity\"
xcopy /E /Y "Trinity\*.xsd"  "C:\db\Projects\Builds\Trinity\"
xcopy /E /Y "Trinity\*.txt"  "C:\db\Projects\Builds\Trinity\"

REM Copy to SVN
xcopy /E /Y "C:\db\Projects\Builds\Trinity\" "C:\db\svn\Trinity\trunk\Trinity\"

REM Zip fresh directory
7za.exe a Latest-Trinity.zip "C:\db\Projects\Builds\Trinity\" -mx9
