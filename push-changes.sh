#!/usr/bin/env sh
set -eu

echo "Prepare commit"

#Get commit details
author="$(git config --get user.name || true)"
email="$(git config --get user.email || true)"

if [ -z "${author}" ] || [ -z "${email}" ]; then
  echo "Git user identity is not configured."
  exit 1
fi

git add -A

if git diff-index --quiet HEAD --; then
  echo "Nothing changed"
  exit 0
fi

branch="${BRANCH:-$(git rev-parse --abbrev-ref HEAD)}"
remote="$(git config --get "branch.${branch}.remote" || true)"
remote="${remote:-origin}"

echo "Pushing changes to ${remote}/${branch}"
git commit -m "Update project version to ${VERSION}"
git push "${remote}" "${branch}"