#!/usr/bin/env bash
set -e

echo "Prepare commit"

if [ -z "$BRANCH" ] || [ -z "$VERSION" ]; then
  echo "BRANCH or VERSION not set"
  exit 1
fi

# Target repo: the fork for contributor PRs, otherwise this repo
TARGET_REPO="${HEAD_REPO:-$GITHUB_REPOSITORY}"

# Fork pushes require the PAT; GITHUB_TOKEN can never push to forks
if [ "$TARGET_REPO" != "$GITHUB_REPOSITORY" ]; then
  if [ -z "$PUSH_TOKEN" ]; then
    echo "PUSH_TOKEN is empty: secrets are not available (use pull_request_target and pass secrets.PUSH_TOKEN)"
    exit 1
  fi
  TOKEN="$PUSH_TOKEN"
else
  TOKEN="${PUSH_TOKEN:-$GITHUB_TOKEN}"
fi

if [ -z "$TOKEN" ]; then
  echo "No token available"
  exit 1
fi

# Remove the GITHUB_TOKEN auth header persisted by actions/checkout
git config --local --unset-all http.https://github.com/.extraheader || true

# Get commit details
author=$(git log -1 --format="%an")
email=$(git log -1 --format="%ae")

git config --local user.email "$email"
git config --local user.name "$author"

git add .
if git diff-index --quiet HEAD; then
  echo "Nothing changed"
  exit 0
fi

echo "Pushing changes to $TARGET_REPO:$BRANCH"
git commit -m "Update project version to $VERSION"

git remote remove target 2>/dev/null || true
git remote add target "https://x-access-token:${TOKEN}@github.com/${TARGET_REPO}.git"
git push target "HEAD:$BRANCH"