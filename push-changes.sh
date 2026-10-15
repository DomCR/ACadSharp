#!/usr/bin/env bash
set -e

echo "Prepare commit"

if [ -z "$BRANCH" ] || [ -z "$VERSION" ]; then
  echo "BRANCH or VERSION not set: skipping push"
  exit 1
fi

# Target repo: the fork for contributor PRs, otherwise this repo
TARGET_REPO="${HEAD_REPO:-$GITHUB_REPOSITORY}"
TOKEN="${PUSH_TOKEN:-$GITHUB_TOKEN}"

if [ -z "$TOKEN" ]; then
  echo "No token available: skipping push"
  exit 1
fi

# Get commit details
author=$(git log -1 --format="%an")
email=$(git log -1 --format="%ae")

git config --local user.email "$email"
git config --local user.name "$author"

git add .
if git diff-index --quiet HEAD; then
  echo "Nothing changed"
  exit 1
fi

echo "Pushing changes to $TARGET_REPO:$BRANCH"
git commit -m "Update project version to $VERSION"

git remote remove target 2>/dev/null || true
git remote add target "https://x-access-token:${TOKEN}@github.com/${TARGET_REPO}.git"
git push target "HEAD:$BRANCH"