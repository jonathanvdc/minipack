# This file defines an 'ecsc' target on which makefiles can rely.
# If the ecsc compiler is not installed, then it is automatically
# installed locally. The 'ECSC' variable is set to a command that
# runs ecsc.
#
# Rules depending on the 'ecsc' target defined by this file should
# use an order-only dependency. Here's some example usage. Use a
# tab for indentation instead of spaces if you include this in a
# makefile.
#
#     include /path/to/use-ecsc.mk
#
#     out.exe: code.cs | ecsc
#         $(ECSC) code.cs -platform clr -o out.exe
#
# You may also want to extend your 'make clean' target by adding
# a dependency on 'clean-ecsc'. That'll ensure that a local 'ecsc'
# install is deleted when 'make clean' is run.
#
#     include /path/to/use-ecsc.mk
#
#     .PHONY: clean
#     clean: clean-ecsc
#         ...
#

ECSC_GIT_REPO:=https://github.com/jonathanvdc/ecsc

ROOT_DIR:=$(shell dirname $(realpath $(lastword $(MAKEFILE_LIST))))
TOOLCHAIN_DIR:=$(ROOT_DIR)/toolchain
LOCAL_ECSC_DIR:=$(TOOLCHAIN_DIR)/ecsc
LOCAL_ECSC_SLN:=$(LOCAL_ECSC_DIR)/src/ecsc.sln
LOCAL_ECSC_EXE:=$(LOCAL_ECSC_DIR)/src/ecsc/bin/Release/ecsc.exe

# Only try to define ECSC if it hasn't been defined already.
# A user might want to set ECSC explicitly from the command-line
# or an environment variable and we should respect that.
ifeq ($(ECSC),)
ifneq ($(wildcard $(LOCAL_ECSC_EXE)),)
# If we have a local ecsc install, then we'll use that.
ECSC_DEPENDENCIES=local-ecsc
ECSC=mono "$(LOCAL_ECSC_EXE)"
else
ifneq ($(shell which ecsc),)
# If ecsc is installed globally, then that's fine too.
ECSC_DEPENDENCIES=
ECSC=ecsc
else
# Otherwise, we'll install ecsc locally.
ECSC_DEPENDENCIES=local-ecsc
ECSC=mono "$(LOCAL_ECSC_EXE)"
endif
endif
endif

# 'ecsc' uses a global ecsc command if possible and installs a local
# copy of ecsc if necessary.
.PHONY: ecsc
ecsc: $(ECSC_DEPENDENCIES)

# 'local-ecsc' explicitly installs a local copy of ecsc.
.PHONY: local-ecsc
local-ecsc: $(LOCAL_ECSC_EXE)

$(LOCAL_ECSC_EXE):
	cd "$(ROOT_DIR)"
	if [ ! -d "$(LOCAL_ECSC_DIR)" ]; then \
		git clone --depth=1 $(ECSC_GIT_REPO) "$(LOCAL_ECSC_DIR)"; \
	fi
	nuget restore "$(LOCAL_ECSC_SLN)" -Verbosity quiet
	msbuild /p:Configuration=Release /verbosity:quiet /nologo "$(LOCAL_ECSC_SLN)"

# 'clean-ecsc' deletes a local copy of ecsc, if one has been created.
.PHONY: clean-ecsc
clean-ecsc:
ifneq ($(wildcard $(LOCAL_ECSC_DIR)),)
	rm -rf "$(LOCAL_ECSC_DIR)"
endif
