:: If IIS Express is installed, run this script to test bank.
SET path=%~dp0
cd /d "C:\Program Files\IIS Express\"
start microsoft-edge:http://localhost:1337
iisexpress.exe /path:%path%bank /port:1337
