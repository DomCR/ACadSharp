echo Prepare commit
#Get commit details
author=`git log -1 --format="%an"`
email=`git log -1 --format="%ae"`

git config --local user.email "$email"
git config --local user.name "$author" 
git add .
if git diff-index --quiet HEAD; then
  echo "Nothing changed"
  exit 0
fi

echo "Pushing changes"
git commit -am "Update project version to $VERSION"
# git push origin ${{ github.ref_name }}