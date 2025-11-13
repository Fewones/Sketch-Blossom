#!/bin/bash
# Git Repository Corruption Fix Script
# This script fixes the "bad object refs/remotes/origin/HEAD 2" error

echo "=== Git Repository Corruption Fix ==="
echo ""

# Step 1: Remove corrupted HEAD references
echo "Step 1: Removing corrupted references..."
rm -f .git/refs/remotes/origin/HEAD
rm -f .git/refs/remotes/origin/"HEAD 2"
rm -f .git/refs/remotes/origin/HEAD\ 2
rm -f .git/logs/refs/remotes/origin/HEAD
rm -f .git/logs/refs/remotes/origin/"HEAD 2"
rm -f .git/logs/refs/remotes/origin/HEAD\ 2
echo "✓ Removed potentially corrupted HEAD files"
echo ""

# Step 2: Clean up packed-refs
echo "Step 2: Cleaning packed-refs..."
if [ -f .git/packed-refs ]; then
    # Create backup
    cp .git/packed-refs .git/packed-refs.backup
    echo "✓ Backed up packed-refs to .git/packed-refs.backup"

    # Remove any lines with "HEAD 2" or malformed HEAD refs
    grep -v "HEAD 2" .git/packed-refs > .git/packed-refs.tmp
    mv .git/packed-refs.tmp .git/packed-refs
    echo "✓ Cleaned packed-refs"
fi
echo ""

# Step 3: Prune remote references
echo "Step 3: Pruning stale remote references..."
git remote prune origin
echo "✓ Pruned stale references"
echo ""

# Step 4: Run git garbage collection
echo "Step 4: Running garbage collection..."
git gc --prune=now
echo "✓ Garbage collection complete"
echo ""

# Step 5: Re-fetch everything
echo "Step 5: Re-fetching from remote..."
git fetch --all --prune
echo "✓ Fetch complete"
echo ""

# Step 6: Reset remote HEAD
echo "Step 6: Resetting remote HEAD..."
git remote set-head origin -a 2>/dev/null || git remote set-head origin main
echo "✓ Remote HEAD reset"
echo ""

echo "=== Fix Complete! ==="
echo ""
echo "Your repository should now be clean. Try fetching in GitHub Desktop again."
echo ""
echo "If you still have issues, you may need to:"
echo "  1. Close GitHub Desktop"
echo "  2. Run this script again"
echo "  3. Re-open GitHub Desktop"
