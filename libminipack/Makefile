include ../use-ecsc.mk

files = \
	*.cs \
	Properties/*.cs

libs = \
	../packages/Newtonsoft.Json.10.0.3/lib/net45/Newtonsoft.Json.dll

common_options = -asm-name=libminipack -asm-version=0.1.0.0 -Wall -Wextra

all: dll flo

dll: bin/clr/libminipack.dll
flo: bin/flo/libminipack.flo

clean:
	rm -rf bin/
	rm -rf obj/

bin/clr/libminipack.dll: $(files) | ecsc
	$(ECSC) $(files) -platform clr --libs $(libs) -o $@ -O3 $(common_options)

bin/flo/libminipack.flo: $(files) | ecsc
	$(ECSC) $(files) -platform ir -runtime clr --libs $(libs) -o $@ $(common_options)
