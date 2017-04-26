; 该脚本使用 HM VNISEdit 脚本编辑器向导产生

; 安装程序初始定义常量
!define PRODUCT_DISPNAME "中建八局二公司项目管理云平台客户端"
!define PRODUCT_NAME "CSCEC82Client"
!define PRODUCT_VERSION "1.0.0.110"
!define PRODUCT_PUBLISHER "DBWorld"
!define PRODUCT_WEB_SITE "http://www.dbworld.cn/"

!define APP_REGISTRY "Software\${PRODUCT_NAME}\Client"

SetCompressor lzma

; ------ MUI 现代界面定义 (1.67 版本以上兼容) ------
!include "MUI.nsh"
!include "LogicLib.nsh"
!include "x64.nsh"

; MUI 预定义常量
!define MUI_ABORTWARNING
!define MUI_ICON "logo.ico" ;${NSISDIR}\Contrib\Graphics\Icons\modern-install

;Request application privileges for Windows Vista
RequestExecutionLevel admin

; 欢迎页面
!insertmacro MUI_PAGE_WELCOME
; 许可协议页面
;!insertmacro MUI_PAGE_LICENSE "c:\path\to\licence\YourSoftwareLicence.txt"
  Page directory DirPre
;; 安装目录选择页面
;!insertmacro MUI_PAGE_DIRECTORY
; 安装过程页面
!insertmacro MUI_PAGE_INSTFILES
; 安装完成页面
!insertmacro MUI_PAGE_FINISH

; 安装界面包含的语言设置
!insertmacro MUI_LANGUAGE "SimpChinese"

; 安装预释放文件
!insertmacro MUI_RESERVEFILE_INSTALLOPTIONS
; ------ MUI 现代界面定义结束 ------

Name "${PRODUCT_DISPNAME} ${PRODUCT_VERSION}"
OutFile "${PRODUCT_NAME}.${PRODUCT_VERSION}.Setup.exe"
InstallDir "$PROGRAMFILES\${PRODUCT_NAME}"
;Get installation folder from registry if available
InstallDirRegKey HKCU ${APP_REGISTRY} "INSTDIR"
ShowInstDetails show

Function DirPre
    ; set the INSTDIR to 
	${If} ${RunningX64}
		SetRegView 64
	${Else}
		SetRegView 32
	${EndIf}
    ReadRegStr $0 HKCU ${APP_REGISTRY} "INSTDIR"
	IfErrors NoNeed 0
    ${If} $0 != ""
        StrCpy $INSTDIR $0
		;MessageBox MB_OK "$0"
        Abort
    ;${Else}
    ;    ; get the install dir from reg
    ;    ReadRegStr $0 HKLM "SOFTWARE\Microsoft\InetStp" "PathWWWRoot"
    ;    ${If} $0 != ""
    ;        StrCpy $INSTDIR $0
    ;    ${EndIf}
    ${EndIf}
	
	NoNeed:
		;MessageBox MB_OK "0+ $0"
FunctionEnd


Function CheckAndDownloadDotNet45
# Let's see if the user has the .NET Framework 4.5 installed on their system or not
# Remember: you need Vista SP2 or 7 SP1.  It is built in to Windows 8, and not needed
# In case you're wondering, running this code on Windows 8 will correctly return is_equal
# or is_greater (maybe Microsoft releases .NET 4.5 SP1 for example)

# Set up our Variables
Var /GLOBAL dotNET45IsThere
Var /GLOBAL dotNET_CMD_LINE
Var /GLOBAL EXIT_CODE

ReadRegDWORD $dotNET45IsThere HKLM "SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full" "Release"
IntCmp $dotNET45IsThere 378389 is_equal is_less is_greater

is_equal:
    Goto done_compare_not_needed
is_greater:
    # Useful if, for example, Microsoft releases .NET 4.5 SP1
    # We want to be able to simply skip install since it's not
    # needed on this system
    Goto done_compare_not_needed
is_less:
    Goto done_compare_needed

done_compare_needed:
    #.NET Framework 4.5 install is *NEEDED*

    # Microsoft Download Center EXE:
    # Web Bootstrapper: http://go.microsoft.com/fwlink/?LinkId=225704
    # Full Download: http://go.microsoft.com/fwlink/?LinkId=225702

    # Setup looks for components\dotNET45Full.exe relative to the install EXE location
    # This allows the installer to be placed on a USB stick (for computers without internet connections)
    # If the .NET Framework 4.5 installer is *NOT* found, Setup will connect to Microsoft's website
    # and download it for you

    # Reboot Required with these Exit Codes:
    # 1641 or 3010

    # Command Line Switches:
    # /showrmui /passive /norestart

    # Silent Command Line Switches:
    # /q /norestart


    # Let's see if the user is doing a Silent install or not
    IfSilent is_quiet is_not_quiet

    is_quiet:
        StrCpy $dotNET_CMD_LINE "/q /norestart"
        Goto LookForLocalFile
    is_not_quiet:
        StrCpy $dotNET_CMD_LINE "/showrmui /passive /norestart"
        Goto LookForLocalFile

    LookForLocalFile:
        # Let's see if the user stored the Full Installer
        IfFileExists "$EXEPATH\components\dotNET45Full.exe" do_local_install do_network_install

        do_local_install:
            # .NET Framework found on the local disk.  Use this copy

            ExecWait '"$EXEPATH\components\dotNET45Full.exe" $dotNET_CMD_LINE' $EXIT_CODE
            Goto is_reboot_requested

        # Now, let's Download the .NET
        do_network_install:

            Var /GLOBAL dotNetDidDownload
            NSISdl::download "http://go.microsoft.com/fwlink/?LinkId=225704" "$TEMP\dotNET45Web.exe" $dotNetDidDownload

            StrCmp $dotNetDidDownload success fail
            success:
                ExecWait '"$TEMP\dotNET45Web.exe" $dotNET_CMD_LINE' $EXIT_CODE
                Goto is_reboot_requested

            fail:
                MessageBox MB_OK|MB_ICONEXCLAMATION "无法下载.NET Framework.  ${PRODUCT_DISPNAME} 安装终止!"
				Abort
				;Quit
                Goto done_dotNET_function

            # $EXIT_CODE contains the return codes.  1641 and 3010 means a Reboot has been requested
            is_reboot_requested:
                ${If} $EXIT_CODE = 1641
                ${OrIf} $EXIT_CODE = 3010
                    SetRebootFlag true
				${ElseIf} $EXIT_CODE != 0
					MessageBox MB_OK|MB_ICONEXCLAMATION "无法安装 .NET Framework.  ${PRODUCT_DISPNAME} 安装终止!"
					Abort
					;Quit
                ${EndIf}

done_compare_not_needed:
    # Done dotNET Install
    Goto done_dotNET_function

#exit the function
done_dotNET_function:

FunctionEnd


Function .onInit
   Call CheckAndDownloadDotNet45
 FunctionEnd


!define Source_File_Path "..\bin\Release\Client\*.*"
!define Revit_Plugin_Folder "..\bin\Release\RevitPlugin"

Section "MainSection" SEC01
  SetOutPath "$INSTDIR"
  SetOverwrite ifnewer
  Delete $INSTDIR\*.exe
  File /r  /x "*.pdb" /x "*.vshost.exe.*" /x "Log" /x "*.xml"  ${Source_File_Path} ;/x "*.manifest"
  File logo.ico

  ;Revit插件所在的目录
  File /r ${Revit_Plugin_Folder}
  
  ;Store installation folder
  WriteRegStr HKCU ${APP_REGISTRY} "INSTDIR" $INSTDIR
  
  WriteRegDWORD HKLM "SOFTWARE\Microsoft\Internet Explorer\MAIN\FeatureControl\FEATURE_BROWSER_EMULATION" "${PRODUCT_NAME}.exe" 0x2710 ;0x270F
  
  StrCpy $0 0
	loop:
	  EnumRegKey $1 HKLM SOFTWARE\Motive\M-Files $0
	  StrCmp $1 "" done
	  IntOp $0 $0 + 1
	    WriteRegStr HKLM "SOFTWARE\Motive\M-Files\$1\Client\MFClient" "VolumeLabel" "${PRODUCT_NAME}"
		WriteRegDWORD HKCU "SOFTWARE\Motive\M-Files\$1\Client\MFStatus" "TrayIconVisible" 0
	  Goto loop
	  ;MessageBox MB_YESNO|MB_ICONQUESTION "$1$\n$\nMore?" IDYES loop
	done:

  
  ;Create uninstaller
  WriteUninstaller "$INSTDIR\Uninstall.exe"
  
SectionEnd

Section -AdditionalIcons
  WriteIniStr "$INSTDIR\${PRODUCT_DISPNAME}.url" "InternetShortcut" "URL" "${PRODUCT_WEB_SITE}"
  CreateDirectory "$SMPROGRAMS\${PRODUCT_DISPNAME}"
  CreateShortCut "$SMPROGRAMS\${PRODUCT_DISPNAME}\${PRODUCT_NAME}.lnk" "$INSTDIR\${PRODUCT_NAME}.exe" "" "$INSTDIR\logo.ico"
  CreateShortCut "$SMPROGRAMS\${PRODUCT_DISPNAME}\Website.lnk" "$INSTDIR\${PRODUCT_NAME}.url"
  CreateShortCut "$SMPROGRAMS\${PRODUCT_DISPNAME}\Uninstall.lnk" "$INSTDIR\Uninstall.exe"
  CreateShortCut "$DESKTOP\${PRODUCT_DISPNAME}.lnk" "$INSTDIR\${PRODUCT_NAME}.exe" "" "$INSTDIR\logo.ico"
SectionEnd

Section -Post
SectionEnd

Section "Uninstall"

  ;ADD YOUR OWN FILES HERE...

  Delete "$INSTDIR\Uninstall.exe"

  RMDir /r "$INSTDIR"
  
  DeleteRegKey /ifempty HKCU ${APP_REGISTRY}

  Delete "$DESKTOP\${PRODUCT_DISPNAME}.lnk"
  
  Delete "$SMPROGRAMS\${PRODUCT_DISPNAME}\${PRODUCT_NAME}.lnk"
  
  Delete "$SMPROGRAMS\${PRODUCT_DISPNAME}\Uninstall.lnk"
  
  Delete "$SMPROGRAMS\${PRODUCT_DISPNAME}\Website.lnk"
  
  Delete "$SMPROGRAMS\${PRODUCT_DISPNAME}"

SectionEnd
