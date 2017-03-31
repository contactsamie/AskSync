@echo on
cls
paket.bootstrapper.exe
paket restore --force
packages\FAKE\tools\Fake.exe build.fsx
pause