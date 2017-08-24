ifeq ($(ECSC),)
ifneq ($(wildcard ecsc/.),)
	ECSC=mono ecsc/src/ecsc/bin/Release/ecsc.exe
else
	ECSC=ecsc
endif
endif