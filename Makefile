# Makefile para tareas comunes de desarrollo
# hice esto con claude pq me da flojera apreneder makefile
DOTNET_DIR := ./backend
CONTAINER_NAME := bolsafeucn-docker

# Valores por defecto en caso de que no se encuentren en appsettings.json:
POSTGRES_USER := bolsafeucn-user
POSTGRES_PASSWORD := bolsafeucn-password
POSTGRES_DB := bolsafeucn-db
# Valores extraídos de appsettings.json (si existen):
APPSETTINGS_FILE := $(DOTNET_DIR)/appsettings.json # Pueden cambiar el appsettings.json por sus variables.
CONNECTION_STRING := $(shell grep -o '"DefaultConnection": *"[^"]*"' $(APPSETTINGS_FILE) | sed 's/"DefaultConnection": *"\(.*\)"/\1/')
POSTGRES_USER := $(shell echo '$(CONNECTION_STRING)' | grep -o 'Username=[^;]*' | cut -d= -f2)
POSTGRES_PASSWORD := $(shell echo '$(CONNECTION_STRING)' | grep -o 'Password=[^;]*' | cut -d= -f2)
POSTGRES_DB := $(shell echo '$(CONNECTION_STRING)' | grep -o 'Database=[^;]*' | cut -d= -f2)

.PHONY: help db-restart db-migrate run watch docker-create docker-rm docker-start docker-stop

help:
	@echo "Makefile - targets disponibles:"
	@echo "  make db-restart      -> drop, update DB y ejecutar dotnet watch (no-hot-reload)"
	@echo "  make db-migrate      -> drop DB, crear nueva migracion y actualizar DB"
	@echo "  make run             -> ejecutar dotnet run"
	@echo "  make watch           -> ejecutar dotnet watch (no-hot-reload)"
	@echo "  make docker-create   -> crear y ejecutar contenedor PostgreSQL"
	@echo "  make docker-rm       -> detener y eliminar contenedor PostgreSQL"
	@echo "  make docker-start    -> iniciar contenedor PostgreSQL existente"
	@echo "  make docker-stop     -> detener contenedor PostgreSQL"

# 1) Drop DB, update y lanzar dotnet watch (sin hot reload)
db-restart:
	@echo "Entrando en $(DOTNET_DIR) y reiniciando la BD..."
	cd $(DOTNET_DIR) && \
		dotnet ef database drop --force && \
		dotnet ef database update && \
		dotnet watch --no-hot-reload

# 2) Drop DB, crear nueva migracion y actualizar DB
db-migrate:
	@echo "Droppeando BD, creando migracion y actualizando..."
	@if [ ! -d "$(DOTNET_DIR)/Migrations" ]; then \
		echo "Carpeta Migrations no existe. Creandola..."; \
		mkdir -p $(DOTNET_DIR)/Migrations; \
	fi
	@MIGRATION_BASE="Migration$$(date +%d%m)"; \
	COUNTER=1; \
	MIGRATION_NAME=$$MIGRATION_BASE; \
	cd $(DOTNET_DIR)/Migrations && \
	while ls -1 | grep -q "^[0-9]*_$${MIGRATION_NAME}\.cs$$" 2>/dev/null; do \
		COUNTER=$$((COUNTER + 1)); \
		MIGRATION_NAME="$${MIGRATION_BASE}_$$COUNTER"; \
	done; \
	cd .. && \
	echo "Nombre de migracion: $$MIGRATION_NAME" && \
	dotnet ef database drop --force && \
	dotnet ef migrations add $$MIGRATION_NAME && \
	dotnet ef database update

# 3) dotnet run simple
run:
	@echo "Ejecutando: cd $(DOTNET_DIR) && dotnet run"
	cd $(DOTNET_DIR) && dotnet run

# 4) dotnet watch simple
watch:
	@echo "Ejecutando: cd $(DOTNET_DIR) && dotnet watch --no-hot-reload"
	cd $(DOTNET_DIR) && dotnet watch --no-hot-reload

# 5) Crear y ejecutar contenedor PostgreSQL
docker-create:
	@echo "Creando contenedor PostgreSQL: $(CONTAINER_NAME)"
	docker run --name $(CONTAINER_NAME) \
		-e POSTGRES_USER=$(POSTGRES_USER) \
		-e POSTGRES_PASSWORD=$(POSTGRES_PASSWORD) \
		-e POSTGRES_DB=$(POSTGRES_DB) \
		-p 5432:5432 \
		-d postgres

# 6) Detener y eliminar contenedor PostgreSQL (si existe)
docker-rm:
	@echo "Deteniendo y eliminando contenedor PostgreSQL: $(CONTAINER_NAME)"
	-@docker stop $(CONTAINER_NAME) >/dev/null 2>&1 || true
	-@docker rm $(CONTAINER_NAME) >/dev/null 2>&1 || true
	@echo "Contenedor $(CONTAINER_NAME) eliminado (si existía)."

# 7) Iniciar contenedor PostgreSQL existente
docker-start:
	@echo "Iniciando contenedor PostgreSQL: $(CONTAINER_NAME)"
	docker start $(CONTAINER_NAME)

# 8) Detener contenedor PostgreSQL
docker-stop:
	@echo "Deteniendo contenedor PostgreSQL: $(CONTAINER_NAME)"
	docker stop $(CONTAINER_NAME)

