.PHONY: help server server-release build build-release format install-signalr health clean

# Default target
help: ## Show this help
	@grep -E '^[a-zA-Z_-]+:.*?## .*$$' $(MAKEFILE_LIST) | sort | awk 'BEGIN {FS = ":.*?## "}; {printf "\033[36m%-18s\033[0m %s\n", $$1, $$2}'

# Server targets
server: ## Run server in development mode
	cd Server && dotnet run

server-release: build-release ## Run server in release mode
	cd Server && dotnet run -c Release

build: ## Build server (debug)
	cd Server && dotnet build

build-release: ## Build server (release)
	cd Server && dotnet build -c Release

format: ## Format server code
	cd Server && dotnet format SignalRServer.csproj

health: ## Check server health endpoint
	@curl -sf http://localhost:5000/health && echo " ✓ Server is healthy" || echo " ✗ Server not responding"

# Unity targets
install-signalr: ## Install SignalR DLLs for Unity (requires pwsh, dotnet)
	rm -rf Unity/Assets/Plugins/SignalR/lib/dll
	cd Unity/Assets/Plugins/SignalR/lib && pwsh ./signalr.ps1

# Cleanup
clean: ## Clean all build artifacts
	cd Server && dotnet clean
	rm -rf Server/bin Server/obj
	rm -rf Unity/Assets/Plugins/SignalR/lib/dll
	rm -rf Unity/Assets/Plugins/SignalR/lib/temp
