REM tasklist /FI "IMAGENAME eq aiadb.exe" 2>NUL | find /I /N "aiadb.exe">NUL
REM if "%ERRORLEVEL%"=="0" taskkill /IM aiadb.exe /F