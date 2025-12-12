@echo off
echo ==========================================
echo   Deploiement compte-courant sur WildFly
echo ==========================================
echo.

echo [INFO] Tentative d'undeploy de l'ancienne version...
call mvn wildfly:undeploy 2>nul
if %errorlevel% equ 0 (
    echo [INFO] Ancienne version supprimee
) else (
    echo [WARN] Aucune version a supprimer
)

echo.
echo [INFO] Compilation du projet...
call mvn clean package
if %errorlevel% neq 0 (
    echo [ERROR] Echec de la compilation
    exit /b 1
)
echo [INFO] Compilation reussie

echo.
echo [INFO] Deploiement sur WildFly...
call mvn wildfly:deploy
if %errorlevel% neq 0 (
    echo [ERROR] Echec du deploiement
    exit /b 1
)
echo [INFO] Deploiement reussi

echo.
echo ==========================================
echo [INFO] Application deployee avec succes !
echo ==========================================
echo.
echo URLs disponibles :
echo   API Config : http://localhost:8080/compte-courant-1.0.0/api/config
echo   Test       : http://localhost:8080/compte-courant-1.0.0/api/config/test
echo   Status     : http://localhost:8080/compte-courant-1.0.0/api/config/status
echo.
echo Pour tester :
echo   curl http://localhost:8080/compte-courant-1.0.0/api/config/test
echo.

pause