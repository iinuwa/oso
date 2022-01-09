.PHONY: rust test package repl lint fmt clean

rust:
	$(MAKE) -C ../.. rust-build
	# @NOTE: this is just the debug lib. Dont use this for release builds.
	if [ -e "../../target/debug/libpolar.dylib" ]; then \
		mkdir -p oso/src/Oso/Native/macos; \
		cp ../../target/debug/libpolar.dylib oso/src/Oso/Native/macos/; \
	fi
	if [ -e "../../target/debug/libpolar.so" ]; then \
		mkdir -p oso/src/Oso/Native/linux; \
		cp ../../target/debug/libpolar.so oso/src/Oso/Native/linux/; \
	fi

build: rust
	cd oso && dotnet build

test: rust
	cd oso && dotnet test 

package: rust
	cd oso/src/Oso && dotnet publish

# repl: rust
#	cd Oso && mvn $(MVN_FLAGS) exec:java -Dexec.mainClass="com.osohq.oso.Oso" -Dexec.args="$(ARGS)"

lint: fmt
	cd oso && dotnet-format --check  --fix-whitespace --fix-style # --fix-analyzers

fmt:
	cd oso && dotnet-format

clean:
	cd oso && dotnet clean
