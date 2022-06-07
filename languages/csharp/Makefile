.PHONY: rust test package repl lint fmt clean

rust:
	$(MAKE) -C ../.. rust-build
	# @NOTE: this is just the debug lib. Dont use this for release builds.
	if [ -e "../../target/x86_64-apple-darwin/debug/libpolar.dylib" ]; then \
		mkdir -p oso/lib/osx-x64; \
		cp ../../target/x86_64-apple-darwin/debug/libpolar.dylib oso/lib/osx-x64/; \
	fi
	if [ -e "../../target/aarch64-apple-darwin/debug/libpolar.dylib" ]; then \
		mkdir -p oso/lib/osx-arm64; \
		cp ../../target/aarch64-apple-darwin/debug/libpolar.dylib oso/lib/osx-arm64/; \
	fi
	if [ -e "../../target/debug/libpolar.so" ]; then \
		mkdir -p oso/lib/linux-x64; \
		cp ../../target/debug/libpolar.so oso/lib/linux-x64/; \
	fi
	if [ -e "../../target/debug/libpolar.dll" ]; then \
		mkdir -p oso/lib/win-x64; \
		cp ../../target/debug/libpolar.dll oso/lib/win-x64/; \
	fi

build: rust
	cd oso && dotnet build

test: rust
	cd oso && dotnet test 

package: rust
	cd oso/src/Oso && dotnet pack

# repl: rust
#	cd Oso && mvn $(MVN_FLAGS) exec:java -Dexec.mainClass="com.osohq.oso.Oso" -Dexec.args="$(ARGS)"

lint: fmt
	cd oso && dotnet-format --check  --fix-whitespace --fix-style # --fix-analyzers

fmt:
	cd oso && dotnet-format

clean:
	cd oso && dotnet clean
