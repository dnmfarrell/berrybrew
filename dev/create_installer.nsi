!include LogicLib.nsh
!include MUI2.nsh
!include "MUI.nsh"

!define PRODUCT_NAME "berrybrew"
!define PRODUCT_VERSION "1.26"
!define PRODUCT_PUBLISHER "Steve Bertrand"
!define PRODUCT_WEB_SITE "https://github.com/stevieb9/berrybrew"
!define PRODUCT_DIR_REGKEY "Software\Microsoft\Windows\CurrentVersion\App Paths\berrybrew.exe"
!define PRODUCT_UNINST_KEY "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PRODUCT_NAME}"
!define PRODUCT_UNINST_ROOT_KEY "HKLM"

!define MUI_ABORTWARNING
!define MUI_ICON "..\Desktop\berrybrew\inc\berrybrew.ico"
!define MUI_UNICON "${NSISDIR}\Contrib\Graphics\Icons\modern-uninstall.ico"

!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_LICENSE "..\Desktop\berrybrew\LICENSE"
!insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_INSTFILES
!insertmacro MUI_PAGE_COMPONENTS

!define MUI_FINISHPAGE_RUN
!define MUI_PAGE_CUSTOMFUNCTION_SHOW ModifyRunCheckbox
!define MUI_FINISHPAGE_RUN_FUNCTION "LaunchFinish"
!insertmacro MUI_PAGE_FINISH

!insertmacro MUI_UNPAGE_INSTFILES
!insertmacro MUI_LANGUAGE "English"

Name "${PRODUCT_NAME} ${PRODUCT_VERSION}"
OutFile "Setup.exe"
InstallDir "$PROGRAMFILES\berrybrew"
InstallDirRegKey HKLM "${PRODUCT_DIR_REGKEY}" ""
ShowInstDetails show
ShowUnInstDetails show

Section "-MainSection" SEC01
  SetOutPath "$PROGRAMFILES\berrybrew"
  SetOverwrite try
  File "..\Desktop\berrybrew\.gitignore"
  SetOutPath "$PROGRAMFILES\berrybrew\bin"
  File "..\Desktop\berrybrew\bin\bbapi.dll"
  File "..\Desktop\berrybrew\bin\berrybrew.exe"
  CreateDirectory "$SMPROGRAMS\berrybrew"
  CreateShortCut "$SMPROGRAMS\berrybrew\berrybrew.lnk" "$PROGRAMFILES\berrybrew\bin\berrybrew.exe"
  CreateShortCut "$DESKTOP\berrybrew.lnk" "$PROGRAMFILES\berrybrew\bin\berrybrew.exe"
  File "..\Desktop\berrybrew\bin\ICSharpCode.SharpZipLib.dll"
  File "..\Desktop\berrybrew\bin\Newtonsoft.Json.dll"
  SetOutPath "$PROGRAMFILES\berrybrew"
  File "..\Desktop\berrybrew\Changes"
  File "..\Desktop\berrybrew\Changes.md"
  File "..\Desktop\berrybrew\CONTRIBUTING.md"
  SetOutPath "$PROGRAMFILES\berrybrew\data"
  File "..\Desktop\berrybrew\data\config.json"
  File "..\Desktop\berrybrew\data\messages.json"
  File "..\Desktop\berrybrew\data\perls.json"
  SetOutPath "$PROGRAMFILES\berrybrew\dev"
  File "..\Desktop\berrybrew\dev\build.bat"
  File "..\Desktop\berrybrew\dev\build_tests.bat"
  SetOutPath "$PROGRAMFILES\berrybrew\dev\data"
  File "..\Desktop\berrybrew\dev\data\config.json"
  File "..\Desktop\berrybrew\dev\data\messages.json"
  File "..\Desktop\berrybrew\dev\data\perls.json"
  SetOutPath "$PROGRAMFILES\berrybrew\dev"
  File "..\Desktop\berrybrew\dev\env_var_refresh.bat"
  File "..\Desktop\berrybrew\dev\post_release.pl"
  File "..\Desktop\berrybrew\dev\release.pl"
  SetOutPath "$PROGRAMFILES\berrybrew\doc"
  File "..\Desktop\berrybrew\doc\Berrybrew API.md"
  File "..\Desktop\berrybrew\doc\berrybrew.md"
  File "..\Desktop\berrybrew\doc\Compile Your Own.md"
  File "..\Desktop\berrybrew\doc\Configuration.md"
  File "..\Desktop\berrybrew\doc\Create a Development Build.md"
  File "..\Desktop\berrybrew\doc\Create a Release.md"
  File "..\Desktop\berrybrew\doc\Unit Testing.md"
;  SetOutPath "$PROGRAMFILES\berrybrew\download"
;  File "..\Desktop\berrybrew\download\berrybrew.zip"
  SetOutPath "$PROGRAMFILES\berrybrew\inc"
  File "..\Desktop\berrybrew\inc\berrybrew.ico"
  File "..\Desktop\berrybrew\inc\Setup.exe"
  File "..\Desktop\berrybrew\inc\test.nsi"
  File "..\Desktop\berrybrew\inc\Untitled 08.nsi"
  SetOutPath "$PROGRAMFILES\berrybrew"
  File "..\Desktop\berrybrew\LICENSE"
  File "..\Desktop\berrybrew\README.md"
  SetOutPath "$PROGRAMFILES\berrybrew\src"
  File "..\Desktop\berrybrew\src\bbconsole.cs"
  File "..\Desktop\berrybrew\src\berrybrew.cs"
  SetOutPath "$PROGRAMFILES\berrybrew\t"
  File "..\Desktop\berrybrew\t\05-available.t"
  File "..\Desktop\berrybrew\t\10-homedir.t"
  File "..\Desktop\berrybrew\t\15-install.t"
  File "..\Desktop\berrybrew\t\20-switch.t"
  File "..\Desktop\berrybrew\t\25-env_vars.t"
  File "..\Desktop\berrybrew\t\30-orphan_perls.t"
  File "..\Desktop\berrybrew\t\35-custom_install.t"
  File "..\Desktop\berrybrew\t\40-name_check.t"
  File "..\Desktop\berrybrew\t\45-template_exec_skip.t"
  File "..\Desktop\berrybrew\t\50-custom_exec_skip.t"
  File "..\Desktop\berrybrew\t\55-register.t"
  File "..\Desktop\berrybrew\t\60-use.t"
  File "..\Desktop\berrybrew\t\70-modules.t"
  File "..\Desktop\berrybrew\t\95-remove.t"
  File "..\Desktop\berrybrew\t\99-clean.t"
  File "..\Desktop\berrybrew\t\BB.pm"
  SetOutPath "$PROGRAMFILES\berrybrew\t\data"
  File "..\Desktop\berrybrew\t\data\available.txt"
  File "..\Desktop\berrybrew\t\data\custom_available.txt"
  SetOutPath "$PROGRAMFILES\berrybrew\t"
  File "..\Desktop\berrybrew\t\run_tests.pl"
  File "..\Desktop\berrybrew\t\setup_test_env.bat"
  File "..\Desktop\berrybrew\t\test.bat"
  File "..\Desktop\berrybrew\t\unset_test_env.bat"
SectionEnd

Section "Install Perl 5.30.0" SEC02
SectionEnd

Section -AdditionalIcons
  SetOutPath $INSTDIR
  WriteIniStr "$INSTDIR\${PRODUCT_NAME}.url" "InternetShortcut" "URL" "${PRODUCT_WEB_SITE}"
  CreateShortCut "$SMPROGRAMS\berrybrew\Website.lnk" "$INSTDIR\${PRODUCT_NAME}.url"
  CreateShortCut "$SMPROGRAMS\berrybrew\Uninstall.lnk" "$INSTDIR\uninst.exe"
SectionEnd

Section -Post
  WriteUninstaller "$INSTDIR\uninst.exe"
  WriteRegStr HKLM "${PRODUCT_DIR_REGKEY}" "" "$PROGRAMFILES\berrybrew\bin\berrybrew.exe"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "DisplayName" "$(^Name)"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "UninstallString" "$INSTDIR\uninst.exe"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "DisplayIcon" "$PROGRAMFILES\berrybrew\bin\berrybrew.exe"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "DisplayVersion" "${PRODUCT_VERSION}"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "URLInfoAbout" "${PRODUCT_WEB_SITE}"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "Publisher" "${PRODUCT_PUBLISHER}"
SectionEnd

Function ModifyRunCheckbox
    SendMessage $mui.FinishPage.Run ${BM_SETCHECK} ${BST_CHECKED} 0
    ShowWindow $mui.FinishPage.Run 0
FunctionEnd

Function LaunchFinish
  SetOutPath $INSTDIR
  Exec '"$SYSDIR\cmd.exe" /C "berrybrew.exe" config'

  ${If} ${SectionIsSelected} ${SEC02}
    Exec '"$SYSDIR\cmd.exe" /C "berrybrew.exe" install 5.30.0_64'
    Exec '"$SYSDIR\cmd.exe" /C "berrybrew.exe" switch 5.30.0_64'
  ${EndIf}
FunctionEnd

Function un.onUninstSuccess
  HideWindow
  MessageBox MB_ICONINFORMATION|MB_OK "$(^Name) was successfully removed from your computer."
FunctionEnd

Function un.onInit
  MessageBox MB_ICONQUESTION|MB_YESNO|MB_DEFBUTTON2 "Are you sure you want to completely remove $(^Name) and all of its components?" IDYES +2
  Abort
FunctionEnd

Section Uninstall
  Delete "$INSTDIR\${PRODUCT_NAME}.url"
  Delete "$INSTDIR\uninst.exe"
  Delete "$PROGRAMFILES\berrybrew\t\unset_test_env.bat"
  Delete "$PROGRAMFILES\berrybrew\t\test.bat"
  Delete "$PROGRAMFILES\berrybrew\t\setup_test_env.bat"
  Delete "$PROGRAMFILES\berrybrew\t\run_tests.pl"
  Delete "$PROGRAMFILES\berrybrew\t\data\custom_available.txt"
  Delete "$PROGRAMFILES\berrybrew\t\data\available.txt"
  Delete "$PROGRAMFILES\berrybrew\t\BB.pm"
  Delete "$PROGRAMFILES\berrybrew\t\99-clean.t"
  Delete "$PROGRAMFILES\berrybrew\t\95-remove.t"
  Delete "$PROGRAMFILES\berrybrew\t\70-modules.t"
  Delete "$PROGRAMFILES\berrybrew\t\60-use.t"
  Delete "$PROGRAMFILES\berrybrew\t\55-register.t"
  Delete "$PROGRAMFILES\berrybrew\t\50-custom_exec_skip.t"
  Delete "$PROGRAMFILES\berrybrew\t\45-template_exec_skip.t"
  Delete "$PROGRAMFILES\berrybrew\t\40-name_check.t"
  Delete "$PROGRAMFILES\berrybrew\t\35-custom_install.t"
  Delete "$PROGRAMFILES\berrybrew\t\30-orphan_perls.t"
  Delete "$PROGRAMFILES\berrybrew\t\25-env_vars.t"
  Delete "$PROGRAMFILES\berrybrew\t\20-switch.t"
  Delete "$PROGRAMFILES\berrybrew\t\15-install.t"
  Delete "$PROGRAMFILES\berrybrew\t\10-homedir.t"
  Delete "$PROGRAMFILES\berrybrew\t\05-available.t"
  Delete "$PROGRAMFILES\berrybrew\src\berrybrew.cs"
  Delete "$PROGRAMFILES\berrybrew\src\bbconsole.cs"
  Delete "$PROGRAMFILES\berrybrew\README.md"
  Delete "$PROGRAMFILES\berrybrew\LICENSE"
  Delete "$PROGRAMFILES\berrybrew\inc\Untitled 08.nsi"
  Delete "$PROGRAMFILES\berrybrew\inc\test.nsi"
  Delete "$PROGRAMFILES\berrybrew\inc\Setup.exe"
  Delete "$PROGRAMFILES\berrybrew\inc\berrybrew.ico"
  Delete "$PROGRAMFILES\berrybrew\download\berrybrew.zip"
  Delete "$PROGRAMFILES\berrybrew\doc\Unit Testing.md"
  Delete "$PROGRAMFILES\berrybrew\doc\Create a Release.md"
  Delete "$PROGRAMFILES\berrybrew\doc\Create a Development Build.md"
  Delete "$PROGRAMFILES\berrybrew\doc\Configuration.md"
  Delete "$PROGRAMFILES\berrybrew\doc\Compile Your Own.md"
  Delete "$PROGRAMFILES\berrybrew\doc\berrybrew.md"
  Delete "$PROGRAMFILES\berrybrew\doc\Berrybrew API.md"
  Delete "$PROGRAMFILES\berrybrew\dev\release.pl"
  Delete "$PROGRAMFILES\berrybrew\dev\post_release.pl"
  Delete "$PROGRAMFILES\berrybrew\dev\env_var_refresh.bat"
  Delete "$PROGRAMFILES\berrybrew\dev\data\perls.json"
  Delete "$PROGRAMFILES\berrybrew\dev\data\messages.json"
  Delete "$PROGRAMFILES\berrybrew\dev\data\config.json"
  Delete "$PROGRAMFILES\berrybrew\dev\build_tests.bat"
  Delete "$PROGRAMFILES\berrybrew\dev\build.bat"
  Delete "$PROGRAMFILES\berrybrew\data\perls.json"
  Delete "$PROGRAMFILES\berrybrew\data\messages.json"
  Delete "$PROGRAMFILES\berrybrew\data\config.json"
  Delete "$PROGRAMFILES\berrybrew\CONTRIBUTING.md"
  Delete "$PROGRAMFILES\berrybrew\Changes.md"
  Delete "$PROGRAMFILES\berrybrew\Changes"
  Delete "$PROGRAMFILES\berrybrew\bin\Newtonsoft.Json.dll"
  Delete "$PROGRAMFILES\berrybrew\bin\ICSharpCode.SharpZipLib.dll"
  Delete "$PROGRAMFILES\berrybrew\bin\berrybrew.exe"
  Delete "$PROGRAMFILES\berrybrew\bin\bbapi.dll"
  Delete "$PROGRAMFILES\berrybrew\.gitignore"

  Delete "$SMPROGRAMS\berrybrew\Uninstall.lnk"
  Delete "$SMPROGRAMS\berrybrew\Website.lnk"
  Delete "$DESKTOP\berrybrew.lnk"
  Delete "$SMPROGRAMS\berrybrew\berrybrew.lnk"

  RMDir "$SMPROGRAMS\berrybrew"
  RMDir "$PROGRAMFILES\berrybrew\t\data"
  RMDir "$PROGRAMFILES\berrybrew\t"
  RMDir "$PROGRAMFILES\berrybrew\src"
  RMDir "$PROGRAMFILES\berrybrew\inc"
  RMDir "$PROGRAMFILES\berrybrew\download"
  RMDir "$PROGRAMFILES\berrybrew\doc"
  RMDir "$PROGRAMFILES\berrybrew\dev\data"
  RMDir "$PROGRAMFILES\berrybrew\dev"
  RMDir "$PROGRAMFILES\berrybrew\data"
  RMDir "$PROGRAMFILES\berrybrew\bin"
  RMDir "$PROGRAMFILES\berrybrew"

  DeleteRegKey ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}"
  DeleteRegKey HKLM "${PRODUCT_DIR_REGKEY}"
  SetAutoClose true
SectionEnd
