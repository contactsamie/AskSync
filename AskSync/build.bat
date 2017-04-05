@echo on
cls
.\paket auto-restore on
.paket\paket.bootstrapper.exe
.paket\paket update --redirects  --createnewbindingfiles
packages\FAKE\tools\Fake.exe build.fsx
pause