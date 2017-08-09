.PHONY: exe
exe:
	make -C minipack exe

.PHONY: all
all:
	make -C minipack all

.PHONY: flo
flo:
	make -C minipack flo

.PHONY: clean
clean:
	make -C minipack clean