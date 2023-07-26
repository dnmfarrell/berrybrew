RequestExecutionLevel admin

!include LogicLib.nsh
!include MUI2.nsh
!include nsProcess.nsh

var perlRootDir 
var perlRootDirSet

!define PRODUCT_NAME "berrybrew-staging"
!define PRODUCT_VERSION "1.40"
!define PRODUCT_PUBLISHER "Steve Bertrand"
!define PRODUCT_WEB_SITE "https://github.com/stevieb9/berrybrew"
!define PRODUCT_DIR_REGKEY "Software\Microsoft\Windows\CurrentVersion\App Paths\berrybrew.exe"
!define PRODUCT_UNINST_KEY "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PRODUCT_NAME}"
!define APP_REGKEY "Software\berrybrew-staging"
!define PRODUCT_UNINST_ROOT_KEY "HKLM"

!define MUI_ABORTWARNING
!define MUI_ICON "..\inc\berrybrew.ico"
!define MUI_UNICON "${NSISDIR}\Contrib\Graphics\Icons\modern-uninstall.ico"
!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_LICENSE "..\LICENSE"
!insertmacro MUI_PAGE_DIRECTORY

; Perl root_path directory
!define MUI_PAGE_HEADER_SUBTEXT "Directory to store the Perl instances"
!define MUI_DIRECTORYPAGE_TEXT_TOP "Choose a directory to store the Perl instances"
!define MUI_DIRECTORYPAGE_VARIABLE $perlRootDir
!define MUI_PAGE_CUSTOMFUNCTION_PRE perlRootPathSelection
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
OutFile "..\staging\berrybrewInstaller.exe"
InstallDir "$PROGRAMFILES\berrybrew\staging"
InstallDirRegKey HKLM "${PRODUCT_DIR_REGKEY}" ""
ShowInstDetails show
ShowUnInstDetails show

Section "-MainSection" SEC_MAIN
  SetOverwrite try

  SetOutPath "$INSTDIR"
  File "..\Changes.md"
  File "..\Changes"
  File "..\README.md"
  File "..\LICENSE"
  
  SetOutPath "$INSTDIR\bin"
  File "..\staging\berrybrew-refresh.bat"
  File "..\staging\berrybrew-refresh.bat"
  File "..\staging\bbapi.dll"
  File "..\staging\berrybrew.exe"
  File "..\staging\bb.exe"
  File "..\staging\berrybrew-ui.exe"
  File "..\staging\berrybrewInstaller.exe"
  File "..\staging\ICSharpCode.SharpZipLib.dll"
  File "..\staging\Newtonsoft.Json.dll"

  SetOutPath "$INSTDIR\data"
  File "..\staging\data\config.json"
  File "..\staging\data\messages.json"
  File "..\staging\data\perls.json"

  SetOutPath "$INSTDIR\doc"  
  File "..\doc\berrybrew.md"
  File "..\doc\Configuration.md"
  
  SetOutPath "$INSTDIR\inc"
  File "..\inc\berrybrew.ico"
SectionEnd

Section "Perl 5.38.0_64" SEC_INSTALL_NEWEST_PERL
SectionEnd

Section "Run UI at startup" SEC_START_UI
SectionEnd

Section "Manage .pl file association" SEC_FILE_ASSOC
SectionEnd

Section -AdditionalIcons
  SetOutPath $INSTDIR
  WriteIniStr "$INSTDIR\${PRODUCT_NAME}.url" "InternetShortcut" "URL" "${PRODUCT_WEB_SITE}"
  CreateShortCut "$SMPROGRAMS\berrybrew\Website.lnk" "$INSTDIR\${PRODUCT_NAME}.url"
  CreateShortCut "$SMPROGRAMS\berrybrew\Uninstall.lnk" "$INSTDIR\uninst.exe"
SectionEnd

Section -Post
  WriteUninstaller "$INSTDIR\uninst.exe"
  
  ${If} ${SectionIsSelected} ${SEC_START_UI}
    WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Run" "BerrybrewUI-staging" "$INSTDIR\bin\berrybrew-ui.exe"
  ${EndIf}

  WriteRegStr HKLM "${PRODUCT_DIR_REGKEY}" "" "$INSTDIR\bin\berrybrew.exe"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "DisplayName" "$(^Name)"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "UninstallString" "$INSTDIR\uninst.exe"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "DisplayIcon" "$INSTDIR\bin\berrybrew.exe"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "DisplayVersion" "${PRODUCT_VERSION}"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "URLInfoAbout" "${PRODUCT_WEB_SITE}"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "Publisher" "${PRODUCT_PUBLISHER}"
  
  WriteRegStr HKLM "${PRODUCT_DIR_REGKEY}" "" "$INSTDIR"
SectionEnd

Function perlRootPathSelection

   ; check if the root path for Perls is already set
   
   ClearErrors
   ReadRegStr $0 HKLM "${APP_REGKEY}" "root_dir"
   ${If} ${Errors}
     StrCpy $perlRootDirSet "0"
   ${Else}
       StrCpy $perlRootDir $0
       StrCpy $perlRootDirSet "1"
       Abort
   ${EndIf} 
FunctionEnd     

Function ModifyRunCheckbox
    SendMessage $mui.FinishPage.Run ${BM_SETCHECK} ${BST_CHECKED} 0
    ShowWindow $mui.FinishPage.Run 0
FunctionEnd

Function LaunchFinish
  SetOutPath $INSTDIR
   
   ; set the root_path Perl directory location

  ${If} $perlRootDirSet == "0"
    nsExec::Exec '"$SYSDIR\cmd.exe" /C if 1==1 "$INSTDIR\bin\berrybrew.exe" options-update'
    ClearErrors
    WriteRegStr HKLM "${APP_REGKEY}" "root_dir" $perlRootDir
    WriteRegStr HKLM "${APP_REGKEY}" "temp_dir" "$perlRootDir\temp"
    ${If} ${Errors}
      MessageBox MB_OK "Error writing registry"
    ${EndIf}      
  ${EndIf}  

  nsExec::Exec '"$SYSDIR\cmd.exe" /C if 1==1 "$INSTDIR\bin\berrybrew.exe" debug options-update'
  nsExec::Exec '"$SYSDIR\cmd.exe" /C if 1==1 "$INSTDIR\bin\berrybrew.exe" config'
  nsExec::Exec '"$SYSDIR\cmd.exe" /C if 1==1 "$INSTDIR\bin\berrybrew.exe" register_orphans'

  ${If} ${SectionIsSelected} ${SEC_INSTALL_NEWEST_PERL}
    ${If} ${FileExists} "$perlRootDir\5.38.0_64\perl\bin\perl.exe"
      MessageBox MB_OK "Perl 5.38.0_64 is already installed, we'll switch to it"
    ${Else}
      ExecWait '"$SYSDIR\cmd.exe" /C if 1==1 "$INSTDIR\bin\berrybrew.exe" install 5.38.0_64'
    ${EndIf}
    nsExec::Exec '"$SYSDIR\cmd.exe" /C if 1==1 "$INSTDIR\bin\berrybrew.exe" switch 5.38.0_64'
  ${EndIf}

  ${If} ${SectionIsSelected} ${SEC_FILE_ASSOC}
    nsExec::Exec '"$SYSDIR\cmd.exe" /C if 1==1 "$INSTDIR\bin\berrybrew.exe" associate set'
    nsExec::Exec '"$SYSDIR\cmd.exe" /C if 1==1 "$INSTDIR\bin\berrybrew.exe" associate'
  ${EndIf}   
FunctionEnd

Function .oninstsuccess
  Exec '"$INSTDIR\bin\berrybrew-ui.exe"'
FunctionEnd

Function un.StopUI
    ${nsProcess::FindProcess} "berrybrew-ui.exe" $R0
    ${If} $R0 == 0
        DetailPrint "berrybrew-ui.exe is running. Closing it down"
        ${nsProcess::KillProcess} "berrybrew-ui.exe" $R0
        DetailPrint "Waiting for berrybrew-ui.exe to close"
        Sleep 2000  
    ${Else}
        DetailPrint "berrybrew-ui.exe was not found to be running"        
    ${EndIf}    
    ${nsProcess::Unload}
FunctionEnd

Function StopUI
    ${nsProcess::FindProcess} "berrybrew-ui.exe" $R0
    ${If} $R0 == 0
        DetailPrint "berrybrew-ui.exe is running. Closing it down"
        ${nsProcess::KillProcess} "berrybrew-ui.exe" $R0
        DetailPrint "Waiting for berrybrew-ui.exe to close"
        Sleep 2000  
    ${Else}
        DetailPrint "berrybrew-ui.exe was not found to be running"        
    ${EndIf}    
    ${nsProcess::Unload}
FunctionEnd

Function .onInit
  SetRegView 64

  Call StopUI
      
  StrCpy $perlRootDir "C:\berrybrew\staging"
  StrCpy $perlRootDirSet "0"

  StrCpy $InstDir "$INSTDIR\"

  ; check for previously installed versions

  IfFileExists "$INSTDIR\bin\berrybrew.exe" file_found file_not_found

    file_found:
   
      ReadRegStr $R0 ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "DisplayVersion"
            
;      ${If} ${PRODUCT_VERSION} == $R0
;        MessageBox MB_OK "berrybrew version $R0 already installed. Aborting."
;        Abort
;      ${EndIf}

      MessageBox MB_ICONQUESTION|MB_YESNO "This will upgrade your existing berrybrew install. Continue?" IDYES true IDNO false
      false: 
        Abort
      true:
        nsExec::Exec '"$SYSDIR\cmd.exe" /C if 1==1 "$INSTDIR\bin\berrybrew" off'
        nsExec::Exec '"$SYSDIR\cmd.exe" /C if 1==1 "$INSTDIR\bin\berrybrew" unconfig' 

        goto end_find_file
      
    file_not_found:
  
      nsExec::ExecToStack '"berrybrew" version'
      Pop $1  

      ${If} $1 == 0
        MessageBox MB_ICONQUESTION|MB_YESNO "You have a previous version of berrybrew. Can we try to disable it?" IDYES yep IDNO nope
        nope:
          Abort
        yep:
          nsExec::Exec '"$SYSDIR\cmd.exe" /C if 1==1 "berrybrew" off'
          nsExec::Exec '"$SYSDIR\cmd.exe" /C if 1==1 "berrybrew" unconfig'
      ${EndIf}
    
    end_find_file:      
FunctionEnd

Function un.onUninstSuccess
  HideWindow
  MessageBox MB_ICONINFORMATION|MB_OK "$(^Name) was successfully removed from your computer."
FunctionEnd

Function un.onInit
  SetRegView 64
  MessageBox MB_ICONQUESTION|MB_YESNO|MB_DEFBUTTON2 "Are you sure you want to completely remove $(^Name) and all of its components?" IDYES +2
  Abort
  
  Call un.StopUI
FunctionEnd
  
Section Uninstall
  nsExec::Exec '"$SYSDIR\cmd.exe" /C if 1==1 "$INSTDIR\bin\berrybrew" associate unset'
  nsExec::Exec '"$SYSDIR\cmd.exe" /C if 1==1 "$INSTDIR\bin\berrybrew.exe" off'
  nsExec::Exec '"$SYSDIR\cmd.exe" /C if 1==1 "$INSTDIR\bin\berrybrew.exe" unconfig'
  
  DeleteRegValue HKLM "Software\Microsoft\Windows\CurrentVersion\Run" "BerrybrewUI-staging"
  DeleteRegKey ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}"
  DeleteRegKey HKLM "${PRODUCT_DIR_REGKEY}"
  DeleteRegKey HKLM "${APP_REGKEY}" 

  Delete "$INSTDIR\${PRODUCT_NAME}.url"
  Delete "$INSTDIR\uninst.exe"
  Delete "$INSTDIR\inc\berrybrew.ico"
  Delete "$INSTDIR\data\perls.json"
  Delete "$INSTDIR\data\messages.json"
  Delete "$INSTDIR\data\config.json"
  Delete "$INSTDIR\doc\berrybrew.md"
  Delete "$INSTDIR\doc\Configuration.md"
  Delete "$INSTDIR\berrybrew-refresh.bat"
  Delete "$INSTDIR\Newtonsoft.Json.dll"
  Delete "$INSTDIR\ICSharpCode.SharpZipLib.dll"
  Delete "$INSTDIR\bb.exe"
  Delete "$INSTDIR\bbapi.dll"
  Delete "$INSTDIR\berrybrew.exe"
  Delete "$INSTDIR\berrybrewInstaller.exe"
  Delete "$INSTDIR\berrybrew-ui.exe"
  Delete "$INSTDIR\uninst.exe"
  Delete "$INSTDIR\berrybrew.lnk"
  Delete "$INSTDIR\berrybrew.url"
  Delete "$INSTDIR\Changes.md"
  Delete "$INSTDIR\Changes"
  Delete "$INSTDIR\LICENSE"
  Delete "$INSTDIR\README.md"

  Delete "$SMPROGRAMS\berrybrew\Uninstall.lnk"
  Delete "$SMPROGRAMS\berrybrew\Website.lnk"
  Delete "$DESKTOP\berrybrew.lnk"
  Delete "$SMPROGRAMS\berrybrew\berrybrew.lnk"
  RMDir "$SMPROGRAMS\berrybrew"

  RMDir "$INSTDIR\bin"
  RMDir "$INSTDIR\inc"
  RMDir "$INSTDIR"
  
  SetAutoClose true
SectionEnd
