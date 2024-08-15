#!/bin/sh
set -eu

echo "Restore project"
dotnet restore

echo "Restore local tools"
dotnet tool restore

echo "Build project"
dotnet build --configuration Release --no-restore

TMP_WORK_DIR="tmp-$GITHUB_SHA"

WIKI_REPO_DIR="wiki-repo"
WIKI_UPDATE_DIR="wiki-update"

echo Create working directory
mkdir -p -- $TMP_WORK_DIR

echo Going to $TMP_WORK_DIR
cd $TMP_WORK_DIR

echo Creating wiki folders
mkdir -p -- $WIKI_REPO_DIR
mkdir -p -- $WIKI_UPDATE_DIR

echo Cloning wiki
git clone https://$GITHUB_ACTOR:$GITHUB_TOKEN@github.com/$GITHUB_REPOSITORY.wiki.git $WIKI_REPO_DIR

echo "Create wiki"
cd ..
dotnet netdocgen $ASSEMBLY_PATH -o "$TMP_WORK_DIR/$WIKI_UPDATE_DIR"
cd $TMP_WORK_DIR

echo "Copying edited wiki"
cp -R "$WIKI_REPO_DIR/.git" "$WIKI_UPDATE_DIR/"

echo Go into the repo
cd $WIKI_UPDATE_DIR

echo Prepare commit
#Get commit details
author=`git log -1 --format="%an"`
email=`git log -1 --format="%ae"`
message=`git log -1 --format="%s"`

git config --local user.email "$email"
git config --local user.name "$author" 
git add .
if git diff-index --quiet HEAD; then
  echo "Nothing changed"
  exit 0
fi

echo "Pushing changes to wiki"
git commit -m "$message" && git push "https://$GITHUB_ACTOR:$GITHUB_TOKEN@github.com/$GITHUB_REPOSITORY.wiki.git"