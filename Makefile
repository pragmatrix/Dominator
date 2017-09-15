DATE:=$(shell date +'%Y%m%d%H%M')
package=Dominator10_${DATE}

.PHONY: all
all:
	echo "no target"

.PHONY: own
own:
	chown -R ${USERNAME} *

conf=Release
msbuild=msbuild.exe /verbosity:m /nologo /p:Configuration=${conf}

# don't want to expose our actual build path, so we build in the tmp directory

package_files=Dominator.Net.dll ToggleSwitch.dll "Windows 10 Dominator.exe" "Windows 10 Dominator.exe.config" "Settings/skype-ads.txt" "Settings/telemetry.txt" "de/Windows 10 Dominator.resources.dll" 

builddir=${package}_build

appdir=Dominator.Windows10
toolsdir=${appdir}/Tools
settingsdir=${appdir}/Settings

.PHONY: package
package:
	rm -rf /tmp/${builddir}
	mkdir -p /tmp/${builddir}
	cp -R * /tmp/${builddir}
	make -C /tmp/${builddir} build 
	
	rm -rf /tmp/${package}
	mkdir -p /tmp/${package}
	cd /tmp/${builddir}/Dominator.Windows10/bin/${conf} && cp --parents ${package_files} /tmp/${package}/
	rm -f /tmp/${package}.zip
	cd /tmp && zip -r ${package}.zip ${package}

.PHONY: build
build:
	${msbuild} Dominator.sln /t:"Clean"
	${msbuild} Dominator.sln /t:"Build"
	


