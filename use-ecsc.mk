# This file defines an 'ecsc' target on which makefiles can rely.
# If the ecsc compiler is not installed, then it is automatically
# installed locally. The 'ECSC' variable is set to a command that
# runs ecsc.
#
# Rules depending on the 'ecsc' target defined by this file should
# use an order-only dependency. Here's some example usage. (use a
# tab for indentation instead of spaces if you include this in a
# makefile).
#
#     include /path/to/use-ecsc.mk
#
#     out.exe: code.cs | ecsc
#         $(ECSC) code.cs -platform clr -o out.exe
#

ROOT_DIR:=$(shell dirname $(realpath $(lastword $(MAKEFILE_LIST))))

ifeq ($(ECSC),)
ifneq ($(wildcard $(ROOT_DIR)/ecsc/src/ecsc/bin/Release/ecsc.exe),)
# If we have a local ecsc install, then we'll use that.
ECSC=mono $(ROOT_DIR)/ecsc/src/ecsc/bin/Release/ecsc.exe
else
ifneq ($(shell which ecsc),)
# If ecsc is installed globally, then that's fine too.
ECSC=ecsc
else
# Otherwise, we'll install ecsc locally.
ECSC_BUILD_COMMAND= \
	cd $(ROOT_DIR); \
	if [ ! -d ecsc ]; then \
		git clone --depth=5 https://github.com/jonathanvdc/ecsc; \
	fi; \
	nuget restore ecsc/src/ecsc.sln; \
	msbuild /p:Configuration=Release /verbosity:quiet ecsc/src/ecsc.sln; \
	cd $(CUR_DIR)
ECSC=mono $(ROOT_DIR)/ecsc/src/ecsc/bin/Release/ecsc.exe
endif
endif
endif

.PHONY: ecsc
ecsc: ; $(ECSC_BUILD_COMMAND)