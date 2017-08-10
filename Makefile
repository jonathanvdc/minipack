.PHONY: exe
exe:
	make -C libminipack flo
	make -C minipack exe

.PHONY: all
all:
	make -C libminipack all
	make -C minipack all

.PHONY: dll
dll:
	make -C libminipack all
	make -C minipack exe

.PHONY: flo
flo:
	make -C libminipack flo
	make -C minipack flo

.PHONY: clean
clean:
	make -C libminipack clean
	make -C minipack clean

.PHONY: install
install: exe
	mono ./minipack/bin/clr/minipack.exe deb-tree minipack.json --version $(MINIPACK_VERSION) --revision $(MINIPACK_REVISION) -fno-include-control -o $(DESTDIR)
