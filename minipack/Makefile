include ../use-ecsc.mk

files = \
	*.cs \
	Properties/*.cs \
	../libminipack/bin/flo/libminipack.flo

libs = \
	../packages/Mono.Cecil.0.9.6.4/lib/net45/Mono.Cecil.dll \
	../packages/Mono.Cecil.0.9.6.4/lib/net45/Mono.Cecil.Mdb.dll \
	../packages/Mono.Cecil.0.9.6.4/lib/net45/Mono.Cecil.Pdb.dll \
	../packages/Mono.Cecil.0.9.6.4/lib/net45/Mono.Cecil.Rocks.dll \
	../packages/Loyc.Essentials.24.3.0/lib/net45/Loyc.Essentials.dll \
	../packages/Loyc.Collections.24.3.0/lib/net45/Loyc.Collections.dll \
	../packages/Loyc.Syntax.24.3.0/lib/net45/Loyc.Syntax.dll \
	../packages/Loyc.Binary.0.1.8/lib/net45/Loyc.Binary.dll \
	../packages/Flame.Compiler.0.9.17/lib/net45/*.dll \
	../packages/Flame.Optimization.0.9.17/lib/net45/*.dll \
	../packages/Flame.Front.0.9.17/lib/net45/*.dll \
	../packages/Newtonsoft.Json.10.0.3/lib/net45/Newtonsoft.Json.dll

common_options = -asm-name=minipack -asm-version=0.1.0.0 -Wall -Wextra -fno-whole-program

all: exe flo

exe: bin/clr/minipack.exe
flo: bin/flo/minipack.flo

clean:
	rm -rf bin/
	rm -rf obj/

bin/clr/minipack.exe: $(files) | ecsc
	$(ECSC) $(files) -platform clr --libs $(libs) -o $@ -O3 $(common_options)

bin/flo/minipack.flo: $(files) | ecsc
	$(ECSC) $(files) -platform ir -runtime clr --libs $(libs) -o $@ $(common_options)
