@echo off
REM Git Repository Corruption Fix Script (Windows)
REM This script fixes the "bad object refs/remotes/origin/HEAD 2" error

echo === Git Repository Corruption Fix ===
echo.

REM Step 1: Remove corrupted HEAD references
echo Step 1: Removing corrupted references...
if exist .git\refs\remotes\origin\HEAD del /F /Q .git\refs\remotes\origin\HEAD
if exist ".git\refs\remotes\origin\HEAD 2" del /F /Q ".git\refs\remotes\origin\HEAD 2"
if exist .git\logs\refs\remotes\origin\HEAD del /F /Q .git\logs\refs\remotes\origin\HEAD
if exist ".git\logs\refs\remotes\origin\HEAD 2" del /F /Q ".git\logs\refs\remotes\origin\HEAD 2"
echo Done: Removed potentially corrupted HEAD files
echo.

REM Step 2: Backup packed-refs
echo Step 2: Backing up packed-refs...
if exist .git\packed-refs copy .git\packed-refs .git\packed-refs.backup >nul
echo Done: Backed up packed-refs
echo.

REM Step 3: Prune remote references
echo Step 3: Pruning stale remote references...
git remote prune origin
echo Done: Pruned stale references
echo.

REM Step 4: Run git garbage collection
echo Step 4: Running garbage collection...
git gc --prune=now
echo Done: Garbage collection complete
echo.

REM Step 5: Re-fetch everything
echo Step 5: Re-fetching from remote...
git fetch --all --prune
echo Done: Fetch complete
echo.

REM Step 6: Reset remote HEAD
echo Step 6: Resetting remote HEAD...
git remote set-head origin main 2>nul
if errorlevel 1 git remote set-head origin -a
echo Done: Remote HEAD reset
echo.

echo === Fix Complete! ===
echo.
echo Your repository should now be clean. Try fetching in GitHub Desktop again.
echo.
echo If you still have issues:
echo   1. Close GitHub Desktop completely
echo   2. Run this script again
echo   3. Re-open GitHub Desktop
echo.
pause
