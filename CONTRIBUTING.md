# Contributing

// TODO

## How to release

1. `git checkout main && git pull --ff-only`
2. `dotnet nbgv tag`
3. `git push origin {tag name}`
4. Navigate to GitHub releases page and create release from tag
5. (If desired) increment version (major or minor) in `//version.json > version`
6. `git add . && git commit -m 'Bumping version' && git push`
