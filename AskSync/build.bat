@echo on
cls
paket.bootstrapper.exe
paket install
packages\FAKE\tools\Fake.exe build.fsx
pause